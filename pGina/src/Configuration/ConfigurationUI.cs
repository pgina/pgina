/*
	Copyright (c) 2017, pGina Team
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
using System.IO;
using System.ServiceProcess;
using System.Reflection;
using Microsoft.Win32;

using pGina.Core;
using pGina.Core.Messages;

using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

using Abstractions.WindowsApi;
using Abstractions.Logging;
using Abstractions.Pipes;

using log4net;
using pGina.Shared.Types;

namespace pGina.Configuration
{
    public partial class ConfigurationUI : Form
    {
        private class DuplicatePluginDetectedException : System.Exception
        {
            public DuplicatePluginDetectedException(string Uuid, string Name) :
                base(string.Format("Duplicate plugin {0} detected with UUID: {1}", Name, Uuid))
            {
            }
        }

        private static readonly string PGINA_SERVICE_NAME = "pGina";

        // Plugin information keyed by Guid
        private Dictionary<string, IPluginBase> m_plugins = new Dictionary<string, IPluginBase>();
        private ILog m_logger = LogManager.GetLogger("ConfigurationUI");

        private ServiceController m_pGinaServiceController = null;
        private System.Timers.Timer m_serviceTimer = new System.Timers.Timer();

        // Plugin data grid view
        private const string PLUGIN_UUID_COLUMN = "Uuid";
        private const string PLUGIN_VERSION_COLUMN = "Version";
        private const string PLUGIN_DESC_COLUMN = "Description";
        private const string PLUGIN_NAME_COLUMN = "Name";
        private const string AUTHENTICATION_COLUMN = "Authentication";
        private const string AUTHORIZATION_COLUMN = "Authorization";
        private const string GATEWAY_COLUMN = "Gateway";
        private const string NOTIFICATION_COLUMN = "Notification";
        private const string PASSWORD_COLUMN = "Change Password";

        // Cred Prov Filter data grid view
        private const string CPF_CP_NAME_COLUMN = "Name";
        private const string CPF_CP_LOGON_COLUMN = "FilterLogon";
        private const string CPF_CP_UNLOCK_COLUMN = "FilterUnlock";
        private const string CPF_CP_CHANGE_PASS_COLUMN = "FilterChangePass";
        private const string CPF_CP_CREDUI_COLUMN = "FilterCredUI";
        private const string CPF_CP_UUID_COLUMN = "Uuid";

        private LogViewWindow logWindow = null;

        public ConfigurationUI()
        {
            VerifyRegistryAccess();
            Framework.Init();

            InitializeComponent();
            InitOptionsTabs();
            InitPluginsDGV();
            PopulatePluginDirs();
            InitOrderLists();

            try
            {
                RefreshPluginLists();
            }
            catch (DuplicatePluginDetectedException e)
            {
                MessageBox.Show(string.Format("Unable to load full plugin list: {0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            InitSimulationTab();
            LoadGeneralSettings();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 10000;
            toolTip1.InitialDelay = 0;
            toolTip1.ReshowDelay = 0;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(this.notify_smtp, "space seperated FQDN list of your smtp servers\nsmtp.domain.local:25");
            toolTip1.SetToolTip(this.notify_email, "space seperated FQDN list of email addresses\nadmin@domain.local");
            toolTip1.SetToolTip(this.notify_user, "smtp username");
            toolTip1.SetToolTip(this.notify_pass, "smtp password");
            toolTip1.SetToolTip(this.notify_cred, "prefer Login credentials instead of smtp username and password");
            toolTip1.SetToolTip(this.notify_ssl, "use encrypted smtp connection");
            toolTip1.SetToolTip(this.ntpservers, "list of NTP servers");
        }

        private void VerifyRegistryAccess()
        {
            // Test write access
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(
                    pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot))
                {
                    string name = "___test_name___";
                    key.SetValue(name, "...");
                    string value = (string)key.GetValue(name);
                    key.DeleteValue(name);
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to access registry, good bye.",
                    "Registry access error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void InitOptionsTabs()
        {
            if (Abstractions.Windows.OsInfo.IsVistaOrLater())
            {
                m_tabs.TabPages.Remove(ginaOptions);
                InitCpOptions();
            }
            else
            {
                m_tabs.TabPages.Remove(cpOptions);
                InitGinaOptions();
            }
        }

        private void InitCpOptions()
        {
            dgvCredProvFilter.RowHeadersVisible = false;
            dgvCredProvFilter.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCredProvFilter.MultiSelect = false;
            dgvCredProvFilter.AllowUserToAddRows = false;

            dgvCredProvFilter.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = CPF_CP_NAME_COLUMN,
                DataPropertyName = CPF_CP_NAME_COLUMN,
                HeaderText = "Credential Provider",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            dgvCredProvFilter.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = CPF_CP_LOGON_COLUMN,
                DataPropertyName = CPF_CP_LOGON_COLUMN,
                HeaderText = "Logon",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvCredProvFilter.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = CPF_CP_UNLOCK_COLUMN,
                DataPropertyName = CPF_CP_UNLOCK_COLUMN,
                HeaderText = "Unlock",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvCredProvFilter.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = CPF_CP_CHANGE_PASS_COLUMN,
                DataPropertyName = CPF_CP_CHANGE_PASS_COLUMN,
                HeaderText = "Change Password",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvCredProvFilter.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = CPF_CP_CREDUI_COLUMN,
                DataPropertyName = CPF_CP_CREDUI_COLUMN,
                HeaderText = "Cred UI",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvCredProvFilter.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = CPF_CP_UUID_COLUMN,
                DataPropertyName = CPF_CP_UUID_COLUMN,
                HeaderText = "UUID",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvCredProvFilter.AutoGenerateColumns = false;
            dgvCredProvFilter.DataSource = CredProvFilterConfig.LoadCredProvsAndFilterSettings();
        }

        private void InitGinaOptions()
        {
            m_txtGinaChain.Text = Settings.Get.ChainedGinaPath;
            chkSpecialButton.Checked = Settings.Get.EnableSpecialActionButton;
            string action = Settings.Get.SpecialAction;
            switch (action)
            {
                case "Shutdown":
                    radioShutdown.Checked = true;
                    break;
                case "Restart":
                    radioRestart.Checked = true;
                    break;
                case "Sleep":
                    radioSleep.Checked = true;
                    break;
                case "Hibernate":
                    radioHibernate.Checked = true;
                    break;
            }

            radioSleep.Enabled = chkSpecialButton.Checked;
            radioShutdown.Enabled = chkSpecialButton.Checked;
            radioRestart.Enabled = chkSpecialButton.Checked;
            radioHibernate.Enabled = chkSpecialButton.Checked;
        }

        private void LoadGeneralSettings()
        {
            m_pginaVersionLbl.Text = "pGina " +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            m_tileImageTxt.Text = Settings.Get.GetSetting("TileImage", null);
            LoadTileImagePreview();

            // Load MOTD settings
            this.enableMotdCB.Checked = Settings.Get.EnableMotd;
            this.motdTB.Text = Settings.Get.GetSetting("Motd");
            this.motdTB.Enabled = this.enableMotdCB.Checked;

            // Service status checkbox
            this.logonUiShowServiceStatusCB.Checked = Settings.Get.ShowServiceStatusInLogonUi;

            // Make sure that the pGina service is installed
            foreach( ServiceController ctrl in ServiceController.GetServices() )
            {
                if (ctrl.ServiceName == PGINA_SERVICE_NAME)
                {
                    m_pGinaServiceController = ctrl;
                    break;
                }
            }

            // This works around an annoying aspect of the textbox where the foreground color
            // can't be changed unless the background color is set.
            this.serviceStatusTB.BackColor = Color.GhostWhite;
            this.cpEnabledTB.BackColor = Color.GhostWhite;
            this.cpRegisteredTB.BackColor = Color.GhostWhite;

            if (m_pGinaServiceController != null)
            {
                // Setup the timer that checks the service status periodically
                m_serviceTimer.Interval = 1500;
                m_serviceTimer.SynchronizingObject = this.serviceStatusTB;
                m_serviceTimer.AutoReset = true;
                m_serviceTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_serviceTimer_Elapsed);
                m_serviceTimer.Start();
                UpdateServiceStatus();
            }
            else
            {
                this.serviceStatusTB.Text = "Not Installed";
                this.serviceStopBtn.Enabled = false;
                this.serviceStartBtn.Enabled = false;
            }

            UpdateCpStatus();

            // Load the unlock setting
            chk_originalUsernameUnlock.Checked = Settings.Get.UseOriginalUsernameInUnlockScenario;

            // Display last username in logon screen
            chk_lastusername.Checked = Settings.Get.LastUsernameEnable;
            chk_preferlocalauthentication.Checked = Settings.Get.PreferLocalAuthentication;

            //ntp server
            //this.ntpservers = Settings.Get.GetGetSetting("ntpservers");
            string[] ntpserverList = Settings.Get.ntpservers;
            this.ntpservers.Text = String.Join("\n", ntpserverList);

            // email notification
            this.notify_smtp.Text = Settings.Get.GetSetting("notify_smtp");
            this.notify_email.Text = Settings.Get.GetSetting("notify_email");
            this.notify_user.Text = Settings.Get.GetSetting("notify_user");
            this.notify_pass.Text = Settings.Get.GetEncryptedSetting("notify_pass");
            this.notify_cred.Checked = Settings.Get.notify_cred;
            this.notify_ssl.Checked = Settings.Get.notify_ssl;
        }

        private void UpdateCpStatus()
        {
            pGina.CredentialProvider.Registration.CredProviderManager manager =
                pGina.CredentialProvider.Registration.CredProviderManager.GetManager();

            if (manager.Registered())
            {
                this.cpRegisteredTB.Text = "Yes";
                this.cpRegisteredTB.ForeColor = Color.Green;
                this.cpRegisterBtn.Text = "Unregister";
                this.cpEnableDisableBtn.Enabled = true;

                if (manager.Enabled())
                {
                    this.cpEnabledTB.Text = "Yes";
                    this.cpEnabledTB.ForeColor = Color.Green;
                    this.cpEnableDisableBtn.Text = "Disable";
                }
                else
                {
                    this.cpEnabledTB.Text = "No";
                    this.cpEnabledTB.ForeColor = Color.Red;
                    this.cpEnableDisableBtn.Text = "Enable";
                }

                if (Abstractions.Windows.OsInfo.Is64Bit() && Abstractions.Windows.OsInfo.IsVistaOrLater() && !manager.Registered6432())
                {
                    MessageBox.Show("Warning: The 32-bit CredentialProvider is not registered.  32-bit apps that " +
                        "make use of the CredentialProvider may not function correctly.");
                }
            }
            else
            {
                this.cpRegisteredTB.Text = "No";
                this.cpRegisteredTB.ForeColor = Color.Red;
                this.cpRegisterBtn.Text = "Register";
                this.cpEnabledTB.Text = "No";
                this.cpEnabledTB.ForeColor = Color.Red;
                this.cpEnableDisableBtn.Enabled = false;
                this.cpEnableDisableBtn.Text = "Enable";
            }
        }

        private void UpdateServiceStatus()
        {
            m_pGinaServiceController.Refresh();
            ServiceControllerStatus stat = m_pGinaServiceController.Status;
            string statusStr = stat.ToString();
            switch (stat)
            {
                case ServiceControllerStatus.Running:
                    this.serviceStatusTB.ForeColor = Color.Green;
                    this.serviceStopBtn.Enabled = true;
                    this.serviceStartBtn.Enabled = false;
                    statusStr = "Running";
                    break;
                case ServiceControllerStatus.ContinuePending:
                    statusStr = "Continue pending...";
                    this.serviceStatusTB.ForeColor = Color.Black;
                    this.serviceStopBtn.Enabled = false;
                    this.serviceStartBtn.Enabled = false;
                    break;
                case ServiceControllerStatus.PausePending:
                    statusStr = "Pause pending...";
                    this.serviceStatusTB.ForeColor = Color.Black;
                    this.serviceStopBtn.Enabled = false;
                    this.serviceStartBtn.Enabled = false;
                    break;
                case ServiceControllerStatus.StartPending:
                    statusStr = "Starting...";
                    this.serviceStatusTB.ForeColor = Color.Black;
                    this.serviceStopBtn.Enabled = false;
                    this.serviceStartBtn.Enabled = false;
                    break;
                case ServiceControllerStatus.StopPending:
                    statusStr = "Stopping...";
                    this.serviceStatusTB.ForeColor = Color.Black;
                    this.serviceStopBtn.Enabled = false;
                    this.serviceStartBtn.Enabled = false;
                    break;
                case ServiceControllerStatus.Paused:
                    this.serviceStatusTB.ForeColor = Color.Black;
                    this.serviceStopBtn.Enabled = true;
                    this.serviceStartBtn.Enabled = true;
                    break;
                case ServiceControllerStatus.Stopped:
                    statusStr = "Stopped";
                    this.serviceStatusTB.ForeColor = Color.Red;
                    this.serviceStopBtn.Enabled = false;
                    this.serviceStartBtn.Enabled = true;
                    break;
            }
            this.serviceStatusTB.Text = statusStr;
        }

        private void LoadTileImagePreview()
        {
            if (!string.IsNullOrEmpty(m_tileImageTxt.Text))
            {
                try
                {
                    m_tileImagePreview.Image = new Bitmap(m_tileImageTxt.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to load a preview image for the selected file, pGina may not be able to show this image at login time.\n", "Error", MessageBoxButtons.OK);
                    m_logger.ErrorFormat("User chose {0} as image file, but we failed to load it? Exception: {1}", m_tileImageTxt.Text, ex);
                    m_tileImagePreview.Image = null;
                }
            }
        }

        private void InitSimulationTab()
        {
            // Set up columns in simPluginResultsListView
            this.simPluginResultsListView.Columns.Add("Stage", -2, HorizontalAlignment.Left);
            this.simPluginResultsListView.Columns.Add("Plugin", -2, HorizontalAlignment.Left);
            this.simPluginResultsListView.Columns.Add("Result", -2, HorizontalAlignment.Left);
            this.simPluginResultsListView.Columns.Add("Message", -2, HorizontalAlignment.Left);
            this.simPluginResultsListView.LabelEdit = false;
            this.simPluginResultsListView.GridLines = true;
            this.simPluginResultsListView.View = View.Details;

            this.logWindow = new LogViewWindow();
            this.logWindow.FormClosing += new FormClosingEventHandler(logWindow_FormClosing);
            ResetStageStatus();
        }

        void logWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.logWindow.Hide();
            e.Cancel = true;  // Don't dispose the window.  We might show it again later.
        }

        private void InitOrderLists()
        {
            // Setup the DataGridViews
            InitPluginOrderDGV(this.authenticateDGV);
            InitPluginOrderDGV(this.authorizeDGV);
            InitPluginOrderDGV(this.gatewayDGV);
            InitPluginOrderDGV(this.eventDGV);
            InitPluginOrderDGV(this.passwdDGV);

            // Load order lists from the registry
            LoadPluginOrderListsFromReg();
        }

        private void InitPluginOrderDGV(DataGridView dgv)
        {
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_UUID_COLUMN,
                Visible = false
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_NAME_COLUMN,
                HeaderText = "Plugin",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
        }

        private void InitPluginsDGV()
        {
            pluginsDG.RowHeadersVisible = false;
            pluginsDG.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            pluginsDG.MultiSelect = false;
            pluginsDG.AllowUserToAddRows = false;

            pluginsDG.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_NAME_COLUMN,
                HeaderText = "Plugin Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = AUTHENTICATION_COLUMN,
                HeaderText = "Authentication",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = AUTHORIZATION_COLUMN,
                HeaderText = "Authorization",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = GATEWAY_COLUMN,
                HeaderText = "Gateway",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = NOTIFICATION_COLUMN,
                HeaderText = "Notification",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = PASSWORD_COLUMN,
                HeaderText = "Change Password",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_DESC_COLUMN,
                HeaderText = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });

            pluginsDG.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_VERSION_COLUMN,
                HeaderText = "Version",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });

            pluginsDG.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_UUID_COLUMN,
                HeaderText = "UUID",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });

            // Implement the cell paint event so that we can blank out cells
            // that shouldn't be there.
            pluginsDG.CellPainting += this.pluginsDG_PaintCell;

            pluginsDG.SelectionChanged += this.pluginsDG_SelectionChanged;
            pluginsDG.CurrentCellDirtyStateChanged += new EventHandler(pluginsDG_CurrentCellDirtyStateChanged);
            pluginsDG.CellValueChanged += new DataGridViewCellEventHandler(pluginsDG_CellValueChanged);
        }

        void pluginsDG_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 &&
                pluginsDG[e.ColumnIndex, e.RowIndex].ValueType == typeof(bool))
            {
                DataGridViewCell cell = this.pluginsDG[e.ColumnIndex, e.RowIndex];
                string uuid = (string)this.pluginsDG.Rows[e.RowIndex].Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = m_plugins[uuid];
                bool checkBoxState = Convert.ToBoolean(cell.Value);
                string columnName = pluginsDG.Columns[e.ColumnIndex].Name;

                switch (columnName)
                {
                    case AUTHENTICATION_COLUMN:
                        SyncStateToList(checkBoxState, plug, authenticateDGV);
                        break;
                    case AUTHORIZATION_COLUMN:
                        SyncStateToList(checkBoxState, plug, authorizeDGV);
                        break;
                    case GATEWAY_COLUMN:
                        SyncStateToList(checkBoxState, plug, gatewayDGV);
                        break;
                    case NOTIFICATION_COLUMN:
                        SyncStateToList(checkBoxState, plug, eventDGV);
                        break;
                    case PASSWORD_COLUMN:
                        SyncStateToList(checkBoxState, plug, passwdDGV);
                        break;
                }
            }
        }

        private void SyncStateToList(bool state, IPluginBase plugin, DataGridView grid)
        {
            if (state)
            {
                this.AddToOrderList(plugin, grid);
            }
            else
            {
                this.RemoveFromOrderList(plugin.Uuid.ToString(), grid);
            }
        }

        void pluginsDG_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.pluginsDG.IsCurrentCellDirty)
            {
                this.pluginsDG.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void pluginsDG_SelectionChanged(object sender, EventArgs e)
        {
            int nSelectedRows = pluginsDG.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = pluginsDG.SelectedRows[0];
                string pluginUuid = (string)row.Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginUuid];

                configureButton.Enabled = plug is IPluginConfiguration;
            }
        }

        private void RefreshPluginLists()
        {
            m_plugins.Clear();
            pluginsDG.Rows.Clear();

            // Get the plugin directories from the list
            List<string> pluginDirs = new List<string>();
            foreach (ListViewItem item in lstPluginDirs.Items)
            {
                if (!pluginDirs.Contains((string)item.Tag))
                    pluginDirs.Add((string)item.Tag);
            }

            if (pluginDirs.Count > 0)
            {
                // Get plugins
                PluginLoader.PluginDirectories = pluginDirs.ToArray();
                PluginLoader.LoadPlugins();
                List<IPluginBase> plugins = PluginLoader.AllPlugins;

                for (int i = 0; i < plugins.Count; i++)
                {
                    IPluginBase p = plugins[i];

                    if (m_plugins.ContainsKey(p.Uuid.ToString()))
                    {
                        m_logger.ErrorFormat("Duplicate plugin: {0} with UUID: {1}", p.Name, p.Uuid);
                        throw new DuplicatePluginDetectedException(p.Uuid.ToString(), p.Name);
                    }

                    this.m_plugins.Add(p.Uuid.ToString(), p);
                    pluginsDG.Rows.Add(
                        new object[] { p.Name, false, false, false, false, false, p.Description, p.Version, p.Uuid.ToString() });
                    DataGridViewRow row = pluginsDG.Rows[i];

                    this.SetupCheckBoxCell<IPluginAuthentication>(row.Cells[AUTHENTICATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthorization>(row.Cells[AUTHORIZATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthenticationGateway>(row.Cells[GATEWAY_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginEventNotifications>(row.Cells[NOTIFICATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginChangePassword>(row.Cells[PASSWORD_COLUMN], p);
                }
            }

            UpdatePluginOrderListsFromUIState();
        }

        private void SetupCheckBoxCell<T>(DataGridViewCell cell, IPluginBase plug) where T : IPluginBase
        {
            if (plug is T)
            {
                cell.Value = PluginLoader.IsEnabledFor<T>(plug);
            }
            else
            {
                // If a cell is read-only, the paint callback will draw over the
                // checkbox so that it is not visible.
                cell.ReadOnly = true;
            }
        }

        private void LoadPluginOrderListsFromReg()
        {
            LoadPluginOrderListFromReg<IPluginAuthentication>(authenticateDGV);
            LoadPluginOrderListFromReg<IPluginAuthenticationGateway>(gatewayDGV);
            LoadPluginOrderListFromReg<IPluginAuthorization>(authorizeDGV);
            LoadPluginOrderListFromReg<IPluginEventNotifications>(eventDGV);
            LoadPluginOrderListFromReg<IPluginChangePassword>(passwdDGV);
        }

        private void LoadPluginOrderListFromReg<T>(DataGridView grid) where T : class, IPluginBase
        {
            grid.Rows.Clear();
            List<T> plugins = PluginLoader.GetOrderedPluginsOfType<T>();
            foreach (IPluginBase plug in plugins)
            {
                AddToOrderList(plug, grid);
            }
        }

        private void UpdatePluginOrderListsFromUIState()
        {
            // Make sure that all checkboxes in the main plugin list agree with the ordered
            // lists.
            foreach (DataGridViewRow row in this.pluginsDG.Rows)
            {
                string uuid = (string)row.Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = m_plugins[uuid];

                if (!row.Cells[AUTHENTICATION_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[AUTHENTICATION_COLUMN].Value, plug, authenticateDGV);

                if (!row.Cells[AUTHORIZATION_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[AUTHORIZATION_COLUMN].Value, plug, authorizeDGV);

                if (!row.Cells[GATEWAY_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[GATEWAY_COLUMN].Value, plug, gatewayDGV);

                if (!row.Cells[NOTIFICATION_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[NOTIFICATION_COLUMN].Value, plug, eventDGV);

                if (!row.Cells[PASSWORD_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[PASSWORD_COLUMN].Value, plug, passwdDGV);
            }

            // Remove any plugins that are no longer in the main list from the
            // ordered lists
            this.RemoveAllNotInMainList(authorizeDGV);
            this.RemoveAllNotInMainList(authenticateDGV);
            this.RemoveAllNotInMainList(gatewayDGV);
            this.RemoveAllNotInMainList(eventDGV);
            this.RemoveAllNotInMainList(passwdDGV);
        }

        private void RemoveAllNotInMainList(DataGridView dgv)
        {
            List<DataGridViewRow> toRemove = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!m_plugins.ContainsKey((string)row.Cells[PLUGIN_UUID_COLUMN].Value))
                {
                    toRemove.Add(row);
                }
            }
            foreach (DataGridViewRow row in toRemove)
                dgv.Rows.Remove(row);
        }

        private void RemoveFromOrderList(string uuid, DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if ((string)row.Cells[PLUGIN_UUID_COLUMN].Value == uuid)
                {
                    dgv.Rows.Remove(row);
                    return;
                }
            }
        }

        /// <summary>
        /// Adds the plugin to the ordered list in the second parameter.  If the plugin
        /// is already in the list, does nothing.
        /// </summary>
        /// <param name="plug">Plugin to add</param>
        /// <param name="dgv">The ordered list to add to.</param>
        private void AddToOrderList(IPluginBase plug, DataGridView dgv)
        {
            if (!viewContainsPlugin(dgv, plug.Uuid.ToString()))
            {
                dgv.Rows.Add(new object[] { plug.Uuid.ToString(), plug.Name });
            }
        }

        private bool viewContainsPlugin(DataGridView dgv, string plugUuid)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if ((string)row.Cells[PLUGIN_UUID_COLUMN].Value == plugUuid)
                    return true;
            }
            return false;
        }

        private void pluginsDG_PaintCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Determine if the cell should have a checkbox or not (via the ReadOnly setting),
            // if not, we draw over the checkbox.
            if (e != null && sender != null)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                    pluginsDG[e.ColumnIndex, e.RowIndex].ReadOnly &&
                    pluginsDG[e.ColumnIndex, e.RowIndex].ValueType == typeof(bool))
                {
                    Type foo = pluginsDG[e.ColumnIndex, e.RowIndex].ValueType;
                    e.PaintBackground(e.CellBounds, true);
                    e.Handled = true;
                }
            }
        }

        private void PopulatePluginDirs()
        {
            // Populate plugin directories UI
            string[] pluginDirectories = Settings.Get.PluginDirectories;
            lstPluginDirs.Columns.Clear();
            lstPluginDirs.Columns.Add("Directory");
            lstPluginDirs.Columns[0].Width = lstPluginDirs.Width - 5;
            lstPluginDirs.Items.Clear();

            foreach (string dir in pluginDirectories)
            {
                ListViewItem item = new ListViewItem(new string[] { dir });
                item.Tag = dir;
                lstPluginDirs.Items.Add(item);
            }
        }

        private void SavePluginDirs()
        {
            // Save changes to plugin directories
            List<string> pluginDirs = new List<string>();
            foreach (ListViewItem item in lstPluginDirs.Items)
            {
                if (!pluginDirs.Contains((string)item.Tag))
                    pluginDirs.Add((string)item.Tag);
            }
            Settings.Get.PluginDirectories = pluginDirs.ToArray();
        }

        private void SavePluginSettings()
        {
            foreach (DataGridViewRow row in pluginsDG.Rows)
            {
                try
                {
                    IPluginBase p = m_plugins[(string)row.Cells[PLUGIN_UUID_COLUMN].Value];
                    int mask = 0;

                    if (Convert.ToBoolean(row.Cells[AUTHENTICATION_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.AuthenticateEnabled;
                    if (Convert.ToBoolean(row.Cells[AUTHORIZATION_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.AuthorizeEnabled;
                    if (Convert.ToBoolean(row.Cells[GATEWAY_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.GatewayEnabled;
                    if (Convert.ToBoolean(row.Cells[NOTIFICATION_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.NotificationEnabled;
                    if (Convert.ToBoolean(row.Cells[PASSWORD_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.ChangePasswordEnabled;

                    Core.Settings.Get.SetSetting(p.Uuid.ToString(), mask);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception when saving data: " + e);
                }
            }
        }

        private void SavePluginOrder()
        {
            SavePluginOrder(authenticateDGV, typeof(IPluginAuthentication));
            SavePluginOrder(authorizeDGV, typeof(IPluginAuthorization));
            SavePluginOrder(gatewayDGV, typeof(IPluginAuthenticationGateway));
            SavePluginOrder(eventDGV, typeof(IPluginEventNotifications));
            SavePluginOrder(passwdDGV, typeof(IPluginChangePassword));
        }

        private void SavePluginOrder(DataGridView grid, Type pluginType)
        {
            string setting = pluginType.Name + "_Order";
            List<string> orderedList = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                orderedList.Add((string)row.Cells[PLUGIN_UUID_COLUMN].Value);
            }
            Settings.Get.SetSetting(setting, orderedList.ToArray<string>());
        }

        private bool CheckPluginSettings()
        {
            bool AUTHENTICATION = false;
            //bool AUTHORIZATION = false;

            foreach (DataGridViewRow row in pluginsDG.Rows)
            {
                if (Convert.ToBoolean(row.Cells[AUTHENTICATION_COLUMN].Value))
                    AUTHENTICATION = true;/*
                if (Convert.ToBoolean(row.Cells[AUTHORIZATION_COLUMN].Value))
                    AUTHORIZATION = true;*/

                if (AUTHENTICATION /*&& AUTHORIZATION*/)
                    return true;
            }

            if (!AUTHENTICATION)
            {
                MessageBox.Show(this, "At least one plugin must be set for Authentication", "Can't save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }/*
            if (!AUTHORIZATION)
            {
                MessageBox.Show(this, "At least one plugin must be set for Authorization", "Can't save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/

            return false;
        }

        private bool SaveSettings()
        {
            if (!this.CheckPluginSettings())
                return false;
            this.SavePluginSettings();
            this.SavePluginDirs();
            this.SavePluginOrder();

            Core.Settings.Get.TileImage = m_tileImageTxt.Text;
            this.LoadTileImagePreview();

            // MOTD stuff
            Settings.Get.EnableMotd = this.enableMotdCB.Checked;
            Settings.Get.Motd = this.motdTB.Text.Trim();

            // Service status checkbox
            Settings.Get.ShowServiceStatusInLogonUi = this.logonUiShowServiceStatusCB.Checked;

            // Save unlock setting
            Settings.Get.UseOriginalUsernameInUnlockScenario = chk_originalUsernameUnlock.Checked;

            // Display last username in logon screen
            Settings.Get.LastUsernameEnable = chk_lastusername.Checked;
            Settings.Get.PreferLocalAuthentication = chk_preferlocalauthentication.Checked;

            if (Abstractions.Windows.OsInfo.IsVistaOrLater())
                this.SaveCpSettings();
            else
                this.SaveGinaSettings();

            // ntp server
            Settings.Get.ntpservers = this.ntpservers.Text.Split('\n');

            // email notification
            Settings.Get.notify_smtp = this.notify_smtp.Text;
            Settings.Get.notify_email = this.notify_email.Text;
            Settings.Get.notify_user = this.notify_user.Text;
            Settings.Get.SetEncryptedSetting("notify_pass", this.notify_pass.Text);
            Settings.Get.notify_cred = this.notify_cred.Checked;
            Settings.Get.notify_ssl = this.notify_ssl.Checked;

            return true;
        }

        private void MoveUp(DataGridView dgv, int index)
        {
            if (index > 0)
            {
                DataGridViewRow row = dgv.Rows[index];
                dgv.Rows.RemoveAt(index);
                dgv.Rows.Insert(index - 1, row);
                dgv.Rows[index - 1].Selected = true;
            }
        }

        private void MoveDown(DataGridView dgv, int index)
        {
            int rows = dgv.Rows.Count;
            if (index < rows - 1)
            {
                DataGridViewRow row = dgv.Rows[index];
                dgv.Rows.RemoveAt(index);
                dgv.Rows.Insert(index + 1, row);
                dgv.Rows[index + 1].Selected = true;
            }
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
                MessageBox.Show("Settings written to registry.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
                this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "Plugin Directory Selection...";
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = folder.SelectedPath;
                if (lstPluginDirs.Items.Find(path, true).Length == 0)
                {
                    ListViewItem item = new ListViewItem(new string[] { path });
                    item.Tag = path;
                    lstPluginDirs.Items.Add(item);
                }

                try
                {
                    this.RefreshPluginLists();
                }
                catch (DuplicatePluginDetectedException ex)
                {
                    MessageBox.Show(string.Format("Unable to load full plugin list: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstPluginDirs.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lstPluginDirs.SelectedItems)
                {
                    lstPluginDirs.Items.Remove(item);
                }

                try
                {
                    this.RefreshPluginLists();
                }
                catch (DuplicatePluginDetectedException ex)
                {
                    MessageBox.Show(string.Format("Unable to load full plugin list: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void configureButton_Click(object sender, EventArgs e)
        {
            int nSelectedRows = pluginsDG.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = pluginsDG.SelectedRows[0];
                string pluginUuid = (string)row.Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginUuid];

                if (plug is IPluginConfiguration)
                {
                    IPluginConfiguration configPlugin = plug as IPluginConfiguration;
                    configPlugin.Configure();
                }
            }
        }

        private void simMethodChanged(object sender, EventArgs e)
        {
            btnLaunchCredUI.Enabled = (sender == m_radioCredUI);
        }

        private void authenticateBtnUp_Click(object sender, EventArgs e)
        {
            if (this.authenticateDGV.SelectedRows.Count > 0)
                MoveUp(this.authenticateDGV, this.authenticateDGV.SelectedRows[0].Index);
        }

        private void authenticateBtnDown_Click(object sender, EventArgs e)
        {
            if (this.authenticateDGV.SelectedRows.Count > 0)
                MoveDown(this.authenticateDGV, this.authenticateDGV.SelectedRows[0].Index);
        }

        private void authorizeBtnUp_Click(object sender, EventArgs e)
        {
            if (this.authorizeDGV.SelectedRows.Count > 0)
                MoveUp(this.authorizeDGV, this.authorizeDGV.SelectedRows[0].Index);
        }

        private void authorizeBtnDown_Click(object sender, EventArgs e)
        {
            if (this.authorizeDGV.SelectedRows.Count > 0)
                MoveDown(this.authorizeDGV, this.authorizeDGV.SelectedRows[0].Index);
        }

        private void gatewayBtnUp_Click(object sender, EventArgs e)
        {
            if (this.gatewayDGV.SelectedRows.Count > 0)
                MoveUp(this.gatewayDGV, this.gatewayDGV.SelectedRows[0].Index);
        }

        private void gatewayBtnDown_Click(object sender, EventArgs e)
        {
            if (this.gatewayDGV.SelectedRows.Count > 0)
                MoveDown(this.gatewayDGV, this.gatewayDGV.SelectedRows[0].Index);
        }

        private void eventBtnUp_Click(object sender, EventArgs e)
        {
            if (this.eventDGV.SelectedRows.Count > 0)
                MoveUp(this.eventDGV, this.eventDGV.SelectedRows[0].Index);
        }

        private void eventBtnDown_Click(object sender, EventArgs e)
        {
            if (this.eventDGV.SelectedRows.Count > 0)
                MoveDown(this.eventDGV, this.eventDGV.SelectedRows[0].Index);
        }

        private void passwdBtnUp_Click(object sender, EventArgs e)
        {
            if (this.passwdDGV.SelectedRows.Count > 0)
                MoveUp(this.passwdDGV, this.passwdDGV.SelectedRows[0].Index);
        }

        private void passwdBtnDown_Click(object sender, EventArgs e)
        {
            if (this.passwdDGV.SelectedRows.Count > 0)
                MoveDown(this.passwdDGV, this.passwdDGV.SelectedRows[0].Index);
        }

        private void btnLaunchCredUI_Click(object sender, EventArgs e)
        {
            ResetSimUI();
            System.Net.NetworkCredential credential = pInvokes.GetCredentials("Simulated Login", "Please enter your credentials...");
            if (credential != null)
            {
                ResetStageStatus();

                m_usernameResult.Text = credential.UserName;
                m_domainResult.Text = credential.Domain;
                m_passwordResult.Text = credential.Password;
            }
        }

        private void HandleLabelTextChange(Label lbl)
        {
            if (lbl.Text == "Success")
                lbl.ForeColor = Color.Green;
            else if (lbl.Text == "Failure")
                lbl.ForeColor = Color.Red;
            else
                lbl.ForeColor = Color.Black;
        }

        private void m_lblAuthResult_TextChanged(object sender, EventArgs e)
        {
            HandleLabelTextChange((Label)sender);
        }

        private void SimLogHandler(string message)
        {
            this.logWindow.LogTextBox.AppendText(message);
        }

        private List<string> GetPluginList()
        {
            List<string> result = new List<string>();

            List<IPluginAuthentication> authPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();
            List<IPluginAuthorization> authzPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();
            List<IPluginAuthenticationGateway> gatewayPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();
            List<IPluginEventNotifications> notePlugins = PluginLoader.GetOrderedPluginsOfType<IPluginEventNotifications>();
            List<IPluginChangePassword> passwdPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginChangePassword>();

            result.Add("Authentication: " + (string.Join(", ", authPlugins.Select(p => p.Name))));
            result.Add("Authorization: " + (string.Join(", ", authzPlugins.Select(p => p.Name))));
            result.Add("Gateway: " + (string.Join(", ", gatewayPlugins.Select(p => p.Name))));
            result.Add("Notification: " + (string.Join(", ", notePlugins.Select(p => p.Name))));
            result.Add("Change Password: " + (string.Join(", ", passwdPlugins.Select(p => p.Name))));

            return result;
        }

        private void ResetSimUI()
        {
            this.simFinalResultMessageTB.Text = null;
            this.simPluginResultsListView.Items.Clear();
            ResetStageStatus();
            m_usernameResult.Text = null;
            m_domainResult.Text = null;
            m_passwordResult.Text = null;
            this.logWindow.LogTextBox.Text = "";
            if (m_radioUseService.Checked || m_radioCredUI.Checked)
            {
                this.logWindow.LogTextBox.AppendText("*****" + Environment.NewLine);
                this.logWindow.LogTextBox.AppendText("***** Log output unavailable when using pGina service or CredUI prompt." +
                    Environment.NewLine);
                this.logWindow.LogTextBox.AppendText("*****" + Environment.NewLine);
            }
            else
            {
                this.logWindow.LogTextBox.AppendText("****" + Environment.NewLine);
                this.logWindow.LogTextBox.AppendText("**** Simulated login starting: " + DateTime.Now.ToString("F") + Environment.NewLine);
                this.logWindow.LogTextBox.AppendText("**** pGina Version:  " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + Environment.NewLine);
                this.logWindow.LogTextBox.AppendText("**** Enabled plugins: " + Environment.NewLine);
                foreach (string s in GetPluginList())
                {
                    this.logWindow.LogTextBox.AppendText("****     " + s + Environment.NewLine);
                }
                this.logWindow.LogTextBox.AppendText("****" + Environment.NewLine);
            }
        }

        private void btnSimGo_Click(object sender, EventArgs e)
        {
            if (m_radioUseService.Checked)
            {
                if (MessageBox.Show("Individual plugin results and results for each stage are unavailable when using the pGina service.  Continue?",
                                "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                    return;

                ResetSimUI();
                DoServiceClientSimulation();
            }
            else
            {
                if (MessageBox.Show("Continuing will save all pending changes in configuration, do you want to continue?",
                                "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                pGina.Shared.Logging.InProcAppender.AddListener(SimLogHandler);
                SaveSettings();
                ResetSimUI();
                DoInternalSimulation();
                pGina.Shared.Logging.InProcAppender.RemoveListener(SimLogHandler);
            }
        }

        private void DoServiceClientSimulation()
        {
            ILog abstractionLogger = LogManager.GetLogger("Abstractions");
            LibraryLogging.AddListener(LibraryLogging.Level.Debug, abstractionLogger.DebugFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Error, abstractionLogger.ErrorFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Info, abstractionLogger.InfoFormat);
            LibraryLogging.AddListener(LibraryLogging.Level.Warn, abstractionLogger.WarnFormat);

            try
            {
                string pipeName = Core.Settings.Get.ServicePipeName;
                PipeClient client = new PipeClient(pipeName);
                client.Start(
                    (Func<IDictionary<string, object>, IDictionary<string, object>>)
                    ((m) =>
                        {
                        MessageType type = (MessageType)Enum.ToObject(typeof(MessageType), m["MessageType"]);

                        // Acceptable server responses are Hello, and LoginResponse
                        switch (type)
                        {
                            case MessageType.Hello:
                                // Send our login request
                                LoginRequestMessage requestMsg = new LoginRequestMessage()
                                {
                                    Username = m_username.Text,
                                    Password = m_password.Text,
                                };
                                return requestMsg.ToDict();
                            case MessageType.LoginResponse:
                                LoginResponseMessage responseMsg = new LoginResponseMessage(m);
                                m_usernameResult.Text = responseMsg.Username;
                                m_passwordResult.Text = responseMsg.Password;
                                m_domainResult.Text = responseMsg.Domain;
                                SetStageStatus(this.simFinalResultPB, responseMsg.Result);
                                if (responseMsg.Message != null)
                                {
                                    this.simFinalResultMessageTB.Text = responseMsg.Message;
                                }
                                // Respond with a disconnect, we're done
                                return (new EmptyMessage(MessageType.Disconnect).ToDict());
                            case MessageType.Ack:   // Ack to our disconnect
                                return null;
                            default:
                                m_logger.ErrorFormat("Server responded with invalid message type: {0}", type);
                                return null;
                        }
                    }),
                (new EmptyMessage(MessageType.Hello)).ToDict(), 1000);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error during service client simulation: {0}", e);
            }

            LibraryLogging.RemoveListener(LibraryLogging.Level.Debug, abstractionLogger.DebugFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Error, abstractionLogger.ErrorFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Info, abstractionLogger.InfoFormat);
            LibraryLogging.RemoveListener(LibraryLogging.Level.Warn, abstractionLogger.WarnFormat);
        }

        private void DoInternalSimulation()
        {
            Color successColor = Color.FromArgb(150, 255, 150);
            Color failColor = Color.FromArgb(255, 150, 150);

            PluginDriver sessionDriver = new PluginDriver();
            sessionDriver.UserInformation.Username = m_username.Text;
            sessionDriver.UserInformation.Password = m_password.Text;

            this.simPluginResultsListView.Items.Clear();

            // Execute the login process
            BooleanResult finalResult = sessionDriver.PerformLoginProcess();

            // Get the authentication results
            PluginActivityInformation actInfo = sessionDriver.SessionProperties.GetTrackedSingle<PluginActivityInformation>();
            bool authed = false;
            foreach (Guid uuid in actInfo.GetAuthenticationPlugins())
            {
                BooleanResult result = actInfo.GetAuthenticationResult(uuid);
                IPluginAuthentication plugin = actInfo.LoadedAuthenticationPlugins.Find(
                    delegate(IPluginAuthentication p) { return uuid == p.Uuid; }
                    );
                ListViewItem item = new ListViewItem(
                    new string[] { "Authentication", plugin.Name, result.Success.ToString(), result.Message });
                if (result.Success) item.BackColor = successColor;
                else item.BackColor = failColor;
                this.simPluginResultsListView.Items.Add(item);
                authed = authed || result.Success;
            }

            // Get the authorization results
            bool authzed = true;  // Default is true
            foreach (Guid uuid in actInfo.GetAuthorizationPlugins())
            {
                BooleanResult result = actInfo.GetAuthorizationResult(uuid);
                IPluginAuthorization plugin = actInfo.LoadedAuthorizationPlugins.Find(
                    delegate(IPluginAuthorization p) { return uuid == p.Uuid; }
                    );
                ListViewItem item = new ListViewItem(
                    new string[] { "Authorization", plugin.Name, result.Success.ToString(), result.Message });
                if (result.Success) item.BackColor = successColor;
                else item.BackColor = failColor;
                this.simPluginResultsListView.Items.Add(item);
                authzed = authzed && result.Success;
            }

            // Get the gateway results
            bool gatewayed = true;
            foreach (Guid uuid in actInfo.GetGatewayPlugins())
            {
                BooleanResult result = actInfo.GetGatewayResult(uuid);
                IPluginAuthenticationGateway plugin = actInfo.LoadedAuthenticationGatewayPlugins.Find(
                    delegate(IPluginAuthenticationGateway p) { return uuid == p.Uuid; });
                ListViewItem item = new ListViewItem(
                    new string[] { "Gateway", plugin.Name, result.Success.ToString(), result.Message });
                if (result.Success) item.BackColor = successColor;
                else item.BackColor = failColor;
                this.simPluginResultsListView.Items.Add(item);
                gatewayed = gatewayed && result.Success;
            }

            this.simPluginResultsListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.simPluginResultsListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.simPluginResultsListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
            this.simPluginResultsListView.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);

            // Update stage results
            SetStageStatus(this.simAuthResultPB, authed);
            SetStageStatus(this.simAuthzResultPB, authzed);
            SetStageStatus(this.simGatewayResultPB, gatewayed);

            m_usernameResult.Text = sessionDriver.UserInformation.Username;
            m_domainResult.Text = sessionDriver.UserInformation.Domain;
            m_passwordResult.Text = sessionDriver.UserInformation.Password;

            SetStageStatus(this.simFinalResultPB, finalResult.Success);
            this.simFinalResultMessageTB.Text = finalResult.Message;

            // Display final list of groups
            this.simResultLocalGroupsTB.Text =
                string.Join(", ", sessionDriver.UserInformation.Groups.Select(groupInfo => groupInfo.Name) );
        }

        private void SetStageStatus(PictureBox pb, bool success)
        {
            if (success)
                pb.Image = Configuration.Properties.Resources.greenCheckMark;
            else
                pb.Image = Configuration.Properties.Resources.redx;
        }

        private void ResetStageStatus()
        {
            this.simAuthResultPB.Image = Configuration.Properties.Resources.grayBar;
            this.simAuthzResultPB.Image = Configuration.Properties.Resources.grayBar;
            this.simGatewayResultPB.Image = Configuration.Properties.Resources.grayBar;
            this.simFinalResultPB.Image = Configuration.Properties.Resources.grayBar;
        }

        private void btnImageBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = m_tileImageTxt.Text;
            ofd.Filter = "Bitmap Files (.bmp)|*.bmp|All Files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_tileImageTxt.Text = ofd.FileName;
                LoadTileImagePreview();
            }
        }

        private void m_tabs_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == m_simTab)
            {
                try
                {
                    if (String.IsNullOrEmpty(m_tileImageTxt.Text.Trim()))
                    {
                        m_tileImage.Image = pGina.Configuration.Properties.Resources.pginalogo;
                    }
                    else
                    {
                        m_tileImage.Image = new Bitmap(m_tileImageTxt.Text);
                    }
                }
                catch (Exception)
                {
                    m_tileImage.Image = pGina.Configuration.Properties.Resources.pginalogo;
                }

                ResetStageStatus();
            }
        }

        private void m_lblAuthorizeResult_TextChanged(object sender, EventArgs e)
        {
            HandleLabelTextChange((Label)sender);
        }

        private void m_lblGatewayResult_TextChanged(object sender, EventArgs e)
        {
            HandleLabelTextChange((Label)sender);
        }

        private void pluginsDG_DoubleClick(object sender, EventArgs e)
        {
            configureButton_Click(sender, e);
        }

        private void serviceStartBtn_Click(object sender, EventArgs e)
        {
            if (m_pGinaServiceController.Status == ServiceControllerStatus.Stopped)
            {
                // Disable this right away, otherwise it isn't disabled until the
                // service controller shows it as "Start pending"
                serviceStartBtn.Enabled = false;
                try
                {
                    m_pGinaServiceController.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("{0}\nA dependent service is disabled?", ex.Message), "Can't start pGina service", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                UpdateServiceStatus();
            }
        }

        private void serviceStopBtn_Click(object sender, EventArgs e)
        {
            if (m_pGinaServiceController.Status == ServiceControllerStatus.Running)
            {
                // Disable this right away, otherwise it isn't disabled until the
                // service controller shows it as "Stop pending"
                serviceStopBtn.Enabled = false;
                try
                {
                    m_pGinaServiceController.Stop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("{0}", ex.Message), "Can't stop pGina service", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                UpdateServiceStatus();
            }
        }

        void m_serviceTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateServiceStatus();
        }

        private void cpRegisterBtn_Click(object sender, EventArgs e)
        {
            pGina.CredentialProvider.Registration.CredProviderManager manager =
                pGina.CredentialProvider.Registration.CredProviderManager.GetManager();

            try
            {
                if (manager.Registered())
                    manager.Uninstall();
                else
                {
                    FolderBrowserDialog dlg = new FolderBrowserDialog();
                    dlg.Description = "Select the folder containing Credential Provider/GINA DLL." +
                        "  Optionally, the DLL(s) may be contained in subfolders named 'x64' " +
                        " and 'Win32' for 64 and 32-bit DLLs.";
                    dlg.SelectedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        manager.CpInfo.Path = dlg.SelectedPath;
                        manager.Install();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            this.UpdateCpStatus();
        }

        private void cpEnableDisableBtn_Click(object sender, EventArgs e)
        {
            pGina.CredentialProvider.Registration.CredProviderManager manager =
                pGina.CredentialProvider.Registration.CredProviderManager.GetManager();

            try
            {
                if (manager.Registered())
                {
                    if (manager.Enabled())
                        manager.Disable();
                    else
                        manager.Enable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            this.UpdateCpStatus();
        }

        private void btnGinaBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DLL Files (*.dll)|*.dll";
            ofd.Multiselect = false;
            ofd.Title = "Select GINA to be chained";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_txtGinaChain.Text = ofd.FileName;
            }
        }

        private void SaveGinaSettings()
        {
            Settings.Get.ChainedGinaPath = m_txtGinaChain.Text;
            Settings.Get.EnableSpecialActionButton = chkSpecialButton.Checked;

            if (radioShutdown.Checked)
                Settings.Get.SpecialAction = "Shutdown";
            else if (radioRestart.Checked)
                Settings.Get.SpecialAction = "Restart";
            else if (radioSleep.Checked)
                Settings.Get.SpecialAction = "Sleep";
            else if (radioHibernate.Checked)
                Settings.Get.SpecialAction = "Hibernate";
        }

        private void SaveCpSettings()
        {
            List<CredProv> credProvs = (List<CredProv>)dgvCredProvFilter.DataSource;
            CredProvFilterConfig.SaveFilterSettings(credProvs);
        }

        private void chkSpecialButton_CheckedChanged(object sender, EventArgs e)
        {
            radioSleep.Enabled = chkSpecialButton.Checked;
            radioShutdown.Enabled = chkSpecialButton.Checked;
            radioRestart.Enabled = chkSpecialButton.Checked;
            radioHibernate.Enabled = chkSpecialButton.Checked;
        }

        private void showTextResultPasswordCB_CheckedChanged(object sender, EventArgs e)
        {
            this.m_passwordResult.UseSystemPasswordChar = ! this.showTextResultPasswordCB.Checked;
        }

        private void viewLogBtn_Click(object sender, EventArgs e)
        {
            this.logWindow.Visible = true;
        }

        private void enableMotdCB_CheckedChanged(object sender, EventArgs e)
        {
            this.motdTB.Enabled = this.enableMotdCB.Checked;
        }

        private void Btn_help(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation.html");
        }
    }
}
