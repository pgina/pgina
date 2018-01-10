namespace pGina.Plugin.scripting
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
            this.tab_pages = new System.Windows.Forms.TabControl();
            this.tab_authentication = new System.Windows.Forms.TabPage();
            this.authentication_sys = new System.Windows.Forms.GroupBox();
            this.authentication_sys_grid = new System.Windows.Forms.DataGridView();
            this.pwd = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.script = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tab_authorization = new System.Windows.Forms.TabPage();
            this.authorization_sys = new System.Windows.Forms.GroupBox();
            this.authorization_sys_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tab_gateway = new System.Windows.Forms.TabPage();
            this.gateway_sys = new System.Windows.Forms.GroupBox();
            this.gateway_sys_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tab_notification = new System.Windows.Forms.TabPage();
            this.notification_usr = new System.Windows.Forms.GroupBox();
            this.notification_usr_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn8 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn8logon = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn8logoff = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.notification_sys = new System.Windows.Forms.GroupBox();
            this.notification_sys_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn4logon = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn4logoff = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tab_changepwd = new System.Windows.Forms.TabPage();
            this.changepwd_usr = new System.Windows.Forms.GroupBox();
            this.changepwd_usr_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn9 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.changepwd_sys = new System.Windows.Forms.GroupBox();
            this.changepwd_sys_grid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn5 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.close_btn = new System.Windows.Forms.Button();
            this.save_btn = new System.Windows.Forms.Button();
            this.help_btn = new System.Windows.Forms.Button();
            this.macros_label = new System.Windows.Forms.Label();
            this.tab_pages.SuspendLayout();
            this.tab_authentication.SuspendLayout();
            this.authentication_sys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authentication_sys_grid)).BeginInit();
            this.tab_authorization.SuspendLayout();
            this.authorization_sys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authorization_sys_grid)).BeginInit();
            this.tab_gateway.SuspendLayout();
            this.gateway_sys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gateway_sys_grid)).BeginInit();
            this.tab_notification.SuspendLayout();
            this.notification_usr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.notification_usr_grid)).BeginInit();
            this.notification_sys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.notification_sys_grid)).BeginInit();
            this.tab_changepwd.SuspendLayout();
            this.changepwd_usr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.changepwd_usr_grid)).BeginInit();
            this.changepwd_sys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.changepwd_sys_grid)).BeginInit();
            this.SuspendLayout();
            //
            // tab_pages
            //
            this.tab_pages.Controls.Add(this.tab_authentication);
            this.tab_pages.Controls.Add(this.tab_authorization);
            this.tab_pages.Controls.Add(this.tab_gateway);
            this.tab_pages.Controls.Add(this.tab_notification);
            this.tab_pages.Controls.Add(this.tab_changepwd);
            this.tab_pages.Location = new System.Drawing.Point(12, 12);
            this.tab_pages.Name = "tab_pages";
            this.tab_pages.SelectedIndex = 0;
            this.tab_pages.Size = new System.Drawing.Size(857, 410);
            this.tab_pages.TabIndex = 0;
            //
            // tab_authentication
            //
            this.tab_authentication.Controls.Add(this.authentication_sys);
            this.tab_authentication.Location = new System.Drawing.Point(4, 22);
            this.tab_authentication.Name = "tab_authentication";
            this.tab_authentication.Padding = new System.Windows.Forms.Padding(3);
            this.tab_authentication.Size = new System.Drawing.Size(849, 384);
            this.tab_authentication.TabIndex = 0;
            this.tab_authentication.Text = "Authentication";
            this.tab_authentication.UseVisualStyleBackColor = true;
            //
            // authentication_sys
            //
            this.authentication_sys.Controls.Add(this.authentication_sys_grid);
            this.authentication_sys.Location = new System.Drawing.Point(6, 6);
            this.authentication_sys.Name = "authentication_sys";
            this.authentication_sys.Size = new System.Drawing.Size(837, 372);
            this.authentication_sys.TabIndex = 0;
            this.authentication_sys.TabStop = false;
            this.authentication_sys.Text = "system context";
            //
            // authentication_sys_grid
            //
            this.authentication_sys_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authentication_sys_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pwd,
            this.script});
            this.authentication_sys_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authentication_sys_grid.Location = new System.Drawing.Point(3, 16);
            this.authentication_sys_grid.Name = "authentication_sys_grid";
            this.authentication_sys_grid.Size = new System.Drawing.Size(831, 353);
            this.authentication_sys_grid.TabIndex = 0;
            //
            // pwd
            //
            this.pwd.HeaderText = "submit pwd";
            this.pwd.Name = "pwd";
            this.pwd.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.pwd.ToolTipText = "submit user password";
            this.pwd.Width = 85;
            //
            // script
            //
            this.script.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.script.HeaderText = "script";
            this.script.Name = "script";
            this.script.ToolTipText = "script to run";
            //
            // tab_authorization
            //
            this.tab_authorization.Controls.Add(this.authorization_sys);
            this.tab_authorization.Location = new System.Drawing.Point(4, 22);
            this.tab_authorization.Name = "tab_authorization";
            this.tab_authorization.Padding = new System.Windows.Forms.Padding(3);
            this.tab_authorization.Size = new System.Drawing.Size(849, 384);
            this.tab_authorization.TabIndex = 1;
            this.tab_authorization.Text = "Authorization";
            this.tab_authorization.UseVisualStyleBackColor = true;
            //
            // authorization_sys
            //
            this.authorization_sys.Controls.Add(this.authorization_sys_grid);
            this.authorization_sys.Location = new System.Drawing.Point(6, 6);
            this.authorization_sys.Name = "authorization_sys";
            this.authorization_sys.Size = new System.Drawing.Size(837, 372);
            this.authorization_sys.TabIndex = 0;
            this.authorization_sys.TabStop = false;
            this.authorization_sys.Text = "system context";
            //
            // authorization_sys_grid
            //
            this.authorization_sys_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authorization_sys_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn2,
            this.dataGridViewTextBoxColumn2});
            this.authorization_sys_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorization_sys_grid.Location = new System.Drawing.Point(3, 16);
            this.authorization_sys_grid.Name = "authorization_sys_grid";
            this.authorization_sys_grid.Size = new System.Drawing.Size(831, 353);
            this.authorization_sys_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn2
            //
            this.dataGridViewCheckBoxColumn2.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn2.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn2.Width = 85;
            //
            // dataGridViewTextBoxColumn2
            //
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "script";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ToolTipText = "script to run";
            //
            // tab_gateway
            //
            this.tab_gateway.Controls.Add(this.gateway_sys);
            this.tab_gateway.Location = new System.Drawing.Point(4, 22);
            this.tab_gateway.Name = "tab_gateway";
            this.tab_gateway.Size = new System.Drawing.Size(849, 384);
            this.tab_gateway.TabIndex = 2;
            this.tab_gateway.Text = "Gateway";
            this.tab_gateway.UseVisualStyleBackColor = true;
            //
            // gateway_sys
            //
            this.gateway_sys.Controls.Add(this.gateway_sys_grid);
            this.gateway_sys.Location = new System.Drawing.Point(6, 6);
            this.gateway_sys.Name = "gateway_sys";
            this.gateway_sys.Size = new System.Drawing.Size(837, 372);
            this.gateway_sys.TabIndex = 0;
            this.gateway_sys.TabStop = false;
            this.gateway_sys.Text = "system context";
            //
            // gateway_sys_grid
            //
            this.gateway_sys_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gateway_sys_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn3,
            this.dataGridViewTextBoxColumn3});
            this.gateway_sys_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gateway_sys_grid.Location = new System.Drawing.Point(3, 16);
            this.gateway_sys_grid.Name = "gateway_sys_grid";
            this.gateway_sys_grid.Size = new System.Drawing.Size(831, 353);
            this.gateway_sys_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn3
            //
            this.dataGridViewCheckBoxColumn3.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn3.Name = "dataGridViewCheckBoxColumn3";
            this.dataGridViewCheckBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn3.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn3.Width = 85;
            //
            // dataGridViewTextBoxColumn3
            //
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.HeaderText = "script";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ToolTipText = "script to run";
            //
            // tab_notification
            //
            this.tab_notification.Controls.Add(this.notification_usr);
            this.tab_notification.Controls.Add(this.notification_sys);
            this.tab_notification.Location = new System.Drawing.Point(4, 22);
            this.tab_notification.Name = "tab_notification";
            this.tab_notification.Size = new System.Drawing.Size(849, 384);
            this.tab_notification.TabIndex = 3;
            this.tab_notification.Text = "Event Notification";
            this.tab_notification.UseVisualStyleBackColor = true;
            //
            // notification_usr
            //
            this.notification_usr.Controls.Add(this.notification_usr_grid);
            this.notification_usr.Location = new System.Drawing.Point(6, 185);
            this.notification_usr.Name = "notification_usr";
            this.notification_usr.Size = new System.Drawing.Size(834, 193);
            this.notification_usr.TabIndex = 1;
            this.notification_usr.TabStop = false;
            this.notification_usr.Text = "user context";
            //
            // notification_usr_grid
            //
            this.notification_usr_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.notification_usr_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn8,
            this.dataGridViewCheckBoxColumn8logon,
            this.dataGridViewCheckBoxColumn8logoff,
            this.dataGridViewTextBoxColumn8});
            this.notification_usr_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notification_usr_grid.Location = new System.Drawing.Point(3, 16);
            this.notification_usr_grid.Name = "notification_usr_grid";
            this.notification_usr_grid.Size = new System.Drawing.Size(828, 174);
            this.notification_usr_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn8
            //
            this.dataGridViewCheckBoxColumn8.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn8.Name = "dataGridViewCheckBoxColumn8";
            this.dataGridViewCheckBoxColumn8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn8.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn8.Width = 85;
            //
            // dataGridViewCheckBoxColumn8logon
            //
            this.dataGridViewCheckBoxColumn8logon.HeaderText = "logon";
            this.dataGridViewCheckBoxColumn8logon.Name = "dataGridViewCheckBoxColumn8logon";
            this.dataGridViewCheckBoxColumn8logon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn8logon.ToolTipText = "run on logon";
            this.dataGridViewCheckBoxColumn8logon.Width = 50;
            //
            // dataGridViewCheckBoxColumn8logoff
            //
            this.dataGridViewCheckBoxColumn8logoff.HeaderText = "logoff";
            this.dataGridViewCheckBoxColumn8logoff.Name = "dataGridViewCheckBoxColumn8logoff";
            this.dataGridViewCheckBoxColumn8logoff.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn8logoff.ToolTipText = "run on logoff";
            this.dataGridViewCheckBoxColumn8logoff.Width = 50;
            //
            // dataGridViewTextBoxColumn8
            //
            this.dataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn8.HeaderText = "script";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.ToolTipText = "script to run";
            //
            // notification_sys
            //
            this.notification_sys.Controls.Add(this.notification_sys_grid);
            this.notification_sys.Location = new System.Drawing.Point(6, 6);
            this.notification_sys.Name = "notification_sys";
            this.notification_sys.Size = new System.Drawing.Size(837, 173);
            this.notification_sys.TabIndex = 0;
            this.notification_sys.TabStop = false;
            this.notification_sys.Text = "system context";
            //
            // notification_sys_grid
            //
            this.notification_sys_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.notification_sys_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn4,
            this.dataGridViewCheckBoxColumn4logon,
            this.dataGridViewCheckBoxColumn4logoff,
            this.dataGridViewTextBoxColumn4});
            this.notification_sys_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notification_sys_grid.Location = new System.Drawing.Point(3, 16);
            this.notification_sys_grid.Name = "notification_sys_grid";
            this.notification_sys_grid.Size = new System.Drawing.Size(831, 154);
            this.notification_sys_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn4
            //
            this.dataGridViewCheckBoxColumn4.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn4.Name = "dataGridViewCheckBoxColumn4";
            this.dataGridViewCheckBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn4.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn4.Width = 85;
            //
            // dataGridViewCheckBoxColumn4logon
            //
            this.dataGridViewCheckBoxColumn4logon.HeaderText = "logon";
            this.dataGridViewCheckBoxColumn4logon.Name = "dataGridViewCheckBoxColumn4logon";
            this.dataGridViewCheckBoxColumn4logon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn4logon.ToolTipText = "run on logon";
            this.dataGridViewCheckBoxColumn4logon.Width = 50;
            //
            // dataGridViewCheckBoxColumn4logoff
            //
            this.dataGridViewCheckBoxColumn4logoff.HeaderText = "logoff";
            this.dataGridViewCheckBoxColumn4logoff.Name = "dataGridViewCheckBoxColumn4logoff";
            this.dataGridViewCheckBoxColumn4logoff.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn4logoff.ToolTipText = "run on logoff";
            this.dataGridViewCheckBoxColumn4logoff.Width = 50;
            //
            // dataGridViewTextBoxColumn4
            //
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.HeaderText = "script";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ToolTipText = "script to run";
            //
            // tab_changepwd
            //
            this.tab_changepwd.Controls.Add(this.changepwd_usr);
            this.tab_changepwd.Controls.Add(this.changepwd_sys);
            this.tab_changepwd.Location = new System.Drawing.Point(4, 22);
            this.tab_changepwd.Name = "tab_changepwd";
            this.tab_changepwd.Size = new System.Drawing.Size(849, 384);
            this.tab_changepwd.TabIndex = 4;
            this.tab_changepwd.Text = "Change Password";
            this.tab_changepwd.UseVisualStyleBackColor = true;
            //
            // changepwd_usr
            //
            this.changepwd_usr.Controls.Add(this.changepwd_usr_grid);
            this.changepwd_usr.Location = new System.Drawing.Point(6, 185);
            this.changepwd_usr.Name = "changepwd_usr";
            this.changepwd_usr.Size = new System.Drawing.Size(834, 193);
            this.changepwd_usr.TabIndex = 1;
            this.changepwd_usr.TabStop = false;
            this.changepwd_usr.Text = "user context";
            //
            // changepwd_usr_grid
            //
            this.changepwd_usr_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.changepwd_usr_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn9,
            this.dataGridViewTextBoxColumn9});
            this.changepwd_usr_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changepwd_usr_grid.Location = new System.Drawing.Point(3, 16);
            this.changepwd_usr_grid.Name = "changepwd_usr_grid";
            this.changepwd_usr_grid.Size = new System.Drawing.Size(828, 174);
            this.changepwd_usr_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn9
            //
            this.dataGridViewCheckBoxColumn9.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn9.Name = "dataGridViewCheckBoxColumn9";
            this.dataGridViewCheckBoxColumn9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn9.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn9.Width = 85;
            //
            // dataGridViewTextBoxColumn9
            //
            this.dataGridViewTextBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn9.HeaderText = "script";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ToolTipText = "script to run";
            //
            // changepwd_sys
            //
            this.changepwd_sys.Controls.Add(this.changepwd_sys_grid);
            this.changepwd_sys.Location = new System.Drawing.Point(6, 6);
            this.changepwd_sys.Name = "changepwd_sys";
            this.changepwd_sys.Size = new System.Drawing.Size(837, 173);
            this.changepwd_sys.TabIndex = 0;
            this.changepwd_sys.TabStop = false;
            this.changepwd_sys.Text = "system context";
            //
            // changepwd_sys_grid
            //
            this.changepwd_sys_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.changepwd_sys_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn5,
            this.dataGridViewTextBoxColumn5});
            this.changepwd_sys_grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changepwd_sys_grid.Location = new System.Drawing.Point(3, 16);
            this.changepwd_sys_grid.Name = "changepwd_sys_grid";
            this.changepwd_sys_grid.Size = new System.Drawing.Size(831, 154);
            this.changepwd_sys_grid.TabIndex = 0;
            //
            // dataGridViewCheckBoxColumn5
            //
            this.dataGridViewCheckBoxColumn5.HeaderText = "submit pwd";
            this.dataGridViewCheckBoxColumn5.Name = "dataGridViewCheckBoxColumn5";
            this.dataGridViewCheckBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn5.ToolTipText = "submit user password";
            this.dataGridViewCheckBoxColumn5.Width = 85;
            //
            // dataGridViewTextBoxColumn5
            //
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn5.HeaderText = "script";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ToolTipText = "script to run";
            //
            // close_btn
            //
            this.close_btn.Location = new System.Drawing.Point(705, 430);
            this.close_btn.Name = "close_btn";
            this.close_btn.Size = new System.Drawing.Size(79, 24);
            this.close_btn.TabIndex = 3;
            this.close_btn.Text = "Cancel";
            this.close_btn.UseVisualStyleBackColor = true;
            this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
            //
            // save_btn
            //
            this.save_btn.Location = new System.Drawing.Point(790, 430);
            this.save_btn.Name = "save_btn";
            this.save_btn.Size = new System.Drawing.Size(79, 24);
            this.save_btn.TabIndex = 4;
            this.save_btn.Text = "Save";
            this.save_btn.UseVisualStyleBackColor = true;
            this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
            //
            // help_btn
            //
            this.help_btn.Location = new System.Drawing.Point(620, 430);
            this.help_btn.Name = "help_btn";
            this.help_btn.Size = new System.Drawing.Size(79, 24);
            this.help_btn.TabIndex = 2;
            this.help_btn.Text = "Help";
            this.help_btn.UseVisualStyleBackColor = true;
            this.help_btn.Click += new System.EventHandler(this.help_btn_Click);
            //
            // macros_label
            //
            this.macros_label.AutoSize = true;
            this.macros_label.Location = new System.Drawing.Point(12, 428);
            this.macros_label.Name = "macros_label";
            this.macros_label.Size = new System.Drawing.Size(572, 26);
            this.macros_label.TabIndex = 1;
            this.macros_label.Text = "Macros: %u = Username, %o = OriginalUsername, %p = Password, %b = oldPassword, %s = " +
                "SID, %e = PasswordEXP,\r\n %i = SessionID, %Ae = Authentication plugins, %Ao = Authorization plugins, %Gw = Gateway plugins";
            //
            // Configuration
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 464);
            this.Controls.Add(this.macros_label);
            this.Controls.Add(this.help_btn);
            this.Controls.Add(this.save_btn);
            this.Controls.Add(this.close_btn);
            this.Controls.Add(this.tab_pages);
            this.Name = "Configuration";
            this.Text = "Configuration";
            this.tab_pages.ResumeLayout(false);
            this.tab_authentication.ResumeLayout(false);
            this.authentication_sys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.authentication_sys_grid)).EndInit();
            this.tab_authorization.ResumeLayout(false);
            this.authorization_sys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.authorization_sys_grid)).EndInit();
            this.tab_gateway.ResumeLayout(false);
            this.gateway_sys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gateway_sys_grid)).EndInit();
            this.tab_notification.ResumeLayout(false);
            this.notification_usr.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.notification_usr_grid)).EndInit();
            this.notification_sys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.notification_sys_grid)).EndInit();
            this.tab_changepwd.ResumeLayout(false);
            this.changepwd_usr.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.changepwd_usr_grid)).EndInit();
            this.changepwd_sys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.changepwd_sys_grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tab_pages;
        private System.Windows.Forms.TabPage tab_authentication;
        private System.Windows.Forms.TabPage tab_authorization;
        private System.Windows.Forms.TabPage tab_gateway;
        private System.Windows.Forms.TabPage tab_notification;
        private System.Windows.Forms.TabPage tab_changepwd;
        private System.Windows.Forms.GroupBox authentication_sys;
        private System.Windows.Forms.GroupBox authorization_sys;
        private System.Windows.Forms.GroupBox gateway_sys;
        private System.Windows.Forms.GroupBox notification_usr;
        private System.Windows.Forms.GroupBox notification_sys;
        private System.Windows.Forms.GroupBox changepwd_usr;
        private System.Windows.Forms.GroupBox changepwd_sys;
        private System.Windows.Forms.DataGridView authentication_sys_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn pwd;
        private System.Windows.Forms.DataGridViewTextBoxColumn script;
        private System.Windows.Forms.DataGridView authorization_sys_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridView gateway_sys_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridView notification_usr_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn8;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn8logon;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn8logoff;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4logon;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4logoff;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridView changepwd_usr_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridView changepwd_sys_grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.Button close_btn;
        private System.Windows.Forms.Button save_btn;
        private System.Windows.Forms.Button help_btn;
        private System.Windows.Forms.DataGridView notification_sys_grid;
        private System.Windows.Forms.Label macros_label;
    }
}
