namespace OpenCredential.Service.Service
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
            this.openCredentialServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.openCredentialServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // openCredentialServiceProcessInstaller
            // 
            this.openCredentialServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.openCredentialServiceProcessInstaller.Password = null;
            this.openCredentialServiceProcessInstaller.Username = null;
            // 
            // openCredentialServiceInstaller
            // 
            this.openCredentialServiceInstaller.Description = "The OpenCredential management service.";
            this.openCredentialServiceInstaller.DisplayName = "OpenCredential Service";
            this.openCredentialServiceInstaller.ServiceName = "OpenCredential";
            this.openCredentialServiceInstaller.ServicesDependedOn = new string[] {
        "RpcSs"};
            this.openCredentialServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.openCredentialServiceProcessInstaller,
            this.openCredentialServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller openCredentialServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller openCredentialServiceInstaller;
    }
}
