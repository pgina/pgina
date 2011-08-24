namespace pGina.Plugin.MySqlLogger
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
            this.label1 = new System.Windows.Forms.Label();
            this.hostTB = new System.Windows.Forms.TextBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTB = new System.Windows.Forms.TextBox();
            this.dbLabel = new System.Windows.Forms.Label();
            this.dbTB = new System.Windows.Forms.TextBox();
            this.userLabel = new System.Windows.Forms.Label();
            this.userTB = new System.Windows.Forms.TextBox();
            this.passwdLabel = new System.Windows.Forms.Label();
            this.passwdTB = new System.Windows.Forms.TextBox();
            this.testButton = new System.Windows.Forms.Button();
            this.createTableBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Host:";
            // 
            // hostTB
            // 
            this.hostTB.Location = new System.Drawing.Point(84, 12);
            this.hostTB.Name = "hostTB";
            this.hostTB.Size = new System.Drawing.Size(292, 20);
            this.hostTB.TabIndex = 1;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(306, 173);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(70, 28);
            this.okBtn.TabIndex = 9;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(230, 173);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(70, 28);
            this.cancelBtn.TabIndex = 8;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(12, 41);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 11;
            this.portLabel.Text = "Port:";
            // 
            // portTB
            // 
            this.portTB.Location = new System.Drawing.Point(84, 38);
            this.portTB.Name = "portTB";
            this.portTB.Size = new System.Drawing.Size(105, 20);
            this.portTB.TabIndex = 2;
            // 
            // dbLabel
            // 
            this.dbLabel.AutoSize = true;
            this.dbLabel.Location = new System.Drawing.Point(12, 67);
            this.dbLabel.Name = "dbLabel";
            this.dbLabel.Size = new System.Drawing.Size(56, 13);
            this.dbLabel.TabIndex = 12;
            this.dbLabel.Text = "Database:";
            // 
            // dbTB
            // 
            this.dbTB.Location = new System.Drawing.Point(84, 64);
            this.dbTB.Name = "dbTB";
            this.dbTB.Size = new System.Drawing.Size(291, 20);
            this.dbTB.TabIndex = 3;
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(12, 110);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(32, 13);
            this.userLabel.TabIndex = 13;
            this.userLabel.Text = "User:";
            // 
            // userTB
            // 
            this.userTB.Location = new System.Drawing.Point(84, 107);
            this.userTB.Name = "userTB";
            this.userTB.Size = new System.Drawing.Size(289, 20);
            this.userTB.TabIndex = 4;
            // 
            // passwdLabel
            // 
            this.passwdLabel.AutoSize = true;
            this.passwdLabel.Location = new System.Drawing.Point(12, 136);
            this.passwdLabel.Name = "passwdLabel";
            this.passwdLabel.Size = new System.Drawing.Size(56, 13);
            this.passwdLabel.TabIndex = 14;
            this.passwdLabel.Text = "Password:";
            // 
            // passwdTB
            // 
            this.passwdTB.Location = new System.Drawing.Point(84, 133);
            this.passwdTB.Name = "passwdTB";
            this.passwdTB.Size = new System.Drawing.Size(288, 20);
            this.passwdTB.TabIndex = 5;
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(12, 172);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(67, 27);
            this.testButton.TabIndex = 6;
            this.testButton.Text = "Test...";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // createTableBtn
            // 
            this.createTableBtn.Location = new System.Drawing.Point(85, 173);
            this.createTableBtn.Name = "createTableBtn";
            this.createTableBtn.Size = new System.Drawing.Size(91, 26);
            this.createTableBtn.TabIndex = 7;
            this.createTableBtn.Text = "Create Table...";
            this.createTableBtn.UseVisualStyleBackColor = true;
            this.createTableBtn.Click += new System.EventHandler(this.createTableBtn_Click);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 213);
            this.Controls.Add(this.createTableBtn);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.passwdTB);
            this.Controls.Add(this.passwdLabel);
            this.Controls.Add(this.userTB);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.dbTB);
            this.Controls.Add(this.dbLabel);
            this.Controls.Add(this.portTB);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.hostTB);
            this.Controls.Add(this.label1);
            this.Name = "Configuration";
            this.Text = "MySQL Logger Plugin Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hostTB;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTB;
        private System.Windows.Forms.Label dbLabel;
        private System.Windows.Forms.TextBox dbTB;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.TextBox userTB;
        private System.Windows.Forms.Label passwdLabel;
        private System.Windows.Forms.TextBox passwdTB;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button createTableBtn;
    }
}