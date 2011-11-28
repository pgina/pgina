/*
	Copyright (c) 2011, pGina Team
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

using pGina.Core;
using pGina.Core.Messages;

using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

using Abstractions.WindowsApi;
using Abstractions.Logging;
using Abstractions.Pipes;

using log4net;

namespace pGina.Configuration
{
    public partial class ConfigurationUI : Form
    {
        private static readonly string PGINA_SERVICE_NAME = "pGina";

        // Plugin information keyed by Guid
        private Dictionary<string, IPluginBase> m_plugins = new Dictionary<string, IPluginBase>();
        private ILog m_logger = LogManager.GetLogger("ConfigurationUI");

        private ServiceController m_pGinaServiceController = null;
        private System.Timers.Timer m_serviceTimer = new System.Timers.Timer();
        
        private const string PLUGIN_UUID_COLUMN = "Uuid";
        private const string PLUGIN_VERSION_COLUMN = "Version";
        private const string PLUGIN_DESC_COLUMN = "Description";
        private const string PLUGIN_NAME_COLUMN = "Name";
        private const string AUTHENTICATION_COLUMN = "Authentication";
        private const string AUTHORIZATION_COLUMN = "Authorization";
        private const string GATEWAY_COLUMN = "Gateway";
        private const string NOTIFICATION_COLUMN = "Notification";
        
        public ConfigurationUI()
        {
            Framework.Init();
            InitializeComponent();
            InitOptionsTabs();
            InitPluginsDGV();
            PopulatePluginDirs();
            InitOrderLists();
            RefreshPluginLists();
            InitLiveLog();
            LoadGeneralSettings();            
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
            CredentialProvider.Registration.Settings msProviderSettings = new CredentialProvider.Registration.Settings()
            {
                ProviderGuid = new Guid("{6f45dc1e-5384-457a-bc13-2cd81b0d28ed}"),
                ShortName = "PasswordProvider",
            };

            CredentialProvider.Registration.CredProviderManager manager = CredentialProvider.Registration.CredProviderManager.GetManager();
            manager.CpInfo = msProviderSettings;
            chkMSProvider.Checked = manager.Enabled();
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
            m_tileImageTxt.Text = Settings.Get.GetSetting("TileImage", null);
            LoadTileImagePreview();
            this.motdTB.Text = Settings.Get.GetSetting("Motd");

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

        private void InitLiveLog()
        {
            m_liveLog.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            m_liveLog.Columns[0].Width = m_liveLog.Width - 5;
            m_liveLog.Columns[0].Text = null;
        }

        private void InitOrderLists()
        {
            // Setup the DataGridViews
            InitPluginOrderDGV(this.authenticateDGV);
            InitPluginOrderDGV(this.authorizeDGV);
            InitPluginOrderDGV(this.gatewayDGV);
            InitPluginOrderDGV(this.eventDGV);
            
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
                    this.m_plugins.Add(p.Uuid.ToString(), p);
                    pluginsDG.Rows.Add(
                        new object[] { p.Name, false, false, false, false, p.Description, p.Version, p.Uuid.ToString() });
                    DataGridViewRow row = pluginsDG.Rows[i];

                    this.SetupCheckBoxCell<IPluginAuthentication>(row.Cells[AUTHENTICATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthorization>(row.Cells[AUTHORIZATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthenticationGateway>(row.Cells[GATEWAY_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginEventNotifications>(row.Cells[NOTIFICATION_COLUMN], p);                    
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
            }

            // Remove any plugins that are no longer in the main list from the
            // ordered lists
            this.RemoveAllNotInMainList(authorizeDGV);
            this.RemoveAllNotInMainList(authenticateDGV);
            this.RemoveAllNotInMainList(gatewayDGV);
            this.RemoveAllNotInMainList(eventDGV);            
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

        private void SaveSettings()
        {
            this.SavePluginSettings();
            this.SavePluginDirs();
            this.SavePluginOrder();

            Core.Settings.Get.TileImage = m_tileImageTxt.Text;
            this.LoadTileImagePreview();
            Settings.Get.Motd = this.motdTB.Text.Trim();

            if (Abstractions.Windows.OsInfo.IsVistaOrLater())
                this.SaveCpSettings();
            else
                this.SaveGinaSettings();
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
            SaveSettings();
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
                this.RefreshPluginLists();
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
                this.RefreshPluginLists();
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
            chkIgnoreAuthenticateError.Enabled = (sender == m_radioEmulate);
            chkIgnoreAuthzError.Enabled = (sender == m_radioEmulate);            
            chkIgnoreGateway.Enabled = (sender == m_radioEmulate);
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

        private void btnLaunchCredUI_Click(object sender, EventArgs e)
        {
            ResetSimUI();
            System.Net.NetworkCredential credential = pInvokes.GetCredentials("Simulated Login", "Please enter your credentials...");
            if (credential != null)
            {
                m_lblAuthResult.Text = "Success";
                m_lblAuthorizeResult.Text = "Success";
                m_lblGatewayResult.Text = "Success";
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
            m_liveLog.Items.Add(new ListViewItem(new string[] { message }));
        }

        private void ResetSimUI()
        {
            m_message.Text = null;
            m_liveLog.Items.Clear();
            m_lblAuthorizeResult.Text = null;
            m_lblAuthResult.Text = null;
            m_lblGatewayResult.Text = null;
            m_usernameResult.Text = null;
            m_domainResult.Text = null;
            m_passwordResult.Text = null;
        }

        private void btnSimGo_Click(object sender, EventArgs e)
        {            
            ResetSimUI();
            pGina.Shared.Logging.InProcAppender.AddListener(SimLogHandler);

            if (m_radioUseService.Checked)
            {
                DoServiceClientSimulation();
            }
            else
            {
                if (MessageBox.Show("Continuing will save all pending changes in configuration, do you want to continue?",
                                "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                SaveSettings();

                DoInternalSimulation();
            }

            pGina.Shared.Logging.InProcAppender.RemoveListener(SimLogHandler);
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
                    (Func<dynamic, dynamic>)
                    ((m) =>
                    {
                        MessageType type = (MessageType) m.MessageType;
                        
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
                                return requestMsg.ToExpando();                                
                            case MessageType.LoginResponse:
                                LoginResponseMessage responseMsg = new LoginResponseMessage(m);
                                SetLabelStatus(m_lblAuthResult, responseMsg.Result);
                                m_usernameResult.Text = responseMsg.Username;
                                m_passwordResult.Text = responseMsg.Password;
                                m_domainResult.Text = responseMsg.Domain;
                                if (responseMsg.Message != null)
                                {
                                    m_message.Text += responseMsg.Message;
                                    m_message.Text += "\r\n";
                                }
                                // Respond with a disconnect, we're done
                                return (new EmptyMessage(MessageType.Disconnect).ToExpando());
                            case MessageType.Ack:   // Ack to our disconnect
                                return null;    
                            default:
                                m_logger.ErrorFormat("Server responded with invalid message type: {0}", type);
                                return null;                                
                        }                                                
                    }),
                (new EmptyMessage(MessageType.Hello)).ToExpando(), 1000);
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

            PluginDriver sessionDriver = new PluginDriver();
            sessionDriver.UserInformation.Username = m_username.Text;
            sessionDriver.UserInformation.Password = m_password.Text;

            // We could use PerformLoginProcess, but we want to know what each
            // stage reports, so we basically duplicate it here to know whats goin on.
            bool authenticated = false;
            bool authorized = false;
            bool gatewayed = false;
            
            PluginDriver.Starting();

            try
            {
                Shared.Types.BooleanResult result = sessionDriver.AuthenticateUser();
                authenticated = result.Success;
                if (result.Message != null)
                {
                    m_message.Text += result.Message;
                    m_message.Text += "\r\n";
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Exception during authenticate: {0}", ex);
            }

            if (authenticated || chkIgnoreAuthenticateError.Checked)
            {
                try
                {
                    Shared.Types.BooleanResult result = sessionDriver.AuthorizeUser();
                    authorized = result.Success;
                    if (result.Message != null)
                    {
                        m_message.Text += result.Message;
                        m_message.Text += "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("Exception during authorize: {0}", ex);
                }
            }

            if (authorized || chkIgnoreAuthzError.Checked)
            {
                try
                {
                    Shared.Types.BooleanResult result = sessionDriver.GatewayProcess();
                    gatewayed = result.Success;
                    if (result.Message != null)
                    {
                        m_message.Text += result.Message;
                        m_message.Text += "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("Exception during gateway: {0}", ex);
                }
            }

            SetLabelStatus(m_lblAuthResult, authenticated);
            SetLabelStatus(m_lblAuthorizeResult, authorized);
            SetLabelStatus(m_lblGatewayResult, gatewayed);
            m_usernameResult.Text = sessionDriver.UserInformation.Username;
            m_domainResult.Text = sessionDriver.UserInformation.Domain;
            m_passwordResult.Text = sessionDriver.UserInformation.Password;

            // Simplify, now that we've reported actual result to the UI
            authenticated |= chkIgnoreAuthenticateError.Checked;
            authorized |= chkIgnoreAuthzError.Checked;
            gatewayed |= chkIgnoreGateway.Checked;            
            
            PluginDriver.Stopping();
        }


        private void SetLabelStatus(Label lbl, bool success)
        {
            if (success)
                lbl.Text = "Success";
            else
                lbl.Text = "Failure";
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

                m_lblAuthorizeResult.Text = null;
                m_lblAuthResult.Text = null;
                m_lblGatewayResult.Text = null;
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

        private void m_liveLog_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text File (.txt)|*.txt|All Files (*.*)|*.*";
                sfd.CheckPathExists = true;

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (StreamWriter file = new StreamWriter(sfd.FileName))
                    {
                        foreach (ListViewItem item in m_liveLog.SelectedItems)
                        {
                            file.WriteLine(item.Text.TrimEnd(new char[] { '\n' }));                            
                        }                        
                    }

                    MessageBox.Show(string.Format("File saved successfully: {0}", sfd.FileName), "Log Export", MessageBoxButtons.OK);
                }
            }
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
                m_pGinaServiceController.Start();
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
                m_pGinaServiceController.Stop();
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
            CredentialProvider.Registration.Settings msProviderSettings = new CredentialProvider.Registration.Settings()
            {
                ProviderGuid = new Guid("{6f45dc1e-5384-457a-bc13-2cd81b0d28ed}"),
                ShortName = "PasswordProvider",
            };

            CredentialProvider.Registration.CredProviderManager manager = CredentialProvider.Registration.CredProviderManager.GetManager();
            manager.CpInfo = msProviderSettings;
            bool currentState = manager.Enabled();

            if (chkMSProvider.Checked != currentState)
            {
                if (chkMSProvider.Checked)
                    manager.Enable();
                else
                {
                    if (MessageBox.Show("You have chosen to disable the built-in password provider, are you sure?", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                    {
                        manager.Disable();
                    }
                }
            }
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
    }
}
