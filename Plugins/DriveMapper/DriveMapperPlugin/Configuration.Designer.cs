namespace pGina.Plugin.DriveMapper
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
            this.driveListDGV = new System.Windows.Forms.DataGridView();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.useAltCredsCB = new System.Windows.Forms.CheckBox();
            this.passLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.unameTextBox = new System.Windows.Forms.TextBox();
            this.unameLabel = new System.Windows.Forms.Label();
            this.driveComboBox = new System.Windows.Forms.ComboBox();
            this.driveLetterLabel = new System.Windows.Forms.Label();
            this.uncTextBox = new System.Windows.Forms.TextBox();
            this.uncPathLabel = new System.Windows.Forms.Label();
            this.removeBtn = new System.Windows.Forms.Button();
            this.addBtn = new System.Windows.Forms.Button();
            this.showPassCB = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.driveListDGV)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // driveListDGV
            // 
            this.driveListDGV.AllowUserToAddRows = false;
            this.driveListDGV.AllowUserToDeleteRows = false;
            this.driveListDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.driveListDGV.Location = new System.Drawing.Point(12, 12);
            this.driveListDGV.MultiSelect = false;
            this.driveListDGV.Name = "driveListDGV";
            this.driveListDGV.Size = new System.Drawing.Size(595, 212);
            this.driveListDGV.TabIndex = 0;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(603, 412);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(85, 28);
            this.okBtn.TabIndex = 1;
            this.okBtn.Text = "Save";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(507, 413);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(90, 27);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.showPassCB);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.useAltCredsCB);
            this.groupBox1.Controls.Add(this.passLabel);
            this.groupBox1.Controls.Add(this.passwordTextBox);
            this.groupBox1.Controls.Add(this.unameTextBox);
            this.groupBox1.Controls.Add(this.unameLabel);
            this.groupBox1.Controls.Add(this.driveComboBox);
            this.groupBox1.Controls.Add(this.driveLetterLabel);
            this.groupBox1.Controls.Add(this.uncTextBox);
            this.groupBox1.Controls.Add(this.uncPathLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 230);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(676, 176);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Drive Details";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(492, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 26);
            this.label1.TabIndex = 9;
            this.label1.Text = "%u will be replaced with \r\nthe username.";
            // 
            // useAltCredsCB
            // 
            this.useAltCredsCB.AutoSize = true;
            this.useAltCredsCB.Location = new System.Drawing.Point(75, 89);
            this.useAltCredsCB.Name = "useAltCredsCB";
            this.useAltCredsCB.Size = new System.Drawing.Size(140, 17);
            this.useAltCredsCB.TabIndex = 8;
            this.useAltCredsCB.Text = "Use different credentials";
            this.useAltCredsCB.UseVisualStyleBackColor = true;
            this.useAltCredsCB.CheckedChanged += new System.EventHandler(this.useAltCredsCB_CheckedChanged);
            // 
            // passLabel
            // 
            this.passLabel.AutoSize = true;
            this.passLabel.Location = new System.Drawing.Point(8, 141);
            this.passLabel.Name = "passLabel";
            this.passLabel.Size = new System.Drawing.Size(56, 13);
            this.passLabel.TabIndex = 7;
            this.passLabel.Text = "Password:";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(75, 138);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(354, 20);
            this.passwordTextBox.TabIndex = 6;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // unameTextBox
            // 
            this.unameTextBox.Location = new System.Drawing.Point(75, 112);
            this.unameTextBox.Name = "unameTextBox";
            this.unameTextBox.Size = new System.Drawing.Size(354, 20);
            this.unameTextBox.TabIndex = 5;
            // 
            // unameLabel
            // 
            this.unameLabel.AutoSize = true;
            this.unameLabel.Location = new System.Drawing.Point(6, 115);
            this.unameLabel.Name = "unameLabel";
            this.unameLabel.Size = new System.Drawing.Size(58, 13);
            this.unameLabel.TabIndex = 4;
            this.unameLabel.Text = "Username:";
            // 
            // driveComboBox
            // 
            this.driveComboBox.FormattingEnabled = true;
            this.driveComboBox.Location = new System.Drawing.Point(75, 19);
            this.driveComboBox.Name = "driveComboBox";
            this.driveComboBox.Size = new System.Drawing.Size(141, 21);
            this.driveComboBox.TabIndex = 3;
            // 
            // driveLetterLabel
            // 
            this.driveLetterLabel.AutoSize = true;
            this.driveLetterLabel.Location = new System.Drawing.Point(6, 22);
            this.driveLetterLabel.Name = "driveLetterLabel";
            this.driveLetterLabel.Size = new System.Drawing.Size(35, 13);
            this.driveLetterLabel.TabIndex = 2;
            this.driveLetterLabel.Text = "Drive:";
            // 
            // uncTextBox
            // 
            this.uncTextBox.Location = new System.Drawing.Point(75, 46);
            this.uncTextBox.Name = "uncTextBox";
            this.uncTextBox.Size = new System.Drawing.Size(405, 20);
            this.uncTextBox.TabIndex = 1;
            // 
            // uncPathLabel
            // 
            this.uncPathLabel.AutoSize = true;
            this.uncPathLabel.Location = new System.Drawing.Point(6, 53);
            this.uncPathLabel.Name = "uncPathLabel";
            this.uncPathLabel.Size = new System.Drawing.Size(58, 13);
            this.uncPathLabel.TabIndex = 0;
            this.uncPathLabel.Text = "UNC Path:";
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new System.Drawing.Point(613, 195);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(76, 30);
            this.removeBtn.TabIndex = 4;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            this.removeBtn.Click += new System.EventHandler(this.removeBtn_Click);
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(613, 159);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(76, 30);
            this.addBtn.TabIndex = 5;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // showPassCB
            // 
            this.showPassCB.AutoSize = true;
            this.showPassCB.Location = new System.Drawing.Point(435, 140);
            this.showPassCB.Name = "showPassCB";
            this.showPassCB.Size = new System.Drawing.Size(77, 17);
            this.showPassCB.TabIndex = 10;
            this.showPassCB.Text = "Show Text";
            this.showPassCB.UseVisualStyleBackColor = true;
            this.showPassCB.CheckedChanged += new System.EventHandler(this.showPassCB_CheckedChanged);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 454);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.driveListDGV);
            this.Name = "Configuration";
            this.Text = "Drive Mapper Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.driveListDGV)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView driveListDGV;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button removeBtn;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.CheckBox useAltCredsCB;
        private System.Windows.Forms.Label passLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox unameTextBox;
        private System.Windows.Forms.Label unameLabel;
        private System.Windows.Forms.ComboBox driveComboBox;
        private System.Windows.Forms.Label driveLetterLabel;
        private System.Windows.Forms.TextBox uncTextBox;
        private System.Windows.Forms.Label uncPathLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox showPassCB;
    }
}