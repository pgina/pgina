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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.timeoutLabel = new System.Windows.Forms.Label();
            this.timeoutTextBox = new System.Windows.Forms.TextBox();
            this.searchDnTextBox = new System.Windows.Forms.TextBox();
            this.authzRequireAuthCB = new System.Windows.Forms.CheckBox();
            this.authzAllowOnErrorCB = new System.Windows.Forms.CheckBox();
            this.sslCertFileTextBox = new System.Windows.Forms.TextBox();
            this.allowEmptyPwCB = new System.Windows.Forms.CheckBox();
            this.searchContextsLabel = new System.Windows.Forms.Label();
            this.dnPatternTextBox = new System.Windows.Forms.TextBox();
            this.searchFilterLabel = new System.Windows.Forms.Label();
            this.dnPatternLabel = new System.Windows.Forms.Label();
            this.searchForDnCheckBox = new System.Windows.Forms.CheckBox();
            this.useTlsCheckBox = new System.Windows.Forms.CheckBox();
            this.ldapServerGroupBox = new System.Windows.Forms.GroupBox();
            this.useAuthBindForAuthzAndGatewayCb = new System.Windows.Forms.CheckBox();
            this.showPwCB = new System.Windows.Forms.CheckBox();
            this.searchPassTextBox = new System.Windows.Forms.TextBox();
            this.sslCertFileBrowseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.authTabPage = new System.Windows.Forms.TabPage();
            this.authtab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.searchContextsTextBox = new System.Windows.Forms.TextBox();
            this.searchFilterTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authzTabPage = new System.Windows.Forms.TabPage();
            this.AuthPathLabel = new System.Windows.Forms.Label();
            this.AuthFilterLabel = new System.Windows.Forms.Label();
            this.authzRuleFilter = new System.Windows.Forms.TextBox();
            this.authzRuleScope = new System.Windows.Forms.ComboBox();
            this.authzRuleDeleteBtn = new System.Windows.Forms.Button();
            this.authzRuleDownBtn = new System.Windows.Forms.Button();
            this.authzRuleUpBtn = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.authzDefaultDenyRB = new System.Windows.Forms.RadioButton();
            this.authzDefaultAllowRB = new System.Windows.Forms.RadioButton();
            this.authzRuleAddButton = new System.Windows.Forms.Button();
            this.authzRulesListBox = new System.Windows.Forms.ListBox();
            this.authzRuleActionComboBox = new System.Windows.Forms.ComboBox();
            this.authzRulePathTB = new System.Windows.Forms.TextBox();
            this.authzRuleMemberComboBox = new System.Windows.Forms.ComboBox();
            this.gatewayTabPage = new System.Windows.Forms.TabPage();
            this.GatewayPathLabel = new System.Windows.Forms.Label();
            this.GatewayFilterLabel = new System.Windows.Forms.Label();
            this.gatewayRuleFilter = new System.Windows.Forms.TextBox();
            this.gatewayRuleScope = new System.Windows.Forms.ComboBox();
            this.gatwayRulePathTB = new System.Windows.Forms.TextBox();
            this.gatewayRuleGroupMemberCB = new System.Windows.Forms.ComboBox();
            this.gatewayRuleDeleteBtn = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.gatewayRuleLocalGroupTB = new System.Windows.Forms.TextBox();
            this.gatewayRulesListBox = new System.Windows.Forms.ListBox();
            this.gatewayRuleAddButton = new System.Windows.Forms.Button();
            this.tabPageChangePassword = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.passwordAttributesDGV = new System.Windows.Forms.DataGridView();
            this.help = new System.Windows.Forms.Button();
            this.ldapServerGroupBox.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.authTabPage.SuspendLayout();
            this.authtab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.authzTabPage.SuspendLayout();
            this.gatewayTabPage.SuspendLayout();
            this.tabPageChangePassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.passwordAttributesDGV)).BeginInit();
            this.SuspendLayout();
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(601, 464);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(81, 26);
            this.saveButton.TabIndex = 4;
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
            this.ldapHostDescriptionLabel.TabIndex = 0;
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
            this.ldapHostTextBox.TabIndex = 1;
            this.descriptionToolTip.SetToolTip(this.ldapHostTextBox, "A whitespace separated list of hostnames or IP addresses.");
            //
            // validateServerCertCheckBox
            //
            this.validateServerCertCheckBox.AutoSize = true;
            this.validateServerCertCheckBox.Location = new System.Drawing.Point(512, 47);
            this.validateServerCertCheckBox.Name = "validateServerCertCheckBox";
            this.validateServerCertCheckBox.Size = new System.Drawing.Size(148, 17);
            this.validateServerCertCheckBox.TabIndex = 8;
            this.validateServerCertCheckBox.Text = "Validate Server Certificate";
            this.descriptionToolTip.SetToolTip(this.validateServerCertCheckBox, "Whether or not to verify the server\'s SSL certificate when connecting.");
            this.validateServerCertCheckBox.UseVisualStyleBackColor = true;
            this.validateServerCertCheckBox.CheckedChanged += new System.EventHandler(this.validateServerCertCheckBox_CheckedChanged);
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Certificate";
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
            this.ldapPortLabel.TabIndex = 2;
            this.ldapPortLabel.Text = "LDAP Port";
            this.descriptionToolTip.SetToolTip(this.ldapPortLabel, "The port number");
            //
            // useSslCheckBox
            //
            this.useSslCheckBox.AutoSize = true;
            this.useSslCheckBox.Location = new System.Drawing.Point(363, 47);
            this.useSslCheckBox.Name = "useSslCheckBox";
            this.useSslCheckBox.Size = new System.Drawing.Size(68, 17);
            this.useSslCheckBox.TabIndex = 6;
            this.useSslCheckBox.Text = "Use SSL";
            this.descriptionToolTip.SetToolTip(this.useSslCheckBox, "Whether or not to use SSL encryption.");
            this.useSslCheckBox.UseVisualStyleBackColor = true;
            this.useSslCheckBox.CheckedChanged += new System.EventHandler(this.useSslCheckBox_CheckedChanged);
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 12;
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
            this.label3.TabIndex = 15;
            this.label3.Text = "Search Password";
            this.descriptionToolTip.SetToolTip(this.label3, "The password to use when searching");
            //
            // timeoutLabel
            //
            this.timeoutLabel.AutoSize = true;
            this.timeoutLabel.Location = new System.Drawing.Point(206, 48);
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.Size = new System.Drawing.Size(45, 13);
            this.timeoutLabel.TabIndex = 4;
            this.timeoutLabel.Text = "Timeout";
            this.descriptionToolTip.SetToolTip(this.timeoutLabel, "The number of seconds to wait for a server to respond before giving up \r\nand movi" +
                    "ng on to the next server.");
            //
            // timeoutTextBox
            //
            this.timeoutTextBox.Location = new System.Drawing.Point(265, 45);
            this.timeoutTextBox.Name = "timeoutTextBox";
            this.timeoutTextBox.Size = new System.Drawing.Size(76, 20);
            this.timeoutTextBox.TabIndex = 5;
            this.descriptionToolTip.SetToolTip(this.timeoutTextBox, "The number of seconds to wait for a server to respond before\r\ngiving up and movin" +
                    "g on to the next server in the list.\r\n");
            //
            // searchDnTextBox
            //
            this.searchDnTextBox.Location = new System.Drawing.Point(110, 97);
            this.searchDnTextBox.Name = "searchDnTextBox";
            this.searchDnTextBox.Size = new System.Drawing.Size(296, 20);
            this.searchDnTextBox.TabIndex = 13;
            this.descriptionToolTip.SetToolTip(this.searchDnTextBox, "Optional.  Used when searching for group \r\nmembership and other search operations" +
                    "\r\n");
            //
            // authzRequireAuthCB
            //
            this.authzRequireAuthCB.AutoSize = true;
            this.authzRequireAuthCB.Location = new System.Drawing.Point(181, 9);
            this.authzRequireAuthCB.Name = "authzRequireAuthCB";
            this.authzRequireAuthCB.Size = new System.Drawing.Size(202, 17);
            this.authzRequireAuthCB.TabIndex = 3;
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
            this.authzAllowOnErrorCB.TabIndex = 4;
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
            this.sslCertFileTextBox.TabIndex = 10;
            this.descriptionToolTip.SetToolTip(this.sslCertFileTextBox, "Optional:  If left empty, the certificate will be validated against the Local Computer certificate store.\r\nPut in MATCH if you have stored the certificate in the Local Computer certification storage.");
            //
            // allowEmptyPwCB
            //
            this.allowEmptyPwCB.AutoSize = true;
            this.allowEmptyPwCB.Location = new System.Drawing.Point(6, 6);
            this.allowEmptyPwCB.Name = "allowEmptyPwCB";
            this.allowEmptyPwCB.Size = new System.Drawing.Size(137, 17);
            this.allowEmptyPwCB.TabIndex = 0;
            this.allowEmptyPwCB.Text = "Allow Empty Passwords";
            this.descriptionToolTip.SetToolTip(this.allowEmptyPwCB, "When selected, empty passwords are used in bind\r\nattempts.  Otherwise, an empty p" +
                    "assword causes\r\nauthentication to immediately fail.");
            this.allowEmptyPwCB.UseVisualStyleBackColor = true;
            //
            // searchContextsLabel
            //
            this.searchContextsLabel.AutoSize = true;
            this.searchContextsLabel.Location = new System.Drawing.Point(3, 94);
            this.searchContextsLabel.Name = "searchContextsLabel";
            this.searchContextsLabel.Size = new System.Drawing.Size(91, 13);
            this.searchContextsLabel.TabIndex = 6;
            this.searchContextsLabel.Text = "Search Context(s)";
            this.descriptionToolTip.SetToolTip(this.searchContextsLabel, "A list of one or more DNs (one per line) that indicate\r\nroots of LDAP subtrees th" +
                    "at will be searched.");
            //
            // dnPatternTextBox
            //
            this.dnPatternTextBox.Location = new System.Drawing.Point(107, 33);
            this.dnPatternTextBox.Name = "dnPatternTextBox";
            this.dnPatternTextBox.Size = new System.Drawing.Size(543, 20);
            this.dnPatternTextBox.TabIndex = 3;
            this.descriptionToolTip.SetToolTip(this.dnPatternTextBox, "Pattern used to generate a DN from a user name.  Use\r\n%u as a placeholder for the" +
                    " user name.");
            //
            // searchFilterLabel
            //
            this.searchFilterLabel.AutoSize = true;
            this.searchFilterLabel.Location = new System.Drawing.Point(3, 68);
            this.searchFilterLabel.Name = "searchFilterLabel";
            this.searchFilterLabel.Size = new System.Drawing.Size(66, 13);
            this.searchFilterLabel.TabIndex = 4;
            this.searchFilterLabel.Text = "Search Filter";
            this.descriptionToolTip.SetToolTip(this.searchFilterLabel, "The filter to use when searching.\r\n%u is replaced with the user name\r\n");
            //
            // dnPatternLabel
            //
            this.dnPatternLabel.AutoSize = true;
            this.dnPatternLabel.Location = new System.Drawing.Point(3, 26);
            this.dnPatternLabel.Name = "dnPatternLabel";
            this.dnPatternLabel.Size = new System.Drawing.Size(85, 13);
            this.dnPatternLabel.TabIndex = 1;
            this.dnPatternLabel.Text = "User DN Pattern";
            this.descriptionToolTip.SetToolTip(this.dnPatternLabel, "The pattern to use when creating a DN from a user name.\r\n%u can be used to indica" +
                    "te the user name.\r\n");
            //
            // searchForDnCheckBox
            //
            this.searchForDnCheckBox.AutoSize = true;
            this.searchForDnCheckBox.Location = new System.Drawing.Point(7, 42);
            this.searchForDnCheckBox.Name = "searchForDnCheckBox";
            this.searchForDnCheckBox.Size = new System.Drawing.Size(94, 17);
            this.searchForDnCheckBox.TabIndex = 2;
            this.searchForDnCheckBox.Text = "Search for DN";
            this.descriptionToolTip.SetToolTip(this.searchForDnCheckBox, "Whether or not to search a set of LDAP trees for the\r\nDN rather than using the ab" +
                    "ove pattern.");
            this.searchForDnCheckBox.UseVisualStyleBackColor = true;
            this.searchForDnCheckBox.CheckedChanged += new System.EventHandler(this.searchForDnCheckBox_CheckedChanged);
            //
            // useTlsCheckBox
            //
            this.useTlsCheckBox.AutoSize = true;
            this.useTlsCheckBox.Location = new System.Drawing.Point(437, 47);
            this.useTlsCheckBox.Name = "useTlsCheckBox";
            this.useTlsCheckBox.Size = new System.Drawing.Size(68, 17);
            this.useTlsCheckBox.TabIndex = 7;
            this.useTlsCheckBox.Text = "StartTLS";
            this.descriptionToolTip.SetToolTip(this.useTlsCheckBox, "Use StartTLS if your LDAP server is not using an alternative (636) port for encry" +
                    "ption\nYou can run encryption and clear text over 389");
            this.useTlsCheckBox.UseVisualStyleBackColor = true;
            this.useTlsCheckBox.CheckedChanged += new System.EventHandler(this.useTlsCheckBox_CheckedChanged);
            //
            // ldapServerGroupBox
            //
            this.ldapServerGroupBox.Controls.Add(this.useAuthBindForAuthzAndGatewayCb);
            this.ldapServerGroupBox.Controls.Add(this.useTlsCheckBox);
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
            this.ldapServerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ldapServerGroupBox.Name = "ldapServerGroupBox";
            this.ldapServerGroupBox.Size = new System.Drawing.Size(676, 151);
            this.ldapServerGroupBox.TabIndex = 0;
            this.ldapServerGroupBox.TabStop = false;
            this.ldapServerGroupBox.Text = "LDAP Server";
            //
            // useAuthBindForAuthzAndGatewayCb
            //
            this.useAuthBindForAuthzAndGatewayCb.AutoSize = true;
            this.useAuthBindForAuthzAndGatewayCb.Location = new System.Drawing.Point(412, 100);
            this.useAuthBindForAuthzAndGatewayCb.Name = "useAuthBindForAuthzAndGatewayCb";
            this.useAuthBindForAuthzAndGatewayCb.Size = new System.Drawing.Size(258, 17);
            this.useAuthBindForAuthzAndGatewayCb.TabIndex = 14;
            this.useAuthBindForAuthzAndGatewayCb.Text = "bind with user credentials instead of anonymously";
            this.useAuthBindForAuthzAndGatewayCb.UseVisualStyleBackColor = true;
            this.useAuthBindForAuthzAndGatewayCb.CheckedChanged += new System.EventHandler(this.useAuthBindForAuthzAndGatewayCb_CheckedChanged);
            //
            // showPwCB
            //
            this.showPwCB.AutoSize = true;
            this.showPwCB.Location = new System.Drawing.Point(412, 126);
            this.showPwCB.Name = "showPwCB";
            this.showPwCB.Size = new System.Drawing.Size(77, 17);
            this.showPwCB.TabIndex = 17;
            this.showPwCB.Text = "Show Text";
            this.showPwCB.UseVisualStyleBackColor = true;
            this.showPwCB.CheckedChanged += new System.EventHandler(this.showPwCB_CheckedChanged);
            //
            // searchPassTextBox
            //
            this.searchPassTextBox.Location = new System.Drawing.Point(110, 123);
            this.searchPassTextBox.Name = "searchPassTextBox";
            this.searchPassTextBox.Size = new System.Drawing.Size(296, 20);
            this.searchPassTextBox.TabIndex = 16;
            this.searchPassTextBox.UseSystemPasswordChar = true;
            //
            // sslCertFileBrowseButton
            //
            this.sslCertFileBrowseButton.Location = new System.Drawing.Point(578, 71);
            this.sslCertFileBrowseButton.Name = "sslCertFileBrowseButton";
            this.sslCertFileBrowseButton.Size = new System.Drawing.Size(80, 20);
            this.sslCertFileBrowseButton.TabIndex = 11;
            this.sslCertFileBrowseButton.Text = "Browse...";
            this.sslCertFileBrowseButton.UseVisualStyleBackColor = true;
            this.sslCertFileBrowseButton.Click += new System.EventHandler(this.sslCertFileBrowseButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.Location = new System.Drawing.Point(519, 464);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(76, 26);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.authTabPage);
            this.tabControl1.Controls.Add(this.authzTabPage);
            this.tabControl1.Controls.Add(this.gatewayTabPage);
            this.tabControl1.Controls.Add(this.tabPageChangePassword);
            this.tabControl1.Location = new System.Drawing.Point(12, 169);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(670, 289);
            this.tabControl1.TabIndex = 1;
            //
            // authTabPage
            //
            this.authTabPage.Controls.Add(this.authtab);
            this.authTabPage.Location = new System.Drawing.Point(4, 22);
            this.authTabPage.Name = "authTabPage";
            this.authTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.authTabPage.Size = new System.Drawing.Size(662, 263);
            this.authTabPage.TabIndex = 0;
            this.authTabPage.Text = "Authentication";
            this.authTabPage.UseVisualStyleBackColor = true;
            //
            // authtab
            //
            this.authtab.Controls.Add(this.tabPage1);
            this.authtab.Controls.Add(this.tabPage2);
            this.authtab.Location = new System.Drawing.Point(0, 0);
            this.authtab.Name = "authtab";
            this.authtab.SelectedIndex = 0;
            this.authtab.Size = new System.Drawing.Size(666, 263);
            this.authtab.TabIndex = 0;
            //
            // tabPage1
            //
            this.tabPage1.Controls.Add(this.searchContextsTextBox);
            this.tabPage1.Controls.Add(this.searchContextsLabel);
            this.tabPage1.Controls.Add(this.dnPatternTextBox);
            this.tabPage1.Controls.Add(this.searchFilterTextBox);
            this.tabPage1.Controls.Add(this.searchFilterLabel);
            this.tabPage1.Controls.Add(this.dnPatternLabel);
            this.tabPage1.Controls.Add(this.searchForDnCheckBox);
            this.tabPage1.Controls.Add(this.allowEmptyPwCB);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(658, 237);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Bind";
            this.tabPage1.UseVisualStyleBackColor = true;
            //
            // searchContextsTextBox
            //
            this.searchContextsTextBox.Location = new System.Drawing.Point(107, 91);
            this.searchContextsTextBox.Multiline = true;
            this.searchContextsTextBox.Name = "searchContextsTextBox";
            this.searchContextsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchContextsTextBox.Size = new System.Drawing.Size(543, 140);
            this.searchContextsTextBox.TabIndex = 7;
            //
            // searchFilterTextBox
            //
            this.searchFilterTextBox.Location = new System.Drawing.Point(107, 65);
            this.searchFilterTextBox.Name = "searchFilterTextBox";
            this.searchFilterTextBox.Size = new System.Drawing.Size(543, 20);
            this.searchFilterTextBox.TabIndex = 5;
            //
            // tabPage2
            //
            this.tabPage2.Controls.Add(this.dataGridView1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(658, 237);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Attribute converter";
            this.tabPage2.UseVisualStyleBackColor = true;
            //
            // dataGridView1
            //
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridView1.Location = new System.Drawing.Point(6, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(645, 225);
            this.dataGridView1.TabIndex = 17;
            //
            // Column1
            //
            this.Column1.HeaderText = "Windows";
            this.Column1.Name = "Column1";
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column1.ToolTipText = "available attributes";
            this.Column1.Width = 250;
            //
            // Column2
            //
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column2.HeaderText = "LDAP";
            this.Column2.Name = "Column2";
            this.Column2.ToolTipText = "LDAP attribute (case sensitive)";
            //
            // authzTabPage
            //
            this.authzTabPage.Controls.Add(this.AuthPathLabel);
            this.authzTabPage.Controls.Add(this.AuthFilterLabel);
            this.authzTabPage.Controls.Add(this.authzRuleFilter);
            this.authzTabPage.Controls.Add(this.authzRuleScope);
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
            this.authzTabPage.Controls.Add(this.authzRulePathTB);
            this.authzTabPage.Controls.Add(this.authzRuleMemberComboBox);
            this.authzTabPage.Location = new System.Drawing.Point(4, 22);
            this.authzTabPage.Name = "authzTabPage";
            this.authzTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.authzTabPage.Size = new System.Drawing.Size(662, 263);
            this.authzTabPage.TabIndex = 1;
            this.authzTabPage.Text = "Authorization";
            this.authzTabPage.UseVisualStyleBackColor = true;
            //
            // AuthPathLabel
            //
            this.AuthPathLabel.AutoSize = true;
            this.AuthPathLabel.Location = new System.Drawing.Point(158, 214);
            this.AuthPathLabel.Name = "AuthPathLabel";
            this.AuthPathLabel.Size = new System.Drawing.Size(32, 13);
            this.AuthPathLabel.TabIndex = 11;
            this.AuthPathLabel.Text = "Path:";
            //
            // AuthFilterLabel
            //
            this.AuthFilterLabel.AutoSize = true;
            this.AuthFilterLabel.Location = new System.Drawing.Point(158, 239);
            this.AuthFilterLabel.Name = "AuthFilterLabel";
            this.AuthFilterLabel.Size = new System.Drawing.Size(32, 13);
            this.AuthFilterLabel.TabIndex = 13;
            this.AuthFilterLabel.Text = "Filter:";
            //
            // authzRuleFilter
            //
            this.authzRuleFilter.Location = new System.Drawing.Point(195, 236);
            this.authzRuleFilter.Name = "authzRuleFilter";
            this.authzRuleFilter.Size = new System.Drawing.Size(273, 20);
            this.authzRuleFilter.TabIndex = 14;
            //
            // authzRuleScope
            //
            this.authzRuleScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authzRuleScope.FormattingEnabled = true;
            this.authzRuleScope.Items.AddRange(new object[] {
            "Base",
            "OneLevel",
            "Subtree"});
            this.authzRuleScope.Location = new System.Drawing.Point(77, 211);
            this.authzRuleScope.Name = "authzRuleScope";
            this.authzRuleScope.Size = new System.Drawing.Size(76, 21);
            this.authzRuleScope.TabIndex = 10;
            //
            // authzRuleDeleteBtn
            //
            this.authzRuleDeleteBtn.Enabled = false;
            this.authzRuleDeleteBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.delete;
            this.authzRuleDeleteBtn.Location = new System.Drawing.Point(622, 82);
            this.authzRuleDeleteBtn.Name = "authzRuleDeleteBtn";
            this.authzRuleDeleteBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleDeleteBtn.TabIndex = 7;
            this.authzRuleDeleteBtn.UseVisualStyleBackColor = true;
            this.authzRuleDeleteBtn.Click += new System.EventHandler(this.authzRuleDeleteBtn_Click);
            //
            // authzRuleDownBtn
            //
            this.authzRuleDownBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.DownArrowSolid;
            this.authzRuleDownBtn.Location = new System.Drawing.Point(622, 123);
            this.authzRuleDownBtn.Name = "authzRuleDownBtn";
            this.authzRuleDownBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleDownBtn.TabIndex = 8;
            this.authzRuleDownBtn.UseVisualStyleBackColor = true;
            this.authzRuleDownBtn.Click += new System.EventHandler(this.authzRuleDownBtn_Click);
            //
            // authzRuleUpBtn
            //
            this.authzRuleUpBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.UpArrowSolid;
            this.authzRuleUpBtn.Location = new System.Drawing.Point(622, 42);
            this.authzRuleUpBtn.Name = "authzRuleUpBtn";
            this.authzRuleUpBtn.Size = new System.Drawing.Size(32, 34);
            this.authzRuleUpBtn.TabIndex = 6;
            this.authzRuleUpBtn.UseVisualStyleBackColor = true;
            this.authzRuleUpBtn.Click += new System.EventHandler(this.authzRuleUpBtn_Click);
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Default:";
            //
            // authzDefaultDenyRB
            //
            this.authzDefaultDenyRB.AutoSize = true;
            this.authzDefaultDenyRB.Location = new System.Drawing.Point(112, 7);
            this.authzDefaultDenyRB.Name = "authzDefaultDenyRB";
            this.authzDefaultDenyRB.Size = new System.Drawing.Size(50, 17);
            this.authzDefaultDenyRB.TabIndex = 2;
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
            this.authzDefaultAllowRB.TabIndex = 1;
            this.authzDefaultAllowRB.TabStop = true;
            this.authzDefaultAllowRB.Text = "Allow";
            this.authzDefaultAllowRB.UseVisualStyleBackColor = true;
            //
            // authzRuleAddButton
            //
            this.authzRuleAddButton.Location = new System.Drawing.Point(545, 236);
            this.authzRuleAddButton.Name = "authzRuleAddButton";
            this.authzRuleAddButton.Size = new System.Drawing.Size(71, 23);
            this.authzRuleAddButton.TabIndex = 16;
            this.authzRuleAddButton.Text = "Add Rule";
            this.authzRuleAddButton.UseVisualStyleBackColor = true;
            this.authzRuleAddButton.Click += new System.EventHandler(this.authzRuleAddButton_Click);
            //
            // authzRulesListBox
            //
            this.authzRulesListBox.FormattingEnabled = true;
            this.authzRulesListBox.HorizontalScrollbar = true;
            this.authzRulesListBox.Location = new System.Drawing.Point(6, 32);
            this.authzRulesListBox.Name = "authzRulesListBox";
            this.authzRulesListBox.Size = new System.Drawing.Size(610, 173);
            this.authzRulesListBox.TabIndex = 5;
            this.authzRulesListBox.SelectedIndexChanged += new System.EventHandler(this.authzRulesListBox_SelectedIndexChanged);
            //
            // authzRuleActionComboBox
            //
            this.authzRuleActionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authzRuleActionComboBox.FormattingEnabled = true;
            this.authzRuleActionComboBox.Items.AddRange(new object[] {
            "allow.",
            "deny."});
            this.authzRuleActionComboBox.Location = new System.Drawing.Point(474, 236);
            this.authzRuleActionComboBox.Name = "authzRuleActionComboBox";
            this.authzRuleActionComboBox.Size = new System.Drawing.Size(65, 21);
            this.authzRuleActionComboBox.TabIndex = 15;
            //
            // authzRulePathTB
            //
            this.authzRulePathTB.Location = new System.Drawing.Point(195, 211);
            this.authzRulePathTB.Name = "authzRulePathTB";
            this.authzRulePathTB.Size = new System.Drawing.Size(421, 20);
            this.authzRulePathTB.TabIndex = 12;
            //
            // authzRuleMemberComboBox
            //
            this.authzRuleMemberComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authzRuleMemberComboBox.FormattingEnabled = true;
            this.authzRuleMemberComboBox.Items.AddRange(new object[] {
            "If:",
            "If not:"});
            this.authzRuleMemberComboBox.Location = new System.Drawing.Point(6, 211);
            this.authzRuleMemberComboBox.Name = "authzRuleMemberComboBox";
            this.authzRuleMemberComboBox.Size = new System.Drawing.Size(65, 21);
            this.authzRuleMemberComboBox.TabIndex = 9;
            //
            // gatewayTabPage
            //
            this.gatewayTabPage.Controls.Add(this.GatewayPathLabel);
            this.gatewayTabPage.Controls.Add(this.GatewayFilterLabel);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleFilter);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleScope);
            this.gatewayTabPage.Controls.Add(this.gatwayRulePathTB);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleGroupMemberCB);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleDeleteBtn);
            this.gatewayTabPage.Controls.Add(this.label6);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleLocalGroupTB);
            this.gatewayTabPage.Controls.Add(this.gatewayRulesListBox);
            this.gatewayTabPage.Controls.Add(this.gatewayRuleAddButton);
            this.gatewayTabPage.Location = new System.Drawing.Point(4, 22);
            this.gatewayTabPage.Name = "gatewayTabPage";
            this.gatewayTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.gatewayTabPage.Size = new System.Drawing.Size(662, 263);
            this.gatewayTabPage.TabIndex = 2;
            this.gatewayTabPage.Text = "Gateway";
            this.gatewayTabPage.UseVisualStyleBackColor = true;
            //
            // GatewayPathLabel
            //
            this.GatewayPathLabel.AutoSize = true;
            this.GatewayPathLabel.Location = new System.Drawing.Point(158, 214);
            this.GatewayPathLabel.Name = "GatewayPathLabel";
            this.GatewayPathLabel.Size = new System.Drawing.Size(32, 13);
            this.GatewayPathLabel.TabIndex = 4;
            this.GatewayPathLabel.Text = "Path:";
            //
            // GatewayFilterLabel
            //
            this.GatewayFilterLabel.AutoSize = true;
            this.GatewayFilterLabel.Location = new System.Drawing.Point(6, 240);
            this.GatewayFilterLabel.Name = "GatewayFilterLabel";
            this.GatewayFilterLabel.Size = new System.Drawing.Size(32, 13);
            this.GatewayFilterLabel.TabIndex = 6;
            this.GatewayFilterLabel.Text = "Filter:";
            //
            // gatewayRuleFilter
            //
            this.gatewayRuleFilter.Location = new System.Drawing.Point(41, 236);
            this.gatewayRuleFilter.Name = "gatewayRuleFilter";
            this.gatewayRuleFilter.Size = new System.Drawing.Size(224, 20);
            this.gatewayRuleFilter.TabIndex = 7;
            //
            // gatewayRuleScope
            //
            this.gatewayRuleScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gatewayRuleScope.FormattingEnabled = true;
            this.gatewayRuleScope.Items.AddRange(new object[] {
            "Base",
            "OneLevel",
            "Subtree"});
            this.gatewayRuleScope.Location = new System.Drawing.Point(77, 211);
            this.gatewayRuleScope.Name = "gatewayRuleScope";
            this.gatewayRuleScope.Size = new System.Drawing.Size(76, 21);
            this.gatewayRuleScope.TabIndex = 3;
            //
            // gatwayRulePathTB
            //
            this.gatwayRulePathTB.Location = new System.Drawing.Point(195, 211);
            this.gatwayRulePathTB.Name = "gatwayRulePathTB";
            this.gatwayRulePathTB.Size = new System.Drawing.Size(421, 20);
            this.gatwayRulePathTB.TabIndex = 5;
            //
            // gatewayRuleGroupMemberCB
            //
            this.gatewayRuleGroupMemberCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gatewayRuleGroupMemberCB.FormattingEnabled = true;
            this.gatewayRuleGroupMemberCB.Items.AddRange(new object[] {
            "If:",
            "If not:",
            "Always:"});
            this.gatewayRuleGroupMemberCB.Location = new System.Drawing.Point(6, 211);
            this.gatewayRuleGroupMemberCB.Name = "gatewayRuleGroupMemberCB";
            this.gatewayRuleGroupMemberCB.Size = new System.Drawing.Size(65, 21);
            this.gatewayRuleGroupMemberCB.TabIndex = 2;
            this.gatewayRuleGroupMemberCB.SelectedIndexChanged += new System.EventHandler(this.gatewayRuleGroupMemberCB_SelectedIndexChanged);
            //
            // gatewayRuleDeleteBtn
            //
            this.gatewayRuleDeleteBtn.Enabled = false;
            this.gatewayRuleDeleteBtn.Image = global::pGina.Plugin.Ldap.Properties.Resources.delete;
            this.gatewayRuleDeleteBtn.Location = new System.Drawing.Point(622, 82);
            this.gatewayRuleDeleteBtn.Name = "gatewayRuleDeleteBtn";
            this.gatewayRuleDeleteBtn.Size = new System.Drawing.Size(32, 34);
            this.gatewayRuleDeleteBtn.TabIndex = 1;
            this.gatewayRuleDeleteBtn.UseVisualStyleBackColor = true;
            this.gatewayRuleDeleteBtn.Click += new System.EventHandler(this.gatewayRuleDeleteBtn_Click);
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(285, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "add to local group:";
            //
            // gatewayRuleLocalGroupTB
            //
            this.gatewayRuleLocalGroupTB.Location = new System.Drawing.Point(386, 236);
            this.gatewayRuleLocalGroupTB.Name = "gatewayRuleLocalGroupTB";
            this.gatewayRuleLocalGroupTB.Size = new System.Drawing.Size(153, 20);
            this.gatewayRuleLocalGroupTB.TabIndex = 9;
            //
            // gatewayRulesListBox
            //
            this.gatewayRulesListBox.FormattingEnabled = true;
            this.gatewayRulesListBox.HorizontalScrollbar = true;
            this.gatewayRulesListBox.Location = new System.Drawing.Point(6, 6);
            this.gatewayRulesListBox.Name = "gatewayRulesListBox";
            this.gatewayRulesListBox.Size = new System.Drawing.Size(610, 199);
            this.gatewayRulesListBox.TabIndex = 0;
            this.gatewayRulesListBox.SelectedIndexChanged += new System.EventHandler(this.gatewayRulesListBox_SelectedIndexChanged);
            //
            // gatewayRuleAddButton
            //
            this.gatewayRuleAddButton.Location = new System.Drawing.Point(545, 236);
            this.gatewayRuleAddButton.Name = "gatewayRuleAddButton";
            this.gatewayRuleAddButton.Size = new System.Drawing.Size(71, 23);
            this.gatewayRuleAddButton.TabIndex = 10;
            this.gatewayRuleAddButton.Text = "Add Rule";
            this.gatewayRuleAddButton.UseVisualStyleBackColor = true;
            this.gatewayRuleAddButton.Click += new System.EventHandler(this.gatewayRuleAddButton_Click);
            //
            // tabPageChangePassword
            //
            this.tabPageChangePassword.Controls.Add(this.label9);
            this.tabPageChangePassword.Controls.Add(this.passwordAttributesDGV);
            this.tabPageChangePassword.Location = new System.Drawing.Point(4, 22);
            this.tabPageChangePassword.Name = "tabPageChangePassword";
            this.tabPageChangePassword.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChangePassword.Size = new System.Drawing.Size(662, 263);
            this.tabPageChangePassword.TabIndex = 3;
            this.tabPageChangePassword.Text = "Change Password";
            this.tabPageChangePassword.UseVisualStyleBackColor = true;
            //
            // label9
            //
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Attributes:";
            //
            // passwordAttributesDGV
            //
            this.passwordAttributesDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.passwordAttributesDGV.Location = new System.Drawing.Point(6, 19);
            this.passwordAttributesDGV.Name = "passwordAttributesDGV";
            this.passwordAttributesDGV.Size = new System.Drawing.Size(648, 238);
            this.passwordAttributesDGV.TabIndex = 1;
            //
            // help
            //
            this.help.Location = new System.Drawing.Point(437, 464);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(76, 26);
            this.help.TabIndex = 2;
            this.help.Text = "Help";
            this.help.UseVisualStyleBackColor = true;
            this.help.Click += new System.EventHandler(this.Btn_help);
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 502);
            this.Controls.Add(this.help);
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
            this.authtab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.authzTabPage.ResumeLayout(false);
            this.authzTabPage.PerformLayout();
            this.gatewayTabPage.ResumeLayout(false);
            this.gatewayTabPage.PerformLayout();
            this.tabPageChangePassword.ResumeLayout(false);
            this.tabPageChangePassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.passwordAttributesDGV)).EndInit();
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
        private System.Windows.Forms.TextBox searchDnTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox timeoutTextBox;
        private System.Windows.Forms.Label timeoutLabel;
        private System.Windows.Forms.TextBox searchPassTextBox;
        private System.Windows.Forms.CheckBox showPwCB;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage authTabPage;
        private System.Windows.Forms.TabPage authzTabPage;
        private System.Windows.Forms.TextBox authzRulePathTB;
        private System.Windows.Forms.ComboBox authzRuleMemberComboBox;
        private System.Windows.Forms.Button gatewayRuleAddButton;
        private System.Windows.Forms.TextBox gatewayRuleLocalGroupTB;
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
        private System.Windows.Forms.TextBox gatwayRulePathTB;
        private System.Windows.Forms.CheckBox authzRequireAuthCB;
        private System.Windows.Forms.CheckBox authzAllowOnErrorCB;
        private System.Windows.Forms.TabPage tabPageChangePassword;
        private System.Windows.Forms.DataGridView passwordAttributesDGV;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabControl authtab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox searchContextsTextBox;
        private System.Windows.Forms.Label searchContextsLabel;
        private System.Windows.Forms.TextBox dnPatternTextBox;
        private System.Windows.Forms.TextBox searchFilterTextBox;
        private System.Windows.Forms.Label searchFilterLabel;
        private System.Windows.Forms.Label dnPatternLabel;
        private System.Windows.Forms.CheckBox searchForDnCheckBox;
        private System.Windows.Forms.CheckBox allowEmptyPwCB;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.ComboBox authzRuleScope;
        private System.Windows.Forms.TextBox authzRuleFilter;
        private System.Windows.Forms.ComboBox gatewayRuleScope;
        private System.Windows.Forms.Label GatewayPathLabel;
        private System.Windows.Forms.Label GatewayFilterLabel;
        private System.Windows.Forms.TextBox gatewayRuleFilter;
        private System.Windows.Forms.Label AuthPathLabel;
        private System.Windows.Forms.Label AuthFilterLabel;
        private System.Windows.Forms.CheckBox useTlsCheckBox;
        private System.Windows.Forms.CheckBox useAuthBindForAuthzAndGatewayCb;
        private System.Windows.Forms.Button help;
    }
}
