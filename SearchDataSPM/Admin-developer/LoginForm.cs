﻿using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SearchDataSPM
{
    public partial class LoginForm : MetroFramework.Forms.MetroForm
    {
        private string connection;
        private SqlConnection cn;
        private log4net.ILog log;

        private ErrorHandler errorHandler = new ErrorHandler();

        public LoginForm()
        {
            InitializeComponent();

            connection = System.Configuration.ConfigurationManager.ConnectionStrings["SearchDataSPM.Properties.Settings.cn"].ConnectionString;

            try
            {
                cn = new SqlConnection(connection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Activate();
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened Admin Login Form on " + System.Environment.UserName + " system.");
        }

        private void LoginBttn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                MetroFramework.MetroMessageBox.Show(this, "Please enter your username.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return;
            }

            try
            {
                string pass = ConverterHash.Encrypt(txtPassword.Text.Trim());
                using (SqlCommand sqlCommand = new SqlCommand("GetEmployeeLogininfo", cn) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    cn.Open();

                    sqlCommand.Parameters.AddWithValue("@username", txtUserName.Text);
                    sqlCommand.Parameters.AddWithValue("@password", pass);

                    int userCount = (int)sqlCommand.ExecuteScalar();
                    if (userCount == 1)
                    {
                        using (AdminControl frm = new AdminControl())//Open main form and hide login form
                        {
                            this.Hide();
                            frm.ShowDialog();
                            frm.Dispose();
                            //Application.Exit();
                        }
                    }
                    else
                    {
                        MetroFramework.MetroMessageBox.Show(this, "Your username and password don't match.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }
        }

        private void txtUserName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                LoginBttn.PerformClick();
            }
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed Admin Login Form on " + System.Environment.UserName + " system.");
            this.Dispose();
        }
    }
}