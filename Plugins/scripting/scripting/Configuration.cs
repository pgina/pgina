using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pGina.Plugin.scripting
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void SaveSettings()
        {
            try
            {
                Settings.Store.authe_sys = Setgridview(this.authentication_sys_grid);

                Settings.Store.autho_sys = Setgridview(this.authorization_sys_grid);

                Settings.Store.gateway_sys = Setgridview(this.gateway_sys_grid);

                Settings.Store.notification_sys = Setgridview(this.notification_sys_grid);
                Settings.Store.notification_usr = Setgridview(this.notification_usr_grid);

                Settings.Store.changepwd_sys = Setgridview(this.changepwd_sys_grid);
                Settings.Store.changepwd_usr = Setgridview(this.changepwd_usr_grid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, String.Format("scripting plugin\n\n{0}", ex.Message), "can't save settings in registry");
            }
        }

        private void LoadSettings()
        {
            try
            {
                Getgridview(this.authentication_sys_grid, (string[])Settings.Store.authe_sys);

                Getgridview(this.authorization_sys_grid, (string[])Settings.Store.autho_sys);

                Getgridview(this.gateway_sys_grid, (string[])Settings.Store.gateway_sys);

                Getgridview(this.notification_sys_grid, (string[])Settings.Store.notification_sys);
                Getgridview(this.notification_usr_grid, (string[])Settings.Store.notification_usr);

                Getgridview(this.changepwd_sys_grid, (string[])Settings.Store.changepwd_sys);
                Getgridview(this.changepwd_usr_grid, (string[])Settings.Store.changepwd_usr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, String.Format("scripting plugin\n\n{0}", ex.Message), "can't load settings from registry");
            }
        }

        private void help_btn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://mutonufoai.github.io/pgina/documentation/plugins/scripting.html");
        }

        private void save_btn_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Getgridview(DataGridView grid, string[] data)
        {
            List<string> lines = data.ToList();
            foreach (string line in lines)
            {
                string[] split = line.Split('\t');
                if (split.Count() == 2)
                {
                    split[0] = split[0].Trim();
                    split[1] = split[1].Trim();

                    if (!String.IsNullOrEmpty(split[0]) && !String.IsNullOrEmpty(split[1]))
                    {
                        bool pwd = Convert.ToBoolean(split[0]);
                        string script = split[1];

                        grid.Rows.Add(pwd, script);
                    }
                }
            }

            /*
            string[] AttribConv = Settings.Store.AttribConv;
            grid.DataSource = data;
            dataGridView1.ColumnCount = 2;
            for (int x = 0; x < AttribConv.Count(); x++)
            {
                string[] split = AttribConv[x].Split('\t');
                if (split.Count() == 2)
                {
                    split[0] = split[0].Trim();
                    split[1] = split[1].Trim();
                    if (!String.IsNullOrEmpty(split[0]) && !String.IsNullOrEmpty(split[1]))
                    {
                        if (AttribConvert.Attribs.Contains(split[0]))
                        //if (Array.Exists(WinValues(), element => element == split[0]))
                        {
                            int index = AttribConvert.Attribs.IndexOf(split[0]);
                            //int index = Array.FindIndex(WinValues(), item => item == split[0]);

                            DataGridViewRow row = new DataGridViewRow();
                            DataGridViewComboBoxCell CellSample = new DataGridViewComboBoxCell();
                            CellSample.DataSource = AttribConvert.Attribs.ToArray(); // list of the string items that I want to insert in ComboBox.
                            CellSample.Value = AttribConvert.Attribs[index]; // default value for the ComboBox
                            row.Cells.Add(CellSample);

                            row.Cells.Add(new DataGridViewTextBoxCell()
                            {
                                Value = split[1]
                            });
                            dataGridView1.Rows.Add(row);
                        }
                    }
                }
            }*/
        }

        private string[] Setgridview(DataGridView grid)
        {
            List<string> AttribConv = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells[1].Value != null)
                {
                    if (row.Cells[0].Value == null)
                    {
                        AttribConv.Add(bool.FalseString + "\t" + row.Cells[1].Value.ToString().Trim());
                    }
                    else
                    {
                        AttribConv.Add(row.Cells[0].Value.ToString() + "\t" + row.Cells[1].Value.ToString().Trim());
                    }
                }
            }
            if (AttribConv.Count > 0)
            {
                return AttribConv.ToArray();
            }

            return new string[] { };
        }
    }
}
