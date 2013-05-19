using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UniTerm;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
//using UniTerm.SysClass;
using System.Xml;
using UniTerm.Sys;


namespace UniTerm
{
    public partial class frmReport : Form
    {
        private string _repName;
        private string _repDBProcName;
        CrystalDecisions.CrystalReports.Engine.ReportDocument myReportDocument;
            
        public frmReport()
        {
            InitializeComponent();
        }

        public struct sctReport
        {
            public string Code;
            public int ProtocolIndex;
            public DateTime DateFrom;
            public DateTime DateTo;
            public string Type;
        }

        sctReport sReport = new sctReport();

        public string SetReportName
        {

            get
            {

                return _repName;

            }

            set
            {

                _repName = value;

            }

        }

        public string SetDBProcName
        {

            get
            {

                return _repDBProcName;

            }

            set
            {

                _repDBProcName = value;

            }

        }

        /// <summary>
        /// ������ XML ������ ��� �������� ����� ����������
        /// </summary>
        private XmlDocument _repXMLForm;
        public XmlDocument SetXMLForm
        {

            set
            {

                _repXMLForm = value;

            }
        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {            
            myReportDocument = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
            string strDirPath = Application.StartupPath + "/Reports/" + _repName +".rpt";

            try
            {
                myReportDocument.Load(strDirPath);
            }
            catch
            {
                MessageBox.Show("��� ����� ������!/n" + strDirPath, "������", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            SetDataReport(_repName);

            
            // @TODO �������� �� ����� ������� DataSetReport
            //if (_repDBProcName == "Rep_GetReportLast")
            //{
            //    SetDataReportLast();
            //}
            //else
            //{
            //    SetDataReport();
            //}
        }

        /// <summary>
        /// ������ ����� ������
        /// </summary>
        public void ReportGenerate()
        {
            DataTable DT = new DataTable();
            Config cConf = new Config();
            string DirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());
            string fileName = DirPath + "/rep_" + _repName + ".csv"; //DirPath + 

            try
            {
                DT = UniTerm.Program.GetDataTableFromCsv(fileName, false);
                myReportDocument.SetDataSource(DT);
                ReportViewer.ReportSource = myReportDocument;
            }
            catch (Exception e)
            {
                MessageBox.Show(_repName + "\n" + e.Message + "\n(" + fileName + ")", "������ ��� ���������� ���� ������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetDataReport(string repName)
        {
            //DataSet DS = new DataSet();
            
            //DBAdapter DBA = new DBAdapter();
            //ProcParams PP = new ProcParams();
            

            // ���������� ����
            frmRepForm RepForm = new frmRepForm();

            try
            {

                ShowFormElement(RepForm);

                if (DialogResult.Yes == RepForm.ShowDialog(this))
                {

                    if (RepForm.DateFrom != "")
                    {
                        sReport.DateFrom = Convert.ToDateTime(RepForm.DateFrom);
                    }

                    if (RepForm.DateTo != "")
                    {
                        sReport.DateTo = Convert.ToDateTime(RepForm.DateTo);
                    }

                    //DBA.ProcParams = PP;
                    //DBA.SetDbData(DS, _repDBProcName); //����� ��������� ������            
                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(_repName + "\n" + e.Message, "������ ��������� ����� ���������� ������.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetDataReportLast()
        {
            DataSet DS = new DataSet();
            //DBAdapter DBA = new DBAdapter();
            int dMonth;
            dMonth = DateTime.Now.Month;
            // ���������� ����
            frmRepForm RepForm = new frmRepForm();

            //ProcParams PP = new ProcParams("month", "N", dMonth.ToString());
            //ReportDocument Report = new ReportDocument();
            //ReportMKTC Report = new ReportMKTC();

            //DBA.ProcParams = PP;
            //DBA.SetDbData(DS, _repDBProcName); //����� ��������� ������

            /*.Select("month = 11")*/
            myReportDocument.SetDataSource(DS.Tables[0]);
            // @TODO �������� �������� ������
            //Report.Load("path");
            

            /*CrystalDecisions.CrystalReports.Engine.TextObject root;
            root = (CrystalDecisions.CrystalReports.Engine.TextObject)
            reportMKTC.ReportDefinition.ReportObjects["CurrDate"];
            root.;
            */

            /*  ��������� ������  */
            /*ParameterFieldDefinitions crParameterFieldDefinitions;
            ParameterFieldDefinition crParameterFieldDefinition;
            ParameterValues crParameterValues = new ParameterValues();
            ParameterDiscreteValue crParameterDiscreteValue = new ParameterDiscreteValue();

            crParameterDiscreteValue.Value = Convert.ToInt32("2010");
            crParameterFieldDefinitions = Report.DataDefinition.ParameterFields;
            crParameterFieldDefinition = crParameterFieldDefinitions["Year"];

            crParameterValues = crParameterFieldDefinition.CurrentValues;

            crParameterValues.Clear();
            crParameterValues.Add(crParameterDiscreteValue);
            crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);
            */
            
            //ReportViewer.ReportSource = Report;
            //ReportViewer.Refresh();            
            ReportViewer.ReportSource = myReportDocument;
            //ReportViewer.Bi DataBind();
        }

        /// <summary>
        /// ���������� ��������� ������� ������
        /// </summary>
        /// <param name="RepForm"></param>
        /// <returns></returns>
        public sctReport GetConfigReport()
        {            

            XmlDocument xmlDoc = _repXMLForm;

            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("//reports");            
            foreach (XmlNode node in appSettingsNode.ChildNodes)
            {
                if (node.Attributes["code"].Value.ToString() == _repName)
                {
                    sReport.Code = node.Attributes["code"].Value.ToString();
                    sReport.ProtocolIndex = Convert.ToInt16(node.Attributes["protocol_index"].Value.ToString());
                    sReport.Type = node.Attributes["type"].Value.ToString();
                    
                }                
            }
            return sReport;
        }

        private void ShowFormElement(UniTerm.frmRepForm RepForm)
        {
            string strForm = "";
            XmlDocument xmlDoc = _repXMLForm;

            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("//reports");
            //xmlDoc.SelectSingleNode("//report[@code='" + _repName + "']");
            foreach (XmlNode node in appSettingsNode.ChildNodes)
            {
                if (node.Attributes["code"].Value.ToString() == _repName)
                {
                    strForm = node.Attributes["form"].Value.ToString();
                }
            }

            if (strForm != "")
            {
                appSettingsNode =
                  xmlDoc.SelectSingleNode("//form[@name='" + strForm + "']");
                foreach (XmlNode node in appSettingsNode.ChildNodes)
                {

                    RepForm.ShowElement(node.Attributes["code"].Value.ToString());
                }
            }
        }
    }
}