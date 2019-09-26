using System;
using NET = System.Net;
using SOCK = System.Net.Sockets;

namespace SipTunnel
{
	internal class SoundProxy
		: IDisposable
	{
		private System.Net.Sockets.UdpClient m_udpClient;

		private readonly byte[] m_BranchAscii;

		private readonly SipTransportBase m_SipTransport;

		//private string m_RemoteHost;
		//private ushort m_RemotePort;
		private NET.IPEndPoint m_RemoteEp;
		private bool m_RemoteHostSet, m_bShuttingDown;

		private System.Threading.Thread m_udpReceiveThread;

		public SoundProxy(string branch, SipTransportBase sipProxy)
		{
			if (null == sipProxy)
				throw new ArgumentNullException("settings");
			if (null == branch)
				throw new ArgumentNullException("branch");
			if (0 == branch.Length)
				throw new ArgumentException("Branch not specified.");
			if (branch.Length > 250)
				throw new ArgumentException("Branch is too long.", "branch");

			m_SipTransport = sipProxy;
			m_BranchAscii = SipTransportBase.g_Ascii.GetBytes(branch);

			m_udpClient = new System.Net.Sockets.UdpClient(
				new NET.IPEndPoint(m_SipTransport.LocalAddress, 0)
			);

			m_udpReceiveThread = new System.Threading.Thread(UdpReceiveProc);
			m_udpReceiveThread.Start();
		}

		public SoundProxy(string sipServerHost, ushort sipServerPort, string branch, SipTransportBase sipProxy)
			: this(branch, sipProxy)
		{
			SetServer(sipServerHost, sipServerPort);
		}

		private void UdpReceiveProc()
		{
			while (!m_bShuttingDown)
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
					byte[] newData = new byte[1 + m_BranchAscii.Length + data.Length];
					newData[0] = (byte)m_BranchAscii.Length;
					m_BranchAscii.CopyTo(newData, 1);
					data.CopyTo(newData, 1 + m_BranchAscii.Length);

					m_SipTransport.SendToPipe(newData);
				}
			}
		}

		public void SendSoundDataToServer(byte[] data)
		{
			if (null == data)
				throw new ArgumentNullException();

			m_udpClient.Send(data, data.Length, m_RemoteEp);
		}

		public void SetServer(string sipServerHost, ushort sipServerPort)
		{
			if (m_RemoteHostSet)
				throw new InvalidOperationException();
			if (null == sipServerHost)
				throw new ArgumentNullException("sipServerHost");

			m_RemoteEp = new NET.IPEndPoint(
				ProgramSettings.IpFromHost(sipServerHost),
				sipServerPort
			);
			m_RemoteHostSet = true;
		}

		public static bool GetSoundData(byte[] data, int dataLen, out string branch, out byte[] soundData)
		{
			branch = null;
			soundData = null;

			if (null == data || dataLen < 2)
				return false;

			branch = SipTransportBase.g_Ascii.GetString(data, 1, data[0]);

			int myDataLen = 1 + data[0];
			soundData = new byte[dataLen - myDataLen];
			Array.Copy(data, myDataLen, soundData, 0, soundData.Length);

			return true;
		}

		public ushort LocalPort
		{
			get
			{
				return (ushort)((System.Net.IPEndPoint)m_udpClient.Client.LocalEndPoint).Port;
			}
		}

		//public string Branch
		//{
		//  get
		//  {
		//    return m_Branch;
		//  }
		//}

		public bool ServerSet
		{
			get
			{
				return m_RemoteHostSet;
			}
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
				m_bShuttingDown = true;

				m_udpClient.Close();
				m_udpClient = null;

				m_udpReceiveThread.Join(3000);
				m_udpReceiveThread = null;
			}
		}
	}
}
