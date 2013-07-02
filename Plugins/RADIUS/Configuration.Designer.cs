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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ipAddressButton = new System.Windows.Forms.RadioButton();
            this.machineNameButton = new System.Windows.Forms.RadioButton();
            this.bothButton = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.ipAddrSuggestionTB = new System.Windows.Forms.TextBox();
            this.useModifiedNameCB = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(272, 202);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 19;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(353, 202);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 20;
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bothButton);
            this.groupBox1.Controls.Add(this.machineNameButton);
            this.groupBox1.Controls.Add(this.ipAddressButton);
            this.groupBox1.Location = new System.Drawing.Point(15, 104);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(413, 43);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Machine Identifier";
            // 
            // ipAddressButton
            // 
            this.ipAddressButton.AutoSize = true;
            this.ipAddressButton.Location = new System.Drawing.Point(7, 20);
            this.ipAddressButton.Name = "ipAddressButton";
            this.ipAddressButton.Size = new System.Drawing.Size(100, 17);
            this.ipAddressButton.TabIndex = 0;
            this.ipAddressButton.TabStop = true;
            this.ipAddressButton.Text = "IP Address Only";
            this.ipAddressButton.UseVisualStyleBackColor = true;
            // 
            // machineNameButton
            // 
            this.machineNameButton.AutoSize = true;
            this.machineNameButton.Location = new System.Drawing.Point(151, 20);
            this.machineNameButton.Name = "machineNameButton";
            this.machineNameButton.Size = new System.Drawing.Size(121, 17);
            this.machineNameButton.TabIndex = 1;
            this.machineNameButton.TabStop = true;
            this.machineNameButton.Text = "Machine Name Only";
            this.machineNameButton.UseVisualStyleBackColor = true;
            // 
            // bothButton
            // 
            this.bothButton.AutoSize = true;
            this.bothButton.Location = new System.Drawing.Point(328, 20);
            this.bothButton.Name = "bothButton";
            this.bothButton.Size = new System.Drawing.Size(47, 17);
            this.bothButton.TabIndex = 2;
            this.bothButton.TabStop = true;
            this.bothButton.Text = "Both";
            this.bothButton.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "IP Address Suggestion:";
            // 
            // ipAddrSuggestionTB
            // 
            this.ipAddrSuggestionTB.Location = new System.Drawing.Point(135, 179);
            this.ipAddrSuggestionTB.Name = "ipAddrSuggestionTB";
            this.ipAddrSuggestionTB.Size = new System.Drawing.Size(118, 20);
            this.ipAddrSuggestionTB.TabIndex = 18;
            // 
            // useModifiedNameCB
            // 
            this.useModifiedNameCB.AutoSize = true;
            this.useModifiedNameCB.Location = new System.Drawing.Point(12, 153);
            this.useModifiedNameCB.Name = "useModifiedNameCB";
            this.useModifiedNameCB.Size = new System.Drawing.Size(207, 17);
            this.useModifiedNameCB.TabIndex = 16;
            this.useModifiedNameCB.Text = "Use modified username for accounting";
            this.useModifiedNameCB.UseVisualStyleBackColor = true;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 231);
            this.Controls.Add(this.useModifiedNameCB);
            this.Controls.Add(this.ipAddrSuggestionTB);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.groupBox1);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton bothButton;
        private System.Windows.Forms.RadioButton machineNameButton;
        private System.Windows.Forms.RadioButton ipAddressButton;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ipAddrSuggestionTB;
        private System.Windows.Forms.CheckBox useModifiedNameCB;
    }
}