namespace pGina.Plugin.Email
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this.domainLabel = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.protocolBox = new System.Windows.Forms.GroupBox();
            this.imapButton = new System.Windows.Forms.RadioButton();
            this.popButton = new System.Windows.Forms.RadioButton();
            this.sslCheckBox = new System.Windows.Forms.CheckBox();
            this.serverTextBox = new System.Windows.Forms.TextBox();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.domainTextBox = new System.Windows.Forms.TextBox();
            this.domainAppendCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.protocolBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.CausesValidation = false;
            label1.Location = new System.Drawing.Point(12, 12);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(49, 13);
            label1.TabIndex = 5;
            label1.Text = "Protocol:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.CausesValidation = false;
            label2.Location = new System.Drawing.Point(9, 40);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(82, 13);
            label2.TabIndex = 7;
            label2.Text = "Server Address:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.CausesValidation = false;
            label3.Location = new System.Drawing.Point(214, 40);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(29, 13);
            label3.TabIndex = 8;
            label3.Text = "Port:";
            // 
            // domainLabel
            // 
            this.domainLabel.AutoSize = true;
            this.domainLabel.CausesValidation = false;
            this.domainLabel.Location = new System.Drawing.Point(23, 39);
            this.domainLabel.Name = "domainLabel";
            this.domainLabel.Size = new System.Drawing.Size(46, 13);
            this.domainLabel.TabIndex = 14;
            this.domainLabel.Text = "Domain:";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(219, 164);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(138, 164);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // protocolBox
            // 
            this.protocolBox.Controls.Add(this.imapButton);
            this.protocolBox.Controls.Add(this.popButton);
            this.protocolBox.Location = new System.Drawing.Point(67, -2);
            this.protocolBox.Name = "protocolBox";
            this.protocolBox.Size = new System.Drawing.Size(116, 34);
            this.protocolBox.TabIndex = 6;
            this.protocolBox.TabStop = false;
            // 
            // imapButton
            // 
            this.imapButton.AutoSize = true;
            this.imapButton.Location = new System.Drawing.Point(62, 12);
            this.imapButton.Name = "imapButton";
            this.imapButton.Size = new System.Drawing.Size(51, 17);
            this.imapButton.TabIndex = 1;
            this.imapButton.Text = "IMAP";
            this.imapButton.UseVisualStyleBackColor = true;
            this.imapButton.CheckedChanged += new System.EventHandler(this.changedProtocol);
            // 
            // popButton
            // 
            this.popButton.AutoSize = true;
            this.popButton.Location = new System.Drawing.Point(3, 12);
            this.popButton.Name = "popButton";
            this.popButton.Size = new System.Drawing.Size(53, 17);
            this.popButton.TabIndex = 0;
            this.popButton.Text = "POP3";
            this.popButton.UseVisualStyleBackColor = true;
            this.popButton.CheckedChanged += new System.EventHandler(this.changedProtocol);
            // 
            // sslCheckBox
            // 
            this.sslCheckBox.AutoSize = true;
            this.sslCheckBox.Checked = true;
            this.sslCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sslCheckBox.Location = new System.Drawing.Point(224, 11);
            this.sslCheckBox.Name = "sslCheckBox";
            this.sslCheckBox.Size = new System.Drawing.Size(68, 17);
            this.sslCheckBox.TabIndex = 10;
            this.sslCheckBox.Text = "Use SSL";
            this.sslCheckBox.UseVisualStyleBackColor = true;
            this.sslCheckBox.Click += new System.EventHandler(this.changedProtocol);
            // 
            // serverTextBox
            // 
            this.serverTextBox.Location = new System.Drawing.Point(92, 38);
            this.serverTextBox.Name = "serverTextBox";
            this.serverTextBox.Size = new System.Drawing.Size(110, 20);
            this.serverTextBox.TabIndex = 11;
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(249, 38);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(36, 20);
            this.portTextBox.TabIndex = 12;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.domainTextBox);
            this.groupBox2.Controls.Add(this.domainLabel);
            this.groupBox2.Controls.Add(this.domainAppendCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(282, 68);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Optional Settings";
            // 
            // domainTextBox
            // 
            this.domainTextBox.Enabled = false;
            this.domainTextBox.Location = new System.Drawing.Point(70, 36);
            this.domainTextBox.Name = "domainTextBox";
            this.domainTextBox.Size = new System.Drawing.Size(120, 20);
            this.domainTextBox.TabIndex = 15;
            // 
            // domainAppendCheckBox
            // 
            this.domainAppendCheckBox.AutoSize = true;
            this.domainAppendCheckBox.Location = new System.Drawing.Point(6, 19);
            this.domainAppendCheckBox.Name = "domainAppendCheckBox";
            this.domainAppendCheckBox.Size = new System.Drawing.Size(169, 17);
            this.domainAppendCheckBox.TabIndex = 1;
            this.domainAppendCheckBox.Text = "Append Domain To Username";
            this.domainAppendCheckBox.UseVisualStyleBackColor = true;
            this.domainAppendCheckBox.Click += new System.EventHandler(this.settingsChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Timeout (ms):";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(92, 64);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(91, 20);
            this.tbTimeout.TabIndex = 15;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 202);
            this.ControlBox = false;
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.serverTextBox);
            this.Controls.Add(this.sslCheckBox);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(this.protocolBox);
            this.Controls.Add(label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Configuration";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Email Auth Plugin Configuration";
            this.protocolBox.ResumeLayout(false);
            this.protocolBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox protocolBox;
        private System.Windows.Forms.RadioButton imapButton;
        private System.Windows.Forms.RadioButton popButton;
        private System.Windows.Forms.CheckBox sslCheckBox;
        private System.Windows.Forms.TextBox serverTextBox;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox domainAppendCheckBox;
        private System.Windows.Forms.TextBox domainTextBox;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTimeout;
    }
}