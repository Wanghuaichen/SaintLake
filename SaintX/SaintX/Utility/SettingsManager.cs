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
        ObservableCollection<ColorfulAssay> _assays = null;
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
            string assayXml = FolderHelper.GetAssaysXml();
            if(File.Exists(assayXml))
            {
                _assays = SerializationHelper.Deserialize<ObservableCollection<ColorfulAssay>>(assayXml);
            }

#if DEBUG
            _assays = GetDummyAssays();
            string protocolCSV = FolderHelper.GetDataFolder() + "Protocol 1.csv";
            _protocol = Protocol.CreateFromCSVFile(protocolCSV);
#else
            string protocolXml = FolderHelper.GetProtocolDefinitionXml();
            if (File.Exists(protocolXml))
            {
                _protocol = SerializationHelper.Deserialize<Protocol>(protocolXml);
            }
#endif       
        }

        public ObservableCollection<ColorfulAssay> Assays
        {
            get 
            {
                return _assays;
            }
            set
            {
                _assays = value;
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
            return new ObservableCollection<ColorfulAssay>(assays);
        }
    }
}
