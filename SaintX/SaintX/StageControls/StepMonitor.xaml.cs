using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            timeEstimation.StartMajorStep(1);
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
