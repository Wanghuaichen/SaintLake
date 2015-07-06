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
using System.Diagnostics;

namespace SaintX.StageControls
{
    /// <summary>
    /// Protocol.xaml 的交互逻辑
    /// </summary>
    public partial class StepMonitor : BaseUserControl
    {
        TimeEstimation timeEstimation = null;
        ObservableCollection<StepDefinitionWithProgressInfo> stepsDefWithProgressInfo = new ObservableCollection<StepDefinitionWithProgressInfo>();
        bool pipeCreated = false;
        System.Timers.Timer chkTimer = new System.Timers.Timer(1000);

        public StepMonitor(Stage stage, BaseHost host):base(stage,host)
        {
            InitializeComponent();
            EVOController.Instance.onCloseSucceed += Instance_onCloseSucceed;
            EVOController.Instance.onStartFinished +=Instance_onStartFinished;
            
        }
        
        private void UpdateWaitStatus()
        {
            if (EVOController.Instance.Started)
            {
                btnStart.IsEnabled = true;
                btnCloseEVOware.IsEnabled = true;
                log.Info("EVOware started successfully.");
                SetInfo("EVOware已经启动。", Colors.Green);
            }
            else
            {
                log.Info("waiting EVOWare.");
                SetInfo("等待EVOware启动完成。", Colors.Black);
            }
        }

        protected override void Initialize()
        {
            log.Info("Initialize StepMonitor");
            try
            {
                CreateNamedPipeServer();
                InitStepsInfo();
                SetNeededTips();
                SetLastRunInfo();
                CalculateShoppingList();
                UpdateWaitStatus();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
            }
        }

        private void SetNeededTips()
        {
            string fileName = string.Format("tipCount_{0}.txt", GlobalVars.Instance.ProtocolName);
            string fullPath = FolderHelper.GetDataFolder() + fileName;
          
            try
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException(string.Format("无法找到位于{0}的文件！", fullPath));
                string expression = File.ReadAllText(fullPath);
                
                expression = expression.Replace("N", GlobalVars.Instance.SampleCount.ToString());
                NCalc.Expression e = new NCalc.Expression(expression);
                
                txtTipsNeed.Text = e.Evaluate().ToString();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
            }
         

        }

        private void SetLastRunInfo()
        {
            if (GlobalVars.Instance.UseLastTimeSetting)
            {
                int startStep = GlobalVars.Instance.LastRunInfos.FinishedSteps + 1;
                txtFromStep.Text = startStep.ToString();
                ChangeBackGroudColor(startStep, true);
            }
        }

        #region events
        void Instance_onCloseSucceed()
        {
            log.Info("Instance_onCloseFinished");
            this.Dispatcher.Invoke((Action)delegate
            {
                log.Info("close EVOware successfully.");
                SetInfo("成功关闭EVOWare", Colors.Green);
            });
        }

        void Instance_onStartFinished()
        {
            log.Info("Instance_onStartFinished");
            this.Dispatcher.Invoke((Action)delegate
            {
                btnStart.IsEnabled = true;
                btnCloseEVOware.IsEnabled = true;
                SetInfo("EVOware已经启动。", Colors.Green);
            });
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            log.Info("btn start clicked.");
            try
            {
                EVOController.Instance.RunScript();
                WriteVariables();
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
                return;
            }
            FolderHelper.WriteResult(true);
            FolderHelper.Backup();
            FeedWaiter();
            timeEstimation.StartMajorStep(1);
            btnStart.IsEnabled = false;
        }

       

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            log.Info("btn exit clicked.");
            try
            {
                EVOController.Instance.Close();
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, Colors.Red);
            }
        }

        #endregion
     
        private void CalculateShoppingList()
        {
            log.Info("calculate shopping list.");
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
           
            List<ReagentShopplist> shoppingList = new List<ReagentShopplist>();
            Dictionary<string,double> reagent_vol = new Dictionary<string,double>();

            foreach(StepDefinition thisStepDef in stepsDef)
            {
                if (thisStepDef.Volume == 0)
                    continue;
                int repeatTimes = int.Parse(thisStepDef.RepeatTimes);
                double thisStepVolumeTotal = thisStepDef.DeadVolume / 1000.0 + thisStepDef.Volume * repeatTimes * GlobalVars.Instance.SampleCount / 1000.0;
                if (reagent_vol.ContainsKey(thisStepDef.SourceLabware))
                    reagent_vol[thisStepDef.SourceLabware] += thisStepVolumeTotal;
                else
                    reagent_vol.Add(thisStepDef.SourceLabware, thisStepVolumeTotal);
                
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
            stepsDefWithProgressInfo.Clear();
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
            if (pipeCreated)
                return;

            pipeCreated = true;
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
                    GlobalVars.Instance.LastRunInfos.FinishedSteps = nStep - 1;
                }
                else
                {
                    GlobalVars.Instance.LastRunInfos.FinishedSteps = nStep;
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
   

      
        private void WriteVariables()
        {
            FolderHelper.WriteVariable("sampleCount", GlobalVars.Instance.SampleCount.ToString());
            FolderHelper.WriteVariable("protocolName", GlobalVars.Instance.ProtocolName.ToString());
            FolderHelper.WriteRunInfo(string.Format("Sample Count: {0}, ProtocolName: {1},AssayName {2}", GlobalVars.Instance.SampleCount, GlobalVars.Instance.ProtocolName, GlobalVars.Instance.AssayName));

        }

        private void FeedWaiter()
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (!process.ProcessName.Contains("FeedMe"))
                    continue;
                while (true)
                {
                    process.Kill();
                    Thread.Sleep(100);
                    if (process.HasExited)
                        break;
                }

            }
        }
        private void SetInfo(string errMsg, Color color)
        {
            txtInfo.Text = errMsg;
            txtInfo.Foreground = new SolidColorBrush(color);
        }

        private bool CheckLabwares(List<StepDefinition> stepsDef, ref string errMsg)
        {
            log.Info("check labwares.");
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
                if (thisStepDef.Volume == 0 )
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
