using SaintX.Data;
using SaintX.StageControls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintX.Utility
{
    class worklist
    {
        #region scripts
        public void GenerateScripts()
        {
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
            List<string> scripts = new List<string>();
            string totalCntFile = FolderHelper.GetOutputFolder() + "stepsCnt.txt";

            File.WriteAllText(totalCntFile, stepsDef.Count.ToString());

            string stepsFolder = FolderHelper.GetOutputFolder() + "Steps\\";
            if (!Directory.Exists(stepsFolder))
            {
                Directory.CreateDirectory(stepsFolder);
            }
            for (int i = 0; i < stepsDef.Count; i++)
            {
                var stepDef = stepsDef[i];
                string curStepFile = string.Format("{0}\\{1}.gwl", stepsFolder, i + 1);
                List<string> strsOneTime = GenerateScriptsThisStep(stepDef, i + 1);
                List<string> strsEveryTimes = new List<string>();
                if(stepDef.PreAction != string.Empty)
                    strsEveryTimes.Add(stepDef.PreAction);
                for (int curTimes = 0; curTimes < int.Parse(stepDef.RepeatTimes); curTimes++)
                {
                    strsEveryTimes.Add(GetNotifyString(true, stepDef.LineNumber, curTimes + 1));
                    strsEveryTimes.AddRange(strsOneTime);
                    if (stepDef.DelaySeconds != string.Empty)
                    {
                        strsEveryTimes.AddRange(GetDelayStrings(stepDef.DelaySeconds));
                    }
                    strsEveryTimes.Add(GetNotifyString(false,stepDef.LineNumber, curTimes + 1));
                }
                if (stepDef.PostAction != string.Empty)
                    strsEveryTimes.Add(stepDef.PostAction);
              
                File.WriteAllLines(curStepFile, strsEveryTimes);
            }
        }

        private List<string> GetDelayStrings(string sDelaySeconds)
        {
            List<string> strs = new List<string>();
            strs.Add("StartTimer(\"100\");");
            strs.Add(string.Format("WaitTimer(\"100\",\"{0}\");", sDelaySeconds));
            return strs;
        }

        private string GetNotifyString(bool startOrEnd, int curStep, int curTimes)
        {
            string notifyExePath = FolderHelper.GetExeFolder() + string.Format("Notifier.exe {0};{1};{2}",startOrEnd ? 's':'e', curStep, curTimes);
            return string.Format("B;Execute(\"{0}\",2,\"\",2);", notifyExePath);
        }

        private List<string> GenerateScriptsThisStep(StepDefinition stepDef, int stepNo)
        {

            if (stepDef.Volume == "" || stepDef.Volume.Trim() == "0")
                return new List<string>() { "C;Reserved" };

            //use r command for simplicity
            if (double.Parse(stepDef.TipType) * 0.9 > double.Parse(stepDef.Volume))
            {
                return GenerateRCommnad(stepDef, stepNo);
            }

            List<string> scripts = new List<string>();
            scripts.Add(string.Format("C;{0}", stepDef.Description));
            List<PipettingInfo> pipettingInfos = GetPipettingInfos(stepDef);

            int tipCountLiha = int.Parse(ConfigurationManager.AppSettings["tipCount"]);
            int tipReusedTimes = 0;
            int maxTipReuseTimes = int.Parse(stepDef.ReuseTimes);

            while (pipettingInfos.Count > 0)
            {
                int srcWellID = pipettingInfos[0].srcWellID;
                var sameSourceWellPipettingInfos = pipettingInfos.Where(x => x.srcWellID == srcWellID);
                pipettingInfos = pipettingInfos.Except(sameSourceWellPipettingInfos).ToList();

                tipReusedTimes = 0;
                bool bNeedWash = false;
                foreach (var pipettingInfo in sameSourceWellPipettingInfos)
                {
                    scripts.Add(GetAspirate(pipettingInfo.srcLabware, pipettingInfo.srcWellID, pipettingInfo.volume, stepDef.LiquidClass));
                    scripts.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.volume, stepDef.LiquidClass));
                    tipReusedTimes++;
                    bNeedWash = tipReusedTimes % maxTipReuseTimes == 0;
                    if (bNeedWash)
                        scripts.Add("W;");
                }
                scripts.Add("W;");
            }

            return scripts;
        }

        private List<PipettingInfo> GetPipettingInfos(StepDefinition stepDef)
        {
            int smpCnt = GlobalVars.Instance.SampleCount;
            double volume = double.Parse(stepDef.Volume);
            int tipCountLiha = int.Parse(ConfigurationManager.AppSettings["tipCount"]);
            int totalTimes = (smpCnt + tipCountLiha - 1) / tipCountLiha;

            int tipUsedTimes = 0;
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
            for (int batchTimes = 0; batchTimes < totalTimes; batchTimes++)
            {
                int thisTimeCnt = Math.Min(smpCnt - batchTimes * tipCountLiha, tipCountLiha);
                int startWellID = batchTimes * tipCountLiha + 1;
                List<int> sourceWellIDs = GetWellIDs(startWellID, thisTimeCnt, stepDef.AspirateConstrain);
                List<int> dstWellIDs = GetWellIDs(startWellID, thisTimeCnt, stepDef.DispenseConstrain);
                pipettingInfos.AddRange(GetPipettingInfosBatch(sourceWellIDs, dstWellIDs, stepDef, ref tipUsedTimes));
            }

            pipettingInfos = pipettingInfos.OrderBy(x => x.srcWellID).ToList();
            return pipettingInfos;
        }

        private List<string> GenerateRCommnad(StepDefinition stepDef, int stepNo)
        {
            List<string> scripts = new List<string>();
            int smpCnt = GlobalVars.Instance.SampleCount;
            // Source labware: Trough 100ml, labeled 'T2', use wells 1 - 8
            // Destination labware: 96 Well Microplate, labeled 'MTP96-2', use wells 1 - 96
            // Liquid class 'Water'
            // DiTi Reuse = 2 (use 2 times), Multi-dispense = max. 5
            // Pipetting direction 0 = left to right
            // No wells to be excluded
            //   R;T2;;Trough 100ml;1;8;MTP96-2;;96 Well Microplate;1;96;100;Water;2;5;0
            string liquidClass = stepDef.LiquidClass;
            string adaptiveWell = string.Format("1;{0}", GlobalVars.Instance.SampleCount);
            string aspWellsUse = IsFixedPostion(stepDef.AspirateConstrain) ? stepDef.AspirateConstrain.Replace('-', ';')
                : adaptiveWell;
            string dispenseWellsUse = IsFixedPostion(stepDef.DispenseConstrain) ? stepDef.DispenseConstrain.Replace('-', ';')
                : adaptiveWell;
            scripts.Add(string.Format("C;{0}", stepDef.Description));
            string reagentCommand = string.Format("R;{0};;;{1};{2};;;{3};{4};{5};{6};5;0",
                stepDef.SourceLabware,
                aspWellsUse,
                stepDef.DestLabware,
                dispenseWellsUse,
                stepDef.Volume,
                stepDef.LiquidClass, stepDef.ReuseTimes);
            scripts.Add(reagentCommand);
            return scripts;
        }


        private string GetAspOrDisp(string sLabware, int wellID, double vol, string liquidClass, bool isAsp)
        {

            string str = string.Format("{4};{0};;;{1};;{2};{3};;",
                        sLabware,
                        wellID,
                        vol, liquidClass, isAsp ? 'A' : 'D');
            return str;
        }

        private string GetAspirate(string sLabware, int srcWellID, double vol, string liquidClass)
        {
            return GetAspOrDisp(sLabware, srcWellID, vol, liquidClass, true);
        }

        private string GetDispense(string sLabware, int dstWellID, double vol, string liquidClass)
        {
            return GetAspOrDisp(sLabware, dstWellID, vol, liquidClass, false);
        }

        private List<PipettingInfo> GetPipettingInfosBatch(List<int> sourceWellIDs,
            List<int> dstWellIDs, StepDefinition stepDef, ref int tipUsedTimes)
        {
            List<string> strs = new List<string>();
            double tipType = int.Parse(stepDef.TipType);
            double maxVolumePerTip = tipType * 0.9;
            double volumeThisStep = double.Parse(stepDef.Volume);
            int nTotalTimes = (int)Math.Ceiling(volumeThisStep / maxVolumePerTip);
            double finishedVolume = 0;
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
            for (int i = 0; i < nTotalTimes; i++)
            {

                double remainingVolume = volumeThisStep - finishedVolume;
                double volumeThisTime = Math.Min(remainingVolume, maxVolumePerTip);
                if (remainingVolume > maxVolumePerTip && remainingVolume - maxVolumePerTip <= tipType * 0.2)
                {
                    volumeThisTime = (int)(remainingVolume / 2.0);
                }
                finishedVolume += volumeThisTime;
                for (int j = 0; j < sourceWellIDs.Count; j++)
                {
                    pipettingInfos.Add(new PipettingInfo(stepDef.SourceLabware,
                        sourceWellIDs[j],
                        volumeThisTime,
                        stepDef.DestLabware,
                        dstWellIDs[j]));
                }
            }
            return pipettingInfos;
        }

        private List<int> GetWellIDs(int firstWellID, int cnt, string wellConstrain)
        {
            List<int> wellIDs = new List<int>();
            int maxAllowedWellCnt = 96;
            if (IsFixedPostion(wellConstrain))
            {
                string[] strs = wellConstrain.Split('-');
                int start = int.Parse(strs[0]);
                int end = int.Parse(strs[1]);
                maxAllowedWellCnt = end - start + 1;
            }
            for (int i = 0; i < cnt; i++)
            {
                int tmpID = firstWellID + i;
                while (tmpID > maxAllowedWellCnt)
                    tmpID -= maxAllowedWellCnt;
                wellIDs.Add(tmpID);
            }
            return wellIDs;
        }



        private int GetConstrainTipCnt(string tipConstrain)
        {
            string[] strs = tipConstrain.Split('-');
            int start = int.Parse(strs[0]);
            int end = int.Parse(strs[1]);
            return end - start + 1;
        }

        private bool IsFixedPostion(string pipettingPosition)
        {
            return pipettingPosition != "*";
        }


        #endregion
    }
}
