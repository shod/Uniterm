using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO.Ports;
//using System.Threading;
using System.Globalization;
using UniTerm.Sys;

namespace UniTerm.Sys
{
    interface IDevice
    {
        void Refresh(SPort cPort);        
    }
    
    interface IReport
    {
        int ProtocolIndex
        {
            get;
            set;
        }
    }

    class CTerm
    {
        public struct ResponseData
        {
            public bool IsError;
            public String strData;
            public String strHead;
            public String strDataOriginal;
            public String strTime;
        }

        /// <summary>
        /// Структура для установки параметров опроса 
        /// </summary>
        public struct RequesParam
        {
            public int ProtocolIndex;
            public DateTime DateFrom;
            public DateTime DateTo;
            public String ReportType;
            public String ReportCode;
        }
        ResponseData ResData = new ResponseData();
        RequesParam _RequestParam;

        string _TermType;
        int _TermId;
        ArrayList arrElement;
        ArrayList arrProtocolElement;
        int _TimeWait;
        int _TimeDelay;
        int CodeCommaPos1 = 8;
        int CodeCommaPos2 = 0;
        private int _TermProtocolIndex = 0; // Индекс протокола для терминала
        
        /* Ошибки */
        const string errNoWork = "00";

        public int TermProtocolIndex
        {
            set
            {
                _TermProtocolIndex = value;
            }
            get
            {
                return _TermProtocolIndex;
            }

        }

        public CTerm.RequesParam RequestParam
        {
            set
            {
                _RequestParam = value;
            }
        }

        public CTerm(int TermId,string TermType)
        {            
            _TermType = TermType;
            _TermId = TermId;
            Config cConf = new Config();
            Config.SysObjectProtocol sProtocol;
            //arrProtocolElement = cConf.getObjectProtocol(TermType);
            _TimeWait = Convert.ToInt16(cConf.getappSettings(Config.SettingField.TimeWait.ToString()));
            _TimeDelay = Convert.ToInt16(cConf.getappSettings(Config.SettingField.TimeDelay.ToString()));
        }

       /// <summary>
        /// Обновляет, опрашивает состояние и получает данные с терминала
        /// Тип терминала задается в конструкторе класса переменной _TermType
        /// </summary>

        public void Refresh(SPort cPort)
        {

            //IDevice cDevice = null;
            switch (_TermType)
            {
                case "IMP01":
                case "TEM05M":
                    Tem05 cTem05 = new Tem05();
                    //cDevice.Refresh(cPort);
                    cTem05._TimeDelay = _TimeDelay;
                    cTem05._TimeWait = _TimeWait;
                    cTem05._TermId = _TermId;

                    cTem05.Refresh(cPort);
                    ResData = cTem05.ResData;
                    break;
                case "SPT961":
                    SPT961 cSPT961 = new SPT961();
                    cSPT961._TimeDelay = _TimeDelay;
                    cSPT961._TimeWait = _TimeWait;
                    cSPT961._TermId = _TermId;
                    cSPT961.RequestParam = _RequestParam;
                    cSPT961.Refresh(cPort);
                    ResData = cSPT961.ResData;
                    break;
                case "TEM104":
                    TEM104 cTEM104 = new TEM104();
                    cTEM104._TimeDelay = _TimeDelay;
                    cTEM104._TimeWait = _TimeWait;
                    cTEM104._TermId = _TermId;
                    cTEM104.RequestParam = _RequestParam;
                    cTEM104.Refresh(cPort);
                    ResData = cTEM104.ResData;
                    break;
                default:
                    throw new ArgumentException("Терминала типа '" + _TermType+"' в системе не описано.");
                    break;
            }
            
        }

        #region Converter

        public string Data_String_Ascii(string Data)
        {

            byte[] asciicharacters = Encoding.ASCII.GetBytes(Data);
            return ByteArrayToHexString(asciicharacters);
            
        }

        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        public static long GetHexToDouble(string strHex)
        {
            string strHexRev = String.Empty;
            int cnt;
     
                cnt = strHex.Length / 2;
     

            for (int i = 0; i < cnt; i++)
                strHexRev = strHex.Substring(i * 2, 2)+strHexRev;
            Int64 longHex = Int64.Parse(strHexRev, System.Globalization.NumberStyles.HexNumber);
            return longHex;
            //return BitConverter.ToInt64(longHex,);
            //.Int64BitsToDouble(longHex);
        }

        public static string Data_Hex_Asc(ref string Data)
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
        #endregion

        /// <summary>
        /// Парсинг данных - это позже
        /// </summary>
        /// <returns></returns>
        public void DataParsing()
        {

            string sep = ";";
            string strItem;
            String retData;
            //_TermProtocolIndex вместо  "1" 
            retData = _TermId.ToString() + sep + _TermProtocolIndex + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            //Config.SysObjectProtocol Protokol; 
            
                foreach (Config.SysObjectProtocol Protocol in arrProtocolElement)
                {
                    strItem = ResData.strData.Substring(Protocol.Index, 8);
                    retData = retData + sep + Data_Hex_Dec(ref strItem);
                }
            
            //return "1;1;21:21:08;49;0;0;0;1;11;0;0;4;41;.22955;1.995;16.5179;667.859;755.027;27.147;434.3;2732.36;97429.3;44310.2;0;84.44;28.86;13.21";
            
        }

        /// <summary>
        /// Возвращает строку данных 
        /// </summary>
        /// <returns></returns>
        public String getDataString()
        {
            string sep = ";";
            string sepArray = "";
            int intStart;
            int intStartHead;
            String strItem;
            String strData, currVal = "";
            /*Это заголовок*/
            String strDataHead;
            String retData, strStatus = "49";
            Tem05 cTem05 = new Tem05();
            SPT961 cSpt961 = new SPT961();
            int Code_D1 = 0;
            int Code_D2 = 0;
            string sResVal = "";
            String[] arrItem;
            Config cConf = new Config();
            string strTime;

            arrProtocolElement = cConf.getObjectProtocol(_TermType, _TermProtocolIndex);

            String strAdd = "";
            strTime = ResData.strTime;

            if (strTime == null)
            {
                strTime = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            }
            retData = _TermId.ToString() + sep + _RequestParam.ProtocolIndex + sep + strTime;
            
            if (_RequestParam.ReportType != null)
            {
                sepArray = "=";
                sep = ",";
                retData = "";
            }

            //Config.SysObjectProtocol Protokol; 
            try
            {
                
                //strData = ResData.strData;
                strDataHead = ResData.strHead;


                String[] arrDataLine = ResData.strData.Split(sepArray.ToCharArray());            

                foreach (string DataLine in arrDataLine)
                {
                    strData = DataLine;
                    strItem = "";
                    foreach (Config.SysObjectProtocolMap Protocol in arrProtocolElement)
                    {
                        /*Если данные*/
                        if (Protocol.Index > 0)
                        {
                            if (_TermType == "TEM05M")
                            {
                                intStart = Convert.ToInt16(Convert.ToInt32(Protocol.Pos, 16));
                                //intStart = Convert.ToInt16(Convert.ToInt32(intStart, 16)) * 2;
                                intStart = intStart * 2;


                                /*if (intStart > 1)
                                    intStart = intStart + 2;
                                */
                                strItem = strData.Substring(intStart, Protocol.Len * 2);
                                strAdd = strAdd + strItem;
                                sResVal = (GetHexToDouble(strItem).ToString());
                            }
                            else if (_TermType == "SPT961")
                            {
                                intStart = Convert.ToInt16(Protocol.Pos);
                                arrItem = strData.Split(';');
                                strItem = arrItem[intStart];

                                sResVal = Data_Hex_Asc(ref strItem);
                                //sResVal = cSpt961.getConvertHexToString(strItem);
                                //sResVal = (GetHexToDouble(strItem).ToString());
                            }
                            else if (_TermType == "TEM104")
                            {
                                intStart = Convert.ToInt16(Protocol.Pos);
                                arrItem = strData.Split(';');
                                strItem = arrItem[intStart];

                                sResVal = strItem;                                
                            }
                            else
                            {
                                sResVal = (GetHexToDouble(strItem).ToString());
                            }
                        }
                        else
                        {
                            intStart = Convert.ToInt16(Convert.ToInt32(Protocol.Pos, 16));
                            intStart = intStart * 2;

                            strItem = strDataHead.Substring(intStart, Protocol.Len * 2);
                            strAdd = strAdd + strItem;

                            sResVal = (GetHexToDouble(strItem).ToString());
                        }

                        /* Значения кода для выставления запятой */
                        if (Protocol.Name == "DR1") // Первый канал
                        {
                            Code_D1 = Convert.ToInt16(sResVal);

                        }

                        if (Protocol.Name == "DR2") // Второй канал
                        {
                            Code_D2 = Convert.ToInt16(sResVal);

                        }

                        if (Protocol.Comma == 1)
                        {
                            sResVal = cTem05.GetStrComma(Code_D1, Protocol.Name, sResVal);
                        }
                        if (Protocol.Comma == 2)
                        {
                            sResVal = cTem05.GetStrComma(Code_D2, Protocol.Name, sResVal);
                        }

                        /*@TODO дописать в общем виде*/
                        /* ДАнные из заголовока*/
                        if (Protocol.Name == "DOR")
                        {
                            string binaryval = "00" + Convert.ToString(Convert.ToInt32(strItem, 16), 2);
                            sResVal = binaryval.Substring(Protocol.Comma + 1, 1);
                        }
                        /* ДАнные из заголовока*/
                        if (Protocol.Name == "WTR")
                        {
                            string binaryval = "00" + Convert.ToString(Convert.ToInt32(strItem, 16), 2);
                            sResVal = binaryval.Substring(Protocol.Comma + 1, 1);
                        }
                        currVal = currVal + sep + sResVal;
                    }
                    retData = retData + sep + strStatus + currVal + sepArray;
                    currVal = "";
                }
            }
            catch(Exception e)
            {
                strStatus = "4";
            }
            //return "1;1;21:21:08;49;0;0;0;1;11;0;0;4;41;.22955;1.995;16.5179;667.859;755.027;27.147;434.3;2732.36;97429.3;44310.2;0;84.44;28.86;13.21";
            return retData ;//= retData + sep + strStatus  + currVal;
        }

        /// <summary>
        /// Возвращает оригинальный ответ от терминала
        /// </summary>
        /// <returns></returns>
        public String getDataResponseHex()
        {
            if (!String.IsNullOrEmpty(ResData.strDataOriginal) && ResData.strDataOriginal.Length > 0)
            {
                return ResData.strDataOriginal;
            }
            else
            {
                return ResData.strData;
            }
        }

        /// <summary>
        /// Возвращает есть ли ошибка при приеме данных 
        /// </summary>
        /// <returns></returns>
        public bool isDataError()
        {
            return ResData.IsError;
        }
    }
}
