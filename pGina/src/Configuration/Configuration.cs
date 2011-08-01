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
    public partial class Configuration : Form
    {
        // Plugin information keyed by Guid
        private Dictionary<string, IPluginBase> m_plugins = new Dictionary<string,IPluginBase>();

        private static readonly string PLUGIN_UUID_COLUMN = "Uuid";
        private static readonly string PLUGIN_NAME_COLUMN = "Name";        
        private static readonly string AUTHENTICATION_COLUMN = "Authentication";
        private static readonly string AUTHORIZATION_COLUMN = "Authorization";
        private static readonly string GATEWAY_COLUMN = "Gateway";
        private static readonly string NOTIFICATION_COLUMN = "Notification";
        private static readonly string USER_SESSION_COLUMN = "UserSession";
        private static readonly string SYSTEM_SESSION_COLUMN = "SystemSession";

        public Configuration()
        {
            Framework.Init();
            InitializeComponent();
            InitPluginsDGV();
            PopulatePluginDirs();
            InitOrderLists();
            RefreshPluginLists();
        }

        private void InitOrderLists()
        {
            // Setup the DataGridViews
            InitPluginOrderDGV(this.authenticateDGV);
            InitPluginOrderDGV(this.authorizeDGV);
            InitPluginOrderDGV(this.gatewayDGV);

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
                Name = PLUGIN_UUID_COLUMN,
                Visible = false
            });
            pluginsDG.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PLUGIN_NAME_COLUMN,
                HeaderText = "Plugin Name",
                Width = 250,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
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

            // Implement the cell paint event so that we can blank out cells
            // that shouldn't be there.
            pluginsDG.CellPainting += this.pluginsDG_PaintCell;

            pluginsDG.SelectionChanged += this.pluginsDG_SelectionChanged;
            pluginsDG.CurrentCellDirtyStateChanged += new EventHandler(pluginsDG_CurrentCellDirtyStateChanged);
            pluginsDG.CellValueChanged += new DataGridViewCellEventHandler(pluginsDG_CellValueChanged);
        }

        void pluginsDG_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 1 && e.RowIndex >= 0)
            {
                DataGridViewCell cell = this.pluginsDG[e.ColumnIndex, e.RowIndex];
                string uuid = (string)this.pluginsDG.Rows[e.RowIndex].Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = m_plugins[uuid];
                bool checkBoxState = Convert.ToBoolean(cell.Value);
                string columnName = pluginsDG.Columns[e.ColumnIndex].Name;

                if (columnName == AUTHENTICATION_COLUMN)
                {
                    if (checkBoxState)
                        this.AddToOrderList(plug, this.authenticateDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.authenticateDGV);
                }
                if (columnName == AUTHORIZATION_COLUMN)
                {
                    if (checkBoxState)
                        this.AddToOrderList(plug, this.authorizeDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.authorizeDGV);
                }
                if (columnName == GATEWAY_COLUMN)
                {
                    if (checkBoxState)
                        this.AddToOrderList(plug, this.gatewayDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.gatewayDGV);
                }
            }
        }

        void pluginsDG_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.pluginsDG.IsCurrentCellDirty)
            {
                this.pluginsDG.CommitEdit(DataGridViewDataErrorContexts.Commit);
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
                        new object[] { p.Uuid.ToString(), p.Name, false, false, false, false, false, false });
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
            this.authenticateDGV.Rows.Clear();
            this.authorizeDGV.Rows.Clear();
            this.gatewayDGV.Rows.Clear();

            List<IPluginAuthentication> authenticatePlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();
            List<IPluginAuthorization> authorizePlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();
            List<IPluginAuthenticationGateway> gatewayPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();
            
            foreach (IPluginBase plug in authenticatePlugins)
            {                
                AddToOrderList(plug, this.authenticateDGV);
            }

            foreach (IPluginBase plug in authorizePlugins)
            {
                AddToOrderList(plug, this.authorizeDGV);
            }

            foreach (IPluginBase plug in gatewayPlugins)
            {
                AddToOrderList(plug, this.gatewayDGV);
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
                {
                    bool enabledForAuthentication = (bool)row.Cells[AUTHENTICATION_COLUMN].Value;
                    if (enabledForAuthentication)
                        this.AddToOrderList(plug, this.authenticateDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.authenticateDGV);
                }

                if (!row.Cells[AUTHORIZATION_COLUMN].ReadOnly)
                {
                    bool enabledForAuthorization = (bool)row.Cells[AUTHORIZATION_COLUMN].Value;
                    if (enabledForAuthorization)
                        this.AddToOrderList(plug, this.authorizeDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.authorizeDGV);
                }

                if (!row.Cells[GATEWAY_COLUMN].ReadOnly)
                {
                    bool enabledForGateway = (bool)row.Cells[GATEWAY_COLUMN].Value;
                    if (enabledForGateway)
                        this.AddToOrderList(plug, this.gatewayDGV);
                    else
                        this.RemoveFromOrderList(uuid, this.gatewayDGV);
                }
            }

            // Remove any plugins that are no longer in the main list from the
            // ordered lists
            this.RemoveAllNotInMainList(authorizeDGV);
            this.RemoveAllNotInMainList(authenticateDGV);
            this.RemoveAllNotInMainList(gatewayDGV);
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
            if (! viewContainsPlugin(dgv, plug.Uuid.ToString()) )
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
                if (e.RowIndex >= 0 && e.ColumnIndex > 1 && 
                    pluginsDG[e.ColumnIndex, e.RowIndex].ReadOnly )
                {
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
            foreach( DataGridViewRow row in pluginsDG.Rows )
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
            List<string> orderedList = new List<string>();
            string setting = typeof(IPluginAuthorization).Name + "_Order";
            orderedList.Clear();
            foreach (DataGridViewRow row in authorizeDGV.Rows)
            {
                orderedList.Add((string)row.Cells[PLUGIN_UUID_COLUMN].Value);
            }
            Settings.Get.SetSetting(setting, orderedList.ToArray<string>());

            setting = typeof(IPluginAuthentication).Name + "_Order";
            orderedList.Clear();
            foreach (DataGridViewRow row in authenticateDGV.Rows)
            {
                orderedList.Add((string)row.Cells[PLUGIN_UUID_COLUMN].Value);
            }
            Settings.Get.SetSetting(setting, orderedList.ToArray<string>());

            setting = typeof(IPluginAuthenticationGateway).Name + "_Order";
            orderedList.Clear();
            foreach (DataGridViewRow row in gatewayDGV.Rows)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
                
        private void btnAdd_Click_1(object sender, EventArgs e)
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

        private void btnRemove_Click_1(object sender, EventArgs e)
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

        private void pluginInfoButton_Click(object sender, EventArgs e)
        {
            int nSelectedRows = pluginsDG.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = pluginsDG.SelectedRows[0];
                string pluginUuid = (string)row.Cells[PLUGIN_UUID_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginUuid];

                PluginInfoForm dialog = new PluginInfoForm();
                dialog.Plugin = plug;
                dialog.Show();
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
    }
}
