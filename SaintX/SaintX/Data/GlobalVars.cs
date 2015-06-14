using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaintX.Data
{
    class GlobalVars
    {
        private static GlobalVars _instance;
        private int _sampleCnt = 16;
        private string panelType = "DNA";

        public GlobalVars()
        {
            SampleLayoutSettings = new SampleLayoutSettings();
        }
        
        static public GlobalVars Instance
        {
            get
            {
                if(_instance == null )
                {
                    _instance = new GlobalVars();
                }
                return _instance;
            }
        }


        public string PanelType
        {
            get
            {
                return panelType;
            }
            set
            {
                panelType = value;
            }
        }

        public int SampleCount 
        { 
            get 
            { 
                return _sampleCnt;
            }
            set 
            { 
                _sampleCnt = value;
            }
        }

        public SampleLayoutSettings SampleLayoutSettings { get; set; }


        public Stage FarthestStage { get; set; }
    }
    
}
