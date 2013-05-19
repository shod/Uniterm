using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace UniTerm.Sys
{
    
    class CFile
    {
        public string fileName;
        private List<String> arrLine = new List<String>();

        public CFile()
        {

        }

        public CFile(string FileName)
        {
            fileName = FileName;
        }

        public void Write(bool flagAdd)
        {
            _Write(flagAdd);
        }

        public void Write()
        {
            _Write(true);
        }

        private void _Write(bool flagAdd)        
        {
        
            //create a TextWriter then open the file

            TextWriter writer = new StreamWriter(fileName, flagAdd);
            
            //now write the date to the file            
            arrLine.ForEach(delegate (String line)
            {
                writer.WriteLine(line);    
            });
            
            //close the writer            
            if (arrLine.Count > 0)
            {
                writer.Close();
            }
            else
            {
                writer.Dispose();
            }
        
        }

        public void AddLine(string strLine)
        {
            arrLine.Add(strLine);
        }

        public List<String> Read()
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(fileName))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    AddLine(line);
                }
            }

            return arrLine;
        }

    }
}
