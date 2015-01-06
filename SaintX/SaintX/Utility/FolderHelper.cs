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

        static public string GetAssaysXml()
        {
            return GetDataFolder() + "assays.xml";
        }

        public static string GetImageFolder()
        {
            return GetExeParentFolder() + "Images\\";
        }

        internal static string GetProjectSettingsXml()
        {
            return GetDataFolder() + "projectSettings.xml";
        }
    }

  

}
