using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace SaintX.StageControls
{
    /// <summary>
    /// Protocol.xaml 的交互逻辑
    /// </summary>
    public partial class StepMonitor : BaseUserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TimeEstimation timeEstimation = null;
        public StepMonitor(Stage stage, BaseHost host):base(stage,host)
        {
            InitializeComponent();
            InitStepsInfo();
            
            CreateNamedPipeServer();
        }

        private void InitStepsInfo()
        {
            var stepsDef = SettingsManager.Instance.Protocol.StepsDefinition;
            timeEstimation = new TimeEstimation(stepsDef);
            List<StepDefinitionWithProgressInfo> stepsDefWithProgressInfo = new List<StepDefinitionWithProgressInfo>();
            foreach(var stepDef in stepsDef)
            {
                var stepDefEx = new StepDefinitionWithProgressInfo(stepDef);
                stepDefEx.IsFinished = true;//test only
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
            timeEstimation.StartMajorStep(1);
        }
    }
}
