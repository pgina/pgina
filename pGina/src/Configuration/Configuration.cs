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

namespace Configuration
{
    public partial class Configuration : Form
    {
        private PluginLoader m_loader = new PluginLoader();
        List<pGina.Configuration.PluginPage> m_pages = new List<pGina.Configuration.PluginPage>();
        
        public Configuration()
        {
            Framework.Init();            
            InitializeComponent();
            LoadSettings();
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

        private void LoadPlugins()
        {
            // Load plugins and add their pages
            m_loader.PluginDirectories = Settings.Get.PluginDirectories; 
            m_loader.Load();

            foreach (IPluginBase plugin in m_loader.AllPlugins)
            {
                pGina.Configuration.PluginPage page = new pGina.Configuration.PluginPage(plugin);
                m_pages.Add(page);
                this.tabConfig.Controls.Add(page.Page);
            }
        }

        private void PopulatePluginOrder()
        {
            // Now plugin order
            lstOrder.Sorting = SortOrder.None;            
            lstOrder.Columns.Clear();
            lstOrder.Columns.Add("Plugin");
            lstOrder.Columns[0].Width = lstOrder.Width / 2;
            lstOrder.Columns.Add("Type");
            lstOrder.Columns[1].Width = lstOrder.Width / 2;

            List<IPluginAuthenticationUI> uiPlugins = m_loader.GetOrderedPluginsOfType<IPluginAuthenticationUI>();
            List<IPluginAuthentication> authPlugins = m_loader.GetOrderedPluginsOfType<IPluginAuthentication>();
            List<IPluginAuthorization> authzPlugins = m_loader.GetOrderedPluginsOfType<IPluginAuthorization>();
            List<IPluginAuthenticationGateway> gatePlugins = m_loader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();

            foreach(var p in uiPlugins)
            {                
                ListViewItem item = new ListViewItem(new string[] { p.Name, "UI" });
                item.Tag = p;                                
                lstOrder.Items.Add(item);
            }

            foreach(var p in authPlugins)
            {                
                ListViewItem item = new ListViewItem(new string[] { p.Name, "Authentication" });                                                
                item.Tag = p;                
                lstOrder.Items.Add(item);
            }

            foreach (var p in authzPlugins)
            {
                ListViewItem item = new ListViewItem(new string[] { p.Name, "Authorization" });
                item.Tag = p;                
                lstOrder.Items.Add(item);
            }

            foreach (var p in gatePlugins)
            {
                ListViewItem item = new ListViewItem(new string[] { p.Name, "Gateway" });
                item.Tag = p;                
                lstOrder.Items.Add(item);
            }            
        }

        private void LoadSettings()
        {
            PopulatePluginDirs();
            LoadPlugins();
            PopulatePluginOrder();                        
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

        private void SavePages()
        {
            foreach (pGina.Configuration.PluginPage page in m_pages)
            {
                page.Save();
            }
        }

        private void SavePluginOrder()
        {
            List<string> ui = new List<string>();
            List<string> auth = new List<string>();
            List<string> authz = new List<string>();
            List<string> gate = new List<string>();

            foreach (ListViewItem item in lstOrder.Items)
            {
                switch (item.SubItems[1].Text)
                {
                    case "UI":
                        ui.Add(((IPluginBase)item.Tag).Uuid.ToString());
                        break;
                    case "Authentication":
                        auth.Add(((IPluginBase)item.Tag).Uuid.ToString());
                        break;
                    case "Authorization":
                        authz.Add(((IPluginBase)item.Tag).Uuid.ToString());
                        break;
                    case "Gateway":
                        gate.Add(((IPluginBase)item.Tag).Uuid.ToString());
                        break;
                }
            }

            pGina.Core.Settings.Get.SetSetting("IPluginAuthenticationUI_Order", ui.ToArray());
            pGina.Core.Settings.Get.SetSetting("IPluginAuthentication_Order", auth.ToArray());
            pGina.Core.Settings.Get.SetSetting("IPluginAuthorization_Order", authz.ToArray());
            pGina.Core.Settings.Get.SetSetting("IPluginAuthenticationGateway_Order", gate.ToArray());
        }

        private void SaveSettings()
        {
            SavePluginDirs();
            SavePages();
            SavePluginOrder();
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

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lstOrder.SelectedItems.Count > 0)
            {
                ListViewItem item = lstOrder.SelectedItems[0];
                int srcIdx = item.Index;                
                int destIdx = item.Index - 1;
                if (destIdx > 0 && lstOrder.Items[destIdx].SubItems[1].Text == item.SubItems[1].Text)
                {                    
                    lstOrder.Items.RemoveAt(srcIdx);
                    lstOrder.Items.Insert(destIdx, item);
                    lstOrder.Items[destIdx].Selected = true;
                }
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lstOrder.SelectedItems.Count > 0)
            {
                ListViewItem item = lstOrder.SelectedItems[0];
                int srcIdx = item.Index;
                int destIdx = item.Index + 1;
                if (destIdx < lstOrder.Items.Count && lstOrder.Items[destIdx].SubItems[1].Text == item.SubItems[1].Text)
                {
                    lstOrder.Items.RemoveAt(srcIdx);
                    lstOrder.Items.Insert(destIdx, item);
                    lstOrder.Items[destIdx].Selected = true;
                }
            }
        }
    }
}
