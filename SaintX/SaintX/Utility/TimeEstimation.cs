﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace Natchs.Data
{
    class TimeEstimation :BindableBase
    {
        Dictionary<ProgressInfo, int> progressInfo_finishedMinorSteps = new Dictionary<ProgressInfo, int>();
        Dictionary<int, int> step_MinorStepCnt = new Dictionary<int, int>();
        int totalMinorSteps = 0;
        Timer timer = new Timer(1000);
        TimeSpan oneSecond = TimeSpan.FromSeconds(1);
        
        public TimeEstimation(List<StepDefinition> stepsDefinition)
        {
            foreach(StepDefinition stepDef in stepsDefinition)
            {
                
                int nStep = stepDef.LineNumber;
                int repeatTimes = 1;//int.Parse(stepDef.RepeatTimes);
                step_MinorStepCnt.Add(nStep, repeatTimes);
                for(int i = 0; i< repeatTimes;i++)
                {
                    totalMinorSteps++;
                    progressInfo_finishedMinorSteps.Add(new ProgressInfo(nStep, i + 1), totalMinorSteps);
                }
            }
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            if (_totalRemaining.TotalSeconds > 0)
            {
                TotalUsed += oneSecond;
                TotalRemaining -= oneSecond;
            }
            
            if( _currentStepRemaining.TotalSeconds > 0)
            {
                CurrentStepUsed += oneSecond;
                CurrentStepRemaining -= oneSecond;
            }
                
        }

        private TimeSpan _totalUsed;
        private TimeSpan _totalRemaining;
        private TimeSpan _currentStepUsed;
        private TimeSpan _currentStepRemaining;

        public void StartMajorStep(int majorStep)
        {
            if(majorStep == 1)
            {
                timer.Start();
                _totalRemaining = TimeSpan.FromSeconds(totalMinorSteps * 60);
                _currentStepRemaining = TimeSpan.FromSeconds(step_MinorStepCnt[1] * 60);
            }
            _currentStepUsed = TimeSpan.FromSeconds(0);
        }

        public void UpdateProgress(int finishedMajorStep, int finishedMinorStep)
        {
            double totalUsedSeconds = _totalUsed.TotalSeconds;
            int finishedMinorSteps = progressInfo_finishedMinorSteps[new ProgressInfo(finishedMajorStep, finishedMinorStep)];
            _totalRemaining = TimeSpan.FromSeconds(totalUsedSeconds * (totalMinorSteps - finishedMinorSteps) * totalUsedSeconds / (finishedMinorSteps));
            _currentStepRemaining = TimeSpan.FromSeconds(finishedMinorStep / _currentStepUsed.TotalSeconds * (step_MinorStepCnt[finishedMajorStep] - finishedMinorSteps));
        }


        public TimeSpan TotalUsed
        {
            get
            {
                return _totalUsed;
            }
            set
            {
                SetProperty(ref _totalUsed, value);
            }
        }
        public TimeSpan TotalRemaining
        {
            get
            {
                return _totalRemaining;
            }
            set
            {
                SetProperty(ref _totalRemaining, value);
            }
        }
        public TimeSpan CurrentStepUsed
        {
            get
            {
                return _currentStepUsed;
            }
            set
            {
                SetProperty(ref _currentStepUsed, value);
            }
        }
        public TimeSpan CurrentStepRemaining
        {
            get
            {
                return _currentStepRemaining;
            }
            set
            {
                SetProperty(ref _currentStepRemaining, value);
            }
        }
    }

    struct ProgressInfo
    {
        public int majorStep;
        public int minorStep;
        public ProgressInfo(int step, int times)
        {
            majorStep = step;
            minorStep = times;
        }
    }
}
