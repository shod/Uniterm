using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using UniTerm.Sys;

namespace UniTerm
{
    public partial class frmMain : Form
    {
        ArrayList arrObject;
        ArrayList arrInfoLine; // Массив строк для записи в файл
        private List<String> arrLineDemo;

        public int intervalNewData = 86400;
        private int _tCount = 0;
        private int _tError = 0;
        private int _Attempt = 0;
        private int _TermIndex = -1; // Опрашивать все терминалы, либо определенный
        private int _TermProtocolIndex = 0; //-1 Индекс протокола для терминала
        SPort cPort;
        CTerm.RequesParam _cTermReqParam;
        // Various colors for logging info
        private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        private enum TerminalAct
        {
            Start,
            Stop
        }

        bool TermStart;
        public frmMain()
        {
            InitializeComponent();

        }        

        private void frmMain_Load(object sender, EventArgs e)
        {
            ReadConfigSetting();
            CreateMenuReport();
            /* Создаем объект порт */
            cPort = new SPort();
            try
            {
                cPort.Open();
            }
            catch
            {
                MessageBox.Show("Нет доступа к порту!", "Ошибка запуска.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                Config cConf = new Config();
                intervalNewData = cConf.getPeriod() * 1000;
                //StartStopProcess(TermStart);
            }
            catch(Exception err)
            {
                StartStopProcess(Convert.ToBoolean(TerminalAct.Stop));
                MessageBox.Show(err.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                lblInfo.Text = "Ошибка в работе системы!";
            }

            //cPort.CurrentDataMode = SPort.DataMode.Hex;
            //cPort.SendData("2f3f210D0A");

            //cPort.SendData("8D0AAF3F218D0A1001101F1D10020930093030330C1003C515");
            //string retdata = cPort.ReturnData();


        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = intervalNewData;
            TerminalRefresh();            
        }

        /// <summary>
        /// Опрос терминалов
        /// </summary>
        private void TerminalRefresh()
        {
            statusMessage.Text = DateTime.Now.ToString( "H:mm:f");
            
            try
            {
                
                lblInfo.Text = "Система в работе.";
                Config.SysObject SysObject, Sterm;
                Config cConf = new Config();
                string strDemo = "";

                strDemo = cConf.getappSettings(Config.SettingField.Demo.ToString());

                if (strDemo != "")
                {
                    if (Convert.ToBoolean(strDemo))
                    {
                        TerminalRefreshDemo();
                        return;
                    }    
                
                }

                

                int c_count;
                CTerm cTermObj;
                int cAttempt = 0;
                _Attempt = Convert.ToInt16(cConf.getappSettings(Config.SettingField.AttemptCount.ToString()));
                
                cConf.getappSettings("Period");
                String strTermData;
                c_count = arrObject.Count;
                bool FlagNoTerm = false;

                for (int ia = 0; ia < c_count; ia++)
                {
                    Sterm = (Config.SysObject)arrObject[ia];
                    if (_TermIndex > 0 && Sterm.Index != _TermIndex)
                    {                        
                        FlagNoTerm = true;
                    }
                    else
                    {
                        FlagNoTerm = false;
                        break;
                    }
                }

                if (FlagNoTerm)
                {
                    lblInfo.Text = "Терминал с номером №" + _TermIndex + " не зарегистрирован в системе.";
                    return;
                }

                //TermFile cFileObj = new TermFile();
                strTermData = "";                
                //_tError = 0;

                arrInfoLine = new ArrayList();
                ArrayList arrResponseLine = new ArrayList();
                ArrayList arrErrorLine = new ArrayList() ; // Массив строк для записи ошибок
                bool FlagError = false;

                /* Прогресс */
                statusProgress.Visible = true;
                statusProgress.Value = 0;                
                
                for (int i = 0; i < c_count; i++)
                {
                    statusProgress.Step = (80 / c_count);
                    statusProgress.PerformStep();

                    SysObject = (Config.SysObject)arrObject[i];

                    FlagError = false;
                    if (_TermIndex == SysObject.Index || _TermIndex == -1)
                    {
                        /* Опрос терминалов */
                        cTermObj = new CTerm(SysObject.Index, SysObject.Type);                        
                        cTermObj.TermProtocolIndex = _TermProtocolIndex;
                        _TermProtocolIndex = _TermProtocolIndex; //-1 Сброс индекса протокола
                        cTermObj.RequestParam = _cTermReqParam;
                        cTermObj.Refresh(cPort);

                        if (cTermObj.isDataError())
                        {
                            FlagError = true;
                            arrErrorLine.Add("[" + SysObject.Index + "]" + cTermObj.getDataResponseHex());
                            //statusProgress.Value = statusProgress.Value - _Attempt;
                            statusProgress.Step = 1;

                            while (cAttempt <= _Attempt)
                            {                                
                                statusProgress.PerformStep();

                                cTermObj.Refresh(cPort);
                                Thread.Sleep((cAttempt + 1) * 200);
                                strTermData = cTermObj.getDataString();
                                if (cTermObj.isDataError() == false)
                                {
                                    strTermData = cTermObj.getDataString();
                                    FlagError = false;
                                    arrErrorLine.Clear();
                                    break;
                                }
                                arrErrorLine.Add("[" + SysObject.Index + "]" + cTermObj.getDataResponseHex());
                                arrResponseLine.Add("[" + SysObject.Index + "][" + cAttempt + "]" + cTermObj.getDataResponseHex());
                                cAttempt++;
                            }
                        }
                        else
                        {
                            //MessageBox.Show(cTermObj.getDataString());
                            strTermData = cTermObj.getDataString();
                        }
                        
                        //Оригинальные ответы
                        arrResponseLine.Add("[" + SysObject.Index + "]"+cTermObj.getDataResponseHex());
                        //arrInfoLine.Add(strTermData.ToString());
                        /* Запись строк в массив */
                        if (FlagError == false)
                        {
                            arrInfoLine.Add(strTermData.ToString());                            
                        }
                        else
                        {
                            _tError++;
                        }

                    }
                    
                    
                   
                    /* Запись оригинальных ответов */
                    if (Convert.ToBoolean(cConf.getappSettings(Config.SettingField.LogResponse.ToString()))
                        & arrResponseLine.Count>0)
                    {

                        LogFileSave(arrResponseLine);                        
                    }
                    _tCount++;
                }

                /* Запись в файл, если это не отчет*/
                if (_cTermReqParam.ReportType == null)
                {
                    if (arrInfoLine.Count > 0)
                    {
                        TermFile cFileObj = new TermFile();

                        foreach (string Line in arrInfoLine)
                        {
                            cFileObj.AddLine(Line);
                        }
                        cFileObj.Write();
                    }
                }

                /* Запись ошибок опросов */
                LogErrorFileSave(arrErrorLine);

                if (_cTermReqParam.ReportType !=  "")
                {
                    ReportFileSave(strTermData.Split('='), _cTermReqParam.ReportCode);
                }
               
        }
        catch(Exception e)
        {
            StartStopProcess(Convert.ToBoolean(TerminalAct.Stop));
            MessageBox.Show(e.Message, "Ошибка!",MessageBoxButtons.OK, MessageBoxIcon.Stop);
            lblInfo.Text = "Ошибка в работе системы!";            
        }
            statusProgress.Visible = false;
            lblTCount.Text = _tCount.ToString();
            lblError.Text = _tError.ToString();
        }

        private void lnkStart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Config cConf = new Config();
            intervalNewData = cConf.getPeriod() * 1000;
            StartStopProcess(TermStart);   
        }
        /// <summary>
        /// Начинает процесс опроса
        /// </summary>
        /// <returns></returns>
        private void StartStopProcess(Boolean tAct)
        {
            if (tAct == Convert.ToBoolean(TerminalAct.Start))
            {
                this.lnkStart.Text = "Выключить опрос";
                timer.Start();
                TermStart = true;
                TerminalRefresh();
            }
            else
            {
                this.lnkStart.Text = "Включить опрос";
                timer.Stop();
                TermStart = false;
            }
        }

        private void lnkSetting_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmSetting fSetting = new frmSetting();
            fSetting.ShowDialog();// ShowDialog(this);
            ReadConfigSetting();
        }

        public static long GetDouble(string strHex)
        {
            string strHexRev = String.Empty;
            int cnt = strHex.Length / 2;
            for (int i = 0; i < cnt; i++)
                strHexRev = strHex.Substring(i * 2, 2) + strHexRev;
            Int64 longHex = Int64.Parse(strHexRev, System.Globalization.NumberStyles.HexNumber);            
            return longHex;
            //return BitConverter.ToInt64(longHex,);
                //.Int64BitsToDouble(longHex);
        }

        public static long GetHexToInt(string strHex)
        {
            
            string strHexRev = String.Empty;
            for (int i = 0; i < 3; i++)
                strHexRev = strHex.Substring(i * 2, 2) + strHexRev;
            Int64 longHex = Int64.Parse(strHexRev, System.Globalization.NumberStyles.HexNumber);
            return longHex;
            //return BitConverter.ToInt64(longHex,);
            //.Int64BitsToDouble(longHex);
        }

        private void ReadConfigSetting()
        {
            Config cConf = new Config();

            /* 
             * Получение массива объектов для опроса
             */
            arrObject = cConf.getObjectList();
            //Config.SysObject cObj;

            intervalNewData = cConf.getPeriod() * 1000;
            timer.Interval = intervalNewData;

            lblPeriod.Text = Convert.ToString(intervalNewData / 1000);
            lblAttempt.Text = cConf.getappSettings(Config.SettingField.AttemptCount.ToString());
            lblTermCount.Text = cConf.getObjectList().Count.ToString();
            lblPort.Text = cConf.getappSettings(Config.SettingField.Port.ToString());
        }

        private void btnOneRef_Click(object sender, EventArgs e)
        {
            _TermIndex = Convert.ToInt16(numTerminal.Value);
            TerminalRefresh();
            _TermIndex = -1;
        }

        /// <summary>
        /// Save Log file
        /// </summary>
        /// <param name="arrLine"></param>
        private void LogFileSave(ArrayList arrLine)
        {
            string strYear = DateTime.Now.Year.ToString();
            string strMonth = DateTime.Now.Month.ToString();
            string strDay = DateTime.Now.Day.ToString();

            Config cConf = new Config();
            CFile cLogFile = new CFile();
            string DirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());
            cLogFile.fileName = DirPath + "/log_" + strYear + strMonth + strDay + ".txt";

            string sLine;
            foreach (string Line in arrLine)
            {
                sLine = "["+DateTime.Now.ToUniversalTime().ToString() + "]" + Line;
                cLogFile.AddLine(sLine);
            }
            cLogFile.Write();                        
        }

        /// <summary>
        /// Save Log file
        /// </summary>
        /// <param name="arrLine"></param>
        private void LogErrorFileSave(ArrayList arrLine)
        {
            string strYear = DateTime.Now.Year.ToString();
            string strMonth = DateTime.Now.Month.ToString();
            string strDay = DateTime.Now.Day.ToString();

            if (arrLine.Count == 0)
            {
                return;
            }

            Config cConf = new Config();
            CFile cLogFile = new CFile();
            string DirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());
            cLogFile.fileName = DirPath + "/errorTerminal.log";

            string sLine;
            foreach (string Line in arrLine)
            {
                sLine = "[" + DateTime.Now.ToUniversalTime().ToString() + "]" + Line;
                cLogFile.AddLine(sLine);
            }
            cLogFile.Write();
        }

        /// <summary>
        /// Save Report file
        /// </summary>
        /// <param name="arrLine"></param>
        private void ReportFileSave(String[] arrLine, string RepName)
        {            

            Config cConf = new Config();
            CFile cLogFile = new CFile();
            string DirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());
            string strFilePath = DirPath + "/rep_" + RepName + ".csv";
            cLogFile.fileName = strFilePath;

            System.IO.File.Delete(@strFilePath);
            
            foreach (string Line in arrLine)
            {                
                cLogFile.AddLine(Line);
            }
            cLogFile.Write();
        }

        /// <summary>
        /// Demo regime
        /// </summary>
        private void TerminalRefreshDemo()
        {            
            Config cConf = new Config();
            arrInfoLine = new ArrayList();
            String strLine;
            String[] arrLine;
            string DirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());            
            CFile FileDemo = new CFile(DirPath+"/demo_act.txt");
            CFile FileDemoNew = new CFile(DirPath + "/demo_act.txt");
            arrLineDemo = FileDemo.Read();

            /* Прогресс */
            statusProgress.Visible = true;
            statusProgress.Value = 0;

            TermFile cFileObj = new TermFile();
            
            int c_count = arrLineDemo.Count;
            int c_Rand;
            Random fixRand = new Random();
            c_Rand = fixRand.Next(1, 7);
            for (int i = 0; i < c_count; i++)
            {
                Thread.Sleep(1000);                   
                statusProgress.Step = (80 / c_count);
                statusProgress.PerformStep();
                //arrInfoLine.Add(arrLineDemo[i]);
                strLine = arrLineDemo[i];
                //strLine = strLine.Replace(".", ",");
                arrLine = strLine.Split(';');
                strLine = "";

                double dd;                
                int idd;
               
                int l_count = arrLine.Length;                
                for (int a = 0; a < l_count; a++)
                {
                    if (a == 2)
                    {
                        arrLine[a] = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
                    }

                    //if (a == 13 || a==14 ||a==15)
                    if (a >10 && Char.IsDigit(Convert.ToChar(arrLine[a].Substring(0,1))))
                    {
                        try
                        {
                            dd = Convert.ToDouble(arrLine[a]);
                            dd = dd + (dd / 100 * 0.5);                            
                            arrLine[a] = dd.ToString();
                        }
                        catch
                        {
                            idd = Convert.ToInt16(arrLine[a]);
                            idd = idd + (idd / 100 * l_count);
                            arrLine[a] = idd.ToString();
                        }
                        
                    }
                }
                strLine = String.Join(";", arrLine);
                strLine = strLine.Replace(",", ".");

                if (i <= c_Rand-1)
                {
                    cFileObj.AddLine(strLine);
                }
                
                FileDemoNew.AddLine(strLine);
            }
            cFileObj.Write();
            FileDemoNew.Write(false);

            _tCount++;
            LogFileSave(arrInfoLine);
            statusProgress.Visible = false;
            lblTCount.Text = _tCount.ToString();
            lblError.Text = _tError.ToString();
        }

        private void lnkProp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmProp fProp = new frmProp();
            fProp.ShowDialog();// ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _TermIndex = Convert.ToInt32(numericUpDown1.Value);
    
            TerminalRefresh();
        }

        private void CreateMenuReport()
        {
            string strValue = "";
            string strName = "";
            string _AppPath;
            _AppPath = Application.StartupPath;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_AppPath + "\\Reports\\repconf.xml");
            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("//reports");
            foreach (XmlNode node in appSettingsNode.ChildNodes)
            {
                strName = node.Attributes["name"].Value.ToString();
                strValue = node.Attributes["code"].Value.ToString();
                ToolStripMenuItem item = new ToolStripMenuItem(strName, null, new System.EventHandler(menuReport_Click), strValue);
                menuReport.DropDownItems.Add(item);
            }

        }

        private void menuReport_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            frmReport frmRep = new frmReport();
            frmReport.sctReport sReport;
            //frmRep.MdiParent = this;
            frmRep.SetDBProcName = "rep" + item.Name;
            frmRep.SetReportName = item.Name;
            frmRep.SetXMLForm = GetXmlRepConfig();            
            frmRep.Show();
            
            sReport = frmRep.GetConfigReport();
            
            // Указание какой протокол терминала запускать
            //_TermProtocolIndex = sReport.ProtocolIndex;
            _cTermReqParam.DateFrom = sReport.DateFrom;
            _cTermReqParam.DateTo = sReport.DateTo;
            _cTermReqParam.ProtocolIndex = sReport.ProtocolIndex;
            _cTermReqParam.ReportType = sReport.Type;
            _cTermReqParam.ReportCode = sReport.Code;
            TerminalRefresh();
            frmRep.ReportGenerate();
        }

        private XmlDocument GetXmlRepConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Application.StartupPath + "\\Reports\\repconf.xml");
            return xmlDoc;
        }

    }
}