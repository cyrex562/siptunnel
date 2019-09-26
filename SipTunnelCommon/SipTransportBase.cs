using System;
using System.Globalization;
using NET = System.Net;
using SOCK = System.Net.Sockets;
using COL = System.Collections.Generic;
using REGEX = System.Text.RegularExpressions;

namespace SipTunnel
{
	internal abstract class SipTransportBase
		: IDisposable
	{
		private SOCK.TcpClient m_tcpConnection;
		private SOCK.NetworkStream m_tcpStream;
		protected bool m_ShuttingDown;
		protected readonly byte[] m_tcpBuffer = new byte[5 + 2]; // XXXPacketTag.Length + 2 bytes for langth of data in packet

		protected readonly ProgramSettings m_Settings;

		protected readonly COL.Dictionary<string, SoundProxy> m_SoundProxies = new COL.Dictionary<string, SoundProxy>(16);

		private static readonly byte[] g_TcpSipPacketTag = new byte[] { 0x42, 0x6F, 0x72, 0x6A, 0x61 };
		private static readonly byte[] g_TcpRtpPacketTag = new byte[] { 0x4D, 0x61, 0x72, 0x69, 0x61 };

		internal static readonly System.Text.ASCIIEncoding g_Ascii = new System.Text.ASCIIEncoding();

		protected const REGEX.RegexOptions m_RegExOptions = REGEX.RegexOptions.Compiled | REGEX.RegexOptions.CultureInvariant | REGEX.RegexOptions.ExplicitCapture | REGEX.RegexOptions.IgnoreCase | REGEX.RegexOptions.Multiline;
		protected static readonly REGEX.Regex
			g_CallId = new REGEX.Regex(@"(?<a>Call-ID:\s*)(?<q>[^\n;\s]+)(?<z>\s*\n)", m_RegExOptions),
			g_SdpAddressInC = new REGEX.Regex(@"(?<a>c=\s*IN\s+IP4\s+)(?<q>(\d|\.)+)(?<z>\s*\n)", m_RegExOptions),
			g_SdpPortInM = new REGEX.Regex(@"(?<a>m=\s*audio\s+)(?<q>\d+)(?<z>\s+.*?\n)", m_RegExOptions);

		public event EventHandler PipeDead;
		public event EventHandler<SipMessageEventArgs> SipReceivedFromPipe;
		public event EventHandler<RtpMessageEventArgs> RtpReceivedFromPipe;
		public event EventHandler<SipMessageEventArgs> SipReceivedFromClient;
		public event EventHandler<RtpMessageEventArgs> RtpReceivedFromClient;

		protected SipTransportBase(ProgramSettings settings)
		{
			if (null == settings)
				throw new ArgumentNullException("settings");

			m_Settings = settings;
		}

		public void SendToPipe(string sipMsg)
		{
			if (null == sipMsg)
				throw new ArgumentNullException("sipMsg");
			if (0 == sipMsg.Length)
				return;

			SendToPipe(
				g_Ascii.GetBytes(sipMsg),
				g_TcpSipPacketTag
			);
		}

		public void SendToPipe(byte[] data)
		{
			if (null == data)
				throw new ArgumentNullException("data");
			if (0 == data.Length)
				return;

			SendToPipe(data, g_TcpRtpPacketTag);
		}

		private void SendToPipe(byte[] data, byte[] tag)
		{
			byte[] newData = new byte[tag.Length + 2 + data.Length];

			tag.CopyTo(newData, 0);

			byte[] dataLen = BitConverter.GetBytes((ushort)data.Length);
			dataLen.CopyTo(newData, tag.Length);

			data.CopyTo(newData, tag.Length + 2);

			if (null == m_tcpStream)
				return;

			lock (m_tcpStream)
			{
				try
				{
					m_tcpStream.Write(newData, 0, newData.Length);
				}
				catch (Exception)
				{
					OnPipeDead(EventArgs.Empty);
				}
			}
		}

		private void OnReadFromTcp(IAsyncResult ar)
		{
			int read = 0;

			try
			{
				read = m_tcpStream.EndRead(ar);
			}
			catch (Exception)
			{
				OnPipeDead(EventArgs.Empty);
				return;
			}

			if (0 == read)
			{
				OnPipeDead(EventArgs.Empty);
				return;
			}

			if (read != m_tcpBuffer.Length)
				throw new SipProxyException("Unexpected length of header read from TCP.");
	
			bool sip = true, rtp = true;
			for (byte i = 0; i < g_TcpSipPacketTag.Length; i++)
			{
				sip = sip && (m_tcpBuffer[i] == g_TcpSipPacketTag[i]);
				rtp = rtp && (m_tcpBuffer[i] == g_TcpRtpPacketTag[i]);
			}

			if (sip == rtp)
				throw new SipProxyException("Unrecognized packet read from TCP.");

			ushort dataLen = BitConverter.ToUInt16(m_tcpBuffer, g_TcpSipPacketTag.Length);
			byte[] myBuff = new byte[dataLen];

			int readTotal = 0;
			while (readTotal < dataLen)
			{
				read = m_tcpStream.Read(myBuff, readTotal, myBuff.Length - readTotal);
				readTotal += read;
			}

			if (sip)
			{
				OnSipReceivedFromTcp(
					new SipMessageEventArgs(g_Ascii.GetString(myBuff, 0, myBuff.Length))
				);
			}
			else
			{
				OnRtpReceivedFromTcp(
					new RtpMessageEventArgs(myBuff)
				);
			}

			try
			{
				m_tcpStream.BeginRead(m_tcpBuffer, 0, m_tcpBuffer.Length, OnReadFromTcp, null);
			}
			catch (Exception)
			{
				OnPipeDead(EventArgs.Empty);
			}
		}

		protected virtual void OnSipReceivedFromTcp(SipMessageEventArgs e)
		{
			if (null != SipReceivedFromPipe)
				SipReceivedFromPipe(this, e);

			if (!e.Cancel && e.SipMessage.StartsWith("BYE", StringComparison.InvariantCultureIgnoreCase))
			{ // We should kill sound proxy
				REGEX.Match m = g_CallId.Match(e.SipMessage);
				if (m.Success && m.Groups["q"].Success)
				{
					string callId = m.Groups["q"].Value;
					SoundProxy sp = null;
					if (m_SoundProxies.TryGetValue(callId, out sp))
					{
						m_SoundProxies.Remove(callId);
						sp.Dispose();
						m_Settings.WriteMessageToLog(
							LogMessageType.Information + 1,
							"Destroyed sound proxy for Call-ID '" + callId + '\''
						);
					}
				}
			}
		}

		protected virtual void OnRtpReceivedFromTcp(RtpMessageEventArgs e)
		{
			if (null != RtpReceivedFromPipe)
				RtpReceivedFromPipe(this, e);

			string callId;
			byte[] soundData;
			SoundProxy.GetSoundData(e.RtpData, e.RtpData.Length, out callId, out soundData);

			SoundProxy sp;
			if (!m_SoundProxies.TryGetValue(callId, out sp))
			{
				m_Settings.WriteMessageToLog(
					LogMessageType.Error,
					"Failed to find sound proxy for Call-ID '" + callId + "'. Ignoring sound packet."
				);
			}

			sp.SendSoundDataToServer(soundData);
		}

		protected virtual void OnSipReceivedFromClient(SipMessageEventArgs e)
		{
			if (null != SipReceivedFromClient)
				SipReceivedFromClient(this, e);
		}

		protected virtual void OnRtpReceivedFromClient(RtpMessageEventArgs e)
		{
			if (null != RtpReceivedFromClient)
				RtpReceivedFromClient(this, e);
		}

		public void PreparePipe(SOCK.TcpClient client)
		{
			if (null == client)
				throw new ArgumentNullException("client");

			m_tcpConnection = client;
			m_tcpStream = m_tcpConnection.GetStream();
			m_tcpStream.BeginRead(m_tcpBuffer, 0, m_tcpBuffer.Length, OnReadFromTcp, null);
		}

		public SoundProxy CreateSoundProxy(string sipMsg, string callId, bool setServer)
		{
			if (null == sipMsg || 0 == sipMsg.Length)
				return null;
			if (null == callId || 0 == callId.Length)
				return null;

			SoundProxy sp = null;
			if (m_SoundProxies.TryGetValue(callId, out sp))
			{
				if (!setServer)
					return sp;
				if (sp.ServerSet)
					return sp;
			}

			REGEX.Match m = g_SdpAddressInC.Match(sipMsg);
			if (!m.Groups["q"].Success)
				return null;

			string ssHost = m.Groups["q"].Value;

			m = g_SdpPortInM.Match(sipMsg);
			if (!m.Groups["q"].Success)
				return null;

			ushort ssPort = ushort.Parse(m.Groups["q"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
			if (0 == ssPort)
				return null;

			if (null == sp)
			{
				if (setServer)
					sp = new SoundProxy(ssHost, ssPort, callId, this);
				else
					sp = new SoundProxy(callId, this);

				m_SoundProxies.Add(callId, sp);
				m_Settings.WriteMessageToLog(
					LogMessageType.Information + 1,
					string.Format(
						CultureInfo.CurrentUICulture,
						"Created sound proxy" + (setServer ? " for {1}:{2}" : string.Empty) + " with branch {0}.",
						callId,
						ssHost,
						ssPort
					)
				);
			}
			else
			{
				sp.SetServer(ssHost, ssPort);
				m_Settings.WriteMessageToLog(
					LogMessageType.Information + 1,
					string.Format(
						CultureInfo.CurrentUICulture,
						"Updated sound proxy for {0}:{1} with branch {2}.",
						ssHost,
						ssPort,
						callId
					)
				);
			}

			return sp;
		}

		public void SendSipMsgToClient(string sipMsg, string host, ushort port)
		{
			NET.IPEndPoint ipEp = new System.Net.IPEndPoint(
				ProgramSettings.IpFromHost(host),
				port
			);
			SendSipMsgToClient(sipMsg, ipEp);
		}

		public abstract void SendSipMsgToClient(string sipMsg, NET.IPEndPoint clientEp);

		protected virtual void OnPipeDead(EventArgs e)
		{
			m_tcpStream = null;
			if (null != PipeDead)
				PipeDead(this, e);
		}

		public void ReconnectPipe()
		{
			if (null != m_tcpConnection)
			{
				m_tcpConnection.Client.Close();
				m_tcpStream = null;
			}

			m_tcpConnection = new SOCK.TcpClient();
			NET.IPEndPoint remoteEp = null;

			m_Settings.WriteMessageToLog(
				LogMessageType.Information,
				string.Format(
					CultureInfo.InvariantCulture,
					"Connecting to {0}:{1}...",
					m_Settings.ClientServerHostIp,
					m_Settings.ClientServerPort
				)
			);

			bool bOk = false;
			while (!bOk && !m_ShuttingDown)
			{
				try
				{
					if (null == remoteEp)
						remoteEp = new NET.IPEndPoint(m_Settings.ClientServerHostIp, m_Settings.ClientServerPort); // Error if DNS is not avaliable

					if (null != remoteEp)
					{
						m_tcpConnection.Connect(remoteEp);
						bOk = true;
					}
				}
				catch (Exception)
				{
					
				}
			}

			if (m_ShuttingDown)
				return;

			m_tcpStream = m_tcpConnection.GetStream();
			m_tcpStream.BeginRead(m_tcpBuffer, 0, m_tcpBuffer.Length, OnReadFromTcp, null);

			m_Settings.WriteMessageToLog(
				LogMessageType.Information,
				string.Format(
					CultureInfo.InvariantCulture,
					"TCP channel connected to SipTunnel server at {0}:{1}.",
					remoteEp.Address,
					remoteEp.Port
				)
			);
		}

		public void PrepareClient(NET.IPAddress localIp, ushort localPort)
		{
			PrepareClient(
				new NET.IPEndPoint(localIp, localPort)
			);
		}

		public abstract void PrepareClient(NET.IPEndPoint localEp);

		public ProgramSettings Settings
		{
			get
			{
				return m_Settings;
			}
		}

		//public SOCK.NetworkStream TcpStream
		//{
		//  get
		//  {
		//    return m_tcpStream;
		//  }
		//}

		//public NET.IPAddress TcpAddress
		//{
		//  get
		//  {
		//    return ((NET.IPEndPoint)m_tcpConnection.Client.LocalEndPoint).Address;
		//  }
		//}

		public NET.IPEndPoint PipeRemoteEndPoint
		{
			get
			{
				return (NET.IPEndPoint)m_tcpConnection.Client.RemoteEndPoint;
			}
		}

		public abstract NET.IPAddress LocalAddress
		{
			get;
		}

		public abstract ushort LocalPort
		{
			get;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_ShuttingDown = true;

				foreach (SoundProxy sp in m_SoundProxies.Values)
					sp.Dispose();

				m_SoundProxies.Clear();

				if (null == m_tcpConnection)
					return;

				if (null == m_tcpStream)
				{
					m_tcpConnection.Close();
				}
				else
				{
					lock (m_tcpStream)
					{
						m_tcpConnection.Close();
						m_tcpStream = null;
					}
				}

				m_tcpConnection = null;
			}
		}
	}
}
