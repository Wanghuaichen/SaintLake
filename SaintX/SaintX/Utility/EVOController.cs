﻿using ManagedWinapi.Windows;
using SaintX.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaintX
{
    class EVOController
    {
        WindowOp winOp = new WindowOp();
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        CheckCondition checkCondition;
        BackgroundWorker backGroundWorker;

        public delegate void DelegateStartFinished();
        public delegate void CloseSucceed();
        public event DelegateStartFinished onStartFinished;
        public event CloseSucceed onCloseSucceed;
        private static EVOController instance;
        public bool AbortMonitoring { get; set; }
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public EVOController Instance
        {
            get
            {
                if (instance == null)
                    instance = new EVOController();
                return instance;
            }
        }

        private EVOController()
        {
            AbortMonitoring = false;
        }

        private bool EVOIsRunning()
        {
            Process[] processlist = Process.GetProcesses();
            string[] mainWindowTitles = processlist.Where(x => x.MainWindowTitle != "").Select(x => x.MainWindowTitle).ToArray();
            bool evoRunning = mainWindowTitles.Where(x => x.Contains("Freedom EVOware")).Count() > 0;
            return evoRunning;
        }

        public void Start()
        {
            if (!EVOIsRunning())
            {
                string userName = ConfigurationManager.AppSettings["user"];
                string password = ConfigurationManager.AppSettings["password"];
                string cmdLine = string.Format(@" -b -u {0} -w {1} -r {2}", userName, password, GlobalVars.Instance.ScriptName);
                Process.Start(@"C:\Program Files (x86)\TECAN\EVOware\Evoware.exe", cmdLine);
            }
            checkCondition = new CheckCondition("Selection", 60);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
       
        }
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            checkCondition.remainSeconds--;
            //bool bStarted = winOp.GetWindow(checkCondition.windowName) != null;
            var allTopWindows = winOp.EnumTopWindows();
            //allTopWindows.ForEach(x => log.Info(x));
            bool bStarted = allTopWindows.Exists(x => x.Contains(checkCondition.windowName));
            log.InfoFormat("found selection:{0}", bStarted);
            bool noTime = checkCondition.remainSeconds == 0;
            if (noTime)
            {
                throw new Exception("无法启动EVOware！");
            }
            if (bStarted)
            {
                log.Info("Started!");
                Started = true;
                Debug.WriteLine("Started!");
                if (onStartFinished != null)
                    onStartFinished();
            }
            if (noTime || bStarted)
            {
                timer.Stop();
            }
        }

        public void RunScript()
        {
            winOp.SelectScriptListBoxItem(GlobalVars.Instance.ScriptName);
            winOp.WaitForRunWindow();
            winOp.ClickRunButton();
        }

        public void Close()
        {
            StartMonitoring();
            SystemWindow selectionWindow = winOp.GetSelectionWindow();
            SystemWindow runWindow = winOp.GetRuntimeControllerWindow();
            SystemWindow startUp = winOp.GetStartupWindow();
            if (runWindow != null)
            {
                CloseRuntimeControlWindow(runWindow);
                return;
            }
            if (selectionWindow != null)
            {
                CloseSelectionWindow(selectionWindow);
                return;
            }
            if(startUp != null)
            {
                startUp.SendClose();
                CloseQuestionWindow();
                return;
            }
        }

        private void StartMonitoring()
        {
            var myThread = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    SleepALittle();
                    SleepALittle();
                    bool processDead = !EVOIsRunning();
                    log.Info(string.Format("Process is dead: {0}", processDead));
                    bool isClosing = false;
                    SystemWindow closingWindow = winOp.GetShuttingDownWindow();
                    if (closingWindow != null)
                    {
                        log.Info("Shutting Down");
                        isClosing = true;
                    }
                    if (isClosing || processDead)
                    {
                        if (onCloseSucceed != null)
                            onCloseSucceed();
                        break;
                    }
                    if (AbortMonitoring)
                        break;
                }
            });
            myThread.Start(); 
        }

        void SleepALittle()
        {
            Thread.Sleep(400);
        }
    
        private void CloseSelectionWindow(SystemWindow selectionWindow)
        {
            selectionWindow.SendClose();
            CloseQuestionWindow();
        }

        private void CloseQuestionWindow()
        {
            DelayCloseWindow("Question");
        }

        private bool DelayCloseWindow(string sName)
        {
            bool bClosed = false;
            for (int i = 0; i < 5; i++)
            {
                SleepALittle();
                SystemWindow win2Close = winOp.GetWindow(sName);
                if (win2Close != null)
                {
                    win2Close.SendClose();
                    bClosed = true;
                    break;
                }
            }
            return bClosed;
        }
        private void CloseRuntimeControlWindow(SystemWindow runWindow)
        {
            runWindow.SendClose();
            bool bok = DelayCloseWindow("Startup");
            if (!bok)
                throw new Exception("无法正常关闭！");
            CloseQuestionWindow();
        }

        public bool Started { get; set; }
    }

    struct CheckCondition
    {
        public string windowName;
        public int remainSeconds;
        public CheckCondition(string name, int maxTime)
        {
            windowName = name;
            remainSeconds = maxTime;
        }
    }
}
