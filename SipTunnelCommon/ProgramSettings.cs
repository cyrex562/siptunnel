using System;
using System.Globalization;
using NET = System.Net;
using COL = System.Collections.Generic;

namespace SipTunnel
{
	/// <summary>
	/// Used to hold cmdline parameters.
	/// </summary>
	internal class ProgramSettings
#if PLAT_WINDOWS || PLAT_MONO
		: IProgramSettings
#endif
	{
		private readonly CommandLineParser m_Parser;
		private byte m_VerbosityLevel;
		private NET.IPAddress m_ClientIp = NET.IPAddress.Any, m_ServerIp = NET.IPAddress.Any;
		private ushort m_ClientPort = 5060, m_ServerPort = 11709, m_ClientServerPort = 11709, m_SipServerPort = 5060;

		public const string SipTunnelInternalHost = "atslf982";

		public ProgramSettings(string[] args)
		{
			m_Parser = new CommandLineParser(args);
			CheckSettings();
		}

		public ProgramSettings(string cmdLine)
		{
			m_Parser = new CommandLineParser(cmdLine);
			CheckSettings();
		}

		public ProgramSettings(CommandLineParser parser)
		{
			if (null == parser)
				throw new ArgumentNullException("parser");

			m_Parser = parser;
			CheckSettings();
		}

		private void CheckSettings()
		{
			#region enable-client-xxx
			if (m_Parser.ContainsParameter("enable-client-udp") && null != m_Parser["enable-client-udp"].Values)
				throw new ProgramSettingsException("Parameter 'enable-client-udp' must have no value.");

			if (m_Parser.ContainsParameter("enable-client-tcp") && null != m_Parser["enable-client-tcp"].Values)
				throw new ProgramSettingsException("Parameter 'enable-client-tcp' must have no value.");

			if (m_Parser.ContainsParameter("enable-client-udp") && m_Parser.ContainsParameter("enable-client-tcp"))
				throw new ProgramSettingsException("Only one type of client (UDP or TCP) can be enabled.");
			#endregion

			#region enable-server-xxx
			if (m_Parser.ContainsParameter("enable-server-udp") && null != m_Parser["enable-server-udp"].Values)
				throw new ProgramSettingsException("Parameter 'enable-client-udp' must have no value.");

			if (m_Parser.ContainsParameter("enable-server-tcp") && null != m_Parser["enable-server-tcp"].Values)
				throw new ProgramSettingsException("Parameter 'enable-client-tcp' must have no value.");

			if (m_Parser.ContainsParameter("enable-server-udp") && m_Parser.ContainsParameter("enable-server-tcp"))
				throw new ProgramSettingsException("Only one type of server (UDP or TCP) can be enabled.");
			#endregion

			#region verbosity
			if (m_Parser.ContainsParameter("verbosity"))
			{
				CommandLineParser.Parameter p = m_Parser["verbosity"];
				if (null == p.Values)
					throw new ProgramSettingsException("Parameter 'verbosity' must have value.");

				foreach (string s in p.Values)
				{
					if (0 == s.Length)
						throw new ProgramSettingsException("Parameter 'verbosity' must have value.");

					byte b;
					if (!TryParseByte(s, NumberStyles.Integer, CultureInfo.CurrentUICulture, out b))
						throw new ProgramSettingsException("Verbosity level must be a number.");
				}
			}
			#endregion

			#region verbosity-level
			if (m_Parser.ContainsParameter("verbosity-level"))
			{
				CommandLineParser.Parameter p = m_Parser["verbosity"];
				if (null == p.Values)
					throw new ProgramSettingsException("Parameter 'verbosity' must have value.");

				foreach (string s in p.Values)
				{
					if (0 == s.Length)
						throw new ProgramSettingsException("Parameter 'verbosity' must have value.");

					byte b;
					if (!TryParseByte(s, NumberStyles.Integer, CultureInfo.CurrentUICulture, out b))
						throw new ProgramSettingsException("Verbosity level must be a number.");
				}
			}
			#endregion

			#region v
			foreach (CommandLineParser.Parameter p in m_Parser)
			{
				if (0 == p.Name.Replace("v", string.Empty).Length && null != p.Values)
					throw new ProgramSettingsException("Parameter '" + p.Name + "' must have no value.");
			}
			#endregion

			#region client-ip
			if (m_Parser.ContainsParameter("client-ip"))
			{
				CommandLineParser.Parameter p = m_Parser["client-ip"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'client-ip' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'client-ip' must have value.");
				if (!TryParseIpAddress(p.Values[0], out m_ClientIp))
					throw new ProgramSettingsException("Value for parameter 'client-ip' must be an IP-address.");
			}
			if (NET.IPAddress.Any.Equals(m_ClientIp))
			{
				NET.IPHostEntry ipHe = NET.Dns.GetHostEntry(string.Empty);
				if (ipHe.AddressList.Length > 0)
					m_ClientIp = ipHe.AddressList[0];
				else
					m_ClientIp = NET.IPAddress.Loopback;
			}
			#endregion

			#region client-port
			if (m_Parser.ContainsParameter("client-port"))
			{
				CommandLineParser.Parameter p = m_Parser["client-port"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'client-port' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'client-port' must have value.");
				if (!TryParseUshort(p.Values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out m_ClientPort) || 0 == m_ClientPort)
					throw new ProgramSettingsException("Value for parameter 'client-port' must be a number from 1 to 65535.");
			}
			#endregion

			#region client-server-host
			if (m_Parser.ContainsParameter("client-server-host"))
			{
				CommandLineParser.Parameter p = m_Parser["client-server-host"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'client-server-host' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'client-server-host' must have value.");
			}
			#endregion

			#region client-server-port
			if (m_Parser.ContainsParameter("client-server-port"))
			{
				CommandLineParser.Parameter p = m_Parser["client-server-port"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'client-server-port' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'client-server-port' must have value.");
				if (!TryParseUshort(p.Values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out m_ClientServerPort) || 0 == m_ClientServerPort)
					throw new ProgramSettingsException("Value for parameter 'client-server-port' must be a number from 1 to 65535.");
			}
			#endregion

			#region server-ip
			if (m_Parser.ContainsParameter("server-ip"))
			{
				CommandLineParser.Parameter p = m_Parser["server-ip"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'server-ip' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'server-ip' must have value.");
				if (!TryParseIpAddress(p.Values[0], out m_ServerIp))
					throw new ProgramSettingsException("Value for parameter 'server-ip' must be an IP-address.");
			}
			if (NET.IPAddress.Any.Equals(m_ServerIp))
			{
				NET.IPHostEntry ipHe = NET.Dns.GetHostEntry(string.Empty);
				if (ipHe.AddressList.Length > 0)
					m_ServerIp = ipHe.AddressList[0];
				else
					m_ServerIp = NET.IPAddress.Loopback;
			}
			#endregion

			#region server-port
			if (m_Parser.ContainsParameter("server-port"))
			{
				CommandLineParser.Parameter p = m_Parser["server-port"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'server-port' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'server-port' must have value.");
				if (!TryParseUshort(p.Values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out m_ServerPort) || 0 == m_ServerPort)
					throw new ProgramSettingsException("Value for parameter 'server-port' must be a number from 1 to 65535.");
			}
			#endregion

			#region sip-server-host
			if (m_Parser.ContainsParameter("sip-server-host"))
			{
				CommandLineParser.Parameter p = m_Parser["sip-server-host"];
				if (null == p.Values)
					throw new ProgramSettingsException("Parameter 'sip-server-host' must have value.");

				foreach (string s in p.Values)
				{
					if (0 == s.Length)
						throw new ProgramSettingsException("Parameter 'sip-server-host' must have value.");
				}
			}
			#endregion

			#region sip-server-port
			if (m_Parser.ContainsParameter("sip-server-port"))
			{
				CommandLineParser.Parameter p = m_Parser["sip-server-port"];
				if (p.Count > 1)
					throw new ProgramSettingsException("Parameter 'sip-server-port' can be specified only once.");
				if (null == p.Values || 0 == p.Values[0].Length)
					throw new ProgramSettingsException("Parameter 'sip-server-port' must have value.");
				if (!TryParseUshort(p.Values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out m_SipServerPort) || 0 == m_SipServerPort)
					throw new ProgramSettingsException("Value for parameter 'sip-server-port' must be a number from 1 to 65535.");
			}
			#endregion

			m_VerbosityLevel = VerbosityLebel;
		}

		private static bool TryParseIpAddress(string address, out NET.IPAddress ipAddress)
		{
#if PLAT_WINDOWS || PLAT_MONO
			return NET.IPAddress.TryParse(address, out ipAddress);
#endif

#if PLAT_WINMOBILE
			if (null == address)
				throw new ArgumentNullException("address");
			if (0 == address.Length)
			{
				ipAddress = null;
				return false;
			}

			try
			{
				ipAddress = NET.IPAddress.Parse(address);
				return true;
			}
			catch (Exception)
			{
				ipAddress = null;
				return false;
			}
#endif
		}

		private static bool TryParseByte(string text, NumberStyles ns, IFormatProvider provider, out byte data)
		{
#if PLAT_WINDOWS || PLAT_MONO
			return byte.TryParse(text, ns, provider, out data);
#endif

#if PLAT_WINMOBILE
			if (null == text)
				throw new ArgumentNullException("address");
			if (0 == text.Length)
			{
				data = 0;
				return false;
			}

			try
			{
				data = byte.Parse(text, ns, provider);
				return true;
			}
			catch (Exception)
			{
				data = 0;
				return false;
			}
#endif
		}

		private static bool TryParseUshort(string text, NumberStyles ns, IFormatProvider provider, out ushort data)
		{
#if PLAT_WINDOWS || PLAT_MONO
			return ushort.TryParse(text, ns, provider, out data);
#endif

#if PLAT_WINMOBILE
			if (null == text)
				throw new ArgumentNullException("address");
			if (0 == text.Length)
			{
				data = 0;
				return false;
			}

			try
			{
				data = ushort.Parse(text, ns, provider);
				return true;
			}
			catch (Exception)
			{
				data = 0;
				return false;
			}
#endif
		}

		public static NET.IPAddress IpFromHost(string host)
		{
			if (null == host)
				throw new ArgumentNullException("host");

			NET.IPAddress retVal = NET.IPAddress.Any;
			if (host.Length > 0)
			{
				if (!TryParseIpAddress(host, out retVal))
				{
					NET.IPHostEntry ipHe = NET.Dns.GetHostEntry(host);
					if (null == ipHe || 0 == ipHe.AddressList.Length)
						throw new ApplicationException();

					retVal = ipHe.AddressList[0];
				}
			}

			return retVal;
		}

		public void WriteMessageToLog(LogMessageType msgType, string text)
		{
			WriteMessageToLog((byte)msgType, text);
		}

		public void WriteMessageToLog(byte verbosityLevel, string text)
		{
			if (null == text || 0 == text.Length)
				return;
			if (verbosityLevel > m_VerbosityLevel)
				return;

			Console.WriteLine(text);
			Console.WriteLine();
		}

		private static bool IsHostInList(string host, COL.ICollection<string> list)
		{
			if (null == host || 0 == host.Length)
				return false;
			if (null == list || 0 == list.Count)
				return false;

			if (host.IndexOf(':') >= 0)
				host = host.Substring(0, host.IndexOf(':'));

			NET.IPHostEntry hostEntry = null;
			NET.IPAddress hostIp;
			if (TryParseIpAddress(host, out hostIp))
			{
				hostEntry = new NET.IPHostEntry();
				hostEntry.AddressList = new NET.IPAddress[] { hostIp };
				hostEntry.Aliases = new string[0];
				hostEntry.HostName = string.Empty;
			}
			else
			{
				hostEntry = NET.Dns.GetHostEntry(host);
			}

			foreach (string h in list)
			{
				if (h.Equals(host, StringComparison.InvariantCultureIgnoreCase))
					return true;

				NET.IPHostEntry ipHe = NET.Dns.GetHostEntry(h);

				if (ipHe.HostName.Equals(host, StringComparison.InvariantCultureIgnoreCase))
					return true;

				foreach (string al in hostEntry.Aliases)
				{
					if (al.Equals(h, StringComparison.InvariantCultureIgnoreCase))
						return true;
					if (al.Equals(ipHe.HostName))
						return true;
				}

				foreach (string alHost in hostEntry.Aliases)
				{
					foreach (string al in ipHe.Aliases)
					{
						if (al.Equals(alHost, StringComparison.InvariantCultureIgnoreCase))
							return true;
					}
				}

				foreach (NET.IPAddress hIp in hostEntry.AddressList)
				{
					foreach (NET.IPAddress ip in ipHe.AddressList)
					{
						if (hIp.Equals(ip))
							return true;
					}
				}
			}

			return false;
		}

		public bool IsClientHost(string host)
		{
			if (null == host || 0 == host.Length)
				return false;

			NET.IPHostEntry ipHe = NET.Dns.GetHostEntry(string.Empty);
			COL.List<string> localHosts = new COL.List<string>(16);

			foreach (NET.IPAddress ip in ipHe.AddressList)
				localHosts.Add(ip.ToString());

			localHosts.AddRange(ipHe.Aliases);
			localHosts.Add(ipHe.HostName);
			localHosts.Add(m_ClientIp.ToString());

			return IsHostInList(host, localHosts);
		}

		public bool IsSipServerHost(string host)
		{
			COL.ICollection<string> hosts;
			if (m_Parser.ContainsParameter("sip-server-host"))
				hosts = m_Parser["sip-server-host"].Values;
			else
				hosts = new string[] { "localhost" };

			return IsHostInList(host, hosts);
		}

		private byte VerbosityLebel
		{
			get
			{
				byte maxLevel = 0;

				if (m_Parser.ContainsParameter("verbosity"))
				{
					foreach (string s in m_Parser["verbosity"].Values)
					{
						byte b = byte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
						if (b > maxLevel)
							maxLevel = b;
					}
				}

				if (m_Parser.ContainsParameter("verbosity-level"))
				{
					foreach (string s in m_Parser["verbosity-level"].Values)
					{
						byte b = byte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
						if (b > maxLevel)
							maxLevel = b;
					}
				}

				foreach (CommandLineParser.Parameter p in m_Parser)
				{
					if (0 == p.Name.Replace("v", string.Empty).Length && maxLevel < p.Name.Length)
						maxLevel = (byte)p.Name.Length;
				}

				return maxLevel;
			}
		}

		public ConnectorType ClientType
		{
			get
			{
				if (m_Parser.ContainsParameter("enable-client-udp"))
					return ConnectorType.UDP;
				else if (m_Parser.ContainsParameter("enable-client-tcp"))
					return ConnectorType.TCP;
				else
					return ConnectorType.None;
			}
		}

		public NET.IPAddress ClientIp
		{
			get
			{
				return m_ClientIp;
			}
		}

		public ushort ClientPort
		{
			get
			{
				return m_ClientPort;
			}
		}

		public string ClientServerHost
		{
			get
			{
				if (m_Parser.ContainsParameter("client-server-host"))
					return m_Parser["client-server-host"].Values[0];

				return "localhost";
			}
		}

		public NET.IPAddress ClientServerHostIp
		{
			get
			{
				return IpFromHost(ClientServerHost);
			}
		}

		public ushort ClientServerPort
		{
			get
			{
				return m_ClientServerPort;
			}
		}

		public ConnectorType ServerType
		{
			get
			{
				if (m_Parser.ContainsParameter("enable-server-udp"))
					return ConnectorType.UDP;
				else if (m_Parser.ContainsParameter("enable-server-tcp"))
					return ConnectorType.TCP;
				else
					return ConnectorType.None;
			}
		}

		public NET.IPAddress ServerIp
		{
			get
			{
				return m_ServerIp;
			}
		}

		public ushort ServerPort
		{
			get
			{
				return m_ServerPort;
			}
		}

		public string SipServerHost
		{
			get
			{
				if (m_Parser.ContainsParameter("sip-server-host"))
					return m_Parser["sip-server-host"].Values[0];

				return "localhost";
			}
		}

		public ushort SipServerPort
		{
			get
			{
				return m_SipServerPort;
			}
		}

#if PLAT_WINDOWS || PLAT_MONO

		int IProgramSettings.ServerPort
		{
			get
			{
				return ServerPort;
			}
		}

		int IProgramSettings.ClientServerPort
		{
			get
			{
				return ClientServerPort;
			}
		}

		int IProgramSettings.ClientPort
		{
			get
			{
				return ClientPort;
			}
		}

		int IProgramSettings.SipServerPort
		{
			get
			{
				return SipServerPort;
			}
		}

		ICommandLineParser IProgramSettings.CommandLineParser
		{
			get
			{
				return m_Parser;
			}
		}

#endif
	}
}
