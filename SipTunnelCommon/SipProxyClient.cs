using System;
using System.Globalization;
using NET = System.Net;
using SOCK = System.Net.Sockets;
using REGEX = System.Text.RegularExpressions;
using COL = System.Collections.Generic;

namespace SipTunnel
{
	internal class SipProxyClient
		: SipProxyBase
	{
#if PLAT_WINDOWS || PLAT_MONO
		private readonly COL.Dictionary<int, NET.IPEndPoint> m_Clients = new COL.Dictionary<int, NET.IPEndPoint>(8);
		private readonly COL.Dictionary<string, NET.IPEndPoint> m_Accounts = new COL.Dictionary<string, NET.IPEndPoint>(8);
#else
		private NET.IPEndPoint m_EpClientStck, m_EpClientContact;
#endif

		private System.Threading.Thread m_tcpThread;
		private readonly System.Threading.ManualResetEvent m_tcpThreadWaitEvent = new System.Threading.ManualResetEvent(false);

		public SipProxyClient(ProgramSettings settings)
		{
			if (null == settings)
				throw new ArgumentNullException("settings");
			if (null == settings.ClientServerHost || 0 == settings.ClientServerHost.Length)
				throw new SipProxyClientException("SipTunnel server host is not set.");
			if (0 == settings.ClientServerPort)
				throw new SipProxyClientException("SipTunnel server port is not set.");
			if (0 == settings.ClientPort)
				throw new SipProxyClientException("SipTunnel client port is not set.");
			if (settings.ClientType == ConnectorType.None)
				throw new InvalidOperationException("Client cannot be created because is is disabled.");

			CreateTransport(settings.ClientType, settings);

			//ReconnectTcp();
			m_tcpThread = new System.Threading.Thread(TcpChannelProc);
			m_tcpThread.Start();

			m_Transport.PrepareClient(Settings.ClientIp, Settings.ClientPort);

			Settings.WriteMessageToLog(
				LogMessageType.Information,
				string.Format(
					CultureInfo.CurrentUICulture,
					"SipTunnel client is started, listening on {0}:{1}.",
					Settings.ClientIp,
					Settings.ClientPort
				)
			);
		}

		private void TcpChannelProc()
		{
			m_Transport.ReconnectPipe();

			m_tcpThreadWaitEvent.WaitOne();
		}

		protected override void OnSipReceivedFromPipe(object sender, SipMessageEventArgs e)
		{
			base.OnSipReceivedFromPipe(sender, e);

			ushort sdpLength = 0;
			SoundProxy sp = null;
			string newSipMsg = CreateSoundProxyAndChangeSdpData(e.SipMessage, out sp, out sdpLength);

			System.IO.StringReader strReader = new System.IO.StringReader(newSipMsg);
			System.Text.StringBuilder sbSipMsg = new System.Text.StringBuilder(newSipMsg.Length + 16);
			string sipMsgLine;
			string localIp = Settings.ClientIp.ToString();
			bool request = false;
			System.Net.IPEndPoint ipEp = null;
			while ((sipMsgLine = strReader.ReadLine()) != null)
			{
				if (0 == sbSipMsg.Length)
				{
					if (!sipMsgLine.StartsWith("SIP/2.0", StringComparison.InvariantCultureIgnoreCase))
					{
						request = true;

#if PLAT_WINDOWS || PLAT_MONO
						REGEX.Match m = g_ServerInMsgType.Match(sipMsgLine);

						if (!m.Success)
							throw new SipProxyClientException("Message header is wrong format.");
						if (!m.Groups["acc"].Success)
							throw new SipProxyClientException("Account code not found in message header.");
						if (!m_Accounts.TryGetValue(m.Groups["acc"].Value, out ipEp))
							throw new SipProxyClientException("Account for code '" + m.Groups["acc"].Value + "' not found.");
#else
						if (null == m_EpClientContact)
							throw new SipProxyClientException("Client end point not set (contact).");

						ipEp = m_EpClientContact;
#endif

						sipMsgLine = g_ServerInMsgType.Replace(sipMsgLine, "${a}${acc}@" + localIp + "${z}");
					}
				}
				else
				{
					if (sipMsgLine.StartsWith("Via:", StringComparison.InvariantCultureIgnoreCase))
					{
						if (request)
						{
							sbSipMsg.AppendFormat(
								CultureInfo.InvariantCulture,
								"Via: SIP/2.0/{0} {1}:{2};rport;SipTunnel\r\n",
								Settings.ServerType == ConnectorType.UDP ? "UDP" : "TCP",
								localIp,
								m_Transport.LocalPort
							);
						}
						else
						{
							REGEX.Match m = g_Via.Match(sipMsgLine);
							if (!m.Success)
								throw new SipProxyClientException("Via header is wrong format or is absent.");

							if (m.Groups["rport"].Success && m.Groups["rport"].Value.Length > 0)
							{ // Using sender's source address and port
								if (!m.Groups["stckVal"].Success)
									throw new SipProxyClientException("Via header parameter 'stck' not found.");

#if PLAT_WINDOWS || PLAT_MONO
								int key = int.Parse(m.Groups["stckVal"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
								if (!m_Clients.ContainsKey(key))
									throw new SipProxyClientException("Failed to find client for value in 'stck'.");

								ipEp = m_Clients[key];
#else
								if (null == m_EpClientStck)
									throw new SipProxyClientException("Client end point not set (stck).");

								ipEp = m_EpClientStck;
#endif
							}
							else
							{ // Using address and port from Via header
								if (!m.Groups["addr"].Success || !m.Groups["port"].Success)
									throw new SipProxyClientException("Address or port in Via header not found.");

								ipEp = new System.Net.IPEndPoint(
									NET.IPAddress.Parse(m.Groups["addr"].Value),
									ushort.Parse(m.Groups["port"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture)
								);
							}
						}
					}
					else if (sipMsgLine.StartsWith("To:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_To.Replace(sipMsgLine, "${a}" + localIp + "${z}");
					else if (sipMsgLine.StartsWith("From:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_From.Match(sipMsgLine);
						if (m.Success && m.Groups["host"].Success && m.Groups["host"].Value.Equals(ProgramSettings.SipTunnelInternalHost, StringComparison.InvariantCulture))
							sipMsgLine = g_From.Replace(sipMsgLine, "${a}${acc}@" + localIp + "${z}");
					}
					else if (sipMsgLine.StartsWith("Content-Length:", StringComparison.CurrentCultureIgnoreCase))
					{
						if (sp != null && sdpLength > 0)
							sipMsgLine = "Content-Length: " + sdpLength.ToString(CultureInfo.InvariantCulture);
					}
					else if (sipMsgLine.StartsWith("Contact:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_Contact.Replace(sipMsgLine, "${a}${acc}@" + localIp + ':' + m_Transport.LocalPort.ToString(CultureInfo.InvariantCulture) + "${z}");
					else if (sipMsgLine.StartsWith("User-Agent:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine += " (via SipTunnel)";
				}

				sbSipMsg.Append(sipMsgLine + "\r\n");
			}

			m_Transport.SendSipMsgToClient(sbSipMsg.ToString(), ipEp);
		}

		protected override void OnSipReceivedFromClient(object sender, SipMessageEventArgs e)
		{
			base.OnSipReceivedFromClient(sender, e);

			if (0 == e.SipMessage.Trim().Length)
				return;

			System.IO.StringReader strReader = new System.IO.StringReader(e.SipMessage);
			System.Text.StringBuilder sbSipMsg = new System.Text.StringBuilder(e.SipMessage.Length + 16);
			string sipMsgLine, callId = null;
			bool createProxy = false;
			while ((sipMsgLine = strReader.ReadLine()) != null)
			{
				if (0 == sbSipMsg.Length)
				{
					if (!sipMsgLine.StartsWith("SIP/2.0", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_ServerInMsgType.Match(sipMsgLine);
						if (m.Success && m.Groups["host"].Success && Settings.IsClientHost(m.Groups["host"].Value))
						{
							sipMsgLine = g_ServerInMsgType.Replace(
								sipMsgLine,
								string.Format(
									CultureInfo.InvariantCulture,
									"${{a}}{0}{1}${{z}}",
									m.Groups["acc"].Success ? m.Groups["acc"].Value + '@' : string.Empty,
									ProgramSettings.SipTunnelInternalHost
								)
							);
						}
					}
				}
				else
				{
					if (sipMsgLine.StartsWith("Via:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_Via.Match(sipMsgLine);
						if (m.Success && m.Groups["rport"].Success)
						{
							int key = e.RemoteEndPoint.GetHashCode();
							sipMsgLine += ";stck=" + key.ToString(CultureInfo.InvariantCulture);

#if PLAT_WINDOWS || PLAT_MONO
							if (!m_Clients.ContainsKey(key))
								m_Clients.Add(key, e.RemoteEndPoint);
#else
							if (null == m_EpClientStck)
								m_EpClientStck = e.RemoteEndPoint;
#endif
						}
					}
					if (sipMsgLine.StartsWith("From:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_From.Replace(sipMsgLine, "${a}${acc}@" + ProgramSettings.SipTunnelInternalHost + "${z}");
					else if (sipMsgLine.StartsWith("Contact:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_Contact.Match(sipMsgLine);

#if PLAT_WINDOWS || PLAT_MONO
						if (m.Success && m.Groups["acc"].Success && !m_Accounts.ContainsKey(m.Groups["acc"].Value))
						{
							if (!m.Groups["host"].Success)
								throw new SipProxyClientException("Host in Contact header not found.");

							ushort port = 5060;
							if (m.Groups["port"].Success && m.Groups["port"].Value.Length > 0)
								port = ushort.Parse(m.Groups["port"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);

							m_Accounts.Add(
								m.Groups["acc"].Value,
								new NET.IPEndPoint(NET.IPAddress.Parse(m.Groups["host"].Value), port)
							);
						}
#else
						if (m.Success && m.Groups["acc"].Success)
						{
							if (!m.Groups["host"].Success)
								throw new SipProxyClientException("Host in Contact header not found.");

							ushort port = 5060;
							if (m.Groups["port"].Success && m.Groups["port"].Value.Length > 0)
								port = ushort.Parse(m.Groups["port"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);

							m_EpClientContact = new NET.IPEndPoint(NET.IPAddress.Parse(m.Groups["host"].Value), port);
						}
#endif
					}
					else if (sipMsgLine.StartsWith("Content-Type:", StringComparison.InvariantCultureIgnoreCase))
						createProxy = (sipMsgLine.IndexOf("application/sdp", StringComparison.InvariantCultureIgnoreCase) > 0);
					else if (sipMsgLine.StartsWith("Call-ID:", StringComparison.InvariantCultureIgnoreCase))
						callId = sipMsgLine.Substring(8).Trim();
					else if (sipMsgLine.StartsWith("To:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_To.Match(sipMsgLine);
						if (m.Success && m.Groups["q"].Success && Settings.IsClientHost(m.Groups["q"].Value))
							sipMsgLine = g_To.Replace(sipMsgLine, "${a}" + ProgramSettings.SipTunnelInternalHost + "${z}");
					}
				}

				sbSipMsg.Append(sipMsgLine + '\n');
			}

			string sipMsg = sbSipMsg.ToString();
			if (createProxy && callId != null && callId.Length > 0)
			{
				m_Transport.CreateSoundProxy(sipMsg, callId, true);
				createProxy = false;
			}

			m_Transport.SendToPipe(sipMsg);
		}

		protected override void OnPipeDead(object sender, EventArgs e)
		{
			if (null == m_tcpThread)
				return;

			Settings.WriteMessageToLog(
				LogMessageType.Error,
				"TCP channel to SipTunnel server closed. Trying to reconnect."
			);

			m_Transport.ReconnectPipe();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != m_tcpThread)
				{
					m_tcpThreadWaitEvent.Set();
					m_tcpThread.Join(3000);
					m_tcpThread = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}
