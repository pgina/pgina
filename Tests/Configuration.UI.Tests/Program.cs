using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

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
