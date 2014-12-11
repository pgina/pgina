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
            this.authGB = new System.Windows.Forms.GroupBox();
            this.sessionTimeoutCB = new System.Windows.Forms.CheckBox();
            this.wisprTimeoutCB = new System.Windows.Forms.CheckBox();
            this.sendCalledStationTB = new System.Windows.Forms.TextBox();
            this.sendCalledStationCB = new System.Windows.Forms.CheckBox();
            this.sendNasIdentifierTB = new System.Windows.Forms.TextBox();
            this.sendNasIdentifierCB = new System.Windows.Forms.CheckBox();
            this.sendNasIpAddrCB = new System.Windows.Forms.CheckBox();
            this.acctGB = new System.Windows.Forms.GroupBox();
            this.forceInterimUpdLbl = new System.Windows.Forms.Label();
            this.forceInterimUpdTB = new System.Windows.Forms.TextBox();
            this.forceInterimUpdCB = new System.Windows.Forms.CheckBox();
            this.sendInterimUpdatesCB = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.enableAuthCB = new System.Windows.Forms.CheckBox();
            this.enableAcctCB = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.acctingForAllUsersCB = new System.Windows.Forms.CheckBox();
            this.authGB.SuspendLayout();
            this.acctGB.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(296, 471);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 22;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(377, 471);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // serverTB
            // 
            this.serverTB.Location = new System.Drawing.Point(58, 21);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(200, 20);
            this.serverTB.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server(s):";
            // 
            // authPortTB
            // 
            this.authPortTB.Location = new System.Drawing.Point(368, 21);
            this.authPortTB.Name = "authPortTB";
            this.authPortTB.Size = new System.Drawing.Size(54, 20);
            this.authPortTB.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(262, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Authentication Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Shared Secret:";
            // 
            // secretTB
            // 
            this.secretTB.Location = new System.Drawing.Point(90, 67);
            this.secretTB.Name = "secretTB";
            this.secretTB.Size = new System.Drawing.Size(229, 20);
            this.secretTB.TabIndex = 13;
            // 
            // showSecretCB
            // 
            this.showSecretCB.AutoSize = true;
            this.showSecretCB.Location = new System.Drawing.Point(337, 69);
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
            this.label4.Location = new System.Drawing.Point(6, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Timeout: ";
            // 
            // timeoutTB
            // 
            this.timeoutTB.Location = new System.Drawing.Point(58, 44);
            this.timeoutTB.Name = "timeoutTB";
            this.timeoutTB.Size = new System.Drawing.Size(28, 20);
            this.timeoutTB.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(90, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "seconds";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(262, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Accounting Port:";
            // 
            // acctPortTB
            // 
            this.acctPortTB.Location = new System.Drawing.Point(368, 44);
            this.acctPortTB.Name = "acctPortTB";
            this.acctPortTB.Size = new System.Drawing.Size(54, 20);
            this.acctPortTB.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(142, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Retry:";
            // 
            // retryTB
            // 
            this.retryTB.Location = new System.Drawing.Point(181, 44);
            this.retryTB.Name = "retryTB";
            this.retryTB.Size = new System.Drawing.Size(32, 20);
            this.retryTB.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(219, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "times";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 445);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "IP Address Suggestion:";
            // 
            // ipAddrSuggestionTB
            // 
            this.ipAddrSuggestionTB.Location = new System.Drawing.Point(133, 442);
            this.ipAddrSuggestionTB.Name = "ipAddrSuggestionTB";
            this.ipAddrSuggestionTB.Size = new System.Drawing.Size(98, 20);
            this.ipAddrSuggestionTB.TabIndex = 20;
            // 
            // useModifiedNameCB
            // 
            this.useModifiedNameCB.AutoSize = true;
            this.useModifiedNameCB.Location = new System.Drawing.Point(24, 414);
            this.useModifiedNameCB.Name = "useModifiedNameCB";
            this.useModifiedNameCB.Size = new System.Drawing.Size(207, 17);
            this.useModifiedNameCB.TabIndex = 18;
            this.useModifiedNameCB.Text = "Use modified username for accounting";
            this.useModifiedNameCB.UseVisualStyleBackColor = true;
            // 
            // authGB
            // 
            this.authGB.Controls.Add(this.sessionTimeoutCB);
            this.authGB.Controls.Add(this.wisprTimeoutCB);
            this.authGB.Controls.Add(this.sendCalledStationTB);
            this.authGB.Controls.Add(this.sendCalledStationCB);
            this.authGB.Controls.Add(this.sendNasIdentifierTB);
            this.authGB.Controls.Add(this.sendNasIdentifierCB);
            this.authGB.Controls.Add(this.sendNasIpAddrCB);
            this.authGB.Location = new System.Drawing.Point(18, 171);
            this.authGB.Name = "authGB";
            this.authGB.Size = new System.Drawing.Size(434, 134);
            this.authGB.TabIndex = 15;
            this.authGB.TabStop = false;
            this.authGB.Text = "Authentication Options";
            // 
            // sessionTimeoutCB
            // 
            this.sessionTimeoutCB.AutoSize = true;
            this.sessionTimeoutCB.Location = new System.Drawing.Point(7, 88);
            this.sessionTimeoutCB.Name = "sessionTimeoutCB";
            this.sessionTimeoutCB.Size = new System.Drawing.Size(140, 17);
            this.sessionTimeoutCB.TabIndex = 0;
            this.sessionTimeoutCB.Text = "Enable Session Timeout";
            this.sessionTimeoutCB.UseVisualStyleBackColor = true;
            // 
            // wisprTimeoutCB
            // 
            this.wisprTimeoutCB.AutoSize = true;
            this.wisprTimeoutCB.Location = new System.Drawing.Point(6, 111);
            this.wisprTimeoutCB.Name = "wisprTimeoutCB";
            this.wisprTimeoutCB.Size = new System.Drawing.Size(173, 17);
            this.wisprTimeoutCB.TabIndex = 1;
            this.wisprTimeoutCB.Text = "WISPr Session Terminate Time";
            this.wisprTimeoutCB.UseVisualStyleBackColor = true;
            // 
            // sendCalledStationTB
            // 
            this.sendCalledStationTB.Location = new System.Drawing.Point(114, 63);
            this.sendCalledStationTB.Name = "sendCalledStationTB";
            this.sendCalledStationTB.Size = new System.Drawing.Size(103, 20);
            this.sendCalledStationTB.TabIndex = 4;
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
            // 
            // sendNasIdentifierCB
            // 
            this.sendNasIdentifierCB.AutoSize = true;
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
            this.sendNasIpAddrCB.Location = new System.Drawing.Point(7, 20);
            this.sendNasIpAddrCB.Name = "sendNasIpAddrCB";
            this.sendNasIpAddrCB.Size = new System.Drawing.Size(320, 17);
            this.sendNasIpAddrCB.TabIndex = 0;
            this.sendNasIpAddrCB.Text = "Send Host IP Address During Authentication (NAS-IP-Address)";
            this.sendNasIpAddrCB.UseVisualStyleBackColor = true;
            // 
            // acctGB
            // 
            this.acctGB.Controls.Add(this.acctingForAllUsersCB);
            this.acctGB.Controls.Add(this.forceInterimUpdLbl);
            this.acctGB.Controls.Add(this.forceInterimUpdTB);
            this.acctGB.Controls.Add(this.forceInterimUpdCB);
            this.acctGB.Controls.Add(this.sendInterimUpdatesCB);
            this.acctGB.Location = new System.Drawing.Point(18, 311);
            this.acctGB.Name = "acctGB";
            this.acctGB.Size = new System.Drawing.Size(434, 97);
            this.acctGB.TabIndex = 16;
            this.acctGB.TabStop = false;
            this.acctGB.Text = "Accounting Options";
            // 
            // forceInterimUpdLbl
            // 
            this.forceInterimUpdLbl.AutoSize = true;
            this.forceInterimUpdLbl.Location = new System.Drawing.Point(184, 65);
            this.forceInterimUpdLbl.Name = "forceInterimUpdLbl";
            this.forceInterimUpdLbl.Size = new System.Drawing.Size(47, 13);
            this.forceInterimUpdLbl.TabIndex = 3;
            this.forceInterimUpdLbl.Text = "seconds";
            // 
            // forceInterimUpdTB
            // 
            this.forceInterimUpdTB.Location = new System.Drawing.Point(142, 62);
            this.forceInterimUpdTB.Name = "forceInterimUpdTB";
            this.forceInterimUpdTB.Size = new System.Drawing.Size(36, 20);
            this.forceInterimUpdTB.TabIndex = 2;
            // 
            // forceInterimUpdCB
            // 
            this.forceInterimUpdCB.AutoSize = true;
            this.forceInterimUpdCB.Location = new System.Drawing.Point(24, 64);
            this.forceInterimUpdCB.Name = "forceInterimUpdCB";
            this.forceInterimUpdCB.Size = new System.Drawing.Size(116, 17);
            this.forceInterimUpdCB.TabIndex = 1;
            this.forceInterimUpdCB.Text = "Send update every";
            this.forceInterimUpdCB.UseVisualStyleBackColor = true;
            // 
            // sendInterimUpdatesCB
            // 
            this.sendInterimUpdatesCB.AutoSize = true;
            this.sendInterimUpdatesCB.Location = new System.Drawing.Point(6, 40);
            this.sendInterimUpdatesCB.Name = "sendInterimUpdatesCB";
            this.sendInterimUpdatesCB.Size = new System.Drawing.Size(128, 17);
            this.sendInterimUpdatesCB.TabIndex = 0;
            this.sendInterimUpdatesCB.Text = "Send Interim Updates";
            this.sendInterimUpdatesCB.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(238, 445);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "(regex supported)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.serverTB);
            this.groupBox1.Controls.Add(this.authPortTB);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.secretTB);
            this.groupBox1.Controls.Add(this.showSecretCB);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.timeoutTB);
            this.groupBox1.Controls.Add(this.retryTB);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.acctPortTB);
            this.groupBox1.Location = new System.Drawing.Point(18, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(434, 100);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Settings";
            // 
            // enableAuthCB
            // 
            this.enableAuthCB.AutoSize = true;
            this.enableAuthCB.Location = new System.Drawing.Point(9, 20);
            this.enableAuthCB.Name = "enableAuthCB";
            this.enableAuthCB.Size = new System.Drawing.Size(207, 17);
            this.enableAuthCB.TabIndex = 0;
            this.enableAuthCB.Text = "Enable Authentication (Authentication)";
            this.enableAuthCB.UseVisualStyleBackColor = true;
            // 
            // enableAcctCB
            // 
            this.enableAcctCB.AutoSize = true;
            this.enableAcctCB.Location = new System.Drawing.Point(223, 19);
            this.enableAcctCB.Name = "enableAcctCB";
            this.enableAcctCB.Size = new System.Drawing.Size(178, 17);
            this.enableAcctCB.TabIndex = 1;
            this.enableAcctCB.Text = "Enable Accounting (Notification)";
            this.enableAcctCB.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.enableAcctCB);
            this.groupBox5.Controls.Add(this.enableAuthCB);
            this.groupBox5.Location = new System.Drawing.Point(18, 13);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(434, 46);
            this.groupBox5.TabIndex = 25;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "RADIUS Features";
            // 
            // acctingForAllUsersCB
            // 
            this.acctingForAllUsersCB.AutoSize = true;
            this.acctingForAllUsersCB.Location = new System.Drawing.Point(7, 17);
            this.acctingForAllUsersCB.Name = "acctingForAllUsersCB";
            this.acctingForAllUsersCB.Size = new System.Drawing.Size(226, 17);
            this.acctingForAllUsersCB.TabIndex = 4;
            this.acctingForAllUsersCB.Text = "Perform accounting for non-RADIUS users";
            this.acctingForAllUsersCB.UseVisualStyleBackColor = true;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 506);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.acctGB);
            this.Controls.Add(this.authGB);
            this.Controls.Add(this.useModifiedNameCB);
            this.Controls.Add(this.ipAddrSuggestionTB);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Configuration";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RADIUS Plugin Configuration";
            this.Load += new System.EventHandler(this.Configuration_Load);
            this.authGB.ResumeLayout(false);
            this.authGB.PerformLayout();
            this.acctGB.ResumeLayout(false);
            this.acctGB.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
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
        private System.Windows.Forms.GroupBox authGB;
        private System.Windows.Forms.TextBox sendNasIdentifierTB;
        private System.Windows.Forms.CheckBox sendNasIdentifierCB;
        private System.Windows.Forms.CheckBox sendNasIpAddrCB;
        private System.Windows.Forms.CheckBox sessionTimeoutCB;
        private System.Windows.Forms.TextBox sendCalledStationTB;
        private System.Windows.Forms.CheckBox sendCalledStationCB;
        private System.Windows.Forms.GroupBox acctGB;
        private System.Windows.Forms.CheckBox sendInterimUpdatesCB;
        private System.Windows.Forms.CheckBox wisprTimeoutCB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox enableAcctCB;
        private System.Windows.Forms.CheckBox enableAuthCB;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label forceInterimUpdLbl;
        private System.Windows.Forms.TextBox forceInterimUpdTB;
        private System.Windows.Forms.CheckBox forceInterimUpdCB;
        private System.Windows.Forms.CheckBox acctingForAllUsersCB;
    }
}