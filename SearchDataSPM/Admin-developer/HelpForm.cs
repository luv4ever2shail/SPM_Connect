﻿using SPMConnectAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Windows.Forms;

namespace SearchDataSPM
{
    public partial class HelpForm : Form
    {
        private SPMSQLCommands connectapi = new SPMSQLCommands();
        private log4net.ILog log;
        private List<string> filestoAttach = new List<string>();
        private ErrorHandler errorHandler = new ErrorHandler();

        public HelpForm()
        {
            InitializeComponent();
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();
            versionlbl.Text = string.Format("SPM Connect Version - {0}", version);
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened Help Form ");
        }

        private void shrtcutbttn_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"\\spm-adfs\SDBASE\SPM Connect SQL\ConnectHotKeys.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/spmconnect/SPM_Connect");
        }

        private List<string> Importfilename()
        {
            filestoAttach.Clear();
            List<string> files = new List<string>();
            openFileDialog1.FileName = "";
            openFileDialog1.Filter =
        "Images (*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|" +
        "All files (*.*)|*.*";

            openFileDialog1.Title = "Connect Error Image Browser";
            openFileDialog1.Multiselect = true;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.

            if (result == DialogResult.OK) // Test result.
            {
                foreach (string file in openFileDialog1.FileNames)
                {
                    try
                    {
                        files.Add(file);
                    }
                    catch (SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }
            return files;
        }

        private void browsebttn_Click(object sender, EventArgs e)
        {
            List<string> filestoattach = Importfilename();

            if (filestoattach.Count > 0)
            {
                label5.Text = "File attached : " + filestoattach.Count;
                //browsebttn.Visible = false;
                filestoAttach = filestoattach;
            }
            else
            {
                label5.Text = "Attach file : ";
                //browsebttn.Visible = true;
            }
        }

        private void Clearall()
        {
            filestoAttach.Clear();
            subtxt.Clear();
            notestxt.Clear();
            label5.Text = "Attach file : ";
            browsebttn.Visible = true;
        }

        private void sendemailbttn_Click(object sender, EventArgs e)
        {
            Sendemailtodevelopers(connectapi.Getuserfullname(), filestoAttach, subtxt.Text, notestxt.Text);
            Clearall();
            MessageBox.Show("Email successfully sent to developer.", "SPM Connect - Developer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Sendemailtodevelopers(string requser, List<string> files, string subject, string notes)
        {
            //connectapi.SPM_Connect();
            string[] nameemail = connectapi.Getdevelopersnamesandemail().ToArray();
            for (int i = 0; i < nameemail.Length; i++)
            {
                string[] values = nameemail[i].Replace("][", "~").Split('~');

                for (int a = 0; a < values.Length; a++)
                {
                    values[a] = values[a].Trim();
                }
                string email = values[0];
                string name = values[1];

                string[] names = name.Replace(" ", "~").Split('~');
                for (int b = 0; b < names.Length; b++)
                {
                    names[b] = names[b].Trim();
                }
                name = names[0];
                connectapi.SendemailListAttachments(email, "Connect Error Submitted - " + subject, "Hello " + name + "," + Environment.NewLine + requser + " sent this error report." + Environment.NewLine + notes + Environment.NewLine + Environment.NewLine + "Triggered by " + connectapi.UserName(), files, "");
            }
        }

        private void nametxt_TextChanged(object sender, EventArgs e)
        {
            if (notestxt.Text.Length > 0)
            {
                sendemailbttn.Enabled = true;
            }
            else
            {
                sendemailbttn.Enabled = false;
            }
        }

        private void notestxt_TextChanged(object sender, EventArgs e)
        {
            if (notestxt.Text.Length > 0)
            {
                sendemailbttn.Enabled = true;
            }
            else
            {
                sendemailbttn.Enabled = false;
            }
        }

        private void HelpForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed Help Form ");
            this.Dispose();
        }
    }
}