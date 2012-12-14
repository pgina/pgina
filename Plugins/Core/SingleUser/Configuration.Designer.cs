namespace pGina.Plugin.SingleUser
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_txtUser = new System.Windows.Forms.TextBox();
            this.m_txtDomain = new System.Windows.Forms.TextBox();
            this.m_txtPass = new System.Windows.Forms.TextBox();
            this.m_dgv = new System.Windows.Forms.DataGridView();
            this.PluginUuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.substituteCB = new System.Windows.Forms.CheckBox();
            this.anyRB = new System.Windows.Forms.RadioButton();
            this.allRB = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(314, 262);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(395, 262);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Domain:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password:";
            // 
            // m_txtUser
            // 
            this.m_txtUser.Location = new System.Drawing.Point(88, 22);
            this.m_txtUser.Name = "m_txtUser";
            this.m_txtUser.Size = new System.Drawing.Size(274, 20);
            this.m_txtUser.TabIndex = 8;
            // 
            // m_txtDomain
            // 
            this.m_txtDomain.Location = new System.Drawing.Point(88, 48);
            this.m_txtDomain.Name = "m_txtDomain";
            this.m_txtDomain.Size = new System.Drawing.Size(274, 20);
            this.m_txtDomain.TabIndex = 9;
            // 
            // m_txtPass
            // 
            this.m_txtPass.Location = new System.Drawing.Point(88, 74);
            this.m_txtPass.Name = "m_txtPass";
            this.m_txtPass.PasswordChar = '*';
            this.m_txtPass.Size = new System.Drawing.Size(274, 20);
            this.m_txtPass.TabIndex = 10;
            this.m_txtPass.UseSystemPasswordChar = true;
            // 
            // m_dgv
            // 
            this.m_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PluginUuid});
            this.m_dgv.Enabled = false;
            this.m_dgv.Location = new System.Drawing.Point(12, 138);
            this.m_dgv.Name = "m_dgv";
            this.m_dgv.Size = new System.Drawing.Size(458, 118);
            this.m_dgv.TabIndex = 12;
            // 
            // PluginUuid
            // 
            this.PluginUuid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.PluginUuid.HeaderText = "Plugin Unique ID";
            this.PluginUuid.Name = "PluginUuid";
            // 
            // substituteCB
            // 
            this.substituteCB.AutoSize = true;
            this.substituteCB.Location = new System.Drawing.Point(12, 115);
            this.substituteCB.Name = "substituteCB";
            this.substituteCB.Size = new System.Drawing.Size(103, 17);
            this.substituteCB.TabIndex = 13;
            this.substituteCB.Text = "Only substitute if";
            this.substituteCB.UseVisualStyleBackColor = true;
            this.substituteCB.CheckedChanged += new System.EventHandler(this.requirePluginCheckChange);
            // 
            // anyRB
            // 
            this.anyRB.AutoSize = true;
            this.anyRB.Location = new System.Drawing.Point(117, 114);
            this.anyRB.Name = "anyRB";
            this.anyRB.Size = new System.Drawing.Size(42, 17);
            this.anyRB.TabIndex = 14;
            this.anyRB.TabStop = true;
            this.anyRB.Text = "any";
            this.anyRB.UseVisualStyleBackColor = true;
            // 
            // allRB
            // 
            this.allRB.AutoSize = true;
            this.allRB.Location = new System.Drawing.Point(161, 114);
            this.allRB.Name = "allRB";
            this.allRB.Size = new System.Drawing.Size(35, 17);
            this.allRB.TabIndex = 15;
            this.allRB.TabStop = true;
            this.allRB.Text = "all";
            this.allRB.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(198, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(272, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "of the following plugins have successfully authenticated.";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 297);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.allRB);
            this.Controls.Add(this.anyRB);
            this.Controls.Add(this.substituteCB);
            this.Controls.Add(this.m_dgv);
            this.Controls.Add(this.m_txtPass);
            this.Controls.Add(this.m_txtDomain);
            this.Controls.Add(this.m_txtUser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "Configuration";
            this.Text = "Single User Login Plugin Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.m_dgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_txtUser;
        private System.Windows.Forms.TextBox m_txtDomain;
        private System.Windows.Forms.TextBox m_txtPass;
        private System.Windows.Forms.DataGridView m_dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn PluginUuid;
        private System.Windows.Forms.CheckBox substituteCB;
        private System.Windows.Forms.RadioButton anyRB;
        private System.Windows.Forms.RadioButton allRB;
        private System.Windows.Forms.Label label4;
    }
}