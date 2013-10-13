namespace pGina.Plugin.RADIUS
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.serverTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.authPortTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.secretTB = new System.Windows.Forms.TextBox();
            this.showSecretCB = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timeoutTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.acctPortTB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.retryTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.ipAddrSuggestionTB = new System.Windows.Forms.TextBox();
            this.useModifiedNameCB = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.sendCalledStationTB = new System.Windows.Forms.TextBox();
            this.sendCalledStationCB = new System.Windows.Forms.CheckBox();
            this.sendNasIdentifierTB = new System.Windows.Forms.TextBox();
            this.sendNasIdentifierCB = new System.Windows.Forms.CheckBox();
            this.sendNasIpAddrCB = new System.Windows.Forms.CheckBox();
            this.sessionTimeoutCB = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.sendInterimUpdatesCB = new System.Windows.Forms.CheckBox();
            this.wisprTimeoutCB = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(272, 393);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 22;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(353, 393);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // serverTB
            // 
            this.serverTB.Location = new System.Drawing.Point(64, 16);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(200, 20);
            this.serverTB.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server(s):";
            // 
            // authPortTB
            // 
            this.authPortTB.Location = new System.Drawing.Point(374, 16);
            this.authPortTB.Name = "authPortTB";
            this.authPortTB.Size = new System.Drawing.Size(54, 20);
            this.authPortTB.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(268, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Authentication Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Shared Secret:";
            // 
            // secretTB
            // 
            this.secretTB.Location = new System.Drawing.Point(96, 78);
            this.secretTB.Name = "secretTB";
            this.secretTB.Size = new System.Drawing.Size(229, 20);
            this.secretTB.TabIndex = 13;
            // 
            // showSecretCB
            // 
            this.showSecretCB.AutoSize = true;
            this.showSecretCB.Location = new System.Drawing.Point(343, 80);
            this.showSecretCB.Name = "showSecretCB";
            this.showSecretCB.Size = new System.Drawing.Size(85, 17);
            this.showSecretCB.TabIndex = 14;
            this.showSecretCB.Text = "Show secret";
            this.showSecretCB.UseVisualStyleBackColor = true;
            this.showSecretCB.CheckedChanged += new System.EventHandler(this.showSecretChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Timeout: ";
            // 
            // timeoutTB
            // 
            this.timeoutTB.Location = new System.Drawing.Point(64, 39);
            this.timeoutTB.Name = "timeoutTB";
            this.timeoutTB.Size = new System.Drawing.Size(28, 20);
            this.timeoutTB.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(96, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "seconds";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(268, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Accounting Port:";
            // 
            // acctPortTB
            // 
            this.acctPortTB.Location = new System.Drawing.Point(374, 39);
            this.acctPortTB.Name = "acctPortTB";
            this.acctPortTB.Size = new System.Drawing.Size(54, 20);
            this.acctPortTB.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(148, 42);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Retry:";
            // 
            // retryTB
            // 
            this.retryTB.Location = new System.Drawing.Point(187, 39);
            this.retryTB.Name = "retryTB";
            this.retryTB.Size = new System.Drawing.Size(32, 20);
            this.retryTB.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(225, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "times";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 366);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "IP Address Suggestion:";
            // 
            // ipAddrSuggestionTB
            // 
            this.ipAddrSuggestionTB.Location = new System.Drawing.Point(130, 363);
            this.ipAddrSuggestionTB.Name = "ipAddrSuggestionTB";
            this.ipAddrSuggestionTB.Size = new System.Drawing.Size(98, 20);
            this.ipAddrSuggestionTB.TabIndex = 20;
            // 
            // useModifiedNameCB
            // 
            this.useModifiedNameCB.AutoSize = true;
            this.useModifiedNameCB.Location = new System.Drawing.Point(21, 335);
            this.useModifiedNameCB.Name = "useModifiedNameCB";
            this.useModifiedNameCB.Size = new System.Drawing.Size(207, 17);
            this.useModifiedNameCB.TabIndex = 18;
            this.useModifiedNameCB.Text = "Use modified username for accounting";
            this.useModifiedNameCB.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.sendCalledStationTB);
            this.groupBox2.Controls.Add(this.sendCalledStationCB);
            this.groupBox2.Controls.Add(this.sendNasIdentifierTB);
            this.groupBox2.Controls.Add(this.sendNasIdentifierCB);
            this.groupBox2.Controls.Add(this.sendNasIpAddrCB);
            this.groupBox2.Location = new System.Drawing.Point(15, 104);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(413, 97);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Authentication Options";
            // 
            // sendCalledStationTB
            // 
            this.sendCalledStationTB.Location = new System.Drawing.Point(114, 63);
            this.sendCalledStationTB.Name = "sendCalledStationTB";
            this.sendCalledStationTB.Size = new System.Drawing.Size(103, 20);
            this.sendCalledStationTB.TabIndex = 4;
            this.sendCalledStationTB.Text = "%macaddr";
            // 
            // sendCalledStationCB
            // 
            this.sendCalledStationCB.AutoSize = true;
            this.sendCalledStationCB.Location = new System.Drawing.Point(7, 65);
            this.sendCalledStationCB.Name = "sendCalledStationCB";
            this.sendCalledStationCB.Size = new System.Drawing.Size(111, 17);
            this.sendCalledStationCB.TabIndex = 3;
            this.sendCalledStationCB.Text = "Called-Station-ID: ";
            this.sendCalledStationCB.UseVisualStyleBackColor = true;
            // 
            // sendNasIdentifierTB
            // 
            this.sendNasIdentifierTB.Location = new System.Drawing.Point(101, 42);
            this.sendNasIdentifierTB.Name = "sendNasIdentifierTB";
            this.sendNasIdentifierTB.Size = new System.Drawing.Size(116, 20);
            this.sendNasIdentifierTB.TabIndex = 2;
            this.sendNasIdentifierTB.Text = "%computername";
            // 
            // sendNasIdentifierCB
            // 
            this.sendNasIdentifierCB.AutoSize = true;
            this.sendNasIdentifierCB.Checked = true;
            this.sendNasIdentifierCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sendNasIdentifierCB.Location = new System.Drawing.Point(7, 44);
            this.sendNasIdentifierCB.Name = "sendNasIdentifierCB";
            this.sendNasIdentifierCB.Size = new System.Drawing.Size(97, 17);
            this.sendNasIdentifierCB.TabIndex = 1;
            this.sendNasIdentifierCB.Text = "NAS Identifier: ";
            this.sendNasIdentifierCB.UseVisualStyleBackColor = true;
            // 
            // sendNasIpAddrCB
            // 
            this.sendNasIpAddrCB.AutoSize = true;
            this.sendNasIpAddrCB.Checked = true;
            this.sendNasIpAddrCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sendNasIpAddrCB.Location = new System.Drawing.Point(7, 20);
            this.sendNasIpAddrCB.Name = "sendNasIpAddrCB";
            this.sendNasIpAddrCB.Size = new System.Drawing.Size(320, 17);
            this.sendNasIpAddrCB.TabIndex = 0;
            this.sendNasIpAddrCB.Text = "Send Host IP Address During Authentication (NAS-IP-Address)";
            this.sendNasIpAddrCB.UseVisualStyleBackColor = true;
            // 
            // sessionTimeoutCB
            // 
            this.sessionTimeoutCB.AutoSize = true;
            this.sessionTimeoutCB.Location = new System.Drawing.Point(7, 19);
            this.sessionTimeoutCB.Name = "sessionTimeoutCB";
            this.sessionTimeoutCB.Size = new System.Drawing.Size(140, 17);
            this.sessionTimeoutCB.TabIndex = 0;
            this.sessionTimeoutCB.Text = "Enable Session Timeout";
            this.sessionTimeoutCB.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.sendInterimUpdatesCB);
            this.groupBox3.Location = new System.Drawing.Point(15, 207);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(413, 46);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Accounting Options";
            // 
            // sendInterimUpdatesCB
            // 
            this.sendInterimUpdatesCB.AutoSize = true;
            this.sendInterimUpdatesCB.Location = new System.Drawing.Point(7, 20);
            this.sendInterimUpdatesCB.Name = "sendInterimUpdatesCB";
            this.sendInterimUpdatesCB.Size = new System.Drawing.Size(128, 17);
            this.sendInterimUpdatesCB.TabIndex = 0;
            this.sendInterimUpdatesCB.Text = "Send Interim Updates";
            this.sendInterimUpdatesCB.UseVisualStyleBackColor = true;
            // 
            // wisprTimeoutCB
            // 
            this.wisprTimeoutCB.AutoSize = true;
            this.wisprTimeoutCB.Location = new System.Drawing.Point(6, 42);
            this.wisprTimeoutCB.Name = "wisprTimeoutCB";
            this.wisprTimeoutCB.Size = new System.Drawing.Size(173, 17);
            this.wisprTimeoutCB.TabIndex = 1;
            this.wisprTimeoutCB.Text = "WISPr Session Terminate Time";
            this.wisprTimeoutCB.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.sessionTimeoutCB);
            this.groupBox4.Controls.Add(this.wisprTimeoutCB);
            this.groupBox4.Location = new System.Drawing.Point(15, 259);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(413, 70);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Timeout Options";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(235, 366);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "(regex supported)";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 424);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.useModifiedNameCB);
            this.Controls.Add(this.ipAddrSuggestionTB);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.retryTB);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.acctPortTB);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.timeoutTB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.showSecretCB);
            this.Controls.Add(this.secretTB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.authPortTB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverTB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Configuration";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RADIUS Plugin Configuration";
            this.Load += new System.EventHandler(this.Configuration_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox serverTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox authPortTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox secretTB;
        private System.Windows.Forms.CheckBox showSecretCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox timeoutTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox acctPortTB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox retryTB;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ipAddrSuggestionTB;
        private System.Windows.Forms.CheckBox useModifiedNameCB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox sendNasIdentifierTB;
        private System.Windows.Forms.CheckBox sendNasIdentifierCB;
        private System.Windows.Forms.CheckBox sendNasIpAddrCB;
        private System.Windows.Forms.CheckBox sessionTimeoutCB;
        private System.Windows.Forms.TextBox sendCalledStationTB;
        private System.Windows.Forms.CheckBox sendCalledStationCB;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox sendInterimUpdatesCB;
        private System.Windows.Forms.CheckBox wisprTimeoutCB;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label10;
    }
}