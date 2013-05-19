using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UniTerm.Sys;
using System.Collections;
using System.IO;

namespace UniTerm
{
    public partial class frmProp : Form
    {
        Config cConfig = new Config();
        List<int> _arrListIndex = new List<int>();
        public frmProp()
        {
            InitializeComponent();
        }

        private void frmProp_Load(object sender, EventArgs e)
        {
            Config.SysObject sObject;            
            ArrayList arrObject = cConfig.getObjectList();

            // Shutdown the painting of the ListBox as items are added.
            lstProp.BeginUpdate();
            // Loop through and add 50 items to the ListBox.

            for (int x = 0; x < arrObject.Count; x++)
            {
                sObject = (Config.SysObject)arrObject[x];
                lstProp.Items.Add(sObject.Type);
                _arrListIndex.Add(sObject.Index);
            }
            // Allow the ListBox to repaint and display the new items.
            lstProp.EndUpdate();
            lstProp.SelectedIndex = 0;
            //ShowProp();
        }

        private void lstProp_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowProp();
        }

        private void ShowProp()
        {
            string strFilePath;
            String strLine;
            String[] arrLine;

            DataTable DT = new DataTable("Table");
            DT.Columns.Add("c_prop", System.Type.GetType("System.String"));
            DT.Columns.Add("c_value", System.Type.GetType("System.String"));
            DataRow NewRows;

            ArrayList arrProtocol;

            String[] strFiles;
            String[] strLines;
            strFilePath = cConfig.getappSettings(Config.SettingField.DataSyncDir.ToString());
            strFiles = Directory.GetFiles(strFilePath, "act_*.txt");
            int c_count;
            c_count = strFiles.GetLength(0);
            if (c_count >0)
            {

                Array.Sort(strFiles);

                foreach (String fname in strFiles)
                {
                    strFilePath = fname;
                }

                CFile FileData = new CFile(strFilePath);
                List<String> arrLines = FileData.Read();
                //strLines = File.ReadAllLines(strFilePath);
                for (int i = 0; i < arrLines.Count; i++)
                {

                    strLine = arrLines[i];
                    //strLine = strLine.Replace(".", ",");
                    arrLine = strLine.Split(';');                    

                    if (arrLine[0].ToString() == _arrListIndex[lstProp.SelectedIndex].ToString())
                    {
                        arrProtocol = cConfig.getObjectProtocol(lstProp.SelectedItem.ToString(), Convert.ToInt16(arrLine[1].ToString()));

                        int mapIndex = 3;
                        foreach (Config.SysObjectProtocolMap Protocol in arrProtocol)
                        {
                            
                            NewRows = DT.NewRow();
                            NewRows["c_prop"] = Protocol.Description;
                            NewRows["c_value"] = arrLine[mapIndex + Protocol.Index];
                            DT.Rows.Add(NewRows);                            
                        }

                        grdProp.DataSource = DT;
                        grdProp.Columns["c_prop"].HeaderText = "Параметр";
                        grdProp.Columns["c_value"].HeaderText = "Значение";

                        grdProp.Columns["c_prop"].Width = 200;                        
                    }
                    strLine = "";
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ShowProp();
        }
    }
}