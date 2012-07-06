namespace pGina.Plugin.MySQLAuth
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.hostLabel = new System.Windows.Forms.Label();
            this.hostTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.portTB = new System.Windows.Forms.TextBox();
            this.userTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.passwordTB = new System.Windows.Forms.TextBox();
            this.passwdCB = new System.Windows.Forms.CheckBox();
            this.useSslCB = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.encBase64RB = new System.Windows.Forms.RadioButton();
            this.encHexRB = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.tableTB = new System.Windows.Forms.TextBox();
            this.dbTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.createTableBtn = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.unameColTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.hashMethodColTB = new System.Windows.Forms.TextBox();
            this.passwdColTB = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(352, 369);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 25);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Close";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(262, 369);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(82, 25);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // hostLabel
            // 
            this.hostLabel.AutoSize = true;
            this.hostLabel.Location = new System.Drawing.Point(34, 23);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(32, 13);
            this.hostLabel.TabIndex = 2;
            this.hostLabel.Text = "Host:";
            // 
            // hostTB
            // 
            this.hostTB.Location = new System.Drawing.Point(72, 20);
            this.hostTB.Name = "hostTB";
            this.hostTB.Size = new System.Drawing.Size(264, 20);
            this.hostTB.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "User:";
            // 
            // portTB
            // 
            this.portTB.Location = new System.Drawing.Point(72, 46);
            this.portTB.Name = "portTB";
            this.portTB.Size = new System.Drawing.Size(68, 20);
            this.portTB.TabIndex = 6;
            // 
            // userTB
            // 
            this.userTB.Location = new System.Drawing.Point(72, 72);
            this.userTB.Name = "userTB";
            this.userTB.Size = new System.Drawing.Size(263, 20);
            this.userTB.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Password:";
            // 
            // passwordTB
            // 
            this.passwordTB.Location = new System.Drawing.Point(72, 98);
            this.passwordTB.Name = "passwordTB";
            this.passwordTB.Size = new System.Drawing.Size(263, 20);
            this.passwordTB.TabIndex = 9;
            this.passwordTB.UseSystemPasswordChar = true;
            // 
            // passwdCB
            // 
            this.passwdCB.AutoSize = true;
            this.passwdCB.Location = new System.Drawing.Point(340, 100);
            this.passwdCB.Name = "passwdCB";
            this.passwdCB.Size = new System.Drawing.Size(77, 17);
            this.passwdCB.TabIndex = 10;
            this.passwdCB.Text = "Show Text";
            this.passwdCB.UseVisualStyleBackColor = true;
            this.passwdCB.CheckedChanged += new System.EventHandler(this.passwdCB_CheckedChanged);
            // 
            // useSslCB
            // 
            this.useSslCB.AutoSize = true;
            this.useSslCB.Location = new System.Drawing.Point(174, 48);
            this.useSslCB.Name = "useSslCB";
            this.useSslCB.Size = new System.Drawing.Size(68, 17);
            this.useSslCB.TabIndex = 11;
            this.useSslCB.Text = "Use SSL";
            this.useSslCB.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.useSslCB);
            this.groupBox1.Controls.Add(this.passwdCB);
            this.groupBox1.Controls.Add(this.passwordTB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.userTB);
            this.groupBox1.Controls.Add(this.portTB);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.hostTB);
            this.groupBox1.Controls.Add(this.hostLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(419, 130);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MySQL Server";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.encBase64RB);
            this.groupBox2.Controls.Add(this.encHexRB);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.tableTB);
            this.groupBox2.Controls.Add(this.dbTB);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 148);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(417, 215);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "User Database";
            // 
            // encBase64RB
            // 
            this.encBase64RB.AutoSize = true;
            this.encBase64RB.Location = new System.Drawing.Point(211, 187);
            this.encBase64RB.Name = "encBase64RB";
            this.encBase64RB.Size = new System.Drawing.Size(64, 17);
            this.encBase64RB.TabIndex = 6;
            this.encBase64RB.TabStop = true;
            this.encBase64RB.Text = "Base 64";
            this.encBase64RB.UseVisualStyleBackColor = true;
            // 
            // encHexRB
            // 
            this.encHexRB.AutoSize = true;
            this.encHexRB.Location = new System.Drawing.Point(119, 187);
            this.encHexRB.Name = "encHexRB";
            this.encHexRB.Size = new System.Drawing.Size(86, 17);
            this.encHexRB.TabIndex = 5;
            this.encHexRB.TabStop = true;
            this.encHexRB.Text = "Hexadecimal";
            this.encHexRB.UseVisualStyleBackColor = true;
            this.encHexRB.CheckedChanged += new System.EventHandler(this.encHexRB_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 189);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Password encoding:";
            // 
            // tableTB
            // 
            this.tableTB.Location = new System.Drawing.Point(114, 45);
            this.tableTB.Name = "tableTB";
            this.tableTB.Size = new System.Drawing.Size(297, 20);
            this.tableTB.TabIndex = 3;
            // 
            // dbTB
            // 
            this.dbTB.Location = new System.Drawing.Point(114, 19);
            this.dbTB.Name = "dbTB";
            this.dbTB.Size = new System.Drawing.Size(297, 20);
            this.dbTB.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(71, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Table:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "MySQL Database:";
            // 
            // createTableBtn
            // 
            this.createTableBtn.Location = new System.Drawing.Point(94, 370);
            this.createTableBtn.Name = "createTableBtn";
            this.createTableBtn.Size = new System.Drawing.Size(100, 24);
            this.createTableBtn.TabIndex = 14;
            this.createTableBtn.Text = "Create Table...";
            this.createTableBtn.UseVisualStyleBackColor = true;
            this.createTableBtn.Click += new System.EventHandler(this.createTableBtn_Click);
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(12, 370);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(76, 24);
            this.testBtn.TabIndex = 15;
            this.testBtn.Text = "Test...";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(43, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Username:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.passwdColTB);
            this.groupBox3.Controls.Add(this.hashMethodColTB);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.unameColTB);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(6, 73);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(404, 108);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Database Columns";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // unameColTB
            // 
            this.unameColTB.Location = new System.Drawing.Point(108, 20);
            this.unameColTB.Name = "unameColTB";
            this.unameColTB.Size = new System.Drawing.Size(290, 20);
            this.unameColTB.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(29, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Hash Method:";
            // 
            // hashMethodColTB
            // 
            this.hashMethodColTB.Location = new System.Drawing.Point(108, 46);
            this.hashMethodColTB.Name = "hashMethodColTB";
            this.hashMethodColTB.Size = new System.Drawing.Size(290, 20);
            this.hashMethodColTB.TabIndex = 10;
            // 
            // passwdColTB
            // 
            this.passwdColTB.Location = new System.Drawing.Point(109, 72);
            this.passwdColTB.Name = "passwdColTB";
            this.passwdColTB.Size = new System.Drawing.Size(289, 20);
            this.passwdColTB.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(47, 75);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Password:";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 406);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.createTableBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Name = "Configuration";
            this.Text = "MySQL Authentication Plugin Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.TextBox hostTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox portTB;
        private System.Windows.Forms.TextBox userTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox passwordTB;
        private System.Windows.Forms.CheckBox passwdCB;
        private System.Windows.Forms.CheckBox useSslCB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tableTB;
        private System.Windows.Forms.TextBox dbTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button createTableBtn;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.RadioButton encHexRB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton encBase64RB;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox unameColTB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox passwdColTB;
        private System.Windows.Forms.TextBox hashMethodColTB;
        private System.Windows.Forms.Label label8;
    }
}