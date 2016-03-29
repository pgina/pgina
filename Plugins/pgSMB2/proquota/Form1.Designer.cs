namespace proquota
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "proquota";
            this.Icon = new System.Drawing.Icon(Properties.Resources._1, 32, 32);
            //this.Size = new System.Drawing.Size(0,0);
            this.Hide();

            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(components);
            this.notifyIcon1.Icon = new System.Drawing.Icon(Properties.Resources._1, 16, 16);
            this.notifyIcon1.Visible = true;
        }

        private System.Windows.Forms.NotifyIcon notifyIcon1;

        #endregion
    }
}

