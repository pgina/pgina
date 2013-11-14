/*
	Copyright (c) 2012, pGina Team
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.pgSMB
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            SettingsToUi();
        }

        private void SettingsToUi()
        {
            string SMBshare_str = Settings.Store.SMBshare;
            string RoamingSource = Settings.Store.RoamingSource;
            string Filename = Settings.Store.Filename;
            string RoamingDest = Settings.Store.RoamingDest;
            int ConnectRetry = Settings.Store.ConnectRetry;
            string Compressor = Settings.Store.Compressor;
            string UncompressCLI = Settings.Store.UncompressCLI;
            string CompressCLI = Settings.Store.CompressCLI;
            string HomeDir = Settings.Store.HomeDir;
            string HomeDirDrive = Settings.Store.HomeDirDrive;
            string ScriptPath = Settings.Store.ScriptPath;
            int MaxStore = Settings.Store.MaxStore;
            string Ntp = Settings.Store.ntp;
            string Email = Settings.Store.email;
            string Smtp = Settings.Store.smtp;

            this.SMBshare.Text = SMBshare_str;
            this.RoamingSource.Text = RoamingSource;
            this.Filename.Text = Filename;
            this.RoamingDest.Text = RoamingDest;
            this.ConnectRetry.Value = (uint)ConnectRetry;
            this.Compressor.Text = Compressor;
            this.UncompressCLI.Text = UncompressCLI;
            this.CompressCLI.Text = CompressCLI;
            this.HomeDir.Text = HomeDir;
            this.HomeDirDrive.Text = HomeDirDrive;
            this.ScriptPath.Text = ScriptPath;
            this.MaxStore.Value = (uint)MaxStore;
            this.MaxStore_calc.Text = ((uint)MaxStore / 1024).ToString("F") + " MByte";
            this.ntp.Text = Ntp;
            this.email.Text = Email;
            this.smtp.Text = Smtp;

            //MessageBox.Show(Environment.CurrentDirectory.ToString());
            //MessageBox.Show(Environment.ExpandEnvironmentVariables(RoamingDest));
        }


        private void Form_Load(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 10000;
            toolTip1.InitialDelay = 0;
            toolTip1.ReshowDelay = 0;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(this.SMBshare, "The UNC name of the share to connect");
            toolTip1.SetToolTip(this.RoamingSource, "Where to store the compressed profile");
            toolTip1.SetToolTip(this.Filename, "The name of the compressed profile");
            toolTip1.SetToolTip(this.RoamingDest, "The local path for the extracted Roaming Profile");
            toolTip1.SetToolTip(this.ConnectRetry, "How often to retry to connect to the share\n(un)compress the profile");
            toolTip1.SetToolTip(this.Compressor, "The Program to (un)compress the user profile");
            toolTip1.SetToolTip(this.UncompressCLI, "The command to uncompress the profile");
            toolTip1.SetToolTip(this.CompressCLI, "The Commandline to compress the user profile");
            toolTip1.SetToolTip(this.HomeDir, "The user home directory");
            toolTip1.SetToolTip(this.HomeDirDrive, "The user home drive");
            toolTip1.SetToolTip(this.ScriptPath, "The full path to your login script (Runonce regkey!)");
            toolTip1.SetToolTip(this.MaxStore, "Maximum profile size in kbytes\n0 == all space");
            toolTip1.SetToolTip(this.ntp, "space seperated FQDN list of your ntp servers");
            toolTip1.SetToolTip(this.email, "space seperated FQDN list of email addresses");
            toolTip1.SetToolTip(this.smtp, "space seperated FQDN list of your smtp servers\nAuthenticated with the login credentials");
        }

        private void UiToSettings()
        {
            Settings.Store.SMBshare = this.SMBshare.Text.Trim();
            Settings.Store.RoamingSource = this.RoamingSource.Text.Trim();
            Settings.Store.Filename = this.Filename.Text.Trim();
            Settings.Store.RoamingDest = this.RoamingDest.Text.Trim();
            Settings.Store.ConnectRetry = Convert.ToInt32(this.ConnectRetry.Value);
            Settings.Store.Compressor = this.Compressor.Text.Trim();
            Settings.Store.UncompressCLI = this.UncompressCLI.Text.Trim();
            Settings.Store.CompressCLI = this.CompressCLI.Text.Trim();
            Settings.Store.MaxStore = Convert.ToInt32(this.MaxStore.Value);
            Settings.Store.HomeDir = this.HomeDir.Text.Trim();
            Settings.Store.HomeDirDrive = this.HomeDirDrive.Text.Trim();
            Settings.Store.ScriptPath = this.ScriptPath.Text.Trim();
            Settings.Store.ntp = this.ntp.Text.Trim();
            Settings.Store.email = this.email.Text.Trim();
            Settings.Store.smtp = this.smtp.Text.Trim();
        }

        private void MaxStore_MB(object sender, EventArgs e)
        {
            this.MaxStore_calc.Text = (this.MaxStore.Value/1024).ToString("F")+" MByte";
        }

        private void Btn_save(object sender, EventArgs e)
        {
            this.UiToSettings();
            this.Close();
        }

        private void Btn_close(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Btn_help(object sender, EventArgs e)
        {
            Form helper = new Form();
            ListBox help_text = new ListBox();

            helper.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            helper.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            helper.ClientSize = new System.Drawing.Size(700, 400);
            helper.Name = "Help";
            helper.Text = "pgSMB plugin Configuration Helper";

            help_text.Location = new System.Drawing.Point(1, 1);
            help_text.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            help_text.Size = new System.Drawing.Size(helper.Size.Width-15, helper.Size.Height-32);
            help_text.ScrollAlwaysVisible = true;

            string[] text = new string[] {
                "This plugin will work like the pgFTP plugin from pGina 1.x,",
                "except that it uses SMB instead of FTP to get and put the profile",
                "",
                "The purpose of pgFTP was to (visversa) compress the users profile and put it on an ftp server",
                "pgFTP made use of 7zip, i prefer MS own imagex available at",
                "http://support.microsoft.com/kb/2525084",
                "you are free to use whatever you want.",
                "",
                "This plugin does the same but make use of a roaming profile, but instead of storing the profile directly on the remote machine",
                "the plugin stores the profile local than compress and upload it.",
                "Why this complicated?",
                "\tDuring the upload its possible that the upload is aborted by the client itself",
                "\tpossible reasons are high server load, low network bandwith, unreliable network (WLan)",
                "\tThe client tries 3 times to upload the profile and there is no way to extend the rety count",
                "\tif the client was not able to upload a file from the profile the upload is aborted.",
                "\tThis is a bad situation when your users are using different machines every time they login",
                "",
                "The purpose of pgSMB is to create a user with the propriate settings to use a roaming profile",
                "extract the profile directly from the SMB on the local machine",
                "adept the ACL (still not domain ready) and leave the other stuff to pGina itself",
                "If the profile could not be downloaded, the user receives a tmp profile which is not uploaded during logoff.",
                "During the logoff/shutdown process the profile is locally compressed, uplaoded and market with an UTC timestamp from a timeserver",
                "If the user still has a local profile (BSOD) the plugin checks the time from the local and remote profile",
                "and only if the remote profile is newer than the local one the remote profile will be downloaded",
                "An existing user without the Comment \"pGina created pgSMB\" will not be touched!",
                "",
                "Some values can be reseted by using the Ldap plugin Attribute converter",
                "   The values are marked with \"Ldap replacing with <attribute name>\"",
                "",
                "Some settings are accessible by a macro",
                "   Macro name <variable name>",
                "",
                        "Roaming Profile:",
                        "   The SMB share to connect: (SMBshare)",
                        "      Here you need to set the UNC path of the users share",
                        "         Ldap replacing with pgSMB_SMBshare",
                        "         Macro name %s",
                        "   Where to store the compressed profile: (RoamingSource)",
                        "      The UNC path to the folder where the compressed file should be stored",
                        "         Ldap replacing with usri4_profile",
                        "         Macro name %r",
                        "   The name and extension of the profile: (Filename)",
                        "      The name and extension of the compressed profile",
                        "         Ldap replacing with pgSMB_Filename",
                        "         Macro name %f",
                        "   Where to extract the Profile: (RoamingDest)",
                        "      The local path where the compressed profile should be extracted",
                        "         Macro name %d",
                        "   Try n times to connect/extract/compress: (ConnectRetry)",
                        "      How often should the program try to do things, like compressing connect to the share ...",
                        "   The Programm to un-comress the Profile (Compressor)",
                        "      What programm should be called to un-compress the profile",
                        "   The command to uncompress the Profile: (UncompressCLI)",
                        "      The commandline to uncompress the compressed profile",
                        "      Extract the profile locally",
                        "   The command to compress the Profile: (CompressCLI)",
                        "      The commandline to compress the compressed profile",
                        "      Store the compressed profile local, it will be uploaded later",
                        "",
                        "User:",
                        "   The user HomeDir: (optional) (HomeDir)",
                        "      The path to the user home directory",
                        "         Ldap replacing with usri4_home_dir",
                        "   The user HomeDirDrive: (optional) (HomeDirDrive)",
                        "      The drive name of the home directory",
                        "         Ldap replacing with usri4_home_dir_drive",
                        "   Script Path: (optional) (ScriptPath)",
                        "      The full path to the login script",
                        "         Ldap replacing with LoginScript",
                        "      It will run in the user's security context",
                        "   The user max storage space in kbytes: (MaxStore)",
                        "      The max storage for users",
                        "         Ldap replacing with usri4_max_storage",
                        "      A user GPO setting to limit the profile size",
                        @"      HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System",
                        "      Can be overridden by the local GPO",
                        "      If the value is 0 the registry GPO settings are removed and all space is available",
                        "",
                        "space seperated ntp FQDN servers: (ntp)",
                        "   a space seperated list of ntp servers. FQDN required",
                        "space seperated emails: (optional) (email)",
                        "   a space seperated list of emails that should be notified if an error occure during the login or logoff process",
                        "space seperated smtp FQDN servers: (optional) (smtp)",
                        "   a space seperated list of smtp servers. FQDN required",
                        "   Authenticated with the username and user-password",
                        "",
                        "Macros:",
                        "%u will be replaced with the user login name",
                        "%f will be replaced with the setting Filename",
                        "%s will be replaced with the setting SMBshare",
                        "%r will be replaced with the setting RoamingSource",
                        "%d will be replaced with the setting RoamingDest",
                        "any other environment variable available for the user SYSTEM will be resolved too"
            };

            foreach (string element in text)
            {
                help_text.Items.Add(element);
            }


            helper.Controls.Add(help_text);
            helper.Show();
        }

    }
}
