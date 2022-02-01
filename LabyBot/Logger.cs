using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabyBot
{
    static class Logger
    {
        static readonly string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static void Log(string Message)
        {
            if (!File.Exists(docPath + "/LabyLogs.txt"))
            {
                StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
            }
            TextWriter tw = new StreamWriter(docPath + "/LabyLogs.txt", true);
            tw.WriteLine(DateTime.Now.ToString() + " | " + Message);
            tw.Close();
        }
    }
}
