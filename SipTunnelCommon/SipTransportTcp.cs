using System;
using NET = System.Net;

namespace SipTunnel
{
	internal class SipTransportTcp
		: SipTransportBase
	{
		public SipTransportTcp(ProgramSettings settings)
			: base(settings)
		{

		}

		public override void PrepareClient(System.Net.IPEndPoint localEp)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void SendSipMsgToClient(string sipMsg, NET.IPEndPoint clientEp)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override System.Net.IPAddress LocalAddress
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public override ushort LocalPort
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}
	}
}
