using System;
using System.Windows.Forms;
using System.Globalization;
using DRW = System.Drawing;
using COL = System.Collections.Generic;
using SipTunnel;

namespace SipTunnelMobile
{
	internal partial class frmMain : Form
	{
		private ProgramSettings m_Settings;
		private SipProxyClient m_Client;

		public frmMain()
		{
			InitializeComponent();

			m_Settings = new ProgramSettings(string.Empty);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			txtClientServerHost.Text = m_Settings.ClientServerHost;
			txtClientServerPort.Text = m_Settings.ClientServerPort.ToString(CultureInfo.CurrentUICulture);

			txtClientIp.Text = m_Settings.ClientIp.ToString();
			txtClientPort.Text = m_Settings.ClientPort.ToString(CultureInfo.CurrentUICulture);
		}

		private void txt_TextChanged(object sender, EventArgs e)
		{
			mnuMainStart.Enabled = (null == m_Client);
			bool bOk = false;

			if (0 == txtClientServerHost.Text.Length)
			{
				mnuMainStart.Enabled = false;
				txtClientServerHost.BackColor = DRW.Color.Yellow;
			}
			else
				txtClientServerHost.BackColor = DRW.SystemColors.Window;

			bOk = false;
			if (txtClientServerPort.Text.Length > 0)
			{
				try
				{
					ushort.Parse(txtClientServerPort.Text, NumberStyles.Integer, CultureInfo.CurrentUICulture);
					bOk = true;
				}
				catch (Exception)
				{

				}
			}
			if (!bOk)
			{
				mnuMainStart.Enabled = false;
				txtClientServerPort.BackColor = DRW.Color.Yellow;
			}
			else
				txtClientServerPort.BackColor = DRW.SystemColors.Window;

			bOk = false;
			if (txtClientIp.Text.Length > 0)
			{
				try
				{
					System.Net.IPAddress.Parse(txtClientIp.Text);
					bOk = true;
				}
				catch (Exception)
				{

				}
			}
			if (!bOk)
			{
				mnuMainStart.Enabled = false;
				txtClientIp.BackColor = DRW.Color.Yellow;
			}
			else
				txtClientIp.BackColor = DRW.SystemColors.Window;

			bOk = false;
			if (txtClientPort.Text.Length > 0)
			{
				try
				{
					ushort.Parse(txtClientPort.Text, NumberStyles.Integer, CultureInfo.CurrentUICulture);
					bOk = true;
				}
				catch (Exception)
				{

				}
			}
			if (!bOk)
			{
				mnuMainStart.Enabled = false;
				txtClientPort.BackColor = DRW.Color.Yellow;
			}
			else
				txtClientPort.BackColor = DRW.SystemColors.Window;
		}

		private void munMainExit_Click(object sender, EventArgs e)
		{
			if (null != m_Client)
			{
				m_Client.Dispose();
				m_Client = null;
			}

			this.Close();
		}

		private void mnuMainStart_Click(object sender, EventArgs e)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
			sb.Append(" --enable-client-udp");
			sb.Append(" --client-server-host:" + txtClientServerHost.Text);
			sb.Append(" --client-server-port:" + txtClientServerPort.Text);
			sb.Append(" --client-ip:" + txtClientIp.Text);
			sb.Append(" --client-port:" + txtClientPort.Text);

			lblSipTunnelServer.Enabled = false;
			lblSipTunnelServerHost.Enabled = false;
			txtClientServerHost.Enabled = false;
			lblSipTunnelServerPort.Enabled = false;
			txtClientServerPort.Enabled = false;

			lblLocalClient.Enabled = false;
			lblLocalClientIpAddress.Enabled = false;
			txtClientIp.Enabled = false;
			lblLocalClientPort.Enabled = false;
			txtClientPort.Enabled = false;

			mnuMainStart.Enabled = false;

			m_Settings = new ProgramSettings(sb.ToString());
			m_Client = new SipProxyClient(m_Settings);
		}
	}
}