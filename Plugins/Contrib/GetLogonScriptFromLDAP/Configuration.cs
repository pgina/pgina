using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using log4net;
using pGina.Shared.Settings;

namespace pGina.Plugin.LogonScriptFromLDAP
{
    public partial class Configuration : Form
    {

        public Configuration()
        {
            InitializeComponent();

            // to display Registry information 
            LoadSettings();

            // making the windows form unresizable
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        /// <summary>
        /// Displays in each textbox what was used the last time
        /// (data retrieved from the registry
        /// </summary>
        private void LoadSettings()
        {
            attribute_TextBox.Text = Settings.Store.Attribute;
            server_TextBox.Text    = Settings.Store.Server;
            domain_TextBox.Text    = Settings.Store.Domain;
        }

        /// <summary>
        /// stores the settings when the Save button is clicked.
        /// /!\ server and attribute textboxes must not be empty otherwise a messagebox pops up.
        /// </summary>
        private void save_btn_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                // settings are stored
                StoreSettings();

                // closing the window
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// checks if attribute and server textboxes have text in them.
        /// </summary>
        private bool ValidateInput()
        {
            if (attribute_TextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the name of the attribute.");
                return false;
            }

            if (server_TextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the address of the server.");
                return false;
            }

            // we accept empty domain names, so we are not checking anything

            return true;
        }

        /// <summary>
        /// stores every textbox content.
        /// </summary>
        private void StoreSettings()
        {
            Settings.Store.Attribute = attribute_TextBox.Text.Trim();
            Settings.Store.Server =    server_TextBox.Text.Trim();

            if (domain_TextBox.Text.Trim().Length == 0) Settings.Store.Domain = ""; // the user doesn't want to use any domain 
            else if (domain_TextBox.Text.Trim().EndsWith(@"\")) 
                Settings.Store.Domain = domain_TextBox.Text.Trim().Remove(domain_TextBox.Text.Trim().Length - 1); // we remove the potential useless \ at the end of the string
            else Settings.Store.Domain = domain_TextBox.Text.Trim();
        }

        /// <summary>
        /// closes the window when the Cancel button is clicked.
        /// the modifications are not saved.
        /// </summary>
        private void cancel_btn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}