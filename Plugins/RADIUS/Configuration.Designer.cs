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
            this.portTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.secretTB = new System.Windows.Forms.TextBox();
            this.showSecretCB = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timeoutTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(212, 98);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(293, 98);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // serverTB
            // 
            this.serverTB.Location = new System.Drawing.Point(62, 13);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(126, 20);
            this.serverTB.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Server:";
            // 
            // portTB
            // 
            this.portTB.Location = new System.Drawing.Point(244, 12);
            this.portTB.Name = "portTB";
            this.portTB.Size = new System.Drawing.Size(54, 20);
            this.portTB.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(209, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Shared Secret:";
            // 
            // secretTB
            // 
            this.secretTB.Location = new System.Drawing.Point(100, 40);
            this.secretTB.Name = "secretTB";
            this.secretTB.Size = new System.Drawing.Size(88, 20);
            this.secretTB.TabIndex = 10;
            // 
            // showSecretCB
            // 
            this.showSecretCB.AutoSize = true;
            this.showSecretCB.Location = new System.Drawing.Point(212, 43);
            this.showSecretCB.Name = "showSecretCB";
            this.showSecretCB.Size = new System.Drawing.Size(85, 17);
            this.showSecretCB.TabIndex = 11;
            this.showSecretCB.Text = "Show secret";
            this.showSecretCB.UseVisualStyleBackColor = true;
            this.showSecretCB.CheckedChanged += new System.EventHandler(this.showSecretChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Timeout: ";
            // 
            // timeoutTB
            // 
            this.timeoutTB.Location = new System.Drawing.Point(62, 70);
            this.timeoutTB.Name = "timeoutTB";
            this.timeoutTB.Size = new System.Drawing.Size(28, 20);
            this.timeoutTB.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(93, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "seconds";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 133);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.timeoutTB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.showSecretCB);
            this.Controls.Add(this.secretTB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.portTB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverTB);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "Configuration";
            this.Text = "RADIUS Plugin Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox serverTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox portTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox secretTB;
        private System.Windows.Forms.CheckBox showSecretCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox timeoutTB;
        private System.Windows.Forms.Label label5;
    }
}