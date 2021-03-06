﻿using System;

namespace SearchDataSPM.Purchasing
{
    public partial class InvoiceFor : MetroFramework.Forms.MetroForm
    {
        public InvoiceFor()
        {
            InitializeComponent();
        }

        public string ValueIWant { get; set; }

        private void JobType_Load(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void MetroTile1_Click(object sender, EventArgs e)
        {
            ValueIWant = "Customer";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void MetroTile2_Click(object sender, EventArgs e)
        {
            ValueIWant = "Vendor";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}