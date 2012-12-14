namespace pGina.Plugin.LocalMachine
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_chkAlwaysAuth = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.m_chkMirror = new System.Windows.Forms.CheckBox();
            this.m_chkAuthzRequireLocal = new System.Windows.Forms.CheckBox();
            this.m_localGroupDgv = new System.Windows.Forms.DataGridView();
            this.LocalGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_chkAuthzLocalAdmin = new System.Windows.Forms.CheckBox();
            this.m_chkAuthzAll = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.m_scrambleAllExceptDGV = new System.Windows.Forms.DataGridView();
            this.Username = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_chkRemoveProfile = new System.Windows.Forms.CheckBox();
            this.m_chkScramble = new System.Windows.Forms.CheckBox();
            this.m_chkGroupFailIsFAIL = new System.Windows.Forms.CheckBox();
            this.m_groupsDgv = new System.Windows.Forms.DataGridView();
            this.Group = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_btnSave = new System.Windows.Forms.Button();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_chkScrambleWhenLMFails = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_localGroupDgv)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_scrambleAllExceptDGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_groupsDgv)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_chkAlwaysAuth);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(342, 54);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authentication";
            // 
            // m_chkAlwaysAuth
            // 
            this.m_chkAlwaysAuth.AutoSize = true;
            this.m_chkAlwaysAuth.Location = new System.Drawing.Point(15, 23);
            this.m_chkAlwaysAuth.Name = "m_chkAlwaysAuth";
            this.m_chkAlwaysAuth.Size = new System.Drawing.Size(174, 17);
            this.m_chkAlwaysAuth.TabIndex = 0;
            this.m_chkAlwaysAuth.Text = "Always authenticate local users";
            this.m_chkAlwaysAuth.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.m_chkMirror);
            this.groupBox2.Controls.Add(this.m_chkAuthzRequireLocal);
            this.groupBox2.Controls.Add(this.m_localGroupDgv);
            this.groupBox2.Controls.Add(this.m_chkAuthzLocalAdmin);
            this.groupBox2.Controls.Add(this.m_chkAuthzAll);
            this.groupBox2.Location = new System.Drawing.Point(12, 73);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(343, 333);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Authorization";
            // 
            // m_chkMirror
            // 
            this.m_chkMirror.AutoSize = true;
            this.m_chkMirror.Location = new System.Drawing.Point(15, 21);
            this.m_chkMirror.Name = "m_chkMirror";
            this.m_chkMirror.Size = new System.Drawing.Size(158, 17);
            this.m_chkMirror.TabIndex = 4;
            this.m_chkMirror.Text = "Mirror groups from local user";
            this.m_chkMirror.UseVisualStyleBackColor = true;
            // 
            // m_chkAuthzRequireLocal
            // 
            this.m_chkAuthzRequireLocal.AutoSize = true;
            this.m_chkAuthzRequireLocal.Location = new System.Drawing.Point(16, 90);
            this.m_chkAuthzRequireLocal.Name = "m_chkAuthzRequireLocal";
            this.m_chkAuthzRequireLocal.Size = new System.Drawing.Size(291, 17);
            this.m_chkAuthzRequireLocal.TabIndex = 3;
            this.m_chkAuthzRequireLocal.Text = "Require membership in one of the following local groups:";
            this.m_chkAuthzRequireLocal.UseVisualStyleBackColor = true;
            this.m_chkAuthzRequireLocal.CheckedChanged += new System.EventHandler(this.m_chkAuthzRequireLocal_CheckedChanged);
            // 
            // m_localGroupDgv
            // 
            this.m_localGroupDgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_localGroupDgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LocalGroup});
            this.m_localGroupDgv.Location = new System.Drawing.Point(15, 113);
            this.m_localGroupDgv.Name = "m_localGroupDgv";
            this.m_localGroupDgv.Size = new System.Drawing.Size(311, 200);
            this.m_localGroupDgv.TabIndex = 2;
            // 
            // LocalGroup
            // 
            this.LocalGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LocalGroup.HeaderText = "Local Groups";
            this.LocalGroup.Name = "LocalGroup";
            // 
            // m_chkAuthzLocalAdmin
            // 
            this.m_chkAuthzLocalAdmin.AutoSize = true;
            this.m_chkAuthzLocalAdmin.Location = new System.Drawing.Point(15, 67);
            this.m_chkAuthzLocalAdmin.Name = "m_chkAuthzLocalAdmin";
            this.m_chkAuthzLocalAdmin.Size = new System.Drawing.Size(239, 17);
            this.m_chkAuthzLocalAdmin.TabIndex = 1;
            this.m_chkAuthzLocalAdmin.Text = "Require local administrator group membership";
            this.m_chkAuthzLocalAdmin.UseVisualStyleBackColor = true;
            // 
            // m_chkAuthzAll
            // 
            this.m_chkAuthzAll.AutoSize = true;
            this.m_chkAuthzAll.Location = new System.Drawing.Point(15, 44);
            this.m_chkAuthzAll.Name = "m_chkAuthzAll";
            this.m_chkAuthzAll.Size = new System.Drawing.Size(179, 17);
            this.m_chkAuthzAll.TabIndex = 0;
            this.m_chkAuthzAll.Text = "Authorize all authenticated users";
            this.m_chkAuthzAll.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.m_chkScrambleWhenLMFails);
            this.groupBox3.Controls.Add(this.m_scrambleAllExceptDGV);
            this.groupBox3.Controls.Add(this.m_chkRemoveProfile);
            this.groupBox3.Controls.Add(this.m_chkScramble);
            this.groupBox3.Controls.Add(this.m_chkGroupFailIsFAIL);
            this.groupBox3.Controls.Add(this.m_groupsDgv);
            this.groupBox3.Location = new System.Drawing.Point(361, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(343, 393);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Gateway";
            // 
            // m_scrambleAllExceptDGV
            // 
            this.m_scrambleAllExceptDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_scrambleAllExceptDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Username});
            this.m_scrambleAllExceptDGV.Location = new System.Drawing.Point(54, 154);
            this.m_scrambleAllExceptDGV.Name = "m_scrambleAllExceptDGV";
            this.m_scrambleAllExceptDGV.Size = new System.Drawing.Size(271, 84);
            this.m_scrambleAllExceptDGV.TabIndex = 11;
            // 
            // Username
            // 
            this.Username.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Username.HeaderText = "Username";
            this.Username.Name = "Username";
            // 
            // m_chkRemoveProfile
            // 
            this.m_chkRemoveProfile.AutoSize = true;
            this.m_chkRemoveProfile.Location = new System.Drawing.Point(15, 42);
            this.m_chkRemoveProfile.Name = "m_chkRemoveProfile";
            this.m_chkRemoveProfile.Size = new System.Drawing.Size(290, 30);
            this.m_chkRemoveProfile.TabIndex = 8;
            this.m_chkRemoveProfile.Text = "Remove account and profile after logout when account \r\ndoes not exist prior to lo" +
    "gon.";
            this.m_chkRemoveProfile.UseVisualStyleBackColor = true;
            this.m_chkRemoveProfile.CheckedChanged += new System.EventHandler(this.m_chkRemoveProfile_CheckedChanged);
            // 
            // m_chkScramble
            // 
            this.m_chkScramble.AutoSize = true;
            this.m_chkScramble.Location = new System.Drawing.Point(15, 78);
            this.m_chkScramble.Name = "m_chkScramble";
            this.m_chkScramble.Size = new System.Drawing.Size(177, 17);
            this.m_chkScramble.TabIndex = 7;
            this.m_chkScramble.Text = "Scramble password after logout.";
            this.m_chkScramble.UseVisualStyleBackColor = true;
            this.m_chkScramble.CheckedChanged += new System.EventHandler(this.m_chkScramble_CheckedChanged);
            // 
            // m_chkGroupFailIsFAIL
            // 
            this.m_chkGroupFailIsFAIL.AutoSize = true;
            this.m_chkGroupFailIsFAIL.Location = new System.Drawing.Point(15, 19);
            this.m_chkGroupFailIsFAIL.Name = "m_chkGroupFailIsFAIL";
            this.m_chkGroupFailIsFAIL.Size = new System.Drawing.Size(291, 17);
            this.m_chkGroupFailIsFAIL.TabIndex = 6;
            this.m_chkGroupFailIsFAIL.Text = "Failure to create or join local groups should prevent login";
            this.m_chkGroupFailIsFAIL.UseVisualStyleBackColor = true;
            // 
            // m_groupsDgv
            // 
            this.m_groupsDgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_groupsDgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Group});
            this.m_groupsDgv.Location = new System.Drawing.Point(15, 262);
            this.m_groupsDgv.Name = "m_groupsDgv";
            this.m_groupsDgv.Size = new System.Drawing.Size(311, 111);
            this.m_groupsDgv.TabIndex = 5;
            // 
            // Group
            // 
            this.Group.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Group.HeaderText = "Mandatory Groups";
            this.Group.Name = "Group";
            // 
            // m_btnSave
            // 
            this.m_btnSave.Location = new System.Drawing.Point(533, 412);
            this.m_btnSave.Name = "m_btnSave";
            this.m_btnSave.Size = new System.Drawing.Size(75, 23);
            this.m_btnSave.TabIndex = 3;
            this.m_btnSave.Text = "Save";
            this.m_btnSave.UseVisualStyleBackColor = true;
            this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
            // 
            // m_btnClose
            // 
            this.m_btnClose.Location = new System.Drawing.Point(618, 412);
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.Size = new System.Drawing.Size(75, 23);
            this.m_btnClose.TabIndex = 4;
            this.m_btnClose.Text = "Close";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.m_btnClose_Click);
            // 
            // m_chkScrambleWhenLMFails
            // 
            this.m_chkScrambleWhenLMFails.AutoSize = true;
            this.m_chkScrambleWhenLMFails.Location = new System.Drawing.Point(37, 101);
            this.m_chkScrambleWhenLMFails.Name = "m_chkScrambleWhenLMFails";
            this.m_chkScrambleWhenLMFails.Size = new System.Drawing.Size(297, 30);
            this.m_chkScrambleWhenLMFails.TabIndex = 12;
            this.m_chkScrambleWhenLMFails.Text = "Only scramble when LocalMachine authentication fails or \r\ndoes not execute.";
            this.m_chkScrambleWhenLMFails.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(51, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(246, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Never scramble the passwords for these accounts:";
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 447);
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_btnSave);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Configuration";
            this.Text = "LocalMachine Plugin Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_localGroupDgv)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_scrambleAllExceptDGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_groupsDgv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox m_chkAlwaysAuth;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox m_chkAuthzAll;
        private System.Windows.Forms.CheckBox m_chkAuthzLocalAdmin;
        private System.Windows.Forms.CheckBox m_chkAuthzRequireLocal;
        private System.Windows.Forms.DataGridView m_localGroupDgv;
        private System.Windows.Forms.CheckBox m_chkMirror;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView m_groupsDgv;
        private System.Windows.Forms.Button m_btnSave;
        private System.Windows.Forms.Button m_btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn Group;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocalGroup;
        private System.Windows.Forms.CheckBox m_chkGroupFailIsFAIL;
        private System.Windows.Forms.CheckBox m_chkScramble;
        private System.Windows.Forms.CheckBox m_chkRemoveProfile;
        private System.Windows.Forms.DataGridView m_scrambleAllExceptDGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn Username;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox m_chkScrambleWhenLMFails;
    }
}