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
        const char comma = ',';
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
            int commaCnt = 12;
            
            FileInfo fileInfo = new FileInfo(csvFile);
            string name = fileInfo.Name;
            string[] strLines = File.ReadAllLines(csvFile);
            strLines = strLines.Where(x => x != "").ToArray();
            List<StepDefinition> stepDefinitions = new List<StepDefinition>();
            for (int i = 1; i < strLines.Length; i++ )
            {
                string sLine = strLines[i];
                string sPreAction = "";
                string sPostAction = "";
                ExtractPrePostActionString(ref sLine, ref sPreAction, ref sPostAction);
                if (sLine.Count(x => x == comma) != commaCnt)
                {
                    throw new Exception("CSV文件格式非法！");
                }

                string[] lineContents = sLine.Split(',');
                SetPrePostActionString(lineContents, sPreAction, sPostAction);
               
                StepDefinition stepDefinition = new StepDefinition(lineContents,i);
                stepDefinitions.Add(stepDefinition);
            }
            return new Protocol(name, stepDefinitions);
        }

        private static void SetPrePostActionString(string[] lineContents, string sPreAction, string sPostAction)
        {
            if (sPreAction != "")
                lineContents[(int)StepDefCol.PreAction] = sPreAction;

            if (sPostAction != "")
                lineContents[(int)StepDefCol.PostAction] = sPostAction;
        }

        private static void ExtractPrePostActionString(ref string sLine, ref string sPreAction, ref string sPostAction)
        {
            while (sLine.Contains('['))
            {
                int startPos = sLine.IndexOf('[');
                int commaCntBeforeBraketMark = sLine.Substring(0, startPos).Count(x => x == comma);
                int firstEndPos = sLine.IndexOf(']');
                if (firstEndPos == -1)
                    throw new Exception("[]格式错误！");
                string sTemp = sLine.Substring(startPos + 1, firstEndPos - startPos - 1);
                sLine = sLine.Remove(startPos, firstEndPos - startPos);
                if (commaCntBeforeBraketMark == (int)StepDefCol.PreAction)
                {
                    sPreAction = sTemp;
                }
                else
                {
                    sPostAction = sTemp;
                }
            }
           
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
        LiquidClass = 7,
        AspiratePosition = 8,
        DispensePosition = 9,
        PreAction = 10,
        PostAction = 11,
        DelaySeconds = 12
    }

    class StepDefinition :BindableBase
    {
        public string Description { get; set; }
        public string SourceLabware { get; set; }
        public string DestLabware { get; set; }
        public string Volume { get; set; }
        public string RepeatTimes { get; set; }
        public string TipType { get; set; }
        public string ReuseTimes { get; set; }
        public string AspirateConstrain { get; set; }
        public string DispenseConstrain { get; set; }
        public string LiquidClass { get; set; }
        public int LineNumber { get; set; }
        public string PreAction { get; set; }
        public string PostAction { get; set; }
        public string DelaySeconds { get; set; }

        public StepDefinition()
        {

        }

        public StepDefinition(string[] lines,int no)
        {
            LineNumber = no;
            Description = lines[(int)StepDefCol.Description];
            RepeatTimes = lines[(int)StepDefCol.RepeatTimes];
            PreAction = lines[(int)StepDefCol.PreAction];
            PostAction = lines[(int)StepDefCol.PostAction];
            DelaySeconds = lines[(int)StepDefCol.DelaySeconds];
            Volume = lines[(int)StepDefCol.Volume];
            if (Volume == "")
                return;
            LiquidClass = lines[(int)StepDefCol.LiquidClass];
            SourceLabware = lines[(int)StepDefCol.SourceLabware];
            DestLabware = lines[(int)StepDefCol.DestLabware];
            TipType = lines[(int)StepDefCol.TipType];
            ReuseTimes = lines[(int)StepDefCol.ReuseTimes];
            AspirateConstrain = lines[(int)StepDefCol.AspiratePosition];
            DispenseConstrain = lines[(int)StepDefCol.DispensePosition];
         
        }
    }

    class StepDefinitionWithProgressInfo : StepDefinition
    {
        private bool _isWorking = false;
        private bool _isFinished = false;

        public bool IsWorking 
        {
            get
            {
                return _isWorking;
            }
            set
            {
                SetProperty(ref _isWorking, value);
            }
        }
        public bool IsFinished 
        {
            get
            {
                return _isFinished;
            }
            set
            {
                SetProperty(ref _isFinished, value);
            }
        }
        public StepDefinitionWithProgressInfo(StepDefinition stepDef)
        {
            Description = stepDef.Description;
            SourceLabware = stepDef.SourceLabware;
            Volume = stepDef.Volume;
            DestLabware = stepDef.DestLabware;
            RepeatTimes = stepDef.RepeatTimes;
            ReuseTimes = stepDef.ReuseTimes;
            TipType = stepDef.TipType;
            LineNumber = stepDef.LineNumber;
        }

    }
}
