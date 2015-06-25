using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaintX.Utility
{
    public class FolderHelper
    {
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public void WriteResult(bool bok)
        {
            string file = GetOutputFolder() + "result.txt";
            File.WriteAllText(file, bok.ToString());
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

        static public string GetDataFolder()
        {
            return GetExeParentFolder() + "Data\\";
        }

        public static string GetImageFolder()
        {
            return GetExeParentFolder() + "Images\\";
        }

        internal static string GetProtocolDefinitionXml()
        {
            return GetDataFolder() + "protocol1.xml";
        }

        internal static void WriteVariable(string file, string s)
        {
            string filePath = GetOutputFolder() + file + ".txt";
            File.WriteAllText(filePath, s);
        }
    }

  

}
