using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace SaintX.StageControls
{
    /// <summary>
    /// Protocol.xaml 的交互逻辑
    /// </summary>
    public partial class StepMonitor : BaseUserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TimeEstimation timeEstimation = null;

        ObservableCollection<StepDefinitionWithProgressInfo> stepsDefWithProgressInfo = new ObservableCollection<StepDefinitionWithProgressInfo>();


        public StepMonitor(Stage stage, BaseHost host):base(stage,host)
        {
            InitializeComponent();
            InitStepsInfo();
            this.Loaded += StepMonitor_Loaded;
         
        }

        void StepMonitor_Loaded(object sender, RoutedEventArgs e)
        {
            CreateNamedPipeServer();
            CalculateShoppingList();
        }

        private void CalculateShoppingList()
        {
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
           
            List<ReagentShopplist> shoppingList = new List<ReagentShopplist>();
            Dictionary<string,double> reagent_vol = new Dictionary<string,double>();

            foreach(StepDefinition thisStepDef in stepsDef)
            {
                if (thisStepDef.Volume == "")
                    continue;
                int repeatTimes = int.Parse(thisStepDef.RepeatTimes);
                double vol =  double.Parse(thisStepDef.Volume);
                if (reagent_vol.ContainsKey(thisStepDef.SourceLabware))
                    reagent_vol[thisStepDef.SourceLabware] += vol * repeatTimes * GlobalVars.Instance.SampleCount / 1000.0;
                else
                    reagent_vol.Add(thisStepDef.SourceLabware, vol * repeatTimes * GlobalVars.Instance.SampleCount / 1000.0);
            }
            foreach(KeyValuePair<string,double> pair in reagent_vol)
            {
                shoppingList.Add(new ReagentShopplist(pair));
            }
            lvShoppingList.ItemsSource = shoppingList;
        }

        private void InitStepsInfo()
        {
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
            timeEstimation = new TimeEstimation(stepsDef);
            foreach(var stepDef in stepsDef)
            {
                var stepDefEx = new StepDefinitionWithProgressInfo(stepDef);
                stepsDefWithProgressInfo.Add(stepDefEx);
            }
            timeInfo.DataContext = timeEstimation;
            lvProtocol.ItemsSource = stepsDefWithProgressInfo;
        }

        #region namedpipe
        private void CreateNamedPipeServer()
        {
            Pipeserver.owner = this;
            Pipeserver.ownerInvoker = new Invoker(this);
            ThreadStart pipeThread = new ThreadStart(Pipeserver.createPipeServer);
            Thread listenerThread = new Thread(pipeThread);
            listenerThread.SetApartmentState(ApartmentState.STA);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        internal void ExecuteCommand(string sCommand)
        {
            if (sCommand.Contains("shutdown"))
            {
                ((Window)this.Parent).Close();
                return;
            }
            string[] strs = sCommand.Split(';');
            log.Info(sCommand);
            if (strs.Length < 2)
            {
                log.Error("Incorrect message");
                return;
            }

            try
            {
                string startOrFinish = strs[0];
                int nStep = int.Parse(strs[1]);
                int nTimes = int.Parse(strs[2]);
                ChangeBackGroudColor(nStep);
                if(startOrFinish.ToLower().Contains("s"))
                {
                    timeEstimation.StartMajorStep(nStep);
                }
                else
                {
                    timeEstimation.UpdateProgress(nStep, nTimes);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void ChangeBackGroudColor(int nStep)
        {
            
        }
        
        #endregion}

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = "";
            bool bAllExist = CheckLabwares(SettingsManager.Instance.Protocol.StepsDefinition, errMsg);
            if(!bAllExist)
            {
                SetInfo(errMsg, Colors.Red);
                return;
            }
            GenerateScripts();
            timeEstimation.StartMajorStep(1);
            btnStart.IsEnabled = true;
        }

        private void GenerateScripts()
        {
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
            List<string> scripts = new List<string>();
            for(int i = 0; i < stepsDef.Count; i++)
            {
                var stepDef = stepsDef[i];
                scripts.AddRange(GenerateScriptsThisStep(stepDef, i + 1));
            }
        }

        private List<string> GenerateScriptsThisStep(StepDefinition stepDef, int stepNo)
        {
            List<string> scripts = new List<string>();
            int smpCnt = GlobalVars.Instance.SampleCount;
            double volume = double.Parse(stepDef.Volume);
            int tipCountLiha = int.Parse(ConfigurationManager.AppSettings["tipCount"]);
            int aspConstrainTipCnt = GetConstrainTipCnt(stepDef.AspirateConstrain);
            int maxAspTipCntOnce = Math.Min(tipCountLiha, aspConstrainTipCnt);
            int totalTimes = (smpCnt + maxAspTipCntOnce - 1) / maxAspTipCntOnce;
            scripts.Add(string.Format("C;{0}",stepDef.Description));
            for (int pipettingDiffSampleRounds = 0; pipettingDiffSampleRounds < totalTimes; pipettingDiffSampleRounds++)
            {
                int thisTimeCnt = Math.Min(smpCnt - pipettingDiffSampleRounds * maxAspTipCntOnce, maxAspTipCntOnce);
                int startWellID = pipettingDiffSampleRounds * maxAspTipCntOnce + 1;
                List<int> sourceWellIDs = GetWellIDs(startWellID, thisTimeCnt, stepDef.AspirateConstrain);
                List<int> dstWellIDs = GetWellIDs(startWellID, thisTimeCnt, stepDef.DispenseConstrain);
                scripts.AddRange(GenerateScriptsSameSample(sourceWellIDs, dstWellIDs, stepDef));
            }
            
            return scripts;
        }

        private IEnumerable<string> GenerateScriptsSameSample(List<int> sourceWellIDs, 
            List<int> dstWellIDs, StepDefinition stepDef)
        {
            List<string> strs = new List<string>();
            double maxVolumePerTip = int.Parse(stepDef.TipType) * 0.9;
            double volumeThisStep = double.Parse(stepDef.Volume);
            int nTotalTimes = (int)Math.Ceiling(volumeThisStep / maxVolumePerTip);
            int curTimes = 0;
            for(int i = 0; i< nTotalTimes; i++)
            {
                double volume = volumeThisStep - maxVolumePerTip * i;
                volume = Math.Min(volume, maxVolumePerTip);
                for(int j = 0; j< sourceWellIDs.Count; j++)
                {
                    strs.Add("A");
                    strs.Add("D");
                }
                curTimes++;
                if (curTimes > stepDef.ReuseTimes)
                {
                    curTimes = 0;
                    strs.Add("W;");
                }
            }
            return strs;
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
            for(int i = 0; i< cnt; i++)
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

        private bool IsFixedPostion(string pipettingPosition )
        {
            return pipettingPosition != "*";
        }
        
        private void SetInfo(string errMsg, Color color)
        {
            txtInfo.Text = errMsg;
            txtInfo.Foreground = new SolidColorBrush(color);
        }

        private bool CheckLabwares(List<StepDefinition> list, string errMsg)
        {
            throw new NotImplementedException();
        }

       
    }

    class ReagentShopplist
    {
        public string Reagent{get;set;}
        public double Volume{get;set;}
        
        public ReagentShopplist(KeyValuePair<string, double> pair)
        {
            // TODO: Complete member initialization
            Reagent = pair.Key;
            Volume = pair.Value;
        }
    }
}
