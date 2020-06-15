﻿using System;
using System.Windows.Forms;

namespace SearchDataSPM.Admin_developer
{
    public partial class UserLogs : Form
    {
        private log4net.ILog log;

        public UserLogs()
        {
            InitializeComponent();
        }

        private void AdvancedDataGridView_FilterStringChanged(object sender, EventArgs e)
        {
            this.logBindingSource.Filter = this.advancedDataGridView1.FilterString;
        }

        private void AdvancedDataGridView_SortStringChanged(object sender, EventArgs e)
        {
            this.logBindingSource.Sort = this.advancedDataGridView1.SortString;
        }

        private void UserLogs_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed User Actions Log ");
            this.Dispose();
        }

        private void UserLogs_Load(object sender, EventArgs e)
        {
            // Suspend the layout logic for the form, while the application is initializing
            this.SuspendLayout();
            // TODO: This line of code loads data into the 'logDataset.Log' table. You can move, or remove it, as needed.
            this.logTableAdapter.Fill(this.logDataset.Log);

            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened User Action Logs");
            // Resume the layout logic
            this.ResumeLayout();
        }
    }
}