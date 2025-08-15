using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseKeyboardTracker
{
    public class GlobalKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc = HookCallback;
        private IntPtr _hookID = IntPtr.Zero;

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyboardHookEventArgs> KeyDown;
        public event EventHandler<KeyboardHookEventArgs> KeyUp;

        [Flags]
        public enum ModifierKeys
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        public class KeyboardHookEventArgs : EventArgs
        {
            public int VirtualKeyCode { get; set; }
            public ModifierKeys Modifiers { get; set; }
            public bool SuppressKeyPress { get; set; }
        }

        public GlobalKeyboardHook()
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

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool keyDown = wParam == (IntPtr)WM_KEYDOWN;
                bool keyUp = wParam == (IntPtr)WM_KEYUP;

                if (keyDown || keyUp)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    ModifierKeys modifiers = GetModifierKeys();

                    var instance = GetCurrentInstance();
                    if (instance != null)
                    {
                        var eventArgs = new KeyboardHookEventArgs
                        {
                            VirtualKeyCode = vkCode,
                            Modifiers = modifiers
                        };

                        if (keyDown)
                        {
                            instance.KeyDown?.Invoke(instance, eventArgs);
                        }
                        else if (keyUp)
                        {
                            instance.KeyUp?.Invoke(instance, eventArgs);
                        }

                        if (eventArgs.SuppressKeyPress)
                        {
                            return (IntPtr)1;
                        }
                    }
                }
            }

            var currentInstance = GetCurrentInstance();
            return CallNextHookEx(currentInstance?._hookID ?? IntPtr.Zero, nCode, wParam, lParam);
        }

        private static ModifierKeys GetModifierKeys()
        {
            ModifierKeys modifiers = ModifierKeys.None;

            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                modifiers |= ModifierKeys.Control;

            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                modifiers |= ModifierKeys.Alt;

            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                modifiers |= ModifierKeys.Shift;

            if ((GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0)
                modifiers |= ModifierKeys.Win;

            return modifiers;
        }

        private static GlobalKeyboardHook _currentInstance;
        private static GlobalKeyboardHook GetCurrentInstance()
        {
            return _currentInstance;
        }

        public void SetAsCurrentInstance()
        {
            _currentInstance = this;
        }

        // Virtual key codes for modifier keys
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // Alt key
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
