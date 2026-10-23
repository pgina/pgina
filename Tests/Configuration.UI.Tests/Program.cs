using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using OpenCredential.Shared.Localization;

internal static class Program
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            // Surface UI exceptions to the test runner instead of waiting on an invisible error dialog.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string assemblyPath = Path.GetFullPath(args[0]);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string candidate = Path.Combine(Path.GetDirectoryName(assemblyPath), new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
            };
            Type type = Assembly.LoadFrom(assemblyPath).GetType("OpenCredential.Configuration.ConfigurationUI", true);
            // Never invoke the public constructor: it verifies HKLM and initializes real settings.
            ConstructorInfo constructor = type.GetConstructor(PrivateInstance, null, new[] { typeof(bool) }, null);
            Check(constructor != null, "Isolated presentation constructor is required.");
            Console.WriteLine("TRACE Configuration UI: constructing isolated main form.");
            using (Form form = (Form)constructor.Invoke(new object[] { false }))
            {
                Check(!((System.Timers.Timer)type.GetField("m_serviceTimer", PrivateInstance).GetValue(form)).Enabled,
                    "Presentation preview must not start the service timer.");
                Check(type.GetField("m_openCredentialServiceController", PrivateInstance).GetValue(form) == null,
                    "Presentation preview must not open a service controller.");
                foreach (string action in new[] { "btnApply", "btnSaveAndClose", "serviceStartBtn", "serviceStopBtn", "cpRegisterBtn", "cpEnableDisableBtn", "btnSimGo" })
                    Check(!Find<Button>(form, action).Enabled, "Stateful actions must be disabled in isolated preview: " + action);
                Find<RadioButton>(form, "m_radioCredUI").Checked = true;
                Check(!Find<Button>(form, "btnLaunchCredUI").Enabled, "Changing preview mode must not enable a real credential prompt.");
                Find<RadioButton>(form, "m_radioEmulate").Checked = true;
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-30000, -30000);
                form.Show();
                var tabs = Find<TabControl>(form, "m_tabs");
                Check(form.AcceptButton == null, "Enter must not implicitly save or authenticate.");
                Check(Find<TextBox>(form, "m_password").UseSystemPasswordChar, "Input password must remain masked.");
                Check(Find<TextBox>(form, "m_passwordResult").UseSystemPasswordChar, "Result password must remain masked.");

                foreach (Size size in new[] { new Size(860, 580), new Size(1200, 900) })
                {
                    Console.WriteLine("TRACE Configuration UI: layout at " + size + ".");
                    form.Size = size;
                    foreach (string name in new[] { "importLanguageButton", "saveLanguageButton", "aboutLink" })
                    {
                        var action = Descendants(form).Single(c => c.Name == name);
                        var position = form.PointToClient(action.PointToScreen(Point.Empty));
                        Check(form.ClientRectangle.Contains(new Rectangle(position, action.Size)), "Language/About action must fit inside the window: " + name);
                    }
                    foreach (TabPage page in tabs.TabPages)
                    {
                        tabs.SelectedTab = page;
                        form.PerformLayout();
                        Application.DoEvents();
                        CheckFooter(form);
                        foreach (Control control in Descendants(page))
                        {
                            if (control is TextBox || control is DataGridView || control is ListView || control is Button)
                            {
                                Check(!String.IsNullOrWhiteSpace(control.AccessibleName) && !control.AccessibleName.StartsWith("Input."), "Missing accessible name/resource: " + control.Name);
                                Check(control.AccessibilityObject.Name == control.AccessibleName, "Accessible object must expose its configured name: " + control.Name);
                            }
                            var grid = control as DataGridView;
                            if (grid != null) Check(grid.StandardTab, "Tab must leave grid: " + grid.Name);
                            var button = control as Button;
                            if (button != null && button.Visible)
                                Check(button.Text != "button2", "Placeholder button text must not be exposed.");
                        }
                        if (page.Name == "m_generalConfigTab" || page.Name == "m_simTab")
                        {
                            var general = Find<TableLayoutPanel>(form, page.Name == "m_generalConfigTab" ? "generalLayout" : "simulationLayout");
                            var groups = general.Controls.Cast<Control>().ToArray();
                            for (int i = 0; i < groups.Length; i++)
                                for (int j = i + 1; j < groups.Length; j++)
                                    Check(!groups[i].Bounds.IntersectsWith(groups[j].Bounds), "Configuration sections must not overlap.");
                        }
                        if (page.Name == "m_pluginOrderTab" || page.Name == "m_pluginConfigTab")
                        {
                            var lastAction = Find<Button>(page, page.Name == "m_pluginOrderTab" ? "eventBtnDown" : "configureButton");
                            page.ScrollControlIntoView(lastAction);
                            Application.DoEvents();
                            Point actionPosition = page.PointToClient(lastAction.PointToScreen(Point.Empty));
                            Check(page.ClientRectangle.Contains(new Rectangle(actionPosition, lastAction.Size)), "Last list action must be reachable by scrolling: " + lastAction.Name);
                        }
                        string lastName = page.Name == "m_generalConfigTab" ? "tbLogonProgressMessage" :
                            page.Name == "m_simTab" ? "viewLogBtn" : page.Name == "cpOptions" ? "textBox1" : null;
                        if (lastName != null)
                        {
                            Control last = Descendants(page).Single(c => c.Name == lastName);
                            page.ScrollControlIntoView(last);
                            Application.DoEvents();
                            Point lastPosition = page.PointToClient(last.PointToScreen(Point.Empty));
                            Check(page.ClientRectangle.Contains(new Rectangle(lastPosition, last.Size)), "Last configuration field must be reachable: " + lastName + " at " + lastPosition + " size " + last.Size + " viewport " + page.ClientSize + " scroll " + page.AutoScrollPosition);
                        }
                        if (args.Length > 1 && page.Name != "ginaOptions")
                        {
                            Directory.CreateDirectory(args[1]);
                            using (var bitmap = new Bitmap(form.Width, form.Height))
                            {
                                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                                bitmap.Save(Path.Combine(args[1], page.Name + "-" + size.Width + ".png"));
                            }
                        }
                    }
                }

                // Exercise the actual existing row-movement handlers with synthetic data only.
                Console.WriteLine("TRACE Configuration UI: ordering, keyboard and status checks.");
                var order = Find<DataGridView>(form, "authenticateDGV");
                type.GetMethod("InitPluginOrderDGV", PrivateInstance).Invoke(form, new object[] { order });
                order.Rows.Add("synthetic-a", "First plugin");
                order.Rows.Add("synthetic-b", "Second plugin");
                type.GetMethod("MoveUp", PrivateInstance).Invoke(form, new object[] { order, 1 });
                Check((string)order.Rows[0].Cells[0].Value == "synthetic-b", "Move up must preserve row data and order.");
                type.GetMethod("MoveDown", PrivateInstance).Invoke(form, new object[] { order, 0 });
                Check((string)order.Rows[0].Cells[0].Value == "synthetic-a", "Move down must preserve row data and order.");
                tabs.SelectedTab = Find<TabPage>(form, "m_pluginOrderTab");
                order.Focus();
                order.GetType().GetMethod("ProcessDialogKey", PrivateInstance).Invoke(order, new object[] { Keys.Tab });
                Check(!order.ContainsFocus, "Keyboard Tab must leave the actual order grid.");

                tabs.SelectedTab = Find<TabPage>(form, "m_simTab");
                var usernameLabel = Find<Label>(form, "label4");
                typeof(Label).GetMethod("ProcessMnemonic", PrivateInstance).Invoke(usernameLabel, new object[] { 'u' });
                Check(Find<TextBox>(form, "m_username").Focused, "Username mnemonic must focus the input.");
                var passwordLabel = Find<Label>(form, "label5");
                typeof(Label).GetMethod("ProcessMnemonic", PrivateInstance).Invoke(passwordLabel, new object[] { 'p' });
                Check(Find<TextBox>(form, "m_password").Focused, "Password mnemonic must focus the input.");
                var revealPassword = Find<CheckBox>(form, "showTextResultPasswordCB");
                revealPassword.Checked = true;
                Check(!Find<TextBox>(form, "m_passwordResult").UseSystemPasswordChar, "Explicit reveal must still work.");
                revealPassword.Checked = false;
                Check(Find<TextBox>(form, "m_passwordResult").UseSystemPasswordChar, "Mask must be restored after reveal.");

                var picture = FindField<PictureBox>(form, "simAuthResultPB");
                type.GetMethod("SetStageStatus", PrivateInstance).Invoke(form, new object[] { picture, true });
                Check(Find<Label>(form, "label6").Text == "Authentication: Success", "Success needs visible text.");
                type.GetMethod("SetStageStatus", PrivateInstance).Invoke(form, new object[] { picture, false });
                Check(Find<Label>(form, "label6").Text == "Authentication: Failure", "Failure needs visible text.");
                type.GetMethod("ResetStageStatus", PrivateInstance).Invoke(form, null);
                Check(Find<Label>(form, "label6").Text == "Authentication: Not run", "Reset must clear the textual status.");
                tabs.SelectedIndex = 0;
                form.Scale(new SizeF(1.5f, 1.5f));
                form.PerformLayout();
                Application.DoEvents();
                CheckFooter(form);
                Check(Find<TextBox>(form, "textBox1").ForeColor == SystemColors.WindowText, "Warning must use Windows contrast colors.");
                form.Close();
            }
            Console.WriteLine("TRACE Configuration UI: constructing isolated offline-status form.");
            TestOfflineStatusUi(assemblyPath);
            TestLocalization(assemblyPath, constructor);
            TestConfigurationAccessPolicy(assemblyPath, constructor);
            Console.WriteLine("PASS Configuration UI: isolated construction, accessible names, keyboard exit from grids, masked passwords, responsive sections/footer, synthetic ordering and textual results; no settings/service/authentication actions.");
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }

    private static void CheckFooter(Form form)
    {
        foreach (string name in new[] { "btnSaveAndClose", "btnApply", "btnOkay" })
        {
            var button = Find<Button>(form, name);
            Check(button.Visible && button.Parent.ClientRectangle.Contains(button.Bounds), "Footer action clipped: " + name);
            Point position = form.PointToClient(button.PointToScreen(Point.Empty));
            Check(form.ClientRectangle.Contains(new Rectangle(position, button.Size)), "Footer action outside window: " + name + " at " + position + " size " + button.Size + " client " + form.ClientSize + " footer " + button.Parent.Parent.Bounds);
        }
    }

    private static void TestConfigurationAccessPolicy(string assemblyPath, ConstructorInfo constructor)
    {
        Type policy = Assembly.LoadFrom(assemblyPath).GetType("OpenCredential.Configuration.ConfigurationAccessPolicy", true);
        var authorize = policy.GetMethod("Authorize", BindingFlags.Static | BindingFlags.NonPublic);
        int reads = 0, prompts = 0;
        Func<object> read = () => { reads++; return null; };
        Func<bool> authenticate = () => { prompts++; return true; };
        Check(!(bool)authorize.Invoke(null, new object[] { false, read, authenticate }) && reads == 0 && prompts == 0,
            "Non-administrators must remain denied even with the default policy.");
        foreach (object value in new object[] { null, false, "False", 0 })
        {
            read = () => value;
            Check((bool)authorize.Invoke(null, new object[] { true, read, authenticate }) && prompts == 0,
                "Missing/disabled policy must not ask for an additional password.");
        }
        foreach (object value in new object[] { true, "True", 1 })
        {
            read = () => value;
            int before = prompts;
            Check((bool)authorize.Invoke(null, new object[] { true, read, authenticate }) && prompts == before + 1,
                "Enabled policy must require reauthentication.");
            Check(!(bool)authorize.Invoke(null, new object[] { true, read, (Func<bool>)(() => false) }),
                "Cancelled/failed reauthentication must prevent access.");
        }
        foreach (object value in new object[] { "invalid", "", 2, new byte[] { 0 } })
        {
            int before = prompts;
            bool rejected = false;
            try { authorize.Invoke(null, new object[] { true, (Func<object>)(() => value), authenticate }); }
            catch (TargetInvocationException error) { rejected = error.InnerException is InvalidDataException; }
            Check(rejected && prompts == before, "Malformed policy must not silently authorize or prompt.");
        }
        bool unreadableRejected = false;
        try { authorize.Invoke(null, new object[] { true, (Func<object>)(() => { throw new UnauthorizedAccessException(); }), authenticate }); }
        catch (TargetInvocationException error) { unreadableRejected = error.InnerException is UnauthorizedAccessException; }
        Check(unreadableRejected, "Unreadable policy must fail closed at startup.");
        using (Form form = (Form)constructor.Invoke(new object[] { false }))
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            var checkbox = Find<CheckBox>(form, "requireConfigurationReauthenticationCB");
            var signInLanguage = Find<ComboBox>(form, "signInLanguageCB");
            Check(signInLanguage.SelectedIndex == 0 && signInLanguage.Items.Count == 3 && Convert.ToString(signInLanguage.Items[2]) == "Español", "Native machine language selector must default to automatic and offer English/Spanish.");
            Check(!checkbox.Checked && !String.IsNullOrWhiteSpace(checkbox.AccessibleName), "Additional password option must default to unchecked and be accessible.");
            var page = Find<TabPage>(form, "m_generalConfigTab");
            Find<TabControl>(form, "m_tabs").SelectedTab = page;
            form.Size = new Size(860, 580);
            page.ScrollControlIntoView(checkbox);
            form.PerformLayout(); Application.DoEvents();
            var position = page.PointToClient(checkbox.PointToScreen(Point.Empty));
            Check(page.ClientRectangle.Contains(new Rectangle(position, checkbox.Size)), "Access option must be reachable by scrolling.");
            checkbox.Checked = true;
            Check(!Find<Button>(form, "btnApply").Enabled, "Changing the preview option must not enable real settings writes.");
            form.Close();
        }
        Console.WriteLine("PASS Configuration access: administrator requirement retained, reauthentication off by default, opt-in/cancel/error policy and isolated UI; no real registry/authentication calls.");
    }

    private static void TestLocalization(string assemblyPath, ConstructorInfo constructor)
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "opencode", "oc-language-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var english = UiLanguage.Current;
        try
        {
            var spanish = LanguageCatalog.Load(directory, "es-CO");
            Check(spanish.Get("Configuration", "Status.Success", "Success") == "Correcto", "Regional language must fall back to built-in Spanish.");
            Check(spanish.Get("Configuration", "Missing.Key", "English fallback") == "English fallback", "Missing keys must retain their English text.");
            foreach (string invalid in new[] {
                "<!DOCTYPE language [<!ENTITY x SYSTEM 'file:///does-not-exist'>]><language schemaVersion='1' culture='es' nativeName='Español'><module id='Configuration'><text key='x'>&x;</text></module></language>",
                "<language schemaVersion='99' culture='es' nativeName='Español'/>",
                "<language schemaVersion='1' culture='es' nativeName='Español'><module id='Configuration'><text key='x'>a</text><text key='x'>b</text></module></language>",
                "<language schemaVersion='1' culture='../bad' nativeName='Bad'/>",
                "<language schemaVersion='1' culture='es' nativeName='Español'><module id='Configuration'><text key='x'>{0,2147483647}</text></module></language>",
                "<language schemaVersion='1' culture='es' nativeName='Español'><module id='Configuration'><text key='x'>{broken}</text></module></language>" })
            {
                bool rejected = false;
                try { using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalid))) LanguageCatalog.Read(stream); }
                catch (Exception) { rejected = true; }
                Check(rejected, "Invalid/unsafe XML must be rejected.");
            }
            string source = Path.Combine(directory, "synthetic.xml");
            File.WriteAllText(source, "<language schemaVersion='1' culture='es-CO' nativeName='Español colombiano'><module id='Configuration'><text key='Status.Success'>Éxito</text><text key='Status.Failure'>Failure</text><text key='Broken.Format'>Sin marcador</text><text key='Broken.Substitution'>Acceso</text></module></language>", Encoding.UTF8);
            string installed = Path.Combine(directory, "installed");
            LanguageCatalog.Import(source, installed);
            var regional = LanguageCatalog.Load(installed, "es-CO");
            Check(regional.Get("Configuration", "Status.Success", "Success") == "Éxito", "Imported regional text must take priority.");
            Check(regional.Get("Configuration", "Status.Failure", "Failure") == "Failure", "An intentional English-valued override must not be treated as a missing key.");
            Check(regional.Get("Configuration", "Action.Apply", "Apply changes") == "&Aplicar cambios", "Partial imported package must fall back to Spanish.");
            Check(regional.Get("Configuration", "Broken.Format", "Value {0}") == "Value {0}", "Changed format tokens must fall back safely.");
            Check(regional.Get("Configuration", "Broken.Substitution", "Logging on %u") == "Logging on %u", "Application substitutions must be preserved.");
            string previous = File.ReadAllText(Path.Combine(installed, "es-CO.xml"));
            bool overwriteRejected = false;
            try { LanguageCatalog.Import(source, installed); } catch (IOException) { overwriteRejected = true; }
            Check(overwriteRejected && File.ReadAllText(Path.Combine(installed, "es-CO.xml")) == previous, "Import must not overwrite an existing translation.");
            File.WriteAllText(source, "<broken>");
            bool badImportRejected = false;
            try { LanguageCatalog.Import(source, installed); } catch (Exception) { badImportRejected = true; }
            Check(badImportRejected && File.ReadAllText(Path.Combine(installed, "es-CO.xml")) == previous && !Directory.GetFiles(installed, "*.tmp").Any(), "Malformed import must preserve previous packages and leave no temporary file.");
            File.WriteAllText(Path.Combine(installed, "es.xml"), "<broken>");
            Check(LanguageCatalog.Load(installed, "es").Get("Configuration", "Status.Success", "Success") == "Correcto", "Damaged external Spanish package must retain built-in Spanish.");
            File.WriteAllText(Path.Combine(installed, "es.xml"), "<language schemaVersion='99' culture='es' nativeName='Español'/>");
            Check(LanguageCatalog.Load(installed, "es").Get("Configuration", "Status.Success", "Success") == "Correcto", "Unsupported external schema must retain built-in Spanish.");
            File.WriteAllText(source, new string('x', 524289));
            bool largeRejected = false;
            try { LanguageCatalog.ReadFile(source); } catch (InvalidDataException) { largeRejected = true; }
            Check(largeRejected, "Oversize packages must be rejected before parsing.");
            Check(LanguageCatalog.Available(installed).Any(c => c.Culture == "es-CO"), "Imported language must be discoverable.");
            UiLanguage.Current = spanish;
            using (Form form = (Form)constructor.Invoke(new object[] { false }))
            {
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-30000, -30000);
                form.Show();
                Check(Find<Button>(form, "btnApply").Text == "&Aplicar cambios", "Main actions must be translated.");
                Check(Find<TabPage>(form, "m_pluginConfigTab").Text == "Selección de complementos", "Designer captions must be translated without editing generated code.");
                Check(Find<TextBox>(form, "m_password").AccessibleName == "Contraseña de la simulación", "Accessible names must be translated.");
                Check(!Find<Button>(form, "importLanguageButton").Enabled && !Find<Button>(form, "saveLanguageButton").Enabled,
                    "Preview must not import languages or write preferences.");
                Find<TextBox>(form, "m_username").Text = "untranslated-user";
                foreach (Size size in new[] { new Size(860, 580), new Size(1200, 900) })
                {
                    form.Size = size;
                    foreach (TabPage page in Find<TabControl>(form, "m_tabs").TabPages)
                    {
                        Find<TabControl>(form, "m_tabs").SelectedTab = page;
                        form.PerformLayout(); Application.DoEvents(); CheckFooter(form);
                    }
                }
                Check(Find<TextBox>(form, "m_username").Text == "untranslated-user", "Localization must not alter user data.");
                var passwordLabel = Find<Label>(form, "label5");
                Find<TabControl>(form, "m_tabs").SelectedTab = Find<TabPage>(form, "m_simTab");
                typeof(Label).GetMethod("ProcessMnemonic", PrivateInstance).Invoke(passwordLabel, new object[] { 'c' });
                Check(Find<TextBox>(form, "m_password").Focused, "Spanish password mnemonic must focus the input.");
                form.Close();
            }
            Type aboutType = Assembly.LoadFrom(assemblyPath).GetType("OpenCredential.Configuration.AboutDialog", true);
            using (Form about = (Form)Activator.CreateInstance(aboutType, true))
            {
                about.ShowInTaskbar = false;
                about.StartPosition = FormStartPosition.Manual;
                about.Location = new Point(-30000, -30000);
                about.Show();
                Check(Find<TextBox>(about, "aboutCredits").Text.Contains("Yuri Mercedes Bermúdez Mazuera") &&
                    Find<TextBox>(about, "aboutCredits").Text.Contains("Pedro Pablo Bermúdez Medina"), "About must credit both named developers.");
                string root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyPath), "..", "..", ".."));
                Check(Find<TextBox>(about, "aboutLicense").Text == File.ReadAllText(Path.Combine(root, "LICENSE")), "Offline legal text must match LICENSE exactly.");
                Check(Find<TextBox>(about, "aboutNotice").Text == File.ReadAllText(Path.Combine(root, "NOTICE")), "Offline attribution must match NOTICE exactly.");
                Check(about.Text == "Acerca de OpenCredential", "About dialog must use the selected language.");
                about.Close();
            }
            Type logType = Assembly.LoadFrom(assemblyPath).GetType("OpenCredential.Configuration.LogViewWindow", true);
            using (Form log = (Form)Activator.CreateInstance(logType))
            {
                Check(log.Text == "Registro de OpenCredential" && Find<Button>(log, "saveBtn").Text == "Guardar", "Log viewer chrome must be translated without altering technical content.");
            }
            TestDatabasePluginLanguages(assemblyPath, directory);
            TestOtherPluginLanguages(assemblyPath, directory);
        }
        finally { UiLanguage.Current = english; }
        Console.WriteLine("PASS Localization: Spanish/regional fallback, safe XML, atomic import, preserved placeholders/data, translated preview and offline legal credits.");
    }

    private static void TestOfflineStatusUi(string configurationPath)
    {
        string pluginPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(configurationPath), "..", "..", "..", "Plugins", "Core", "bin", "OpenCredential.Plugin.DatabaseAuth.dll"));
        Type type = Assembly.LoadFrom(pluginPath).GetType("OpenCredential.Plugin.DatabaseAuth.Configuration", true);
        ConstructorInfo constructor = type.GetConstructor(PrivateInstance, null, new[] { typeof(bool) }, null);
        Check(constructor != null, "Database Auth needs an isolated presentation constructor.");
        using (Form form = (Form)constructor.Invoke(new object[] { false }))
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
            var tabs = Find<TabControl>(form, "tabControl1");
            tabs.SelectedTab = Find<TabPage>(form, "offlineStatusTab");
            form.PerformLayout();
            Application.DoEvents();
            var expiry = Find<NumericUpDown>(form, "offlineMaxAgeHours");
            Check(expiry.Minimum == 1 && expiry.Maximum == 168 && expiry.Value == 24, "Offline age bounds and default must match the policy.");
            var status = Find<TextBox>(form, "offlineStatusText");
            Check(status.ReadOnly && !String.IsNullOrWhiteSpace(status.AccessibleName), "Cache status is named and read-only.");
            foreach (string name in new[] { "synchronizeOfflineButton", "refreshOfflineButton", "saveButton" })
                Check(!Find<Button>(form, name).Enabled, "Isolated plugin preview must not sync, inspect real data or save settings.");
            foreach (string name in new[] { "synchronizeOfflineButton", "refreshOfflineButton" })
            {
                var button = Find<Button>(form, name);
                Point position = form.PointToClient(button.PointToScreen(Point.Empty));
                Check(form.ClientRectangle.Contains(new Rectangle(position, button.Size)), "Offline action must fit inside the plugin window.");
            }
            form.Close();
        }
        Console.WriteLine("PASS Database Auth UI: offline-status tab, accessible names, age policy and isolated preview; no real cache or database accessed.");
    }

    private static void TestDatabasePluginLanguages(string configurationPath, string languageDirectory)
    {
        var previous = UiLanguage.Current;
        try
        {
            foreach (string culture in new[] { "en", "es" })
            foreach (string module in new[] { "DatabaseAuth", "DatabaseLogger" })
            {
                UiLanguage.Current = LanguageCatalog.Load(languageDirectory, culture);
                string pluginPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(configurationPath), "..", "..", "..", "Plugins", "Core", "bin", "OpenCredential.Plugin." + module + ".dll"));
                var type = Assembly.LoadFrom(pluginPath).GetType("OpenCredential.Plugin." + module + ".Configuration", true);
                var constructor = type.GetConstructor(PrivateInstance, null, new[] { typeof(bool) }, null);
                Check(constructor != null, "Plugin localization tests require an isolated constructor: " + module);
                using (Form form = (Form)constructor.Invoke(new object[] { false }))
                {
                    form.ShowInTaskbar = false;
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(-30000, -30000);
                    form.Show();
                    bool auth = module == "DatabaseAuth";
                    Check(form.FormBorderStyle == FormBorderStyle.Sizable, "Translated database dialogs must allow resizing.");
                    var password = Find<TextBox>(form, auth ? "passwordTB" : "passwdTB");
                    Check(password.UseSystemPasswordChar, "Localized plugin password must remain masked.");
                    Find<TextBox>(form, "hostTB").Text = "synthetic-host";
                    Find<TextBox>(form, "userTB").Text = "synthetic-user";
                    Find<TextBox>(form, "dbTB").Text = "synthetic_database";
                    var provider = Find<ComboBox>(form, "providerCB");
                    provider.SelectedItem = "PostgreSql";
                    Check(provider.Text == "PostgreSql", "Provider values must retain their technical identity.");
                    var tls = Find<ComboBox>(form, "sslModeCB");
                    tls.SelectedItem = "VerifyFull";
                    Check(tls.Text == "VerifyFull", "TLS option values must not be translated.");
                    Check(Find<TextBox>(form, "hostTB").Text == "synthetic-host" && Find<TextBox>(form, "dbTB").Text == "synthetic_database", "Provider refresh must not change user-entered settings.");
                    foreach (var button in Descendants(form).OfType<Button>())
                        Check(!button.Enabled, "Synthetic plugin must not write settings, query SQL or create tables: " + button.Name);
                    if (culture == "es")
                    {
                        Check(form.Text.Contains(auth ? "Autenticación" : "Registro"), "Plugin title must be translated.");
                        Check(Find<Button>(form, auth ? "saveButton" : "okBtn").Text == (auth ? "Guardar y cerrar" : "Guardar"), "Save action must use Spanish.");
                        Check(Find<GroupBox>(form, auth ? "groupBox1" : "groupBox2").Text == (auth ? "Servidor de base de datos" : "Servidor"), "Provider changes must not restore English captions.");
                        if (auth)
                        {
                            Check(Find<TabPage>(form, "offlineStatusTab").Text == "Disponibilidad sin conexión", "Offline tab must be translated.");
                            Check(Find<TextBox>(form, "offlineStatusText").Text.StartsWith("Seleccione"), "Offline preview must not read a real cache.");
                            var errorMethod = type.GetMethod("DatabaseErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
                            string errorText = (string)errorMethod.Invoke(null, new object[] { new InvalidOperationException("synthetic-sensitive-diagnostic") });
                            Check(errorText.StartsWith("Falló") && !errorText.Contains("synthetic-sensitive-diagnostic"), "Provider exception payloads must not leak into translated UI errors.");
                            var statusType = type.Assembly.GetType("OpenCredential.Plugin.DatabaseAuth.OfflineCacheStatus", true);
                            var status = Activator.CreateInstance(statusType, true);
                            statusType.GetProperty("Exists").SetValue(status, true, null);
                            statusType.GetProperty("LastFullSyncUtc").SetValue(status, (DateTime?)DateTime.UtcNow, null);
                            string summary = (string)type.GetMethod("OfflineDisplay", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { status });
                            Check(summary.StartsWith("Copia local: disponible") && summary.Contains("UTC"), "Offline summary must be translated while preserving explicit UTC.");
                            CheckLocalizedRule(type, form, "DatabaseAuth", "listBoxAuthzRules");
                        }
                        else
                        {
                            var diagnostic = type.GetMethod("DiagnosticResult", BindingFlags.Static | BindingFlags.NonPublic);
                            Check((string)diagnostic.Invoke(null, new object[] { "Table created." }) == "Tabla creada.", "Logger result summary must be translated.");
                            string failure = (string)diagnostic.Invoke(null, new object[] { "Error: synthetic-sensitive-provider-detail" });
                            Check(failure.StartsWith("Falló") && !failure.Contains("synthetic-sensitive-provider-detail"), "Logger engine errors must not leak provider details.");
                        }
                    }
                    foreach (var size in new[] { new Size(700, 560), new Size(1000, 820) })
                    {
                        form.Size = size;
                        form.PerformLayout(); Application.DoEvents();
                        foreach (string action in auth ? new[] { "testBtn", "createTableBtn", "saveButton", "cancelButton" } : new[] { "testButton", "createTableBtn", "okBtn", "cancelBtn" })
                        {
                            var button = Find<Button>(form, action);
                            var position = form.PointToClient(button.PointToScreen(Point.Empty));
                            Check(form.ClientRectangle.Contains(new Rectangle(position, button.Size)), "Plugin footer must remain in the window: " + action);
                        }
                        CheckPluginFields(form, module);
                        if (auth)
                        foreach (TabPage page in Find<TabControl>(form, "tabControl1").TabPages)
                        {
                            Find<TabControl>(form, "tabControl1").SelectedTab = page;
                            form.PerformLayout(); Application.DoEvents();
                            CheckPluginFields(form, module);
                            if (page.Name == "tabPage4")
                            foreach (TabPage schemaPage in Find<TabControl>(form, "tabControlDBSchema").TabPages)
                            {
                                Find<TabControl>(form, "tabControlDBSchema").SelectedTab = schemaPage;
                                form.PerformLayout(); Application.DoEvents();
                                CheckPluginFields(form, module);
                            }
                        }
                    }
                    if (auth)
                    {
                        var tabs = Find<TabControl>(form, "tabControl1");
                        tabs.SelectedTab = Find<TabPage>(form, "tabPage4");
                        var schema = Find<TabControl>(form, "tabControlDBSchema");
                        schema.SelectedTab = Find<TabPage>(form, "tabPage1");
                        var last = Find<TextBox>(form, "lastAttemptColTB");
                        schema.SelectedTab.ScrollControlIntoView(last);
                        form.PerformLayout(); Application.DoEvents();
                        Point position = schema.SelectedTab.PointToClient(last.PointToScreen(Point.Empty));
                        Check(schema.SelectedTab.ClientRectangle.Contains(new Rectangle(position, last.Size)), "Last localized schema field must be reachable by scrolling.");
                    }
                    form.Close();
                }
            }
        }
        finally { UiLanguage.Current = previous; }
        Console.WriteLine("PASS Database plugin localization: English/Spanish, responsive fields/footer, isolated previews, masked passwords, stable provider/TLS values and no SQL/settings/cache access.");
    }

    private static void CheckPluginFields(Form form, string module)
    {
        foreach (var table in Descendants(form).OfType<TableLayoutPanel>())
        {
            var children = table.Controls.Cast<Control>().Where(c => c.Visible).ToArray();
            for (int i = 0; i < children.Length; i++)
            for (int j = i + 1; j < children.Length; j++)
                Check(!children[i].Bounds.IntersectsWith(children[j].Bounds), "Localized fields must not overlap: " + module + "/" + children[i].Name + "/" + children[j].Name);
        }
    }

    private static void TestOtherPluginLanguages(string configurationPath, string languageDirectory)
    {
        var previous = UiLanguage.Current;
        try
        {
            foreach (string culture in new[] { "en", "es" })
            foreach (string module in new[] { "SessionLimit", "SingleUser", "Sample", "LocalMachine", "DriveMapper", "Ldap" })
            {
                UiLanguage.Current = LanguageCatalog.Load(languageDirectory, culture);
                string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(configurationPath), "..", "..", "..", "Plugins", "Core", "bin", "OpenCredential.Plugin." + module + ".dll"));
                var type = Assembly.LoadFrom(path).GetType("OpenCredential.Plugin." + module + (module == "DriveMapper" ? ".ConfigurationUi" : ".Configuration"), true);
                var constructor = type.GetConstructor(PrivateInstance, null, new[] { typeof(bool) }, null);
                Check(constructor != null, "Small plugin needs an isolated constructor: " + module);
                using (Form form = (Form)constructor.Invoke(new object[] { false }))
                {
                    form.ShowInTaskbar = false;
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(-30000, -30000);
                    form.Show();
                    if (culture == "es")
                    {
                        var titles = new Dictionary<string, string> { { "SessionLimit", "Límite de duración de sesión" }, { "SingleUser", "Acceso con usuario único" },
                            { "Sample", "Complemento de ejemplo" }, { "LocalMachine", "Cuentas locales de Windows" }, { "DriveMapper", "Asignación de unidades de red" }, { "Ldap", "Configuración LDAP" } };
                        Check(form.Text == titles[module], "Plugin title must be translated: " + module);
                    }
                    foreach (var button in Descendants(form).OfType<Button>()) Check(!button.Enabled, "Small plugin preview must not write configuration.");
                    if (module == "Sample") Check(type.GetField("m_settings", PrivateInstance).GetValue(form) == null, "Sample preview must not instantiate real registry settings.");
                    if (module == "SingleUser")
                    {
                        Check(Find<TextBox>(form, "m_txtPass").UseSystemPasswordChar, "SingleUser password must remain masked.");
                        var grid = Find<DataGridView>(form, "m_dgv");
                        grid.Rows.Add("synthetic-plugin-uuid");
                        Check(grid.Columns[0].Name == "PluginUuid" && (string)grid.Rows[0].Cells[0].Value == "synthetic-plugin-uuid", "Translation must preserve UUID column identity and row data.");
                        Check(grid.StandardTab, "Tab must leave the plugin UUID grid.");
                    }
                    if (module == "DriveMapper")
                    {
                        Check(Find<TextBox>(form, "m_passwordTb").UseSystemPasswordChar, "DriveMapper must mask passwords.");
                        var credentials = Find<ComboBox>(form, "m_credsCb");
                        Check(credentials.Items.Cast<string>().SequenceEqual(new[] { "Original", "Final", "Custom" }), "Credential enum values must remain stable.");
                        Check(Find<DataGridView>(form, "m_mapListDgv").Columns["UNC"].DataPropertyName == "UncPath", "Mapping bindings must retain their identifiers.");
                    }
                    if (module == "Ldap")
                    {
                        Check(Find<TextBox>(form, "searchPassTextBox").UseSystemPasswordChar || Find<TextBox>(form, "searchPassTextBox").PasswordChar != '\0', "LDAP password must remain masked.");
                        Check(Find<DataGridView>(form, "passwordAttributesDGV").Columns[0].Name == "Attribute Name", "LDAP attribute binding must retain its identity.");
                        if (culture == "es")
                        {
                            CheckLocalizedRule(type, form, "Ldap", "authzRulesListBox");
                            var encryption = Find<ComboBox>(form, "m_encryptionMethodCb");
                            Check((string)encryption.Items[0] == "No Encryption" && encryption.GetItemText(encryption.Items[0]) == "Sin cifrado", "LDAP formatting must translate display without replacing persisted option values.");
                        }
                    }
                    form.Size = form.MinimumSize;
                    form.PerformLayout(); Application.DoEvents();
                    CheckPluginFields(form, module);
                    if (module == "Ldap")
                    foreach (TabPage page in Find<TabControl>(form, "tabControl1").TabPages)
                    {
                        Find<TabControl>(form, "tabControl1").SelectedTab = page;
                        form.PerformLayout(); Application.DoEvents(); CheckPluginFields(form, module);
                    }
                    foreach (var button in Descendants(form).OfType<Button>().Where(b => b.Parent.Name == "pluginFooter"))
                    {
                        Point position = form.PointToClient(button.PointToScreen(Point.Empty));
                        Check(form.ClientRectangle.Contains(new Rectangle(position, button.Size)), "Plugin footer must fit at minimum size: " + module + "/" + button.Name);
                    }
                    form.Close();
                }
            }
        }
        finally { UiLanguage.Current = previous; }
        Console.WriteLine("PASS Remaining plugin localization: isolated English/Spanish forms, visible actions, masked passwords, stable identifiers and no real settings access.");
    }

    private static void CheckLocalizedRule(Type configurationType, Form form, string module, string listName)
    {
        string name = "OpenCredential.Plugin." + module + ".";
        Type condition = configurationType.Assembly.GetType(name + "GroupRule+Condition", true);
        Type ruleType = configurationType.Assembly.GetType(name + "GroupAuthzRule", true);
        object rule = Activator.CreateInstance(ruleType, new[] { (object)"synthetic-group", Enum.Parse(condition, "MEMBER_OF"), true });
        string serialized = (string)ruleType.GetMethod("ToRegString").Invoke(rule, null);
        var list = Find<ListBox>(form, listName);
        list.Items.Add(rule);
        Check(list.GetItemText(rule).StartsWith("Si pertenece al grupo") && list.GetItemText(rule).Contains("synthetic-group"), "Rules must translate presentation without altering group names.");
        Check(serialized == (string)ruleType.GetMethod("ToRegString").Invoke(rule, null) && ReferenceEquals(list.Items[list.Items.Count - 1], rule), "Rule formatting must preserve rule object and serialized policy.");
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control nested in Descendants(child)) yield return nested;
        }
    }

    private static T Find<T>(Control root, string name) where T : Control
    {
        return Descendants(root).OfType<T>().Single(c => c.Name == name);
    }

    private static T FindField<T>(Form form, string name)
    {
        return (T)form.GetType().GetField(name, PrivateInstance).GetValue(form);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
