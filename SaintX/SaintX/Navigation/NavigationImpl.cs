using SaintX.Interfaces;
using SaintX.StageControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaintX.Navigation
{
    public class StageArgs : EventArgs
    {
        protected Stage _stage;
        public StageArgs(Stage stage)
        {
            _stage = stage;
        }
    }

    public class Navigate2Args : StageArgs
    {
        public Stage DestStage
        {
            get
            {
                return _stage;
            }
        }
        public Navigate2Args(Stage stage):base(stage)
        {

        }
    }

    public class StageFinishedArgs : StageArgs
    {
        public Stage SourceStage
        {
            get
            {
                return _stage;
            }
        }
        public StageFinishedArgs(Stage stage)
            : base(stage)
        {

        }
    }

    public abstract class BaseHost : Window, IHost
    {
        public event EventHandler onStageChanged;
        protected bool preventUI = false;
        protected Stage farthestStage = Stage.AssayDef;
        protected List<BaseUserControl> stageUserControls = new List<BaseUserControl>();
        public BaseHost()
        {
            AddSteps();
        }

        private void AddSteps()
        {
            stageUserControls.Add(new AssayDefinition(Stage.AssayDef, this));
            stageUserControls.Add(new BarcodeDefinition(Stage.BarcodeDef, this));
            stageUserControls.Add(new StepMonitor(Stage.StepMonitor, this));
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            foreach (var control in stageUserControls)
            {
                IStageControl iStageControl = (IStageControl)control;
                if (iStageControl != null)
                    iStageControl.onFinished += iStageControl_onFinished;
            }
        }

        void iStageControl_onFinished(object sender, EventArgs e)
        {
            Stage finishedStage = ((StageFinishedArgs)e).SourceStage;
            farthestStage = (Stage)(finishedStage + 1);
            NavigateTo(farthestStage);
        }

        protected void NavigateTo(Stage stage)
        {
            if (onStageChanged != null)
                onStageChanged(this, new Navigate2Args(stage));
        }
    }

    public abstract class BaseUserControl : UserControl, IStageControl
    {
        private Stage _stage;
        bool resultIsOk = false;
        public BaseUserControl(Stage stage, BaseHost host)
        {
            _stage = stage;
            host.onStageChanged += onStageChanged;
        }
        
        public BaseUserControl()
        {
        }

        public bool ResultIsOk 
        { 
            get
            {
                return resultIsOk;
            }
            set 
            {
                resultIsOk = value;
            }
        }


        protected virtual void onStageChanged(object sender, EventArgs e)
        {
            Navigate2Args navigate2Args = (Navigate2Args)e;
            this.Visibility = navigate2Args.DestStage == _stage ? Visibility.Visible : Visibility.Hidden;
        }

        public Stage CurStage { get { return _stage; } }


        public event EventHandler onFinished;

        public void NotifyFinished()
        {
            if(onFinished != null)
                onFinished(this,new StageFinishedArgs(_stage));
        }
    }
}
