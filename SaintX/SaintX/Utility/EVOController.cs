using SaintX.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            bool evoRunning = processlist.Where(x => x.ProcessName.Contains("Evoware.exe")).Count() > 0;
            if (evoRunning)
                return;
            checkCondition = new CheckCondition("Selection", 60);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            Process.Start(@"C:\Program Files (x86)\TECAN\EVOware\Evoware.exe", "-b -r testworklist");
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

        private void RunScript()
        {
            winOp.SelectScript(GlobalVars.Instance.ScriptName);
            winOp.WaitForRunWindow();
            winOp.RunScript();
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
