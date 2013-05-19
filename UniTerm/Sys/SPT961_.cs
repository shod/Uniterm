using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace UniTerm.Sys
{    

    class SPT961
    {
        private string ProtocolName = "SPT961";
        public int _TimeWait;
        public int _TimeDelay;
        public int _TermId;
        public CTerm.ResponseData ResData;
        ArrayList arrObjectCommand;
        ArrayList arrProtocolCommand;

        private string strSOH = "1001";
        private string strHeader = "101f1d";
        private string strData = "1002#1003";
        private string strCRC = "";

        /// <summary>
        /// Обновляет, опрашивает состояние и получает данные с терминала 
        /// </summary>
        public void Refresh(SPort cPort)
        {
            Config cConf = new Config();
            Config.SysObjectCommand Sterm;
            string strResData = "";
            string sCommand;
            String[] arrResData; // Массив ответа
            String[] arrInfoResp; // Массив Информации

            /*
             * Для прохода по параметрам протокола
             */
            Config.SysObjectProtocol Sprotokol;            
            /* 
             * Получение массива объектов для опроса
             */
            arrObjectCommand = cConf.getObjectTypeCommand(ProtocolName);
            arrProtocolCommand = cConf.getObjectProtocol(ProtocolName);                    

            string strCheck = ">";
            string retData = "";
            try
            {
                cPort.CurrentDataMode = SPort.DataMode.Hex;
                /*
                 * Проходим по набору команд
                 */
                cPort.Clear();
                int c_count = arrObjectCommand.Count;

                for (int ia = 0; ia < c_count; ia++)
                {
                    Sterm = (Config.SysObjectCommand)arrObjectCommand[ia];
                    sCommand = Sterm.Value;
               
                    cPort.SendData(sCommand);                    
                    Thread.Sleep(_TimeDelay);
                    ResData.strData = cPort.ReturnData();


                    if (ResData.strData.Length >= 2 && ia == (c_count-1))
                    {
                        retData = ResData.strData.Substring(0, 2);
                        //char c = (char)Convert.ToInt32(retData, 16);
                        retData = Data_Hex_Asc(ref retData);
                    }
                    else
                    {
                        ResData.IsError = true;
                        ResData.strData = "0 - Приемник не подключен"; //0 - приёмник;
                    }
                }
                
                /* 
                 * Проверка на готовность ответа порта
                 */
                //if (retData == strCheck)
                if (1 == 1)
                {
                    string ProtocolCommand;
                    int pos;
                    cPort.Clear();
                    //int c_count = arrProtocolCommand.Count;
                    ResData.IsError = false;
                    sCommand = "";
                    foreach (Config.SysObjectProtocolMap Protocol in arrProtocolCommand)
                    {
                        cPort.Clear();
                        ProtocolCommand = Protocol.Command;
                        //sCommand = MessagePrepare(sCommand);
                        
                        //frmMain.LogErrorFileSave(ProtocolCommand);
                        cPort.SendData(ProtocolCommand);
                        //pos = ProtocolCommand.IndexOf("0c1003");
                        //sCommand = ProtocolCommand.Substring(17, (pos - 17));

                        //Thread.Sleep(_TimeDelay);
                        Thread.Sleep(1500);
                        //Thread.Sleep(_TimeWait);
                        /* ТЕст */
                        /*if (Protocol.Index == 1)
                        {
                            ResData.strData = "1001101f0310020930093030330c093130343031303030303209a12fe00c1003bf85";
                        }
                        else if (Protocol.Index == 2)
                        {
                            ResData.strData = "1001101f0310020930093039390c093936312e303234333130333209a12fe00c1003cdc5";
                        }
                        else
                        {
                            ResData.strData = cPort.ReturnData();
                        }*/
                        ResData.strData = cPort.ReturnData().Replace(" ", "");
                        ///retData = retData + sep + Data_Hex_Dec(ref strItem);                    

                        /*-------------------------------------------*/

                        // TODO отключил - проверить
                        //if (ResData.strData.Length > 2)
                        //{                                                        
                        //    retData = ResData.strData.Substring(0, 2);
                        //}
                        //else
                        //{
                        //    ResData.IsError = true;
                        //    ResData.strData = "0 - Данные не поступают"; //0 - приёмник;
                        //}


                        if (ResData.strData.Length > 2)
                        {
                            String res = ResData.strData;
                            res = res.Replace(" ", "");//.Substring(4);                                                

                            if (res.Length <= 2)
                            {
                                ResData.IsError = true;
                                ResData.strData = "1 - со счетчиком связь отсутствует";
                            }
                            else
                            {
                                //_TimeWait = Convert.ToInt16(res.Substring(2, 2)) * 500;
                                res = res.Substring(4);
                                if (res.Substring(0, 2) == "FF")
                                {
                                    ResData.IsError = true;
                                    ResData.strData = "1 - Связь со счетчиком отсутствует.";
                                }
                                else
                                {
                                    /* Вычисление позиции старта данных */
                                    //long var1;
                                    //byte var2 = 7;
                                    //byte var3 = Convert.ToByte(res.Substring(1, 1));
                                    //var1 = (var3 & var2) + (byte)5;
                                    //int start = Convert.ToInt16(var1);
                                    //int start = Convert.ToInt16(ResData.strData.IndexOf("0c09"))+4;

                                    arrResData = Regex.Split(ResData.strData, "0C");
                                    arrInfoResp = Regex.Split(arrResData[1], "09");                                    

                                    ResData.strData = arrInfoResp[1];
                                    ResData.strHead = arrResData[0];
                                    /* ПРоверка на весь пакет */
                                    if (ResData.strData.Length == 0)
                                    {
                                        ResData.IsError = true;
                                        ResData.strData = "3 - Данные со счетчика не поступают.";
                                    }
                                    //@TODO
                                    else if (ResData.strData.Length < 4 * 2)
                                    {
                                        ResData.IsError = true;
                                        ResData.strData = "4 - Данные искажены.";
                                    }
                                }
                            }

                            strResData = strResData + ResData.strData;
                        }
                        else
                        {
                            ResData.IsError = true;
                            ResData.strData = "00";
                            break;
                        }
                    }
                    ResData.strData = strResData;
                    return;

                }
            }
            catch (Exception e)
            {
                ResData.IsError = true;
                ResData.strData = e.Message;
            }
        }
        #region HexFunction
        public string Data_Hex_Asc(ref string Data)
        {

            string Data1 = "";

            string sData = "";

            while (Data.Length > 0)

            //first take two hex value using substring.

            //then convert Hex value into ascii.

            //then convert ascii value into character.
            {
                Data1 = System.Convert.ToChar(System.Convert.ToUInt32(Data.Substring(0, 2), 16)).ToString();

                sData = sData + Data1;

                Data = Data.Substring(2, Data.Length - 2);

            }
            return sData;
        }

        public string Data_Hex_Dec(ref string Data)
        {

            string Data1 = "";

            string sData = "";

            while (Data.Length > 0)

            //first take two hex value using substring.

            //then convert Hex value into ascii.

            //then convert ascii value into character.
            {
                Data1 = System.Convert.ToUInt32(Data.Substring(0, 2), 16).ToString();

                sData = sData + Data1;

                Data = Data.Substring(2, Data.Length - 2);

            }
            return sData;
        }
        #endregion //HexFunction
        ////////////////////////////////////////////////////////
        //Функция вычисляет и возвращает циклический код для
        //последовательности из len байтов, указанной *msg. 
        //Используется порождающий полином:
        //(X в степени 16)+(X в степени 12)+(X в степени 5)+1.
        //Полиному соответствует битовая маска 0x1021.
        //
        private int CRCode(string msg, int len)
        {
            int j;
            int intb;
            int crc = 0;
            string binaryX;
            byte[] asciicharacters = Encoding.ASCII.GetBytes(msg);
            foreach (byte b in asciicharacters)            
            {
                binaryX = Convert.ToString(b, 2); //returns in base 2
                intb = Convert.ToInt32(binaryX);
                crc = crc ^ (int)intb << 8;
                for (j = 0; j < 8; j++)
                {
                    //if (crc & 0x8000 != 0)
                    if ((crc & 0x8000) != 0)
                    {
                        
                        crc = (crc << 1) ^ 0x1021;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }

        
        /*
         * Функция подготавливает сообщения к передаче
         */
        private string MessagePrepare(string Command)
        {
            string strMessage = strHeader + strData.Replace("#", Command);
            //string lngCommand = CTerm.GetHexToDouble(command).ToString();
            //char[] s1 = strMessage.ToCharArray;            
            int intValue = CRCode(strMessage, strMessage.Length);

            byte[] intBytes = BitConverter.GetBytes(intValue);
            Array.Reverse(intBytes);
            byte[] result = intBytes;

            //int intValue = CRCode(Command, Command.Length);
            string dd = intBytes[0].ToString();
            string CRC1 = //Data_Hex_Asc(ref dd);
            
            dd = intBytes[4].ToString();
            string CRC2 = Data_Hex_Asc(ref dd);
            string strMessage1 = strMessage + CRC1 + CRC2;

            intValue = CRCode(strMessage1, strMessage1.Length);
            
            //strMessage = strSOH + strMessage + crc.ToString();
            return strMessage;
        }

       /* private String[] ByteSplit(string Sep, String StrData)
        {
            int idx = 0;
            String[] StrResult;
            string element = "";
            string info = "";
            for (id=0; id<(StrData.Length/2); id++)
            {                
                element = StrData.Substring(id * 2,2);
                info = info + element;
                if(element == "09")
                {
                    //StrResult[] = info;
                    info = "";
                    idx++;
                }
            }
            return StrResult;
        }*/
    }
}
