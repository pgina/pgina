namespace pGina.Plugin.SessionLimit
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
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.globalLimitTB = new System.Windows.Forms.TextBox();
            this.globalLimitLbl = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(227, 100);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(81, 26);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(140, 100);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 26);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // globalLimitTB
            // 
            this.globalLimitTB.Location = new System.Drawing.Point(152, 23);
            this.globalLimitTB.Name = "globalLimitTB";
            this.globalLimitTB.Size = new System.Drawing.Size(145, 20);
            this.globalLimitTB.TabIndex = 2;
            // 
            // globalLimitLbl
            // 
            this.globalLimitLbl.AutoSize = true;
            this.globalLimitLbl.Location = new System.Drawing.Point(12, 26);
            this.globalLimitLbl.Name = "globalLimitLbl";
            this.globalLimitLbl.Size = new System.Drawing.Size(130, 13);
            this.globalLimitLbl.TabIndex = 3;
            this.globalLimitLbl.Text = "Global time limit (minutes): ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(244, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "A value of 0 means sessions will not be logged off.";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 138);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.globalLimitLbl);
            this.Controls.Add(this.globalLimitTB);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Name = "Configuration";
            this.Text = "Session Limit Plugin Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox globalLimitTB;
        private System.Windows.Forms.Label globalLimitLbl;
        private System.Windows.Forms.Label label1;
    }
}