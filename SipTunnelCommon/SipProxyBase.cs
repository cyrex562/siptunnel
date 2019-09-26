using System;
using System.Globalization;
using REGEX = System.Text.RegularExpressions;

namespace SipTunnel
{
	internal abstract class SipProxyBase
		: IDisposable
	{
		protected SipTransportBase m_Transport;

		protected const REGEX.RegexOptions m_RegExOptions = REGEX.RegexOptions.Compiled | REGEX.RegexOptions.CultureInvariant | REGEX.RegexOptions.ExplicitCapture | REGEX.RegexOptions.IgnoreCase | REGEX.RegexOptions.Singleline;
		protected static readonly REGEX.Regex
			g_Via = new REGEX.Regex(@"(?<a>^Via:\s+SIP/2\.0/UDP\s+)((?<addr>(\w|\.|-)+):(?<port>\d+))(?<params>((?<rport>;\s*rport)|(?<stck>;\s*stck\s*=\s*(?<stckVal>\d+))|(;[^;\n\s]+))*)(?<z>\s*$)", m_RegExOptions),
			g_SdpAddressInC = new REGEX.Regex(@"(?<a>^c=\s*IN\s+IP4\s+)(?<q>(\d|\.)+)(?<z>\s*$)", m_RegExOptions),
			g_SdpAddressInO = new REGEX.Regex(@"(?<a>^o=\s*\S+\s+\d+\s+\d+\s+IN\s+IP4\s+)(?<q>(\w|\.|-)+)(?<z>\s*$)", m_RegExOptions),
			g_SdpPortInM = new REGEX.Regex(@"(?<a>^m=\s*audio\s+)(?<q>\d+)(?<z>\s+.*?$)", m_RegExOptions),
			g_From = new REGEX.Regex(@"(?<a>^From:.*?<sip:)(?<acc>(\w|-)+)@(?<host>(\w|-|.)+)(?<z>>.*?$)", m_RegExOptions),
			g_To = new REGEX.Regex(@"(?<a>^To:.*?<sip:(\w|-)+@)(?<q>(\w|-|.)+)(?<z>>.*?$)", m_RegExOptions),
			g_Contact = new REGEX.Regex(@"(?<a>^Contact:.*?<sip:)(?<acc>(\w|-)+)@(?<host>(\w|-|\.)+)(:(?<port>(\d+)))?(?<z>.*?$)", m_RegExOptions),
			g_ServerInMsgType = new REGEX.Regex(@"(?<a>^[a-z]+\s+sip:)((?<acc>(\w|-)+)@)?(?<host>(\w|\.|-)+(:\d+)?)(?<z>.*?\sSIP/2\.0\s*?$)", m_RegExOptions);

		//g_UserAgent = new REGEX.Regex(@"(?<a>User-Agent:\s+.+?)(?<z>\s*\n)", m_RegExOptions),
		//g_UriInAuth = new REGEX.Regex(@"(?<a>Authorization:.+?)(?<q>,\s*uri=""sip:(\w|.|-)+?"")(?<z>.*?\n)", m_RegExOptions);
		//g_Contact = new REGEX.Regex(@"Contact:.+?\n", m_RegExOptions),

		public SipProxyBase()
		{
		}

		protected void CreateTransport(ConnectorType transportType, ProgramSettings settings)
		{
			if (transportType == ConnectorType.UDP)
				m_Transport = new SipTransportUdp(settings);
			else if (transportType == ConnectorType.TCP)
				m_Transport = new SipTransportTcp(settings);

			m_Transport.PipeDead += OnPipeDead;
			m_Transport.SipReceivedFromPipe += OnSipReceivedFromPipe;
			m_Transport.SipReceivedFromClient += OnSipReceivedFromClient;
		}

		protected virtual void OnPipeDead(object sender, EventArgs e)
		{

		}

		protected virtual void OnSipReceivedFromPipe(object sender, SipMessageEventArgs e)
		{

		}

		protected virtual void OnSipReceivedFromClient(object sender, SipMessageEventArgs e)
		{

		}

		protected string CreateSoundProxyAndChangeSdpData(string sipMsg, out SoundProxy sp, out ushort length)
		{
			sp = null;
			length = 0;

			string localIp = m_Transport.LocalAddress.ToString();
			System.IO.StringReader strReader = new System.IO.StringReader(sipMsg);
			System.Text.StringBuilder sbSipMsg = new System.Text.StringBuilder(sipMsg.Length + 16);
			string sipMsgLine, callId = null;
			bool createProxy = false, startCounting = false;
			while ((sipMsgLine = strReader.ReadLine()) != null)
			{
				if (sipMsgLine.StartsWith("Content-Type:", StringComparison.InvariantCultureIgnoreCase))
				{
					createProxy = (sipMsgLine.IndexOf("application/sdp", StringComparison.InvariantCultureIgnoreCase) > 0);
					if (!createProxy)
						return sipMsg;
				}
				else if (sipMsgLine.StartsWith("Call-ID:", StringComparison.InvariantCultureIgnoreCase))
					callId = sipMsgLine.Substring(8).Trim();
				else if (null != sp && sipMsgLine.StartsWith("o=", StringComparison.InvariantCultureIgnoreCase))
					sipMsgLine = g_SdpAddressInO.Replace(sipMsgLine, "${a}" + localIp + "${z}");
				else if (null != sp && sipMsgLine.StartsWith("c=", StringComparison.InvariantCultureIgnoreCase))
					sipMsgLine = g_SdpAddressInC.Replace(sipMsgLine, "${a}" + localIp + "${z}");
				else if (null != sp && sipMsgLine.StartsWith("m=", StringComparison.InvariantCultureIgnoreCase))
					sipMsgLine = g_SdpPortInM.Replace(sipMsgLine, "${a}" + sp.LocalPort.ToString(CultureInfo.InvariantCulture) + "${z}");
				else if (0 == sipMsgLine.Length)
					startCounting = true;

				if (createProxy && null == sp && null != callId && callId.Length > 0)
				{
					sp = m_Transport.CreateSoundProxy(sipMsg, callId, false);
					if (null == sp)
						return sipMsg;
				}

				sbSipMsg.Append(sipMsgLine + "\r\n");
				if (startCounting && sipMsgLine.Length > 0)
					length += (ushort)(sipMsgLine.Length + 2);
			}

			return sbSipMsg.ToString();
		}

		protected ProgramSettings Settings
		{
			get
			{
				return m_Transport.Settings;
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
				m_Transport.Dispose();
				m_Transport = null;
			}
		}
	}
}