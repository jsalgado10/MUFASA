using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace MATH3338
{
    class Log
    {
        #region Variables and Instantiations

        BackgroundWorker logBW;
        private string fileNameLog;
        StreamWriter log;

        #endregion

        #region Properties

        public string logFile
        {
            set
            {
                fileNameLog = value;
            }
            get
            {
                return fileNameLog;
            }
        }

        #endregion

        #region Constructors

        public Log(object sender,string logFile)
        {
            logBW = sender as BackgroundWorker;
            fileNameLog = logFile;
        }

        #endregion

        #region Methods

        public void debugWrite(string msg)
        {
            string logPath = Directory.GetCurrentDirectory().ToString() + "\\" + fileNameLog;
            string logmsg = msg;

            using (log = new StreamWriter(logPath, true))
            {
                if (!File.Exists(logPath))
                {
                    File.Create(logPath);
                }

                //bwLog.ReportProgress(0, msg);                
                log.WriteLine(string.Format("{0:MM/dd/yyy hh:mm:ss.fff tt} - {1}", DateTime.Now, msg));
                
            }
        }
        #endregion
    }
}
