using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Natchs.Utility
{
    class PhysicalSettings
    {
        public PhysicalSettings()
        {
            MaxSampleCount = 96;
            StartGrid = 1;
        }
        public int MaxSampleCount { get; set; }
        public int StartGrid { get; set; }
    }
}
