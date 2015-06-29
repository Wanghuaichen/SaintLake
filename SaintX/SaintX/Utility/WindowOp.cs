using ManagedWinapi.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;

namespace SaintX
{
    public class WindowOp
    {
        
        static bool firstRun = true;
        WindowMessenger winMessenger = new WindowMessenger();
        public List<string> EnumTopWindows()
        {
            return SystemWindow.AllToplevelWindows.Where(x => x.Title != "").Select(x => x.Title).ToList();
        }

        public SystemWindow GetWindow(string winTitle)
        {
            SystemWindow[] selectionWindows = SystemWindow.AllToplevelWindows.Where(x => x.Title == winTitle).ToArray();
            if (selectionWindows.Count() == 0)
                return null;
            else
                return selectionWindows[0];
        }


        public SystemWindow GetSelectionWindow()
        {
            return GetWindow("Selection");
        }
        public SystemWindow GetRuntimeControllerWindow()
        {
            return GetWindow("Runtime Controller");
        }

        private bool IsEVOware(string s)
        {
            return s.Contains("EVOware") && s.Contains("Freedom");
        }

        //public bool ShowEVOware(bool bVisible)
        //{
        //    if (winEVOware != null)
        //        winEVOware.Visible = bVisible;
        //    return winEVOware != null;
        //}

        protected void Select(AutomationElement element)
        {
            SelectionItemPattern select = (SelectionItemPattern)element.GetCurrentPattern(SelectionItemPattern.Pattern);
            select.Select();
        }

        protected void Click(AutomationElement element)
        {
            var click = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            click.Invoke();
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        
        const int SW_SHOWMINNOACTIVE = 7;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void MinimizeWindow(IntPtr handle)
        {
            ShowWindow(handle, SW_SHOWMINNOACTIVE);
        }



        internal void WaitForRunWindow()
        {
            bool bFound = false;
            for (int i = 0; i < 20; i++)
            {
                var runWindow = GetWindow("Runtime Controller");
                if (runWindow != null)
                {
                    bFound = true;
                    break;
                }
                Thread.Sleep(250);
            }
            Thread.Sleep(200);
            if (!bFound)
                throw new Exception("运行窗口不存在！");

        }

        public void RunScript()
        {
            var runWindow = GetWindow("Runtime Controller");
            if (runWindow == null)
                throw new Exception("运行窗口不存在！");
            if (firstRun)
            {
                firstRun = false;
            }
            else
            {
                ClickNewButton(runWindow);
                Thread.Sleep(200);
            }
            ClickRunButton(runWindow);
            MinimizeWindow(runWindow.HWnd);

        }

        private void ClickNewButton(SystemWindow runWindow)
        {
            SystemWindow[] newButtons = runWindow.AllDescendantWindows.Where(x => x.Title == "New Run").ToArray();
            if (newButtons.Count() == 0)
                throw new Exception("新建按钮无法找到！");
            ClickButton(newButtons[0]);

        }
        private void ClickRunButton(SystemWindow runWindow)
        {
            SystemWindow[] runButtons = runWindow.AllDescendantWindows.Where(x => x.Title == "Run").ToArray();
            if (runButtons.Count() == 0)
                throw new Exception("运行按钮无法找到！");
            ClickButton(runButtons[0]);
        }

        private void ClickButton(SystemWindow systemWindow)
        {
            systemWindow.Highlight();
            POINT pt = winMessenger.GetWindowCenter(systemWindow);
            winMessenger.MouseMove(pt.X, pt.Y);
            winMessenger.Click();
        }


        public void SelectScript(string scriptName)
        {
            var window = GetSelectionWindow();
            if (window == null)
                throw new Exception("无法找到脚本选择窗口！");
            AutomationElement scriptWindow = AutomationElement.FromHandle(window.HWnd);
            AutomationElement result = scriptWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, scriptName));
            if (result == null)
                throw new Exception("无法找到相应脚本！");
            Select(result);
            Thread.Sleep(250);
            var okButton = FindButton(scriptWindow, "OK");
            Click(okButton);
        }

        private AutomationElement FindButton(AutomationElement currentWindow, string buttonName)
        {
            var isButtonCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
            var isEnableCondition = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            var isOkCondition = new PropertyCondition(AutomationElement.NameProperty, buttonName);
            var enabledButtonCondition = new AndCondition(new Condition[] { isButtonCondition, isEnableCondition, isOkCondition });
            var okButton = currentWindow.FindFirst(TreeScope.Descendants, enabledButtonCondition);
            if (okButton == null)
                throw new Exception("无法找到下一步按钮！");
            return okButton;
        }



    }
}
