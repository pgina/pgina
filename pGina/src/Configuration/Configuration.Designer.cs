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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
            this.pluginsGroupBox = new System.Windows.Forms.GroupBox();
            this.pluginInfoButton = new System.Windows.Forms.Button();
            this.pluginsDG = new System.Windows.Forms.DataGridView();
            this.configureButton = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstPluginDirs = new System.Windows.Forms.ListView();
            this.btnOkay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listView4 = new System.Windows.Forms.ListView();
            this.listView3 = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
            this.uiOrderLabel = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.pluginsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pluginsDG)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pluginsGroupBox
            // 
            this.pluginsGroupBox.Controls.Add(this.pluginInfoButton);
            this.pluginsGroupBox.Controls.Add(this.pluginsDG);
            this.pluginsGroupBox.Controls.Add(this.configureButton);
            this.pluginsGroupBox.Location = new System.Drawing.Point(12, 107);
            this.pluginsGroupBox.Name = "pluginsGroupBox";
            this.pluginsGroupBox.Size = new System.Drawing.Size(940, 195);
            this.pluginsGroupBox.TabIndex = 8;
            this.pluginsGroupBox.TabStop = false;
            this.pluginsGroupBox.Text = "All Available Plugins";
            // 
            // pluginInfoButton
            // 
            this.pluginInfoButton.Location = new System.Drawing.Point(859, 134);
            this.pluginInfoButton.Name = "pluginInfoButton";
            this.pluginInfoButton.Size = new System.Drawing.Size(74, 24);
            this.pluginInfoButton.TabIndex = 11;
            this.pluginInfoButton.Text = "Info...";
            this.pluginInfoButton.UseVisualStyleBackColor = true;
            this.pluginInfoButton.Click += new System.EventHandler(this.pluginInfoButton_Click);
            // 
            // pluginsDG
            // 
            this.pluginsDG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pluginsDG.Location = new System.Drawing.Point(10, 19);
            this.pluginsDG.Name = "pluginsDG";
            this.pluginsDG.Size = new System.Drawing.Size(838, 169);
            this.pluginsDG.TabIndex = 10;
            // 
            // configureButton
            // 
            this.configureButton.Location = new System.Drawing.Point(860, 164);
            this.configureButton.Name = "configureButton";
            this.configureButton.Size = new System.Drawing.Size(74, 25);
            this.configureButton.TabIndex = 9;
            this.configureButton.Text = "Configure...";
            this.configureButton.UseVisualStyleBackColor = true;
            this.configureButton.Click += new System.EventHandler(this.configureButton_Click);
            // 
            // btnDown
            // 
            this.btnDown.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.btnDown.Location = new System.Drawing.Point(192, 147);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(39, 41);
            this.btnDown.TabIndex = 7;
            this.btnDown.UseVisualStyleBackColor = true;
            // 
            // btnUp
            // 
            this.btnUp.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.btnUp.Location = new System.Drawing.Point(192, 100);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(39, 41);
            this.btnUp.TabIndex = 6;
            this.btnUp.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRemove);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.lstPluginDirs);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(831, 89);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugin Directories";
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(750, 48);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click_1);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(749, 19);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click_1);
            // 
            // lstPluginDirs
            // 
            this.lstPluginDirs.Location = new System.Drawing.Point(10, 19);
            this.lstPluginDirs.MultiSelect = false;
            this.lstPluginDirs.Name = "lstPluginDirs";
            this.lstPluginDirs.Size = new System.Drawing.Size(734, 52);
            this.lstPluginDirs.TabIndex = 5;
            this.lstPluginDirs.UseCompatibleStateImageBehavior = false;
            this.lstPluginDirs.View = System.Windows.Forms.View.Details;
            // 
            // btnOkay
            // 
            this.btnOkay.Location = new System.Drawing.Point(871, 672);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(790, 672);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.listView4);
            this.groupBox2.Controls.Add(this.listView3);
            this.groupBox2.Controls.Add(this.listView2);
            this.groupBox2.Controls.Add(this.uiOrderLabel);
            this.groupBox2.Controls.Add(this.listView1);
            this.groupBox2.Controls.Add(this.btnDown);
            this.groupBox2.Controls.Add(this.btnUp);
            this.groupBox2.Location = new System.Drawing.Point(12, 308);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(940, 358);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plugin Order";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(776, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Gateway";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(532, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Authorization";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Authentication";
            // 
            // button7
            // 
            this.button7.Image = ((System.Drawing.Image)(resources.GetObject("button7.Image")));
            this.button7.Location = new System.Drawing.Point(660, 147);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(39, 41);
            this.button7.TabIndex = 18;
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Image = ((System.Drawing.Image)(resources.GetObject("button6.Image")));
            this.button6.Location = new System.Drawing.Point(894, 147);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(39, 41);
            this.button6.TabIndex = 17;
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.Location = new System.Drawing.Point(660, 100);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(39, 41);
            this.button5.TabIndex = 16;
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.Location = new System.Drawing.Point(894, 100);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(39, 41);
            this.button4.TabIndex = 15;
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Image = global::pGina.Configuration.Properties.Resources.DownArrowSolid;
            this.button3.Location = new System.Drawing.Point(426, 147);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(39, 41);
            this.button3.TabIndex = 14;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Image = global::pGina.Configuration.Properties.Resources.UpArrowSolid;
            this.button2.Location = new System.Drawing.Point(426, 100);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(39, 41);
            this.button2.TabIndex = 13;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // listView4
            // 
            this.listView4.Location = new System.Drawing.Point(712, 32);
            this.listView4.Name = "listView4";
            this.listView4.Size = new System.Drawing.Size(176, 317);
            this.listView4.TabIndex = 12;
            this.listView4.UseCompatibleStateImageBehavior = false;
            // 
            // listView3
            // 
            this.listView3.Location = new System.Drawing.Point(478, 32);
            this.listView3.Name = "listView3";
            this.listView3.Size = new System.Drawing.Size(176, 318);
            this.listView3.TabIndex = 11;
            this.listView3.UseCompatibleStateImageBehavior = false;
            // 
            // listView2
            // 
            this.listView2.Location = new System.Drawing.Point(244, 32);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(176, 319);
            this.listView2.TabIndex = 10;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // uiOrderLabel
            // 
            this.uiOrderLabel.AutoSize = true;
            this.uiOrderLabel.Location = new System.Drawing.Point(54, 16);
            this.uiOrderLabel.Name = "uiOrderLabel";
            this.uiOrderLabel.Size = new System.Drawing.Size(89, 13);
            this.uiOrderLabel.TabIndex = 9;
            this.uiOrderLabel.Text = "Authentication UI";
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(10, 32);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(176, 320);
            this.listView1.TabIndex = 8;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 707);
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
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button configureButton;
        private System.Windows.Forms.DataGridView pluginsDG;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button pluginInfoButton;
        private System.Windows.Forms.Label uiOrderLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listView4;
        private System.Windows.Forms.ListView listView3;
        private System.Windows.Forms.ListView listView2;


    }
}

