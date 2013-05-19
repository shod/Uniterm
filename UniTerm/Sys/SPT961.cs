using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace UniTerm.Sys
{

    class SPT961 : IDevice , IReport
    {
        private string ProtocolName = "SPT961";
        public int _TimeWait;
        public int _TimeDelay;
        public int _TermId;
        public CTerm.ResponseData ResData;
        ArrayList arrObjectCommand;
        ArrayList arrProtocolCommand;

        private string strSOH = "1001";
        private string strHeader = "101f";
        private string strSTX = "100209";
        private string strETX = "1003";
        private string strData = "";
        private string strCRC = "";
        private int _ProtocolIndex = 0;
        private CTerm.RequesParam _RequestParam;
        private DateTime _RequestDate;

        public int ProtocolIndex
        {
            get
            {
                return _ProtocolIndex;
            }
            set
            {
                _ProtocolIndex = value;
            }
        }

        public CTerm.RequesParam RequestParam
        {
            set
            {
                _RequestParam = value;
            }
        }

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
            String[] arrResPortion; // Массив ответа
            string FNC;
            DateTime dtRequest;
            String[] arrResponce = new string[0]; // Массив порций ответов
            int i_request = 1;

            /*
             * Для прохода по параметрам протокола
             */
            Config.SysObjectProtocol Sprotokol;            
            /* 
             * Получение массива объектов для опроса
             */
            arrObjectCommand = cConf.getObjectTypeCommand(ProtocolName);
            //arrProtocolCommand = cConf.getObjectProtocol(ProtocolName, _RequestParam.ProtocolIndex);                    

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
                    //sCommand = Sterm.Value;

                    /*
                     * Если указан индекс, который следует опрашивать
                     * Или индекс не указан
                     */
                    if (((_RequestParam.ProtocolIndex == 0) &&  (Sterm.Mode == "auto")) || (_RequestParam.ProtocolIndex == Sterm.Index))
                    {

                        String Command = Sterm.Value;
                        String[] arrCmd = Command.Split(';');

                        int c_cmd = 1;
                        if (_RequestParam.ReportType == "period_day")
                        {
                            c_cmd = 12;
                            i_request = 2;
                        }
                        else if (_RequestParam.ReportType == "period_month")
                        {
                            c_cmd = System.DateTime.DaysInMonth(_RequestParam.DateFrom.Year, _RequestParam.DateFrom.Month);
                            i_request = 2;
                        }

                        arrResponce = new string[c_cmd];

                        bool flag_cicle = true;
                        for (int iq = 0; iq < c_cmd && flag_cicle; iq++)
                        {

                            for (int iqr = 0; iqr < i_request; iqr++)
                            {
                                if (_RequestParam.ReportType == "period_day")
                                {

                                    dtRequest = new DateTime(_RequestParam.DateFrom.Year, _RequestParam.DateFrom.Month, _RequestParam.DateFrom.Day, iq, 1, 0);
                                    _RequestDate = dtRequest;
                                    /* Подготовка сообщения */
                                    sCommand = MessagePrepare(Sterm);

                                    /* Посылка сообщения */
                                    cPort.SendData(sCommand);
                                }
                                else if (_RequestParam.ReportType == "period_month")
                                {

                                    dtRequest = new DateTime(_RequestParam.DateFrom.Year, _RequestParam.DateFrom.Month, iq+1, 0, 1, 0);
                                    _RequestDate = dtRequest;
                                    /* Подготовка сообщения */
                                    sCommand = MessagePrepare(Sterm);

                                    /* Посылка сообщения */
                                    cPort.SendData(sCommand);
                                }
                                else
                                {
                                    /* Подготовка сообщения */
                                    sCommand = MessagePrepare(Sterm);

                                    /* Посылка сообщения */
                                    cPort.SendData(sCommand);
                                }
                                Thread.Sleep(_TimeDelay);

                                ResData.strData = cPort.ReturnData();

                                /* @TODO Временно */
                                //ResData.strData
                                arrResponce[iq] = "1001101f20100209300936353533320c0938093909323031310931320933330931370c093809390931310930093009300c093709390931310930093009300c09300c09302e350c0937382e380c09302e31313138380c09352e383037370c0931352e373433350c0932340c09300c09302e3030300c1003b832";
                                /**/

                                ResData.strDataOriginal = ResData.strData;

                                if (arrResponce[iq].Length >= 2)// && ia == (c_count - 1))
                                {
                                    ResData.IsError = false;
                                    //retData = ResData.strData.Substring(0, 2);
                                    //retData = Data_Hex_Asc(ref retData);
                                    cPort.Clear();
                                    break;
                                }
                                else
                                {
                                    ResData.IsError = true;
                                    ResData.strData = "0 - Приемник не подключен"; //0 - приёмник;                                                                        
                                }
                                cPort.Clear();
                            } // end for попытки

                            // Если ошибка, то прекращаем вообще запросы
                            if (ResData.IsError == true)
                            {
                                flag_cicle = false;
                                Exception myException = new Exception("SPT961 - Ошибка при получении данных.");                                
                                throw myException;
                                break;
                            }
                        } // end for запросов
                    }
                }
                
                /* 
                 * Проверка на готовность ответа порта
                 */
                ResData.IsError = false;
                ResData.strData = "";
                //if (ResData.IsError == false)
                String strResponceLine; // Одна порция данных
                String res;
                string ProtocolCommand;
                int ir = 0;
                while (ir < arrResponce.Length)
                {
                    
                    int pos;                    

                    strResponceLine = arrResponce[ir];
                    
                    //strResData = ResData.strData;

                    if (strResponceLine.Length > 2)
                    {
                        
                        res = strResponceLine.Replace(" ", "");//.Substring(4);                                                

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

                                /* Разделяем строку ответа на массивы 
                                 и вынимаем второй массив - он и есть данные ответа
                                 */
                                arrResData = Regex.Split(strResponceLine.ToUpper(), "0C");


                                //FNC = arrResData[0]

                                /* Цикл считывания параметров 
                                 // @TODO Сделать проверку на считываемый код FNC
                                 */

                                // Время показаний
                                if (_RequestParam.ReportType == "period_day")
                                {
                                    arrResPortion = Regex.Split(arrResData[1], "09");
                                    strResData = strResData + ";" + arrResPortion[4];
                                }
                                else if (_RequestParam.ReportType == "period_month")
                                {
                                    arrResPortion = Regex.Split(arrResData[1], "09");
                                    strResData = strResData + ";" + arrResPortion[1];
                                }
                                /*else
                                {
                                    arrResPortion = Regex.Split(arrResData[1], "09");
                                    strResData = strResData + ";" + arrResPortion[1] + "." + arrResPortion[2] + "." + arrResPortion[3] + " " + arrResPortion[1] + "." + arrResPortion[1];
                                }*/
                                

                                int d_count = arrResData.Length-1;
                                for (int di = 4; di < d_count; di++)
                                {
                                    arrResPortion = Regex.Split(arrResData[di], "09");
                                    strResData = strResData + ";" + arrResPortion[1];
                                }


                                arrInfoResp = Regex.Split(arrResData[1], "09");

                                //ResData.strData = arrInfoResp[1];
                                ResData.strHead = arrResData[0];
                                /* ПРоверка на весь пакет */
                                if (strResponceLine.Length == 0)
                                {
                                    ResData.IsError = true;
                                    ResData.strData = "3 - Данные со счетчика не поступают.";
                                }
                                else if (strResponceLine.Length < 4 * 2)
                                {
                                    ResData.IsError = true;
                                    ResData.strData = "4 - Данные искажены.";
                                }
                            }
                        }

                        //strResData = strResData + ResData.strData;
                    }
                    else
                    {
                        ResData.IsError = true;
                        //ResData.strData = "00";                        
                    }

                    ResData.strData = ResData.strData + strResData.Replace(" ", "") + "=";
                    strResData = "";

                    ir++;
                }
                ResData.strData=ResData.strData.TrimEnd('=');
                //ResData.strData.Substring(0,ResData.strData.Length-2);
                return;
                            
            }
            catch (Exception e)
            {
                ResData.IsError = true;
                ResData.strData = e.Message + ResData.strData;
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

        /*
         * converting a Hexadecimal String(hex string) to ASCII.
         */
        private string HexString2Ascii(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;           
        }
        #endregion //HexFunction
        ////////////////////////////////////////////////////////
        //Функция вычисляет и возвращает циклический код для
        //последовательности из len байтов, указанной *msg. 
        //Используется порождающий полином:
        //(X в степени 16)+(X в степени 12)+(X в степени 5)+1.
        //Полиному соответствует битовая маска 0x1021.
        //        
        int CRCode(string msg, int len)
        {            
            int crc = 0, i = 0;
            //len = len / 2;
            while (len-- > 0)
            {                
                crc = crc ^ (int)msg[i++] << 8;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (crc << 1) ^ 0x1021;
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }
        
        /*
         * Функция подготавливает сообщения к передаче
         */
        private string MessagePrepare(Config.SysObjectCommand Sterm)
        {
            string Command = Sterm.Value;
            String[] arrCmd = Command.Split(';');
            string strHexCommand = HexString2Ascii(arrCmd[1]);

            if (arrCmd[0] == "18")
            {
                strHexCommand = strHexCommand + "0c09" + GetDate() + "0c";
                strHexCommand = strHexCommand.ToUpper();
            }

            strHexCommand = strHexCommand.Replace("2E", "09");
            string strMessage = strHeader + arrCmd[0] + strSTX + strHexCommand + strETX;// strData.Replace("#", Command);
            //strMessage = "101f18100209300936353533320c0938093909323031310931320933330931370c1003";
            string strMessageConvert = strMessage.ToUpper();

            string sd = Data_Hex_Asc(ref strMessageConvert);
            
            //string lngCommand = CTerm.GetHexToDouble(command).ToString();
            //char[] s1 = strMessage.ToCharArray;            
            int intValue = CRCode(sd, sd.Length);
            //int intValue = CRCode(Command, Command.Length);
            

            byte[] intBytes = BitConverter.GetBytes(intValue);

            //Array.Reverse(intBytes);
            byte[] result = intBytes;
            
            int dd = intBytes[0];
            string CRC2 = dd.ToString("X2");         
            
            dd = intBytes[1];
            string CRC1 = dd.ToString("X2");
            strMessage = strMessage + CRC1 + CRC2;

            //intValue = CRCode(strMessage, strMessage.Length);
            //intBytes = BitConverter.GetBytes(intValue);
            strMessage = strSOH + strMessage;
            return strMessage;
        }

        private string GetDate()
        {
            string[] strDate = new string[6];
            DateTime dt;
            if (_RequestParam.ReportCode != null)// DateFrom.Day>0)
            {
                dt = _RequestDate;
            }else{
                dt = DateTime.Now;
            }

            strDate[0] = dt.Day.ToString();
            strDate[1] = dt.Month.ToString();
            strDate[2] = dt.Year.ToString();
            strDate[3] = dt.Hour.ToString();
            strDate[4] = dt.Minute.ToString();
            strDate[5] = dt.Second.ToString();
            string res = HexString2Ascii(String.Join(".", strDate));
            return res;
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
