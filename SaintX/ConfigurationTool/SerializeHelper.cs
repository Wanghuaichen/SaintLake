using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ConfigurationTool
{
    class SerializeHelper
    {
        public static T Deserialize<T>(string xmlFileName)
        {
            if (!File.Exists(xmlFileName))
            {
                string errorMessage = string.Format("文件不存在", xmlFileName);
                throw new ArgumentException(errorMessage, "xmlFileName");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (Stream fs = new FileStream(xmlFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;

                using (XmlReader reader = XmlReader.Create(fs, settings))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }
    }
}
