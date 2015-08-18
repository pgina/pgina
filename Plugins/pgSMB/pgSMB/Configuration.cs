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
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation/plugins/pgsmb.html");
        }
    }
}
