using System;
using System.Drawing;
using System.Windows.Forms;
using OpenCredential.Shared.Localization;

namespace OpenCredential.Plugin.Presentation
{
    // Linked into UI plugins; the shared text catalog remains independent of WinForms.
    internal static class PluginPresentation
    {
        internal static string Text(string module, string key, string english)
        {
            return UiLanguage.Text(module, key, english);
        }

        internal static void Apply(Control root, string module)
        {
            if (root is Form || root is Label || root is Button || root is CheckBox || root is RadioButton || root is GroupBox || root is TabPage)
            {
                root.Text = Text(module, root is Form ? "Title" : "Control." + root.Name, root.Text);
                root.AccessibleName = root.Text.Replace("&", "");
            }
            if (root is Label) root.AutoSize = true;
            foreach (Control child in root.Controls) Apply(child, module);
        }

        internal static FlowLayoutPanel Actions(params Control[] controls)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill, WrapContents = true };
            foreach (Control control in controls)
            {
                control.Dock = DockStyle.None;
                control.Anchor = AnchorStyles.None;
                control.Margin = new Padding(4);
                if (control is Button) { control.AutoSize = true; control.MinimumSize = new Size(90, 30); }
                panel.Controls.Add(control);
            }
            return panel;
        }

        internal static void DisablePreviewActions(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is Button) child.Enabled = false;
                DisablePreviewActions(child);
            }
        }

        internal static void Options(ComboBox control, string module)
        {
            // For controls persisted by index/original SelectedItem, never by parsing the displayed Text.
            control.FormattingEnabled = true;
            control.Format += delegate(object sender, ListControlConvertEventArgs args)
            {
                int index = control.Items.IndexOf(args.ListItem);
                args.Value = Text(module, "Option." + control.Name + "." + index, Convert.ToString(args.ListItem));
            };
        }

        internal static TableLayoutPanel Fields(params Control[] pairs)
        {
            var table = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, ColumnCount = 2, Padding = new Padding(6) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int index = 0; index < pairs.Length; index += 2)
            {
                int row = index / 2;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                Control label = pairs[index], field = pairs[index + 1];
                field.Margin = new Padding(4, 5, 4, 5);
                field.Dock = DockStyle.Fill;
                field.TabIndex = index + 1;
                if (label == null)
                {
                    if (field is Label) field.MaximumSize = new Size(420, 0);
                    table.Controls.Add(field, 0, row);
                    table.SetColumnSpan(field, 2);
                }
                else
                {
                    label.AutoSize = true;
                    label.MaximumSize = new Size(260, 0);
                    label.Anchor = AnchorStyles.Left;
                    label.Margin = new Padding(4, 5, 4, 5);
                    label.TabIndex = index;
                    table.Controls.Add(label, 0, row);
                    table.Controls.Add(field, 1, row);
                    if (string.IsNullOrEmpty(field.AccessibleName)) field.AccessibleName = label.Text.Replace("&", "");
                }
            }
            return table;
        }

        internal static void Stack(Control container, params Control[] controls)
        {
            container.SuspendLayout();
            container.Controls.Clear();
            var table = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, ColumnCount = 1, Padding = new Padding(4) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < controls.Length; i++)
            {
                var control = controls[i];
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(4);
                control.TabIndex = i;
                if (control is GroupBox) { control.AutoSize = true; ((GroupBox)control).AutoSizeMode = AutoSizeMode.GrowAndShrink; }
                table.Controls.Add(control, 0, i);
            }
            container.Controls.Add(table);
            if (container is TabPage) ((TabPage)container).AutoScroll = true;
            container.ResumeLayout(true);
        }

        internal static void Shell(Form form, Control body, params Control[] buttons)
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.Dock = DockStyle.Fill;
            table.Controls.Add(body, 0, 0);
            var footer = Actions(buttons);
            footer.Name = "pluginFooter";
            table.Controls.Add(footer, 0, 1);
            form.Controls.Add(table);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MinimumSize = new Size(700, 560);
            form.Size = new Size(780, 720);
        }
    }
}
