using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDBarcodes
{
    class GlobalVars
    {
        private static GlobalVars _instance;
        private int _sampleCnt = 16;
        private Dictionary<CellPosition, string> pos_Barcodes = new Dictionary<CellPosition, string>();
        private int startGrid = 0;


        private GlobalVars()
        {
            startGrid = int.Parse(ConfigurationManager.AppSettings["startGrid"]);
            MaxSampleCount = int.Parse(ConfigurationManager.AppSettings["maxSampleCount"]);
        }

        static public GlobalVars Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalVars();
                   
                }
                return _instance;
            }
        }

        public int StartGrid
        {
            get
            {
                return startGrid;
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

        public Dictionary<CellPosition, string> BarcodeSetting
        {
            get
            {
                return pos_Barcodes;
            }
        }

        public int MaxSampleCount { get; set; }
    }

    public struct CellPosition
    {
        public int colIndex;
        public int rowIndex;
        public CellPosition(int ix, int iy)
        {
            colIndex = ix;
            rowIndex = iy;
        }

        public CellPosition(int wellIndex)
        {
            colIndex = wellIndex / 16;
            rowIndex = wellIndex - 16 * colIndex;
        }

        static public string GetDescription(CellPosition cellPosition)
        {
            int gridStartPos = GlobalVars.Instance.StartGrid;
            return string.Format("[条{0}行{1}]", gridStartPos + cellPosition.colIndex, cellPosition.rowIndex + 1);
        }
    }
}
