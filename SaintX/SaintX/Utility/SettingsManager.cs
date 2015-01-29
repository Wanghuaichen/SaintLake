using Saint.TestSetting;
using SaintX.Data;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SaintX.Utility
{
    class SettingsManager
    {
        static SettingsManager _settingsManager = null;
        Protocol _protocol = null;
        TestSetting testSetting = new TestSetting();
        private Utility.PhysicalSettings _physicalSettings = new PhysicalSettings();
        static public SettingsManager Instance
        {
            get
            {
                if(_settingsManager == null)
                {
                    _settingsManager = new SettingsManager();
                }
                return _settingsManager;
            }
        }

        private SettingsManager()
        {
            string assayGroupSettingXml = FolderHelper.GetAssayGroupSettingXml();
            string protocolCSV =  "Protocol 1.csv";
            if (File.Exists(assayGroupSettingXml))
            {
                testSetting = SerializationHelper.Deserialize<TestSetting>(assayGroupSettingXml);
                protocolCSV = testSetting.ProtocolFileName;
            }
            else
            {
                testSetting.Assays = GetDummyAssays();
                testSetting.ProtocolFileName = "Protocol 1.csv";
                SerializationHelper.Serialize(assayGroupSettingXml, testSetting);
            }
            _protocol = Protocol.CreateFromCSVFile(FolderHelper.GetDataFolder() + protocolCSV);
        }

        public List<ColorfulAssay> Assays
        {
            get 
            {
                return testSetting.Assays.ToList();
            }
            set
            {
                testSetting.Assays = new ObservableCollection<ColorfulAssay>(value);
            }
        }

        public Protocol Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                _protocol = value;
            }
        }

        public PhysicalSettings PhysicalSettings
        {
            get
            {
                return _physicalSettings;
            }
            set
            {
                _physicalSettings = value;
            }
        }

        private ObservableCollection<ColorfulAssay> GetDummyAssays()
        {
            List<ColorfulAssay> assays = new List<ColorfulAssay>();
            assays.Add(new ColorfulAssay("HBV",Color.FromArgb(255,255,0,0)));
            assays.Add(new ColorfulAssay("HIV",Color.FromArgb(255,0,255,0)));
            assays.Add(new ColorfulAssay("HCV",Color.FromArgb(255,0,0,255)));
            return  new ObservableCollection<ColorfulAssay>(assays);
        }
    }


    
}
