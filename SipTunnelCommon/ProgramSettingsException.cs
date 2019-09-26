using System;

namespace SipTunnel
{
	internal class ProgramSettingsException
		: ApplicationException
	{
		public ProgramSettingsException(string message)
			: base(message)
		{

		}
	}
}
