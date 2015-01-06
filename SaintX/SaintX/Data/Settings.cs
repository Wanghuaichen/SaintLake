using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SaintX.Data
{
    
    class ProjectSettings
    {
        /* 项目 溶液1 溶液2 溶液3 溶液4 PCR增强剂 PCRmix 内标 混匀      等待 
           内容 体积  体积  体积  体积   体积     体积  体积  转速&时间 时间
        */
        public string name;
        public List<Reagent> reagents;
        public MixParam mixParam;
        public int waitTimeSeconds;
        public ProjectSettings() 
        {

        }
    }
    
    class Reagent
    {
        public string label;
        public double volume;
    }

    class MixParam
    {
        public int speed;
        public int timeSeconds;
    }

}
