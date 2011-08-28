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
            this.m_chkRequirePlugin = new System.Windows.Forms.CheckBox();
            this.m_dgv = new System.Windows.Forms.DataGridView();
            this.PluginUuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(212, 237);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(293, 237);
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
            this.m_txtUser.Size = new System.Drawing.Size(159, 20);
            this.m_txtUser.TabIndex = 8;
            // 
            // m_txtDomain
            // 
            this.m_txtDomain.Location = new System.Drawing.Point(88, 48);
            this.m_txtDomain.Name = "m_txtDomain";
            this.m_txtDomain.Size = new System.Drawing.Size(159, 20);
            this.m_txtDomain.TabIndex = 9;
            // 
            // m_txtPass
            // 
            this.m_txtPass.Location = new System.Drawing.Point(88, 74);
            this.m_txtPass.Name = "m_txtPass";
            this.m_txtPass.PasswordChar = '*';
            this.m_txtPass.Size = new System.Drawing.Size(159, 20);
            this.m_txtPass.TabIndex = 10;
            this.m_txtPass.UseSystemPasswordChar = true;
            // 
            // m_chkRequirePlugin
            // 
            this.m_chkRequirePlugin.AutoSize = true;
            this.m_chkRequirePlugin.Location = new System.Drawing.Point(16, 114);
            this.m_chkRequirePlugin.Name = "m_chkRequirePlugin";
            this.m_chkRequirePlugin.Size = new System.Drawing.Size(346, 17);
            this.m_chkRequirePlugin.TabIndex = 11;
            this.m_chkRequirePlugin.Text = "Only substitute when one of the following plugins has authenticated:";
            this.m_chkRequirePlugin.UseVisualStyleBackColor = true;
            this.m_chkRequirePlugin.CheckedChanged += new System.EventHandler(this.m_chkRequirePlugin_CheckedChanged);
            // 
            // m_dgv
            // 
            this.m_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PluginUuid});
            this.m_dgv.Enabled = false;
            this.m_dgv.Location = new System.Drawing.Point(16, 137);
            this.m_dgv.Name = "m_dgv";
            this.m_dgv.Size = new System.Drawing.Size(346, 79);
            this.m_dgv.TabIndex = 12;
            // 
            // PluginUuid
            // 
            this.PluginUuid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.PluginUuid.HeaderText = "Plugin Unique ID";
            this.PluginUuid.Name = "PluginUuid";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 272);
            this.Controls.Add(this.m_dgv);
            this.Controls.Add(this.m_chkRequirePlugin);
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
        private System.Windows.Forms.CheckBox m_chkRequirePlugin;
        private System.Windows.Forms.DataGridView m_dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn PluginUuid;
    }
}