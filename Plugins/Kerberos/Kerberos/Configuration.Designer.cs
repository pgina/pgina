namespace pGina.Plugin.Kerberos
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
            this.rText = new System.Windows.Forms.TextBox();
            this.save = new System.Windows.Forms.Button();
            this.description = new System.Windows.Forms.Label();
            this.cancel = new System.Windows.Forms.Button();
            this.help = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Realm:";
            //
            // rText
            //
            this.rText.Location = new System.Drawing.Point(46, 6);
            this.rText.Name = "rText";
            this.rText.Size = new System.Drawing.Size(214, 20);
            this.rText.TabIndex = 1;
            //
            // save
            //
            this.save.Location = new System.Drawing.Point(185, 94);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 5;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            //
            // description
            //
            this.description.Location = new System.Drawing.Point(3, 29);
            this.description.Name = "description";
            this.description.Size = new System.Drawing.Size(251, 38);
            this.description.TabIndex = 2;
            this.description.Text = "Enter the target Kerberos realm name ex: REALM.UTAH.EDU";
            //
            // cancel
            //
            this.cancel.Location = new System.Drawing.Point(104, 94);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            //
            // help
            //
            this.help.Location = new System.Drawing.Point(23, 94);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(75, 23);
            this.help.TabIndex = 3;
            this.help.Text = "Help";
            this.help.UseVisualStyleBackColor = true;
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 129);
            this.Controls.Add(this.help);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.description);
            this.Controls.Add(this.save);
            this.Controls.Add(this.rText);
            this.Controls.Add(this.label1);
            this.Name = "Configuration";
            this.Text = "KRB5 Realm Configuration";
            this.Click += new System.EventHandler(this.Btn_help);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rText;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Label description;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button help;
    }
}