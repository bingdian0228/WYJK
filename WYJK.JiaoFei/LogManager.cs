using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.JiaoFei
{
    public class LogManager
    {
        private string logFilePath = string.Empty;

        public LogManager(string logFilePath)
        {

            this.logFilePath = logFilePath;

            FileInfo file = new FileInfo(logFilePath);

            if (!file.Exists)
            {

                file.Create().Close();

            }

        }

        public void SaveLog(string message, DateTime writerTime)
        {

            string log = writerTime.ToString() + ":" + message;

            StreamWriter sw = new StreamWriter(logFilePath, true);

            sw.WriteLine(log);

            sw.Close();

        }
    }
}
