using System;
using System.IO;
using System.Reflection;

namespace RestSharpDemo.Utilities
{
    public class LogWriter
    {
        private DirectoryInfo BaseLogFolder { get; set; }
        private DirectoryInfo CurLogFolder  { get; set; }
        private string CurrentTestName { get; set; }
        private string CurrentFileName { get; set; }

        private static void WriteCurMethodMessage(MethodBase method, Type classType, String messageStr)
        {
            Console.WriteLine(linkCurMethodMessage(method, classType, messageStr));
        }

        public static string linkCurMethodMessage(MethodBase method, Type classType, String messageStr)
        {
            return $"{method.Name}(): " + classType.ToString() + " - " + messageStr;
        }

        private void Constructor(DirectoryInfo baseLogFolder, string currentTestName, string currentFileName)
        {
            BaseLogFolder = baseLogFolder;
            CurrentTestName = currentTestName;
            CurrentFileName = currentFileName;

            CurLogFolder = Directory.CreateDirectory(Path.Combine(BaseLogFolder.FullName, CurrentTestName));
        }

        public LogWriter(DirectoryInfo curLogFolder, string currentTestName)
        {
            Constructor(curLogFolder, currentTestName, currentTestName);
        }
        public LogWriter(DirectoryInfo curLogFolder, string currentTestName, string currentFileName)
        {
            Constructor(curLogFolder, currentTestName, currentFileName);
        }

        public void LogWrite(string eventName, string logMessage)
        {
            try
            {
                var logFilePathName = Path.Combine(CurLogFolder.FullName, CurrentFileName + ".txt");
                using (StreamWriter w = File.AppendText(logFilePathName))
                {
                    Log(eventName, logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string eventName, string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("event: {0}", eventName);
                txtWriter.WriteLine("     : {0}", logMessage);
                txtWriter.WriteLine("-------------------------------------------------------");
            }
            catch (Exception ex)
            {
            }
        }

        public void FinalLogWrite()
        {
            LogWrite("================================================",
                     "================================================");
        }
    }

}
