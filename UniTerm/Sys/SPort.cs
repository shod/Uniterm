#region Namespace Inclusions
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
#endregion;

namespace UniTerm.Sys
{
    class SPort
    {
        #region Public Enumerations
        public struct ResponseData
        {
            public bool IsError;
            public String strData;
        }
        public enum DataMode { Text, Hex }
        private DataMode _DataMode;
        public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
        private String _resData = "";
        private bool _resError = false;
        #endregion

        #region Local Variables
        // Create the serial port with basic settings 
        private SerialPort comport = new SerialPort();
        
        #endregion

        #region Constructor               
        public SPort() 
        { 
            // Attach a method to be called when there
            // When data is recieved through the port, call this method
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //comport.ErrorReceived += new SerialErrorReceivedEventHandler(port_ErrorReceived);
            
        }
        #endregion

        public void Open()
        {
            Config cConf = new Config();

            // Set the port's settings
            comport.BaudRate = 4800;
            comport.DataBits = 8;
            comport.StopBits = StopBits.One;
            comport.Parity = Parity.None;
            comport.PortName = cConf.getappSettings(Config.SettingField.Port.ToString());
            comport.RtsEnable = true;
            comport.DtrEnable = true;
            //19.02.2012
            //comport.ReadTimeout = 500;
            //comport.WriteTimeout = 300;

            // Begin communications 
            comport.Open();
            // Enter an application loop to keep this thread alive 
        }

        public void Close()
        {
            if (comport.IsOpen)
            {
                comport.Close();
            }
        }

        public String ReturnData()
        {
            //return "C2C83232C8340495070000";
            return _resData;
        }

        public ResponseData PortQuery_()
        {
            ResponseData resData = new ResponseData();
            return resData;
        }

        public void SendData(string Message)
        {
            if (CurrentDataMode == DataMode.Text)
            {
                // Send the user's text straight out the port
                comport.Write(Message);
            }
            else
            {
                try
                {
                    // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
                    byte[] data = HexStringToByteArray(Message);

                    // Send the binary data out the port
                    comport.Write(data, 0, data.Length);
             
                }
                catch (FormatException)
                {
                    // Inform the user if the hex string was not properly formatted
                    //Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
                    throw new ArgumentException("Ошибка получения данных из порта" );
                }
            }
            Thread.Sleep(2);        
            
        }

        public void Clear()
        {
            //comport.DiscardOutBuffer();
            comport.DiscardInBuffer();
            _resData = "";
        }

        #region Local Properties
        public DataMode CurrentDataMode
        {
            get
            {
                if (_DataMode == DataMode.Hex) return DataMode.Hex;
                else return DataMode.Text;
            }
            set
            {
                if (value == DataMode.Text) _DataMode = DataMode.Text;
                else _DataMode = DataMode.Hex;
            }
        }
        #endregion

        /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
        /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
        /// <returns> Returns an array of bytes. </returns>
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");            
            byte[] buffer;
            if (s.Length < 2)
            {
                buffer = new byte[1];
                byte bb = (byte)Convert.ToByte(s, 16);
                buffer[0] = bb; 
            }
            else
            {
                buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }
        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        #region Event Handlers

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // This method will be called when there is data waiting in the port's buffer
            
            //Thread.Sleep(1);
            if (comport.IsOpen == true)
            {

                // Determain which mode (string or binary) the user is in
                if (CurrentDataMode == DataMode.Text)
                {
                    // Read all the data waiting in the buffer
                    string data = comport.ReadExisting();

                    // Display the text to the user in the terminal
                    //Log(LogMsgType.Incoming, data);
                    _resData = data;
                }
                else
                {
                    // Obtain the number of bytes waiting in the port's buffer
                    int bytes = comport.BytesToRead;

                    // Create a byte array buffer to hold the incoming data
                    byte[] buffer = new byte[bytes];

                    // Read the data from the port and store it in our buffer
                    comport.Read(buffer, 0, bytes);

                    // Show the user the incoming data in hex format
                    //Log(LogMsgType.Incoming, ByteArrayToHexString(buffer));
                    _resData = _resData + ByteArrayToHexString(buffer);                    
                }
            }
        }

        //private void port_ErrorReceived(object sender, SerialErrorReceivedEventHandler e)
        //{
        //    _resData = "";
        //    _resError = true;
        //}

        //public bool isError()
        //{
        //    return _resError;
        //}
        #endregion
    }
}
