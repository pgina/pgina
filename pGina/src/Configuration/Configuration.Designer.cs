namespace pGina.Configuration
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
            this.pluginsGroupBox = new System.Windows.Forms.GroupBox();
            this.pluginInfoButton = new System.Windows.Forms.Button();
            this.pluginsDG = new System.Windows.Forms.DataGridView();
            this.configureButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstPluginDirs = new System.Windows.Forms.ListView();
            this.btnOkay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gatewayDGV = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.gatewayBtnDown = new System.Windows.Forms.Button();
            this.gatewayBtnUp = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.authenticateDGV = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.authenticateBtnDown = new System.Windows.Forms.Button();
            this.authenticateBtnUp = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.authorizeDGV = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.authorizeBtnUp = new System.Windows.Forms.Button();
            this.authorizeBtnDown = new System.Windows.Forms.Button();
            this.pluginsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pluginsDG)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gatewayDGV)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authenticateDGV)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authorizeDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // pluginsGroupBox
            // 
            this.pluginsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginsGroupBox.Controls.Add(this.pluginInfoButton);
            this.pluginsGroupBox.Controls.Add(this.pluginsDG);
            this.pluginsGroupBox.Controls.Add(this.configureButton);
            this.pluginsGroupBox.Location = new System.Drawing.Point(12, 139);
            this.pluginsGroupBox.Name = "pluginsGroupBox";
            this.pluginsGroupBox.Size = new System.Drawing.Size(940, 163);
            this.pluginsGroupBox.TabIndex = 8;
            this.pluginsGroupBox.TabStop = false;
            this.pluginsGroupBox.Text = "All Available Plugins";
            // 
            // pluginInfoButton
            // 
            this.pluginInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginInfoButton.Location = new System.Drawing.Point(859, 92);
            this.pluginInfoButton.Name = "pluginInfoButton";
            this.pluginInfoButton.Size = new System.Drawing.Size(74, 24);
            this.pluginInfoButton.TabIndex = 11;
            this.pluginInfoButton.Text = "Info...";
            this.pluginInfoButton.UseVisualStyleBackColor = true;
            this.pluginInfoButton.Click += new System.EventHandler(this.pluginInfoButton_Click);
            // 
            // pluginsDG
            // 
            this.pluginsDG.AllowUserToAddRows = false;
            this.pluginsDG.AllowUserToDeleteRows = false;
            this.pluginsDG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginsDG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pluginsDG.Location = new System.Drawing.Point(10, 19);
            this.pluginsDG.Name = "pluginsDG";
            this.pluginsDG.Size = new System.Drawing.Size(838, 128);
            this.pluginsDG.TabIndex = 10;
            // 
            // configureButton
            // 
            this.configureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.configureButton.Location = new System.Drawing.Point(860, 122);
            this.configureButton.Name = "configureButton";
            this.configureButton.Size = new System.Drawing.Size(74, 25);
            this.configureButton.TabIndex = 9;
            this.configureButton.Text = "Configure...";
            this.configureButton.UseVisualStyleBackColor = true;
            this.configureButton.Click += new System.EventHandler(this.configureButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnRemove);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.lstPluginDirs);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(940, 121);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugin Directories";
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(859, 48);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click_1);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(858, 19);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click_1);
            // 
            // lstPluginDirs
            // 
            this.lstPluginDirs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPluginDirs.Location = new System.Drawing.Point(10, 19);
            this.lstPluginDirs.MultiSelect = false;
            this.lstPluginDirs.Name = "lstPluginDirs";
            this.lstPluginDirs.Size = new System.Drawing.Size(843, 96);
            this.lstPluginDirs.TabIndex = 5;
            this.lstPluginDirs.UseCompatibleStateImageBehavior = false;
            this.lstPluginDirs.View = System.Windows.Forms.View.Details;
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(877, 512);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(796, 512);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Location = new System.Drawing.Point(12, 308);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(940, 183);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plugin Order";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(928, 158);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gatewayDGV);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.gatewayBtnDown);
            this.panel1.Controls.Add(this.gatewayBtnUp);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(621, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(304, 152);
            this.panel1.TabIndex = 22;
            // 
            // gatewayDGV
            // 
            this.gatewayDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gatewayDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gatewayDGV.Location = new System.Drawing.Point(5, 16);
            this.gatewayDGV.Name = "gatewayDGV";
            this.gatewayDGV.Size = new System.Drawing.Size(263, 136);
            this.gatewayDGV.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Gateway";
            // 
            // gatewayBtnDown
            // 
            this.gatewayBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.gatewayBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.gatewayBtnDown.Location = new System.Drawing.Point(274, 85);
            this.gatewayBtnDown.Name = "gatewayBtnDown";
            this.gatewayBtnDown.Size = new System.Drawing.Size(26, 27);
            this.gatewayBtnDown.TabIndex = 17;
            this.gatewayBtnDown.UseVisualStyleBackColor = true;
            this.gatewayBtnDown.Click += new System.EventHandler(this.gatewayBtnDown_Click);
            // 
            // gatewayBtnUp
            // 
            this.gatewayBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.gatewayBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.gatewayBtnUp.Location = new System.Drawing.Point(274, 38);
            this.gatewayBtnUp.Name = "gatewayBtnUp";
            this.gatewayBtnUp.Size = new System.Drawing.Size(26, 27);
            this.gatewayBtnUp.TabIndex = 15;
            this.gatewayBtnUp.UseVisualStyleBackColor = true;
            this.gatewayBtnUp.Click += new System.EventHandler(this.gatewayBtnUp_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.authenticateDGV);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.authenticateBtnDown);
            this.panel3.Controls.Add(this.authenticateBtnUp);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(303, 152);
            this.panel3.TabIndex = 20;
            // 
            // authenticateDGV
            // 
            this.authenticateDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.authenticateDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authenticateDGV.Location = new System.Drawing.Point(0, 16);
            this.authenticateDGV.Name = "authenticateDGV";
            this.authenticateDGV.Size = new System.Drawing.Size(263, 136);
            this.authenticateDGV.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(95, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Authentication";
            // 
            // authenticateBtnDown
            // 
            this.authenticateBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authenticateBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.authenticateBtnDown.Location = new System.Drawing.Point(269, 85);
            this.authenticateBtnDown.Name = "authenticateBtnDown";
            this.authenticateBtnDown.Size = new System.Drawing.Size(26, 27);
            this.authenticateBtnDown.TabIndex = 14;
            this.authenticateBtnDown.UseVisualStyleBackColor = true;
            this.authenticateBtnDown.Click += new System.EventHandler(this.authenticateBtnDown_Click);
            // 
            // authenticateBtnUp
            // 
            this.authenticateBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authenticateBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.authenticateBtnUp.Location = new System.Drawing.Point(269, 38);
            this.authenticateBtnUp.Name = "authenticateBtnUp";
            this.authenticateBtnUp.Size = new System.Drawing.Size(26, 27);
            this.authenticateBtnUp.TabIndex = 13;
            this.authenticateBtnUp.UseVisualStyleBackColor = true;
            this.authenticateBtnUp.Click += new System.EventHandler(this.authenticateBtnUp_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.authorizeDGV);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.authorizeBtnUp);
            this.panel2.Controls.Add(this.authorizeBtnDown);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(312, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(303, 152);
            this.panel2.TabIndex = 23;
            // 
            // authorizeDGV
            // 
            this.authorizeDGV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.authorizeDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.authorizeDGV.Location = new System.Drawing.Point(3, 16);
            this.authorizeDGV.Name = "authorizeDGV";
            this.authorizeDGV.Size = new System.Drawing.Size(263, 136);
            this.authorizeDGV.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(101, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Authorization";
            // 
            // authorizeBtnUp
            // 
            this.authorizeBtnUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authorizeBtnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.authorizeBtnUp.Location = new System.Drawing.Point(272, 38);
            this.authorizeBtnUp.Name = "authorizeBtnUp";
            this.authorizeBtnUp.Size = new System.Drawing.Size(26, 27);
            this.authorizeBtnUp.TabIndex = 16;
            this.authorizeBtnUp.UseVisualStyleBackColor = true;
            this.authorizeBtnUp.Click += new System.EventHandler(this.authorizeBtnUp_Click);
            // 
            // authorizeBtnDown
            // 
            this.authorizeBtnDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.authorizeBtnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.authorizeBtnDown.Location = new System.Drawing.Point(272, 85);
            this.authorizeBtnDown.Name = "authorizeBtnDown";
            this.authorizeBtnDown.Size = new System.Drawing.Size(26, 27);
            this.authorizeBtnDown.TabIndex = 18;
            this.authorizeBtnDown.UseVisualStyleBackColor = true;
            this.authorizeBtnDown.Click += new System.EventHandler(this.authorizeBtnDown_Click);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 547);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.pluginsGroupBox);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOkay);
            this.Name = "Configuration";
            this.Text = "pGina Configuration";
            this.pluginsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pluginsDG)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gatewayDGV)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authenticateDGV)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.authorizeDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListView lstPluginDirs;
        private System.Windows.Forms.GroupBox pluginsGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button configureButton;
        private System.Windows.Forms.DataGridView pluginsDG;
        private System.Windows.Forms.Button pluginInfoButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button authorizeBtnDown;
        private System.Windows.Forms.Button gatewayBtnDown;
        private System.Windows.Forms.Button authorizeBtnUp;
        private System.Windows.Forms.Button gatewayBtnUp;
        private System.Windows.Forms.Button authenticateBtnDown;
        private System.Windows.Forms.Button authenticateBtnUp;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView gatewayDGV;
        private System.Windows.Forms.DataGridView authenticateDGV;
        private System.Windows.Forms.DataGridView authorizeDGV;


    }
}

