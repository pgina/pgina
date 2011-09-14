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
            this.ldapServerGroupBox = new System.Windows.Forms.GroupBox();
            this.sslCertFileBrowseButton = new System.Windows.Forms.Button();
            this.sslCertFileTextBox = new System.Windows.Forms.TextBox();
            this.authenticationGroupBox = new System.Windows.Forms.GroupBox();
            this.searchPassTextBox = new System.Windows.Forms.TextBox();
            this.searchDnTextBox = new System.Windows.Forms.TextBox();
            this.searchContextsTextBox = new System.Windows.Forms.TextBox();
            this.searchFilterTextBox = new System.Windows.Forms.TextBox();
            this.dnPatternTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.showPwCB = new System.Windows.Forms.CheckBox();
            this.ldapServerGroupBox.SuspendLayout();
            this.authenticationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(607, 409);
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
            this.validateServerCertCheckBox.Location = new System.Drawing.Point(80, 79);
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
            this.label1.Location = new System.Drawing.Point(3, 105);
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
            this.useSslCheckBox.Location = new System.Drawing.Point(6, 79);
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
            this.dnPatternLabel.Location = new System.Drawing.Point(6, 22);
            this.dnPatternLabel.Name = "dnPatternLabel";
            this.dnPatternLabel.Size = new System.Drawing.Size(60, 13);
            this.dnPatternLabel.TabIndex = 0;
            this.dnPatternLabel.Text = "DN Pattern";
            this.descriptionToolTip.SetToolTip(this.dnPatternLabel, "The pattern to use when creating a DN from a user name.\r\n%u can be used to indica" +
                    "te the user name.\r\n");
            // 
            // searchForDnCheckBox
            // 
            this.searchForDnCheckBox.AutoSize = true;
            this.searchForDnCheckBox.Location = new System.Drawing.Point(9, 47);
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
            this.searchFilterLabel.Location = new System.Drawing.Point(6, 72);
            this.searchFilterLabel.Name = "searchFilterLabel";
            this.searchFilterLabel.Size = new System.Drawing.Size(66, 13);
            this.searchFilterLabel.TabIndex = 3;
            this.searchFilterLabel.Text = "Search Filter";
            this.descriptionToolTip.SetToolTip(this.searchFilterLabel, "The filter to use when searching.\r\n%u is replaced with the user name\r\n");
            // 
            // searchContextsLabel
            // 
            this.searchContextsLabel.AutoSize = true;
            this.searchContextsLabel.Location = new System.Drawing.Point(6, 101);
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
            this.label2.Location = new System.Drawing.Point(6, 152);
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
            this.label3.Location = new System.Drawing.Point(6, 178);
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
            // ldapServerGroupBox
            // 
            this.ldapServerGroupBox.Controls.Add(this.timeoutTextBox);
            this.ldapServerGroupBox.Controls.Add(this.timeoutLabel);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileBrowseButton);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileTextBox);
            this.ldapServerGroupBox.Controls.Add(this.label1);
            this.ldapServerGroupBox.Controls.Add(this.validateServerCertCheckBox);
            this.ldapServerGroupBox.Controls.Add(this.useSslCheckBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortLabel);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostDescriptionLabel);
            this.ldapServerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ldapServerGroupBox.Name = "ldapServerGroupBox";
            this.ldapServerGroupBox.Size = new System.Drawing.Size(676, 136);
            this.ldapServerGroupBox.TabIndex = 3;
            this.ldapServerGroupBox.TabStop = false;
            this.ldapServerGroupBox.Text = "LDAP Server";
            // 
            // sslCertFileBrowseButton
            // 
            this.sslCertFileBrowseButton.Location = new System.Drawing.Point(578, 102);
            this.sslCertFileBrowseButton.Name = "sslCertFileBrowseButton";
            this.sslCertFileBrowseButton.Size = new System.Drawing.Size(80, 20);
            this.sslCertFileBrowseButton.TabIndex = 9;
            this.sslCertFileBrowseButton.Text = "Browse...";
            this.sslCertFileBrowseButton.UseVisualStyleBackColor = true;
            this.sslCertFileBrowseButton.Click += new System.EventHandler(this.sslCertFileBrowseButton_Click);
            // 
            // sslCertFileTextBox
            // 
            this.sslCertFileTextBox.Location = new System.Drawing.Point(109, 102);
            this.sslCertFileTextBox.Name = "sslCertFileTextBox";
            this.sslCertFileTextBox.Size = new System.Drawing.Size(463, 20);
            this.sslCertFileTextBox.TabIndex = 8;
            // 
            // authenticationGroupBox
            // 
            this.authenticationGroupBox.Controls.Add(this.showPwCB);
            this.authenticationGroupBox.Controls.Add(this.searchPassTextBox);
            this.authenticationGroupBox.Controls.Add(this.label3);
            this.authenticationGroupBox.Controls.Add(this.searchDnTextBox);
            this.authenticationGroupBox.Controls.Add(this.label2);
            this.authenticationGroupBox.Controls.Add(this.searchContextsTextBox);
            this.authenticationGroupBox.Controls.Add(this.searchContextsLabel);
            this.authenticationGroupBox.Controls.Add(this.searchFilterTextBox);
            this.authenticationGroupBox.Controls.Add(this.searchFilterLabel);
            this.authenticationGroupBox.Controls.Add(this.searchForDnCheckBox);
            this.authenticationGroupBox.Controls.Add(this.dnPatternTextBox);
            this.authenticationGroupBox.Controls.Add(this.dnPatternLabel);
            this.authenticationGroupBox.Location = new System.Drawing.Point(12, 165);
            this.authenticationGroupBox.Name = "authenticationGroupBox";
            this.authenticationGroupBox.Size = new System.Drawing.Size(676, 206);
            this.authenticationGroupBox.TabIndex = 4;
            this.authenticationGroupBox.TabStop = false;
            this.authenticationGroupBox.Text = "Authentication";
            // 
            // searchPassTextBox
            // 
            this.searchPassTextBox.Location = new System.Drawing.Point(110, 175);
            this.searchPassTextBox.Name = "searchPassTextBox";
            this.searchPassTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchPassTextBox.TabIndex = 11;
            this.searchPassTextBox.UseSystemPasswordChar = true;
            // 
            // searchDnTextBox
            // 
            this.searchDnTextBox.Location = new System.Drawing.Point(110, 149);
            this.searchDnTextBox.Name = "searchDnTextBox";
            this.searchDnTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchDnTextBox.TabIndex = 9;
            // 
            // searchContextsTextBox
            // 
            this.searchContextsTextBox.Location = new System.Drawing.Point(110, 98);
            this.searchContextsTextBox.Multiline = true;
            this.searchContextsTextBox.Name = "searchContextsTextBox";
            this.searchContextsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchContextsTextBox.Size = new System.Drawing.Size(462, 45);
            this.searchContextsTextBox.TabIndex = 6;
            // 
            // searchFilterTextBox
            // 
            this.searchFilterTextBox.Location = new System.Drawing.Point(110, 69);
            this.searchFilterTextBox.Name = "searchFilterTextBox";
            this.searchFilterTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchFilterTextBox.TabIndex = 4;
            // 
            // dnPatternTextBox
            // 
            this.dnPatternTextBox.Location = new System.Drawing.Point(110, 19);
            this.dnPatternTextBox.Name = "dnPatternTextBox";
            this.dnPatternTextBox.Size = new System.Drawing.Size(463, 20);
            this.dnPatternTextBox.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(525, 409);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(76, 26);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // showPwCB
            // 
            this.showPwCB.AutoSize = true;
            this.showPwCB.Location = new System.Drawing.Point(581, 177);
            this.showPwCB.Name = "showPwCB";
            this.showPwCB.Size = new System.Drawing.Size(77, 17);
            this.showPwCB.TabIndex = 12;
            this.showPwCB.Text = "Show Text";
            this.showPwCB.UseVisualStyleBackColor = true;
            this.showPwCB.CheckedChanged += new System.EventHandler(this.showPwCB_CheckedChanged);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 447);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.authenticationGroupBox);
            this.Controls.Add(this.ldapServerGroupBox);
            this.Controls.Add(this.saveButton);
            this.Name = "Configuration";
            this.Text = "LDAP Plugin Settings";
            this.ldapServerGroupBox.ResumeLayout(false);
            this.ldapServerGroupBox.PerformLayout();
            this.authenticationGroupBox.ResumeLayout(false);
            this.authenticationGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox authenticationGroupBox;
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
    }
}