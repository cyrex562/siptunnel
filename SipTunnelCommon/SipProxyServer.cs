using System;
using System.Globalization;
using NET = System.Net;
using SOCK = System.Net.Sockets;
using COL = System.Collections.Generic;
using REGEX = System.Text.RegularExpressions;

namespace SipTunnel
{
	internal class SipProxyServer
		: SipProxyBase
	{
		private readonly NET.IPEndPoint m_PipeRemoteEp;

		public event EventHandler PipeDead;

		public SipProxyServer(SOCK.TcpClient tcpClient, ProgramSettings settings)
		{
			if (null == settings)
				throw new ArgumentNullException("settings");
			if (settings.ServerType == ConnectorType.None)
				throw new InvalidOperationException("Server cannot be created because is is disabled.");
			if (null == tcpClient)
				throw new ArgumentNullException("tcpClient");

			CreateTransport(settings.ServerType, settings);
			m_Transport.PrepareClient(settings.ServerIp, 0);
			m_Transport.PreparePipe(tcpClient);
			m_PipeRemoteEp = m_Transport.PipeRemoteEndPoint;
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
			string localIp = m_Transport.LocalAddress.ToString();
			bool request = false;
			while ((sipMsgLine = strReader.ReadLine()) != null)
			{
				if (0 == sbSipMsg.Length)
				{
					if (!sipMsgLine.StartsWith("SIP/2.0", StringComparison.InvariantCultureIgnoreCase))
					{
						request = true;
						REGEX.Match m = g_ServerInMsgType.Match(sipMsgLine);
						if (m.Success && m.Groups["host"].Success && m.Groups["host"].Value.Equals(ProgramSettings.SipTunnelInternalHost, StringComparison.InvariantCulture))
						{
							sipMsgLine = g_ServerInMsgType.Replace(
								sipMsgLine,
								String.Format(
									CultureInfo.InvariantCulture,
									"${{a}}{0}{1}${{z}}",
									m.Groups["acc"].Success ? m.Groups["acc"].Value + '@' : String.Empty,
									Settings.SipServerHost
								)
							);
						}
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
								"Via: SIP/2.0/{0} {1}:{2};rport;SipTunnel\n",
								Settings.ServerType == ConnectorType.UDP ? "UDP" : "TCP",
								localIp,
								m_Transport.LocalPort
							);
						}
					}
					else if (sipMsgLine.StartsWith("To:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_To.Match(sipMsgLine);
						if (m.Success && m.Groups["q"].Success && m.Groups["q"].Value.Equals(ProgramSettings.SipTunnelInternalHost, StringComparison.InvariantCulture))
							sipMsgLine = g_To.Replace(sipMsgLine, "${a}" + Settings.SipServerHost + "${z}");
					}
					else if (sipMsgLine.StartsWith("Content-Length:", StringComparison.CurrentCultureIgnoreCase))
					{
						if (sp != null && sdpLength > 0)
							sipMsgLine = "Content-Length: " + sdpLength.ToString(CultureInfo.InvariantCulture);
					}
					else if (sipMsgLine.StartsWith("From:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_From.Replace(sipMsgLine, "${a}${acc}@" + Settings.SipServerHost + "${z}");
					else if (sipMsgLine.StartsWith("Contact:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_Contact.Replace(sipMsgLine, "${a}${acc}@" + localIp + ':' + m_Transport.LocalPort.ToString(CultureInfo.InvariantCulture) + "${z}");
					else if (sipMsgLine.StartsWith("User-Agent:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine += " (via SipTunnel)";
				}

				sbSipMsg.AppendLine(sipMsgLine);
			}

			m_Transport.SendSipMsgToClient(sbSipMsg.ToString(), Settings.SipServerHost, Settings.SipServerPort);
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
					//if (!sipMsgLine.StartsWith("SIP/2.0", StringComparison.InvariantCultureIgnoreCase))
					//{
					//  REGEX.Match m = g_ServerInMsgType.Match(sipMsgLine);
					//  if (m.Success && m.Groups["q"].Success && Settings.IsSipServerHost(m.Groups["q"].Value))
					//    sipMsgLine = g_ServerInMsgType.Replace(sipMsgLine, "${a}" + ProgramSettings.SipTunnelInternalHost + "${z}");
					//}
				}
				else
				{
					if (sipMsgLine.StartsWith("Via:", StringComparison.InvariantCultureIgnoreCase))
					{
						if (sipMsgLine.Contains("SipTunnel"))
							continue;
					}
					else if (sipMsgLine.StartsWith("To:", StringComparison.InvariantCultureIgnoreCase))
						sipMsgLine = g_To.Replace(sipMsgLine, "${a}" + ProgramSettings.SipTunnelInternalHost + "${z}");
					else if (sipMsgLine.StartsWith("Content-Type:", StringComparison.InvariantCultureIgnoreCase))
						createProxy = sipMsgLine.Contains("application/sdp");
					else if (sipMsgLine.StartsWith("Call-ID:", StringComparison.InvariantCultureIgnoreCase))
						callId = sipMsgLine.Substring(8).Trim();
					else if (sipMsgLine.StartsWith("From:", StringComparison.InvariantCultureIgnoreCase))
					{
						REGEX.Match m = g_From.Match(sipMsgLine);
						if (m.Success && m.Groups["host"].Success && Settings.IsSipServerHost(m.Groups["host"].Value))
							sipMsgLine = g_From.Replace(sipMsgLine, "${a}${acc}@" + ProgramSettings.SipTunnelInternalHost + "${z}");
					}
				}

				sbSipMsg.AppendLine(sipMsgLine);
			}

			string sipMsg = sbSipMsg.ToString();
			if (createProxy && null != callId && callId.Length > 0)
			{
				m_Transport.CreateSoundProxy(sipMsg, callId, true);
				createProxy = false;
			}

			m_Transport.SendToPipe(sipMsg);
		}

		protected override void OnPipeDead(object sender, EventArgs e)
		{
			if (null != PipeDead)
				PipeDead(this, e);
		}

		public NET.IPEndPoint PipeRemoteEndPoint
		{
			get
			{
				return m_PipeRemoteEp;
			}
		}
	}
}
