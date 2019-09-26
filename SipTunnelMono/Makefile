#
# Makefile for SipTunnel
# SipTunnel requires Mono to run and gmcs (Mono C# compiler 2.0) to compile
#

SOURCE_MONO = \
 Program.cs \
 AssemblyInfo.cs

SOURCE_COMMON = \
 ../SipTunnelCommon/CommandLineParser.cs \
 ../SipTunnelCommon/CommandLineParserException.cs \
 ../SipTunnelCommon/ProgramSettings.cs \
 ../SipTunnelCommon/ProgramSettingsException.cs \
 ../SipTunnelCommon/ServerListener.cs \
 ../SipTunnelCommon/SipProxyException.cs \
 ../SipTunnelCommon/SipProxyBase.cs \
 ../SipTunnelCommon/SipProxyClient.cs \
 ../SipTunnelCommon/SipProxyClientException.cs \
 ../SipTunnelCommon/SipProxyServer.cs \
 ../SipTunnelCommon/SoundProxy.cs \
 ../SipTunnelCommon/SipTransportBase.cs \
 ../SipTunnelCommon/SipTransportUdp.cs \
 ../SipTunnelCommon/SipTransportTcp.cs \
 ../SipTunnelCommon/Interfaces.cs \
 ../SipTunnelCommon/MessageEventArgs.cs

TARGET = \
 SipTunnelMono.bin



$(TARGET) : $(SOURCE_MONO) $(SOURCE_COMMON)
	gmcs -debug- -optimize+ -target:exe -out:$(TARGET) -define:PLAT_MONO $(SOURCE_MONO) $(SOURCE_COMMON)

clean:
	rm -f $(TARGET)