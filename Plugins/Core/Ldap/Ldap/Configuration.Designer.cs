namespace pGina.Plugin.Ldap
{
    partial class Configuration
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
            this.saveButton = new System.Windows.Forms.Button();
            this.ldapHostDescriptionLabel = new System.Windows.Forms.Label();
            this.ldapHostTextBox = new System.Windows.Forms.TextBox();
            this.descriptionToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.validateServerCertCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ldapPortTextBox = new System.Windows.Forms.TextBox();
            this.ldapPortLabel = new System.Windows.Forms.Label();
            this.useSslCheckBox = new System.Windows.Forms.CheckBox();
            this.dnPatternLabel = new System.Windows.Forms.Label();
            this.searchForDnCheckBox = new System.Windows.Forms.CheckBox();
            this.searchFilterLabel = new System.Windows.Forms.Label();
            this.searchContextsLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.timeoutLabel = new System.Windows.Forms.Label();
            this.timeoutTextBox = new System.Windows.Forms.TextBox();
            this.searchDnTextBox = new System.Windows.Forms.TextBox();
            this.authzRequireAuthCB = new System.Windows.Forms.CheckBox();
            this.authzAllowOnErrorCB = new System.Windows.Forms.CheckBox();
            this.sslCertFileTextBox = new System.Windows.Forms.TextBox();
            this.groupMemberAttrTB = new System.Windows.Forms.TextBox();
            this.groupDNPattern = new System.Windows.Forms.TextBox();
            this.allowEmptyPwCB = new System.Windows.Forms.CheckBox();
            this.dnPatternTextBox = new System.Windows.Forms.TextBox();
            this.ldapServerGroupBox = new System.Windows.Forms.GroupBox();
            this.DereferenceComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.showPwCB = new System.Windows.Forms.CheckBox();
            this.searchPassTextBox = new System.Windows.Forms.TextBox();
            this.sslCertFileBrowseButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.searchContextsTextBox = new System.Windows.Forms.TextBox();
            this.searchFilterTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.authTabPage = new System.Windows.Forms.TabPage();
            this.authzTabPage = new System.Windows.Forms.TabPage();
            this.authzRuleDeleteBtn = new System.Windows.Forms.Button();
            this.authzRuleDownBtn = new System.Windows.Forms.Button();
            this.authzRuleUpBtn = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.authzDefaultDenyRB = new System.Windows.Forms.RadioButton();
            this.authzDefaultAllowRB = new System.Windows.Forms.RadioButton();
            this.authzRuleAddButton = new System.Windows.Forms.Button();
            this.authzRulesListBox = new System.Windows.Forms.ListBox();
            this.authzRuleActionComboBox = new System.Windows.Forms.ComboBox();
            this.authzRuleGroupTB = new System.Windows.Forms.TextBox();
            this.authzRuleMemberComboBox = new System.Windows.Forms.ComboBox();
            this.gatewayTabPage = new System.Windows.Forms.TabPage();
            this.gatwayRemoteGroupTB = new System.Windows.Forms.TextBox();
            this.gatewayRuleGroupMemberCB = new System.Windows.Forms.ComboBox();
            this.gatewayRuleDeleteBtn = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.gatewayLocalGroupTB = new System.Windows.Forms.TextBox();
            this.gatewayRulesListBox = new System.Windows.Forms.ListBox();
            this.addGatewayGroupRuleButton = new System.Windows.Forms.Button();
            this.ldapServerGroupBox.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.authTabPage.SuspendLayout();
            this.authzTabPage.SuspendLayout();
            this.gatewayTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(601, 500);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(81, 26);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // ldapHostDescriptionLabel
            // 
            this.ldapHostDescriptionLabel.AutoSize = true;
            this.ldapHostDescriptionLabel.Location = new System.Drawing.Point(6, 22);
            this.ldapHostDescriptionLabel.Name = "ldapHostDescriptionLabel";
            this.ldapHostDescriptionLabel.Size = new System.Drawing.Size(71, 13);
            this.ldapHostDescriptionLabel.TabIndex = 1;
            this.ldapHostDescriptionLabel.Text = "LDAP Host(s)";
            this.descriptionToolTip.SetToolTip(this.ldapHostDescriptionLabel, "A whitespace separated list of hostnames or IP addresses.");
            // 
            // ldapHostTextBox
            // 
            this.ldapHostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ldapHostTextBox.Location = new System.Drawing.Point(110, 19);
            this.ldapHostTextBox.Name = "ldapHostTextBox";
            this.ldapHostTextBox.Size = new System.Drawing.Size(462, 20);
            this.ldapHostTextBox.TabIndex = 2;
            this.descriptionToolTip.SetToolTip(this.ldapHostTextBox, "A whitespace separated list of hostnames or IP addresses.");
            // 
            // validateServerCertCheckBox
            // 
            this.validateServerCertCheckBox.AutoSize = true;
            this.validateServerCertCheckBox.Location = new System.Drawing.Point(440, 47);
            this.validateServerCertCheckBox.Name = "validateServerCertCheckBox";
            this.validateServerCertCheckBox.Size = new System.Drawing.Size(148, 17);
            this.validateServerCertCheckBox.TabIndex = 6;
            this.validateServerCertCheckBox.Text = "Validate Server Certificate";
            this.descriptionToolTip.SetToolTip(this.validateServerCertCheckBox, "Whether or not to verify the server\'s SSL certificate when connecting.");
            this.validateServerCertCheckBox.UseVisualStyleBackColor = true;
            this.validateServerCertCheckBox.CheckedChanged += new System.EventHandler(this.validateServerCertCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "SSL Certificate File";
            this.descriptionToolTip.SetToolTip(this.label1, "File containing the LDAP server\'s public certificate.  If this is left blank (and" +
        " \"Validate\" is checked) the server\'s\r\ncertificate is verified against the Window" +
        "s certificate store.");
            // 
            // ldapPortTextBox
            // 
            this.ldapPortTextBox.Location = new System.Drawing.Point(110, 45);
            this.ldapPortTextBox.Name = "ldapPortTextBox";
            this.ldapPortTextBox.Size = new System.Drawing.Size(70, 20);
            this.ldapPortTextBox.TabIndex = 3;
            this.descriptionToolTip.SetToolTip(this.ldapPortTextBox, "The port number");
            // 
            // ldapPortLabel
            // 
            this.ldapPortLabel.AutoSize = true;
            this.ldapPortLabel.Location = new System.Drawing.Point(6, 48);
            this.ldapPortLabel.Name = "ldapPortLabel";
            this.ldapPortLabel.Size = new System.Drawing.Size(57, 13);
            this.ldapPortLabel.TabIndex = 4;
            this.ldapPortLabel.Text = "LDAP Port";
            this.descriptionToolTip.SetToolTip(this.ldapPortLabel, "The port number");
            // 
            // useSslCheckBox
            // 
            this.useSslCheckBox.AutoSize = true;
            this.useSslCheckBox.Location = new System.Drawing.Point(363, 47);
            this.useSslCheckBox.Name = "useSslCheckBox";
            this.useSslCheckBox.Size = new System.Drawing.Size(68, 17);
            this.useSslCheckBox.TabIndex = 5;
            this.useSslCheckBox.Text = "Use SSL";
            this.descriptionToolTip.SetToolTip(this.useSslCheckBox, "Whether or not to use SSL encryption.");
            this.useSslCheckBox.UseVisualStyleBackColor = true;
            this.useSslCheckBox.CheckedChanged += new System.EventHandler(this.useSslCheckBox_CheckedChanged);
            // 
            // dnPatternLabel
            // 
            this.dnPatternLabel.AutoSize = true;
            this.dnPatternLabel.Location = new System.Drawing.Point(6, 36);
            this.dnPatternLabel.Name = "dnPatternLabel";
            this.dnPatternLabel.Size = new System.Drawing.Size(85, 13);
            this.dnPatternLabel.TabIndex = 0;
            this.dnPatternLabel.Text = "User DN Pattern";
            this.descriptionToolTip.SetToolTip(this.dnPatternLabel, "The pattern to use when creating a DN from a user name.\r\n%u can be used to indica" +
        "te the user name.\r\n");
            // 
            // searchForDnCheckBox
            // 
            this.searchForDnCheckBox.AutoSize = true;
            this.searchForDnCheckBox.Location = new System.Drawing.Point(6, 68);
            this.searchForDnCheckBox.Name = "searchForDnCheckBox";
            this.searchForDnCheckBox.Size = new System.Drawing.Size(94, 17);
            this.searchForDnCheckBox.TabIndex = 2;
            this.searchForDnCheckBox.Text = "Search for DN";
            this.descriptionToolTip.SetToolTip(this.searchForDnCheckBox, "Whether or not to search a set of LDAP trees for the\r\nDN rather than using the ab" +
        "ove pattern.");
            this.searchForDnCheckBox.UseVisualStyleBackColor = true;
            this.searchForDnCheckBox.CheckedChanged += new System.EventHandler(this.searchForDnCheckBox_CheckedChanged);
            // 
            // searchFilterLabel
            // 
            this.searchFilterLabel.AutoSize = true;
            this.searchFilterLabel.Location = new System.Drawing.Point(6, 94);
            this.searchFilterLabel.Name = "searchFilterLabel";
            this.searchFilterLabel.Size = new System.Drawing.Size(66, 13);
            this.searchFilterLabel.TabIndex = 3;
            this.searchFilterLabel.Text = "Search Filter";
            this.descriptionToolTip.SetToolTip(this.searchFilterLabel, "The filter to use when searching.\r\n%u is replaced with the user name\r\n");
            // 
            // searchContextsLabel
            // 
            this.searchContextsLabel.AutoSize = true;
            this.searchContextsLabel.Location = new System.Drawing.Point(6, 120);
            this.searchContextsLabel.Name = "searchContextsLabel";
            this.searchContextsLabel.Size = new System.Drawing.Size(91, 13);
            this.searchContextsLabel.TabIndex = 5;
            this.searchContextsLabel.Text = "Search Context(s)";
            this.descriptionToolTip.SetToolTip(this.searchContextsLabel, "A list of one or more DNs (one per line) that indicate\r\nroots of LDAP subtrees th" +
        "at will be searched.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Search DN";
            this.descriptionToolTip.SetToolTip(this.label2, "The DN to use when connecting to the server in order\r\nto perform the search.  If " +
        "this is left blank, an anonymous\r\nbind will be attempted.");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Search Password";
            this.descriptionToolTip.SetToolTip(this.label3, "The password to use when searching");
            // 
            // timeoutLabel
            // 
            this.timeoutLabel.AutoSize = true;
            this.timeoutLabel.Location = new System.Drawing.Point(206, 48);
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.Size = new System.Drawing.Size(45, 13);
            this.timeoutLabel.TabIndex = 10;
            this.timeoutLabel.Text = "Timeout";
            this.descriptionToolTip.SetToolTip(this.timeoutLabel, "The number of seconds to wait for a server to respond before giving up \r\nand movi" +
        "ng on to the next server.");
            // 
            // timeoutTextBox
            // 
            this.timeoutTextBox.Location = new System.Drawing.Point(265, 45);
            this.timeoutTextBox.Name = "timeoutTextBox";
            this.timeoutTextBox.Size = new System.Drawing.Size(76, 20);
            this.timeoutTextBox.TabIndex = 11;
            this.descriptionToolTip.SetToolTip(this.timeoutTextBox, "The number of seconds to wait for a server to respond before\r\ngiving up and movin" +
        "g on to the next server in the list.\r\n");
            // 
            // searchDnTextBox
            // 
            this.searchDnTextBox.Location = new System.Drawing.Point(110, 97);
            this.searchDnTextBox.Name = "searchDnTextBox";
            this.searchDnTextBox.Size = new System.Drawing.Size(335, 20);
            this.searchDnTextBox.TabIndex = 9;
            this.descriptionToolTip.SetToolTip(this.searchDnTextBox, "Optional.  Used when searching for group \r\nmembership and other search operations" +
        "\r\n");
            // 
            // authzRequireAuthCB
            // 
            this.authzRequireAuthCB.AutoSize = true;
            this.authzRequireAuthCB.Location = new System.Drawing.Point(181, 9);
            this.authzRequireAuthCB.Name = "authzRequireAuthCB";
            this.authzRequireAuthCB.Size = new System.Drawing.Size(202, 17);
            this.authzRequireAuthCB.TabIndex = 19;
            this.authzRequireAuthCB.Text = "Deny when LDAP authentication fails";
            this.descriptionToolTip.SetToolTip(this.authzRequireAuthCB, resources.GetString("authzRequireAuthCB.ToolTip"));
            this.authzRequireAuthCB.UseVisualStyleBackColor = true;
            // 
            // authzAllowOnErrorCB
            // 
            this.authzAllowOnErrorCB.AutoSize = true;
            this.authzAllowOnErrorCB.Location = new System.Drawing.Point(400, 9);
            this.authzAllowOnErrorCB.Name = "authzAllowOnErrorCB";
            this.authzAllowOnErrorCB.Size = new System.Drawing.Size(184, 17);
            this.authzAllowOnErrorCB.TabIndex = 20;
            this.authzAllowOnErrorCB.Text = "Allow when server is unreachable";
            this.descriptionToolTip.SetToolTip(this.authzAllowOnErrorCB, "When the LDAP server is down or another unexpected error \r\noccurs, authorization " +
        "succeeds if this is checked. ");
            this.authzAllowOnErrorCB.UseVisualStyleBackColor = true;
            // 
            // sslCertFileTextBox
            // 
            this.sslCertFileTextBox.Location = new System.Drawing.Point(109, 71);
            this.sslCertFileTextBox.Name = "sslCertFileTextBox";
            this.sslCertFileTextBox.Size = new System.Drawing.Size(463, 20);
            this.sslCertFileTextBox.TabIndex = 8;
            this.descriptionToolTip.SetToolTip(this.sslCertFileTextBox, "Optional:  If left empty, the certificate will be validated\r\nagainst the Windows " +
        "certificate store.");
            // 
            // groupMemberAttrTB
            // 
            this.groupMemberAttrTB.Location = new System.Drawing.Point(535, 149);
            this.groupMemberAttrTB.Name = "groupMemberAttrTB";
            this.groupMemberAttrTB.Size = new System.Drawing.Size(135, 20);
            this.groupMemberAttrTB.TabIndex = 3;
            this.descriptionToolTip.SetToolTip(this.groupMemberAttrTB, "The attribute that stores the group\'s membership.");
            // 
            // groupDNPattern
            // 
            this.groupDNPattern.Location = new System.Drawing.Point(110, 149);
            this.groupDNPattern.Name = "groupDNPattern";
            this.groupDNPattern.Size = new System.Drawing.Size(326, 20);
            this.groupDNPattern.TabIndex = 1;
            this.descriptionToolTip.SetToolTip(this.groupDNPattern, "A pattern that describes how to generate a group DN\r\nfrom a group name.  Use %g a" +
        "s a place holder for the \r\ngroup name.");
            // 
            // allowEmptyPwCB
            // 
            this.allowEmptyPwCB.AutoSize = true;
            this.allowEmptyPwCB.Location = new System.Drawing.Point(6, 6);
            this.allowEmptyPwCB.Name = "allowEmptyPwCB";
            this.allowEmptyPwCB.Size = new System.Drawing.Size(137, 17);
            this.allowEmptyPwCB.TabIndex = 13;
            this.allowEmptyPwCB.Text = "Allow Empty Passwords";
            this.descriptionToolTip.SetToolTip(this.allowEmptyPwCB, "When selected, empty passwords are used in bind\r\nattempts.  Otherwise, an empty p" +
        "assword causes\r\nauthentication to immediately fail.");
            this.allowEmptyPwCB.UseVisualStyleBackColor = true;
            // 
            // dnPatternTextBox
            // 
            this.dnPatternTextBox.Location = new System.Drawing.Point(110, 33);
            this.dnPatternTextBox.Name = "dnPatternTextBox";
            this.dnPatternTextBox.Size = new System.Drawing.Size(463, 20);
            this.dnPatternTextBox.TabIndex = 1;
            this.descriptionToolTip.SetToolTip(this.dnPatternTextBox, "Pattern used to generate a DN from a user name.  Use\r\n%u as a placeholder for the" +
        " user name.");
            // 
            // ldapServerGroupBox
            // 
            this.ldapServerGroupBox.Controls.Add(this.DereferenceComboBox);
            this.ldapServerGroupBox.Controls.Add(this.label8);
            this.ldapServerGroupBox.Controls.Add(this.showPwCB);
            this.ldapServerGroupBox.Controls.Add(this.timeoutTextBox);
            this.ldapServerGroupBox.Controls.Add(this.timeoutLabel);
            this.ldapServerGroupBox.Controls.Add(this.searchPassTextBox);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileBrowseButton);
            this.ldapServerGroupBox.Controls.Add(this.label3);
            this.ldapServerGroupBox.Controls.Add(this.searchDnTextBox);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileTextBox);
            this.ldapServerGroupBox.Controls.Add(this.label2);
            this.ldapServerGroupBox.Controls.Add(this.label1);
            this.ldapServerGroupBox.Controls.Add(this.validateServerCertCheckBox);
            this.ldapServerGroupBox.Controls.Add(this.useSslCheckBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortLabel);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostDescriptionLabel);
            this.ldapServerGroupBox.Controls.Add(this.groupMemberAttrTB);
            this.ldapServerGroupBox.Controls.Add(this.groupDNPattern);
            this.ldapServerGroupBox.Controls.Add(this.label5);
            this.ldapServerGroupBox.Controls.Add(this.label4);
            this.ldapServerGroupBox.Location = new System.Drawing.Point(12, 9);
            this.ldapServerGroupBox.Name = "ldapServerGroupBox";
            this.ldapServerGroupBox.Size = new System.Drawing.Size(676, 207);
            this.ldapServerGroupBox.TabIndex = 3;
            this.ldapServerGroupBox.TabStop = false;
            this.ldapServerGroupBox.Text = "LDAP Server";
            // 
            // DereferenceComboBox
            // 
            this.DereferenceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DereferenceComboBox.FormattingEnabled = true;
            this.DereferenceComboBox.Items.AddRange(new object[] {
            "Never",
            "In searching",
            "When finding base object",
            "Always"});
            this.DereferenceComboBox.Location = new System.Drawing.Point(116, 175);
            this.DereferenceComboBox.Name = "DereferenceComboBox";
            this.DereferenceComboBox.Size = new System.Drawing.Size(175, 21);
            this.DereferenceComboBox.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 178);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Dereference aliases:";
            // 
            // showPwCB
            // 
            this.showPwCB.AutoSize = true;
            this.showPwCB.Location = new System.Drawing.Point(451, 126);
            this.showPwCB.Name = "showPwCB";
            this.showPwCB.Size = new System.Drawing.Size(77, 17);
            this.showPwCB.TabIndex = 12;
            this.showPwCB.Text = "Show Text";
            this.showPwCB.UseVisualStyleBackColor = true;
            this.showPwCB.CheckedChanged += new System.EventHandler(this.showPwCB_CheckedChanged);
            // 
            // searchPassTextBox
            // 
            this.searchPassTextBox.Location = new System.Drawing.Point(110, 123);
            this.searchPassTextBox.Name = "searchPassTextBox";
            this.searchPassTextBox.Size = new System.Drawing.Size(335, 20);
            this.searchPassTextBox.TabIndex = 11;
            this.searchPassTextBox.UseSystemPasswordChar = true;
            // 
            // sslCertFileBrowseButton
            // 
            this.sslCertFileBrowseButton.Location = new System.Drawing.Point(578, 71);
            this.sslCertFileBrowseButton.Name = "sslCertFileBrowseButton";
            this.sslCertFileBrowseButton.Size = new System.Drawing.Size(80, 20);
            this.sslCertFileBrowseButton.TabIndex = 9;
            this.sslCertFileBrowseButton.Text = "Browse...";
            this.sslCertFileBrowseButton.UseVisualStyleBackColor = true;
            this.sslCertFileBrowseButton.Click += new System.EventHandler(this.sslCertFileBrowseButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(442, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Member Attribute";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Group DN Pattern";
            // 
            // searchContextsTextBox
            // 
            this.searchContextsTextBox.Location = new System.Drawing.Point(110, 117);
            this.searchContextsTextBox.Multiline = true;
            this.searchContextsTextBox.Name = "searchContextsTextBox";
            this.searchContextsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchContextsTextBox.Size = new System.Drawing.Size(462, 89);
            this.searchContextsTextBox.TabIndex = 6;
            // 
            // searchFilterTextBox
            // 
            this.searchFilterTextBox.Location = new System.Drawing.Point(110, 91);
            this.searchFilterTextBox.Name = "searchFilterTextBox";
            this.searchFilterTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchFilterTextBox.TabIndex = 4;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(524, 500);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(76, 26);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.authTabPage);
            this.tabControl1.Controls.Add(this.authzTabPage);
            this.tabControl1.Controls.Add(this.gatewayTabPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 234);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(670, 260);
            this.tabControl1.TabIndex = 6;
            // 
            // authTabPage
            // 
            this.authTabPage.Controls.Add(this.searchContextsTextBox);
            this.authTabPage.Controls.Add(this.searchContextsLabel);
            this.authTabPage.Controls.Add(this.allowEmptyPwCB);
            this.authTabPage.Controls.Add(this.dnPatternTextBox);
            this.authTabPage.Controls.Add(this.searchFilterTextBox);
            this.authTabPage.Controls.Add(this.searchFilterLabel);
            this.authTabPage.Controls.Add(this.dnPatternLabel);
            this.authTabPage.Controls.Add(this.searchForDnCheckBox);
            this.authTabPage.Location = new System.Drawing.Point(4, 22);
            this.authTabPage.Name = "authTabPage";
            this.authTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.authTabPage.Size = new System.Drawing.Size(662, 234);
            this.authTabPage.TabIndex = 0;
            this.authTabPage.Text = "Authentication";
            this.authTabPage.UseVisualStyleBackColor = true;
            // 
            // authzTabPage
            // 
            this.authzTabPage.Controls.Add(this.authzAllowOnErrorCB);
            this.authzTabPage.Controls.Add(this.authzRequireAuthCB);
            this.authzTabPage.Controls.Add(this.authzRuleDeleteBtn);
            this.authzTabPage.Controls.Add(this.authzRuleDownBtn);
            this.authzTabPage.Controls.Add(this.authzRuleUpBtn);
            this.authzTabPage.Controls.Add(this.label7);
            this.authzTabPage.Controls.Add(this.authzDefaultDenyRB);
            this.authzTabPage.Controls.Add(this.authzDefaultAllowRB);
            this.authzTabPage.Controls.Add(this.authzRuleAddButton);
            this.authzTabPage.Controls.Add(this.authzRulesListBox);
            this.authzTabPage.Controls.Add(this.authzRuleActionComboBox);
            this.authzTabPage.Controls.Add(this.authzRuleGroupTB);
            this.authzTabPage.Controls.Add(this.authzRuleMemberComboBox);
            this.authzTabPage.Location = new System.Drawing.Point(4, 22);
            this.authzTabPage.Name = "authzTabPage";
            this.authzTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.authzTabPage.Size = new System.Drawing.Size(662, 234);
            this.authzTabPage.TabIndex = 1;
            this.authzTabPage.Text = "Authorization";
            this.authzTabPage.UseVisualStyleBackColor = true;
            // 
            // authzRuleDeleteBtn
            // 
            this.authzRuleDeleteBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.delete;
            this.authzRuleDeleteBtn.Location = new System.Drawing.Point(583, 82);
            this.authzRuleDeleteBtn.Name = "authzRuleDeleteBtn";
            this.authzRuleDeleteBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleDeleteBtn.TabIndex = 18;
            this.authzRuleDeleteBtn.UseVisualStyleBackColor = true;
            this.authzRuleDeleteBtn.Click += new System.EventHandler(this.authzRuleDeleteBtn_Click);
            // 
            // authzRuleDownBtn
            // 
            this.authzRuleDownBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.DownArrowSolid;
            this.authzRuleDownBtn.Location = new System.Drawing.Point(583, 123);
            this.authzRuleDownBtn.Name = "authzRuleDownBtn";
            this.authzRuleDownBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleDownBtn.TabIndex = 17;
            this.authzRuleDownBtn.UseVisualStyleBackColor = true;
            this.authzRuleDownBtn.Click += new System.EventHandler(this.authzRuleDownBtn_Click);
            // 
            // authzRuleUpBtn
            // 
            this.authzRuleUpBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.UpArrowSolid;
            this.authzRuleUpBtn.Location = new System.Drawing.Point(583, 42);
            this.authzRuleUpBtn.Name = "authzRuleUpBtn";
            this.authzRuleUpBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleUpBtn.TabIndex = 16;
            this.authzRuleUpBtn.UseVisualStyleBackColor = true;
            this.authzRuleUpBtn.Click += new System.EventHandler(this.authzRuleUpBtn_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Default:";
            // 
            // authzDefaultDenyRB
            // 
            this.authzDefaultDenyRB.AutoSize = true;
            this.authzDefaultDenyRB.Location = new System.Drawing.Point(112, 7);
            this.authzDefaultDenyRB.Name = "authzDefaultDenyRB";
            this.authzDefaultDenyRB.Size = new System.Drawing.Size(50, 17);
            this.authzDefaultDenyRB.TabIndex = 14;
            this.authzDefaultDenyRB.TabStop = true;
            this.authzDefaultDenyRB.Text = "Deny";
            this.authzDefaultDenyRB.UseVisualStyleBackColor = true;
            // 
            // authzDefaultAllowRB
            // 
            this.authzDefaultAllowRB.AutoSize = true;
            this.authzDefaultAllowRB.Location = new System.Drawing.Point(56, 7);
            this.authzDefaultAllowRB.Name = "authzDefaultAllowRB";
            this.authzDefaultAllowRB.Size = new System.Drawing.Size(50, 17);
            this.authzDefaultAllowRB.TabIndex = 13;
            this.authzDefaultAllowRB.TabStop = true;
            this.authzDefaultAllowRB.Text = "Allow";
            this.authzDefaultAllowRB.UseVisualStyleBackColor = true;
            // 
            // authzRuleAddButton
            // 
            this.authzRuleAddButton.Location = new System.Drawing.Point(503, 185);
            this.authzRuleAddButton.Name = "authzRuleAddButton";
            this.authzRuleAddButton.Size = new System.Drawing.Size(71, 23);
            this.authzRuleAddButton.TabIndex = 12;
            this.authzRuleAddButton.Text = "Add Rule";
            this.authzRuleAddButton.UseVisualStyleBackColor = true;
            this.authzRuleAddButton.Click += new System.EventHandler(this.authzRuleAddButton_Click);
            // 
            // authzRulesListBox
            // 
            this.authzRulesListBox.FormattingEnabled = true;
            this.authzRulesListBox.Location = new System.Drawing.Point(6, 32);
            this.authzRulesListBox.Name = "authzRulesListBox";
            this.authzRulesListBox.Size = new System.Drawing.Size(571, 147);
            this.authzRulesListBox.TabIndex = 11;
            // 
            // authzRuleActionComboBox
            // 
            this.authzRuleActionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authzRuleActionComboBox.FormattingEnabled = true;
            this.authzRuleActionComboBox.Items.AddRange(new object[] {
            "allow.",
            "deny."});
            this.authzRuleActionComboBox.Location = new System.Drawing.Point(432, 185);
            this.authzRuleActionComboBox.Name = "authzRuleActionComboBox";
            this.authzRuleActionComboBox.Size = new System.Drawing.Size(65, 21);
            this.authzRuleActionComboBox.TabIndex = 6;
            // 
            // authzRuleGroupTB
            // 
            this.authzRuleGroupTB.Location = new System.Drawing.Point(194, 185);
            this.authzRuleGroupTB.Name = "authzRuleGroupTB";
            this.authzRuleGroupTB.Size = new System.Drawing.Size(232, 20);
            this.authzRuleGroupTB.TabIndex = 5;
            // 
            // authzRuleMemberComboBox
            // 
            this.authzRuleMemberComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authzRuleMemberComboBox.FormattingEnabled = true;
            this.authzRuleMemberComboBox.Items.AddRange(new object[] {
            "If member of LDAP group:",
            "If not member of LDAP group:"});
            this.authzRuleMemberComboBox.Location = new System.Drawing.Point(11, 185);
            this.authzRuleMemberComboBox.Name = "authzRuleMemberComboBox";
            this.authzRuleMemberComboBox.Size = new System.Drawing.Size(177, 21);
            this.authzRuleMemberComboBox.TabIndex = 4;
            // 
            // gatewayTabPage
            // 
            this.gatewayTabPage.Controls.Add(this.gatwayRemoteGroupTB);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleGroupMemberCB);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleDeleteBtn);
            this.gatewayTabPage.Controls.Add(this.label6);
            this.gatewayTabPage.Controls.Add(this.gatewayLocalGroupTB);
            this.gatewayTabPage.Controls.Add(this.gatewayRulesListBox);
            this.gatewayTabPage.Controls.Add(this.addGatewayGroupRuleButton);
            this.gatewayTabPage.Location = new System.Drawing.Point(4, 22);
            this.gatewayTabPage.Name = "gatewayTabPage";
            this.gatewayTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.gatewayTabPage.Size = new System.Drawing.Size(662, 234);
            this.gatewayTabPage.TabIndex = 2;
            this.gatewayTabPage.Text = "Gateway";
            this.gatewayTabPage.UseVisualStyleBackColor = true;
            // 
            // gatwayRemoteGroupTB
            // 
            this.gatwayRemoteGroupTB.Location = new System.Drawing.Point(209, 207);
            this.gatwayRemoteGroupTB.Name = "gatwayRemoteGroupTB";
            this.gatwayRemoteGroupTB.Size = new System.Drawing.Size(131, 20);
            this.gatwayRemoteGroupTB.TabIndex = 21;
            // 
            // gatewayRuleGroupMemberCB
            // 
            this.gatewayRuleGroupMemberCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gatewayRuleGroupMemberCB.FormattingEnabled = true;
            this.gatewayRuleGroupMemberCB.Items.AddRange(new object[] {
            "If member of LDAP group:",
            "If not member of LDAP group:",
            "Always:"});
            this.gatewayRuleGroupMemberCB.Location = new System.Drawing.Point(10, 207);
            this.gatewayRuleGroupMemberCB.Name = "gatewayRuleGroupMemberCB";
            this.gatewayRuleGroupMemberCB.Size = new System.Drawing.Size(185, 21);
            this.gatewayRuleGroupMemberCB.TabIndex = 20;
            this.gatewayRuleGroupMemberCB.SelectedIndexChanged += new System.EventHandler(this.gatewayRuleGroupMemberCB_SelectedIndexChanged);
            // 
            // gatewayRuleDeleteBtn
            // 
            this.gatewayRuleDeleteBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.delete;
            this.gatewayRuleDeleteBtn.Location = new System.Drawing.Point(622, 89);
            this.gatewayRuleDeleteBtn.Name = "gatewayRuleDeleteBtn";
            this.gatewayRuleDeleteBtn.Size = new System.Drawing.Size(32, 34);
            this.gatewayRuleDeleteBtn.TabIndex = 19;
            this.gatewayRuleDeleteBtn.UseVisualStyleBackColor = true;
            this.gatewayRuleDeleteBtn.Click += new System.EventHandler(this.gatewayRuleDeleteBtn_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(346, 210);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "add to local group:";
            // 
            // gatewayLocalGroupTB
            // 
            this.gatewayLocalGroupTB.Location = new System.Drawing.Point(447, 207);
            this.gatewayLocalGroupTB.Name = "gatewayLocalGroupTB";
            this.gatewayLocalGroupTB.Size = new System.Drawing.Size(105, 20);
            this.gatewayLocalGroupTB.TabIndex = 7;
            // 
            // gatewayRulesListBox
            // 
            this.gatewayRulesListBox.FormattingEnabled = true;
            this.gatewayRulesListBox.Location = new System.Drawing.Point(6, 6);
            this.gatewayRulesListBox.Name = "gatewayRulesListBox";
            this.gatewayRulesListBox.Size = new System.Drawing.Size(610, 186);
            this.gatewayRulesListBox.TabIndex = 9;
            // 
            // addGatewayGroupRuleButton
            // 
            this.addGatewayGroupRuleButton.Location = new System.Drawing.Point(558, 205);
            this.addGatewayGroupRuleButton.Name = "addGatewayGroupRuleButton";
            this.addGatewayGroupRuleButton.Size = new System.Drawing.Size(71, 23);
            this.addGatewayGroupRuleButton.TabIndex = 8;
            this.addGatewayGroupRuleButton.Text = "Add Rule";
            this.addGatewayGroupRuleButton.UseVisualStyleBackColor = true;
            this.addGatewayGroupRuleButton.Click += new System.EventHandler(this.addGatewayGroupRuleButton_Click);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 538);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.ldapServerGroupBox);
            this.Controls.Add(this.saveButton);
            this.Name = "Configuration";
            this.Text = "LDAP Plugin Settings";
            this.ldapServerGroupBox.ResumeLayout(false);
            this.ldapServerGroupBox.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.authTabPage.ResumeLayout(false);
            this.authTabPage.PerformLayout();
            this.authzTabPage.ResumeLayout(false);
            this.authzTabPage.PerformLayout();
            this.gatewayTabPage.ResumeLayout(false);
            this.gatewayTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label ldapHostDescriptionLabel;
        private System.Windows.Forms.TextBox ldapHostTextBox;
        private System.Windows.Forms.ToolTip descriptionToolTip;
        private System.Windows.Forms.TextBox ldapPortTextBox;
        private System.Windows.Forms.Label ldapPortLabel;
        private System.Windows.Forms.GroupBox ldapServerGroupBox;
        private System.Windows.Forms.CheckBox useSslCheckBox;
        private System.Windows.Forms.Button sslCertFileBrowseButton;
        private System.Windows.Forms.TextBox sslCertFileTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox validateServerCertCheckBox;
        private System.Windows.Forms.Label searchFilterLabel;
        private System.Windows.Forms.CheckBox searchForDnCheckBox;
        private System.Windows.Forms.TextBox dnPatternTextBox;
        private System.Windows.Forms.Label dnPatternLabel;
        private System.Windows.Forms.TextBox searchContextsTextBox;
        private System.Windows.Forms.Label searchContextsLabel;
        private System.Windows.Forms.TextBox searchFilterTextBox;
        private System.Windows.Forms.TextBox searchDnTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox timeoutTextBox;
        private System.Windows.Forms.Label timeoutLabel;
        private System.Windows.Forms.TextBox searchPassTextBox;
        private System.Windows.Forms.CheckBox showPwCB;
        private System.Windows.Forms.CheckBox allowEmptyPwCB;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage authTabPage;
        private System.Windows.Forms.TabPage authzTabPage;
        private System.Windows.Forms.TextBox groupDNPattern;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox authzRuleGroupTB;
        private System.Windows.Forms.ComboBox authzRuleMemberComboBox;
        private System.Windows.Forms.TextBox groupMemberAttrTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button addGatewayGroupRuleButton;
        private System.Windows.Forms.TextBox gatewayLocalGroupTB;
        private System.Windows.Forms.ComboBox authzRuleActionComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox gatewayRulesListBox;
        private System.Windows.Forms.ListBox authzRulesListBox;
        private System.Windows.Forms.Button authzRuleAddButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton authzDefaultDenyRB;
        private System.Windows.Forms.RadioButton authzDefaultAllowRB;
        private System.Windows.Forms.Button authzRuleDownBtn;
        private System.Windows.Forms.Button authzRuleUpBtn;
        private System.Windows.Forms.Button gatewayRuleDeleteBtn;
        private System.Windows.Forms.Button authzRuleDeleteBtn;
        private System.Windows.Forms.TabPage gatewayTabPage;
        private System.Windows.Forms.ComboBox gatewayRuleGroupMemberCB;
        private System.Windows.Forms.TextBox gatwayRemoteGroupTB;
        private System.Windows.Forms.CheckBox authzRequireAuthCB;
        private System.Windows.Forms.CheckBox authzAllowOnErrorCB;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox DereferenceComboBox;
    }
}
