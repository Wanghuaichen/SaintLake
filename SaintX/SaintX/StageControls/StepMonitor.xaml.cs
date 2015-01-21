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
            
            #region set "--:--:--" for times
            string defaultTime = "--:--:--";
            txtTimeUsedThisStep.Text = defaultTime;
            txtTimeUsed.Text = defaultTime;
            txtRemainingTime.Text = defaultTime;
            txtRemianingTimeThisStep.Text = defaultTime;
            #endregion

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
            
            bool bAllExist = CheckLabwares(SettingsManager.Instance.Protocol.StepsDefinition, errMsg);
            if(!bAllExist)
            {
                SetInfo(errMsg, Colors.Red);
                return;
            }
            try
            {
                GenerateScripts();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
                return;
            }
            
            timeEstimation.StartMajorStep(1);
            btnStart.IsEnabled = false;
            timeInfo.DataContext = timeEstimation;
        }

        private void GenerateScripts()
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
            for(int i = 0; i < stepsDef.Count; i++)
            {
                var stepDef = stepsDef[i];

                string curStepFile = string.Format("{0}\\{1}.txt", stepsFolder,i + 1);
                File.WriteAllLines(curStepFile, GenerateScriptsThisStep(stepDef, i + 1));
            }
        }

        private List<string> GenerateScriptsThisStep(StepDefinition stepDef, int stepNo)
        {
            List<string> scripts = new List<string>();
            int smpCnt = GlobalVars.Instance.SampleCount;
            if (stepDef.Volume == "" || stepDef.Volume.Trim() == "0")
                return new List<string>() { "Reserved"};
            
            // Source labware: Trough 100ml, labeled 'T2', use wells 1 - 8
            // Destination labware: 96 Well Microplate, labeled 'MTP96-2', use wells 1 - 96
            // Liquid class 'Water'
            // DiTi Reuse = 2 (use 2 times), Multi-dispense = max. 5
            // Pipetting direction 0 = left to right
            // No wells to be excluded
            //   R;T2;;Trough 100ml;1;8;MTP96-2;;96 Well Microplate;1;96;100;Water;2;5;0
            string liquidClass = stepDef.LiquidClass;
            string adaptiveWell = string.Format("1;{0}",GlobalVars.Instance.SampleCount);
            string aspWellsUse = IsFixedPostion(stepDef.AspirateConstrain) ? stepDef.AspirateConstrain.Replace('-',';') 
                : adaptiveWell;
            string dispenseWellsUse = IsFixedPostion(stepDef.DispenseConstrain) ? stepDef.DispenseConstrain.Replace('-',';')
                : adaptiveWell;
            scripts.Add(string.Format("C;{0}", stepDef.Description));
            string reagentCommand = string.Format("R;{0};;;{1};{2};;;{3};{4};{5};{6};5;0",
                stepDef.SourceLabware,
                aspWellsUse,
                stepDef.DestLabware,
                dispenseWellsUse,
                stepDef.Volume,
                stepDef.LiquidClass,stepDef.RepeatTimes);
            scripts.Add(reagentCommand);
            return scripts;
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

        private bool CheckLabwares(List<StepDefinition> stepsDef, string errMsg)
        {
#if DEBUG
            return true;
#else
            var labels = EVOScriptReader.LabwareInfos.Keys.ToList();
            foreach(var thisStepDef in stepsDef)
            {

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
#endif
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
