namespace pGina.Plugin.MySQLAuth
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.hostLabel = new System.Windows.Forms.Label();
            this.hostTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.portTB = new System.Windows.Forms.TextBox();
            this.userTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.passwordTB = new System.Windows.Forms.TextBox();
            this.passwdCB = new System.Windows.Forms.CheckBox();
            this.useSslCB = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dbTB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControlDBSchema = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.encBase64RB = new System.Windows.Forms.RadioButton();
            this.encHexRB = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.userPrimaryKeyColTB = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.passwdColTB = new System.Windows.Forms.TextBox();
            this.hashMethodColTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.unameColTB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.userTableTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupTablePrimaryKeyColTB = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupNameColTB = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupTableNameTB = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.userGroupGroupFKColTB = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.userGroupUserFKColTB = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.userGroupTableNameTB = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.createTableBtn = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabAuthz = new System.Windows.Forms.TabPage();
            this.label20 = new System.Windows.Forms.Label();
            this.btnAuthzGroupRuleDown = new System.Windows.Forms.Button();
            this.btnAuthzGroupRuleDelete = new System.Windows.Forms.Button();
            this.btnAuthzGroupRuleUp = new System.Windows.Forms.Button();
            this.btnAuthzGroupRuleAdd = new System.Windows.Forms.Button();
            this.cbAuthzGroupRuleAllowOrDeny = new System.Windows.Forms.ComboBox();
            this.tbAuthzRuleGroup = new System.Windows.Forms.TextBox();
            this.cbAuthzMySqlGroupMemberOrNot = new System.Windows.Forms.ComboBox();
            this.ckDenyWhenMySqlAuthFails = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.rbDefaultDeny = new System.Windows.Forms.RadioButton();
            this.rbDefaultAllow = new System.Windows.Forms.RadioButton();
            this.listBoxAuthzRules = new System.Windows.Forms.ListBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.gtwRuleDeleteBtn = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.gtwRulesListBox = new System.Windows.Forms.ListBox();
            this.gtwRuleAddBtn = new System.Windows.Forms.Button();
            this.gtwRuleLocalGroupTB = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.gtwRuleMysqlGroupTB = new System.Windows.Forms.TextBox();
            this.gtwRuleConditionCB = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.tabControlDBSchema.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabAuthz.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(481, 453);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 25);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Close";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(391, 453);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(82, 25);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // hostLabel
            // 
            this.hostLabel.AutoSize = true;
            this.hostLabel.Location = new System.Drawing.Point(34, 23);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(32, 13);
            this.hostLabel.TabIndex = 2;
            this.hostLabel.Text = "Host:";
            // 
            // hostTB
            // 
            this.hostTB.Location = new System.Drawing.Point(72, 20);
            this.hostTB.Name = "hostTB";
            this.hostTB.Size = new System.Drawing.Size(264, 20);
            this.hostTB.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "User:";
            // 
            // portTB
            // 
            this.portTB.Location = new System.Drawing.Point(72, 46);
            this.portTB.Name = "portTB";
            this.portTB.Size = new System.Drawing.Size(68, 20);
            this.portTB.TabIndex = 6;
            // 
            // userTB
            // 
            this.userTB.Location = new System.Drawing.Point(72, 72);
            this.userTB.Name = "userTB";
            this.userTB.Size = new System.Drawing.Size(263, 20);
            this.userTB.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Password:";
            // 
            // passwordTB
            // 
            this.passwordTB.Location = new System.Drawing.Point(72, 98);
            this.passwordTB.Name = "passwordTB";
            this.passwordTB.Size = new System.Drawing.Size(263, 20);
            this.passwordTB.TabIndex = 9;
            this.passwordTB.UseSystemPasswordChar = true;
            // 
            // passwdCB
            // 
            this.passwdCB.AutoSize = true;
            this.passwdCB.Location = new System.Drawing.Point(340, 100);
            this.passwdCB.Name = "passwdCB";
            this.passwdCB.Size = new System.Drawing.Size(77, 17);
            this.passwdCB.TabIndex = 10;
            this.passwdCB.Text = "Show Text";
            this.passwdCB.UseVisualStyleBackColor = true;
            this.passwdCB.CheckedChanged += new System.EventHandler(this.passwdCB_CheckedChanged);
            // 
            // useSslCB
            // 
            this.useSslCB.AutoSize = true;
            this.useSslCB.Location = new System.Drawing.Point(174, 48);
            this.useSslCB.Name = "useSslCB";
            this.useSslCB.Size = new System.Drawing.Size(68, 17);
            this.useSslCB.TabIndex = 11;
            this.useSslCB.Text = "Use SSL";
            this.useSslCB.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.useSslCB);
            this.groupBox1.Controls.Add(this.passwdCB);
            this.groupBox1.Controls.Add(this.passwordTB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.userTB);
            this.groupBox1.Controls.Add(this.dbTB);
            this.groupBox1.Controls.Add(this.portTB);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.hostTB);
            this.groupBox1.Controls.Add(this.hostLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 152);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MySQL Server";
            // 
            // dbTB
            // 
            this.dbTB.Location = new System.Drawing.Point(110, 124);
            this.dbTB.Name = "dbTB";
            this.dbTB.Size = new System.Drawing.Size(299, 20);
            this.dbTB.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "MySQL Database:";
            // 
            // tabControlDBSchema
            // 
            this.tabControlDBSchema.Controls.Add(this.tabPage1);
            this.tabControlDBSchema.Controls.Add(this.tabPage2);
            this.tabControlDBSchema.Controls.Add(this.tabPage3);
            this.tabControlDBSchema.Location = new System.Drawing.Point(6, 6);
            this.tabControlDBSchema.Name = "tabControlDBSchema";
            this.tabControlDBSchema.SelectedIndex = 0;
            this.tabControlDBSchema.Size = new System.Drawing.Size(530, 228);
            this.tabControlDBSchema.TabIndex = 16;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.encBase64RB);
            this.tabPage1.Controls.Add(this.encHexRB);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.userTableTB);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(522, 202);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "User Table";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // encBase64RB
            // 
            this.encBase64RB.AutoSize = true;
            this.encBase64RB.Location = new System.Drawing.Point(206, 176);
            this.encBase64RB.Name = "encBase64RB";
            this.encBase64RB.Size = new System.Drawing.Size(64, 17);
            this.encBase64RB.TabIndex = 6;
            this.encBase64RB.TabStop = true;
            this.encBase64RB.Text = "Base 64";
            this.encBase64RB.UseVisualStyleBackColor = true;
            // 
            // encHexRB
            // 
            this.encHexRB.AutoSize = true;
            this.encHexRB.Location = new System.Drawing.Point(114, 176);
            this.encHexRB.Name = "encHexRB";
            this.encHexRB.Size = new System.Drawing.Size(86, 17);
            this.encHexRB.TabIndex = 5;
            this.encHexRB.TabStop = true;
            this.encHexRB.Text = "Hexadecimal";
            this.encHexRB.UseVisualStyleBackColor = true;
            this.encHexRB.CheckedChanged += new System.EventHandler(this.encHexRB_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.userPrimaryKeyColTB);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.passwdColTB);
            this.groupBox3.Controls.Add(this.hashMethodColTB);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.unameColTB);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(3, 40);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(513, 132);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Column Names";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // userPrimaryKeyColTB
            // 
            this.userPrimaryKeyColTB.Location = new System.Drawing.Point(89, 98);
            this.userPrimaryKeyColTB.Name = "userPrimaryKeyColTB";
            this.userPrimaryKeyColTB.Size = new System.Drawing.Size(412, 20);
            this.userPrimaryKeyColTB.TabIndex = 14;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(18, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Primary Key:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(27, 75);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Password:";
            // 
            // passwdColTB
            // 
            this.passwdColTB.Location = new System.Drawing.Point(89, 72);
            this.passwdColTB.Name = "passwdColTB";
            this.passwdColTB.Size = new System.Drawing.Size(412, 20);
            this.passwdColTB.TabIndex = 11;
            // 
            // hashMethodColTB
            // 
            this.hashMethodColTB.Location = new System.Drawing.Point(88, 46);
            this.hashMethodColTB.Name = "hashMethodColTB";
            this.hashMethodColTB.Size = new System.Drawing.Size(413, 20);
            this.hashMethodColTB.TabIndex = 10;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Hash Method:";
            // 
            // unameColTB
            // 
            this.unameColTB.Location = new System.Drawing.Point(88, 20);
            this.unameColTB.Name = "unameColTB";
            this.unameColTB.Size = new System.Drawing.Size(413, 20);
            this.unameColTB.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Username:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 178);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Password encoding:";
            // 
            // userTableTB
            // 
            this.userTableTB.Location = new System.Drawing.Point(91, 14);
            this.userTableTB.Name = "userTableTB";
            this.userTableTB.Size = new System.Drawing.Size(413, 20);
            this.userTableTB.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Table Name:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.groupTableNameTB);
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(522, 202);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Group Table";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.groupTablePrimaryKeyColTB);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.groupNameColTB);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Location = new System.Drawing.Point(3, 34);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(513, 81);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Column Names";
            // 
            // groupTablePrimaryKeyColTB
            // 
            this.groupTablePrimaryKeyColTB.Location = new System.Drawing.Point(83, 50);
            this.groupTablePrimaryKeyColTB.Name = "groupTablePrimaryKeyColTB";
            this.groupTablePrimaryKeyColTB.Size = new System.Drawing.Size(414, 20);
            this.groupTablePrimaryKeyColTB.TabIndex = 3;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 53);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "Primary Key:";
            // 
            // groupNameColTB
            // 
            this.groupNameColTB.Location = new System.Drawing.Point(83, 21);
            this.groupNameColTB.Name = "groupNameColTB";
            this.groupNameColTB.Size = new System.Drawing.Size(414, 20);
            this.groupNameColTB.TabIndex = 1;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 24);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(70, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Group Name:";
            // 
            // groupTableNameTB
            // 
            this.groupTableNameTB.Location = new System.Drawing.Point(86, 8);
            this.groupTableNameTB.Name = "groupTableNameTB";
            this.groupTableNameTB.Size = new System.Drawing.Size(415, 20);
            this.groupTableNameTB.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 11);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(68, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Table Name:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Controls.Add(this.userGroupTableNameTB);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(522, 202);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "User-Group Table";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.userGroupGroupFKColTB);
            this.groupBox5.Controls.Add(this.label16);
            this.groupBox5.Controls.Add(this.userGroupUserFKColTB);
            this.groupBox5.Controls.Add(this.label15);
            this.groupBox5.Location = new System.Drawing.Point(3, 37);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(516, 76);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Column Names";
            // 
            // userGroupGroupFKColTB
            // 
            this.userGroupGroupFKColTB.Location = new System.Drawing.Point(110, 42);
            this.userGroupGroupFKColTB.Name = "userGroupGroupFKColTB";
            this.userGroupGroupFKColTB.Size = new System.Drawing.Size(395, 20);
            this.userGroupGroupFKColTB.TabIndex = 3;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 45);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(98, 13);
            this.label16.TabIndex = 2;
            this.label16.Text = "Group Foreign Key:";
            // 
            // userGroupUserFKColTB
            // 
            this.userGroupUserFKColTB.Location = new System.Drawing.Point(110, 16);
            this.userGroupUserFKColTB.Name = "userGroupUserFKColTB";
            this.userGroupUserFKColTB.Size = new System.Drawing.Size(395, 20);
            this.userGroupUserFKColTB.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(13, 19);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "User Foreign Key:";
            // 
            // userGroupTableNameTB
            // 
            this.userGroupTableNameTB.Location = new System.Drawing.Point(113, 11);
            this.userGroupTableNameTB.Name = "userGroupTableNameTB";
            this.userGroupTableNameTB.Size = new System.Drawing.Size(395, 20);
            this.userGroupTableNameTB.TabIndex = 1;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(32, 14);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Table Name:";
            // 
            // createTableBtn
            // 
            this.createTableBtn.Location = new System.Drawing.Point(90, 450);
            this.createTableBtn.Name = "createTableBtn";
            this.createTableBtn.Size = new System.Drawing.Size(100, 24);
            this.createTableBtn.TabIndex = 14;
            this.createTableBtn.Text = "Create Tables...";
            this.createTableBtn.UseVisualStyleBackColor = true;
            this.createTableBtn.Click += new System.EventHandler(this.createTableBtn_Click);
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(8, 450);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(76, 24);
            this.testBtn.TabIndex = 15;
            this.testBtn.Text = "Test...";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabAuthz);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(12, 170);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(550, 273);
            this.tabControl1.TabIndex = 16;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.tabControlDBSchema);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(542, 247);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "Database Schema";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabAuthz
            // 
            this.tabAuthz.Controls.Add(this.label20);
            this.tabAuthz.Controls.Add(this.btnAuthzGroupRuleDown);
            this.tabAuthz.Controls.Add(this.btnAuthzGroupRuleDelete);
            this.tabAuthz.Controls.Add(this.btnAuthzGroupRuleUp);
            this.tabAuthz.Controls.Add(this.btnAuthzGroupRuleAdd);
            this.tabAuthz.Controls.Add(this.cbAuthzGroupRuleAllowOrDeny);
            this.tabAuthz.Controls.Add(this.tbAuthzRuleGroup);
            this.tabAuthz.Controls.Add(this.cbAuthzMySqlGroupMemberOrNot);
            this.tabAuthz.Controls.Add(this.ckDenyWhenMySqlAuthFails);
            this.tabAuthz.Controls.Add(this.label19);
            this.tabAuthz.Controls.Add(this.rbDefaultDeny);
            this.tabAuthz.Controls.Add(this.rbDefaultAllow);
            this.tabAuthz.Controls.Add(this.listBoxAuthzRules);
            this.tabAuthz.Location = new System.Drawing.Point(4, 22);
            this.tabAuthz.Name = "tabAuthz";
            this.tabAuthz.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuthz.Size = new System.Drawing.Size(542, 247);
            this.tabAuthz.TabIndex = 2;
            this.tabAuthz.Text = "Authorization";
            this.tabAuthz.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(6, 35);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(96, 13);
            this.label20.TabIndex = 12;
            this.label20.Text = "Authorization rules:";
            // 
            // btnAuthzGroupRuleDown
            // 
            this.btnAuthzGroupRuleDown.Image = global::pGina.Plugin.MySQLAuth.Properties.Resources.DownArrowSolid;
            this.btnAuthzGroupRuleDown.Location = new System.Drawing.Point(508, 169);
            this.btnAuthzGroupRuleDown.Name = "btnAuthzGroupRuleDown";
            this.btnAuthzGroupRuleDown.Size = new System.Drawing.Size(29, 30);
            this.btnAuthzGroupRuleDown.TabIndex = 11;
            this.btnAuthzGroupRuleDown.UseVisualStyleBackColor = true;
            this.btnAuthzGroupRuleDown.Click += new System.EventHandler(this.btnAuthzGroupRuleDown_Click);
            // 
            // btnAuthzGroupRuleDelete
            // 
            this.btnAuthzGroupRuleDelete.Image = global::pGina.Plugin.MySQLAuth.Properties.Resources.delete;
            this.btnAuthzGroupRuleDelete.Location = new System.Drawing.Point(508, 119);
            this.btnAuthzGroupRuleDelete.Name = "btnAuthzGroupRuleDelete";
            this.btnAuthzGroupRuleDelete.Size = new System.Drawing.Size(29, 30);
            this.btnAuthzGroupRuleDelete.TabIndex = 10;
            this.btnAuthzGroupRuleDelete.UseVisualStyleBackColor = true;
            this.btnAuthzGroupRuleDelete.Click += new System.EventHandler(this.btnAuthzGroupRuleDelete_Click);
            // 
            // btnAuthzGroupRuleUp
            // 
            this.btnAuthzGroupRuleUp.Image = global::pGina.Plugin.MySQLAuth.Properties.Resources.UpArrowSolid;
            this.btnAuthzGroupRuleUp.Location = new System.Drawing.Point(507, 69);
            this.btnAuthzGroupRuleUp.Name = "btnAuthzGroupRuleUp";
            this.btnAuthzGroupRuleUp.Size = new System.Drawing.Size(29, 30);
            this.btnAuthzGroupRuleUp.TabIndex = 9;
            this.btnAuthzGroupRuleUp.UseVisualStyleBackColor = true;
            this.btnAuthzGroupRuleUp.Click += new System.EventHandler(this.btnAuthzGroupRuleUp_Click);
            // 
            // btnAuthzGroupRuleAdd
            // 
            this.btnAuthzGroupRuleAdd.Location = new System.Drawing.Point(447, 218);
            this.btnAuthzGroupRuleAdd.Name = "btnAuthzGroupRuleAdd";
            this.btnAuthzGroupRuleAdd.Size = new System.Drawing.Size(54, 23);
            this.btnAuthzGroupRuleAdd.TabIndex = 8;
            this.btnAuthzGroupRuleAdd.Text = "Add";
            this.btnAuthzGroupRuleAdd.UseVisualStyleBackColor = true;
            this.btnAuthzGroupRuleAdd.Click += new System.EventHandler(this.btnAuthzGroupRuleAdd_Click);
            // 
            // cbAuthzGroupRuleAllowOrDeny
            // 
            this.cbAuthzGroupRuleAllowOrDeny.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAuthzGroupRuleAllowOrDeny.FormattingEnabled = true;
            this.cbAuthzGroupRuleAllowOrDeny.Items.AddRange(new object[] {
            "allow.",
            "deny."});
            this.cbAuthzGroupRuleAllowOrDeny.Location = new System.Drawing.Point(374, 220);
            this.cbAuthzGroupRuleAllowOrDeny.Name = "cbAuthzGroupRuleAllowOrDeny";
            this.cbAuthzGroupRuleAllowOrDeny.Size = new System.Drawing.Size(62, 21);
            this.cbAuthzGroupRuleAllowOrDeny.TabIndex = 7;
            // 
            // tbAuthzRuleGroup
            // 
            this.tbAuthzRuleGroup.Location = new System.Drawing.Point(217, 220);
            this.tbAuthzRuleGroup.Name = "tbAuthzRuleGroup";
            this.tbAuthzRuleGroup.Size = new System.Drawing.Size(149, 20);
            this.tbAuthzRuleGroup.TabIndex = 6;
            // 
            // cbAuthzMySqlGroupMemberOrNot
            // 
            this.cbAuthzMySqlGroupMemberOrNot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAuthzMySqlGroupMemberOrNot.FormattingEnabled = true;
            this.cbAuthzMySqlGroupMemberOrNot.Items.AddRange(new object[] {
            "If member of MySQL group:",
            "If not member of MySQL group:"});
            this.cbAuthzMySqlGroupMemberOrNot.Location = new System.Drawing.Point(9, 220);
            this.cbAuthzMySqlGroupMemberOrNot.Name = "cbAuthzMySqlGroupMemberOrNot";
            this.cbAuthzMySqlGroupMemberOrNot.Size = new System.Drawing.Size(200, 21);
            this.cbAuthzMySqlGroupMemberOrNot.TabIndex = 5;
            // 
            // ckDenyWhenMySqlAuthFails
            // 
            this.ckDenyWhenMySqlAuthFails.AutoSize = true;
            this.ckDenyWhenMySqlAuthFails.Location = new System.Drawing.Point(180, 7);
            this.ckDenyWhenMySqlAuthFails.Name = "ckDenyWhenMySqlAuthFails";
            this.ckDenyWhenMySqlAuthFails.Size = new System.Drawing.Size(212, 17);
            this.ckDenyWhenMySqlAuthFails.TabIndex = 4;
            this.ckDenyWhenMySqlAuthFails.Text = "Deny when MySQL authentication fails.";
            this.ckDenyWhenMySqlAuthFails.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 8);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(44, 13);
            this.label19.TabIndex = 3;
            this.label19.Text = "Default:";
            // 
            // rbDefaultDeny
            // 
            this.rbDefaultDeny.AutoSize = true;
            this.rbDefaultDeny.Location = new System.Drawing.Point(112, 6);
            this.rbDefaultDeny.Name = "rbDefaultDeny";
            this.rbDefaultDeny.Size = new System.Drawing.Size(50, 17);
            this.rbDefaultDeny.TabIndex = 2;
            this.rbDefaultDeny.TabStop = true;
            this.rbDefaultDeny.Text = "Deny";
            this.rbDefaultDeny.UseVisualStyleBackColor = true;
            // 
            // rbDefaultAllow
            // 
            this.rbDefaultAllow.AutoSize = true;
            this.rbDefaultAllow.Location = new System.Drawing.Point(56, 6);
            this.rbDefaultAllow.Name = "rbDefaultAllow";
            this.rbDefaultAllow.Size = new System.Drawing.Size(50, 17);
            this.rbDefaultAllow.TabIndex = 1;
            this.rbDefaultAllow.TabStop = true;
            this.rbDefaultAllow.Text = "Allow";
            this.rbDefaultAllow.UseVisualStyleBackColor = true;
            // 
            // listBoxAuthzRules
            // 
            this.listBoxAuthzRules.FormattingEnabled = true;
            this.listBoxAuthzRules.Location = new System.Drawing.Point(9, 51);
            this.listBoxAuthzRules.Name = "listBoxAuthzRules";
            this.listBoxAuthzRules.Size = new System.Drawing.Size(492, 160);
            this.listBoxAuthzRules.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.gtwRuleDeleteBtn);
            this.tabPage5.Controls.Add(this.label18);
            this.tabPage5.Controls.Add(this.gtwRulesListBox);
            this.tabPage5.Controls.Add(this.gtwRuleAddBtn);
            this.tabPage5.Controls.Add(this.gtwRuleLocalGroupTB);
            this.tabPage5.Controls.Add(this.label17);
            this.tabPage5.Controls.Add(this.gtwRuleMysqlGroupTB);
            this.tabPage5.Controls.Add(this.gtwRuleConditionCB);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(542, 247);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "Gateway";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // gtwRuleDeleteBtn
            // 
            this.gtwRuleDeleteBtn.Image = global::pGina.Plugin.MySQLAuth.Properties.Resources.delete;
            this.gtwRuleDeleteBtn.Location = new System.Drawing.Point(505, 93);
            this.gtwRuleDeleteBtn.Name = "gtwRuleDeleteBtn";
            this.gtwRuleDeleteBtn.Size = new System.Drawing.Size(31, 33);
            this.gtwRuleDeleteBtn.TabIndex = 7;
            this.gtwRuleDeleteBtn.UseVisualStyleBackColor = true;
            this.gtwRuleDeleteBtn.Click += new System.EventHandler(this.gtwRuleDeleteBtn_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 13);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(77, 13);
            this.label18.TabIndex = 6;
            this.label18.Text = "Gateway rules:";
            // 
            // gtwRulesListBox
            // 
            this.gtwRulesListBox.FormattingEnabled = true;
            this.gtwRulesListBox.Location = new System.Drawing.Point(9, 29);
            this.gtwRulesListBox.Name = "gtwRulesListBox";
            this.gtwRulesListBox.Size = new System.Drawing.Size(490, 147);
            this.gtwRulesListBox.TabIndex = 5;
            // 
            // gtwRuleAddBtn
            // 
            this.gtwRuleAddBtn.Location = new System.Drawing.Point(433, 215);
            this.gtwRuleAddBtn.Name = "gtwRuleAddBtn";
            this.gtwRuleAddBtn.Size = new System.Drawing.Size(66, 22);
            this.gtwRuleAddBtn.TabIndex = 4;
            this.gtwRuleAddBtn.Text = "Add Rule";
            this.gtwRuleAddBtn.UseVisualStyleBackColor = true;
            this.gtwRuleAddBtn.Click += new System.EventHandler(this.gtwRuleAddBtn_Click);
            // 
            // gtwRuleLocalGroupTB
            // 
            this.gtwRuleLocalGroupTB.Location = new System.Drawing.Point(194, 217);
            this.gtwRuleLocalGroupTB.Name = "gtwRuleLocalGroupTB";
            this.gtwRuleLocalGroupTB.Size = new System.Drawing.Size(233, 20);
            this.gtwRuleLocalGroupTB.TabIndex = 3;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(93, 221);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(95, 13);
            this.label17.TabIndex = 2;
            this.label17.Text = "add to local group:";
            // 
            // gtwRuleMysqlGroupTB
            // 
            this.gtwRuleMysqlGroupTB.Location = new System.Drawing.Point(194, 191);
            this.gtwRuleMysqlGroupTB.Name = "gtwRuleMysqlGroupTB";
            this.gtwRuleMysqlGroupTB.Size = new System.Drawing.Size(233, 20);
            this.gtwRuleMysqlGroupTB.TabIndex = 1;
            // 
            // gtwRuleConditionCB
            // 
            this.gtwRuleConditionCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gtwRuleConditionCB.FormattingEnabled = true;
            this.gtwRuleConditionCB.Items.AddRange(new object[] {
            "If a member of MySQL group:",
            "If not a member of MySQL group:"});
            this.gtwRuleConditionCB.Location = new System.Drawing.Point(6, 191);
            this.gtwRuleConditionCB.Name = "gtwRuleConditionCB";
            this.gtwRuleConditionCB.Size = new System.Drawing.Size(182, 21);
            this.gtwRuleConditionCB.TabIndex = 0;
            this.gtwRuleConditionCB.SelectedIndexChanged += new System.EventHandler(this.gtwRuleConditionCB_SelectedIndexChanged);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 490);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.createTableBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Name = "Configuration";
            this.Text = "MySQL Plugin Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControlDBSchema.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabAuthz.ResumeLayout(false);
            this.tabAuthz.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.TextBox hostTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox portTB;
        private System.Windows.Forms.TextBox userTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox passwordTB;
        private System.Windows.Forms.CheckBox passwdCB;
        private System.Windows.Forms.CheckBox useSslCB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox userTableTB;
        private System.Windows.Forms.TextBox dbTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button createTableBtn;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.RadioButton encHexRB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton encBase64RB;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox unameColTB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox passwdColTB;
        private System.Windows.Forms.TextBox hashMethodColTB;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabControl tabControlDBSchema;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox userPrimaryKeyColTB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox groupTablePrimaryKeyColTB;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox groupNameColTB;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox groupTableNameTB;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox userGroupGroupFKColTB;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox userGroupUserFKColTB;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox userGroupTableNameTB;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button gtwRuleAddBtn;
        private System.Windows.Forms.TextBox gtwRuleLocalGroupTB;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox gtwRuleMysqlGroupTB;
        private System.Windows.Forms.ComboBox gtwRuleConditionCB;
        private System.Windows.Forms.Button gtwRuleDeleteBtn;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ListBox gtwRulesListBox;
        private System.Windows.Forms.TabPage tabAuthz;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.RadioButton rbDefaultDeny;
        private System.Windows.Forms.RadioButton rbDefaultAllow;
        private System.Windows.Forms.ListBox listBoxAuthzRules;
        private System.Windows.Forms.Button btnAuthzGroupRuleAdd;
        private System.Windows.Forms.ComboBox cbAuthzGroupRuleAllowOrDeny;
        private System.Windows.Forms.TextBox tbAuthzRuleGroup;
        private System.Windows.Forms.ComboBox cbAuthzMySqlGroupMemberOrNot;
        private System.Windows.Forms.CheckBox ckDenyWhenMySqlAuthFails;
        private System.Windows.Forms.Button btnAuthzGroupRuleUp;
        private System.Windows.Forms.Button btnAuthzGroupRuleDown;
        private System.Windows.Forms.Button btnAuthzGroupRuleDelete;
        private System.Windows.Forms.Label label20;
    }
}