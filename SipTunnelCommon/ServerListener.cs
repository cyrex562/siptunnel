using System;
using System.Globalization;
using COL = System.Collections.Generic;
using NET = System.Net;
using SOCK = System.Net.Sockets;

namespace SipTunnel
{
	/// <summary>
	/// Listens for incoming connections from clients and created <see cref="SipProxyServer"/> for each connection.
	/// </summary>
	internal class ServerListener
	{
		private readonly SOCK.TcpListener m_tcpListener;
		private readonly COL.Dictionary<NET.IPEndPoint, SipProxyServer> m_ClientConnections = new COL.Dictionary<NET.IPEndPoint, SipProxyServer>(16);

		private readonly ProgramSettings m_Settings;

		public ServerListener(ProgramSettings settings)
		{
			if (null == settings)
				throw new ArgumentNullException("settings");
			if (0 == settings.ServerPort)
				throw new ApplicationException();

			m_Settings = settings;

			m_tcpListener = new SOCK.TcpListener(
				new NET.IPEndPoint(NET.IPAddress.Any, settings.ServerPort)
			);
			m_tcpListener.Start();
			m_tcpListener.BeginAcceptTcpClient(OnConnect, null);

			m_Settings.WriteMessageToLog(
				LogMessageType.Information,
				string.Format(CultureInfo.CurrentUICulture, "SipTunnel server started and is listening on port {0}.", m_Settings.ServerPort)
			);
		}

		private void OnConnect(IAsyncResult ar)
		{
			SOCK.TcpClient newClient = null;
			try
			{
				newClient = m_tcpListener.EndAcceptTcpClient(ar);
			}
			catch (Exception)
			{
			}

			if (null != newClient)
			{
				System.Net.IPEndPoint newEp = (System.Net.IPEndPoint)newClient.Client.RemoteEndPoint;

				m_Settings.WriteMessageToLog(
					LogMessageType.Information + 1,
					string.Format(CultureInfo.CurrentUICulture, "Client {0}:{1} connected.", newEp.Address, newEp.Port)
				);

				SipProxyServer sps = new SipProxyServer(newClient, m_Settings);
				sps.PipeDead += Proxy_PipeDead;
				m_ClientConnections.Add(newEp, sps);
			}

			m_tcpListener.BeginAcceptTcpClient(OnConnect, null);
		}

		void Proxy_PipeDead(object sender, EventArgs e)
		{
			SipProxyServer sps = (SipProxyServer)sender;
			bool removed = m_ClientConnections.Remove(sps.PipeRemoteEndPoint);

			if (removed)
			{
				sps.Dispose();

				m_Settings.WriteMessageToLog(
					LogMessageType.Information,
					string.Format(
					CultureInfo.InstalledUICulture,
					"Removed server SipProxy for {0}:{1}.",
					sps.PipeRemoteEndPoint.Address,
					sps.PipeRemoteEndPoint.Port
					)
				);
			}
		}
	}
}
