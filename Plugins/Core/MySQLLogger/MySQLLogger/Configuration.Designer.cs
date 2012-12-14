namespace pGina.Plugin.MySqlLogger
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
            this.hostTB = new System.Windows.Forms.TextBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTB = new System.Windows.Forms.TextBox();
            this.dbLabel = new System.Windows.Forms.Label();
            this.dbTB = new System.Windows.Forms.TextBox();
            this.userLabel = new System.Windows.Forms.Label();
            this.userTB = new System.Windows.Forms.TextBox();
            this.passwdLabel = new System.Windows.Forms.Label();
            this.passwdTB = new System.Windows.Forms.TextBox();
            this.testButton = new System.Windows.Forms.Button();
            this.createTableBtn = new System.Windows.Forms.Button();
            this.showPassCB = new System.Windows.Forms.CheckBox();
            this.eventsBox = new System.Windows.Forms.GroupBox();
            this.remoteDisconnectEvtCB = new System.Windows.Forms.CheckBox();
            this.remoteConnectEvtCB = new System.Windows.Forms.CheckBox();
            this.remoteControlEvtCB = new System.Windows.Forms.CheckBox();
            this.consoleDisconnectEvtCB = new System.Windows.Forms.CheckBox();
            this.consoleConnectEvtCB = new System.Windows.Forms.CheckBox();
            this.unlockEvtCB = new System.Windows.Forms.CheckBox();
            this.lockEvtCB = new System.Windows.Forms.CheckBox();
            this.logoffEvtCB = new System.Windows.Forms.CheckBox();
            this.logonEvtCB = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.eventTableTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.optionsBox = new System.Windows.Forms.GroupBox();
            this.useModNameCB = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.eventModeCB = new System.Windows.Forms.CheckBox();
            this.sessionModeCB = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.sessionTableTB = new System.Windows.Forms.TextBox();
            this.eventsBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.optionsBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host:";
            // 
            // hostTB
            // 
            this.hostTB.Location = new System.Drawing.Point(92, 19);
            this.hostTB.Name = "hostTB";
            this.hostTB.Size = new System.Drawing.Size(307, 20);
            this.hostTB.TabIndex = 0;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(430, 394);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(70, 28);
            this.okBtn.TabIndex = 7;
            this.okBtn.Text = "Save";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(354, 394);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(70, 28);
            this.cancelBtn.TabIndex = 6;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(17, 48);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 11;
            this.portLabel.Text = "Port:";
            // 
            // portTB
            // 
            this.portTB.Location = new System.Drawing.Point(92, 45);
            this.portTB.Name = "portTB";
            this.portTB.Size = new System.Drawing.Size(97, 20);
            this.portTB.TabIndex = 1;
            // 
            // dbLabel
            // 
            this.dbLabel.AutoSize = true;
            this.dbLabel.Location = new System.Drawing.Point(17, 74);
            this.dbLabel.Name = "dbLabel";
            this.dbLabel.Size = new System.Drawing.Size(56, 13);
            this.dbLabel.TabIndex = 12;
            this.dbLabel.Text = "Database:";
            // 
            // dbTB
            // 
            this.dbTB.Location = new System.Drawing.Point(92, 71);
            this.dbTB.Name = "dbTB";
            this.dbTB.Size = new System.Drawing.Size(307, 20);
            this.dbTB.TabIndex = 2;
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(17, 158);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(32, 13);
            this.userLabel.TabIndex = 13;
            this.userLabel.Text = "User:";
            // 
            // userTB
            // 
            this.userTB.Location = new System.Drawing.Point(92, 155);
            this.userTB.Name = "userTB";
            this.userTB.Size = new System.Drawing.Size(307, 20);
            this.userTB.TabIndex = 5;
            // 
            // passwdLabel
            // 
            this.passwdLabel.AutoSize = true;
            this.passwdLabel.Location = new System.Drawing.Point(17, 184);
            this.passwdLabel.Name = "passwdLabel";
            this.passwdLabel.Size = new System.Drawing.Size(56, 13);
            this.passwdLabel.TabIndex = 14;
            this.passwdLabel.Text = "Password:";
            // 
            // passwdTB
            // 
            this.passwdTB.Location = new System.Drawing.Point(92, 181);
            this.passwdTB.Name = "passwdTB";
            this.passwdTB.Size = new System.Drawing.Size(307, 20);
            this.passwdTB.TabIndex = 6;
            this.passwdTB.UseSystemPasswordChar = true;
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(6, 396);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(67, 27);
            this.testButton.TabIndex = 4;
            this.testButton.Text = "Test...";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // createTableBtn
            // 
            this.createTableBtn.Location = new System.Drawing.Point(79, 396);
            this.createTableBtn.Name = "createTableBtn";
            this.createTableBtn.Size = new System.Drawing.Size(91, 26);
            this.createTableBtn.TabIndex = 5;
            this.createTableBtn.Text = "Create Table...";
            this.createTableBtn.UseVisualStyleBackColor = true;
            this.createTableBtn.Click += new System.EventHandler(this.createTableBtn_Click);
            // 
            // showPassCB
            // 
            this.showPassCB.AutoSize = true;
            this.showPassCB.Location = new System.Drawing.Point(411, 184);
            this.showPassCB.Name = "showPassCB";
            this.showPassCB.Size = new System.Drawing.Size(77, 17);
            this.showPassCB.TabIndex = 7;
            this.showPassCB.Text = "Show Text";
            this.showPassCB.UseVisualStyleBackColor = true;
            this.showPassCB.CheckedChanged += new System.EventHandler(this.showPassCB_CheckedChanged);
            // 
            // eventsBox
            // 
            this.eventsBox.Controls.Add(this.remoteDisconnectEvtCB);
            this.eventsBox.Controls.Add(this.remoteConnectEvtCB);
            this.eventsBox.Controls.Add(this.remoteControlEvtCB);
            this.eventsBox.Controls.Add(this.consoleDisconnectEvtCB);
            this.eventsBox.Controls.Add(this.consoleConnectEvtCB);
            this.eventsBox.Controls.Add(this.unlockEvtCB);
            this.eventsBox.Controls.Add(this.lockEvtCB);
            this.eventsBox.Controls.Add(this.logoffEvtCB);
            this.eventsBox.Controls.Add(this.logonEvtCB);
            this.eventsBox.Location = new System.Drawing.Point(6, 274);
            this.eventsBox.Name = "eventsBox";
            this.eventsBox.Size = new System.Drawing.Size(494, 66);
            this.eventsBox.TabIndex = 2;
            this.eventsBox.TabStop = false;
            this.eventsBox.Text = "Events";
            // 
            // remoteDisconnectEvtCB
            // 
            this.remoteDisconnectEvtCB.AutoSize = true;
            this.remoteDisconnectEvtCB.Location = new System.Drawing.Point(261, 42);
            this.remoteDisconnectEvtCB.Name = "remoteDisconnectEvtCB";
            this.remoteDisconnectEvtCB.Size = new System.Drawing.Size(120, 17);
            this.remoteDisconnectEvtCB.TabIndex = 7;
            this.remoteDisconnectEvtCB.Text = "Remote Disconnect";
            this.remoteDisconnectEvtCB.UseVisualStyleBackColor = true;
            // 
            // remoteConnectEvtCB
            // 
            this.remoteConnectEvtCB.AutoSize = true;
            this.remoteConnectEvtCB.Location = new System.Drawing.Point(261, 19);
            this.remoteConnectEvtCB.Name = "remoteConnectEvtCB";
            this.remoteConnectEvtCB.Size = new System.Drawing.Size(106, 17);
            this.remoteConnectEvtCB.TabIndex = 6;
            this.remoteConnectEvtCB.Text = "Remote Connect";
            this.remoteConnectEvtCB.UseVisualStyleBackColor = true;
            // 
            // remoteControlEvtCB
            // 
            this.remoteControlEvtCB.AutoSize = true;
            this.remoteControlEvtCB.Location = new System.Drawing.Point(383, 19);
            this.remoteControlEvtCB.Name = "remoteControlEvtCB";
            this.remoteControlEvtCB.Size = new System.Drawing.Size(99, 17);
            this.remoteControlEvtCB.TabIndex = 8;
            this.remoteControlEvtCB.Text = "Remote Control";
            this.remoteControlEvtCB.UseVisualStyleBackColor = true;
            // 
            // consoleDisconnectEvtCB
            // 
            this.consoleDisconnectEvtCB.AutoSize = true;
            this.consoleDisconnectEvtCB.Location = new System.Drawing.Point(134, 42);
            this.consoleDisconnectEvtCB.Name = "consoleDisconnectEvtCB";
            this.consoleDisconnectEvtCB.Size = new System.Drawing.Size(121, 17);
            this.consoleDisconnectEvtCB.TabIndex = 5;
            this.consoleDisconnectEvtCB.Text = "Console Disconnect";
            this.consoleDisconnectEvtCB.UseVisualStyleBackColor = true;
            // 
            // consoleConnectEvtCB
            // 
            this.consoleConnectEvtCB.AutoSize = true;
            this.consoleConnectEvtCB.Location = new System.Drawing.Point(134, 19);
            this.consoleConnectEvtCB.Name = "consoleConnectEvtCB";
            this.consoleConnectEvtCB.Size = new System.Drawing.Size(107, 17);
            this.consoleConnectEvtCB.TabIndex = 4;
            this.consoleConnectEvtCB.Text = "Console Connect";
            this.consoleConnectEvtCB.UseVisualStyleBackColor = true;
            // 
            // unlockEvtCB
            // 
            this.unlockEvtCB.AutoSize = true;
            this.unlockEvtCB.Location = new System.Drawing.Point(68, 42);
            this.unlockEvtCB.Name = "unlockEvtCB";
            this.unlockEvtCB.Size = new System.Drawing.Size(60, 17);
            this.unlockEvtCB.TabIndex = 3;
            this.unlockEvtCB.Text = "Unlock";
            this.unlockEvtCB.UseVisualStyleBackColor = true;
            // 
            // lockEvtCB
            // 
            this.lockEvtCB.AutoSize = true;
            this.lockEvtCB.Location = new System.Drawing.Point(68, 19);
            this.lockEvtCB.Name = "lockEvtCB";
            this.lockEvtCB.Size = new System.Drawing.Size(50, 17);
            this.lockEvtCB.TabIndex = 2;
            this.lockEvtCB.Text = "Lock";
            this.lockEvtCB.UseVisualStyleBackColor = true;
            // 
            // logoffEvtCB
            // 
            this.logoffEvtCB.AutoSize = true;
            this.logoffEvtCB.Location = new System.Drawing.Point(6, 42);
            this.logoffEvtCB.Name = "logoffEvtCB";
            this.logoffEvtCB.Size = new System.Drawing.Size(56, 17);
            this.logoffEvtCB.TabIndex = 1;
            this.logoffEvtCB.Text = "Logoff";
            this.logoffEvtCB.UseVisualStyleBackColor = true;
            // 
            // logonEvtCB
            // 
            this.logonEvtCB.AutoSize = true;
            this.logonEvtCB.Location = new System.Drawing.Point(6, 19);
            this.logonEvtCB.Name = "logonEvtCB";
            this.logonEvtCB.Size = new System.Drawing.Size(56, 17);
            this.logonEvtCB.TabIndex = 0;
            this.logonEvtCB.Text = "Logon";
            this.logonEvtCB.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.sessionTableTB);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.eventTableTB);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.showPassCB);
            this.groupBox2.Controls.Add(this.passwdTB);
            this.groupBox2.Controls.Add(this.passwdLabel);
            this.groupBox2.Controls.Add(this.userTB);
            this.groupBox2.Controls.Add(this.userLabel);
            this.groupBox2.Controls.Add(this.dbTB);
            this.groupBox2.Controls.Add(this.dbLabel);
            this.groupBox2.Controls.Add(this.portTB);
            this.groupBox2.Controls.Add(this.portLabel);
            this.groupBox2.Controls.Add(this.hostTB);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 53);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(488, 213);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server";
            // 
            // eventTableTB
            // 
            this.eventTableTB.Location = new System.Drawing.Point(91, 96);
            this.eventTableTB.Name = "eventTableTB";
            this.eventTableTB.Size = new System.Drawing.Size(308, 20);
            this.eventTableTB.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Event Table:";
            // 
            // optionsBox
            // 
            this.optionsBox.Controls.Add(this.useModNameCB);
            this.optionsBox.Location = new System.Drawing.Point(6, 347);
            this.optionsBox.Name = "optionsBox";
            this.optionsBox.Size = new System.Drawing.Size(494, 41);
            this.optionsBox.TabIndex = 3;
            this.optionsBox.TabStop = false;
            this.optionsBox.Text = "Options";
            // 
            // useModNameCB
            // 
            this.useModNameCB.AutoSize = true;
            this.useModNameCB.Location = new System.Drawing.Point(7, 18);
            this.useModNameCB.Name = "useModNameCB";
            this.useModNameCB.Size = new System.Drawing.Size(139, 17);
            this.useModNameCB.TabIndex = 0;
            this.useModNameCB.Text = "Use Modified Username";
            this.useModNameCB.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.sessionModeCB);
            this.groupBox3.Controls.Add(this.eventModeCB);
            this.groupBox3.Location = new System.Drawing.Point(13, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(487, 32);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mode";
            // 
            // eventModeCB
            // 
            this.eventModeCB.AutoSize = true;
            this.eventModeCB.Location = new System.Drawing.Point(73, 11);
            this.eventModeCB.Name = "eventModeCB";
            this.eventModeCB.Size = new System.Drawing.Size(84, 17);
            this.eventModeCB.TabIndex = 0;
            this.eventModeCB.Text = "Event Mode";
            this.eventModeCB.UseVisualStyleBackColor = true;
            this.eventModeCB.CheckStateChanged += new System.EventHandler(this.ModeChange);
            // 
            // sessionModeCB
            // 
            this.sessionModeCB.AutoSize = true;
            this.sessionModeCB.Location = new System.Drawing.Point(318, 11);
            this.sessionModeCB.Name = "sessionModeCB";
            this.sessionModeCB.Size = new System.Drawing.Size(93, 17);
            this.sessionModeCB.TabIndex = 1;
            this.sessionModeCB.Text = "Session Mode";
            this.sessionModeCB.UseVisualStyleBackColor = true;
            this.sessionModeCB.CheckStateChanged += new System.EventHandler(this.ModeChange);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Session Table:";
            // 
            // sessionTableTB
            // 
            this.sessionTableTB.Location = new System.Drawing.Point(92, 123);
            this.sessionTableTB.Name = "sessionTableTB";
            this.sessionTableTB.Size = new System.Drawing.Size(307, 20);
            this.sessionTableTB.TabIndex = 4;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 437);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.optionsBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.eventsBox);
            this.Controls.Add(this.createTableBtn);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Configuration";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "MySQL Logger Plugin Configuration";
            this.eventsBox.ResumeLayout(false);
            this.eventsBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.optionsBox.ResumeLayout(false);
            this.optionsBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hostTB;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTB;
        private System.Windows.Forms.Label dbLabel;
        private System.Windows.Forms.TextBox dbTB;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.TextBox userTB;
        private System.Windows.Forms.Label passwdLabel;
        private System.Windows.Forms.TextBox passwdTB;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button createTableBtn;
        private System.Windows.Forms.CheckBox showPassCB;
        private System.Windows.Forms.GroupBox eventsBox;
        private System.Windows.Forms.CheckBox logoffEvtCB;
        private System.Windows.Forms.CheckBox logonEvtCB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox unlockEvtCB;
        private System.Windows.Forms.CheckBox lockEvtCB;
        private System.Windows.Forms.CheckBox consoleDisconnectEvtCB;
        private System.Windows.Forms.CheckBox consoleConnectEvtCB;
        private System.Windows.Forms.CheckBox remoteControlEvtCB;
        private System.Windows.Forms.CheckBox remoteDisconnectEvtCB;
        private System.Windows.Forms.CheckBox remoteConnectEvtCB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox eventTableTB;
        private System.Windows.Forms.GroupBox optionsBox;
        private System.Windows.Forms.CheckBox useModNameCB;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox sessionModeCB;
        private System.Windows.Forms.CheckBox eventModeCB;
        private System.Windows.Forms.TextBox sessionTableTB;
        private System.Windows.Forms.Label label3;
    }
}