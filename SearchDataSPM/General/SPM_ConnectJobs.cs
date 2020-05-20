﻿using SPMConnectAPI;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SPMConnectAPI.ConnectConstants;

namespace SearchDataSPM
{
    public partial class SPM_ConnectJobs : Form
    {
        #region SPM Connect Load

        private readonly SPMSQLCommands connectapi = new SPMSQLCommands();
        private readonly DataTable dt;
        private bool doneshowingSplash;
        private log4net.ILog log;

        public SPM_ConnectJobs()

        {
            InitializeComponent();

            dt = new DataTable();
            //connectapi.SPM_Connect();
        }

        private void Reload_Click(object sender, EventArgs e)
        {
            clearandhide();
            txtSearch.Clear();
            txtSearch.Focus();
            SendKeys.Send("~");
            dataGridView.Refresh();
        }

        private void Showallitems()
        {
            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();

                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM [SPM_Database].[dbo].[SPMJobs] ORDER BY Job DESC", connectapi.cn);

                dt.Clear();
                sda.Fill(dt);
                dataGridView.DataSource = dt;
                dataGridView.Sort(dataGridView.Columns[0], ListSortDirection.Descending);
                dataGridView.Columns[9].Visible = false;
                dataGridView.Columns[10].Visible = false;
                dataGridView.Columns[0].Width = 60;
                dataGridView.Columns[1].Width = 40;
                dataGridView.Columns[2].Width = 40;
                dataGridView.Columns[3].Width = 60;
                dataGridView.Columns[4].Width = 250;
                dataGridView.Columns[5].Width = 60;
                dataGridView.Columns[6].Width = 160;
                dataGridView.Columns[7].Width = 40;
                dataGridView.Columns[8].Width = 40;
                UpdateFont();

                //foreach (DataGridViewRow row in dataGridView.Rows)
                //{
                //    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                //    linkCell.Value = row.Cells[0].Value;
                //    row.Cells[0] = linkCell;
                //}
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message, "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Data cannot be retrieved from server. Please contact the admin.", "SPM Connect - SQL SERVER Job", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void SPM_Connect_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            versionlabel.Text = assembly.GetName().Version.ToString(3);
            versionlabel.Text = "V" + versionlabel.Text;
            TreeViewToolTip.SetToolTip(versionlabel, "SPM Connnect " + versionlabel.Text);
            Showallitems();

            if (ConnectUser.Management)
            {
                contextMenuStrip1.Items[3].Enabled = true;
                contextMenuStrip1.Items[3].Visible = true;
            }
            if (ConnectUser.PurchaseReq)
            {
                purchasereq.Enabled = true;
            }
            if (ConnectUser.Quote)
            {
                quotebttn.Enabled = true;
            }

            if (ConnectUser.Shipping)
            {
                shippingbttn.Enabled = true;
            }

            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened SPM Connect Jobs ");
        }

        private void UpdateFont()
        {
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
            dataGridView.DefaultCellStyle.Font = new Font("Arial", 9.5F, FontStyle.Bold);
            dataGridView.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView.DefaultCellStyle.BackColor = Color.FromArgb(237, 237, 237);
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.Yellow;
            dataGridView.DefaultCellStyle.SelectionBackColor = Color.Black;
        }

        #endregion SPM Connect Load

        #region Public Table & variables

        //public static string ItemNo;
        public static string description;

        public static string family;

        public static string Manufacturer;

        public static string oem;

        // variables required outside the functions to perfrom
        private readonly string fullsearch = ("FullSearch LIKE '%{0}%'");

        private DataTable table0 = new DataTable();
        private DataTable table1 = new DataTable();
        private DataTable table2 = new DataTable();
        private DataTable table3 = new DataTable();

        #endregion Public Table & variables

        #region Search Parameters

        public void txtSearch_KeyDown(object sender, KeyEventArgs e)

        {
            if (e.KeyCode == Keys.Return)

            {
                if (Descrip_txtbox.Visible)
                {
                    clearandhide();
                }
                Showallitems();
                mainsearch();
                if (txtSearch.Text.Length > 0)
                {
                    Descrip_txtbox.Show();
                    SendKeys.Send("{TAB}");
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void clearandhide()
        {
            Descrip_txtbox.Hide();
            Descrip_txtbox.Clear();
            filteroem_txtbox.Hide();
            filteroem_txtbox.Clear();
            filteroemitem_txtbox.Hide();
            filteroemitem_txtbox.Clear();
            filter4.Hide();
            filter4.Clear();
            table0.Clear();
            table1.Clear();
            table2.Clear();
            table3.Clear();
        }

        private void Descrip_txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            DataView dv = table0.DefaultView;
            table0 = dv.ToTable();

            if (e.KeyCode == Keys.Return)
            {
                string search2 = Descrip_txtbox.Text;
                try
                {
                    search2 = search2.Replace("'", "''");
                    search2 = search2.Replace("[", "[[]");
                    var secondFilter = string.Format(fullsearch, search2);
                    if (string.IsNullOrEmpty(dv.RowFilter))
                        dv.RowFilter = secondFilter;
                    else
                        dv.RowFilter += " AND " + secondFilter;
                    dataGridView.DataSource = dv;
                    SearchStringPosition();
                    searchtext(Descrip_txtbox.Text);
                    table1 = dv.ToTable();
                    dataGridView.Refresh();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Search Criteria Operator.", "SPM Connect");
                    Descrip_txtbox.Clear();
                    SendKeys.Send("~");
                }

                if (Descrip_txtbox.Text.Length > 0)
                {
                    filteroem_txtbox.Show();
                    SendKeys.Send("{TAB}");
                }
                else
                {
                    filteroem_txtbox.Hide();
                    filteroemitem_txtbox.Hide();
                    filter4.Hide();
                }
                if (!Descrip_txtbox.Visible)
                {
                    filteroem_txtbox.Hide();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void filter4_KeyDown(object sender, KeyEventArgs e)
        {
            DataView dv = table3.DefaultView;
            table3 = dv.ToTable();
            if (e.KeyCode == Keys.Return)
            {
                string search5 = filter4.Text;
                try
                {
                    search5 = search5.Replace("'", "''");
                    search5 = search5.Replace("[", "[[]");
                    var fifthfilter = string.Format(fullsearch, search5);

                    if (string.IsNullOrEmpty(dv.RowFilter))
                        dv.RowFilter = fifthfilter;
                    else
                        dv.RowFilter += " AND " + fifthfilter;
                    dataGridView.DataSource = dv;
                    SearchStringPosition();
                    searchtext(filter4.Text);
                    dataGridView.Refresh();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Search Criteria Operator.", "SPM Connect");
                    filter4.Clear();
                    SendKeys.Send("~");
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void filteroem_txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            DataView dv = table1.DefaultView;
            table1 = dv.ToTable();

            if (e.KeyCode == Keys.Return)
            {
                string search3 = filteroem_txtbox.Text;
                try
                {
                    search3 = search3.Replace("'", "''");
                    search3 = search3.Replace("[", "[[]");
                    var thirdFilter = string.Format(fullsearch, search3);
                    if (string.IsNullOrEmpty(dv.RowFilter))
                        dv.RowFilter = thirdFilter;
                    else
                        dv.RowFilter += " AND " + thirdFilter;
                    dataGridView.DataSource = dv;
                    SearchStringPosition();
                    searchtext(filteroem_txtbox.Text);
                    table2 = dv.ToTable();
                    dataGridView.Refresh();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Search Criteria Operator.", "SPM Connect");
                    filteroem_txtbox.Clear();
                    SendKeys.Send("~");
                }

                if (filteroem_txtbox.Text.Length > 0)
                {
                    filteroemitem_txtbox.Show();
                    SendKeys.Send("{TAB}");
                }
                else
                {
                    filteroemitem_txtbox.Hide();
                    filter4.Hide();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void filteroemitem_txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            DataView dv = table2.DefaultView;
            table2 = dv.ToTable();
            if (e.KeyCode == Keys.Return)
            {
                string search4 = filteroemitem_txtbox.Text;
                try
                {
                    search4 = search4.Replace("'", "''");
                    search4 = search4.Replace("[", "[[]");
                    var fourthfilter = string.Format(fullsearch, search4);

                    if (string.IsNullOrEmpty(dv.RowFilter))
                        dv.RowFilter = fourthfilter;
                    else
                        dv.RowFilter += " AND " + fourthfilter;
                    dataGridView.DataSource = dv;
                    SearchStringPosition();
                    searchtext(filteroemitem_txtbox.Text);
                    table3 = dv.ToTable();
                    dataGridView.Refresh();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Search Criteria Operator.", "SPM Connect");
                    filteroemitem_txtbox.Clear();
                    SendKeys.Send("~");
                }

                if (filteroemitem_txtbox.Text.Length > 0)
                {
                    filter4.Show();
                    SendKeys.Send("{TAB}");
                }
                else
                {
                    filter4.Hide();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void mainsearch()
        {
            DataView dv = dt.DefaultView;
            string search1 = txtSearch.Text;
            try
            {
                search1 = search1.Replace("'", "''");
                search1 = search1.Replace("[", "[[]");
                dv.RowFilter = string.Format(fullsearch, search1);
                dataGridView.DataSource = dt;
                table0 = dv.ToTable();
                dataGridView.Update();
                SearchStringPosition();
                searchtext(txtSearch.Text);
                dataGridView.Refresh();
            }
            catch (Exception)

            {
                MessageBox.Show("Invalid Search Criteria Operator.", "SPM Connect");
                txtSearch.Clear();
            }
        }

        #endregion Search Parameters

        #region Highlight Search Results

        private bool IsSelected;

        private string sw;

        private void dataGridView_CellPainting_1(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && IsSelected)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                if (!string.IsNullOrEmpty(sw))
                {
                    string val = (string)e.FormattedValue;
                    int sindx = val.IndexOf(sw, StringComparison.CurrentCultureIgnoreCase);
                    if (sindx >= 0)
                    {
                        Rectangle hl_rect = new Rectangle
                        {
                            Y = e.CellBounds.Y + 2,
                            Height = e.CellBounds.Height - 5
                        };

                        string sBefore = val.Substring(0, sindx);
                        string sWord = val.Substring(sindx, sw.Length);
                        Size s1 = TextRenderer.MeasureText(e.Graphics, sBefore, e.CellStyle.Font, e.CellBounds.Size);
                        Size s2 = TextRenderer.MeasureText(e.Graphics, sWord, e.CellStyle.Font, e.CellBounds.Size);

                        if (s1.Width > 5)
                        {
                            hl_rect.X = e.CellBounds.X + s1.Width - 5;
                            hl_rect.Width = s2.Width - 6;
                        }
                        else
                        {
                            hl_rect.X = e.CellBounds.X + 2;
                            hl_rect.Width = s2.Width - 6;
                        }

                        SolidBrush hl_brush = (e.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.None
                            ? new SolidBrush(Color.Black)
                            : new SolidBrush(Color.FromArgb(126, 206, 253));
                        e.Graphics.FillRectangle(hl_brush, hl_rect);

                        hl_brush.Dispose();
                    }
                }
                e.PaintContent(e.CellBounds);
            }
        }

        private void SearchStringPosition()
        {
            IsSelected = true;
        }

        private void searchtext(string searchkey)
        {
            sw = searchkey;
        }

        #endregion Highlight Search Results

        #region AdminControlLabel

        private void SPM_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("http://www.spm-automation.com/");
        }

        #endregion AdminControlLabel

        #region datagridview events

        private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1) return;
            _ = dataGridView.Rows[e.RowIndex];

            if (e.Button == MouseButtons.Right)
            {
                int columnindex = e.RowIndex;
                dataGridView.ClearSelection();
                dataGridView.Rows[columnindex].Selected = true;
            }
        }

        private void dataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(237, 237, 237);
            }
        }

        private void dataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(205, 230, 247);
            }
        }

        #endregion datagridview events

        #region Get BOM

        private void getBOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processbom(GetAssynumber());
        }

        // public static string jobtree;

        private void processbom(string itemvalue)
        {
            TreeView treeView = new TreeView(item: itemvalue);
            treeView.Show();
        }

        #endregion Get BOM

        #region GetProjectEng

        private void checksqltable(string job, string bom)
        {
            contextMenuStrip1.Visible = false;
            using (SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) FROM [SPM_Database].[dbo].[SPMJobsPath] WHERE JobNo = '" + job + "' AND BOMNo = '" + bom + "' AND Path is not null", connectapi.cn))
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();

                int userCount = (int)sqlCommand.ExecuteScalar();
                if (userCount > 0)
                {
                    connectapi.cn.Close();
                    grabpathfromtable(job, bom);
                }
                else
                {
                    connectapi.cn.Close();
                    DialogResult result = MessageBox.Show("Project folder not assigned. Would you like to assign one now?", "SPM Connect",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        //code for Yes
                        createnewpath(job, bom);
                    }
                    else if (result == DialogResult.No)
                    {
                        //code for No
                    }
                }
            }
        }

        private void createnewentry(string job, string bom, string folderPath, bool openfolder)
        {
            string sql = "INSERT INTO [SPM_Database].[dbo].[SPMJobsPath] ([JobNo], [BOMNo],[Path])" +
                "VALUES( '" + job + "','" + bom + "','" + folderPath + "')";

            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();
                SqlCommand _command = new SqlCommand
                {
                    Connection = connectapi.cn,
                    CommandText = sql
                };
                _command.ExecuteNonQuery();
                // MessageBox.Show("Item added to the catalog.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException)
            {
                // MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Technical error occured while saving the path. Please contact the admin.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
            if (openfolder)
            {
                openprojecteng(folderPath);
            }
        }

        private void createnewpath(string job, string bom)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(openFileDialog1.FileName);
                //MessageBox.Show(folderPath);
                //Process.Start(folderPath);
                createnewentry(job, bom, folderPath, true);

                //OpenFileDialog folderBrowser = new OpenFileDialog();
                //// Set validate names and check file exists to false otherwise windows will
                //// not let you select "Folder Selection."
                //folderBrowser.ValidateNames = false;
                //folderBrowser.CheckFileExists = false;
                //folderBrowser.CheckPathExists = true;
                //// Always default to Folder Selection.
                //folderBrowser.FileName = "Folder Selection.";
                //if (folderBrowser.ShowDialog() == DialogResult.OK)
                //{
                //    string folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                //    MessageBox.Show(folderPath);
                //    Process.Start(folderPath);
                //    // ...
                //}
            }
        }

        private string getbomitem()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(job);
                return Convert.ToString(slectedrow.Cells[3].Value);
            }
            return null;
        }

        private string getjob()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(job);
                return Convert.ToString(slectedrow.Cells[0].Value);
            }
            return null;
        }

        private void grabpathfromtable(string job, string bom)
        {
            DataTable _acountsTb = new DataTable();
            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();

                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM [SPM_Database].[dbo].[SPMJobsPath] WHERE  JobNo = '" + job + "' AND BOMNo = '" + bom + "'", connectapi.cn);

                sda.Fill(_acountsTb);
                string path = _acountsTb.Rows[0]["Path"].ToString();
                openprojecteng(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void openprojecteng(string folderPath)
        {
            Process.Start(folderPath);
        }

        private void projectEngineeringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checksqltable(getjob(), getbomitem());
        }

        #endregion GetProjectEng

        #region RemapFolderPath

        private void deleterecord(string job, string bom)
        {
            string sql = "DELETE FROM [SPM_Database].[dbo].[SPMJobsPath] WHERE  JobNo = '" + job + "' AND BOMNo = '" + bom + "'";

            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();
                SqlCommand _command = new SqlCommand
                {
                    Connection = connectapi.cn,
                    CommandText = sql
                };
                _command.ExecuteNonQuery();
                // MessageBox.Show("Item added to the catalog.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException)
            {
                // MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Technical error while updating path. Please contact the admin.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void remapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("Would you like to re-assign the folder?", "SPM Connect",
            //                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //{
            //    //code for Yes
            //    string job = getjob();
            //    string bom = getbomitem();
            //    deleterecord(job, bom);
            //    checksqltable(job, bom);
            //}
            //else if (result == DialogResult.No)
            //{
            //    //code for No
            //}
            string job = getjob();
            string bom = getbomitem();
            deleterecord(job, bom);
            checksqltable(job, bom);
        }

        #endregion RemapFolderPath

        #region CreateFolders

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                MessageBox.Show(
                  "Source directory does not exist or could not be found: "
                  + sourceDirName);
                return;
            }
            if (Directory.Exists(destDirName))
            {
                if (MessageBox.Show(destDirName + " already exists\r\nDo you want to overwrite it?", "Overwrite Folder  - SPM Connect", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                {
                    DirectoryInfo[] dirs = dir.GetDirectories();
                    // If the destination directory doesn't exist, create it.
                    if (!Directory.Exists(destDirName))
                    {
                        Directory.CreateDirectory(destDirName);
                    }

                    // Get the files in the directory and copy them to the new location.
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        string temppath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(temppath, true);
                    }

                    // If copying subdirectories, copy them and their contents to new location.
                    if (copySubDirs)
                    {
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            string temppath = Path.Combine(destDirName, subdir.Name);
                            DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                        }
                    }
                }
                else { return; }
            }
            else
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
        }

        private void CreateFolderButton_Click(object sender, EventArgs e)
        {
            ServiceReportHome ecr = new ServiceReportHome();
            ecr.Show();
        }

        private void Createfolders()
        {
            try
            {
                string jobnumber = Getjobnumber();
                string salesorder = getsalesorder();
                string jobdescription = getjobdescription();
                string customer = connectapi.Getcustomeralias(getcutomerid());
                if (customer.Length > 1)
                {
                    DialogResult result = MessageBox.Show(
                 "JobNumber = " + jobnumber + Environment.NewLine +
                 "SalesOrder = " + salesorder + Environment.NewLine +
                 "Job Description = " + jobdescription + Environment.NewLine +
                 "Customer = " + customer, "Create Job Folders?",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        Createjobfolders(jobnumber, customer, jobdescription, salesorder);
                    }
                }
                else
                {
                    MetroFramework.MetroMessageBox.Show(this, "Customer short name not found. Error with customer alias. Please contact admin.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Customer short name not found. Error with customer alias.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void createFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Createfolders();
        }

        private async void Createjobfolders(string jobnumber, string customer, string jobdescription, string salesorder)
        {
            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            string ValueIWantFromProptForm = "";
            JobType jobtype = new JobType();
            if (jobtype.ShowDialog() == DialogResult.OK)
            {
                ValueIWantFromProptForm = jobtype.ValueIWant;
            }

            if (ValueIWantFromProptForm.Length > 0)
            {
                await Task.Run(() => SplashDialog("Creating Folders.....")).ConfigureAwait(true);
                string destpatheng = "";
                string destpaths300 = "";
                string sourcepathseng = "";
                string sourcepaths300 = "";
                if (ValueIWantFromProptForm == "project")
                {
                    sourcepathseng = connectapi.GetConnectParameterValue("ProjectEngSp");
                    destpatheng = connectapi.GetConnectParameterValue("ProjectEngDp") + jobnumber + "_" + customer + "_" + jobdescription;
                    createnewentry(getjob(), getbomitem(), destpatheng, false);
                    sourcepaths300 = connectapi.GetConnectParameterValue("ProjectSalesSp");
                    destpaths300 = connectapi.GetConnectParameterValue("ProjectSalesDp") + jobnumber + "_" + customer + "_" + jobdescription;
                }
                else if (ValueIWantFromProptForm == "spare")
                {
                    sourcepathseng = connectapi.GetConnectParameterValue("SpareEngSp");
                    destpatheng = connectapi.GetConnectParameterValue("SpareEngDp") + jobnumber + "_" + customer + "_Spare Parts";
                    sourcepaths300 = connectapi.GetConnectParameterValue("SpareSalesSp");
                    destpaths300 = connectapi.GetConnectParameterValue("SpareSalesDp") + salesorder + "_" + customer + "_Spare Parts";
                }
                else if (ValueIWantFromProptForm == "service")
                {
                    sourcepathseng = connectapi.GetConnectParameterValue("ServiceEngSp");
                    destpatheng = connectapi.GetConnectParameterValue("ServiceEngDp") + jobnumber + "_" + customer + "_Service_" + jobdescription;
                    sourcepaths300 = connectapi.GetConnectParameterValue("ServiceSalesSp");
                    destpaths300 = connectapi.GetConnectParameterValue("ServiceSalesDp") + salesorder + "_" + customer + "_Service_" + jobdescription;
                }
                if (ValueIWantFromProptForm == "project" || ValueIWantFromProptForm == "spare" || ValueIWantFromProptForm == "service")
                {
                    DirectoryCopy(sourcepathseng, destpatheng, true);
                    DirectoryCopy(sourcepaths300, destpaths300, true);
                }
                doneshowingSplash = true;

                MessageBox.Show("Job folders created sucessfully!.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Job type selection was not made. System cannot create folders for the selected job.", "SPM Connect - Create New Folders", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Cursor.Current = Cursors.Default;
            this.Enabled = true;
        }

        private string GetAssynumber()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(ItemNo);
                return Convert.ToString(slectedrow.Cells[3].Value);
            }
            else
            {
                return "";
            }
        }

        private string getcutomerid()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(ItemNo);
                return Convert.ToString(slectedrow.Cells[10].Value);
            }
            else
            {
                return "";
            }
        }

        private string getjobdescription()
        {
            string jobdescription;
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];
                jobdescription = Convert.ToString(slectedrow.Cells[4].Value);
                //MessageBox.Show(jobdescription);

                Regex reg = new Regex("[*'\"/,_&#^@]");
                jobdescription = reg.Replace(jobdescription, "-");
                jobdescription = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(jobdescription.ToLower());
                return jobdescription;
            }
            else
            {
                return "";
            }
        }

        private string Getjobnumber()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(ItemNo);
                return Convert.ToString(slectedrow.Cells[0].Value);
            }
            else
            {
                return "";
            }
        }

        private string getsalesorder()
        {
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];

                //MessageBox.Show(ItemNo);
                return Convert.ToString(slectedrow.Cells[5].Value);
            }
            else
            {
                return "";
            }
        }

        private void SplashDialog(string message)
        {
            doneshowingSplash = false;
            ThreadPool.QueueUserWorkItem((_) =>
            {
                using (var splashForm = new Dialog())
                {
                    splashForm.TopMost = true;
                    splashForm.Focus();
                    splashForm.Activate();
                    splashForm.Message = message;
                    splashForm.Location = new Point(this.Location.X + ((this.Width - splashForm.Width) / 2), this.Location.Y + ((this.Height - splashForm.Height) / 2));
                    splashForm.Show();
                    while (!doneshowingSplash)
                        Application.DoEvents();
                    splashForm.Close();
                }
            });
        }

        #endregion CreateFolders

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Home))
            {
                Reload.PerformClick();
                return true;
            }
            if (keyData == (Keys.Control | Keys.B))
            {
                string item;
                if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
                {
                    int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                    DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];
                    item = Convert.ToString(slectedrow.Cells[3].Value);
                }
                else
                {
                    item = "";
                }
                processbom(item);

                return true;
            }

            if (keyData == (Keys.Control | Keys.W))
            {
                this.Close();
                return true;
            }

            if (keyData == (Keys.Control | Keys.D))
            {
                showworkorder();
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void cribbttn_Click(object sender, EventArgs e)
        {
            if (ConnectUser.CribCheckout)
            {
                InvInOut invInOut = new InvInOut();
                invInOut.Show();
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Your request can't be completed based on your security settings.", "SPM Connect - Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Ecrbutton_Click(object sender, EventArgs e)
        {
            ECRHome ecr = new ECRHome();
            ecr.Show();
        }

        private void GetEstimateBOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string item;
            string estid;
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];
                item = Convert.ToString(slectedrow.Cells[3].Value);
                estid = Convert.ToString(slectedrow.Cells[1].Value);
                //MessageBox.Show(ItemNo);
            }
            else
            {
                item = "";
                estid = "";
            }

            ProcessbomEstimate(item, estid);
        }

        private void ProcessbomEstimate(string itemvalue, string estid)
        {
            EstimateBOM treeView = new EstimateBOM();
            treeView.item(itemvalue);
            treeView.estimate(estid);
            treeView.Show();
        }

        private void purchasereq_Click(object sender, EventArgs e)
        {
            int openforms = Application.OpenForms.Count;
            bool checkmaintenance = connectapi.GetConnectParameterValue("PurchaseReqDev") == "1";
            if (openforms >= 2)
            {
                bool purchasereqopen = false;

                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.Name == "PurchaseReqform")
                    {
                        purchasereqopen = true;
                        if (!checkmaintenance)
                        {
                            frm.Show();
                            frm.Activate();
                            frm.BringToFront();
                            frm.Focus();
                            frm.WindowState = FormWindowState.Normal;
                            break;
                        }
                        else
                        {
                            frm.Show();
                            frm.Activate();
                            frm.BringToFront();
                            frm.Focus();
                            frm.WindowState = FormWindowState.Normal;
                            frm.Close();
                            MetroFramework.MetroMessageBox.Show(this, "SPM Connect puchase req module is under maintenance. Please check back soon. Sorry for the inconvenience.", "Purhase Req Under Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                    }
                }
                if (!purchasereqopen)
                {
                    if (!checkmaintenance)
                    {
                        PurchaseReqform purchaseReq = new PurchaseReqform();
                        purchaseReq.Show();
                    }
                    else if (checkmaintenance && ConnectUser.Developer)
                    {
                        PurchaseReqform purchaseReq = new PurchaseReqform();
                        purchaseReq.Show();
                    }
                    else
                    {
                        MetroFramework.MetroMessageBox.Show(this, "SPM Connect puchase req module is under maintenance. Please check back soon. Sorry for the inconvenience.", "Purhase Req Under Maintenance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #region GetWorkorder

        private string getselectedjobnumber()
        {
            string item = "";
            if (dataGridView.SelectedRows.Count == 1 || dataGridView.SelectedCells.Count == 1)
            {
                int selectedrowindex = dataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow slectedrow = dataGridView.Rows[selectedrowindex];
                item = Convert.ToString(slectedrow.Cells[0].Value);
            }
            return item;
        }

        private void getWorkOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showworkorder();
        }

        private void showworkorder()
        {
            SPM_ConnectWM sPM_ConnectWM = new SPM_ConnectWM(jobno: getselectedjobnumber());
            sPM_ConnectWM.Show();
        }

        #endregion GetWorkorder

        #region FormCLosing

        private void SPM_ConnectJobs_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed SPM Connect Jobs ");
            this.Dispose();
        }

        #endregion FormCLosing

        #region Quotes

        private void quotebttn_Click_1(object sender, EventArgs e)
        {
            General.SPM_ConnectQuoteManagement quoteTracking = new General.SPM_ConnectQuoteManagement();
            quoteTracking.Show();
        }

        #endregion Quotes

        private void scanwobttn_Click(object sender, EventArgs e)
        {
            if (ConnectUser.WOScan)
            {
                ScanWO scanWO = new ScanWO();
                scanWO.Show();
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Your request can't be completed based on your security settings.", "SPM Connect - Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Shippingbttn_Click(object sender, EventArgs e)
        {
            ShippingHome shipping = new ShippingHome();
            shipping.Show();
        }

        private void viewCurrentJobReleaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewRelease viewRelease = new ViewRelease(wrkorder: Getjobnumber(), job: true, jobassyno: GetAssynumber(), jobno: Getjobnumber());
            viewRelease.Show();
        }
    }
}