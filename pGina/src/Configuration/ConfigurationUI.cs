using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using pGina.Core;
using pGina.Shared.Logging;
using pGina.Shared.Interfaces;

namespace pGina.Configuration
{
    public partial class ConfigurationUI : Form
    {
        // Plugin information keyed by Guid
        private Dictionary<string, IPluginBase> m_plugins = new Dictionary<string, IPluginBase>();
        
        private const string PLUGIN_UUID_COLUMN = "Uuid";
        private const string PLUGIN_VERSION_COLUMN = "Version";
        private const string PLUGIN_DESC_COLUMN = "Description";
        private const string PLUGIN_NAME_COLUMN = "Name";
        private const string AUTHENTICATION_COLUMN = "Authentication";
        private const string AUTHORIZATION_COLUMN = "Authorization";
        private const string GATEWAY_COLUMN = "Gateway";
        private const string NOTIFICATION_COLUMN = "Notification";
        private const string USER_SESSION_COLUMN = "UserSession";
        private const string SYSTEM_SESSION_COLUMN = "SystemSession";

        public ConfigurationUI()
        {
            Framework.Init();
            InitializeComponent();
            InitPluginsDGV();
            PopulatePluginDirs();
            InitOrderLists();
            RefreshPluginLists();
            InitLiveLog();
        }

        private void InitLiveLog()
        {
            m_liveLog.HeaderStyle = ColumnHeaderStyle.None;
            m_liveLog.Columns[0].Width = m_liveLog.Width - 5;         
        }

        private void InitOrderLists()
        {
            // Setup the DataGridViews
            InitPluginOrderDGV(this.authenticateDGV);
            InitPluginOrderDGV(this.authorizeDGV);
            InitPluginOrderDGV(this.gatewayDGV);
            InitPluginOrderDGV(this.eventDGV);
            InitPluginOrderDGV(this.userDGV);
            InitPluginOrderDGV(this.systemDGV);            

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
                Name = USER_SESSION_COLUMN,
                HeaderText = "User Session",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = SYSTEM_SESSION_COLUMN,
                HeaderText = "System Session",
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
                    case SYSTEM_SESSION_COLUMN:
                        SyncStateToList(checkBoxState, plug, systemDGV);
                        break;
                    case USER_SESSION_COLUMN:
                        SyncStateToList(checkBoxState, plug, userDGV);
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
                        new object[] { p.Name, false, false, false, false, false, false, p.Description, p.Version, p.Uuid.ToString() });
                    DataGridViewRow row = pluginsDG.Rows[i];

                    this.SetupCheckBoxCell<IPluginAuthentication>(row.Cells[AUTHENTICATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthorization>(row.Cells[AUTHORIZATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginAuthenticationGateway>(row.Cells[GATEWAY_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginEventNotifications>(row.Cells[NOTIFICATION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginUserSessionHelper>(row.Cells[USER_SESSION_COLUMN], p);
                    this.SetupCheckBoxCell<IPluginSystemSessionHelper>(row.Cells[SYSTEM_SESSION_COLUMN], p);
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
            LoadPluginOrderListFromReg<IPluginSystemSessionHelper>(systemDGV);
            LoadPluginOrderListFromReg<IPluginUserSessionHelper>(userDGV);                        
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

                if (!row.Cells[USER_SESSION_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[USER_SESSION_COLUMN].Value, plug, userDGV);

                if (!row.Cells[SYSTEM_SESSION_COLUMN].ReadOnly)
                    SyncStateToList((bool)row.Cells[SYSTEM_SESSION_COLUMN].Value, plug, systemDGV);
                
            }

            // Remove any plugins that are no longer in the main list from the
            // ordered lists
            this.RemoveAllNotInMainList(authorizeDGV);
            this.RemoveAllNotInMainList(authenticateDGV);
            this.RemoveAllNotInMainList(gatewayDGV);
            this.RemoveAllNotInMainList(eventDGV);
            this.RemoveAllNotInMainList(systemDGV);
            this.RemoveAllNotInMainList(userDGV);
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
                    if (Convert.ToBoolean(row.Cells[SYSTEM_SESSION_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.SystemSessionEnabled;
                    if (Convert.ToBoolean(row.Cells[USER_SESSION_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.UserSessionEnabled;

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
            SavePluginOrder(systemDGV, typeof(IPluginSystemSessionHelper));
            SavePluginOrder(userDGV, typeof(IPluginUserSessionHelper));                        
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
            SaveSettings();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
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
            if (sender == m_radioCredUI || sender == m_radioUseService)
            {
                m_useSavedConfig.Checked = true;
                m_useSavedConfig.Enabled = false;
            }
            else
            {
                m_useSavedConfig.Enabled = true;
            }
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

        private void systemBtnUp_Click(object sender, EventArgs e)
        {
            if (this.systemDGV.SelectedRows.Count > 0)
                MoveUp(this.systemDGV, this.systemDGV.SelectedRows[0].Index);
        }

        private void systemBtnDown_Click(object sender, EventArgs e)
        {
            if (this.systemDGV.SelectedRows.Count > 0)
                MoveDown(this.systemDGV, this.systemDGV.SelectedRows[0].Index);
        }

        private void userBtnUp_Click(object sender, EventArgs e)
        {
            if (this.userDGV.SelectedRows.Count > 0)
                MoveUp(this.userDGV, this.userDGV.SelectedRows[0].Index);
        }

        private void userBtnDown_Click(object sender, EventArgs e)
        {
            if (this.userDGV.SelectedRows.Count > 0)
                MoveDown(this.userDGV, this.userDGV.SelectedRows[0].Index);
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
    }
}
