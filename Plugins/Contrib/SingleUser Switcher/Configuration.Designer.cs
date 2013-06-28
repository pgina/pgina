namespace pGina.Plugin.SingleUserSwitcher
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
            this.ldapGroup = new System.Windows.Forms.TextBox();
            this.sessionDeleteButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.altUsername = new System.Windows.Forms.TextBox();
            this.sessionsList = new System.Windows.Forms.ListBox();
            this.addSession = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.altPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(327, 524);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(408, 524);
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
            this.label1.Location = new System.Drawing.Point(16, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Domain (for any session) :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password:";
            // 
            // m_txtUser
            // 
            this.m_txtUser.Location = new System.Drawing.Point(91, 62);
            this.m_txtUser.Name = "m_txtUser";
            this.m_txtUser.Size = new System.Drawing.Size(327, 20);
            this.m_txtUser.TabIndex = 8;
            // 
            // m_txtDomain
            // 
            this.m_txtDomain.Location = new System.Drawing.Point(144, 19);
            this.m_txtDomain.Name = "m_txtDomain";
            this.m_txtDomain.Size = new System.Drawing.Size(274, 20);
            this.m_txtDomain.TabIndex = 2;
            // 
            // m_txtPass
            // 
            this.m_txtPass.Location = new System.Drawing.Point(91, 87);
            this.m_txtPass.Name = "m_txtPass";
            this.m_txtPass.PasswordChar = '*';
            this.m_txtPass.Size = new System.Drawing.Size(327, 20);
            this.m_txtPass.TabIndex = 9;
            this.m_txtPass.UseSystemPasswordChar = true;
            // 
            // m_dgv
            // 
            this.m_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PluginUuid});
            this.m_dgv.Enabled = false;
            this.m_dgv.Location = new System.Drawing.Point(19, 379);
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
            this.substituteCB.Location = new System.Drawing.Point(19, 356);
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
            this.anyRB.Location = new System.Drawing.Point(124, 355);
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
            this.allRB.Location = new System.Drawing.Point(168, 355);
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
            this.label4.Location = new System.Drawing.Point(205, 357);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(272, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "of the following plugins have successfully authenticated.";
            // 
            // ldapGroup
            // 
            this.ldapGroup.Location = new System.Drawing.Point(157, 255);
            this.ldapGroup.Name = "ldapGroup";
            this.ldapGroup.Size = new System.Drawing.Size(131, 20);
            this.ldapGroup.TabIndex = 28;
            // 
            // sessionDeleteButton
            // 
            this.sessionDeleteButton.Location = new System.Drawing.Point(460, 170);
            this.sessionDeleteButton.Name = "sessionDeleteButton";
            this.sessionDeleteButton.Size = new System.Drawing.Size(88, 34);
            this.sessionDeleteButton.TabIndex = 26;
            this.sessionDeleteButton.Text = "Remove session";
            this.sessionDeleteButton.UseVisualStyleBackColor = true;
            this.sessionDeleteButton.Click += new System.EventHandler(this.sessionDeleteButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(294, 258);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(143, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "open session with username:";
            // 
            // altUsername
            // 
            this.altUsername.Location = new System.Drawing.Point(443, 255);
            this.altUsername.Name = "altUsername";
            this.altUsername.Size = new System.Drawing.Size(105, 20);
            this.altUsername.TabIndex = 29;
            // 
            // sessionsList
            // 
            this.sessionsList.FormattingEnabled = true;
            this.sessionsList.Location = new System.Drawing.Point(19, 139);
            this.sessionsList.Name = "sessionsList";
            this.sessionsList.Size = new System.Drawing.Size(425, 95);
            this.sessionsList.TabIndex = 24;
            // 
            // addSession
            // 
            this.addSession.Location = new System.Drawing.Point(473, 307);
            this.addSession.Name = "addSession";
            this.addSession.Size = new System.Drawing.Size(75, 23);
            this.addSession.TabIndex = 31;
            this.addSession.Text = "Add Session";
            this.addSession.UseVisualStyleBackColor = true;
            this.addSession.Click += new System.EventHandler(this.addSession_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 258);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "If member of LDAP group: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(382, 284);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 31;
            this.label7.Text = "password:";
            // 
            // altPassword
            // 
            this.altPassword.Location = new System.Drawing.Point(443, 281);
            this.altPassword.Name = "altPassword";
            this.altPassword.PasswordChar = '*';
            this.altPassword.Size = new System.Drawing.Size(105, 20);
            this.altPassword.TabIndex = 30;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(16, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(102, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "Default session :";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(19, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(128, 13);
            this.label9.TabIndex = 33;
            this.label9.Text = "Alternative sessions :";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 566);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.altPassword);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ldapGroup);
            this.Controls.Add(this.sessionDeleteButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.altUsername);
            this.Controls.Add(this.sessionsList);
            this.Controls.Add(this.addSession);
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
            this.Text = "Single User Switcher Plugin Configuration";
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
        private System.Windows.Forms.TextBox ldapGroup;
        private System.Windows.Forms.Button sessionDeleteButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox altUsername;
        private System.Windows.Forms.ListBox sessionsList;
        private System.Windows.Forms.Button addSession;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox altPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}