using ManagedWinapi.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SaintX
{
    class WindowMessenger
    {
        #region nativeAPI
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out  Rect lpRect);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd,
           int wMsg, IntPtr wParam, string lParam);


        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SETTEXT = 0x000C;
        private const int WM_CHAR = 0x0102;
        private const int VK_RETURN = 0x0D;
        private const int KEYEVENTF_KEYUP = 0x02;
        private const int VK_UP = 0x26;
        #endregion



        internal void Click()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        internal void MouseMove(int x, int y)
        {
            SetCursorPos(x, y);
        }
        public void Keyup()
        {
            keybd_event(VK_UP, 0, 0, 0);
            keybd_event(VK_UP, 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(50);
        }


        internal POINT GetWindowCenter(SystemWindow window)
        {
            int x = window.Rectangle.Left + window.Rectangle.Width / 2;
            int y = window.Rectangle.Top + window.Rectangle.Height / 2;
            return new POINT(x, y);
        }

    }
}
