namespace pGina.Plugin.LogonScriptFromLDAP
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
            this.components = new System.ComponentModel.Container();
            this.attribute_label = new System.Windows.Forms.Label();
            this.server_label = new System.Windows.Forms.Label();
            this.attribute_TextBox = new System.Windows.Forms.TextBox();
            this.server_TextBox = new System.Windows.Forms.TextBox();
            this.save_btn = new System.Windows.Forms.Button();
            this.cancel_btn = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.domain_TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // attribute_label
            // 
            this.attribute_label.AutoSize = true;
            this.attribute_label.Location = new System.Drawing.Point(45, 34);
            this.attribute_label.Name = "attribute_label";
            this.attribute_label.Size = new System.Drawing.Size(206, 13);
            this.attribute_label.TabIndex = 0;
            this.attribute_label.Text = "Attribute corresponding to the script name:";
            this.toolTip1.SetToolTip(this.attribute_label, "Name of the attribute corresponding to the name of the logon script you\'re lookin" +
        "g for");
            // 
            // server_label
            // 
            this.server_label.AutoSize = true;
            this.server_label.Location = new System.Drawing.Point(44, 94);
            this.server_label.Name = "server_label";
            this.server_label.Size = new System.Drawing.Size(163, 13);
            this.server_label.TabIndex = 0;
            this.server_label.Text = "Server to find the logon script on:";
            this.toolTip1.SetToolTip(this.server_label, "Address of the server to retrieve the logon script from");
            // 
            // attribute_TextBox
            // 
            this.attribute_TextBox.Location = new System.Drawing.Point(47, 50);
            this.attribute_TextBox.Name = "attribute_TextBox";
            this.attribute_TextBox.Size = new System.Drawing.Size(204, 20);
            this.attribute_TextBox.TabIndex = 2;
            this.toolTip1.SetToolTip(this.attribute_TextBox, "Name of the attribute corresponding to the name of the logon script you\'re lookin" +
        "g for");
            // 
            // server_TextBox
            // 
            this.server_TextBox.Location = new System.Drawing.Point(47, 110);
            this.server_TextBox.Name = "server_TextBox";
            this.server_TextBox.Size = new System.Drawing.Size(204, 20);
            this.server_TextBox.TabIndex = 3;
            this.toolTip1.SetToolTip(this.server_TextBox, "Address of the server to retrieve the logon script from");
            // 
            // save_btn
            // 
            this.save_btn.Location = new System.Drawing.Point(191, 215);
            this.save_btn.Name = "save_btn";
            this.save_btn.Size = new System.Drawing.Size(77, 31);
            this.save_btn.TabIndex = 1;
            this.save_btn.Text = "Save";
            this.save_btn.UseVisualStyleBackColor = true;
            this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
            // 
            // cancel_btn
            // 
            this.cancel_btn.Location = new System.Drawing.Point(109, 215);
            this.cancel_btn.Name = "cancel_btn";
            this.cancel_btn.Size = new System.Drawing.Size(77, 31);
            this.cancel_btn.TabIndex = 5;
            this.cancel_btn.Text = "Cancel";
            this.cancel_btn.UseVisualStyleBackColor = true;
            this.cancel_btn.Click += new System.EventHandler(this.cancel_btn_Click);
            // 
            // domain_TextBox
            // 
            this.domain_TextBox.Location = new System.Drawing.Point(47, 163);
            this.domain_TextBox.Name = "domain_TextBox";
            this.domain_TextBox.Size = new System.Drawing.Size(204, 20);
            this.domain_TextBox.TabIndex = 4;
            this.toolTip1.SetToolTip(this.domain_TextBox, "Windows domain to authenticate the user on");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 144);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Windows domain:";
            this.toolTip1.SetToolTip(this.label1, "Windows domain to authenticate the user on");
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 269);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.domain_TextBox);
            this.Controls.Add(this.cancel_btn);
            this.Controls.Add(this.save_btn);
            this.Controls.Add(this.server_TextBox);
            this.Controls.Add(this.attribute_TextBox);
            this.Controls.Add(this.server_label);
            this.Controls.Add(this.attribute_label);
            this.Name = "Configuration";
            this.Text = "LDAP Logon Script Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label attribute_label;
        private System.Windows.Forms.Label server_label;
        private System.Windows.Forms.TextBox attribute_TextBox;
        private System.Windows.Forms.TextBox server_TextBox;
        private System.Windows.Forms.Button cancel_btn;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox domain_TextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button save_btn;
    }
}