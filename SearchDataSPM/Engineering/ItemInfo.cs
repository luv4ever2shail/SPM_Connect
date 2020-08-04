﻿using SPMConnectAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SearchDataSPM.Engineering
{
    public partial class ItemInfo : Form
    {
        #region steupvariables

        private readonly DataTable _treeTB;
        private readonly SPMConnectAPI.SPMSQLCommands connectapi = new SPMConnectAPI.SPMSQLCommands();
        private readonly DataTable dt = new DataTable();
        private readonly string iteminfo2;
        private readonly DataTable PO;
        private readonly int PW;
        private readonly TreeNode root = new TreeNode();
        private bool hiden;
        private log4net.ILog log;
        private bool rootnodedone;

        #endregion steupvariables

        public ItemInfo(string itemno = "")
        {
            InitializeComponent();
            PW = 515;
            hiden = true;
            SlidePanel.Width = 0;
            _treeTB = new DataTable();
            PO = new DataTable();

            this.iteminfo2 = itemno;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.W))
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Addbutton_Click(object sender, EventArgs e)
        {
            try
            {
                errorProvider1.Clear();
                if (qtytxtbox.Text.Length > 0 && qtytxtbox.Text != "0")
                {
                    Shipping connectapi = new Shipping();
                    DataTable iteminfo = new DataTable();
                    iteminfo.Clear();
                    iteminfo = connectapi.GetIteminfo(itemnotxt.Text);
                    DataRow r = iteminfo.Rows[0];

                    string _itemno = itemnotxt.Text;
                    string _description = r["Description"].ToString();
                    string _family = r["FamilyCode"].ToString();
                    string _oem = r["Manufacturer"].ToString();
                    string _manufacturer = r["ManufacturerItemNumber"].ToString();
                    string qty = qtytxtbox.Text;

                    SearchNodes(_itemno, treeView1.Nodes[0]);
                    if (itemexists != "yes")
                    {
                        TreeNode child = new TreeNode
                        {
                            Text = _itemno + " - " + _description + " (" + qty + ")",
                            Tag = _itemno + "][" + _description + "][" + _family + "][" + _manufacturer + "][" + _oem + "][" + qty
                        };

                        treeView1.SelectedNode = treeView1.Nodes[0];
                        root.Nodes.Add(child);
                        CallRecursive();
                        if (!root.IsExpanded)
                        {
                            treeView1.ExpandAll();
                        }
                        savbttn.Visible = true;
                        itemnotxt.Clear();
                    }
                    else
                    {
                        savbttn.Visible = false;
                        itemexists = null;
                        itemnotxt.Clear();
                    }
                }
                else
                {
                    errorProvider1.SetError(qtytxtbox, "Cannot be null");
                }
            }
            catch
            {
                return;
            }
        }

        private void AddItems()
        {
            // Print each node recursively.
            foreach (TreeNode n in treeView1.Nodes)
            {
                SaveRecursive(n);
            }

            Lockdownsave();
        }

        private void AddItemToAssy(string itemno, string qty, string assyno)
        {
            DateTime datecreated = DateTime.Now;
            string sqlFormattedDatetime = datecreated.ToString("yyyy-MM-dd HH:mm:ss");
            string username = connectapi.ConnectUser.Name;

            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();
                SqlCommand cmd = connectapi.cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO [SPM_Database].[dbo].[ItemDependencies] ([AssyNo],[ItemNo],[Qty]," +
                    "DateCreated, CreatedBy, LastSaved, LastUser)" +
                    " VALUES('" + assyno + "','" + itemno + "','" + qty + "','" + sqlFormattedDatetime + "','" + username + "','" + sqlFormattedDatetime + "','" + username + "')";
                cmd.ExecuteNonQuery();
                connectapi.cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SPM Connect - Error on Add item to dependency", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void Addnewbttn_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (salepricetext.Text.Length > 0 && qtytxt.Text.Length > 0)
            {
                string itemnumber = ItemTxtBox.Text;
                string userfullname = connectapi.ConnectUser.Name;
                Createnewentry(itemnumber, userfullname);
                Clearalltextboxes();
                Showitemstogridview(itemnumber);
            }
            else
            {
                errorProvider1.SetError(costpricetxt, "Cannot be null");
                errorProvider1.SetError(salepricetext, "Cannot be null");
                MessageBox.Show("SalesPrice and quantity cannot be empty in order to add new entry.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Text = !hiden ? "S\nH\nO\nW\nA\nD\nV\nA\nN\nC\nE\nO\nP\nT\nI\nO\nN\nS" : "H\nI\nD\nE\nA\nD\nV\nA\nN\nC\nE\nO\nP\nT\nI\nO\nN\nS";
            timer1.Start();
        }

        private void CallRecursive()
        {
            // Print each node recursively.
            foreach (TreeNode n in treeView1.Nodes)
            {
                if (n.Nodes.Count > 0)
                {
                    PrintRecursive(n);
                }
            }
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            updatebttn.Visible = false;
            addbutton.Visible = true;
            savbttn.Visible = true;
            cancelbutton.Visible = false;
            itemnotxt.Clear();
            Addremovecontextmenu.Enabled = true;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.Size = new Size(970, 725);
                dataGridView2.Visible = true;
            }
            else
            {
                this.Size = new Size(970, 570);
                dataGridView2.Visible = false;
            }
        }

        private void Checkfordata(string itemnumber)
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) FROM [SPM_Database].[dbo].[Inventory] WHERE [ItemNumber]='" + itemnumber + "'", connectapi.cn))
            {
                try
                {
                    if (connectapi.cn.State == ConnectionState.Closed)
                        connectapi.cn.Open();

                    int userCount = (int)sqlCommand.ExecuteScalar();
                    if (userCount <= 0)
                    {
                        connectapi.Addcpoieditemtosqltablefromgenius(iteminfo2, iteminfo2);
                    }
                    connectapi.cn.Close();
                    Filldatatable(iteminfo2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connectapi.cn.Close();
                }
            }
        }

        private void CheckItemDependenciesRight()
        {
            if (connectapi.ConnectUser.ItemDependencies)
            {
                itemupdatetools.Visible = true;
                itemupdatetools.Enabled = true;
                treeView1.ContextMenuStrip = Addremovecontextmenu;
                treeView1.Size = new Size(413, 365);
                treeView1.AllowDrop = true;
            }
            else
            {
                itemupdatetools.Visible = false;
                itemupdatetools.Enabled = false;
                treeView1.ContextMenuStrip = null;
                treeView1.Size = new Size(treeView1.Size.Width, 502);
                treeView1.AllowDrop = false;
            }
        }

        private void CheckManagement()
        {
            if (connectapi.ConnectUser.Management)
            {
                panel1.Visible = true;
                groupBox2.Size = new Size(406, 524);
            }
            else
            {
                groupBox2.Size = new Size(427, 524);
                panel1.Visible = false;
            }
        }

        private void CheckPriceRights()
        {
            if (connectapi.ConnectUser.PriceRight)
            {
                LastCostLbl.Visible = true;
                AvgCostLbl.Visible = true;
                checkBox1.Visible = true;
                locationsList.Size = new Size(170, 75);
            }
            else
            {
                LastCostLbl.Visible = false;
                AvgCostLbl.Visible = false;
                checkBox1.Visible = false;
                locationsList.Size = new Size(170, 130);

            }
        }

        private void Clearalltextboxes()
        {
            costpricetxt.Clear();
            salepricetext.Clear();
            qtytxt.Clear();
            NotesCosttxt.Clear();
        }

        private void Costpricetxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9\.+\b]"))
            {
                // Stop the character from being entered into the control since it is illegal.
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Costpricetxt_Leave(object sender, EventArgs e)
        {
            if (Double.TryParse(costpricetxt.Text, NumberStyles.Currency, null, out double amount))
            {
                costpricetxt.Text = amount.ToString("C");
            }
        }

        private void Createnewentry(string itemnumber, string user)
        {
            DateTime datecreated = DateTime.Now;
            string sqlFormattedDate = datecreated.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (connectapi.cn.State == ConnectionState.Closed)
                connectapi.cn.Open();
            try
            {
                SqlCommand cmd = connectapi.cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO [SPM_Database].[dbo].[Item_CostRollup] (ItemId, Cost, SalesPrice, Qty, Notes, Date, LastSavedBy) VALUES('" + itemnumber + "','" + costpricetxt.Text + "','" + salepricetext.Text + "','" + qtytxt.Text + "','" + NotesCosttxt.Text + "','" + sqlFormattedDate + "','" + user + "')";
                cmd.ExecuteNonQuery();
                connectapi.cn.Close();
                //MessageBox.Show("New entry created", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void Deleteassy(string assyitem)
        {
            string sql = "DELETE FROM [SPM_Database].[dbo].[ItemDependencies] WHERE [AssyNo] = '" + assyitem + "' ";

            try
            {
                connectapi.cn.Open();
                SqlCommand _command = connectapi.cn.CreateCommand();
                _command.CommandText = sql;
                _command.ExecuteNonQuery();
                // MessageBox.Show("Item added to the catalog.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException)
            {
                // MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Technical error while updating to autocad catalog. Please contact the admin.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void Fetchitemdetails(string itemno)
        {
            if (itemno.Length == 6)
            {
                Fillselectediteminfo(itemno);
            }
            else
            {
                qtytxtbox.Clear();
                itmdeslbl.Text = "Description : ";
                itmoemlbl.Text = "Manufacturer : ";
                itemoemitmlbl.Text = "OEM Item No : ";
            }
        }

        private void Filldatatable(string itemnumber)
        {
            DataTable dt = connectapi.ShowIventoryItems(itemnumber);
            if (dt.Rows.Count > 0)
            {
                Fillinfo(dt.Rows[0]);
                foreach (DataRow dr in dt.Rows)
                {
                    locationsList.Text += dr["Location"].ToString() + Environment.NewLine;
                }
            }
        }

        private void Filldatatabletreeview()
        {
            //string sql = "SELECT *  FROM [SPM_Database].[dbo].[ItemDependencies]";
            const string sql = "SELECT *  FROM [SPM_Database].[dbo].[ItemsDependenciesBOM]";
            try
            {
                _treeTB.Clear();
                connectapi.cn.Open();
                SqlDataAdapter _adapter = new SqlDataAdapter(sql, connectapi.cn);
                _adapter.Fill(_treeTB);
                connectapi.cn.Close();
                Fillrootnode();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Fillinfo(DataRow r)
        {
            if (!r.IsNull(0))
            {
                ItemTxtBox.Text = r["ItemNumber"].ToString();
                Descriptiontxtbox.Text = r["Description"].ToString();
                oemtxtbox.Text = r["Manufacturer"].ToString();
                oemitemtxtbox.Text = r["ManufacturerItemNumber"].ToString();
                familytxtbox.Text = r["FamilyCode"].ToString();
                sparetxtbox.Text = r["Spare"].ToString();
                mattxt.Text = r["Material"].ToString();
                designbytxt.Text = r["DesignedBy"].ToString();
                categorytxtbox.Text = r["FamilyType"].ToString();
                surfacetxt.Text = r["SurfaceProtection"].ToString();
                heattreat.Text = r["HeatTreatment"].ToString();
                datecreatedtxt.Text = r["DateCreated"].ToString();
                dateeditxt.Text = r["LastEdited"].ToString();
                Lastsavedtxtbox.Text = r["LastSavedBy"].ToString();
                notestxt.Text = r["Notes"].ToString();
                OnHandLbl.Text = "On Hand: " + r["OnHand"].ToString();
                ReservedLbl.Text = "Reserved: " + r["Reserved"].ToString();
                AvailableLbl.Text = "Available: " + r["Available"].ToString();
                ProductionLbl.Text = "In Production: " + r["InProduction"].ToString();
                OnOrderLbl.Text = "On Order: " + r["OnOrder"].ToString();
                NetAvailableLbl.Text = "Net Available: " + r["NetQtyAvailable"].ToString();
                //LocationLbl.Text = "Location: " + r["Location"].ToString();
                LastCostLbl.Text = "Last Cost: $" + r["LastCost"].ToString();
                AvgCostLbl.Text = "Average Cost: $" + r["AverageCost"].ToString();
            }
        }

        private void Fillrootnode()
        {
            try
            {
                treeView1.Nodes.Clear();
                RemoveChildNodes(root);
                treeView1.ResetText();

                root.Text = iteminfo2 + " - " + Descriptiontxtbox.Text;
                root.Tag = iteminfo2 + "][" + Descriptiontxtbox.Text + "][" + familytxtbox.Text + "][" + oemtxtbox.Text + "][" + oemitemtxtbox.Text + "][1";
                Setimageaccordingtofamily(familytxtbox.Text, root);

                treeView1.Nodes.Add(root);

                PopulateTreeView(iteminfo2, root);
            }
            catch
            {
                treeView1.TopNode.Nodes.Clear();
                treeView1.Nodes.Clear();
                RemoveChildNodes(root);
                treeView1.ResetText();
            }
        }

        private void Fillselectediteminfo(string item)
        {
            Shipping connectapi = new Shipping();
            DataTable iteminfo = new DataTable();
            iteminfo.Clear();
            iteminfo = connectapi.GetIteminfo(item);
            if (iteminfo.Rows.Count > 0)
            {
                DataRow r = iteminfo.Rows[0];
                itmdeslbl.Text = "Description : " + r["Description"].ToString();
                itmoemlbl.Text = "Manufacturer : " + r["Manufacturer"].ToString();
                itemoemitmlbl.Text = "OEM Item No : " + r["ManufacturerItemNumber"].ToString();
            }
        }

        private void ItemInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Closed Item Details " + iteminfo2 + " ");
            this.Dispose();
        }

        private void ItemInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (savbttn.Visible && itemupdatetools.Visible)
            {
                errorProvider1.SetError(savbttn, "Save before closing");
                DialogResult result = MetroFramework.MetroMessageBox.Show(this, "Do you want to save changes?", "SPM Connect", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    ProcessSaveButton();
                }
                else
                {
                }
            }
        }

        private void Itemnotxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (sender as TextBox)?.SelectionStart == 0 && e.KeyChar == (char)Keys.Space;
        }

        private void Itemnotxt_TextChanged(object sender, EventArgs e)
        {
            if (itemnotxt.Text.Length == 6 && Char.IsLetter(itemnotxt.Text[0]))
            {
                Fetchitemdetails(itemnotxt.Text.Trim());
            }
            else
            {
                qtytxtbox.Clear();
                itmdeslbl.Text = "Description : ";
                itmoemlbl.Text = "Manufacturer : ";
                itemoemitmlbl.Text = "OEM Item No : ";
            }
        }

        private void Lockdownsave()
        {
            treeView1.SelectedNode = root;
            savbttn.Visible = false;
            treeView1.ContextMenuStrip = null;
            MessageBox.Show("Assembly updated successfully.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            addbutton.Visible = true;
        }

        private void ParentView_Load(object sender, EventArgs e)
        {
            // Suspend the layout logic for the form, while the application is initializing
            this.SuspendLayout();
            this.Text = "ItemInfo - SPM Connect (" + iteminfo2 + ")";
            CheckManagement();
            CheckItemDependenciesRight();
            CheckPriceRights();
            if (connectapi.ConnectUser.Management)
            {
                Showitemstogridview(iteminfo2);
            }
            if (connectapi.ConnectUser.PriceRight)
            {
                SHOWITEMPRICE(iteminfo2);
            }

            Checkfordata(iteminfo2);
            Startprocessfortreeview();
            CallRecursive();
            treeView1.ExpandAll();
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Opened Item Details " + iteminfo2 + " ");
            // Resume the layout logic
            this.ResumeLayout();
        }

        private void PopulateTreeView(string parentId, TreeNode parentNode)
        {
            TreeNode childNode;

            foreach (DataRow dr in _treeTB.Select("[AssyNo] ='" + parentId + "'"))
            {
                TreeNode t = new TreeNode
                {
                    Text = dr["ItemNumber"].ToString() + " - " + dr["Description"].ToString() + " (" + dr["QuantityPerAssembly"].ToString() + ") ",
                    Name = dr["ItemNumber"].ToString(),
                    Tag = dr["ItemNumber"].ToString() + "][" + dr["Description"].ToString() + "][" + dr["ItemFamily"].ToString() + "][" + dr["Manufacturer"].ToString() + "][" + dr["ManufacturerItemNumber"].ToString() + "][" + dr["QuantityPerAssembly"].ToString()
                };
                if (parentNode == null)
                {
                    t.NodeFont = new Font("Arial", 10, FontStyle.Bold);
                    t.Text = dr["AssyNo"].ToString() + " - " + dr["AssyDescription"].ToString() + " (" + dr["QuantityPerAssembly"].ToString() + ") ";
                    t.Name = dr["AssyNo"].ToString();
                    t.Tag = dr["AssyNo"].ToString() + "][" + dr["AssyDescription"].ToString() + "][" + dr["AssyFamily"].ToString() + "][" + dr["AssyManufacturer"].ToString() + "][" + dr["AssyManufacturerItemNumber"].ToString() + "][1";

                    treeView1.Nodes.Add(t);
                    childNode = t;
                }
                else
                {
                    Font f = new Font("Arial", 10, FontStyle.Bold);
                    // t.NodeFont = f;
                    parentNode.Nodes.Add(t);
                    childNode = t;
                }
                //MessageBox.Show((dr["ItemNumber"].ToString()) + "Parent " + (dr["AssyNo"].ToString()));
                //PopulateTreeView((dr["ItemNumber"].ToString()), childNode);
            }
            // treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private void PrintRecursive(TreeNode treeNode)
        {
            // Print the node.
            if (treeNode.Nodes.Count == 0)
            {
            }
            if (treeNode.Index == 0 && !rootnodedone)
            {
                rootnodedone = true;
            }
            else
            {
                string s = treeNode.Tag.ToString();
                string[] values = s.Replace("][", "~").Split('~');
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].Trim();
                }
                //DataRow r = _treeTB.Rows[int.Parse(treeNode.Tag.ToString())];
                //string family = r["ItemFamily"].ToString();
                string family = values[2];
                Setimageaccordingtofamily(family, treeNode);
            }

            // Print each node recursively.
            foreach (TreeNode tn in treeNode.Nodes)
            {
                PrintRecursive(tn);
            }
        }

        private void ProcessSaveButton()
        {
            treeView1.SelectedNode = treeView1.Nodes[0];
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Parent == null)
            {
                treeView1.PathSeparator = ".";

                // Get the count of the child tree nodes contained in the SelectedNode.
                int myNodeCount = treeView1.SelectedNode.GetNodeCount(true);
                //MessageBox.Show(myNodeCount.ToString());
                _ = (decimal)myNodeCount /
                  (decimal)treeView1.GetNodeCount(true) * 100;

                if (myNodeCount > 0)
                {
                    Deleteassy(ItemTxtBox.Text);
                    AddItems();
                }
                else
                {
                    Deleteassy(ItemTxtBox.Text);
                    Lockdownsave();
                    //MessageBox.Show("Assembly list cannot be empty in order to save to AutoCad Catalog.", "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void Qtytxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9+\b]"))
            {
                // Stop the character from being entered into the control since it is illegal.
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Qtytxtbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Qtytxtbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9+\b]"))
            {
                // Stop the character from being entered into the control since it is illegal.
            }
            else
            {
                e.Handled = true;
            }
        }

        private void RemoveChildNodes(TreeNode parentNode)
        {
            if (parentNode.Nodes.Count > 0)
            {
                for (int i = parentNode.Nodes.Count - 1; i >= 0; i--)
                {
                    parentNode.Nodes[i].Remove();
                }
            }
        }

        private void RemoveItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    // treeView1.Nodes.Remove(treeView1.SelectedNode);
                }
                else
                {
                    DialogResult result = MessageBox.Show("Remove item from the dependency list?", "SPM Connect", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        treeView1.SelectedNode.Parent.Nodes.Remove(treeView1.SelectedNode);
                        savbttn.Visible = true;
                    }
                    else if (result == DialogResult.No)
                    {
                        //code for No
                    }
                }
            }
        }

        private void Salepricetext_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[0-9\.+\b]"))
            {
                // Stop the character from being entered into the control since it is illegal.
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Salepricetext_Leave(object sender, EventArgs e)
        {
            if (Double.TryParse(salepricetext.Text, NumberStyles.Currency, null, out double amount))
            {
                salepricetext.Text = amount.ToString("C");
            }

            if (salepricetext.Text.Length == 0)
            {
            }
        }

        private void Savbttn_Click(object sender, EventArgs e)
        {
            ProcessSaveButton();
        }

        private void SaveEachnode(TreeNode treeNode)
        {
            if (treeNode.Parent == null)
            {
                //string parentnode = treeNode.Tag.ToString();

                ////MessageBox.Show(parentnode);
                //Splittagtovariables(parentnode);
            }
            else
            {
                string childnode = treeNode.Tag.ToString();
                Splittagtovariables(childnode);
            }
        }

        private void SaveRecursive(TreeNode treeNode)
        {
            SaveEachnode(treeNode);
            foreach (TreeNode tn in treeNode.Nodes)
            {
                // MessageBox.Show(treeNode.Text);
                SaveRecursive(tn);
            }
        }

        private void Setimageaccordingtofamily(string family, TreeNode treeNode)
        {
            if (family == "AG")
            {
                treeNode.ImageIndex = 4;
            }
            else if (family == "JOB")
            {
                treeNode.ImageIndex = 12;
            }
            else if (family == "AS" || family == "ASPN")
            {
                treeNode.ImageIndex = 0;
            }
            else if (family == "ECC")
            {
                treeNode.ImageIndex = 3;
            }
            else if (family == "MPC" || family == "PU")
            {
                treeNode.ImageIndex = 5;
            }
            else if (family == "MA" || family == "MAWE")
            {
                treeNode.ImageIndex = 6;
            }
            else if (family == "FAHW")
            {
                treeNode.ImageIndex = 7;
            }
            else if (family == "ASEL")
            {
                treeNode.ImageIndex = 8;
            }
            else if (family == "PCC")
            {
                treeNode.ImageIndex = 9;
            }
            else if (family == "MT")
            {
                treeNode.ImageIndex = 10;
            }
            else
            {
                treeNode.ImageIndex = family == "DR" ? 11 : 2;
            }
        }

        private void SHOWITEMPRICE(string itemnumber)
        {
            String sql = "SELECT *  FROM [dbo].[PriceItemsFromPO]  WHERE Item = '" + itemnumber + "' ORDER BY PurchaseOrder DESC";
            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();

                SqlDataAdapter sda = new SqlDataAdapter(sql, connectapi.cn);

                PO.Clear();
                sda.Fill(PO);
                dataGridView2.DataSource = PO;
                dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView2.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView2.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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

        private void Showitemstogridview(string itemnumber)
        {
            string sql = "SELECT * FROM [SPM_Database].[dbo].[Item_CostRollup] WHERE ItemId ='" + itemnumber + "'";
            try
            {
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();

                SqlDataAdapter sda = new SqlDataAdapter(sql, connectapi.cn);

                dt.Clear();
                sda.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Sort(dataGridView1.Columns[5], ListSortDirection.Descending);
                //dataGridView1.Columns[8].Visible = false;
                dataGridView1.Columns[0].Visible = false;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].Width = 60;
                dataGridView1.Columns[2].Width = 80;
                dataGridView1.Columns[3].Width = 60;
                dataGridView1.Columns[4].Width = 200;
                dataGridView1.Columns[5].Width = 160;
                dataGridView1.Columns[6].Width = 160;
                UpdateFont();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SPM Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // MessageBox.Show("Data cannot be retrieved from server. Please contact the admin.", "SPM Connect - SQL SERVER ENG", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Application.Exit();
            }
            finally
            {
                connectapi.cn.Close();
            }

            //dataGridView.Location = new Point(0, 40);
        }

        private void Splittagtovariables(string s)
        {
            string[] values = s.Replace("][", "~").Split('~');

            //string[] values = s.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            AddItemToAssy(values[0], values[5], ItemTxtBox.Text);
        }

        private void SPM_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.spm-automation.com/");
        }

        private void Startprocessfortreeview()
        {
            try
            {
                treeView1.Nodes.Clear();
                RemoveChildNodes(root);
                treeView1.ResetText();
                Filldatatabletreeview();
                //CallRecursive();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (hiden)
            {
                SlidePanel.Width += 50;
                if (SlidePanel.Width >= PW)
                {
                    timer1.Stop();
                    hiden = false;
                    this.Refresh();
                }
            }
            else
            {
                SlidePanel.Width -= 50;
                if (SlidePanel.Width <= 0)
                {
                    timer1.Stop();
                    hiden = true;
                    this.Refresh();
                }
            }
        }

        private void TreeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            //e.Node.SelectedImageIndex = 0;
            // e.Node.ImageIndex = 0;
            if (e.Node.ImageIndex == 1)
            {
                e.Node.SelectedImageIndex = 0;
                e.Node.ImageIndex = 0;
            }
        }

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == 0)
            {
                e.Node.SelectedImageIndex = 1;
                e.Node.ImageIndex = 1;
            }
        }

        private void TreeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            switch (e.Node.ImageIndex)
            {
                case 1:
                    e.Node.SelectedImageIndex = 1;
                    e.Node.ImageIndex = 1;
                    break;

                case 2:
                    e.Node.SelectedImageIndex = 2;
                    e.Node.ImageIndex = 2;
                    break;

                case 3:
                    e.Node.SelectedImageIndex = 3;
                    e.Node.ImageIndex = 3;
                    break;

                case 4:
                    e.Node.SelectedImageIndex = 4;
                    e.Node.ImageIndex = 4;
                    break;

                case 5:
                    e.Node.SelectedImageIndex = 5;
                    e.Node.ImageIndex = 5;
                    break;

                case 6:
                    e.Node.SelectedImageIndex = 6;
                    e.Node.ImageIndex = 6;
                    break;

                case 7:
                    e.Node.SelectedImageIndex = 7;
                    e.Node.ImageIndex = 7;
                    break;

                case 8:
                    e.Node.SelectedImageIndex = 8;
                    e.Node.ImageIndex = 8;
                    break;

                case 9:
                    e.Node.SelectedImageIndex = 9;
                    e.Node.ImageIndex = 9;
                    break;

                case 10:
                    e.Node.SelectedImageIndex = 10;
                    e.Node.ImageIndex = 10;
                    break;

                case 11:
                    e.Node.SelectedImageIndex = 11;
                    e.Node.ImageIndex = 11;
                    break;

                case 12:
                    e.Node.SelectedImageIndex = 12;
                    e.Node.ImageIndex = 12;
                    break;

                default:
                    e.Node.SelectedImageIndex = 0;
                    e.Node.ImageIndex = 0;
                    break;
            }
        }

        private void TreeView1_DragDrop(object sender, DragEventArgs e)
        {
            DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
            string itemnumber = Convert.ToString(rowToMove.Cells[0].Value);
            string description = Convert.ToString(rowToMove.Cells[1].Value);
            string family = Convert.ToString(rowToMove.Cells[2].Value);
            string oem = Convert.ToString(rowToMove.Cells[3].Value);
            string oemitem = Convert.ToString(rowToMove.Cells[4].Value);

            SearchNodes(itemnumber, treeView1.Nodes[0]);

            if (itemexists != "yes")
            {
                TreeNode child = new TreeNode
                {
                    Text = itemnumber + " - " + description + " (1)",
                    Tag = itemnumber + "][" + description + "][" + family + "][" + oem + "][" + oemitem + "][1"
                };

                root.Nodes.Add(child);

                CallRecursive();
                if (!root.IsExpanded)
                {
                    treeView1.ExpandAll();
                }
                savbttn.Visible = true;
            }
            else
            {
                itemexists = null;
            }
        }

        private void TreeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void TreeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Down))
            {
                _ = new TreeNode();
                TreeNode node = treeView1.SelectedNode;
                treeView1.SelectedNode = node.NextVisibleNode;
                node.TreeView.Focus();
            }
            else if (e.KeyChar == Convert.ToChar(Keys.Up))
            {
                _ = new TreeNode();
                TreeNode node = treeView1.SelectedNode;
                treeView1.SelectedNode = node.NextVisibleNode;
                node.TreeView.Focus();
            }
        }

        private void TreeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (itemupdatetools.Visible)
            {
                if (!root.IsSelected)
                {
                    Addremovecontextmenu.Enabled = false;
                    addbutton.Visible = false;
                    savbttn.Visible = false;
                    updatebttn.Visible = true;
                    cancelbutton.Visible = true;
                    treeView1.AllowDrop = false;
                    string s = treeView1.SelectedNode.Tag.ToString();
                    string[] values = s.Replace("][", "~").Split('~');
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].Trim();
                    }

                    itemnotxt.Text = values[0];
                    qtytxtbox.Text = values[5];
                }
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        private void Updatebttn_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && !root.IsSelected)
            {
                if (qtytxtbox.Text == "0")
                {
                    qtytxtbox.Text = "1";
                }

                Shipping connectapi = new Shipping();
                DataTable iteminfo = new DataTable();
                iteminfo.Clear();
                iteminfo = connectapi.GetIteminfo(itemnotxt.Text);
                DataRow r = iteminfo.Rows[0];
                treeView1.SelectedNode.Text = itemnotxt.Text + " - " + r["Description"].ToString() + " (" + qtytxtbox.Text + ")";
                treeView1.SelectedNode.Tag = itemnotxt.Text + "][" + r["Description"].ToString() + "][" + r["FamilyCode"].ToString() + "][" + r["Manufacturer"].ToString() + "][" + r["ManufacturerItemNumber"].ToString() + "][" + qtytxtbox.Text;
                itemnotxt.Clear();
                savbttn.Visible = true;
                addbutton.Visible = true;
                updatebttn.Visible = false;
                cancelbutton.Visible = false;
                treeView1.AllowDrop = true;
                Addremovecontextmenu.Enabled = true;
            }
        }

        private void UpdateFont()
        {
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 8.5F, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(237, 237, 237);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Yellow;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Black;
        }

        #region search before addding item

        private readonly List<TreeNode> _CurrentNodeMatches = new List<TreeNode>();

        private string itemexists;

        private void SearchNodes(string SearchText, TreeNode StartNode)
        {
            //TreeNode node = null;
            while (StartNode != null)
            {
                if (StartNode.Text.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    MessageBox.Show("Item already added to the assembly list", "SPM Conect", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    itemexists = "yes";
                    treeView1.SelectedNode = StartNode;
                    _CurrentNodeMatches.Add(StartNode);
                }

                if (StartNode.Nodes.Count != 0)
                {
                    SearchNodes(SearchText, StartNode.Nodes[0]);//Recursive Search
                }
                StartNode = StartNode.NextNode;
            }
        }

        #endregion search before addding item

        private void InvGroupBox_Enter(object sender, EventArgs e)
        {
        }
    }
}