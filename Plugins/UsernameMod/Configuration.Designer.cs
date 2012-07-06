using System.Collections.Generic;
using System.Windows.Forms;
namespace pGina.Plugin.UsernameMod
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
            System.Windows.Forms.Label label2;
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.authButton = new System.Windows.Forms.RadioButton();
            this.authZButton = new System.Windows.Forms.RadioButton();
            this.gatewayButton = new System.Windows.Forms.RadioButton();
            this.actionBox = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.descLabel2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.descLabel1 = new System.Windows.Forms.Label();
            this.removeButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.downButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.rulesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rulesListView = new System.Windows.Forms.ListView();
            label2 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(5, 48);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(40, 13);
            label2.TabIndex = 12;
            label2.Text = "Action:";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(207, 402);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(71, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(279, 402);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // authButton
            // 
            this.authButton.AutoSize = true;
            this.authButton.Checked = true;
            this.authButton.Location = new System.Drawing.Point(8, 22);
            this.authButton.Name = "authButton";
            this.authButton.Size = new System.Drawing.Size(93, 17);
            this.authButton.TabIndex = 0;
            this.authButton.TabStop = true;
            this.authButton.Text = "Authentication";
            this.authButton.UseVisualStyleBackColor = true;
            // 
            // authZButton
            // 
            this.authZButton.AutoSize = true;
            this.authZButton.Location = new System.Drawing.Point(107, 22);
            this.authZButton.Name = "authZButton";
            this.authZButton.Size = new System.Drawing.Size(86, 17);
            this.authZButton.TabIndex = 1;
            this.authZButton.TabStop = true;
            this.authZButton.Text = "Authorization";
            this.authZButton.UseVisualStyleBackColor = true;
            // 
            // gatewayButton
            // 
            this.gatewayButton.AutoSize = true;
            this.gatewayButton.Location = new System.Drawing.Point(199, 22);
            this.gatewayButton.Name = "gatewayButton";
            this.gatewayButton.Size = new System.Drawing.Size(67, 17);
            this.gatewayButton.TabIndex = 2;
            this.gatewayButton.TabStop = true;
            this.gatewayButton.Text = "Gateway";
            this.gatewayButton.UseVisualStyleBackColor = true;
            // 
            // actionBox
            // 
            this.actionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionBox.FormattingEnabled = true;
            this.actionBox.Location = new System.Drawing.Point(51, 45);
            this.actionBox.Name = "actionBox";
            this.actionBox.Size = new System.Drawing.Size(114, 21);
            this.actionBox.TabIndex = 6;
            this.actionBox.SelectionChangeCommitted += new System.EventHandler(this.dropDownActionChange);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(88, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(71, 20);
            this.textBox1.TabIndex = 7;
            // 
            // descLabel2
            // 
            this.descLabel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.descLabel2.AutoSize = true;
            this.descLabel2.Location = new System.Drawing.Point(3, 6);
            this.descLabel2.Name = "descLabel2";
            this.descLabel2.Size = new System.Drawing.Size(102, 13);
            this.descLabel2.TabIndex = 8;
            this.descLabel2.Text = "will be replaced with";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(111, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(86, 20);
            this.textBox2.TabIndex = 9;
            // 
            // descLabel1
            // 
            this.descLabel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.descLabel1.AutoSize = true;
            this.descLabel1.Location = new System.Drawing.Point(3, 6);
            this.descLabel1.Name = "descLabel1";
            this.descLabel1.Size = new System.Drawing.Size(79, 13);
            this.descLabel1.TabIndex = 10;
            this.descLabel1.Text = "The characters";
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(272, 181);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 13;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.btnRemRule_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flowLayoutPanel2);
            this.groupBox2.Controls.Add(this.flowLayoutPanel1);
            this.groupBox2.Controls.Add(this.gatewayButton);
            this.groupBox2.Controls.Add(this.addButton);
            this.groupBox2.Controls.Add(this.authZButton);
            this.groupBox2.Controls.Add(this.authButton);
            this.groupBox2.Controls.Add(label2);
            this.groupBox2.Controls.Add(this.actionBox);
            this.groupBox2.Location = new System.Drawing.Point(7, 228);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(353, 168);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add Rule";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.descLabel2);
            this.flowLayoutPanel2.Controls.Add(this.textBox2);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(6, 108);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(341, 27);
            this.flowLayoutPanel2.TabIndex = 14;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.descLabel1);
            this.flowLayoutPanel1.Controls.Add(this.textBox1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 72);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(341, 28);
            this.flowLayoutPanel1.TabIndex = 13;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(272, 139);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 11;
            this.addButton.Text = "Add Rule";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.btnAddRule_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.downButton);
            this.groupBox1.Controls.Add(this.upButton);
            this.groupBox1.Controls.Add(this.rulesListView);
            this.groupBox1.Controls.Add(this.removeButton);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(353, 210);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "List of Rules";
            // 
            // downButton
            // 
            this.downButton.Image = global::pGina.Plugin.UsernameMod.Properties.Resources.DownArrowSolid;
            this.downButton.Location = new System.Drawing.Point(211, 181);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(25, 25);
            this.downButton.TabIndex = 15;
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // upButton
            // 
            this.upButton.Image = global::pGina.Plugin.UsernameMod.Properties.Resources.UpArrowSolid;
            this.upButton.Location = new System.Drawing.Point(241, 181);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(25, 25);
            this.upButton.TabIndex = 14;
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // rulesHeader
            // 
            this.rulesHeader.Text = "Rules";
            this.rulesHeader.Width = 335;
            // 
            // rulesListView
            // 
            this.rulesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.rulesHeader});
            this.rulesListView.Location = new System.Drawing.Point(6, 19);
            this.rulesListView.MultiSelect = false;
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.Size = new System.Drawing.Size(341, 156);
            this.rulesListView.TabIndex = 10;
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = System.Windows.Forms.View.Details;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 427);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Configuration";
            this.Text = "Modify Username Plugin Configuration";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton authButton;
        private System.Windows.Forms.RadioButton authZButton;
        private System.Windows.Forms.RadioButton gatewayButton;
        private System.Windows.Forms.ComboBox actionBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label descLabel2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label descLabel1;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private ListView rulesListView;
        private ColumnHeader rulesHeader;
    }
}