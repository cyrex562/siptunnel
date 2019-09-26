using System;
using NET = System.Net;

namespace SipTunnel
{
	internal class SipMessageEventArgs
		: System.ComponentModel.CancelEventArgs
	{
		private string m_SipMsg;
		private NET.IPEndPoint m_RemoteEp;

		public SipMessageEventArgs(string sipMsg)
			: this(sipMsg, null)
		{

		}

		public SipMessageEventArgs(string sipMsg, NET.IPEndPoint remoteEp)
		{
			if (null == sipMsg)
				throw new ArgumentNullException("sipMsg");

			m_SipMsg = sipMsg;
			m_RemoteEp = remoteEp;
		}

		public string SipMessage
		{
			get
			{
				return m_SipMsg;
			}
		}

		public NET.IPEndPoint RemoteEndPoint
		{
			get
			{
				return m_RemoteEp;
			}
		}
	}

	internal class RtpMessageEventArgs
		: EventArgs
	{
		private byte[] m_RtpData;
		private NET.IPEndPoint m_RemoteEp;

		public RtpMessageEventArgs(byte[] rtpData)
			: this(rtpData, null)
		{

		}

		public RtpMessageEventArgs(byte[] rtpData, NET.IPEndPoint remoteEp)
		{
			if (null == rtpData)
				throw new ArgumentNullException("rtpData");

			m_RtpData = rtpData;
			m_RemoteEp = remoteEp;
		}

		public byte[] RtpData
		{
			get
			{
				return m_RtpData;
			}
		}

		public NET.IPEndPoint RemoteEndPoint
		{
			get
			{
				return m_RemoteEp;
			}
		}
	}
}
