﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SearchDataSPM.General
{
    public partial class Compare : Form

    {
        #region steupvariables

        private readonly DataTable acountsTb;
        private readonly SPMConnectAPI.ConnectAPI connectapi = new SPMConnectAPI.ConnectAPI();
        private readonly TreeNode root = new TreeNode();
        private readonly TreeNode root2 = new TreeNode();
        private string txtvalue;

        #endregion steupvariables

        #region loadtree

        private string itemnumber;

        public Compare()
        {
            InitializeComponent();
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            int w = Width >= screen.Width ? screen.Width : (screen.Width + Width) / 3;
            int h = Height >= screen.Height ? screen.Height : (screen.Height + Height) / 3;
            this.Location = new Point((screen.Width - w) / 2, (screen.Height - h) / 2);
            this.Size = new Size(w, h);
            acountsTb = new DataTable();
        }

        public string item(string item)
        {
            if (item.Length > 0)
                return itemnumber = item;
            return null;
        }

        private void ParentView_Load(object sender, EventArgs e)
        {
            Assy_txtbox.Focus();
            Assy_txtbox.Text = itemnumber;

            if (Assy_txtbox.Text.Length == 5 || Assy_txtbox.Text.Length == 6)
            {
                //SendKeys.Send("~");
                itemnumber = null;
                startprocessofbom();
            }
        }

        #endregion loadtree

        #region assytextbox and button events

        private void Assy_txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                startprocessofbom();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Assy_txtbox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // treeView1.TopNode.Nodes.Clear();
            cleanup();
            //SendKeys.Send("~");
        }

        private void cleanup()
        {
            treeView1.Nodes.Clear();
            treeView1.ResetText();
            RemoveChildNodes(root);
            acountsTb.Clear();
            Assy_txtbox.Clear();
        }

        private void cleaup2()
        {
            treeView1.Nodes.Clear();
            treeView1.ResetText();
            RemoveChildNodes(root);
            acountsTb.Clear();
        }

        private void cleaup22()
        {
            treeView2.Nodes.Clear();
            treeView2.ResetText();
            RemoveChildNodes(root2);
            acountsTb.Clear();
        }

        private void filldatatable()
        {
            const string sql = "SELECT *  FROM [SPM_Database].[dbo].[SPMConnectBOM] ORDER BY [ItemNumber]";
            try
            {
                acountsTb.Clear();
                if (connectapi.cn.State == ConnectionState.Closed)
                    connectapi.cn.Open();
                SqlDataAdapter _adapter = new SqlDataAdapter(sql, connectapi.cn);

                _adapter.Fill(acountsTb);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "BOM - Fill data table Items", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connectapi.cn.Close();
            }
        }

        private void fillrootnode(string treeview)
        {
            //if (Assy_txtbox.Text.Length == 6)
            //{
            if (treeview == "1")
            {
                Assy_txtbox.BackColor = Color.White; //to add high light
                try
                {
                    try
                    {
                        treeView1.Nodes.Clear();
                        RemoveChildNodes(root);
                        treeView1.ResetText();

                        //DataRow[] dr = _productTB.Select("ItemNumber = '" + txtvalue.ToString() + "'");
                        DataRow[] dr = acountsTb.Select("AssyNo = '" + txtvalue + "'");

                        root.Text = dr[0]["AssyNo"].ToString() + " - " + dr[0]["AssyDescription"].ToString();
                        root.Tag = acountsTb.Rows.IndexOf(dr[0]);

                        //Font f = FontStyle.Bold);
                        // root.NodeFont = f;

                        treeView1.Nodes.Add(root);
                        //root.ImageIndex = 0;

                        PopulateTreeView(Assy_txtbox.Text, "1", root);

                        treeView1.SelectedNode = treeView1.Nodes[0];
                    }
                    catch
                    {
                        treeView1.Nodes.Clear();
                        RemoveChildNodes(root);
                        treeView1.ResetText();

                        //DataRow[] dr = _productTB.Select("ItemNumber = '" + txtvalue.ToString() + "'");
                        DataRow[] dr = acountsTb.Select("ItemNumber = '" + txtvalue + "'");

                        root.Text = dr[0]["ItemNumber"].ToString() + " - " + dr[0]["Description"].ToString();
                        root.Tag = acountsTb.Rows.IndexOf(dr[0]);

                        //Font f = FontStyle.Bold);
                        // root.NodeFont = f;

                        treeView1.Nodes.Add(root);

                        PopulateTreeView(Assy_txtbox.Text, "1", root);

                        treeView1.SelectedNode = treeView1.Nodes[0];
                    }
                }
                catch
                {
                    treeView1.TopNode.Nodes.Clear();
                    treeView1.Nodes.Clear();
                    RemoveChildNodes(root);
                    treeView1.ResetText();
                }
            }
            else
            {
                assytxt2.BackColor = Color.White; //to add high light
                try
                {
                    try
                    {
                        treeView2.Nodes.Clear();
                        RemoveChildNodes(root2);
                        treeView2.ResetText();

                        //DataRow[] dr = _productTB.Select("ItemNumber = '" + txtvalue.ToString() + "'");
                        DataRow[] dr = acountsTb.Select("AssyNo = '" + assytxt2.Text + "'");

                        root2.Text = dr[0]["AssyNo"].ToString() + " - " + dr[0]["AssyDescription"].ToString();
                        root2.Tag = acountsTb.Rows.IndexOf(dr[0]);

                        //Font f = FontStyle.Bold);
                        // root.NodeFont = f;

                        treeView2.Nodes.Add(root2);
                        //root.ImageIndex = 0;

                        PopulateTreeView(assytxt2.Text, "2", root2);

                        treeView2.SelectedNode = treeView2.Nodes[0];
                    }
                    catch
                    {
                        treeView2.Nodes.Clear();
                        RemoveChildNodes(root2);
                        treeView2.ResetText();

                        //DataRow[] dr = _productTB.Select("ItemNumber = '" + txtvalue.ToString() + "'");
                        DataRow[] dr = acountsTb.Select("ItemNumber = '" + assytxt2.Text + "'");

                        root2.Text = dr[0]["ItemNumber"].ToString() + " - " + dr[0]["Description"].ToString();
                        root2.Tag = acountsTb.Rows.IndexOf(dr[0]);

                        //Font f = FontStyle.Bold);
                        // root.NodeFont = f;

                        treeView2.Nodes.Add(root2);

                        PopulateTreeView(assytxt2.Text, "2", root2);

                        treeView2.SelectedNode = treeView2.Nodes[0];
                    }
                }
                catch
                {
                    treeView2.TopNode.Nodes.Clear();
                    treeView2.Nodes.Clear();
                    RemoveChildNodes(root2);
                    treeView2.ResetText();
                }
            }
        }

        private void PopulateTreeView(string parentId, string treeview, TreeNode parentNode)
        {
            if (treeview == "1")
            {
                TreeNode childNode;

                foreach (DataRow dr in acountsTb.Select("[AssyNo] ='" + parentId + "'"))
                {
                    TreeNode t = new TreeNode
                    {
                        Text = dr["ItemNumber"].ToString() + " - " + dr["Description"].ToString() + " ( " + dr["QuantityPerAssembly"].ToString() + " ) ",
                        Name = dr["ItemNumber"].ToString(),
                        Tag = acountsTb.Rows.IndexOf(dr)
                    };
                    if (parentNode == null)
                    {
                        Font f = new Font("Arial", 10, FontStyle.Bold);
                        t.NodeFont = f;
                        t.Text = dr["AssyNo"].ToString() + " - " + dr["AssyDescription"].ToString() + " ( " + dr["QuantityPerAssembly"].ToString() + " ) ";
                        t.Name = dr["ItemNumber"].ToString();
                        t.Tag = acountsTb.Rows.IndexOf(dr);
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
                    PopulateTreeView(dr["ItemNumber"].ToString(), "1", childNode);
                }
            }
            else
            {
                TreeNode childNode;

                foreach (DataRow dr in acountsTb.Select("[AssyNo] ='" + parentId + "'"))
                {
                    TreeNode t = new TreeNode
                    {
                        Text = dr["ItemNumber"].ToString() + " - " + dr["Description"].ToString() + " ( " + dr["QuantityPerAssembly"].ToString() + " ) ",
                        Name = dr["ItemNumber"].ToString(),
                        Tag = acountsTb.Rows.IndexOf(dr)
                    };
                    if (parentNode == null)
                    {
                        Font f = new Font("Arial", 10, FontStyle.Bold);
                        t.NodeFont = f;
                        t.Text = dr["AssyNo"].ToString() + " - " + dr["AssyDescription"].ToString() + " ( " + dr["QuantityPerAssembly"].ToString() + " ) ";
                        t.Name = dr["ItemNumber"].ToString();
                        t.Tag = acountsTb.Rows.IndexOf(dr);
                        treeView2.Nodes.Add(t);
                        childNode = t;
                    }
                    else
                    {
                        Font f = new Font("Arial", 10, FontStyle.Bold);
                        // t.NodeFont = f;
                        parentNode.Nodes.Add(t);
                        childNode = t;
                    }
                    PopulateTreeView(dr["ItemNumber"].ToString(), "2", childNode);
                }
            }

            // treeView1.SelectedNode = treeView1.Nodes[0];
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

        private void startprocessofbom()
        {
            txtvalue = Assy_txtbox.Text;
            try
            {
                treeView1.Nodes.Clear();
                RemoveChildNodes(root);
                treeView1.ResetText();

                filldatatable();
                fillrootnode("1");
            }
            catch (Exception)

            {
                if (!String.IsNullOrEmpty(txtvalue) && Char.IsLetter(txtvalue[0]))
                {
                    MessageBox.Show(" Item does not contain a Bill OF Material on Genius.", "SPM Connect - Bill Of Manufacturing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Assy_txtbox.Clear();
                }
                else
                {
                    MessageBox.Show("Invalid Search Parameter / Item Not Found On Genius.", "SPM Connect - Bill Of Manufacturing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    cleaup2();
                    Assy_txtbox.BackColor = Color.IndianRed;
                }
            }
        }

        #endregion assytextbox and button events

        #region treeview events

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        #endregion treeview events

        public void Compare1(TreeNode t1, TreeNode t2)
        {
            foreach (TreeNode item1 in t1.Nodes)
            {
                bool isFind = false;

                foreach (TreeNode item2 in t2.Nodes)
                {
                    if (item1.Text == item2.Text)
                    {
                        //find the same item. Continue to compare its children items.

                        Compare1(item1, item2);

                        isFind = true;
                        item2.ForeColor = Color.Gray;

                        Font boldFont = new Font(treeView2.Font, FontStyle.Strikeout);
                        item2.NodeFont = boldFont;
                        break;
                    }
                    if (item2.ForeColor != Color.Gray)
                    {
                        item2.ForeColor = Color.Green;
                    }
                }

                if (isFind)
                {
                    item1.ForeColor = Color.Gray;
                    continue;
                }
                else
                {
                    item1.ForeColor = Color.Red;
                }
            }
        }

        private void assytxt2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                startprocessofbom2();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CompareTreeNodes(treeView1, treeView2);
            treeView1.ExpandAll();
            treeView2.ExpandAll();
        }

        private void CompareRecursiveTree(TreeNode tn1, TreeNode tn2)
        {
            if (tn1.Text != tn2.Text)
            {
                //tn1.ForeColor = Color.Red;
                //tn2.ForeColor = Color.Red;

                TreeNode tnp = tn2;
                while (tnp.Parent != null)
                {
                    tnp = tnp.Parent;
                }
                tnp.BackColor = Color.Yellow;
            }
            // colour differently on comparing text
            int areThey = String.Compare(tn1.Tag.ToString(), tn2.Tag.ToString(), StringComparison.OrdinalIgnoreCase);

            if (areThey < 0)
            {
                tn1.ForeColor = Color.Red;
                tn2.ForeColor = Color.Green;
            }
            else if (areThey > 0)
            {
                tn1.ForeColor = Color.Green;
                tn2.ForeColor = Color.Red;
            }
            int compare = Math.Min(tn1.Nodes.Count, tn2.Nodes.Count);
            // ignore extra nodes
            for (int i = 0; i < compare; i++)
            {
                CompareRecursiveTree(tn1.Nodes[i], tn2.Nodes[i]);
            }
        }

        private void CompareTreeNodes(System.Windows.Forms.TreeView tv1, System.Windows.Forms.TreeView tv2)
        {
            int compare = Math.Max(tv1.Nodes.Count, tv2.Nodes.Count);
            // ignore extra nodes
            for (int i = 0; i < compare; i++)
            {
                //CompareRecursiveTree(tv1.Nodes[i], tv2.Nodes[i]);
                Compare1(tv1.Nodes[i], tv2.Nodes[i]);
            }
        }

        private void startprocessofbom2()
        {
            txtvalue = Assy_txtbox.Text;
            try
            {
                treeView2.Nodes.Clear();
                RemoveChildNodes(root2);
                filldatatable();
                fillrootnode("2");
                treeView2.ResetText();
            }
            catch (Exception)

            {
                if (!String.IsNullOrEmpty(txtvalue) && Char.IsLetter(txtvalue[0]))
                {
                    MessageBox.Show(" Item does not contain a Bill OF Material on Genius.", "SPM Connect - Bill Of Manufacturing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    // Assy_txtbox.Clear();
                    this.Hide();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid Search Parameter / Item Not Found On Genius.", "SPM Connect - Bill Of Manufacturing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    cleaup22();
                    Assy_txtbox.BackColor = Color.IndianRed; //to add high light
                    //Assy_txtbox.Clear();
                }
            }
        }

        private void TreeView_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        //void CompareRecursiveTree(TreeNode tn1, TreeNode tn2)
        //{
        //    if (tn1.Text != tn2.Text)
        //    {
        //        tn1.ForeColor = Color.Red;
        //        tn2.ForeColor = Color.Red;
        //    }
        //    int compare = Math.Min(tn1.Nodes.Count, tn2.Nodes.Count);
        //    // ignore extra nodes
        //    for (int i = 0; i < compare; i++)
        //    {
        //        CompareRecursiveTree(tn1.Nodes[i], tn2.Nodes[i]);
        //    }
        //}
    }
}