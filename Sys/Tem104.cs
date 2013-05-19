using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace UniTerm.Sys
{

    class TEM104 : IDevice, IReport
    {
        private string ProtocolName = "TEM104";
        public int _TimeWait;
        public int _TimeDelay;
        public int _TermId;
        public CTerm.ResponseData ResData;
        ArrayList arrObjectCommand;
        ArrayList arrProtocolCommand;

        private string strData = "";
        
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
            string sCommand;
            string _resData;
            string[] testData = {"AA00FF0F014015270313000011713F39B7380000110F3DC2C1A80000006A3E56B5A200D4926200D491FC00000066000000000000000000000000000019AA10F432320000000232",
            "AA00FF0F020845281503270313403B"};

            cPort.TimeWait = _TimeWait;

            /*
             * Для прохода по параметрам протокола
             */
            Config.SysObjectProtocol Sprotokol;
            /* 
             * Получение массива объектов для опроса
             */
            arrObjectCommand = cConf.getObjectTypeCommand(ProtocolName);

            cPort.CurrentDataMode = SPort.DataMode.Hex;
            /*
             * Проходим по набору комманд
             */
            cPort.Clear();
            if (!InitDevice(cPort))
            {
                Thread.Sleep(_TimeDelay);
                if (!InitDevice(cPort))
                {
                    Exception myException = new Exception("TEM104 - Ошибка при инициализации.");
                    throw myException;                    
                }
            }

            cPort.Clear();
            if (!TimeDevice(cPort))
            {
                Thread.Sleep(_TimeDelay);
                if (!TimeDevice(cPort))
                {
                    Exception myException = new Exception("TEM104 - Ошибка при получении последнего времени с устройства.");
                    throw myException;
                }
            }            

            Sterm = (Config.SysObjectCommand)arrObjectCommand[0];
            String[] arrCmd = Sterm.Value.Split(';');

            int c_count = arrCmd.Length;

            /*
            * Проход по кодам опроса
            */
            for (int ia = 0; ia < c_count; ia++)
            {
                cPort.Clear();
                /* Подготовка сообщения */
                sCommand = MessagePrepare(arrCmd[ia]);

                /* Посылка сообщения */
                cPort.SendData(sCommand);
                Thread.Sleep(_TimeDelay);

                ///!!@TODO Убрать после теста!!!!
                //_resData = testData[ia];
                _resData = cPort.ReturnData();
                if (!CheckMessage(_resData)){
                    cPort.Clear();
                    cPort.SendData(sCommand);
                    Thread.Sleep(_TimeDelay);
                }
                _resData = cPort.ReturnData();
                _resData = DecodeResponce(_resData, arrCmd[ia]);
                strData = strData + _resData;
            }

            ResData.strData = strData.Replace(",",".");

            /* 
             * Проверка на готовность ответа порта
             */
            ResData.IsError = false;            
        }

        /// <summary>
        /// Разбор ответа от прибора
        /// </summary>
        /// <returns></returns>
        private string DecodeResponce(string Package, string Command)
        {
            string Res = "";
            switch (Command)
            {
                case "0F0103014040": /// Данные текущие
                    Res = PackageDataCurrent(Package);
                    break;
                case "0F02020008": /// Время
                    Res = getDateTime(Package);
                    break;
                case "008040":
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return Res;
        }

        /// <summary>
        /// Разбирает пакет с текущими параметрами прибора
        /// </summary>
        /// <param name="Package">Пакет</param>
        /// <returns>Строка значений через запятую по типу протокола</returns>
        private string PackageDataCurrent(String Package)
        {
            string sep = ";";
            string ResValue = "";
            decimal decValue = 0;
            long longVal;
            float flotVal;
            string strValue;

            /// Отрезаем заголовок
            Package = Package.Substring(12);

            /// Объем, V
            longVal = HexToDouble(getParamValue(Package, "0004", 4));
            strValue = longVal.ToString();
            flotVal = HexString2Float(getParamValue(Package, "0008", 4));
            decValue = Convert.ToDecimal(longVal) + Convert.ToDecimal(flotVal);
            decValue = Math.Round(decValue, 2);
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            /// Масса, M
            longVal = HexToDouble(getParamValue(Package, "000C", 4));
            strValue = longVal.ToString();
            flotVal = HexString2Float(getParamValue(Package, "0010", 4));
            decValue = Convert.ToDecimal(longVal) + Convert.ToDecimal(flotVal);
            decValue = Math.Round(decValue, 2);
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            /// Энергия, Q, Гкал
            longVal = HexToDouble(getParamValue(Package, "0014", 4));
            strValue = longVal.ToString();
            flotVal = HexString2Float(getParamValue(Package, "0018", 4));
            decValue = Convert.ToDecimal(longVal) + Convert.ToDecimal(flotVal);
            decValue = Math.Round(decValue, 2);
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            /// Температура, T1, T2
            decValue = HexToDouble(getParamValue(Package, "0036", 2));
            decValue = decValue / 100;
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            decValue = HexToDouble(getParamValue(Package, "0038", 2));
            decValue = decValue / 100;
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            /// Давление, P1, МПа
            decValue = HexToDouble(getParamValue(Package, "003A", 1));
            decValue = decValue / 100;
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;

            decValue = HexToDouble(getParamValue(Package, "003B", 1));
            decValue = decValue / 100;
            strValue = decValue.ToString();
            ResValue = ResValue + sep + strValue;


            return ResValue;
        }

        /// <summary>
        /// Возвращает значение, запрашиваемого параметра
        /// </summary>
        /// <param name="Package">Пакет</param>
        /// <param name="addrHex">Адрес в пакете (Hex)</param>
        /// <param name="Len">Длина параметра</param>
        /// <returns>Значение</returns>
        private string getParamValue(String Package, string addrHex, int Len){
            String retD = "";
            int intPos = Convert.ToUInt16(HexToDouble(addrHex));
            //int pos = Package.IndexOf(intPos);
            retD = Package.Substring(intPos * 2 , Len * 2);
            //retD = retD.Substring(0, retD.Length);
            return retD;
        }

        /// <summary>
        /// Возвращает последнее время считывания данных
        /// </summary>
        /// <param name="Package"></param>
        /// <returns>Формат(Hour:Minute:Second)</returns>
        private string getDateTime(String Package)
        {
            string strRes;
            /// Отрезаем заголовок
            Package = Package.Substring(12);
            strRes = Package.Substring(4, 2) + ":" + Package.Substring(2, 2) + ":" + Package.Substring(0, 2);
            return strRes;
        }

        private bool TimeDevice(SPort cPort)
        {
            string resData;
            cPort.CurrentDataMode = SPort.DataMode.Hex;
            cPort.Clear();
            string setData = MessagePrepare("0F02020008");
            cPort.SendData(setData);

            Thread.Sleep(_TimeWait);
            //resData = "AA00FF0F020845281503270313403B";
            resData = cPort.ReturnData();

            if (CheckMessage(resData))
            {
                ResData.strTime = getDateTime(resData);
                return true;
            }
            else
            {
                return false;
            };
        }

        private bool InitDevice(SPort cPort)
        {
            
            cPort.CurrentDataMode = SPort.DataMode.Hex;
            cPort.Clear();
            cPort.SendData("5500FF000000AB");
            Thread.Sleep(_TimeWait);
            string strData = cPort.ReturnData();

            //return true;
            return CheckMessage(strData);

        }
        /*
         * Проверка сообщения по контрольной сумме
         */
        private bool CheckMessage(string Data)
        {
            Data = Data.Replace(" ", "");

            if (Data == "")
            {
                return false;
            }
            bool blnRes = false;
            char[] arrMsg = Data.ToCharArray();
            int cnt = arrMsg.Length;
            Int64 CS = 0;
            string strByte;
            string strLastCS = strByte = arrMsg[cnt - 2].ToString() + arrMsg[cnt - 1].ToString();
            for (int i = 0; i < cnt - 2; i++)
            {
                strByte = arrMsg[i].ToString()+arrMsg[++i].ToString();

                CS = CS + Convert.ToInt64(Data_Hex_Dec(ref strByte));
 
            }
            CS = ~CS;
            String hexValue = CS.ToString("X");
            arrMsg = hexValue.ToCharArray();
            cnt = arrMsg.Length;
            strByte = arrMsg[cnt-2].ToString()+arrMsg[cnt-1].ToString();

            if (strLastCS == strByte)
            {
                blnRes = true;
            }
            

            return blnRes;
        }

        /*
         * Подсчет контрольной суммы по сообщению
         */
        private String CRCode(string Data)
        {
            bool blnRes = false;
    
            char[] arrMsg = Data.ToCharArray();
            int cnt = arrMsg.Length;
            Int64 CS = 0;
            string strByte;
            
            for (int i = 0; i < cnt; i++)
            {
                strByte = arrMsg[i].ToString() + arrMsg[++i].ToString();

                CS = CS + Convert.ToInt64(Data_Hex_Dec(ref strByte));

            }
            CS = ~CS;
            String hexValue = CS.ToString("X");
            hexValue = hexValue.Substring(hexValue.Length - 2);

            return hexValue;
        }

        
        #region HexFunction

        /// <summary>
        /// Перевод из хекса в тип дабл
        /// </summary>
        /// <param name="strHex"></param>
        /// <returns></returns>
        public static long HexToDouble(string strHex)
        {
            string strHexRev = String.Empty;
            int cnt;

            cnt = strHex.Length / 2;


            for (int i = 0; i < cnt; i++)
                strHexRev = strHexRev + strHex.Substring(i * 2, 2);
            Int64 longHex = Int64.Parse(strHexRev, System.Globalization.NumberStyles.HexNumber);
            return longHex;
        }

        /// <summary>
        /// Hex to Float 
        /// </summary>
        /// <param name="strHex"></param>
        /// <returns></returns>
        private float HexString2Float(string strHex)
        {
            //41c9fd08
            //byte[] bytes = BitConverter.GetBytes(strHex);
            int alen = strHex.Length / 2;
            byte[] bytes = new byte[alen];
            for (int i = 0, j = 0; i < strHex.Length; i += 2, j++)
                bytes[alen - 1 - j] = Convert.ToByte(strHex.Substring(i, 2), 16);

            float myFload = BitConverter.ToSingle(bytes, 0);
            return myFload;
        }

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
        
        /// <summary>
        /// Функция подготавливает сообщения к передаче         
        /// </summary>
        /// <param name="Command">
        /// Command - комманда пакета. (0F0103014040)
        /// </param>
        /// <returns>Сообщение с CRC2 кодом</returns>
        private string MessagePrepare(string Command)
        {

            string strHeader = "5500FF";
            string strMessage = strHeader + Command;
           
            //string strMessageConvert = strMessage.ToUpper();
            //string sd = Data_Hex_Asc(ref strMessageConvert);

            string CRC2 = CRCode(strMessage);          
   
            strMessage = strMessage + CRC2;

            return strMessage;
        }

        private string GetDate()
        {
            string[] strDate = new string[6];
            DateTime dt;
            if (_RequestParam.DateFrom.Day > 0)
            {
                dt = _RequestDate;
            }
            else
            {
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

    }
}
