﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SearchDataSPM.Quotes
{
    public partial class QuoteDetails : Form
    {
        private readonly SPMConnectAPI.ConnectAPI connectapi = new SPMConnectAPI.ConnectAPI();
        private readonly DataTable dt = new DataTable();
        private readonly List<string> list = new List<string>();
        private readonly string quoteno2 = "";
        private log4net.ILog log;

        public QuoteDetails(string quoteno)
        {
            InitializeComponent();
            this.quoteno2 = quoteno;
            dt = new DataTable();
        }

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
                    foreach (FileInfo file in dir.GetFiles())
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
                foreach (FileInfo file in dir.GetFiles())
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

        private void Backbttn_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your path." })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    webBrowser1.Url = new Uri(fbd.SelectedPath);
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void Category()
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT Category FROM [SPM_Database].[dbo].[QuoteFilters]", connectapi.cn))
            {
                try
                {
                    if (connectapi.cn.State == ConnectionState.Closed)
                        connectapi.cn.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    AutoCompleteStringCollection MyCollection = new AutoCompleteStringCollection();
                    while (reader.Read())
                    {
                        MyCollection.Add(reader.GetString(0));
                    }

                    //familytxtbox.AutoCompleteCustomSource = MyCollection;
                    Categorycombox.AutoCompleteCustomSource = MyCollection;
                    Categorycombox.DataSource = MyCollection;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SPM Connect New Item - Fill Customers Source", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connectapi.cn.Close();
                }
            }
        }

        private void Closedchkbox_Click(object sender, EventArgs e)
        {
            DialogResult result = MetroFramework.MetroMessageBox.Show(this, "Are you sure want to make this choice?", "SPM Connect?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (closedchkbox.Checked)
                {
                    closedchkbox.Checked = true;
                    cvttojobchkbox.Checked = false;
                    cvttojobchkbox.Visible = false;
                    closedchkbox.Enabled = false;
                    Regex reg = new Regex("[*'\"/,_&#^@]");
                    string jobdescription = reg.Replace(Descriptiontxtbox.Text, "");
                    jobdescription = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(jobdescription.ToLower());
                    string customer = reg.Replace(familycombobox.Text, "");
                    //customer = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(customer.ToLower());

                    //string sourcepath = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\Q" + ItemTxtBox.Text + "_" + customer + "_" + jobdescription;
                    string sourcepath = txtPath.Text;
                    string destinationpath = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\Closed Opportunities\Lost or Dead for other Reasons\Q" + ItemTxtBox.Text + "_" + customer + "_" + jobdescription;
                    Movefoldertoscrap(sourcepath, destinationpath);
                    webBrowser1.Url = new Uri(destinationpath);
                    txtPath.Text = destinationpath;
                    Perfromsavebttn(false);
                    MetroFramework.MetroMessageBox.Show(this, "Lost opportunity. Folders moved to new location.", "SPM Connect - Lost Quote", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                closedchkbox.Checked = false;
            }
        }

        private void Createfolders(string quotenumber)
        {
            Regex reg = new Regex("[?<>|:*'\"/,_&#^@]");
            string jobdescription = reg.Replace(Descriptiontxtbox.Text, "");
            jobdescription = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(jobdescription.ToLower());

            string customer = reg.Replace(familycombobox.Text, "");
            //customer = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(customer.ToLower());

            string destpaths300 = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\Q" + quotenumber + "_" + customer + "_" + jobdescription;
            const string sourcepaths300 = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\#### Sample Opportunity";

            if (!Directory.Exists(destpaths300))
            {
                DirectoryCopy(sourcepaths300, destpaths300, true);
                txtPath.Text = destpaths300 + @"\";
            }
        }

        private void Createnewitemtosql(string quote, string description, string customer, string category, string rating, string howfound, string quotedprice, string currency, string closed, string convertedtojob, string notes, string user)
        {
            DateTime dateedited = DateTime.Now;
            string sqlFormattedDate = dateedited.ToString("yyyy-MM-dd HH:mm:ss");
            if (connectapi.cn.State == ConnectionState.Closed)
                connectapi.cn.Open();
            try
            {
                SqlCommand cmd = connectapi.cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE [SPM_Database].[dbo].[Opportunities] SET Title = '" + description + "',Company_Name = '" + customer + "',Category = '" + category + "',Rating = '" + rating + "',How_Found = '" + howfound + "',Est_Revenue = '" + quotedprice + "',Currency = '" + currency + "',Closed = '" + closed + "',Converted_to_Job = '" + convertedtojob + "',Comments = '" + notes + "',LastSavedby = '" + user + "',LastSaved = @value2,FolderPath = '" + txtPath.Text + "' WHERE Quote = '" + quote + "' ";

                cmd.Parameters.AddWithValue("@value2", sqlFormattedDate);
                cmd.ExecuteNonQuery();
                connectapi.cn.Close();
                //MessageBox.Show("Item sucessfully saved SPM Connect Server.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void Cvttojobchkbox_Click(object sender, EventArgs e)
        {
            DialogResult result = MetroFramework.MetroMessageBox.Show(this, "Are you sure want to make this choice?", "SPM Connect?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (cvttojobchkbox.Checked)
                {
                    cvttojobchkbox.Checked = true;
                    closedchkbox.Checked = false;
                    closedchkbox.Visible = false;
                    cvttojobchkbox.Enabled = false;
                    Regex reg = new Regex("[*'\"/,_&#^@]");
                    string jobdescription = reg.Replace(Descriptiontxtbox.Text, "");
                    jobdescription = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(jobdescription.ToLower());
                    string customer = reg.Replace(familycombobox.Text, "");
                    //customer = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(customer.ToLower());

                    //string sourcepath = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\Q" + ItemTxtBox.Text + "_" + customer + "_" + jobdescription;
                    string sourcepath = txtPath.Text;
                    string destinationpath = @"\\spm-adfs\SPM\S300 Sales and Project Management\Sales\Opportunities\Closed Opportunities\Transfered_to_Jobs\Q" + ItemTxtBox.Text + "_" + customer + "_" + jobdescription;
                    Movefoldertoscrap(sourcepath, destinationpath);
                    webBrowser1.Url = new Uri(destinationpath);
                    txtPath.Text = destinationpath;
                    Perfromsavebttn(false);
                    MetroFramework.MetroMessageBox.Show(this, "Quote converted to job and folders are moved to new location.", "SPM Connect - Move to Job", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                cvttojobchkbox.Checked = false;
            }
        }

        private void Editbttn_Click(object sender, EventArgs e)
        {
            Processeditbutton();
        }

        private void Familycombobox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                //familycombobox = this.familycombobox.Text;
                familycombobox.Focus();
            }
        }

        private void Fillcustomers()
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT * FROM [SPM_Database].[dbo].[Customersmerged] ORDER BY Customers", connectapi.cn))
            {
                try
                {
                    if (connectapi.cn.State == ConnectionState.Closed)
                        connectapi.cn.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    AutoCompleteStringCollection MyCollection = new AutoCompleteStringCollection();
                    while (reader.Read())
                    {
                        MyCollection.Add(reader.GetString(0));
                    }

                    //familytxtbox.AutoCompleteCustomSource = MyCollection;
                    familycombobox.AutoCompleteCustomSource = MyCollection;
                    familycombobox.DataSource = MyCollection;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SPM Connect New Item - Fill Customers Source", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connectapi.cn.Close();
                }
            }
            familycombobox.SelectedItem = null;
        }

        private void Filldatatable(string quotenumber)
        {
            String sql = "SELECT *  FROM [SPM_Database].[dbo].[Opportunities] WHERE [Quote]='" + quotenumber + "'";
            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();
                SqlDataAdapter _adapter = new SqlDataAdapter(sql, connectapi.cn);
                dt.Clear();
                _adapter.Fill(dt);
                Fillinfo();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void Fillinfo()
        {
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                ItemTxtBox.Text = r["Quote"].ToString();
                Descriptiontxtbox.Text = r["Title"].ToString();
                notestxt.Text = r["Comments"].ToString();
                textBox1.Text = r["Est_Revenue"].ToString();
                txtPath.Text = r["FolderPath"].ToString();
                if (txtPath.Text.Length > 0)
                {
                    webBrowser1.Url = new Uri(txtPath.Text);
                }

                string quoteddate = r["Quote_Date"].ToString();
                string quotedby = r["Employee"].ToString();
                string lastsavedby = r["LastSavedby"].ToString();
                string lastsaved = r["Lastsaved"].ToString();
                if (quoteddate.Length > 0)
                {
                    quotedatelbl.Text = "Quote Date : " + quoteddate.Substring(0, 10);
                    quotedatelbl.Text = "Quote Date : " + quoteddate.Substring(0, 10);
                }

                Quotedbylbl.Text = "Quoted By : " + quotedby;
                Lsatsavedbylbl.Text = "Last Saved By : " + lastsavedby;
                Lastsavedlbl.Text = "Last Saved : " + lastsaved;

                string category = r["Category"].ToString();
                if (category.Length > 0)
                {
                    Categorycombox.SelectedItem = category;
                }

                string customer = r["Company_Name"].ToString();
                if (customer.Length > 0)
                {
                    familycombobox.SelectedItem = customer;
                }

                string rating = r["Rating"].ToString();
                if (rating.Length > 0)
                {
                    Ratingcombobox.SelectedItem = rating;
                }

                string howfound = r["How_Found"].ToString();
                if (howfound.Length > 0)
                {
                    Howfndcombox.SelectedItem = howfound;
                }

                string currency = r["Currency"].ToString();
                if (currency.Length > 0)
                {
                    currencycombox.SelectedItem = currency;
                }

                //if (r["Converted_to_Job"].ToString().Equals("1"))
                //{
                //    cvttojobchkbox.Checked = true;

                //}

                //if (r["Closed"].ToString().Equals("1"))
                //{
                //    closedchkbox.Checked = true;

                //}

                if (r["Converted_to_Job"].ToString().Equals("1") && r["Closed"].ToString().Equals("0"))
                {
                    cvttojobchkbox.Checked = true;
                    cvttojobchkbox.Enabled = false;
                    closedchkbox.Visible = false;
                }
                else if (r["Closed"].ToString().Equals("1") && r["Converted_to_Job"].ToString().Equals("0"))
                {
                    closedchkbox.Enabled = false;
                    closedchkbox.Checked = true;
                    cvttojobchkbox.Visible = false;
                }
            }
        }

        private void Forwardbttn_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
                webBrowser1.GoForward();
        }

        private void Graballinfor()
        {
            list.Clear();
            string quote = ItemTxtBox.Text;
            string description = Descriptiontxtbox.Text;
            string customer = familycombobox.Text;
            string category = Categorycombox.Text;
            string rating = Ratingcombobox.Text;
            string howfound = Howfndcombox.Text;
            string quotedprice = textBox1.Text;
            string currency = currencycombox.Text;
            string opportunityclosed = closedchkbox.Checked ? "1" : "0";
            string convertedtojob = cvttojobchkbox.Checked ? "1" : "0";
            string notes = notestxt.Text;
            list.Add(quote);
            list.Add(description);
            list.Add(customer);
            list.Add(category);
            list.Add(rating);
            list.Add(howfound);
            list.Add(quotedprice);
            list.Add(currency);
            list.Add(opportunityclosed);
            list.Add(convertedtojob);
            list.Add(notes);

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Replace("'", "''");
            }
        }

        private void HowFound()
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT [How Found] FROM [SPM_Database].[dbo].[QuoteFilters]  where [How Found] is not null ", connectapi.cn))
            {
                try
                {
                    if (connectapi.cn.State == ConnectionState.Closed)
                        connectapi.cn.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    AutoCompleteStringCollection MyCollection = new AutoCompleteStringCollection();
                    while (reader.Read())
                    {
                        MyCollection.Add(reader.GetString(0));
                    }

                    //familytxtbox.AutoCompleteCustomSource = MyCollection;
                    Howfndcombox.AutoCompleteCustomSource = MyCollection;
                    Howfndcombox.DataSource = MyCollection;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SPM Connect New Item - Fill howfound Source", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connectapi.cn.Close();
                }
            }
        }

        private void Movefoldertoscrap(string sourcefile, string destfile)
        {
            try
            {
                Directory.Move(sourcefile, destfile);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "SPM Connect - Move to Lost Opportunity", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Perfromlockdown()
        {
            editbttn.Visible = true;
            button3.Visible = false;
            savbttn.Enabled = false;
            savbttn.Visible = false;
            familycombobox.Enabled = false;
            Descriptiontxtbox.Enabled = false;
            Categorycombox.Enabled = false;
            Ratingcombobox.Enabled = false;
            textBox1.Enabled = false;
            Howfndcombox.Enabled = false;
            currencycombox.Enabled = false;
            closedchkbox.Enabled = false;
            cvttojobchkbox.Enabled = false;
            notestxt.Enabled = false;
        }

        private void Perfromsavebttn(bool createfolder)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.Enabled = false;
            Perfromlockdown();
            Graballinfor();
            if (createfolder)
            {
                Createfolders(ItemTxtBox.Text);
            }
            Createnewitemtosql(list[0], list[1], list[2], list[3], list[4], list[5], list[6], list[7], list[8], list[9], list[10], connectapi.ConnectUser.Name);

            this.Enabled = true;
            Cursor.Current = Cursors.Default;
            Filldatatable(list[0]);
        }

        private void Processeditbutton()
        {
            editbttn.Visible = false;
            button3.Visible = true;
            savbttn.Enabled = true;
            savbttn.Visible = true;
            familycombobox.Enabled = true;
            Descriptiontxtbox.Enabled = true;
            Categorycombox.Enabled = true;
            Ratingcombobox.Enabled = true;
            textBox1.Enabled = true;
            Howfndcombox.Enabled = true;
            currencycombox.Enabled = true;
            notestxt.Enabled = true;

            DataRow r = dt.Rows[0];
            if (r["Converted_to_Job"].ToString().Equals("1") && r["Closed"].ToString().Equals("0"))
            {
                cvttojobchkbox.Checked = true;
                cvttojobchkbox.Enabled = false;
                closedchkbox.Visible = false;
            }
            else if (r["Closed"].ToString().Equals("1") && r["Converted_to_Job"].ToString().Equals("0"))
            {
                closedchkbox.Enabled = false;
                closedchkbox.Checked = true;
                cvttojobchkbox.Visible = false;
            }
            else
            {
                cvttojobchkbox.Enabled = true;
                closedchkbox.Visible = true;
                closedchkbox.Enabled = true;
                cvttojobchkbox.Visible = true;
            }
        }

        private void QuoteDetails_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed Quote Details " + quoteno2 + " ");
            this.Dispose();
        }

        private void QuoteDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (savbttn.Visible)
            {
                DialogResult result = MetroFramework.MetroMessageBox.Show(this, "Are you sure want to close without saving changes?", "SPM Connect", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = result == DialogResult.No;
                }
            }
        }

        private void QuoteDetails_Load(object sender, EventArgs e)
        {
            // Suspend the layout logic for the form, while the application is initializing
            this.SuspendLayout();
            this.Text = "Quote Details - " + quoteno2;
            Fillcustomers();
            HowFound();
            Rating();
            Category();
            Filldatatable(quoteno2);
            Processeditbutton();
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened Quote Details " + quoteno2 + " ");
            if (dt.Rows.Count <= 0)
                this.Close();
            // Resume the layout logic
            this.ResumeLayout();
        }

        private void Rating()
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT [Rating] FROM [SPM_Database].[dbo].[QuoteFilters]  WHERE [Rating] is not null ", connectapi.cn))
            {
                try
                {
                    if (connectapi.cn.State == ConnectionState.Closed)
                        connectapi.cn.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    AutoCompleteStringCollection MyCollection = new AutoCompleteStringCollection();
                    while (reader.Read())
                    {
                        MyCollection.Add(reader.GetString(0));
                    }

                    //familytxtbox.AutoCompleteCustomSource = MyCollection;
                    Ratingcombobox.AutoCompleteCustomSource = MyCollection;
                    Ratingcombobox.DataSource = MyCollection;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SPM Connect New Item - Fill Rating Source", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connectapi.cn.Close();
                }
            }
        }

        private void Savbttn_Click(object sender, EventArgs e)
        {
            Perfromsavebttn(true);
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            //Remove previous formatting, or the decimal check will fail including leading zeros
            string value = textBox1.Text.Replace(",", "")
                .Replace("$", "").Replace(".", "").TrimStart('0');
            //Check we are indeed handling a number
            if (decimal.TryParse(value, out decimal ul))
            {
                ul /= 100;
                //Unsub the event so we don't enter a loop
                textBox1.TextChanged -= TextBox1_TextChanged;
                //Format the text as currency
                textBox1.Text = string.Format(CultureInfo.CreateSpecificCulture("en-US"), "{0:C2}", ul);
                textBox1.TextChanged += TextBox1_TextChanged;
                textBox1.Select(textBox1.Text.Length, 0);
            }
            bool goodToGo = TextisValid(textBox1.Text);
        }

        private bool TextisValid(string text)
        {
            Regex money = new Regex(@"^\$(\d{1,3}(\,\d{3})*|(\d+))(\.\d{2})?$");
            return money.IsMatch(text);
        }
    }
}