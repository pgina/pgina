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
        private Dictionary<string, IPluginBase> m_plugins = new Dictionary<string,IPluginBase>();

        private static readonly string PLUGIN_NAME_COLUMN = "Name";
        private static readonly string AUTH_UI_COLUMN = "UI";
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
            RefreshPluginList();
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
                Width = 250,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            pluginsDG.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = AUTH_UI_COLUMN,
                HeaderText = "UI",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
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
        }

        private void RefreshPluginList()
        {
            // Get plugins
            PluginLoader loader = new PluginLoader();
            loader.PluginDirectories = Settings.Get.PluginDirectories;
            loader.Load();
            List<IPluginBase> plugins = loader.AllPlugins;
            
            m_plugins.Clear();
            pluginsDG.Rows.Clear();

            for(int i = 0; i < plugins.Count; i++ )
            {
                IPluginBase p = plugins[i];
                this.m_plugins.Add(p.Name, p);
                pluginsDG.Rows.Add(
                    new object[] { p.Name, false, false, false, false, false, false, false });
                DataGridViewRow row = pluginsDG.Rows[i];

                this.SetupCheckBoxCell<IPluginAuthenticationUI>(row.Cells[AUTH_UI_COLUMN], p);
                this.SetupCheckBoxCell<IPluginAuthentication>(row.Cells[AUTHENTICATION_COLUMN], p);
                this.SetupCheckBoxCell<IPluginAuthorization>(row.Cells[AUTHORIZATION_COLUMN], p);
                this.SetupCheckBoxCell<IPluginAuthenticationGateway>(row.Cells[GATEWAY_COLUMN], p);
                this.SetupCheckBoxCell<IPluginEventNotifications>(row.Cells[NOTIFICATION_COLUMN], p);
                this.SetupCheckBoxCell<IPluginUserSessionHelper>(row.Cells[USER_SESSION_COLUMN], p);
                this.SetupCheckBoxCell<IPluginSystemSessionHelper>(row.Cells[SYSTEM_SESSION_COLUMN], p);
            }
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

        private void pluginsDG_PaintCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Determine if the cell should have a checkbox or not (via the ReadOnly setting), 
            // if not, we draw over the checkbox.
            if (e != null && sender != null)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex > 0 && 
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
            lstPluginDirs.Columns[0].Width = lstPluginDirs.Width;
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
                    IPluginBase p = m_plugins[(string)row.Cells[PLUGIN_NAME_COLUMN].Value];
                    int mask = 0;

                    if (Convert.ToBoolean(row.Cells[AUTH_UI_COLUMN].Value))
                        mask |= (int)Core.PluginLoader.State.UIEnabled;
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

        private void SaveSettings()
        {
            this.SavePluginSettings();
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
            }
        }

        private void configureButton_Click(object sender, EventArgs e)
        {
            int nSelectedRows = pluginsDG.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = pluginsDG.SelectedRows[0];
                string pluginName = (string)row.Cells[PLUGIN_NAME_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginName];

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
                string pluginName = (string)row.Cells[PLUGIN_NAME_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginName];

                configureButton.Enabled = plug is IPluginConfiguration;
            }
        }

        private void pluginInfoButton_Click(object sender, EventArgs e)
        {
            int nSelectedRows = pluginsDG.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = pluginsDG.SelectedRows[0];
                string pluginName = (string)row.Cells[PLUGIN_NAME_COLUMN].Value;
                IPluginBase plug = this.m_plugins[pluginName];

                PluginInfoForm dialog = new PluginInfoForm();
                dialog.Plugin = plug;
                dialog.Show();
            }
        }

    }
}
