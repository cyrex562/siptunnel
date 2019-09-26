using System;

namespace SipTunnel
{
	//[Serializable]
	internal class SipProxyServerException
		: SipProxyException
	{
		//public SipProxyServerException()
		//  : base()
		//{

		//}

		public SipProxyServerException(string message)
			: base(message)
		{

		}

		public SipProxyServerException(string message, Exception innerExcpetion)
			: base(message, innerExcpetion)
		{

		}

		//protected SipProxyServerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		//  : base(info, context)
		//{

		//}
	}
}
