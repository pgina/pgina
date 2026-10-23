using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenCredential.Shared.Localization;

namespace OpenCredential.Configuration
{
    internal static class MachineSignInLanguage
    {
        internal static string Read()
        {
            using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            using (var key = machine.OpenSubKey(OpenCredential.Shared.Settings.OpenCredentialDynamicSettings.OpenCredentialRoot))
                return key == null ? "auto" : Convert.ToString(key.GetValue("SignInLanguage", "auto", RegistryValueOptions.DoNotExpandEnvironmentNames));
        }

        internal static void Save(string language)
        {
            if (language != "auto" && language != "en" && language != "es") throw new ArgumentException("Unsupported sign-in language.");
            using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            using (var key = machine.CreateSubKey(OpenCredential.Shared.Settings.OpenCredentialDynamicSettings.OpenCredentialRoot))
                key.SetValue("SignInLanguage", language, RegistryValueKind.String);
        }
    }

    internal static class LanguagePreferences
    {
        internal const string RegistryPath = @"Software\OpenCredential\Configuration";
        internal static string DirectoryPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages"); } }

        internal static string Read()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    return key == null ? "auto" : Convert.ToString(key.GetValue("Language", "auto"));
            }
            catch (Exception e) when (e is System.Security.SecurityException || e is UnauthorizedAccessException || e is IOException) { return "auto"; }
        }

        internal static void Initialize()
        {
            string selected = Read();
            try
            {
                UiLanguage.Current = LanguageCatalog.Load(DirectoryPath, selected == "auto" ?
                    System.Globalization.CultureInfo.CurrentUICulture.Name : selected);
            }
            catch (ArgumentException) { UiLanguage.Current = LanguageCatalog.Load(DirectoryPath, "en"); }
        }
    }

    public partial class ConfigurationUI
    {
        private static string T(string key, string english) { return UiLanguage.Text("Configuration", key, english); }

        private Control LanguageHeader(Control heading)
        {
            var header = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 1, TabIndex = 0 };
            header.Controls.Add(heading);
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, TabIndex = 0 };
            actions.Controls.Add(new Label { AutoSize = true, Text = T("Language.Label", "Language"), Margin = new Padding(4, 8, 4, 4) });
            var choice = new ComboBox { Name = "languageSelector", DropDownStyle = ComboBoxStyle.DropDownList, Width = 180,
                AccessibleName = T("Language.Label", "Language"), TabIndex = 0 };
            var import = new Button { Name = "importLanguageButton", Text = T("Language.Import", "Import language…"), AutoSize = true, TabIndex = 1 };
            var apply = new Button { Name = "saveLanguageButton", Text = T("Language.Apply", "Save language"), AutoSize = true, TabIndex = 2 };
            var about = new LinkLabel { Name = "aboutLink", Text = T("About.Link", "About…"), AutoSize = true,
                Margin = new Padding(8), TabIndex = 3 };
            Action<string> populate = selected =>
            {
                choice.Items.Clear();
                choice.Items.Add(T("Language.Automatic", "Automatic (Windows)"));
                choice.Items.AddRange(LanguageCatalog.Available(LanguagePreferences.DirectoryPath).Cast<object>().ToArray());
                choice.SelectedIndex = 0;
                foreach (var item in choice.Items.OfType<LanguageCatalog>())
                    if (string.Equals(item.Culture, selected, StringComparison.OrdinalIgnoreCase)) choice.SelectedItem = item;
            };
            // Preview construction never reads preferences or installs/saves languages.
            populate(m_presentationOnly ? "en" : LanguagePreferences.Read());
            import.Enabled = apply.Enabled = !m_presentationOnly;
            import.Click += delegate
            {
                using (var dialog = new OpenFileDialog { Filter = "Language XML (*.xml)|*.xml", CheckFileExists = true, Multiselect = false })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        var installed = LanguageCatalog.Import(dialog.FileName, LanguagePreferences.DirectoryPath);
                        populate(installed.Culture);
                        MessageBox.Show(this, T("Language.Imported", "Language imported. Select and save it, then reopen configuration to apply it."));
                    }
                    catch (Exception ex)
                    {
                        m_logger.WarnFormat("Language import failed ({0}).", ex.GetType().Name);
                        MessageBox.Show(this, T("Language.Error", "Unable to complete the operation. Check package format, permissions and whether the language is already installed."), T("Error.Title", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };
            apply.Click += delegate
            {
                try
                {
                    var selected = choice.SelectedItem as LanguageCatalog;
                    using (var key = Registry.CurrentUser.CreateSubKey(LanguagePreferences.RegistryPath))
                        key.SetValue("Language", selected == null ? "auto" : selected.Culture);
                    MessageBox.Show(this, T("Language.Restart", "Language saved. Reopen configuration to apply it. Other pending changes have not been saved."));
                }
                catch (Exception ex)
                {
                    m_logger.WarnFormat("Language preference could not be saved ({0}).", ex.GetType().Name);
                    MessageBox.Show(this, T("Language.Error", "Unable to complete the operation. Check package format, permissions and whether the language is already installed."), T("Error.Title", "Error"));
                }
            };
            about.LinkClicked += delegate { using (var dialog = new AboutDialog()) dialog.ShowDialog(this); };
            actions.Controls.AddRange(new Control[] { choice, import, apply, about });
            header.Controls.Add(actions);
            return header;
        }

        private static void TranslateCaptions(Control parent)
        {
            // Explicit presentation types only: never translate a user's text, credentials or stored settings.
            foreach (Control control in parent.Controls)
            {
                if (control is Label || control is Button || control is CheckBox || control is RadioButton ||
                    control is GroupBox || control is TabPage)
                    control.Text = T("Control." + control.Name, control.Text);
                TranslateCaptions(control);
            }
        }
    }
}
