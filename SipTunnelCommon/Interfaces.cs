using System;

namespace SipTunnel
{
	public enum LogMessageType
		: byte
	{
		Error,
		Warning,
		Information
	}

	public enum ConnectorType
	{
		None,
		UDP,
		TCP
	}

#if PLAT_WINDOWS || PLAT_MONO
	public interface IParameter
	{
		string Name
		{
			get;
		}

		byte Count
		{
			get;
		}

		System.Collections.Generic.IList<string> Values
		{
			get;
		}
	}

	public interface ICommandLineParser
	{
		bool ContainsParameter(string name);

		IParameter this[string name]
		{
			get;
		}
	}

	public interface IProgramSettings
	{
		void WriteMessageToLog(LogMessageType msgType, string text);

		void WriteMessageToLog(byte verbosityLevel, string text);

		ConnectorType ClientType
		{
			get;
		}

		System.Net.IPAddress ClientIp
		{
			get;
		}

		int ClientPort
		{
			get;
		}

		string ClientServerHost
		{
			get;
		}

		int ClientServerPort
		{
			get;
		}

		ConnectorType ServerType
		{
			get;
		}

		System.Net.IPAddress ServerIp
		{
			get;
		}

		int ServerPort
		{
			get;
		}

		string SipServerHost
		{
			get;
		}

		int SipServerPort
		{
			get;
		}

		ICommandLineParser CommandLineParser
		{
			get;
		}
	}

	public interface ISipTunnelPlugin
	{
		bool Initialize(IProgramSettings settings);

		bool OnSipReceiveFromPipe(string sipMsg);

		bool OnSipSendToPipe(string sipMsg);

		//void OnRtpReceiveFromPipe(RtpMessageEventArgs e);

		//void OnRtpSendToPipe(RtpMessageEventArgs e);

		bool OnSipReceiveFromClient(string sipMsg);

		bool OnSipSendToClient(string sipMsg);

		//void OnRtpReceiveFromClient(RtpMessageEventArgs e);

		//void OnRtpSendToClient(RtpMessageEventArgs e);

		string Name
		{
			get;
		}
	}

	public interface ISipTunnelServerPlugin
		: ISipTunnelPlugin
	{

	}

	public interface ISipTunnelClientPlugin
	: ISipTunnelPlugin
	{

	}
#endif
}