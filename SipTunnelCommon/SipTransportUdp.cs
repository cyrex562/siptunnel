using System;
using System.Globalization;
using NET = System.Net;
using SOCK = System.Net.Sockets;
using REGEX = System.Text.RegularExpressions;

namespace SipTunnel
{
	internal class SipTransportUdp
		: SipTransportBase
	{
		private SOCK.UdpClient m_udpClient;
		private System.Threading.Thread m_udpReceiveThread;

		public SipTransportUdp(ProgramSettings settings)
			: base(settings)
		{
			m_udpReceiveThread = new System.Threading.Thread(UdpReceiveProc);
		}

		private void UdpReceiveProc()
		{
			while (!m_ShuttingDown)
			{
				NET.IPEndPoint remoteEp = null;
				byte[] data = null;
				try
				{
					data = m_udpClient.Receive(ref remoteEp);
				}
				catch (Exception)
				{
					
				}

				if (null != data)
				{
					string sipMsg = g_Ascii.GetString(data, 0, data.Length);
					m_Settings.WriteMessageToLog(
						LogMessageType.Information + 3,
						string.Format(
							CultureInfo.CurrentUICulture,
							"SIP message received from {0}:{1}\n{2}",
							remoteEp.Address,
							remoteEp.Port,
							sipMsg
						)
					);

					base.OnSipReceivedFromClient(
						new SipMessageEventArgs(sipMsg, remoteEp)
					);
				}
			}
		}

		public override void PrepareClient(System.Net.IPEndPoint localEp)
		{
			if (null == localEp)
				throw new ArgumentNullException("localEp");
			if (null != m_udpClient)
				throw new InvalidOperationException("UDP already initialized.");

			m_udpClient = new SOCK.UdpClient(localEp);
			m_udpReceiveThread.Start();
		}

		public override void SendSipMsgToClient(string sipMsg, NET.IPEndPoint clientEp)
		{
			if (null == sipMsg)
				throw new ArgumentNullException("sipMsg");
			if (null == clientEp)
				throw new ArgumentNullException("clientEp");
			if (0 == sipMsg.Length)
				return;

			m_Settings.WriteMessageToLog(
				LogMessageType.Information + 2,
				string.Format(
					CultureInfo.CurrentUICulture,
					"Sending SIP message to {0}:{1}\n{2}",
					clientEp.Address,
					clientEp.Port,
					sipMsg
				)
			);

			byte[] data = g_Ascii.GetBytes(sipMsg);
			m_udpClient.Send(data, data.Length, clientEp);
		}

		public override System.Net.IPAddress LocalAddress
		{
			get
			{
				return ((NET.IPEndPoint)m_udpClient.Client.LocalEndPoint).Address;
			}
		}

		public override ushort LocalPort
		{
			get
			{
				return (ushort)((NET.IPEndPoint)m_udpClient.Client.LocalEndPoint).Port;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (null != m_udpClient)
				{
					m_udpClient.Close();
					m_udpClient = null;
				}

				if (null != m_udpReceiveThread)
				{
					m_udpReceiveThread.Join(3000);
					m_udpReceiveThread = null;
				}
			}
		}
	}
}
