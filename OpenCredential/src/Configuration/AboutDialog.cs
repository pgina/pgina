using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenCredential.Shared.Localization;

namespace OpenCredential.Configuration
{
    internal sealed class AboutDialog : Form
    {
        private static string T(string key, string english) { return UiLanguage.Text("Configuration", key, english); }

        internal AboutDialog()
        {
            Name = "aboutDialog";
            Text = T("About.Title", "About OpenCredential");
            AccessibleName = Text;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Font = SystemFonts.MessageBoxFont;
            Size = new Size(760, 640);
            MinimumSize = new Size(560, 480);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var assembly = typeof(ConfigurationUI).Assembly;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            layout.Controls.Add(new Label { AutoSize = true, Text = "OpenCredential " + (version == null ? assembly.GetName().Version.ToString() : version.InformationalVersion) });
            var credits = new TextBox { Name = "aboutCredits", Multiline = true, ReadOnly = true, BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill, Height = 80, BackColor = SystemColors.Window, ForeColor = SystemColors.WindowText,
                AccessibleName = T("About.Credits", "Developers of this version"), Text = T("About.Credits", "Developers of this version") +
                    "\r\nYuri Mercedes Bermúdez Mazuera\r\nPedro Pablo Bermúdez Medina" };
            layout.Controls.Add(credits);
            layout.Controls.Add(new Label { AutoSize = true, Dock = DockStyle.Fill, MaximumSize = new Size(660, 0),
                Text = T("About.Origin", "Unofficial fork of pGina. Credit and copyright remain with the pGina Team and contributors. No affiliation, endorsement or sponsorship by the original project.") });
            var tabs = new TabControl { Dock = DockStyle.Fill, AccessibleName = T("About.License", "BSD-3-Clause license (original legal text)") };
            foreach (var entry in new[] { new[] { "License", "LICENSE", T("About.License", "BSD-3-Clause license (original legal text)") },
                new[] { "Notice", "NOTICE", T("About.Notice", "Notices and attribution") } })
            {
                var page = new TabPage(entry[2]);
                string content;
                using (var stream = assembly.GetManifestResourceStream("OpenCredential.Configuration.Legal." + entry[1]))
                using (var reader = new StreamReader(stream)) content = reader.ReadToEnd();
                page.Controls.Add(new TextBox { Name = "about" + entry[0], Text = content, Multiline = true, ReadOnly = true,
                    Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, WordWrap = false, AccessibleName = entry[2],
                    BackColor = SystemColors.Window, ForeColor = SystemColors.WindowText });
                tabs.TabPages.Add(page);
            }
            layout.Controls.Add(tabs);
            var footer = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var close = new Button { Text = T("About.Close", "Close"), AutoSize = true, DialogResult = DialogResult.Cancel };
            close.AccessibleName = close.Text;
            footer.Controls.Add(close);
            // A copyable URL avoids launching a browser unexpectedly from an elevated configuration process.
            footer.Controls.Add(new TextBox { ReadOnly = true, Width = 340, Text = "https://github.com/pedropablobm/OpenCredential",
                AccessibleName = "GitHub", BorderStyle = BorderStyle.FixedSingle });
            layout.Controls.Add(footer);
            Controls.Add(layout);
            CancelButton = close;
        }
    }
}
