﻿using System;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SipTunnelWin")]
[assembly: AssemblyDescription("Sipunnel for Windows")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("BoresSoft")]
[assembly: AssemblyProduct("SipTunnel")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly:CLSCompliant(true)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("0.9.*")]

[assembly: System.Net.SocketPermission(SecurityAction.RequestMinimum, Unrestricted=true)]