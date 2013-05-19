using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UniTerm.Sys;

namespace UniTerm.Sys
{
    class TermFile:CFile
    {
        private string strDirPath;        
        public TermFile()
        {
            Config cConf = new Config();
            strDirPath = cConf.getappSettings(Config.SettingField.DataSyncDir.ToString());
            int lastNum = GetLastFuleNum();
            lastNum++;
            string strNum = "000";
            int len = 3;
            
            
            if (lastNum<10)
            {
                len = 1;
            }
            else if (lastNum < 100)
            {
                len = 2;
            }
            strNum = strNum.Substring(0,3 - len) + lastNum.ToString();

            string strYear = DateTime.Now.Year.ToString();
            string strMonth = DateTime.Now.Month.ToString();
            string strDay = DateTime.Now.Day.ToString();
            
            fileName = strDirPath+"/act_" + strYear + strMonth + strDay + strNum + ".txt";            
        }

        /// <summary>
        ///  Получение списка записанных файлов
        /// </summary>
        /// <returns></returns>
        private int GetLastFuleNum()
        {
            String[] strFiles;
            Config SConf = new Config();
            //string strDirPath = "G:/temp/"; //SConf.getappSettings("DataSyncDir");
            string strFilePath = strDirPath;// GetFileLoadPath(); 

            string strYear = DateTime.Now.Year.ToString();
            string strMonth = DateTime.Now.Month.ToString();
            string strDay = DateTime.Now.Day.ToString();

            strFiles = Directory.GetFiles(strFilePath, "act_" + strYear + strMonth + strDay + "*.txt");
            

            //Сортировочка:
            Array.Sort(strFiles);
            String strNum = "1";
            foreach (String fname in strFiles)
            {
                strNum = fname.Substring(fname.Length - 7, 3);
            }
            //Array.Sort(strFiles, 0, strFiles.Length, new FileSort());                          

            return Convert.ToInt16(strNum);
        }
        
    }
}
