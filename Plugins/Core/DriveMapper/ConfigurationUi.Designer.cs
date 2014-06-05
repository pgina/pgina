namespace pGina.Plugin.DriveMapper
{
    partial class ConfigurationUi
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
            this.m_mapListDgv = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_deleteMapButton = new System.Windows.Forms.Button();
            this.m_newMapButton = new System.Windows.Forms.Button();
            this.m_groupTb = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.m_showPasswordCb = new System.Windows.Forms.CheckBox();
            this.m_credsCb = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.m_passwordTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.m_usernameTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.m_uncPathTb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_driveLetterCb = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_saveCloseButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_mapListDgv)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_mapListDgv
            // 
            this.m_mapListDgv.AllowUserToAddRows = false;
            this.m_mapListDgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_mapListDgv.Location = new System.Drawing.Point(12, 12);
            this.m_mapListDgv.MultiSelect = false;
            this.m_mapListDgv.Name = "m_mapListDgv";
            this.m_mapListDgv.RowHeadersVisible = false;
            this.m_mapListDgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_mapListDgv.Size = new System.Drawing.Size(562, 303);
            this.m_mapListDgv.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_deleteMapButton);
            this.groupBox1.Controls.Add(this.m_newMapButton);
            this.groupBox1.Controls.Add(this.m_groupTb);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.m_showPasswordCb);
            this.groupBox1.Controls.Add(this.m_credsCb);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.m_passwordTb);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.m_usernameTb);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.m_uncPathTb);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.m_driveLetterCb);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 321);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 143);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // m_deleteMapButton
            // 
            this.m_deleteMapButton.Location = new System.Drawing.Point(481, 107);
            this.m_deleteMapButton.Name = "m_deleteMapButton";
            this.m_deleteMapButton.Size = new System.Drawing.Size(75, 23);
            this.m_deleteMapButton.TabIndex = 14;
            this.m_deleteMapButton.Text = "Delete";
            this.m_deleteMapButton.UseVisualStyleBackColor = true;
            this.m_deleteMapButton.Click += new System.EventHandler(this.m_deleteMapButton_Click);
            // 
            // m_newMapButton
            // 
            this.m_newMapButton.Location = new System.Drawing.Point(410, 107);
            this.m_newMapButton.Name = "m_newMapButton";
            this.m_newMapButton.Size = new System.Drawing.Size(65, 23);
            this.m_newMapButton.TabIndex = 13;
            this.m_newMapButton.Text = "New";
            this.m_newMapButton.UseVisualStyleBackColor = true;
            this.m_newMapButton.Click += new System.EventHandler(this.newMapButton_Click);
            // 
            // m_groupTb
            // 
            this.m_groupTb.Location = new System.Drawing.Point(220, 48);
            this.m_groupTb.Name = "m_groupTb";
            this.m_groupTb.Size = new System.Drawing.Size(336, 20);
            this.m_groupTb.TabIndex = 12;
            this.m_groupTb.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(175, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Group:";
            // 
            // m_showPasswordCb
            // 
            this.m_showPasswordCb.AutoSize = true;
            this.m_showPasswordCb.Location = new System.Drawing.Point(490, 77);
            this.m_showPasswordCb.Name = "m_showPasswordCb";
            this.m_showPasswordCb.Size = new System.Drawing.Size(53, 17);
            this.m_showPasswordCb.TabIndex = 10;
            this.m_showPasswordCb.Text = "Show";
            this.m_showPasswordCb.UseVisualStyleBackColor = true;
            this.m_showPasswordCb.CheckedChanged += new System.EventHandler(this.m_showPasswordCb_CheckedChanged);
            // 
            // m_credsCb
            // 
            this.m_credsCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_credsCb.FormattingEnabled = true;
            this.m_credsCb.Location = new System.Drawing.Point(70, 48);
            this.m_credsCb.Name = "m_credsCb";
            this.m_credsCb.Size = new System.Drawing.Size(99, 21);
            this.m_credsCb.TabIndex = 9;
            this.m_credsCb.SelectedIndexChanged += new System.EventHandler(this.m_credsCb_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Credentials:";
            // 
            // m_passwordTb
            // 
            this.m_passwordTb.Location = new System.Drawing.Point(267, 75);
            this.m_passwordTb.Name = "m_passwordTb";
            this.m_passwordTb.Size = new System.Drawing.Size(217, 20);
            this.m_passwordTb.TabIndex = 7;
            this.m_passwordTb.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(205, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password:";
            // 
            // m_usernameTb
            // 
            this.m_usernameTb.Location = new System.Drawing.Point(70, 75);
            this.m_usernameTb.Name = "m_usernameTb";
            this.m_usernameTb.Size = new System.Drawing.Size(129, 20);
            this.m_usernameTb.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Username:";
            // 
            // m_uncPathTb
            // 
            this.m_uncPathTb.Location = new System.Drawing.Point(190, 20);
            this.m_uncPathTb.Name = "m_uncPathTb";
            this.m_uncPathTb.Size = new System.Drawing.Size(366, 20);
            this.m_uncPathTb.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(126, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "UNC Path:";
            // 
            // m_driveLetterCb
            // 
            this.m_driveLetterCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_driveLetterCb.FormattingEnabled = true;
            this.m_driveLetterCb.Location = new System.Drawing.Point(70, 20);
            this.m_driveLetterCb.Name = "m_driveLetterCb";
            this.m_driveLetterCb.Size = new System.Drawing.Size(50, 21);
            this.m_driveLetterCb.TabIndex = 1;
            this.m_driveLetterCb.SelectedIndexChanged += new System.EventHandler(this.m_driveLetterCb_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drive:";
            // 
            // m_saveCloseButton
            // 
            this.m_saveCloseButton.Location = new System.Drawing.Point(392, 473);
            this.m_saveCloseButton.Name = "m_saveCloseButton";
            this.m_saveCloseButton.Size = new System.Drawing.Size(95, 23);
            this.m_saveCloseButton.TabIndex = 2;
            this.m_saveCloseButton.Text = "Save and Close";
            this.m_saveCloseButton.UseVisualStyleBackColor = true;
            this.m_saveCloseButton.Click += new System.EventHandler(this.m_saveCloseButton_Click);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.Location = new System.Drawing.Point(502, 473);
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
            this.m_cancelButton.TabIndex = 3;
            this.m_cancelButton.Text = "Cancel";
            this.m_cancelButton.UseVisualStyleBackColor = true;
            this.m_cancelButton.Click += new System.EventHandler(this.m_cancelButton_Click);
            // 
            // ConfigurationUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 508);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_saveCloseButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.m_mapListDgv);
            this.Name = "ConfigurationUi";
            this.Text = "Drive Mapper Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.m_mapListDgv)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView m_mapListDgv;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox m_uncPathTb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox m_driveLetterCb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_passwordTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_usernameTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button m_deleteMapButton;
        private System.Windows.Forms.Button m_newMapButton;
        private System.Windows.Forms.TextBox m_groupTb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox m_showPasswordCb;
        private System.Windows.Forms.ComboBox m_credsCb;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button m_saveCloseButton;
        private System.Windows.Forms.Button m_cancelButton;
    }
}