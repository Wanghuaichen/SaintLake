using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace SaintX.Data
{
    class Protocol
    {
        public string Name { get; set; }
        public List<StepDefinition> StepsDefinition { get; set; }
        public Protocol(string name, List<StepDefinition> definitions)
        {
            Name = name;
            StepsDefinition = definitions;
        }
        public Protocol()
        {

        }
        static public Protocol  CreateFromCSVFile(string csvFile)
        {
            int commaCnt = 4;
            FileInfo fileInfo = new FileInfo(csvFile);
            string name = fileInfo.Name;
            string[] strLines = File.ReadAllLines(csvFile);
            strLines = strLines.Where(x => x != "").ToArray();
            List<StepDefinition> stepDefinitions = new List<StepDefinition>();
            for (int i = 1; i < strLines.Length; i++ )
            {
                string sLine = strLines[i];
                if (sLine.Count(x => x == ',') != commaCnt)
                {
                    throw new Exception("CSV文件格式非法！");
                }
                string[] lineContents = sLine.Split(',');
                StepDefinition stepDefinition = new StepDefinition(lineContents,i);
                stepDefinitions.Add(stepDefinition);
            }
            return new Protocol(name, stepDefinitions);
        }
    }

    enum StepDefCol
    {
        Description = 0,
        SourceLabware = 1,
        Volume = 2,
        DestLabware = 3,
        RepeatTimes = 4,
        TipType = 5,
        ReuseTimes = 6,
        AspiratePosition = 7,
        DispensePosition = 8
    }

    class StepDefinition
    {
        public string Description { get; set; }
        public string SourceLabware { get; set; }
        public string DestLabware { get; set; }
        public string Volume { get; set; }
        public string RepeatTimes { get; set; }
        public string TipType { get; set; }
        public int ReuseTimes { get; set; }
        public string AspirateConstrain { get; set; }
        public string DispenseConstrain { get; set; }
        public int LineNumber { get; set; }

        public StepDefinition()
        {

        }
        public StepDefinition(string[] lines,int no)
        {
            Description = lines[(int)StepDefCol.Description];
            SourceLabware = lines[(int)StepDefCol.SourceLabware];
            Volume = lines[(int)StepDefCol.Volume];
            DestLabware = lines[(int)StepDefCol.DestLabware];
            RepeatTimes = lines[(int)StepDefCol.RepeatTimes];
            TipType = lines[(int)StepDefCol.TipType];
            ReuseTimes = int.Parse(lines[(int)StepDefCol.ReuseTimes]);
            AspirateConstrain = lines[(int)StepDefCol.AspiratePosition];
            DispenseConstrain = lines[(int)StepDefCol.DispensePosition];
            LineNumber = no;
        }
    }

    class StepDefinitionWithProgressInfo : StepDefinition
    {
        public bool IsWorking { get; set; }
        public bool IsFinished { get; set; }
        public StepDefinitionWithProgressInfo(StepDefinition stepDef)
        {
            Description = stepDef.Description;
            SourceLabware = stepDef.SourceLabware;
            Volume = stepDef.Volume;
            DestLabware = stepDef.DestLabware;
            RepeatTimes = stepDef.RepeatTimes;
            LineNumber = stepDef.LineNumber;
        }

    }
}
