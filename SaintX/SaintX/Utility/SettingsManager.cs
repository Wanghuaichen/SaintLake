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
        private Utility.ProjectSettings _projectSettings;
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
            string projectSettingsXml = FolderHelper.GetProjectSettingsXml();
            if(File.Exists(projectSettingsXml))
            {
                _protocol = SerializationHelper.Deserialize<Protocol>(projectSettingsXml);
            }
            
            #if DEBUG
            _assays = GetDummyAssays();
            _protocol = new Protocol();
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

        public ProjectSettings ProjectSettings
        {
            get
            {
                return _projectSettings;
            }
            set
            {
                _projectSettings = value;
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
