using System;
using SipTunnel;

namespace SipTunnelWin
{
	internal class Program
	{
		private static ProgramSettings m_ProgSettings;
		private static ServerListener m_Server;
		private static SipProxyClient m_Client;

		public static void Main(string[] args)
		{
			m_ProgSettings = new ProgramSettings(args);

			if (m_ProgSettings.ServerType != ConnectorType.None)
				m_Server = new ServerListener(m_ProgSettings);

			if (m_ProgSettings.ClientType != ConnectorType.None)
				m_Client = new SipProxyClient(m_ProgSettings);

			// Use Ctrl+C to break execution
			System.Threading.Thread.CurrentThread.Suspend();
		}
	}
}
