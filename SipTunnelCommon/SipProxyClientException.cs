using System;

namespace SipTunnel
{
	//[Serializable]
	internal class SipProxyClientException
		: SipProxyException
	{
		//public SipProxyClientException()
		//  : base()
		//{

		//}

		public SipProxyClientException(string message)
			: base(message)
		{

		}

		public SipProxyClientException(string message, Exception innerExcpetion)
			: base(message, innerExcpetion)
		{

		}

		//protected SipProxyClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		//  : base(info, context)
		//{

		//}
	}
}
