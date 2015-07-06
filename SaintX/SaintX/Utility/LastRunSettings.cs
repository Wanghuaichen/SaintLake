using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natchs.Utility
{
    [Serializable]
    public class LastRunInfos
    {
        public LastRunInfos(string protocol,string assayName, int smpCnt, int finishedStep)
        {
            Protocol = protocol;
            AssayName = assayName;
            SampleCount = smpCnt;
            this.finishedSteps = finishedStep;
        }
        public LastRunInfos()
        {
        }

        int finishedSteps;


        public int FinishedSteps 
        { 
            get
            { 
                return finishedSteps;
            }
            set
            {
                finishedSteps = value;
            }
        }

        public string Protocol { get; set; }
        public string AssayName { get; set; }
        public int SampleCount { get; set; }
    }


}
