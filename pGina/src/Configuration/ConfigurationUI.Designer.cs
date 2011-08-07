namespace pGina.Configuration
{
    partial class ConfigurationUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_tabs = new System.Windows.Forms.TabControl();
            this.m_generalConfigTab = new System.Windows.Forms.TabPage();
            this.m_pluginConfigTab = new System.Windows.Forms.TabPage();
            this.pluginsGroupBox = new System.Windows.Forms.GroupBox();
            this.pluginsDG = new System.Windows.Forms.DataGridView();
            this.configureButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstPluginDirs = new System.Windows.Forms.ListView();
            this.m_pluginOrderTab = new System.Windows.Forms.TabPage();
            this.m_simTab = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.m_message = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.m_password = new System.Windows.Forms.TextBox();
            this.m_username = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.m_tileImage = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.m_liveLog = new System.Windows.Forms.ListView();
            this.LogEntry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.m_useSavedConfig = new System.Windows.Forms.CheckBox();
            this.m_radioCredUI = new System.Windows.Forms.RadioButton();
            this.m_radioEmulate = new System.Windows.Forms.RadioButton();
            this.m_radioUseService = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOkay = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.authenticateDGV = new System.Windows.Forms.DataGridView();
            this.authenticateBtnDown = new System.Windows.Forms.Button();
            this.authenticateBtnUp = new System.Windows.Forms.Button();
            this.authorizeBtnDown = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.authorizeBtnUp = new System.Windows.Forms.Button();
            this.authorizeDGV = new System.Windows.Forms.DataGridView();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.gatewayBtnDown = new System.Windows.Forms.Button();
            this.gatewayBtnUp = new System.Windows.Forms.Button();
            this.gatewayDGV = new System.Windows.Forms.DataGridView();
            this.systemBtnUp = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.systemBtnDown = new System.Windows.Forms.Button();
            this.systemDGV = new System.Windows.Forms.DataGridView();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.userBtnDown = new System.Windows.Forms.Button();
            this.userBtnUp = new System.Windows.Forms.Button();
            this.userDGV = new System.Windows.Forms.DataGridView();
            this.eventBtnUp = new System.Windows.Forms.Button();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.eventBtnDown = new System.Windows.Forms.Button();
            this.eventDGV = new System.Windows.Forms.DataGridView();
            this.m_tabs.SuspendLayout();
            this.m_pluginConfigTab.SuspendLayout();
            this.pluginsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pluginsDG)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.m_pluginOrderTab.SuspendLayout();
            this.m_simTab.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_tileImage)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authenticateDGV)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authorizeDGV)).BeginInit();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gatewayDGV)).BeginInit();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.systemDGV)).BeginInit();
            this.groupBox10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.userDGV)).BeginInit();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // m_tabs
            // 
            this.m_tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_tabs.Controls.Add(this.m_generalConfigTab);
            this.m_tabs.Controls.Add(this.m_pluginConfigTab);
            this.m_tabs.Controls.Add(this.m_pluginOrderTab);
            this.m_tabs.Controls.Add(this.m_simTab);
            this.m_tabs.Location = new System.Drawing.Point(12, 12);
            this.m_tabs.Name = "m_tabs";
            this.m_tabs.SelectedIndex = 0;
            this.m_tabs.Size = new System.Drawing.Size(798, 573);
            this.m_tabs.TabIndex = 0;
            // 
            // m_generalConfigTab
            // 
            this.m_generalConfigTab.Location = new System.Drawing.Point(4, 22);
            this.m_generalConfigTab.Name = "m_generalConfigTab";
            this.m_generalConfigTab.Padding = new System.Windows.Forms.Padding(3);
            this.m_generalConfigTab.Size = new System.Drawing.Size(790, 531);
            this.m_generalConfigTab.TabIndex = 1;
            this.m_generalConfigTab.Text = "General";
            this.m_generalConfigTab.UseVisualStyleBackColor = true;
            // 
            // m_pluginConfigTab
            // 
            this.m_pluginConfigTab.Controls.Add(this.pluginsGroupBox);
            this.m_pluginConfigTab.Controls.Add(this.groupBox1);
            this.m_pluginConfigTab.Location = new System.Drawing.Point(4, 22);
            this.m_pluginConfigTab.Name = "m_pluginConfigTab";
            this.m_pluginConfigTab.Padding = new System.Windows.Forms.Padding(3);
            this.m_pluginConfigTab.Size = new System.Drawing.Size(790, 547);
            this.m_pluginConfigTab.TabIndex = 0;
            this.m_pluginConfigTab.Text = "Plugin Selection";
            this.m_pluginConfigTab.UseVisualStyleBackColor = true;
            // 
            // pluginsGroupBox
            // 
            this.pluginsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginsGroupBox.Controls.Add(this.pluginsDG);
            this.pluginsGroupBox.Controls.Add(this.configureButton);
            this.pluginsGroupBox.Location = new System.Drawing.Point(15, 212);
            this.pluginsGroupBox.Name = "pluginsGroupBox";
            this.pluginsGroupBox.Size = new System.Drawing.Size(758, 314);
            this.pluginsGroupBox.TabIndex = 9;
            this.pluginsGroupBox.TabStop = false;
            this.pluginsGroupBox.Text = "Current Plugins";
            // 
            // pluginsDG
            // 
            this.pluginsDG.AllowUserToAddRows = false;
            this.pluginsDG.AllowUserToDeleteRows = false;
            this.pluginsDG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.pluginsDG.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.pluginsDG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.pluginsDG.DefaultCellStyle = dataGridViewCellStyle2;
            this.pluginsDG.Location = new System.Drawing.Point(10, 19);
            this.pluginsDG.Name = "pluginsDG";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.pluginsDG.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.pluginsDG.Size = new System.Drawing.Size(735, 253);
            this.pluginsDG.TabIndex = 10;
            // 
            // configureButton
            // 
            this.configureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.configureButton.Location = new System.Drawing.Point(671, 278);
            this.configureButton.Name = "configureButton";
            this.configureButton.Size = new System.Drawing.Size(74, 25);
            this.configureButton.TabIndex = 9;
            this.configureButton.Text = "Configure...";
            this.configureButton.UseVisualStyleBackColor = true;
            this.configureButton.Click += new System.EventHandler(this.configureButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnRemove);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.lstPluginDirs);
            this.groupBox1.Location = new System.Drawing.Point(15, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(758, 186);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search Directories";
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(670, 151);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(589, 151);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lstPluginDirs
            // 
            this.lstPluginDirs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPluginDirs.Location = new System.Drawing.Point(10, 19);
            this.lstPluginDirs.MultiSelect = false;
            this.lstPluginDirs.Name = "lstPluginDirs";
            this.lstPluginDirs.Size = new System.Drawing.Size(735, 123);
            this.lstPluginDirs.TabIndex = 5;
            this.lstPluginDirs.UseCompatibleStateImageBehavior = false;
            this.lstPluginDirs.View = System.Windows.Forms.View.Details;
            // 
            // m_pluginOrderTab
            // 
            this.m_pluginOrderTab.Controls.Add(this.groupBox10);
            this.m_pluginOrderTab.Controls.Add(this.groupBox8);
            this.m_pluginOrderTab.Controls.Add(this.groupBox11);
            this.m_pluginOrderTab.Controls.Add(this.groupBox2);
            this.m_pluginOrderTab.Controls.Add(this.groupBox9);
            this.m_pluginOrderTab.Controls.Add(this.groupBox7);
            this.m_pluginOrderTab.Location = new System.Drawing.Point(4, 22);
            this.m_pluginOrderTab.Name = "m_pluginOrderTab";
            this.m_pluginOrderTab.Size = new System.Drawing.Size(790, 547);
            this.m_pluginOrderTab.TabIndex = 3;
            this.m_pluginOrderTab.Text = "Plugin Order";
            this.m_pluginOrderTab.UseVisualStyleBackColor = true;
            // 
            // m_simTab
            // 
            this.m_simTab.Controls.Add(this.groupBox6);
            this.m_simTab.Controls.Add(this.groupBox5);
            this.m_simTab.Controls.Add(this.groupBox4);
            this.m_simTab.Controls.Add(this.groupBox3);
            this.m_simTab.Location = new System.Drawing.Point(4, 22);
            this.m_simTab.Name = "m_simTab";
            this.m_simTab.Padding = new System.Windows.Forms.Padding(3);
            this.m_simTab.Size = new System.Drawing.Size(790, 531);
            this.m_simTab.TabIndex = 2;
            this.m_simTab.Text = "Simulation";
            this.m_simTab.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.m_message);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.label6);
            this.groupBox6.Location = new System.Drawing.Point(16, 161);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(315, 203);
            this.groupBox6.TabIndex = 2;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Results";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 96);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Message:";
            // 
            // m_message
            // 
            this.m_message.AcceptsReturn = true;
            this.m_message.AcceptsTab = true;
            this.m_message.Location = new System.Drawing.Point(76, 93);
            this.m_message.Multiline = true;
            this.m_message.Name = "m_message";
            this.m_message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.m_message.Size = new System.Drawing.Size(223, 92);
            this.m_message.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(52, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Gateway:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Authorization:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Authentication:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.m_password);
            this.groupBox5.Controls.Add(this.m_username);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.m_tileImage);
            this.groupBox5.Location = new System.Drawing.Point(355, 18);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(354, 256);
            this.groupBox5.TabIndex = 1;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Simulated LogonUI";
            // 
            // button1
            // 
            this.button1.Image = global::pGina.Configuration.Properties.Resources.arrow_right_3;
            this.button1.Location = new System.Drawing.Point(305, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(23, 20);
            this.button1.TabIndex = 5;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // m_password
            // 
            this.m_password.Location = new System.Drawing.Point(75, 208);
            this.m_password.Name = "m_password";
            this.m_password.Size = new System.Drawing.Size(222, 20);
            this.m_password.TabIndex = 4;
            // 
            // m_username
            // 
            this.m_username.Location = new System.Drawing.Point(75, 182);
            this.m_username.Name = "m_username";
            this.m_username.Size = new System.Drawing.Size(222, 20);
            this.m_username.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 211);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Username:";
            // 
            // m_tileImage
            // 
            this.m_tileImage.Location = new System.Drawing.Point(121, 28);
            this.m_tileImage.Name = "m_tileImage";
            this.m_tileImage.Size = new System.Drawing.Size(130, 130);
            this.m_tileImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_tileImage.TabIndex = 0;
            this.m_tileImage.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.m_liveLog);
            this.groupBox4.Location = new System.Drawing.Point(16, 370);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(754, 142);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Live Log";
            // 
            // m_liveLog
            // 
            this.m_liveLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_liveLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LogEntry});
            this.m_liveLog.FullRowSelect = true;
            this.m_liveLog.GridLines = true;
            this.m_liveLog.Location = new System.Drawing.Point(6, 18);
            this.m_liveLog.Name = "m_liveLog";
            this.m_liveLog.ShowItemToolTips = true;
            this.m_liveLog.Size = new System.Drawing.Size(742, 114);
            this.m_liveLog.TabIndex = 0;
            this.m_liveLog.UseCompatibleStateImageBehavior = false;
            this.m_liveLog.View = System.Windows.Forms.View.Details;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.m_useSavedConfig);
            this.groupBox3.Controls.Add(this.m_radioCredUI);
            this.groupBox3.Controls.Add(this.m_radioEmulate);
            this.groupBox3.Controls.Add(this.m_radioUseService);
            this.groupBox3.Location = new System.Drawing.Point(16, 18);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(315, 137);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Simulation Method";
            // 
            // m_useSavedConfig
            // 
            this.m_useSavedConfig.AutoSize = true;
            this.m_useSavedConfig.Checked = true;
            this.m_useSavedConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_useSavedConfig.Location = new System.Drawing.Point(21, 104);
            this.m_useSavedConfig.Name = "m_useSavedConfig";
            this.m_useSavedConfig.Size = new System.Drawing.Size(144, 17);
            this.m_useSavedConfig.TabIndex = 3;
            this.m_useSavedConfig.Text = "Use Saved Configuration";
            this.m_useSavedConfig.UseVisualStyleBackColor = true;
            // 
            // m_radioCredUI
            // 
            this.m_radioCredUI.AutoSize = true;
            this.m_radioCredUI.Location = new System.Drawing.Point(21, 74);
            this.m_radioCredUI.Name = "m_radioCredUI";
            this.m_radioCredUI.Size = new System.Drawing.Size(142, 17);
            this.m_radioCredUI.TabIndex = 2;
            this.m_radioCredUI.Text = "Launch a CredUI Prompt";
            this.m_radioCredUI.UseVisualStyleBackColor = true;
            this.m_radioCredUI.CheckedChanged += new System.EventHandler(this.simMethodChanged);
            // 
            // m_radioEmulate
            // 
            this.m_radioEmulate.AutoSize = true;
            this.m_radioEmulate.Checked = true;
            this.m_radioEmulate.Location = new System.Drawing.Point(21, 51);
            this.m_radioEmulate.Name = "m_radioEmulate";
            this.m_radioEmulate.Size = new System.Drawing.Size(133, 17);
            this.m_radioEmulate.TabIndex = 1;
            this.m_radioEmulate.TabStop = true;
            this.m_radioEmulate.Text = "Emulate pGina Service";
            this.m_radioEmulate.UseVisualStyleBackColor = true;
            this.m_radioEmulate.CheckedChanged += new System.EventHandler(this.simMethodChanged);
            // 
            // m_radioUseService
            // 
            this.m_radioUseService.AutoSize = true;
            this.m_radioUseService.Location = new System.Drawing.Point(21, 28);
            this.m_radioUseService.Name = "m_radioUseService";
            this.m_radioUseService.Size = new System.Drawing.Size(114, 17);
            this.m_radioUseService.TabIndex = 0;
            this.m_radioUseService.Text = "Use pGina Service";
            this.m_radioUseService.UseVisualStyleBackColor = true;
            this.m_radioUseService.CheckedChanged += new System.EventHandler(this.simMethodChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(650, 602);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(731, 602);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 3;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.authenticateBtnDown);
            this.groupBox7.Controls.Add(this.authenticateBtnUp);
            this.groupBox7.Controls.Add(this.authenticateDGV);
            this.groupBox7.Location = new System.Drawing.Point(16, 16);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(367, 169);
            this.groupBox7.TabIndex = 12;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Authentication";
            // 
            // authenticateDGV
            // 
            this.authenticateDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authenticateDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authenticateDGV.Location = new System.Drawing.Point(15, 20);
            this.authenticateDGV.Name = "authenticateDGV";
            this.authenticateDGV.Size = new System.Drawing.Size(309, 131);
            this.authenticateDGV.TabIndex = 0;
            // 
            // authenticateBtnDown
            // 
            this.authenticateBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authenticateBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.authenticateBtnDown.Location = new System.Drawing.Point(330, 89);
            this.authenticateBtnDown.Name = "authenticateBtnDown";
            this.authenticateBtnDown.Size = new System.Drawing.Size(26, 27);
            this.authenticateBtnDown.TabIndex = 16;
            this.authenticateBtnDown.UseVisualStyleBackColor = true;
            this.authenticateBtnDown.Click += new System.EventHandler(this.authenticateBtnDown_Click);
            // 
            // authenticateBtnUp
            // 
            this.authenticateBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authenticateBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.authenticateBtnUp.Location = new System.Drawing.Point(330, 56);
            this.authenticateBtnUp.Name = "authenticateBtnUp";
            this.authenticateBtnUp.Size = new System.Drawing.Size(26, 27);
            this.authenticateBtnUp.TabIndex = 15;
            this.authenticateBtnUp.UseVisualStyleBackColor = true;
            this.authenticateBtnUp.Click += new System.EventHandler(this.authenticateBtnUp_Click);
            // 
            // authorizeBtnDown
            // 
            this.authorizeBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authorizeBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.authorizeBtnDown.Location = new System.Drawing.Point(339, 89);
            this.authorizeBtnDown.Name = "authorizeBtnDown";
            this.authorizeBtnDown.Size = new System.Drawing.Size(26, 27);
            this.authorizeBtnDown.TabIndex = 16;
            this.authorizeBtnDown.UseVisualStyleBackColor = true;
            this.authorizeBtnDown.Click += new System.EventHandler(this.authorizeBtnDown_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.authorizeBtnDown);
            this.groupBox2.Controls.Add(this.authorizeBtnUp);
            this.groupBox2.Controls.Add(this.authorizeDGV);
            this.groupBox2.Location = new System.Drawing.Point(398, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(376, 169);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Authorization";
            // 
            // authorizeBtnUp
            // 
            this.authorizeBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authorizeBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.authorizeBtnUp.Location = new System.Drawing.Point(339, 56);
            this.authorizeBtnUp.Name = "authorizeBtnUp";
            this.authorizeBtnUp.Size = new System.Drawing.Size(26, 27);
            this.authorizeBtnUp.TabIndex = 15;
            this.authorizeBtnUp.UseVisualStyleBackColor = true;
            this.authorizeBtnUp.Click += new System.EventHandler(this.authorizeBtnUp_Click);
            // 
            // authorizeDGV
            // 
            this.authorizeDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authorizeDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authorizeDGV.Location = new System.Drawing.Point(15, 20);
            this.authorizeDGV.Name = "authorizeDGV";
            this.authorizeDGV.Size = new System.Drawing.Size(318, 131);
            this.authorizeDGV.TabIndex = 0;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.gatewayBtnDown);
            this.groupBox8.Controls.Add(this.gatewayBtnUp);
            this.groupBox8.Controls.Add(this.gatewayDGV);
            this.groupBox8.Location = new System.Drawing.Point(16, 191);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(367, 169);
            this.groupBox8.TabIndex = 18;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Gateway";
            // 
            // gatewayBtnDown
            // 
            this.gatewayBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.gatewayBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.gatewayBtnDown.Location = new System.Drawing.Point(330, 89);
            this.gatewayBtnDown.Name = "gatewayBtnDown";
            this.gatewayBtnDown.Size = new System.Drawing.Size(26, 27);
            this.gatewayBtnDown.TabIndex = 16;
            this.gatewayBtnDown.UseVisualStyleBackColor = true;
            this.gatewayBtnDown.Click += new System.EventHandler(this.gatewayBtnDown_Click);
            // 
            // gatewayBtnUp
            // 
            this.gatewayBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.gatewayBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.gatewayBtnUp.Location = new System.Drawing.Point(330, 56);
            this.gatewayBtnUp.Name = "gatewayBtnUp";
            this.gatewayBtnUp.Size = new System.Drawing.Size(26, 27);
            this.gatewayBtnUp.TabIndex = 15;
            this.gatewayBtnUp.UseVisualStyleBackColor = true;
            this.gatewayBtnUp.Click += new System.EventHandler(this.gatewayBtnUp_Click);
            // 
            // gatewayDGV
            // 
            this.gatewayDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gatewayDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gatewayDGV.Location = new System.Drawing.Point(15, 20);
            this.gatewayDGV.Name = "gatewayDGV";
            this.gatewayDGV.Size = new System.Drawing.Size(309, 131);
            this.gatewayDGV.TabIndex = 0;
            // 
            // systemBtnUp
            // 
            this.systemBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.systemBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.systemBtnUp.Location = new System.Drawing.Point(339, 56);
            this.systemBtnUp.Name = "systemBtnUp";
            this.systemBtnUp.Size = new System.Drawing.Size(26, 27);
            this.systemBtnUp.TabIndex = 15;
            this.systemBtnUp.UseVisualStyleBackColor = true;
            this.systemBtnUp.Click += new System.EventHandler(this.systemBtnUp_Click);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.systemBtnDown);
            this.groupBox9.Controls.Add(this.systemBtnUp);
            this.groupBox9.Controls.Add(this.systemDGV);
            this.groupBox9.Location = new System.Drawing.Point(398, 191);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(376, 169);
            this.groupBox9.TabIndex = 19;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "System Session";
            // 
            // systemBtnDown
            // 
            this.systemBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.systemBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.systemBtnDown.Location = new System.Drawing.Point(339, 89);
            this.systemBtnDown.Name = "systemBtnDown";
            this.systemBtnDown.Size = new System.Drawing.Size(26, 27);
            this.systemBtnDown.TabIndex = 16;
            this.systemBtnDown.UseVisualStyleBackColor = true;
            this.systemBtnDown.Click += new System.EventHandler(this.systemBtnDown_Click);
            // 
            // systemDGV
            // 
            this.systemDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.systemDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.systemDGV.Location = new System.Drawing.Point(15, 20);
            this.systemDGV.Name = "systemDGV";
            this.systemDGV.Size = new System.Drawing.Size(318, 131);
            this.systemDGV.TabIndex = 0;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.userBtnDown);
            this.groupBox10.Controls.Add(this.userBtnUp);
            this.groupBox10.Controls.Add(this.userDGV);
            this.groupBox10.Location = new System.Drawing.Point(16, 366);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(367, 169);
            this.groupBox10.TabIndex = 20;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "User Session";
            // 
            // userBtnDown
            // 
            this.userBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.userBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.userBtnDown.Location = new System.Drawing.Point(330, 89);
            this.userBtnDown.Name = "userBtnDown";
            this.userBtnDown.Size = new System.Drawing.Size(26, 27);
            this.userBtnDown.TabIndex = 16;
            this.userBtnDown.UseVisualStyleBackColor = true;
            this.userBtnDown.Click += new System.EventHandler(this.userBtnDown_Click);
            // 
            // userBtnUp
            // 
            this.userBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.userBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.userBtnUp.Location = new System.Drawing.Point(330, 56);
            this.userBtnUp.Name = "userBtnUp";
            this.userBtnUp.Size = new System.Drawing.Size(26, 27);
            this.userBtnUp.TabIndex = 15;
            this.userBtnUp.UseVisualStyleBackColor = true;
            this.userBtnUp.Click += new System.EventHandler(this.userBtnUp_Click);
            // 
            // userDGV
            // 
            this.userDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.userDGV.Location = new System.Drawing.Point(15, 20);
            this.userDGV.Name = "userDGV";
            this.userDGV.Size = new System.Drawing.Size(309, 131);
            this.userDGV.TabIndex = 0;
            // 
            // eventBtnUp
            // 
            this.eventBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.eventBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.eventBtnUp.Location = new System.Drawing.Point(339, 56);
            this.eventBtnUp.Name = "eventBtnUp";
            this.eventBtnUp.Size = new System.Drawing.Size(26, 27);
            this.eventBtnUp.TabIndex = 15;
            this.eventBtnUp.UseVisualStyleBackColor = true;
            this.eventBtnUp.Click += new System.EventHandler(this.eventBtnUp_Click);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.eventBtnDown);
            this.groupBox11.Controls.Add(this.eventBtnUp);
            this.groupBox11.Controls.Add(this.eventDGV);
            this.groupBox11.Location = new System.Drawing.Point(398, 366);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(376, 169);
            this.groupBox11.TabIndex = 21;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Event Notification";
            // 
            // eventBtnDown
            // 
            this.eventBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.eventBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.eventBtnDown.Location = new System.Drawing.Point(339, 89);
            this.eventBtnDown.Name = "eventBtnDown";
            this.eventBtnDown.Size = new System.Drawing.Size(26, 27);
            this.eventBtnDown.TabIndex = 16;
            this.eventBtnDown.UseVisualStyleBackColor = true;
            this.eventBtnDown.Click += new System.EventHandler(this.eventBtnDown_Click);
            // 
            // eventDGV
            // 
            this.eventDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eventDGV.Location = new System.Drawing.Point(15, 20);
            this.eventDGV.Name = "eventDGV";
            this.eventDGV.Size = new System.Drawing.Size(318, 131);
            this.eventDGV.TabIndex = 0;
            // 
            // ConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 637);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.m_tabs);
            this.Name = "ConfigurationUI";
            this.Text = "pGina Configuration";
            this.m_tabs.ResumeLayout(false);
            this.m_pluginConfigTab.ResumeLayout(false);
            this.pluginsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pluginsDG)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.m_pluginOrderTab.ResumeLayout(false);
            this.m_simTab.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_tileImage)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.authenticateDGV)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.authorizeDGV)).EndInit();
            this.groupBox8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gatewayDGV)).EndInit();
            this.groupBox9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.systemDGV)).EndInit();
            this.groupBox10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.userDGV)).EndInit();
            this.groupBox11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.eventDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl m_tabs;
        private System.Windows.Forms.TabPage m_pluginConfigTab;
        private System.Windows.Forms.TabPage m_generalConfigTab;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListView lstPluginDirs;
        private System.Windows.Forms.GroupBox pluginsGroupBox;
        private System.Windows.Forms.DataGridView pluginsDG;
        private System.Windows.Forms.Button configureButton;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.TabPage m_simTab;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView m_liveLog;
        private System.Windows.Forms.ColumnHeader LogEntry;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton m_radioCredUI;
        private System.Windows.Forms.RadioButton m_radioEmulate;
        private System.Windows.Forms.RadioButton m_radioUseService;
        private System.Windows.Forms.CheckBox m_useSavedConfig;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.PictureBox m_tileImage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_password;
        private System.Windows.Forms.TextBox m_username;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox m_message;
        private System.Windows.Forms.TabPage m_pluginOrderTab;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button authenticateBtnDown;
        private System.Windows.Forms.Button authenticateBtnUp;
        private System.Windows.Forms.DataGridView authenticateDGV;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button authorizeBtnDown;
        private System.Windows.Forms.Button authorizeBtnUp;
        private System.Windows.Forms.DataGridView authorizeDGV;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button gatewayBtnDown;
        private System.Windows.Forms.Button gatewayBtnUp;
        private System.Windows.Forms.DataGridView gatewayDGV;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Button systemBtnDown;
        private System.Windows.Forms.Button systemBtnUp;
        private System.Windows.Forms.DataGridView systemDGV;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button userBtnDown;
        private System.Windows.Forms.Button userBtnUp;
        private System.Windows.Forms.DataGridView userDGV;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Button eventBtnDown;
        private System.Windows.Forms.Button eventBtnUp;
        private System.Windows.Forms.DataGridView eventDGV;
    }
}