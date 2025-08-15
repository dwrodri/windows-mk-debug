using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseKeyboardTracker
{
    public class GlobalMouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        private LowLevelMouseProc _proc = HookCallback;
        private IntPtr _hookID = IntPtr.Zero;

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<Point> MouseMoved;

        public GlobalMouseHook()
        {
            _proc = HookCallback;
        }

        public void Install()
        {
            SetAsCurrentInstance();
            _hookID = SetHook(_proc);
        }

        public void Uninstall()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                POINT hookStruct = (POINT)Marshal.PtrToStructure(lParam, typeof(POINT));
                
                // Get the singleton instance to fire the event
                var instance = GetCurrentInstance();
                if (instance != null)
                {
                    instance.MouseMoved?.Invoke(instance, new Point(hookStruct.x, hookStruct.y));
                }
            }

            var currentInstance = GetCurrentInstance();
            return CallNextHookEx(currentInstance?._hookID ?? IntPtr.Zero, nCode, wParam, lParam);
        }

        private static GlobalMouseHook _currentInstance;
        private static GlobalMouseHook GetCurrentInstance()
        {
            return _currentInstance;
        }

        public void SetAsCurrentInstance()
        {
            _currentInstance = this;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
