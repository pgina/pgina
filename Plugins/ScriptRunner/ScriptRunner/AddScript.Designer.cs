namespace pGina.Plugin.ScriptRunner
{
    partial class AddScript
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
            this.fileLabel = new System.Windows.Forms.Label();
            this.fileTB = new System.Windows.Forms.TextBox();
            this.browseFileBtn = new System.Windows.Forms.Button();
            this.batchRB = new System.Windows.Forms.RadioButton();
            this.powerShellRB = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.userSessionCB = new System.Windows.Forms.CheckBox();
            this.systemSessionCB = new System.Windows.Forms.CheckBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(12, 62);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(26, 13);
            this.fileLabel.TabIndex = 0;
            this.fileLabel.Text = "File:";
            // 
            // fileTB
            // 
            this.fileTB.Location = new System.Drawing.Point(68, 59);
            this.fileTB.Name = "fileTB";
            this.fileTB.Size = new System.Drawing.Size(290, 20);
            this.fileTB.TabIndex = 1;
            // 
            // browseFileBtn
            // 
            this.browseFileBtn.Location = new System.Drawing.Point(364, 56);
            this.browseFileBtn.Name = "browseFileBtn";
            this.browseFileBtn.Size = new System.Drawing.Size(69, 25);
            this.browseFileBtn.TabIndex = 2;
            this.browseFileBtn.Text = "Browse...";
            this.browseFileBtn.UseVisualStyleBackColor = true;
            this.browseFileBtn.Click += new System.EventHandler(this.browseFileBtn_Click);
            // 
            // batchRB
            // 
            this.batchRB.AutoSize = true;
            this.batchRB.Location = new System.Drawing.Point(87, 10);
            this.batchRB.Name = "batchRB";
            this.batchRB.Size = new System.Drawing.Size(53, 17);
            this.batchRB.TabIndex = 3;
            this.batchRB.TabStop = true;
            this.batchRB.Text = "Batch";
            this.batchRB.UseVisualStyleBackColor = true;
            // 
            // powerShellRB
            // 
            this.powerShellRB.AutoSize = true;
            this.powerShellRB.Location = new System.Drawing.Point(146, 10);
            this.powerShellRB.Name = "powerShellRB";
            this.powerShellRB.Size = new System.Drawing.Size(78, 17);
            this.powerShellRB.TabIndex = 4;
            this.powerShellRB.TabStop = true;
            this.powerShellRB.Text = "PowerShell";
            this.powerShellRB.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Script type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Run in:";
            // 
            // userSessionCB
            // 
            this.userSessionCB.AutoSize = true;
            this.userSessionCB.Location = new System.Drawing.Point(87, 36);
            this.userSessionCB.Name = "userSessionCB";
            this.userSessionCB.Size = new System.Drawing.Size(88, 17);
            this.userSessionCB.TabIndex = 7;
            this.userSessionCB.Text = "User Session";
            this.userSessionCB.UseVisualStyleBackColor = true;
            // 
            // systemSessionCB
            // 
            this.systemSessionCB.AutoSize = true;
            this.systemSessionCB.Location = new System.Drawing.Point(181, 36);
            this.systemSessionCB.Name = "systemSessionCB";
            this.systemSessionCB.Size = new System.Drawing.Size(100, 17);
            this.systemSessionCB.TabIndex = 8;
            this.systemSessionCB.Text = "System Session";
            this.systemSessionCB.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(359, 98);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(83, 22);
            this.okBtn.TabIndex = 9;
            this.okBtn.Text = "Add";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(261, 98);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(92, 22);
            this.cancelBtn.TabIndex = 10;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // AddScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 132);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.systemSessionCB);
            this.Controls.Add(this.userSessionCB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.powerShellRB);
            this.Controls.Add(this.batchRB);
            this.Controls.Add(this.browseFileBtn);
            this.Controls.Add(this.fileTB);
            this.Controls.Add(this.fileLabel);
            this.Name = "AddScript";
            this.Text = "AddScript";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.TextBox fileTB;
        private System.Windows.Forms.Button browseFileBtn;
        private System.Windows.Forms.RadioButton batchRB;
        private System.Windows.Forms.RadioButton powerShellRB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox userSessionCB;
        private System.Windows.Forms.CheckBox systemSessionCB;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}