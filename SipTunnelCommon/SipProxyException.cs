using System;

namespace SipTunnel
{
	//[Serializable]
	internal class SipProxyException
		: ApplicationException
	{
		//public SipProxyException()
		//  : base()
		//{

		//}

		public SipProxyException(string message)
			: base(message)
		{

		}

		public SipProxyException(string message, Exception innerExcpetion)
			: base(message, innerExcpetion)
		{

		}

		//protected SipProxyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		//  : base(info, context)
		//{

		//}
	}
}
