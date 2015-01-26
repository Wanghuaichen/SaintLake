using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections;

namespace SaintX.StageControls
{
    /// <summary>
    /// Protocol.xaml 的交互逻辑
    /// </summary>
    public partial class StepMonitor : BaseUserControl
    {
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
            if (strs.Length < 3)
            {
                log.Error("Incorrect message");
                return;
            }

            try
            {
                string startOrFinish = strs[0];
                int nStep = int.Parse(strs[1]);
                int nTimes = int.Parse(strs[2]);
                bool isStart = startOrFinish.ToLower().Contains("s");
                ChangeBackGroudColor(nStep,isStart);
                if(isStart)
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

        private void ChangeBackGroudColor(int nStep,bool isStart)
        {
            foreach(var thisStepWithProgressInfo in stepsDefWithProgressInfo)
            {
                if (isStart)
                {
                    thisStepWithProgressInfo.IsWorking = thisStepWithProgressInfo.LineNumber == nStep;
                    thisStepWithProgressInfo.IsFinished = thisStepWithProgressInfo.LineNumber < nStep;
                }
                else
                {
                    thisStepWithProgressInfo.IsWorking = false;
                    thisStepWithProgressInfo.IsFinished = thisStepWithProgressInfo.LineNumber <= nStep;
                }
            }
        }
        
        #endregion
   

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = "";
            
            bool bAllExist = CheckLabwares(SettingsManager.Instance.Protocol.StepsDefinition, ref errMsg);
            if(!bAllExist)
            {
                SetInfo(errMsg, Colors.Red);
                return;
            }
            try
            {
                //GenerateScripts();
                worklist worklist = new worklist();
                worklist.GenerateScripts();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
                return;
            }
            
            timeEstimation.StartMajorStep(1);
            btnStart.IsEnabled = false;
            
        }
        private void SetInfo(string errMsg, Color color)
        {
            txtInfo.Text = errMsg;
            txtInfo.Foreground = new SolidColorBrush(color);
        }

        private bool CheckLabwares(List<StepDefinition> stepsDef, ref string errMsg)
        {
            List<string> labels = null;
            try
            {
                labels = EVOScriptReader.LabwareInfos.Keys.ToList();
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }

            foreach(var thisStepDef in stepsDef)
            {
                if (thisStepDef.Volume == "0" || thisStepDef.Volume == "")
                    continue;

                string[] srcAndDst = new string[] { thisStepDef.SourceLabware, thisStepDef.DestLabware };
                foreach(string labwareLabelShouldExist in srcAndDst)
                {
                    
                    if (!labels.Contains(labwareLabelShouldExist))
                    {
                        errMsg = string.Format("无法在Script中找到名为：{0}的器件。", thisStepDef.SourceLabware);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    class PipettingInfo
    {
        public string srcLabware;
        public int srcWellID;
        public double volume;
        public string dstLabware;
        public int dstWellID;

        public PipettingInfo(string srcLabware,int srcWellID, double volume, string dstLabware, int dstWellID)
        {
            this.srcLabware = srcLabware;
            this.srcWellID = srcWellID;
            this.volume = volume;
            this.dstLabware = dstLabware;
            this.dstWellID = dstWellID;
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
