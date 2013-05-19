using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;

namespace UniTerm.Sys
{
    class Tem05 : IDevice
    {
        public int _TimeWait;
        public int _TimeDelay;
        public int _TermId;

        public enum Parameter
        {
            RK1,
            KB1
        }

        /// <summary>
        /// Структура параметров выставления точности показаний
        /// </summary>
        private struct CommaSet
        {
            public string Code_HEX;
            public int Dmm;
            public int G;
            public decimal Gmax;
        }


        public CTerm.ResponseData ResData;

        /// <summary>
        /// Вставляет разделитель
        /// </summary>
        /// <param name="CodePos">Код позиции диаметра переведенный из HEX</param>
        /// <param name="ParamCode">Параметр протокола</param>
        /// <param name="Value">Значение из протокола данных</param>
        /// <returns></returns>
        public string GetStrComma(int CodePos, string ParamCode, String Value)
        {
            int pos = 0;
            string ResValue;

            if (ParamCode == "TPD" || ParamCode == "TOB")
            {
                pos = 2;
            }
            else if (ParamCode == "TVD")
            {
                pos = 2;
            }
            else
            {
                int[] CTable = CommaTable(ParamCode);
                pos = CTable[CodePos];
            }
            if ((Value.Length - pos) <= 0)
            {
                ResValue = Value.Insert(0, ".");
            }
            else
            {
                ResValue = Value.Insert(Value.Length - pos, ".");
            }
            return ResValue;
        }

        /// <summary>
        /// Получение позиции для запятой 
        /// </summary>
        /// <param name="ParamCode"></param>
        /// <returns></returns>
        private int[] CommaTable(string ParamCode)
        {
            
            int[] arrVal;
            arrVal = new int[] { 0 };

            switch (ParamCode)
            {
                case "RK1":
                    arrVal = new int[24]{5,5,4,4,4,4,4,4,3,3,3,3,3,3,2,3,2,2,2,2,2,4,3,3};
                    break;
                case "KB1":
                    arrVal = new int[24]{3,3,2,2,2,2,2,2,1,1,1,1,1,1,0,1,0,0,0,0,0,2,1,1};
                    break;
                case "EM1":
                    arrVal = new int[24]{4,4,3,3,3,3,3,3,2,2,2,2,2,2,1,2,1,1,1,1,1,3,2,2};
                    break;
                case "VK1":
                    arrVal = new int[24]{3,3,2,2,2,2,2,2,1,1,1,1,1,1,0,1,0,0,0,0,0,2,1,1};
                    break;
                case "MKP1":
                    arrVal = new int[24]{3,3,2,2,2,2,2,2,1,1,1,1,1,1,0,1,0,0,0,0,0,2,1,1};
                    break;
                case "RK2":
                    arrVal = new int[24] { 5, 5, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 2, 3, 2, 2, 2, 2, 2, 4, 3, 3 };
                    break;
                case "KB2":
                    arrVal = new int[24] { 3, 4, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 0, 0, 0, 0, 0, 2, 1, 1 };
                    break;
                case "EM2":
                    arrVal = new int[24] { 4, 4, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 1, 2, 1, 1, 1, 1, 1, 3, 2, 2 };
                    break;
                case "VK2":
                    arrVal = new int[24] { 3, 3, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 2, 1, 1 };
                    break;
                case "MKP2":
                    arrVal = new int[24] { 3, 3, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 2, 1, 1 };
                    break;
            }
            return arrVal;
        }

        /// <summary>
        /// Обновляет, опрашивает состояние и получает данные с терминала 
        /// </summary>
        public void Refresh(SPort cPort)
        {
            string strCheck = ">";
            string retData = "";
            try
            {
                cPort.Clear();
                cPort.CurrentDataMode = SPort.DataMode.Hex;
                cPort.SendData("64");
                Thread.Sleep(_TimeDelay);
                ResData.strData = cPort.ReturnData();

                if (ResData.strData.Length >= 2)
                {
                    retData = ResData.strData.Substring(0, 2);
                    //char c = (char)Convert.ToInt32(retData, 16);
                    retData = Data_Hex_Asc(ref retData);
                }
                else
                {
                    ResData.IsError = true;
                    ResData.strData = "0 - Приемник не подключен"; //0 - прикмник;
                }

                /* 
                 * Проверка на готовность ответа порта
                 */
                if (retData == strCheck)
                {
                    cPort.SendData(_TermId.ToString("x"));
                    // TODO отключил - проверить
                    Thread.Sleep(_TimeWait);

                    ResData.IsError = false;

                    retData = cPort.ReturnData().Substring(0, 2);

                    if (retData == "3E")
                    {
                        String res = cPort.ReturnData();
                        res = res.Replace(" ", "");//.Substring(4);                                                

                        if (res.Length <= 2)
                        {
                            ResData.IsError = true;
                            ResData.strData = "1 - со счетчиком связь отсутствует";
                        }
                        else if (res.Substring(2, 2) == "3E")
                        {
                            ResData.IsError = true;
                            ResData.strData = "2 - Удаленный терминал не отвечает;";
                        }
                        else
                        {
                            _TimeWait = Convert.ToInt16(res.Substring(2, 2)) * 500;
                            res = res.Substring(4);
                            if (res.Substring(0, 2) == "FF")
                            {
                                ResData.IsError = true;
                                ResData.strData = "1 - Связь со счетчиком отсутствует.";
                            }
                            else
                            {
                                /* Вычисление позиции старта данных */
                                long var1;
                                byte var2 = 7;
                                byte var3 = Convert.ToByte(res.Substring(1, 1));
                                var1 = (var3 & var2) + (byte)5;
                                int start = Convert.ToInt16(var1);
                                ResData.strData = res.Substring(start * 2);
                                ResData.strHead = res.Substring(0, start * 2);
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

                    }
                    else if (retData.Substring(0, 2) == "00")
                    {
                        ResData.IsError = true;
                        ResData.strData = "00";
                    }
                    return;

                }
            }
            catch (Exception e)
            {
                ResData.IsError = true;
            }
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
    }

}
