using Saint.Setting;
using SaintX.Data;
using SaintX.Setting;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
        //TestSetting testSetting = new TestSetting();
        //private Utility.PhysicalSettings _physicalSettings = new PhysicalSettings();
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
            //UpdateProtocol();
            
        }

        public void UpdateProtocol()
        {
            //string panelType = GlobalVars.Instance.ScriptName;
            //string assayGroupSettingXml = FolderHelper.GetDataFolder() + string.Format("{0}.xml", panelType);
            //string protocolCSV = string.Format("{0}.csv", panelType);
            //if (File.Exists(assayGroupSettingXml))
            //{
            //    testSetting = SerializeHelper.Deserialize<TestSetting>(assayGroupSettingXml);
            //    protocolCSV = testSetting.ProtocolFileName;
            //}
            //else
            //{
            //    testSetting.Assays = GetDummyAssays();
            //    testSetting.ProtocolFileName = protocolCSV;
            //    SerializeHelper.Serialize(assayGroupSettingXml, testSetting);
            //}

            _protocol = Protocol.CreateFromCSVFile(FolderHelper.GetDataFolder() + GlobalVars.Instance.ScriptName + ".csv");
        }

        //public List<ColorfulAssay> Assays
        //{
        //    get 
        //    {
        //        return testSetting.Assays.ToList();
        //    }
        //    set
        //    {
        //        testSetting.Assays = new ObservableCollection<ColorfulAssay>(value);
        //    }
        //}

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

        //public PhysicalSettings PhysicalSettings
        //{
        //    get
        //    {
        //        return _physicalSettings;
        //    }
        //    set
        //    {
        //        _physicalSettings = value;
        //    }
        //}

        //private ObservableCollection<ColorfulAssay> GetDummyAssays()
        //{
        //    List<ColorfulAssay> assays = new List<ColorfulAssay>();
        //    assays.Add(new ColorfulAssay("HBV",Color.FromArgb(255,255,0,0)));
        //    assays.Add(new ColorfulAssay("HIV",Color.FromArgb(255,0,255,0)));
        //    assays.Add(new ColorfulAssay("HCV",Color.FromArgb(255,0,0,255)));
        //    return  new ObservableCollection<ColorfulAssay>(assays);
        //}
    }


    
}
