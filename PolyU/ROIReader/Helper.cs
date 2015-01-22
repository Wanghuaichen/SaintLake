using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ROIReader
{
    public class Helper
    {
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        public static string GetOutputFolder()
        {
            string sOutputFolder = GetExeParentFolder() + "Output\\";

            if (!Directory.Exists(sOutputFolder))
            {
                Directory.CreateDirectory(sOutputFolder);
            }
            return sOutputFolder;
        }

        public static void WriteResult(bool bok)
        {
            string sOutputFolder = GetOutputFolder();
            string sResultFile = sOutputFolder + "result.txt";
            File.WriteAllText(sResultFile, bok.ToString());
        }

        public static void WriteErrorMsg(string sMessage)
        {
            string sOutputFolder = GetOutputFolder();
            string sResultFile = sOutputFolder + "errorMsg.txt";
            File.WriteAllText(sResultFile, sMessage);
        }

        internal static void WriteAvg(double avg)
        {
            string sOutputFolder = GetOutputFolder();
            string sAvgFile = sOutputFolder + "average.txt";
            File.WriteAllText(sAvgFile, avg.ToString());
        }

        internal static void WriteValue(List<string> values)
        {
            string sOutputFolder = GetOutputFolder();
            Console.WriteLine("write to: " + sOutputFolder + "values.txt");
            File.WriteAllLines(sOutputFolder + "values.txt",values);
        }
    }
}
