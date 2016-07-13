namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	partial class ProjectInstaller
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.remarQServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.remarQServiceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller1
			// 
			this.remarQServiceProcessInstaller.Password = null;
			this.remarQServiceProcessInstaller.Username = null;
			this.remarQServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
			// 
			// serviceInstaller1
			// 
			this.remarQServiceInstaller.ServiceName = "Service1";
			this.remarQServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.remarQServiceProcessInstaller,
            this.remarQServiceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller remarQServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller remarQServiceInstaller;
	}
}