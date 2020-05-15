﻿using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SearchDataSPM.ECR
{
    public partial class ECR_Users : Form
    {
        private readonly SPMConnectAPI.ECR connectapi = new SPMConnectAPI.ECR();
        private DataTable dt;
        private string formlabel = "";
        private log4net.ILog log;
        private bool supervisor;

        public ECR_Users()
        {
            InitializeComponent();
        }

        public string ValueIWant { get; set; }

        public string formtext(string formname)
        {
            if (formname.Length > 0)
                return formlabel = formname;
            return "ECR Select available to user";
        }

        public bool IsSupervisor(bool issupervisor)
        {
            if (issupervisor)
                return supervisor = issupervisor;
            return false;
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string item;
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];
                item = Convert.ToString(slectedrow.Cells[0].Value);
                //MessageBox.Show(item);
            }
            else
            {
                item = "";
            }
            ValueIWant = item;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void ECR_Users_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed ECR Users Available ");
            this.Dispose();
        }

        private void ECR_Users_Load(object sender, EventArgs e)
        {
            this.Text = formlabel;
            dt = new DataTable();
            if (supervisor)
            {
                dt.Clear();
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT id, Name, Department, UserName FROM [SPM_Database].[dbo].[Users] where [ECRApproval2] = '1' ", connectapi.cn))
                {
                    try
                    {
                        if (connectapi.cn.State == ConnectionState.Closed)
                            connectapi.cn.Open();

                        dt.Clear();
                        sda.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SPM Connect - Show all shipping Home", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connectapi.cn.Close();
                    }
                }
            }
            else
            {
                dt.Clear();
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT id, Name, Department, UserName FROM [SPM_Database].[dbo].[Users] where [ECRHandler] = '1' ", connectapi.cn))
                {
                    try
                    {
                        if (connectapi.cn.State == ConnectionState.Closed)
                            connectapi.cn.Open();

                        dt.Clear();
                        sda.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SPM Connect - Show all shipping Home", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connectapi.cn.Close();
                    }
                }
            }

            dataGridView.DataSource = dt;
            _ = dt.DefaultView;
            dataGridView.Sort(dataGridView.Columns[1], ListSortDirection.Ascending);
            UpdateFont();
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened ECR Users Available ");
        }

        private void UpdateFont()
        {
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
            dataGridView.DefaultCellStyle.Font = new Font("Arial", 8.5F, FontStyle.Regular);
            dataGridView.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView.DefaultCellStyle.BackColor = Color.FromArgb(237, 237, 237);
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.Yellow;
            dataGridView.DefaultCellStyle.SelectionBackColor = Color.Black;
        }
    }
}