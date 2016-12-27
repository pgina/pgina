namespace pGina.Plugin.SSHAuth
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
            this.hostLabel = new System.Windows.Forms.Label();
            this.hostText = new System.Windows.Forms.TextBox();
            this.save = new System.Windows.Forms.Button();
            this.hostDescription = new System.Windows.Forms.Label();
            this.cancel = new System.Windows.Forms.Button();
            this.help = new System.Windows.Forms.Button();
            this.portLabel = new System.Windows.Forms.Label();
            this.portText = new System.Windows.Forms.TextBox();
            this.portDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // hostLabel
            //
            this.hostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hostLabel.AutoSize = true;
            this.hostLabel.Location = new System.Drawing.Point(8, 9);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(32, 13);
            this.hostLabel.TabIndex = 0;
            this.hostLabel.Text = "Host:";
            this.hostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // hostText
            //
            this.hostText.Location = new System.Drawing.Point(46, 6);
            this.hostText.Name = "hostText";
            this.hostText.Size = new System.Drawing.Size(214, 20);
            this.hostText.TabIndex = 1;
            //
            // save
            //
            this.save.Location = new System.Drawing.Point(185, 114);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 5;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            //
            // hostDescription
            //
            this.hostDescription.Location = new System.Drawing.Point(43, 29);
            this.hostDescription.Name = "hostDescription";
            this.hostDescription.Size = new System.Drawing.Size(217, 28);
            this.hostDescription.TabIndex = 2;
            this.hostDescription.Text = "Hostname or address of SSH Server";
            this.hostDescription.Click += new System.EventHandler(this.description_Click);
            //
            // cancel
            //
            this.cancel.Location = new System.Drawing.Point(105, 114);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            //
            // help
            //
            this.help.Location = new System.Drawing.Point(24, 114);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(75, 23);
            this.help.TabIndex = 3;
            this.help.Text = "Help";
            this.help.UseVisualStyleBackColor = true;
            //
            // portLabel
            //
            this.portLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(11, 63);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 6;
            this.portLabel.Text = "Port:";
            this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.portLabel.Click += new System.EventHandler(this.label1_Click);
            //
            // portText
            //
            this.portText.Location = new System.Drawing.Point(46, 60);
            this.portText.Name = "portText";
            this.portText.Size = new System.Drawing.Size(214, 20);
            this.portText.TabIndex = 7;
            //
            // portDescription
            //
            this.portDescription.Location = new System.Drawing.Point(44, 83);
            this.portDescription.Name = "portDescription";
            this.portDescription.Size = new System.Drawing.Size(216, 28);
            this.portDescription.TabIndex = 8;
            this.portDescription.Text = "Port or service of SSH server (usually 22)";
            this.portDescription.Click += new System.EventHandler(this.label1_Click_1);
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 154);
            this.Controls.Add(this.portDescription);
            this.Controls.Add(this.portText);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.help);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.hostDescription);
            this.Controls.Add(this.save);
            this.Controls.Add(this.hostText);
            this.Controls.Add(this.hostLabel);
            this.Name = "Configuration";
            this.Text = "SSHAuth Configuration";
            this.Click += new System.EventHandler(this.Btn_help);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.TextBox hostText;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Label hostDescription;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button help;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portText;
        private System.Windows.Forms.Label portDescription;
    }
}