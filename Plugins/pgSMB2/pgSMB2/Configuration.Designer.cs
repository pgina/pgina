/*
	Copyright (c) 2014, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors
		  may be used to endorse or promote products derived from this software without
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace pGina.Plugin.pgSMB2
{
    partial class Configuration
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
            this.Gateway_group = new System.Windows.Forms.GroupBox();
            this.ntp = new System.Windows.Forms.TextBox();
            this.Description = new System.Windows.Forms.Label();
            this.ntp_label = new System.Windows.Forms.Label();
            this.User_group = new System.Windows.Forms.GroupBox();
            this.MaxStore_calc = new System.Windows.Forms.Label();
            this.MaxStore_label = new System.Windows.Forms.Label();
            this.MaxStore = new System.Windows.Forms.NumericUpDown();
            this.HomeDir = new System.Windows.Forms.TextBox();
            this.ScriptPath_label = new System.Windows.Forms.Label();
            this.HomeDir_label = new System.Windows.Forms.Label();
            this.ScriptPath = new System.Windows.Forms.TextBox();
            this.HomeDirDrive = new System.Windows.Forms.TextBox();
            this.HomeDirDrive_label = new System.Windows.Forms.Label();
            this.Roaming_group = new System.Windows.Forms.GroupBox();
            this.CompressCLI = new System.Windows.Forms.TextBox();
            this.CompressCLI_label = new System.Windows.Forms.Label();
            this.UncompressCLI = new System.Windows.Forms.TextBox();
            this.UncompressCLI_label = new System.Windows.Forms.Label();
            this.Compressor = new System.Windows.Forms.TextBox();
            this.Compressor_label = new System.Windows.Forms.Label();
            this.ConnectRetry_label = new System.Windows.Forms.Label();
            this.ConnectRetry = new System.Windows.Forms.NumericUpDown();
            this.RoamingDest_label = new System.Windows.Forms.Label();
            this.RoamingDest = new System.Windows.Forms.TextBox();
            this.Filename_label = new System.Windows.Forms.Label();
            this.Filename = new System.Windows.Forms.TextBox();
            this.RoamingSource_label = new System.Windows.Forms.Label();
            this.RoamingSource = new System.Windows.Forms.TextBox();
            this.SMBshare_label = new System.Windows.Forms.Label();
            this.SMBshare = new System.Windows.Forms.TextBox();
            this.save = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.email = new System.Windows.Forms.TextBox();
            this.email_label = new System.Windows.Forms.Label();
            this.smtp = new System.Windows.Forms.TextBox();
            this.smtp_label = new System.Windows.Forms.Label();
            this.help = new System.Windows.Forms.Button();
            this.Gateway_group.SuspendLayout();
            this.User_group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStore)).BeginInit();
            this.Roaming_group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConnectRetry)).BeginInit();
            this.SuspendLayout();
            //
            // Gateway_group
            //
            this.Gateway_group.Controls.Add(this.smtp);
            this.Gateway_group.Controls.Add(this.smtp_label);
            this.Gateway_group.Controls.Add(this.email);
            this.Gateway_group.Controls.Add(this.email_label);
            this.Gateway_group.Controls.Add(this.ntp);
            this.Gateway_group.Controls.Add(this.Description);
            this.Gateway_group.Controls.Add(this.ntp_label);
            this.Gateway_group.Controls.Add(this.User_group);
            this.Gateway_group.Controls.Add(this.Roaming_group);
            this.Gateway_group.Location = new System.Drawing.Point(12, 12);
            this.Gateway_group.Name = "Gateway_group";
            this.Gateway_group.Size = new System.Drawing.Size(668, 362);
            this.Gateway_group.TabIndex = 0;
            this.Gateway_group.TabStop = false;
            this.Gateway_group.Text = "Gateway";
            //
            // ntp
            //
            this.ntp.Location = new System.Drawing.Point(345, 221);
            this.ntp.Name = "ntp";
            this.ntp.Size = new System.Drawing.Size(311, 20);
            this.ntp.TabIndex = 26;
            this.ntp.WordWrap = false;
            //
            // Description
            //
            this.Description.AutoSize = true;
            this.Description.Location = new System.Drawing.Point(333, 325);
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(257, 26);
            this.Description.TabIndex = 4;
            this.Description.Text = "Macro %u = UserName\r\nany other Environment Varaiable will also be resolved";
            //
            // ntp_label
            //
            this.ntp_label.AutoSize = true;
            this.ntp_label.Location = new System.Drawing.Point(342, 205);
            this.ntp_label.Name = "ntp_label";
            this.ntp_label.Size = new System.Drawing.Size(198, 13);
            this.ntp_label.TabIndex = 27;
            this.ntp_label.Text = "space seperated ntp FQDN servers (ntp)";
            //
            // User_group
            //
            this.User_group.Controls.Add(this.MaxStore_calc);
            this.User_group.Controls.Add(this.MaxStore_label);
            this.User_group.Controls.Add(this.MaxStore);
            this.User_group.Controls.Add(this.HomeDir);
            this.User_group.Controls.Add(this.ScriptPath_label);
            this.User_group.Controls.Add(this.HomeDir_label);
            this.User_group.Controls.Add(this.ScriptPath);
            this.User_group.Controls.Add(this.HomeDirDrive);
            this.User_group.Controls.Add(this.HomeDirDrive_label);
            this.User_group.Location = new System.Drawing.Point(339, 19);
            this.User_group.Name = "User_group";
            this.User_group.Size = new System.Drawing.Size(323, 180);
            this.User_group.TabIndex = 3;
            this.User_group.TabStop = false;
            this.User_group.Text = "User";
            //
            // MaxStore_calc
            //
            this.MaxStore_calc.AutoSize = true;
            this.MaxStore_calc.Location = new System.Drawing.Point(105, 153);
            this.MaxStore_calc.Name = "MaxStore_calc";
            this.MaxStore_calc.Size = new System.Drawing.Size(0, 13);
            this.MaxStore_calc.TabIndex = 25;
            //
            // MaxStore_label
            //
            this.MaxStore_label.AutoSize = true;
            this.MaxStore_label.Location = new System.Drawing.Point(3, 134);
            this.MaxStore_label.Name = "MaxStore_label";
            this.MaxStore_label.Size = new System.Drawing.Size(240, 13);
            this.MaxStore_label.TabIndex = 20;
            this.MaxStore_label.Text = "The user max storage space in kbytes (MaxStore)";
            //
            // MaxStore
            //
            this.MaxStore.Location = new System.Drawing.Point(6, 151);
            this.MaxStore.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.MaxStore.Name = "MaxStore";
            this.MaxStore.Size = new System.Drawing.Size(93, 20);
            this.MaxStore.TabIndex = 19;
            this.MaxStore.ValueChanged += new System.EventHandler(this.MaxStore_MB);
            //
            // HomeDir
            //
            this.HomeDir.Location = new System.Drawing.Point(6, 32);
            this.HomeDir.Name = "HomeDir";
            this.HomeDir.Size = new System.Drawing.Size(311, 20);
            this.HomeDir.TabIndex = 19;
            this.HomeDir.WordWrap = false;
            //
            // ScriptPath_label
            //
            this.ScriptPath_label.AutoSize = true;
            this.ScriptPath_label.Location = new System.Drawing.Point(3, 95);
            this.ScriptPath_label.Name = "ScriptPath_label";
            this.ScriptPath_label.Size = new System.Drawing.Size(117, 13);
            this.ScriptPath_label.TabIndex = 24;
            this.ScriptPath_label.Text = "Script Path (ScriptPath)";
            //
            // HomeDir_label
            //
            this.HomeDir_label.AutoSize = true;
            this.HomeDir_label.Location = new System.Drawing.Point(6, 16);
            this.HomeDir_label.Name = "HomeDir_label";
            this.HomeDir_label.Size = new System.Drawing.Size(143, 13);
            this.HomeDir_label.TabIndex = 20;
            this.HomeDir_label.Text = "The user HomeDir (HomeDir)";
            //
            // ScriptPath
            //
            this.ScriptPath.Location = new System.Drawing.Point(6, 111);
            this.ScriptPath.Name = "ScriptPath";
            this.ScriptPath.Size = new System.Drawing.Size(311, 20);
            this.ScriptPath.TabIndex = 23;
            this.ScriptPath.WordWrap = false;
            //
            // HomeDirDrive
            //
            this.HomeDirDrive.Location = new System.Drawing.Point(6, 72);
            this.HomeDirDrive.Name = "HomeDirDrive";
            this.HomeDirDrive.Size = new System.Drawing.Size(311, 20);
            this.HomeDirDrive.TabIndex = 21;
            this.HomeDirDrive.WordWrap = false;
            //
            // HomeDirDrive_label
            //
            this.HomeDirDrive_label.AutoSize = true;
            this.HomeDirDrive_label.Location = new System.Drawing.Point(3, 55);
            this.HomeDirDrive_label.Name = "HomeDirDrive_label";
            this.HomeDirDrive_label.Size = new System.Drawing.Size(193, 13);
            this.HomeDirDrive_label.TabIndex = 22;
            this.HomeDirDrive_label.Text = "The user HomeDirDrive (HomeDirDrive)";
            //
            // Roaming_group
            //
            this.Roaming_group.Controls.Add(this.CompressCLI);
            this.Roaming_group.Controls.Add(this.CompressCLI_label);
            this.Roaming_group.Controls.Add(this.UncompressCLI);
            this.Roaming_group.Controls.Add(this.UncompressCLI_label);
            this.Roaming_group.Controls.Add(this.Compressor);
            this.Roaming_group.Controls.Add(this.Compressor_label);
            this.Roaming_group.Controls.Add(this.ConnectRetry_label);
            this.Roaming_group.Controls.Add(this.ConnectRetry);
            this.Roaming_group.Controls.Add(this.RoamingDest_label);
            this.Roaming_group.Controls.Add(this.RoamingDest);
            this.Roaming_group.Controls.Add(this.Filename_label);
            this.Roaming_group.Controls.Add(this.Filename);
            this.Roaming_group.Controls.Add(this.RoamingSource_label);
            this.Roaming_group.Controls.Add(this.RoamingSource);
            this.Roaming_group.Controls.Add(this.SMBshare_label);
            this.Roaming_group.Controls.Add(this.SMBshare);
            this.Roaming_group.Location = new System.Drawing.Point(6, 19);
            this.Roaming_group.Name = "Roaming_group";
            this.Roaming_group.Size = new System.Drawing.Size(327, 337);
            this.Roaming_group.TabIndex = 2;
            this.Roaming_group.TabStop = false;
            this.Roaming_group.Text = "Roaming Profile";
            //
            // CompressCLI
            //
            this.CompressCLI.Location = new System.Drawing.Point(6, 306);
            this.CompressCLI.Name = "CompressCLI";
            this.CompressCLI.Size = new System.Drawing.Size(315, 20);
            this.CompressCLI.TabIndex = 18;
            this.CompressCLI.WordWrap = false;
            //
            // CompressCLI_label
            //
            this.CompressCLI_label.AutoSize = true;
            this.CompressCLI_label.Location = new System.Drawing.Point(3, 290);
            this.CompressCLI_label.Name = "CompressCLI_label";
            this.CompressCLI_label.Size = new System.Drawing.Size(256, 13);
            this.CompressCLI_label.TabIndex = 17;
            this.CompressCLI_label.Text = "The command to compress the Profile (CompressCLI)";
            //
            // UncompressCLI
            //
            this.UncompressCLI.Location = new System.Drawing.Point(6, 267);
            this.UncompressCLI.Name = "UncompressCLI";
            this.UncompressCLI.Size = new System.Drawing.Size(315, 20);
            this.UncompressCLI.TabIndex = 16;
            this.UncompressCLI.WordWrap = false;
            //
            // UncompressCLI_label
            //
            this.UncompressCLI_label.AutoSize = true;
            this.UncompressCLI_label.Location = new System.Drawing.Point(3, 251);
            this.UncompressCLI_label.Name = "UncompressCLI_label";
            this.UncompressCLI_label.Size = new System.Drawing.Size(281, 13);
            this.UncompressCLI_label.TabIndex = 15;
            this.UncompressCLI_label.Text = "The command to uncompress the Profile (UncompressCLI)";
            //
            // Compressor
            //
            this.Compressor.Location = new System.Drawing.Point(6, 228);
            this.Compressor.Name = "Compressor";
            this.Compressor.Size = new System.Drawing.Size(315, 20);
            this.Compressor.TabIndex = 14;
            this.Compressor.WordWrap = false;
            //
            // Compressor_label
            //
            this.Compressor_label.AutoSize = true;
            this.Compressor_label.Location = new System.Drawing.Point(3, 212);
            this.Compressor_label.Name = "Compressor_label";
            this.Compressor_label.Size = new System.Drawing.Size(262, 13);
            this.Compressor_label.TabIndex = 13;
            this.Compressor_label.Text = "The Programm to un-comress the Profile (Compressor)";
            //
            // ConnectRetry_label
            //
            this.ConnectRetry_label.AutoSize = true;
            this.ConnectRetry_label.Location = new System.Drawing.Point(3, 173);
            this.ConnectRetry_label.Name = "ConnectRetry_label";
            this.ConnectRetry_label.Size = new System.Drawing.Size(273, 13);
            this.ConnectRetry_label.TabIndex = 12;
            this.ConnectRetry_label.Text = "Try n times to connect/extract/compress (ConnectRetry)";
            //
            // ConnectRetry
            //
            this.ConnectRetry.Location = new System.Drawing.Point(6, 189);
            this.ConnectRetry.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.ConnectRetry.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ConnectRetry.Name = "ConnectRetry";
            this.ConnectRetry.Size = new System.Drawing.Size(56, 20);
            this.ConnectRetry.TabIndex = 11;
            this.ConnectRetry.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            //
            // RoamingDest_label
            //
            this.RoamingDest_label.AutoSize = true;
            this.RoamingDest_label.Location = new System.Drawing.Point(3, 134);
            this.RoamingDest_label.Name = "RoamingDest_label";
            this.RoamingDest_label.Size = new System.Drawing.Size(226, 13);
            this.RoamingDest_label.TabIndex = 10;
            this.RoamingDest_label.Text = "Where to extract the Profile (RoamingDest) %d";
            //
            // RoamingDest
            //
            this.RoamingDest.Location = new System.Drawing.Point(6, 150);
            this.RoamingDest.Name = "RoamingDest";
            this.RoamingDest.Size = new System.Drawing.Size(315, 20);
            this.RoamingDest.TabIndex = 9;
            this.RoamingDest.WordWrap = false;
            //
            // Filename_label
            //
            this.Filename_label.AutoSize = true;
            this.Filename_label.Location = new System.Drawing.Point(3, 95);
            this.Filename_label.Name = "Filename_label";
            this.Filename_label.Size = new System.Drawing.Size(251, 13);
            this.Filename_label.TabIndex = 8;
            this.Filename_label.Text = "The name and extension of the Profile (Filename) %f";
            //
            // Filename
            //
            this.Filename.Location = new System.Drawing.Point(6, 111);
            this.Filename.Name = "Filename";
            this.Filename.Size = new System.Drawing.Size(315, 20);
            this.Filename.TabIndex = 7;
            this.Filename.WordWrap = false;
            //
            // RoamingSource_label
            //
            this.RoamingSource_label.AutoSize = true;
            this.RoamingSource_label.Location = new System.Drawing.Point(3, 55);
            this.RoamingSource_label.Name = "RoamingSource_label";
            this.RoamingSource_label.Size = new System.Drawing.Size(286, 13);
            this.RoamingSource_label.TabIndex = 6;
            this.RoamingSource_label.Text = "Where to store the compressed Profile (RoamingSource) %r";
            //
            // RoamingSource
            //
            this.RoamingSource.Location = new System.Drawing.Point(6, 72);
            this.RoamingSource.Name = "RoamingSource";
            this.RoamingSource.Size = new System.Drawing.Size(315, 20);
            this.RoamingSource.TabIndex = 5;
            this.RoamingSource.WordWrap = false;
            //
            // SMBshare_label
            //
            this.SMBshare_label.AutoSize = true;
            this.SMBshare_label.Location = new System.Drawing.Point(6, 16);
            this.SMBshare_label.Name = "SMBshare_label";
            this.SMBshare_label.Size = new System.Drawing.Size(209, 13);
            this.SMBshare_label.TabIndex = 4;
            this.SMBshare_label.Text = "The SMB share to connect (SMBshare) %s";
            //
            // SMBshare
            //
            this.SMBshare.Location = new System.Drawing.Point(6, 32);
            this.SMBshare.Name = "SMBshare";
            this.SMBshare.Size = new System.Drawing.Size(315, 20);
            this.SMBshare.TabIndex = 1;
            this.SMBshare.WordWrap = false;
            //
            // save
            //
            this.save.Location = new System.Drawing.Point(512, 383);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 1;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.Btn_save);
            //
            // close
            //
            this.close.Location = new System.Drawing.Point(593, 383);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 2;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.Btn_close);
            //
            // email
            //
            this.email.Location = new System.Drawing.Point(345, 261);
            this.email.Name = "email";
            this.email.Size = new System.Drawing.Size(311, 20);
            this.email.TabIndex = 28;
            this.email.WordWrap = false;
            this.email.ReadOnly = true;
            //
            // email_label
            //
            this.email_label.AutoSize = true;
            this.email_label.Location = new System.Drawing.Point(342, 244);
            this.email_label.Name = "email_label";
            this.email_label.Size = new System.Drawing.Size(151, 13);
            this.email_label.TabIndex = 29;
            this.email_label.Text = "space seperated emails (email)";
            //
            // smtp
            //
            this.smtp.Location = new System.Drawing.Point(345, 300);
            this.smtp.Name = "smtp";
            this.smtp.Size = new System.Drawing.Size(311, 20);
            this.smtp.TabIndex = 30;
            this.smtp.WordWrap = false;
            this.smtp.ReadOnly = true;
            //
            // smtp_label
            //
            this.smtp_label.AutoSize = true;
            this.smtp_label.Location = new System.Drawing.Point(342, 283);
            this.smtp_label.Name = "smtp_label";
            this.smtp_label.Size = new System.Drawing.Size(212, 13);
            this.smtp_label.TabIndex = 31;
            this.smtp_label.Text = "space seperated smtp FQDN servers (smtp)";
            //
            // help
            //
            this.help.Location = new System.Drawing.Point(431, 383);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(75, 23);
            this.help.TabIndex = 3;
            this.help.Text = "Help";
            this.help.UseVisualStyleBackColor = true;
            this.help.Click += new System.EventHandler(this.Btn_help);
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 412);
            this.Controls.Add(this.help);
            this.Controls.Add(this.close);
            this.Controls.Add(this.save);
            this.Controls.Add(this.Gateway_group);
            this.Name = "Configuration";
            this.Text = "pgSMB2 plugin Configuration";
            this.Load += new System.EventHandler(this.Form_Load);
            this.Gateway_group.ResumeLayout(false);
            this.Gateway_group.PerformLayout();
            this.User_group.ResumeLayout(false);
            this.User_group.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStore)).EndInit();
            this.Roaming_group.ResumeLayout(false);
            this.Roaming_group.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConnectRetry)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox Gateway_group;
        private System.Windows.Forms.GroupBox Roaming_group;
        private System.Windows.Forms.TextBox SMBshare;
        private System.Windows.Forms.GroupBox User_group;
        private System.Windows.Forms.Label ConnectRetry_label;
        private System.Windows.Forms.NumericUpDown ConnectRetry;
        private System.Windows.Forms.Label RoamingDest_label;
        private System.Windows.Forms.TextBox RoamingDest;
        private System.Windows.Forms.Label Filename_label;
        private System.Windows.Forms.TextBox Filename;
        private System.Windows.Forms.Label RoamingSource_label;
        private System.Windows.Forms.TextBox RoamingSource;
        private System.Windows.Forms.Label SMBshare_label;
        private System.Windows.Forms.Label MaxStore_label;
        private System.Windows.Forms.NumericUpDown MaxStore;
        private System.Windows.Forms.TextBox HomeDir;
        private System.Windows.Forms.Label ScriptPath_label;
        private System.Windows.Forms.Label HomeDir_label;
        private System.Windows.Forms.TextBox ScriptPath;
        private System.Windows.Forms.TextBox HomeDirDrive;
        private System.Windows.Forms.Label HomeDirDrive_label;
        private System.Windows.Forms.TextBox CompressCLI;
        private System.Windows.Forms.Label CompressCLI_label;
        private System.Windows.Forms.TextBox UncompressCLI;
        private System.Windows.Forms.Label UncompressCLI_label;
        private System.Windows.Forms.TextBox Compressor;
        private System.Windows.Forms.Label Compressor_label;
        private System.Windows.Forms.Label Description;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Label MaxStore_calc;
        private System.Windows.Forms.TextBox ntp;
        private System.Windows.Forms.Label ntp_label;
        private System.Windows.Forms.TextBox smtp;
        private System.Windows.Forms.Label smtp_label;
        private System.Windows.Forms.TextBox email;
        private System.Windows.Forms.Label email_label;
        private System.Windows.Forms.Button help;
    }
}