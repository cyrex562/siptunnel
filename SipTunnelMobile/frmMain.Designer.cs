namespace SipTunnelMobile
{
	partial class frmMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblSipTunnelServer = new System.Windows.Forms.Label();
			this.lblSipTunnelServerHost = new System.Windows.Forms.Label();
			this.lblSipTunnelServerPort = new System.Windows.Forms.Label();
			this.txtClientServerHost = new System.Windows.Forms.TextBox();
			this.txtClientServerPort = new System.Windows.Forms.TextBox();
			this.lblLocalClient = new System.Windows.Forms.Label();
			this.lblLocalClientIpAddress = new System.Windows.Forms.Label();
			this.lblLocalClientPort = new System.Windows.Forms.Label();
			this.txtClientIp = new System.Windows.Forms.TextBox();
			this.txtClientPort = new System.Windows.Forms.TextBox();
			this.mnuMain = new System.Windows.Forms.MainMenu();
			this.mnuMainStart = new System.Windows.Forms.MenuItem();
			this.munMainExit = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// lblSipTunnelServer
			// 
			this.lblSipTunnelServer.Location = new System.Drawing.Point(3, 11);
			this.lblSipTunnelServer.Name = "lblSipTunnelServer";
			this.lblSipTunnelServer.Size = new System.Drawing.Size(100, 14);
			this.lblSipTunnelServer.Text = "SipTunnel server:";
			// 
			// lblSipTunnelServerHost
			// 
			this.lblSipTunnelServerHost.Location = new System.Drawing.Point(13, 37);
			this.lblSipTunnelServerHost.Name = "lblSipTunnelServerHost";
			this.lblSipTunnelServerHost.Size = new System.Drawing.Size(54, 21);
			this.lblSipTunnelServerHost.Text = "Host:";
			// 
			// lblSipTunnelServerPort
			// 
			this.lblSipTunnelServerPort.Location = new System.Drawing.Point(13, 61);
			this.lblSipTunnelServerPort.Name = "lblSipTunnelServerPort";
			this.lblSipTunnelServerPort.Size = new System.Drawing.Size(54, 21);
			this.lblSipTunnelServerPort.Text = "Port:";
			// 
			// txtClientServerHost
			// 
			this.txtClientServerHost.Location = new System.Drawing.Point(88, 37);
			this.txtClientServerHost.Name = "txtClientServerHost";
			this.txtClientServerHost.Size = new System.Drawing.Size(149, 21);
			this.txtClientServerHost.TabIndex = 4;
			this.txtClientServerHost.TextChanged += new System.EventHandler(this.txt_TextChanged);
			// 
			// txtClientServerPort
			// 
			this.txtClientServerPort.Location = new System.Drawing.Point(88, 61);
			this.txtClientServerPort.Name = "txtClientServerPort";
			this.txtClientServerPort.Size = new System.Drawing.Size(59, 21);
			this.txtClientServerPort.TabIndex = 4;
			this.txtClientServerPort.TextChanged += new System.EventHandler(this.txt_TextChanged);
			// 
			// lblLocalClient
			// 
			this.lblLocalClient.Location = new System.Drawing.Point(3, 101);
			this.lblLocalClient.Name = "lblLocalClient";
			this.lblLocalClient.Size = new System.Drawing.Size(100, 14);
			this.lblLocalClient.Text = "Local client:";
			// 
			// lblLocalClientIpAddress
			// 
			this.lblLocalClientIpAddress.Location = new System.Drawing.Point(13, 127);
			this.lblLocalClientIpAddress.Name = "lblLocalClientIpAddress";
			this.lblLocalClientIpAddress.Size = new System.Drawing.Size(69, 21);
			this.lblLocalClientIpAddress.Text = "IP address:";
			// 
			// lblLocalClientPort
			// 
			this.lblLocalClientPort.Location = new System.Drawing.Point(13, 151);
			this.lblLocalClientPort.Name = "lblLocalClientPort";
			this.lblLocalClientPort.Size = new System.Drawing.Size(54, 21);
			this.lblLocalClientPort.Text = "Port:";
			// 
			// txtClientIp
			// 
			this.txtClientIp.Location = new System.Drawing.Point(88, 127);
			this.txtClientIp.Name = "txtClientIp";
			this.txtClientIp.Size = new System.Drawing.Size(149, 21);
			this.txtClientIp.TabIndex = 4;
			this.txtClientIp.TextChanged += new System.EventHandler(this.txt_TextChanged);
			// 
			// txtClientPort
			// 
			this.txtClientPort.Location = new System.Drawing.Point(88, 151);
			this.txtClientPort.Name = "txtClientPort";
			this.txtClientPort.Size = new System.Drawing.Size(59, 21);
			this.txtClientPort.TabIndex = 4;
			this.txtClientPort.TextChanged += new System.EventHandler(this.txt_TextChanged);
			// 
			// mnuMain
			// 
			this.mnuMain.MenuItems.Add(this.mnuMainStart);
			this.mnuMain.MenuItems.Add(this.munMainExit);
			// 
			// mnuMainStart
			// 
			this.mnuMainStart.Text = "Start";
			this.mnuMainStart.Click += new System.EventHandler(this.mnuMainStart_Click);
			// 
			// munMainExit
			// 
			this.munMainExit.Text = "Exit";
			this.munMainExit.Click += new System.EventHandler(this.munMainExit_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(240, 268);
			this.Controls.Add(this.txtClientPort);
			this.Controls.Add(this.txtClientServerPort);
			this.Controls.Add(this.txtClientIp);
			this.Controls.Add(this.lblLocalClientPort);
			this.Controls.Add(this.txtClientServerHost);
			this.Controls.Add(this.lblLocalClientIpAddress);
			this.Controls.Add(this.lblSipTunnelServerPort);
			this.Controls.Add(this.lblLocalClient);
			this.Controls.Add(this.lblSipTunnelServerHost);
			this.Controls.Add(this.lblSipTunnelServer);
			this.Menu = this.mnuMain;
			this.Name = "frmMain";
			this.Text = "SipTunnel";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblSipTunnelServer;
		private System.Windows.Forms.Label lblSipTunnelServerHost;
		private System.Windows.Forms.Label lblSipTunnelServerPort;
		private System.Windows.Forms.TextBox txtClientServerHost;
		private System.Windows.Forms.TextBox txtClientServerPort;
		private System.Windows.Forms.Label lblLocalClient;
		private System.Windows.Forms.Label lblLocalClientIpAddress;
		private System.Windows.Forms.Label lblLocalClientPort;
		private System.Windows.Forms.TextBox txtClientIp;
		private System.Windows.Forms.TextBox txtClientPort;
		private System.Windows.Forms.MainMenu mnuMain;
		private System.Windows.Forms.MenuItem mnuMainStart;
		private System.Windows.Forms.MenuItem munMainExit;
	}
}