using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace rkeyboard.WinHook {
    public class Interceptor {
        public const int WH_KEYBOARD_LL = 13;
        public const int KEY_DOWN = 0x0100;
        public const int KEY_UP = 0x0101;
        public const int WM_SYSKEY_DOWN = 0x0104;
        public const int WM_SYSKEY_UP = 0x0105;
        
        public static IntPtr InstallHook(LowLevelKeyboardProc proc) {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public static bool UninstallHook(IntPtr hook) {
            return UnhookWindowsHookEx(hook);
        }

        public static IntPtr CallNextHook(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam) {
            return CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}