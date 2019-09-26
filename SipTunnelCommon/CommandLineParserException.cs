using System;

namespace SipTunnel
{
	internal class CommandLineParserException
		: ApplicationException
	{
		public CommandLineParserException(string message)
			: base(message)
		{

		}
	}
}
