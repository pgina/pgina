using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCredential.Plugin.DatabaseAuth
{
    public partial class Configuration
    {
        private TextBox offlineStatusText;
        private NumericUpDown offlineMaxAgeHours;
        private Button synchronizeOfflineButton;
        private Button refreshOfflineButton;
        private bool offlineSyncRunning;

        private void InitializeOfflineStatusControls(bool preview)
        {
            var resources = new ComponentResourceManager(typeof(Configuration));
            var page = new TabPage(resources.GetString("OfflineStatus.Title")) { Name = "offlineStatusTab", Padding = new Padding(8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(4) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var expiry = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, TabIndex = 0 };
            var expiryLabel = new Label { Text = resources.GetString("OfflineStatus.MaximumAge"), AutoSize = true, Margin = new Padding(4, 7, 4, 4) };
            offlineMaxAgeHours = new NumericUpDown { Name = "offlineMaxAgeHours", Minimum = 1, Maximum = 168, Value = 24,
                Width = 80, AccessibleName = resources.GetString("OfflineStatus.MaximumAge"), TabIndex = 1 };
            expiry.Controls.Add(expiryLabel);
            expiry.Controls.Add(offlineMaxAgeHours);
            layout.Controls.Add(expiry, 0, 0);
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, TabIndex = 1 };
            synchronizeOfflineButton = new Button { Name = "synchronizeOfflineButton", Text = resources.GetString("OfflineStatus.Sync"), AutoSize = true,
                MinimumSize = new Size(120, 30), Enabled = !preview, TabIndex = 0 };
            refreshOfflineButton = new Button { Name = "refreshOfflineButton", Text = resources.GetString("OfflineStatus.Refresh"), AutoSize = true,
                MinimumSize = new Size(120, 30), Enabled = !preview, TabIndex = 1 };
            synchronizeOfflineButton.AccessibleName = synchronizeOfflineButton.Text.Replace("&", "");
            refreshOfflineButton.AccessibleName = refreshOfflineButton.Text.Replace("&", "");
            actions.Controls.Add(synchronizeOfflineButton);
            actions.Controls.Add(refreshOfflineButton);
            layout.Controls.Add(actions, 0, 1);
            offlineStatusText = new TextBox { Name = "offlineStatusText", Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, TabIndex = 2,
                BackColor = SystemColors.Window, ForeColor = SystemColors.WindowText,
                AccessibleName = resources.GetString("OfflineStatus.StatusName"), Text = resources.GetString("OfflineStatus.Initial") };
            layout.Controls.Add(offlineStatusText, 0, 2);
            var help = new Label { Text = resources.GetString("OfflineStatus.Help"), AutoSize = true, Dock = DockStyle.Fill, MaximumSize = new Size(480, 0) };
            layout.Controls.Add(help, 0, 3);
            page.Controls.Add(layout);
            tabControl1.TabPages.Add(page);
            refreshOfflineButton.Click += delegate { RefreshOfflineStatus(); };
            synchronizeOfflineButton.Click += SynchronizeOffline_Click;
            tabControl1.SelectedIndexChanged += delegate
            {
                if (!preview && !offlineSyncRunning && tabControl1.SelectedTab == page) RefreshOfflineStatus();
            };
        }

        private void RefreshOfflineStatus()
        {
            try
            {
                synchronizeOfflineButton.Enabled = Settings.IsLocalCacheEnabled() && !offlineSyncRunning;
                offlineStatusText.Text = (Settings.IsLocalCacheEnabled() ? "Local caching enabled." : "Local caching disabled.") +
                    Environment.NewLine + "Saved maximum record age: " + Settings.GetOfflineCacheMaxAgeHours() + " hours." +
                    Environment.NewLine + LocalUserCache.GetStatus().ToDisplayText();
            }
            catch (Exception)
            {
                offlineStatusText.Text = "Unable to read the local copy. Check cache permissions and SQLite availability. No credentials are displayed.";
            }
        }

        private async void SynchronizeOffline_Click(object sender, EventArgs e)
        {
            if (offlineSyncRunning) return;
            offlineSyncRunning = true;
            bool wasEnabled = synchronizeOfflineButton.Enabled;
            bool couldSave = saveButton.Enabled;
            synchronizeOfflineButton.Enabled = refreshOfflineButton.Enabled = saveButton.Enabled = false;
            offlineStatusText.Text = "Synchronizing the complete user copy using saved settings. Existing data remains valid until the new copy commits.";
            try
            {
                var result = await Task.Run(() => PluginImpl.SynchronizeLocalCache(true));
                if (IsDisposed || Disposing) return;
                RefreshOfflineStatus();
                offlineStatusText.AppendText(Environment.NewLine + "Requested synchronization: " + result + ".");
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing) offlineStatusText.Text = "Synchronization failed. Check the saved configuration; the previous complete copy was not replaced.";
            }
            finally
            {
                offlineSyncRunning = false;
                if (!IsDisposed && !Disposing)
                {
                    synchronizeOfflineButton.Enabled = wasEnabled;
                    refreshOfflineButton.Enabled = true;
                    saveButton.Enabled = couldSave;
                }
            }
        }
    }
}
