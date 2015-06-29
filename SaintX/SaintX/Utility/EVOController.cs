using ManagedWinapi.Windows;
using SaintX.Data;
using System;
using System.Collections.Generic;
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
        public delegate void delStartFinished();
        public event delStartFinished onStartFinished;
        public void Start()
        {
            Process[] processlist = Process.GetProcesses();
            string[] mainWindowTitles = processlist.Where(x=>x.MainWindowTitle != "").Select(x => x.MainWindowTitle).ToArray();
            bool evoRunning = mainWindowTitles.Where(x => x.Contains("Freedom EVOware")).Count() > 0;
            if (evoRunning)
                return;
            checkCondition = new CheckCondition("Selection", 60);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            Process.Start(@"C:\Program Files (x86)\TECAN\EVOware\Evoware.exe", "-b -r " + GlobalVars.Instance.ScriptName);
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            checkCondition.remainSeconds--;
            bool bStarted = winOp.GetWindow(checkCondition.windowName) != null;
            bool noTime = checkCondition.remainSeconds == 0;
            if (noTime)
            {
                throw new Exception("无法启动EVOware！");
            }
            if (bStarted)
            {
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
            winOp.SelectScript(GlobalVars.Instance.ScriptName);
            winOp.WaitForRunWindow();
            winOp.RunScript();
           
        }

        public void Close()
        {
            SystemWindow selectionWindow = winOp.GetSelectionWindow();
            SystemWindow runWindow = winOp.GetRuntimeControllerWindow();

            if (runWindow != null)
            {
                CloseRuntimeControlWindow(runWindow);
            }

            if (selectionWindow != null)
            {
                CloseSelectionWindow(selectionWindow);
            }
        }

    
        private void CloseSelectionWindow(SystemWindow selectionWindow)
        {
            selectionWindow.SendClose();
            CloseQuestionWindow();
        }
        private void CloseQuestionWindow()
        {
            Thread.Sleep(800);
            SystemWindow questionWindow = winOp.GetWindow("Question");
            if (questionWindow != null)
                questionWindow.SendClose();
        }

        private void CloseRuntimeControlWindow(SystemWindow runWindow)
        {
            bool bok = false;
            runWindow.SendClose();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(500);
                SystemWindow startupWindow = winOp.GetWindow("Startup");
                if (startupWindow != null)
                {
                    startupWindow.SendClose();
                    bok = true;
                    break;
                }
            }
            if (!bok)
                throw new Exception("无法正常关闭！");
            CloseQuestionWindow();

        }
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
