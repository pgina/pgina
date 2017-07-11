namespace pGina.Plugin.MultiEmail
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
            this.m_serversListDgv = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_deleteServerButton = new System.Windows.Forms.Button();
            this.m_newServerButton = new System.Windows.Forms.Button();
            this.protocolComboBox = new System.Windows.Forms.ComboBox();
            this.protocolLabel = new System.Windows.Forms.Label();
            this.serverTextBox = new System.Windows.Forms.TextBox();
            this.serverLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.sslCheckBox = new System.Windows.Forms.CheckBox();
            this.appendDomainCheckBox = new System.Windows.Forms.CheckBox();
            this.domainTextBox = new System.Windows.Forms.TextBox();
            this.domainLabel = new System.Windows.Forms.Label();
            this.timeoutTextBox = new System.Windows.Forms.TextBox();
            this.timeoutLabel = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.m_saveCloseButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_serversListDgv)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            //
            // m_serversListDgv
            //
            this.m_serversListDgv.AllowUserToAddRows = false;
            this.m_serversListDgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_serversListDgv.Location = new System.Drawing.Point(12, 12);
            this.m_serversListDgv.MultiSelect = false;
            this.m_serversListDgv.Name = "m_serversListDgv";
            this.m_serversListDgv.RowHeadersVisible = false;
            this.m_serversListDgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_serversListDgv.Size = new System.Drawing.Size(562, 200);
            this.m_serversListDgv.TabIndex = 0;
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.m_deleteServerButton);
            this.groupBox1.Controls.Add(this.m_newServerButton);
            this.groupBox1.Controls.Add(this.protocolComboBox);
            this.groupBox1.Controls.Add(this.protocolLabel);
            this.groupBox1.Controls.Add(this.serverTextBox);
            this.groupBox1.Controls.Add(this.serverLabel);
            this.groupBox1.Controls.Add(this.portTextBox);
            this.groupBox1.Controls.Add(this.portLabel);
            this.groupBox1.Controls.Add(this.sslCheckBox);
            this.groupBox1.Controls.Add(this.appendDomainCheckBox);
            this.groupBox1.Controls.Add(this.domainTextBox);
            this.groupBox1.Controls.Add(this.domainLabel);
            this.groupBox1.Controls.Add(this.timeoutTextBox);
            this.groupBox1.Controls.Add(this.timeoutLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 221);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 105);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            //
            // m_deleteServerButton
            //
            this.m_deleteServerButton.Location = new System.Drawing.Point(481, 74);
            this.m_deleteServerButton.Name = "m_deleteServerButton";
            this.m_deleteServerButton.Size = new System.Drawing.Size(75, 23);
            this.m_deleteServerButton.TabIndex = 13;
            this.m_deleteServerButton.Text = "Delete";
            this.m_deleteServerButton.UseVisualStyleBackColor = true;
            this.m_deleteServerButton.Click += new System.EventHandler(this.m_deleteServerButton_Click);
            //
            // m_newServerButton
            //
            this.m_newServerButton.Location = new System.Drawing.Point(410, 74);
            this.m_newServerButton.Name = "m_newServerButton";
            this.m_newServerButton.Size = new System.Drawing.Size(65, 23);
            this.m_newServerButton.TabIndex = 12;
            this.m_newServerButton.Text = "New";
            this.m_newServerButton.UseVisualStyleBackColor = true;
            this.m_newServerButton.Click += new System.EventHandler(this.newServerButton_Click);
            //
            // protocolComboBox
            //
            this.protocolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolComboBox.FormattingEnabled = true;
            this.protocolComboBox.Location = new System.Drawing.Point(61, 19);
            this.protocolComboBox.Name = "protocolComboBox";
            this.protocolComboBox.Size = new System.Drawing.Size(59, 21);
            this.protocolComboBox.TabIndex = 1;
            this.protocolComboBox.SelectedIndexChanged += new System.EventHandler(this.protocolComboBox_SelectedIndexChanged);
            //
            // protocolLabel
            //
            this.protocolLabel.AutoSize = true;
            this.protocolLabel.Location = new System.Drawing.Point(6, 22);
            this.protocolLabel.Name = "protocolLabel";
            this.protocolLabel.Size = new System.Drawing.Size(49, 13);
            this.protocolLabel.TabIndex = 0;
            this.protocolLabel.Text = "Protocol:";
            //
            // serverTextBox
            //
            this.serverTextBox.Location = new System.Drawing.Point(173, 19);
            this.serverTextBox.Name = "serverTextBox";
            this.serverTextBox.Size = new System.Drawing.Size(212, 20);
            this.serverTextBox.TabIndex = 3;
            //
            // serverLabel
            //
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(126, 22);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(41, 13);
            this.serverLabel.TabIndex = 2;
            this.serverLabel.Text = "Server:";
            //
            // portTextBox
            //
            this.portTextBox.Location = new System.Drawing.Point(426, 19);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(50, 20);
            this.portTextBox.TabIndex = 5;
            //
            // portLabel
            //
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(391, 22);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 4;
            this.portLabel.Text = "Port:";
            //
            // sslCheckBox
            //
            this.sslCheckBox.AutoSize = true;
            this.sslCheckBox.Location = new System.Drawing.Point(482, 21);
            this.sslCheckBox.Name = "sslCheckBox";
            this.sslCheckBox.Size = new System.Drawing.Size(68, 17);
            this.sslCheckBox.TabIndex = 6;
            this.sslCheckBox.Text = "Use SSL";
            this.sslCheckBox.UseVisualStyleBackColor = true;
            this.sslCheckBox.CheckedChanged += new System.EventHandler(this.sslCheckBox_CheckedChanged);
            //
            // appendDomainCheckBox
            //
            this.appendDomainCheckBox.AutoSize = true;
            this.appendDomainCheckBox.Location = new System.Drawing.Point(9, 50);
            this.appendDomainCheckBox.Name = "appendDomainCheckBox";
            this.appendDomainCheckBox.Size = new System.Drawing.Size(165, 17);
            this.appendDomainCheckBox.TabIndex = 7;
            this.appendDomainCheckBox.Text = "Append Domain to Username";
            this.appendDomainCheckBox.UseVisualStyleBackColor = true;
            this.appendDomainCheckBox.CheckedChanged += new System.EventHandler(this.domainAppendCheckBox_CheckedChanged);
            //
            // domainTextBox
            //
            this.domainTextBox.Location = new System.Drawing.Point(227, 47);
            this.domainTextBox.Name = "domainTextBox";
            this.domainTextBox.Size = new System.Drawing.Size(165, 20);
            this.domainTextBox.TabIndex = 9;
            this.domainTextBox.TextChanged += new System.EventHandler(this.domainTextBox_TextChanged);
            //
            // domainLabel
            //
            this.domainLabel.AutoSize = true;
            this.domainLabel.Location = new System.Drawing.Point(175, 51);
            this.domainLabel.Name = "domainLabel";
            this.domainLabel.Size = new System.Drawing.Size(46, 13);
            this.domainLabel.TabIndex = 8;
            this.domainLabel.Text = "Domain:";
            //
            // timeoutTextBox
            //
            this.timeoutTextBox.Location = new System.Drawing.Point(452, 48);
            this.timeoutTextBox.Name = "timeoutTextBox";
            this.timeoutTextBox.Size = new System.Drawing.Size(76, 20);
            this.timeoutTextBox.TabIndex = 11;
            //
            // timeoutLabel
            //
            this.timeoutLabel.AutoSize = true;
            this.timeoutLabel.Location = new System.Drawing.Point(398, 51);
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.Size = new System.Drawing.Size(48, 13);
            this.timeoutLabel.TabIndex = 10;
            this.timeoutLabel.Text = "Timeout:";
            //
            // btnHelp
            //
            this.btnHelp.Location = new System.Drawing.Point(349, 333);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(71, 23);
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.Btn_help);
            //
            // m_saveCloseButton
            //
            this.m_saveCloseButton.Location = new System.Drawing.Point(503, 333);
            this.m_saveCloseButton.Name = "m_saveCloseButton";
            this.m_saveCloseButton.Size = new System.Drawing.Size(71, 23);
            this.m_saveCloseButton.TabIndex = 4;
            this.m_saveCloseButton.Text = "Save";
            this.m_saveCloseButton.UseVisualStyleBackColor = true;
            this.m_saveCloseButton.Click += new System.EventHandler(this.m_saveCloseButton_Click);
            //
            // m_cancelButton
            //
            this.m_cancelButton.Location = new System.Drawing.Point(426, 333);
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.Size = new System.Drawing.Size(71, 23);
            this.m_cancelButton.TabIndex = 3;
            this.m_cancelButton.Text = "Cancel";
            this.m_cancelButton.UseVisualStyleBackColor = true;
            this.m_cancelButton.Click += new System.EventHandler(this.m_cancelButton_Click);
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 368);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_saveCloseButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.m_serversListDgv);
            this.Name = "Configuration";
            this.Text = "Multi Email Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.m_serversListDgv)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView m_serversListDgv;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox protocolComboBox;
        private System.Windows.Forms.Label protocolLabel;
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.CheckBox sslCheckBox;
        private System.Windows.Forms.CheckBox appendDomainCheckBox;
        private System.Windows.Forms.TextBox domainTextBox;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.TextBox timeoutTextBox;
        private System.Windows.Forms.Label timeoutLabel;
        private System.Windows.Forms.Button m_deleteServerButton;
        private System.Windows.Forms.Button m_newServerButton;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button m_saveCloseButton;
        private System.Windows.Forms.Button m_cancelButton;
    }
}