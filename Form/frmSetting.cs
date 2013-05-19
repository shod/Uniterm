using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using UniTerm.Sys;
using System.IO.Ports;
namespace UniTerm
{
    public partial class frmSetting : Form
    {
        private string _PathFolder; /* Путь к папке c данными*/
        private string _numPeriod; /* Частота опроса*/
        private string _numErr; /* Кол-во попыток опроса при ошибке*/

        public frmSetting()
        {
            InitializeComponent();
            InitSetting();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dlgFolder.ShowDialog();
            _PathFolder = dlgFolder.SelectedPath;
            lblDirPath.Text = _PathFolder;
        }

        private void dlgFolder_HelpRequest(object sender, EventArgs e)
        {
            _PathFolder = dlgFolder.SelectedPath;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Config cConf = new Config();
            
            cConf.SaveSettings(Config.SettingField.Period.ToString(), numPeriod.Value.ToString());
            cConf.SaveSettings(Config.SettingField.AttemptCount.ToString(), numErr.Value.ToString());
            cConf.SaveSettings(Config.SettingField.LogResponse.ToString(), chkResponse.Checked.ToString());
            cConf.SaveSettings(Config.SettingField.DataSyncDir.ToString(), lblDirPath.Text);
            cConf.SaveSettings(Config.SettingField.TimeDelay.ToString(), numDelay.Value.ToString());
            cConf.SaveSettings(Config.SettingField.TimeWait.ToString(), numWait.Value.ToString());

            cConf.SaveSettings(Config.SettingField.Demo.ToString(), chkDemo.Checked.ToString());
            this.Close();
        }
        
        private void InitSetting()
        {
            string strDemo = "";
            try
            {
                Sys.Config cConf = new UniTerm.Sys.Config();
                numPeriod.Value = cConf.getPeriod();
                numErr.Value = Convert.ToInt16(cConf.getappSettings("AttemptCount"));
                lblDirPath.Text = cConf.getappSettings("DataSyncDir");

                chkResponse.Checked = Convert.ToBoolean(cConf.getappSettings(Config.SettingField.LogResponse.ToString()));
                numDelay.Value = Convert.ToInt16(cConf.getappSettings(Config.SettingField.TimeDelay.ToString()));
                numWait.Value = Convert.ToInt16(cConf.getappSettings(Config.SettingField.TimeWait.ToString()));

                strDemo = cConf.getappSettings(Config.SettingField.Demo.ToString());

                if (strDemo == "")
                {
                    chkDemo.Visible = false;
                }
                else {
                    chkDemo.Checked = Convert.ToBoolean(cConf.getappSettings(Config.SettingField.Demo.ToString()));
                }
                

                // Check port
                //string[] ports = SerialPort.GetPortNames();
           

                // Display each port name to the console.
                /*foreach(string port in ports)
                {                    
                    comboPort.Items.Add("Com" + port);
                } 
                */

                //@todo 
                //if (Convert.ToBoolean(cConf.getappSettings(Config.SettingField.LogResponse.ToString()))==True)
                //{
                //    chkResponse.Checked = true;
                //}
            }
            catch (Exception e)
            {             
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Stop);             
            }
        }

    }
}