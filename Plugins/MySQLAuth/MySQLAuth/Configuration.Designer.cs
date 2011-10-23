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
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dbTB = new System.Windows.Forms.TextBox();
            this.tableTB = new System.Windows.Forms.TextBox();
            this.createTableBtn = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(263, 265);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 25);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(350, 265);
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
            this.useSslCB.Location = new System.Drawing.Point(72, 124);
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
            this.groupBox1.Size = new System.Drawing.Size(419, 158);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MySQL Server";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableTB);
            this.groupBox2.Controls.Add(this.dbTB);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 176);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(417, 84);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "User Database";
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(71, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Table:";
            // 
            // dbTB
            // 
            this.dbTB.Location = new System.Drawing.Point(114, 19);
            this.dbTB.Name = "dbTB";
            this.dbTB.Size = new System.Drawing.Size(297, 20);
            this.dbTB.TabIndex = 2;
            // 
            // tableTB
            // 
            this.tableTB.Location = new System.Drawing.Point(114, 45);
            this.tableTB.Name = "tableTB";
            this.tableTB.Size = new System.Drawing.Size(297, 20);
            this.tableTB.TabIndex = 3;
            // 
            // createTableBtn
            // 
            this.createTableBtn.Location = new System.Drawing.Point(96, 266);
            this.createTableBtn.Name = "createTableBtn";
            this.createTableBtn.Size = new System.Drawing.Size(100, 24);
            this.createTableBtn.TabIndex = 14;
            this.createTableBtn.Text = "Create Table...";
            this.createTableBtn.UseVisualStyleBackColor = true;
            this.createTableBtn.Click += new System.EventHandler(this.createTableBtn_Click);
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(14, 266);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(76, 24);
            this.testBtn.TabIndex = 15;
            this.testBtn.Text = "Test...";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 306);
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
    }
}