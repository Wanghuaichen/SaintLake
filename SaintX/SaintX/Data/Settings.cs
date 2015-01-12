using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SaintX.Data
{
    
    class Protocol
    {

    }
    class StepDefinition
    {
        public string Description { get; set; }
        public string SourceLabware { get; set; }
        public string DestLabware { get; set; }
        public double Volume { get; set; }
    }

    class MixParam
    {
        public int speed;
        public int timeSeconds;
    }

}
