using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;

namespace UniTerm.Sys
{  

    class Config
    {
        #region Local Variables
        private string _AppPath;
        private string Config_Protocol = "config/protocol.xml";
        private int _intPeriod;
        
        public struct SysObject
        {
            public int Index;
            public string Type;
        }

        public struct SysObjectCommand
        {            
            public string Value;
            public string Mode; //Режим auto - работа в автоматическом режиме. repo - для отчетов
            public int Index;
        }

        public struct SysObjectProtocol
        {
            public int Index;
            public string Name;
            public string Command;
            public ArrayList MapStruct;            
        }

        public struct SysObjectProtocolMap
        {
            public int Index;
            public string Name;
            public string Pos;
            public int Len;
            public int Comma;
            public string Description;
            public string Command;
        }

        public enum SettingField
        {
            Period,
            AttemptCount,
            DataSyncDir,
            Port,
            LogResponse,
            TimeDelay,
            TimeWait,
            Demo
        }
        #endregion

        #region Protocol Config
        /// <summary>
        /// Возвращает массив объектов по структуре SysObject
        /// </summary>
        /// <returns></returns>
        public ArrayList getObjectList()
        {
            ArrayList arrField = new ArrayList();
            SysObject sObject;
            DataSet DS = new DataSet();            

            try
            {
                GetXMLFileData(DS, Config_Protocol);
            }
            catch (Exception e)
            {
                string sError = e.Message;                     
            }

            DataRow row;
            XmlDataDocument XDoc = new XmlDataDocument(DS);
            XmlNodeList ProtocolNode = XDoc.DocumentElement.SelectNodes("//objects/obj");
            
            
            
            foreach (XmlNode xmlNode in ProtocolNode)
            {
                row = XDoc.GetRowFromElement((XmlElement)xmlNode);

                if (row != null)
                {                    
                    sObject.Index= Convert.ToInt16(row[0].ToString());
                    sObject.Type = row[1].ToString();
                    arrField.Add(sObject);
                }
            }

            return arrField;
             
        }

        /// <summary>
        /// Возвращает массив элементов протокола
        /// </summary>
        /// <param name="TermType"></param>

        public ArrayList getObjectProtocol(string TermType, int ProtocolIndex)
        {
            ArrayList arrField = new ArrayList();
            //ArrayList arrMap;
            //SysObjectProtocol sObject;
            SysObjectProtocolMap pMap;
            DataSet DS = new DataSet();


            try
            {
                GetXMLFileData(DS, Config_Protocol);
            }
            catch (Exception e)
            {
                string sError = e.Message;
            }

            DataRow rowM;
            XmlDataDocument XDoc = new XmlDataDocument(DS);
            XmlNodeList ProtocolNode = XDoc.DocumentElement.SelectNodes("//objtypes/objtype[@name='" + TermType + "'][@index='" + ProtocolIndex + "']/col");


            foreach (XmlNode xmlNode in ProtocolNode)
            {
                rowM = XDoc.GetRowFromElement((XmlElement)xmlNode);

                if (rowM != null)
                {
                    
                    pMap.Name = rowM[0].ToString();
                    pMap.Index = Convert.ToInt16(rowM[1].ToString());
                    pMap.Pos = rowM[2].ToString();
                    pMap.Len = Convert.ToInt16(rowM[3].ToString());
                    if (rowM[4].ToString() == "")
                    {
                        rowM[4] = 0;
                    }
                    pMap.Comma = Convert.ToInt16(rowM[4].ToString());
                    pMap.Description = rowM[5].ToString();
                    pMap.Command = rowM[6].ToString();
                    
                    //arrMap[pMap.Index] = pMap;
                    arrField.Add(pMap);            
                }
            }

            return arrField;

        }

        //private ArrayList getObjectProtocol_old(string TermType)
        //{
        //    ArrayList arrField = new ArrayList();
        //    ArrayList arrMap;
        //    SysObjectProtocol sObject;
        //    SysObjectProtocolMap pMap;
        //    DataSet DS = new DataSet();
            

        //    try
        //    {
        //        GetXMLFileData(DS, Config_Protocol);
        //    }
        //    catch (Exception e)
        //    {
        //        string sError = e.Message;                     
        //    }

        //    DataRow row, rowM;
        //    XmlDataDocument XDoc = new XmlDataDocument(DS);
        //    XmlNodeList ProtocolNode = XDoc.DocumentElement.SelectNodes("//objtypes/objtype");
            
                        
        //    foreach (XmlNode xmlNode in ProtocolNode)
        //    {
        //        row = XDoc.GetRowFromElement((XmlElement)xmlNode);

        //        if (row != null)
        //        {                    
        //            sObject.Index = Convert.ToInt16(row[0].ToString());
        //            sObject.Name = row[1].ToString();
                                                            

        //            /* 
        //             * Описание структуры протокола
        //             */

        //            XmlNodeList MapNode = xmlNode.SelectNodes("//objtypes/objtype[@name='" + sObject.Name + "']/col");
        //            //XmlDataDocument XDocMap = new XmlDataDocument(DS);
        //            //XmlNodeList MapNode = XDocMap.DocumentElement.SelectNodes("//objtypes/objtype[" + sObject.Name + "]/col");

        //            arrMap = new ArrayList();
        //            foreach (XmlNode xmlNodeMap in MapNode)
        //            {
        //                rowM = XDoc.GetRowFromElement((XmlElement)xmlNodeMap);

        //                if (rowM != null)
        //                {                            
        //                    pMap.Name = rowM[0].ToString();
        //                    pMap.Index = Convert.ToInt16(rowM[1].ToString());
        //                    pMap.Pos = rowM[2].ToString();
        //                    pMap.Len = Convert.ToInt16(rowM[3].ToString());
        //                    pMap.Comma = 0;
        //                    //arrMap[pMap.Index] = pMap;
        //                    ///arrMap.Add(pMap);
        //                }
        //            }
        //            sObject.MapStruct = arrMap;
        //            arrField.Add(sObject);
        //        }
        //    }

        //    return arrField;

        //}
        #endregion

        public Config()
        {
            _AppPath = Application.StartupPath;
        }

        /// <summary>
        /// Считывает данные из XML файла в DataSet
        /// </summary>
        /// <param name="dsData"></param>
        /// <param name="CodeFile"></param>
        public void GetXMLFileData(DataSet dsData, string CodeFile)
        {
            string strFilePath = _AppPath + "/" + CodeFile;                             
            dsData.ReadXml(strFilePath);

        }

        public int getPeriod()
        {
            _intPeriod = Convert.ToInt16(getappSettings("Period"));
            return _intPeriod;
        }

        /// <summary>
        /// Получение списка команд. Тестовый режим
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public ArrayList getObjectTypeCommand(string TermType)
        {
            ArrayList arrFieldObj = new ArrayList();
            SysObjectCommand spObject;
            DataSet DS = new DataSet();

            try
            {
                GetXMLFileData(DS, Config_Protocol);
            }
            catch (Exception e)
            {
                string sError = e.Message;
            }

            DataRow row;
            XmlDataDocument XDoc = new XmlDataDocument(DS);
            //XmlNodeList ProtocolNode = XDoc.DocumentElement.SelectNodes("//objtypes/objtype[@name='" + TermType + "']/cmd");
            XmlNodeList ProtocolNode = XDoc.DocumentElement.SelectNodes("//objtypes/objtype[@name='" + TermType + "']");


            XmlNode node;

            foreach (XmlNode xmlNode in ProtocolNode)
            {
                row = XDoc.GetRowFromElement((XmlElement)xmlNode);

                if (row != null)
                {
                    spObject = new SysObjectCommand();
                    spObject.Value = xmlNode.Attributes.GetNamedItem("cmd").Value;

                    node = xmlNode.Attributes["index"];
                    if (node != null)
                    {
                        spObject.Index = Convert.ToInt16(xmlNode.Attributes.GetNamedItem("index").Value);
                    }

                    node = xmlNode.Attributes["mode"];
                    if (node != null)
                    {
                        spObject.Mode = xmlNode.Attributes.GetNamedItem("mode").Value;
                    }else{
                        spObject.Mode = "auto";
                    }
                    arrFieldObj.Add(spObject);                    
                }
            }

           
            return arrFieldObj;

        }

        #region Main Config
  
        public string getappSettings(string Name)
        {
            string ConStr;
            
            //ConStr = ConfigurationSettings.AppSettings[Name];
            ConStr = GetAppConfig(Name);
            return ConStr;
        }
        #endregion

        #region Work app Config
        public void SaveSettings(string Name, string Value)
        {
            //System.Configuration.ConfigurationSettings.AppSettings.Set(Name, Value);
            UpdateKey(Name, Value);
        }

        // Updates a key within the App.config
        public void UpdateKey(string strKey, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_AppPath +
                                 "\\App.config");
            if (!KeyExists(strKey, xmlDoc))
                throw new ArgumentNullException("Key", "<" + strKey +
                      "> does not exist in the configuration. Update failed.");
            XmlNode appSettingsNode =
               xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                {
                    childNode.Attributes["value"].Value = newValue;
                    break;
                }
            }
            xmlDoc.Save(_AppPath +
                                         "\\App.config");
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }

        // Determines if a key exists within the App.config
        public bool KeyExists(string strKey, XmlDocument xmlDoc)
        {
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(_AppPath +
            //                     "\\App.config");


            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                    return true;
            }
            return false;
        }


        //This code will add a listviewitem 
        //to a listview for each database entry 
        //in the appSettings section of an App.config file.
        private string GetAppConfig(string Name)
        {
            string strValue = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_AppPath +
                                         "\\App.config");
            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("configuration/appSettings");
            foreach (XmlNode node in appSettingsNode.ChildNodes)
            {
                if (node.Attributes["key"].Value == Name)
                {
                    strValue = node.Attributes["value"].Value.ToString();
                }
                //ListViewItem lvi = new ListViewItem();
                //string connStr = node.Attributes["value"].Value.ToString();
                //string keyName = node.Attributes["key"].Value.ToString();
                //lvi.Text = keyName;
                //lvi.SubItems.Add(connStr);
                //this.lvDatabases.Items.Add(lvi);
            }
            return strValue;
        }
        #endregion
    }
}
