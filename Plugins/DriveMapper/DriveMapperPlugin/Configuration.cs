using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using pGina.Shared.Settings;

namespace pGina.Plugin.DriveMapper
{
    public partial class Configuration : Form
    {
        private static readonly string DRIVE_UNC_COLUMN = "UNC";
        private static readonly string DRIVE_LETTER_COLUMN = "DriveLetter";
        private static readonly string USE_ALTERNATE_CREDENTIALS_COLUMN = "UseAltCreds";
        private static readonly string USERNAME_COLUMN = "Username";
        private static readonly string PASS_COLUMN = "Password";
        private static readonly string[] DRIVES = {
                              "Z:", "Y:", "X:", "W:", "V:", "U:", "T:", "S:", "R:", "Q:",
                              "P:", "O:", "N:", "M:", "L:", "K:", "J:", "I:", "H:", "G:", "F:",
                              "D:", "B:", "A:"};
        private BindingList<DriveEntry> driveList;

        public Configuration()
        {
            InitializeComponent();
            driveList = new BindingList<DriveEntry>(Settings.Load());
            InitUI();
        }

        private void InitUI()
        {
            SetupDriveListDGV();
            SetupDriveComboBox();
            InitDetailsView();
            this.uncTextBox.Leave += new EventHandler(uncTextBox_Leave);
            this.unameTextBox.Leave += new EventHandler(unameTextBox_Leave);
            this.passwordTextBox.Leave += new EventHandler(passwordTextBox_Leave);
        }

        private void InitDetailsView()
        {
            // Initially disable everything in case no rows are selected in
            // the DataGridView.  If this is not the case, the selectionchanged
            // callback will enable what's needed.
            this.driveComboBox.Enabled = false;
            this.uncTextBox.Enabled = false;
            this.uncTextBox.Text = "";
            this.unameTextBox.Text = "";
            this.unameTextBox.Enabled = false;
            this.passwordTextBox.Text = "";
            this.passwordTextBox.Enabled = false;
            this.useAltCredsCB.Enabled = false;
        }

        private void SetupDriveComboBox()
        {
            foreach (string letter in DRIVES)
            {
                this.driveComboBox.Items.Add(letter);
            }
            this.driveComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.driveComboBox.SelectedIndexChanged += new EventHandler(driveComboBox_SelectedIndexChanged);
        }

        private void SetupDriveListDGV()
        {
            DataGridView dgv = this.driveListDGV;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;

            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = DRIVE_LETTER_COLUMN,
                DataPropertyName = "Drive",
                HeaderText = "Drive",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = DRIVE_UNC_COLUMN,
                DataPropertyName = "UncPath",
                HeaderText = "UNC Path",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false
            });
            dgv.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = USE_ALTERNATE_CREDENTIALS_COLUMN,
                DataPropertyName = "UseAltCreds",
                HeaderText = "Use Different Creds",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = USERNAME_COLUMN,
                DataPropertyName = "UserName",
                Visible = false
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = PASS_COLUMN,
                DataPropertyName = "Password",
                Visible = false
            });

            dgv.SelectionChanged += new EventHandler(driveListDGV_SelectionChanged);
            dgv.CellValueChanged += new DataGridViewCellEventHandler(driveListDGV_CellValueChanged);
            dgv.DataSource = this.driveList;
        }

        void driveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox && sender == this.driveComboBox)
            {
                int nSelectedRows = this.driveListDGV.SelectedRows.Count;
                if (nSelectedRows > 0)
                {
                    DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                    string dgvDrive = row.Cells[DRIVE_LETTER_COLUMN].Value as string;
                    string comboBoxDrive = this.driveComboBox.SelectedItem as string;
                    
                    // Do we need to make a change at all?
                    if (dgvDrive != comboBoxDrive)
                    {
                        if (DriveInList(comboBoxDrive))
                        {
                            MessageBox.Show("Drive " + comboBoxDrive + " is already used for another mapping.");
                            this.driveComboBox.SelectedItem = FindUnusedDriveLetter();
                        }
                        else
                        {
                            row.Cells[DRIVE_LETTER_COLUMN].Value = comboBoxDrive;
                        }
                    }
                }
            }
        }

        void driveListDGV_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (this.driveListDGV.Columns[e.ColumnIndex].Name == DRIVE_UNC_COLUMN)
            {
                this.uncTextBox.Text = this.driveListDGV[e.ColumnIndex, e.RowIndex].Value as string;
            }
        }

        void driveListDGV_SelectionChanged(object sender, EventArgs e)
        {
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                this.driveComboBox.Enabled = true;
                this.uncTextBox.Enabled = true;
                this.useAltCredsCB.Enabled = true;
                // Enabling of unameTextBox and passwordTextBox is managed in the
                // checkbox callback.
                PushAllFromDGV();
            }
            else
            {
                this.driveComboBox.Enabled = false;
                this.uncTextBox.Enabled = false;
                this.uncTextBox.Text = "";
                this.unameTextBox.Text = "";
                this.unameTextBox.Enabled = false;
                this.passwordTextBox.Text = "";
                this.passwordTextBox.Enabled = false;
                this.useAltCredsCB.Enabled = false;
            }
        }

        private void PushAllFromDGV()
        {
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DriveEntry entry = (DriveEntry)this.driveListDGV.SelectedRows[0].DataBoundItem;
                this.driveComboBox.SelectedItem = entry.Drive;
                this.uncTextBox.Text = entry.UncPath;
                this.unameTextBox.Text = entry.UserName;
                this.passwordTextBox.Text = entry.Password;
                this.useAltCredsCB.Checked = entry.UseAltCreds;
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Settings.Save(driveList.ToList());
            this.Close();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            this.driveList.Add(new DriveEntry
            {
                Drive = FindUnusedDriveLetter(),
                UncPath = "",
                UseAltCreds = false,
                UserName = "",
                Password = ""
            });
            this.driveListDGV.Rows[this.driveListDGV.Rows.Count - 1].Selected = true;
        }

        private string FindUnusedDriveLetter()
        {
            foreach (string d in DRIVES)
            {
                if (!DriveInList(d))
                    return d;
            }
            return null;
        }

        private bool DriveInList(string d)
        {
            bool driveUsed = false;
            foreach (DriveEntry entry in this.driveList)
            {
                if (entry.Drive == d)
                {
                    driveUsed = true;
                    break;
                }
            }
            return driveUsed;           
        }


        void uncTextBox_Leave(object sender, EventArgs e)
        {
            // Update the data in the DataGridView
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                row.Cells[DRIVE_UNC_COLUMN].Value = this.uncTextBox.Text.Trim();
            }
        }

        void passwordTextBox_Leave(object sender, EventArgs e)
        {
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                row.Cells[PASS_COLUMN].Value = this.passwordTextBox.Text;
            }
        }

        void unameTextBox_Leave(object sender, EventArgs e)
        {
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                row.Cells[USERNAME_COLUMN].Value = this.unameTextBox.Text;
            }
        }

        private void useAltCredsCB_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox)
            {
                // Update the fields in the lower area
                CheckBox cb = (CheckBox)sender;
                bool useAltCreds = cb.Checked;
                if (useAltCreds)
                {
                    this.unameTextBox.Enabled = true;
                    this.passwordTextBox.Enabled = true;
                }
                else
                {
                    this.unameTextBox.Enabled = false;
                    this.unameTextBox.Text = "";
                    this.passwordTextBox.Enabled = false;
                    this.passwordTextBox.Text = "";
                }

                // Update the data in the DataGridView
                int nSelectedRows = this.driveListDGV.SelectedRows.Count;
                if (nSelectedRows > 0)
                {
                    DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                    row.Cells[USE_ALTERNATE_CREDENTIALS_COLUMN].Value = useAltCreds;
                }
            }        
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            int nSelectedRows = this.driveListDGV.SelectedRows.Count;
            if (nSelectedRows > 0)
            {
                DataGridViewRow row = this.driveListDGV.SelectedRows[0];
                this.driveListDGV.Rows.Remove(row);
            }
        }
    }
}
