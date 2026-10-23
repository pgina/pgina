using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenCredential.Configuration
{
    public partial class ConfigurationUI
    {
        private readonly ComponentResourceManager presentationResources = new ComponentResourceManager(typeof(ConfigurationUI));
        private readonly Dictionary<PictureBox, Label> stageText = new Dictionary<PictureBox, Label>();
        private CheckBox requireConfigurationReauthenticationCB;
        private ComboBox signInLanguageCB;
        private static readonly string[] signInLanguages = { "auto", "en", "es" };

        private string PresentationText(string key)
        {
            return T(key, presentationResources.GetString(key) ?? key);
        }

        // Keep runtime composition separate from the WinForms designer's serialization.
        private void InitializePresentation()
        {
            SuspendLayout();
            Font = SystemFonts.MessageBoxFont;
            MinimumSize = new Size(860, 580);
            AccessibleName = Text;
            AccessibleDescription = PresentationText("Shell.Description");
            Text = PresentationText("Shell.Heading");
            TranslateCaptions(this);
            SetText(btnSaveAndClose, "Action.SaveClose");
            SetText(btnApply, "Action.Apply");
            SetText(btnOkay, "Action.Close");
            btnSaveAndClose.UseMnemonic = true;
            // Enter must not save configuration or run a login unexpectedly.
            AcceptButton = null;
            CancelButton = null;

            var shell = CreateLayout("configurationLayout", 1);
            shell.Dock = DockStyle.Fill;
            shell.AutoSize = false;
            shell.Padding = new Padding(12);
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var heading = new Label { Name = "configurationHeading", AutoSize = true,
                Text = PresentationText("Shell.Heading"), Margin = new Padding(4, 4, 4, 12) };
            shell.Controls.Add(LanguageHeader(heading), 0, 0);
            m_tabs.Dock = DockStyle.Fill;
            m_tabs.Margin = new Padding(0);
            m_tabs.AccessibleName = PresentationText("Shell.Tabs");
            m_tabs.Multiline = true;
            shell.Controls.Add(m_tabs, 0, 1);
            var footer = CreateLayout("configurationFooter", 2);
            footer.ColumnStyles.Clear();
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            m_openCredentialVersionLbl.Text = "OpenCredential " + Application.ProductVersion;
            m_openCredentialVersionLbl.Anchor = AnchorStyles.Left;
            footer.Controls.Add(m_openCredentialVersionLbl, 0, 0);
            var actions = Actions(btnSaveAndClose, btnApply, btnOkay);
            actions.WrapContents = false;
            actions.Name = "configurationActions";
            actions.TabIndex = 1;
            footer.Controls.Add(actions, 1, 0);
            shell.Controls.Add(footer, 0, 2);
            Controls.Add(shell);

            BuildGeneralLayout();
            BuildPluginLayout();
            BuildOrderLayout();
            BuildSimulationLayout();
            BuildProviderLayout();
            NameInputs();
            ApplyAccessibleDefaults(this);
            var headingFont = new Font(Font.FontFamily, 12f, FontStyle.Bold);
            heading.Font = headingFont;
            heading.MaximumSize = Size.Empty;
            heading.Disposed += delegate { headingFont.Dispose(); };
            ResumeLayout(true);
        }

        private static TableLayoutPanel CreateLayout(string name, int columns)
        {
            var layout = new TableLayoutPanel { Name = name, ColumnCount = columns,
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, Margin = new Padding(0), Padding = new Padding(4), TabStop = false };
            for (int i = 0; i < columns; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            return layout;
        }

        private static FlowLayoutPanel Actions(params Button[] buttons)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, TabStop = false };
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                button.Anchor = AnchorStyles.None;
                button.AutoSize = true;
                button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button.MinimumSize = new Size(96, 32);
                button.Padding = new Padding(8, 3, 8, 3);
                button.Margin = new Padding(4);
                button.TabIndex = i;
                panel.Controls.Add(button);
            }
            return panel;
        }

        private static void Stack(GroupBox group, params Control[] controls)
        {
            group.SuspendLayout();
            group.Controls.Clear();
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            group.Dock = DockStyle.Fill;
            group.Padding = new Padding(10, 8, 10, 10);
            group.Margin = new Padding(6);
            var layout = CreateLayout(group.Name + "Layout", 1);
            for (int i = 0; i < controls.Length; i++)
            {
                Control control = controls[i];
                control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(4, 5, 4, 5);
                control.TabIndex = i;
                layout.Controls.Add(control, 0, i);
            }
            group.Controls.Add(layout);
            group.ResumeLayout(true);
        }

        private static TableLayoutPanel PageLayout(TabPage page, string name, int columns, int minimumWidth)
        {
            page.SuspendLayout();
            page.AutoScroll = true;
            page.AutoScrollMargin = new Size(12, 12);
            var layout = CreateLayout(name, columns);
            layout.MinimumSize = new Size(minimumWidth, 0);
            page.Controls.Add(layout);
            page.ResumeLayout(true);
            return layout;
        }

        private void BuildGeneralLayout()
        {
            SetText(groupBox12, "General.Image");
            SetText(motdGB, "General.Message");
            SetText(btnImageBrowse, "Action.Browse");
            m_tileImagePreview.SizeMode = PictureBoxSizeMode.Zoom;
            m_tileImagePreview.Size = new Size(96, 96);
            m_tileImagePreview.Anchor = AnchorStyles.Left;
            var imagePath = CreateLayout("imagePathLayout", 2);
            imagePath.ColumnStyles[1].SizeType = SizeType.AutoSize;
            imagePath.Controls.Add(m_tileImageTxt, 0, 0);
            imagePath.Controls.Add(Actions(btnImageBrowse), 1, 0);
            m_tileImageTxt.Dock = DockStyle.Fill;
            Stack(groupBox12, label10, imagePath, label11, m_tileImagePreview);
            Stack(motdGB, motdLabel, motdTB, label12);
            var serviceHelp = new Label { AutoSize = true, Text = PresentationText("General.ServiceHelp") };
            var providerHelp = new Label { AutoSize = true, Text = PresentationText("General.ProviderHelp") };
            Stack(servGB, logonUiShowServiceStatusCB, servStatusLabel, serviceStatusTB, Actions(serviceStartBtn, serviceStopBtn), serviceHelp);
            Stack(groupBox14, label13, cpRegisteredTB, cpEnabledLabel, cpEnabledTB, Actions(cpRegisterBtn, cpEnableDisableBtn), providerHelp);
            Stack(groupBox4, chk_originalUsernameUnlock);
            Stack(groupBox17, label19, tbLogonProgressMessage, label20);
            var layout = PageLayout(m_generalConfigTab, "generalLayout", 2, 700);
            layout.Controls.Add(servGB, 0, 0);
            layout.Controls.Add(groupBox14, 1, 0);
            layout.Controls.Add(groupBox12, 0, 1);
            layout.Controls.Add(motdGB, 1, 1);
            layout.Controls.Add(groupBox4, 0, 2);
            layout.SetColumnSpan(groupBox4, 2);
            layout.Controls.Add(groupBox17, 0, 3);
            layout.SetColumnSpan(groupBox17, 2);
            var accessGroup = new GroupBox { Name = "configurationAccessGroup", Text = PresentationText("Access.Title") };
            requireConfigurationReauthenticationCB = new CheckBox { Name = "requireConfigurationReauthenticationCB",
                Text = PresentationText("Access.RequirePassword"), AutoSize = true, Checked = false,
                AccessibleName = PresentationText("Access.RequirePassword") };
            var accessHelp = new Label { AutoSize = true, Text = PresentationText("Access.Help") };
            var signInLabel = new Label { AutoSize = true, Text = PresentationText("Language.SignIn") };
            signInLanguageCB = new ComboBox { Name = "signInLanguageCB", DropDownStyle = ComboBoxStyle.DropDownList,
                AccessibleName = PresentationText("Language.SignIn"), Width = 220 };
            signInLanguageCB.Items.AddRange(new object[] { PresentationText("Language.SignInAuto"), "English", "Español" });
            signInLanguageCB.SelectedIndex = 0;
            var signInHelp = new Label { AutoSize = true, Text = PresentationText("Language.SignInHelp") };
            Stack(accessGroup, requireConfigurationReauthenticationCB, accessHelp, signInLabel, signInLanguageCB, signInHelp);
            layout.Controls.Add(accessGroup, 0, 4);
            layout.SetColumnSpan(accessGroup, 2);
            accessGroup.TabIndex = 6;
            servGB.TabIndex = 0;
            groupBox14.TabIndex = 1;
            groupBox12.TabIndex = 2;
            motdGB.TabIndex = 3;
            groupBox4.TabIndex = 4;
            groupBox17.TabIndex = 5;
        }

        private static void ListGroup(GroupBox group, Control list, params Button[] buttons)
        {
            group.Controls.Clear();
            group.Dock = DockStyle.Fill;
            group.Margin = new Padding(6);
            group.Padding = new Padding(8);
            var layout = CreateLayout(group.Name + "Layout", 1);
            layout.Dock = DockStyle.Fill;
            layout.AutoSize = false;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            list.Dock = DockStyle.Fill;
            list.Margin = new Padding(4);
            list.TabIndex = 0;
            layout.Controls.Add(list, 0, 0);
            var actions = Actions(buttons);
            actions.TabIndex = 1;
            layout.Controls.Add(actions, 0, 1);
            group.Controls.Add(layout);
        }

        private void BuildPluginLayout()
        {
            SetText(configureButton, "Action.Configure");
            SetText(btnAdd, "Action.AddDirectory");
            SetText(btnRemove, "Action.RemoveDirectory");
            ListGroup(groupBox1, lstPluginDirs, btnAdd, btnRemove);
            ListGroup(pluginsGroupBox, pluginsDG, configureButton);
            var layout = PageLayout(m_pluginConfigTab, "pluginsLayout", 1, 700);
            layout.AutoSize = false;
            layout.Dock = DockStyle.Fill;
            layout.MinimumSize = new Size(700, 480);
            m_pluginConfigTab.AutoScrollMinSize = layout.MinimumSize;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
            layout.Controls.Add(groupBox1, 0, 0);
            layout.Controls.Add(pluginsGroupBox, 0, 1);
            groupBox1.TabIndex = 0;
            pluginsGroupBox.TabIndex = 1;
            lstPluginDirs.FullRowSelect = true;
        }

        private void BuildOrderLayout()
        {
            GroupBox[] groups = { groupBox7, groupBox2, groupBox8, groupBox19, groupBox11 };
            DataGridView[] grids = { authenticateDGV, authorizeDGV, gatewayDGV, changePasswordOrderDGV, eventDGV };
            Button[] up = { authenticateBtnUp, authorizeBtnUp, gatewayBtnUp, changePasswordOrderBtnUp, eventBtnUp };
            Button[] down = { authenticateBtnDown, authorizeBtnDown, gatewayBtnDown, changePasswordOrderBtnDown, eventBtnDown };
            m_pluginOrderTab.AutoScroll = true;
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.MinimumSize = new Size(760, 440);
            m_pluginOrderTab.AutoScrollMinSize = tableLayoutPanel1.MinimumSize;
            for (int i = 0; i < groups.Length; i++)
            {
                up[i].Image = null;
                down[i].Image = null;
                SetText(up[i], "Action.MoveUp");
                SetText(down[i], "Action.MoveDown");
                up[i].AccessibleName = groups[i].Text + ": " + up[i].Text.Replace("&", "");
                down[i].AccessibleName = groups[i].Text + ": " + down[i].Text.Replace("&", "");
                grids[i].AccessibleName = groups[i].Text + " plugin order";
                grids[i].AccessibleDescription = PresentationText("Order.Description");
                ListGroup(groups[i], grids[i], up[i], down[i]);
                groups[i].TabIndex = i;
            }
        }

        private void BuildSimulationLayout()
        {
            btnSimGo.Image = null;
            SetText(btnSimGo, "Action.Simulate");
            SetText(label4, "Label.SimulationUsername");
            SetText(label5, "Label.SimulationPassword");
            btnLaunchCredUI.Image = null;
            SetText(btnLaunchCredUI, "Action.CredentialPrompt");
            btnLaunchCredUI.AccessibleName = PresentationText("Action.CredentialPrompt");
            stageText.Add(simAuthResultPB, label6);
            stageText.Add(simAuthzResultPB, label7);
            stageText.Add(simGatewayResultPB, label8);
            stageText.Add(simFinalResultPB, label15);
            simAuthResultPB.Visible = simAuthzResultPB.Visible = simGatewayResultPB.Visible = simFinalResultPB.Visible = false;
            m_tileImage.SizeMode = PictureBoxSizeMode.Zoom;
            m_tileImage.Size = new Size(96, 96);
            Stack(groupBox3, m_radioEmulate, m_radioUseService, m_radioCredUI, Actions(btnLaunchCredUI));
            Stack(groupBox5, m_tileImage, label4, m_username, label5, m_password, Actions(btnSimGo));
            Stack(groupBox6, label1, m_usernameResult, label3, m_domainResult, label2, m_passwordResult, showTextResultPasswordCB);
            Stack(groupBox10, label6, label7, label8);
            simPluginResultsListView.MinimumSize = new Size(0, 200);
            Stack(groupBox13, simPluginResultsListView, label15, label9, simFinalResultMessageTB, label16, simResultLocalGroupsTB);
            // Hidden legacy pictures retain their existing ownership and image updates.
            groupBox10.Controls.AddRange(new Control[] { simAuthResultPB, simAuthzResultPB, simGatewayResultPB });
            groupBox13.Controls.Add(simFinalResultPB);
            var layout = PageLayout(m_simTab, "simulationLayout", 2, 700);
            layout.Controls.Add(groupBox3, 0, 0);
            layout.SetColumnSpan(groupBox3, 2);
            layout.Controls.Add(groupBox5, 0, 1);
            layout.Controls.Add(groupBox6, 1, 1);
            layout.Controls.Add(groupBox10, 0, 2);
            layout.SetColumnSpan(groupBox10, 2);
            layout.Controls.Add(groupBox13, 0, 3);
            layout.SetColumnSpan(groupBox13, 2);
            var logActions = Actions(viewLogBtn);
            layout.Controls.Add(logActions, 0, 4);
            layout.SetColumnSpan(logActions, 2);
            groupBox3.TabIndex = 0;
            groupBox5.TabIndex = 1;
            groupBox6.TabIndex = 2;
            groupBox10.TabIndex = 3;
            groupBox13.TabIndex = 4;
            logActions.TabIndex = 5;
            m_password.AccessibleDescription = PresentationText("Simulation.PasswordHelp");
            m_passwordResult.AccessibleDescription = PresentationText("Simulation.ResultPasswordHelp");
        }

        private void BuildProviderLayout()
        {
            textBox1.BackColor = SystemColors.Window;
            textBox1.ForeColor = SystemColors.WindowText;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.MinimumSize = new Size(0, 100);
            textBox1.AccessibleName = PresentationText("Provider.Warning");
            textBox1.Text = PresentationText("Provider.WarningText");
            Stack(groupBox18, defaultCPTileCB);
            Stack(groupBox20, m_hideUsernameFieldCb, m_hidePasswordFieldCb, label21);
            dgvCredProvFilter.MinimumSize = new Size(0, 220);
            Stack(groupBox9, label17, dgvCredProvFilter, label18, textBox1);
            var layout = PageLayout(cpOptions, "providerLayout", 1, 700);
            layout.Controls.Add(groupBox18, 0, 0);
            layout.Controls.Add(groupBox20, 0, 1);
            layout.Controls.Add(groupBox9, 0, 2);
            groupBox18.TabIndex = 0;
            groupBox20.TabIndex = 1;
            groupBox9.TabIndex = 2;
        }

        private void SetText(Control control, string key)
        {
            control.Text = PresentationText(key);
        }

        private void DisablePreviewActions()
        {
            // Even accidental input to the off-screen test window cannot affect the host.
            foreach (Button button in new[] { btnSaveAndClose, btnApply, serviceStartBtn, serviceStopBtn,
                cpRegisterBtn, cpEnableDisableBtn, btnAdd, btnRemove, configureButton, btnSimGo,
                btnLaunchCredUI, viewLogBtn, btnImageBrowse, btnGinaBrowse })
                button.Enabled = false;
        }

        private void NameInputs()
        {
            Control[] inputs = { serviceStatusTB, cpRegisteredTB, cpEnabledTB, m_tileImageTxt, motdTB,
                tbLogonProgressMessage, lstPluginDirs, pluginsDG, dgvCredProvFilter, m_username,
                m_password, m_usernameResult, m_domainResult, m_passwordResult, simPluginResultsListView,
                simFinalResultMessageTB, simResultLocalGroupsTB, m_txtGinaChain };
            string[] keys = { "ServiceStatus", "Registered", "Enabled", "TilePath", "Message", "Progress",
                "Directories", "Plugins", "Providers", "Username", "Password", "ResultUsername", "ResultDomain",
                "ResultPassword", "PluginResults", "ResultMessage", "ResultGroups", "GinaPath" };
            for (int i = 0; i < inputs.Length; i++) inputs[i].AccessibleName = PresentationText("Input." + keys[i]);
        }

        private void ApplyAccessibleDefaults(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.Font = Font;
                if (String.IsNullOrEmpty(control.AccessibleName) && !String.IsNullOrWhiteSpace(control.Text))
                    control.AccessibleName = control.Text.Replace("&", "");
                var label = control as Label;
                if (label != null)
                {
                    label.MaximumSize = new Size(320, 0);
                    label.TabStop = false;
                }
                var grid = control as DataGridView;
                if (grid != null)
                {
                    grid.StandardTab = true;
                    grid.BackgroundColor = SystemColors.Window;
                    grid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
                    grid.DefaultCellStyle.BackColor = SystemColors.Window;
                    grid.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                    grid.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
                    grid.DefaultCellStyle.Font = Font;
                    grid.RowTemplate.Height = Math.Max(28, Font.Height + 10);
                }
                ApplyAccessibleDefaults(control);
            }
        }

        private void UpdateStageText(PictureBox picture, string resourceKey)
        {
            Label label;
            if (!stageText.TryGetValue(picture, out label)) return;
            string stage = picture == simAuthResultPB ? "Stage.Authentication" :
                picture == simAuthzResultPB ? "Stage.Authorization" :
                picture == simGatewayResultPB ? "Stage.Gateway" : "Stage.Result";
            label.Text = PresentationText(stage) + ": " + PresentationText(resourceKey);
            label.AccessibleName = label.Text;
            label.ForeColor = SystemColors.ControlText;
        }
    }
}
