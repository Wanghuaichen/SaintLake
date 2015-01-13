using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaintX.Utility
{
    class ProjectSettings
    {
        public ProjectSettings()
        {
            MaxSampleCount = 96;
            StartGrid = 1;
        }
        public int MaxSampleCount { get; set; }

        public int StartGrid { get; set; }
    }
}
