namespace pGina.Service.Service
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
            this.pGinaServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.pGinaServiceProjectInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // pGinaServiceProcessInstaller
            // 
            this.pGinaServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.pGinaServiceProcessInstaller.Password = null;
            this.pGinaServiceProcessInstaller.Username = null;
            // 
            // pGinaServiceProjectInstaller
            // 
            this.pGinaServiceProjectInstaller.Description = "pGina Management Service";
            this.pGinaServiceProjectInstaller.DisplayName = "pGina Service";
            this.pGinaServiceProjectInstaller.ServiceName = "pGina";
            this.pGinaServiceProjectInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.pGinaServiceProcessInstaller,
            this.pGinaServiceProjectInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller pGinaServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller pGinaServiceProjectInstaller;
    }
}