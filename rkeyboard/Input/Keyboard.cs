using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using rkeyboard.WinUser;

namespace rkeyboard {
    public class Keyboard {
        public static void SendKeyDown(int vkey) {
            var input = new Input {
                type = (uint) InputType.Keyboard,
                input = new InputUnion {
                    ki = new KeyboardInput {
                        wVk = (ushort) vkey,
                        dwFlags = (uint) KeyEventFlags.KeyDown,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Input)));
        }
        
        public static void SendKeyUp(int vkey) {
            var input = new Input {
                type = (uint) InputType.Keyboard,
                input = new InputUnion {
                    ki = new KeyboardInput {
                        wVk = (ushort) vkey,
                        dwFlags = (uint) KeyEventFlags.KeyUp,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Input)));
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int KEY_DOWN = 0x0100;
        private const int KEY_UP = 0x0101;
        private const int WM_SYSKEY_DOWN = 0x0104;
        private const int WM_SYSKEY_UP = 0x0105;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hook;
        private IntPtr _pause;
        private Action<int> _onKeyDown;
        private Action<int> _onKeyUp;

        public Keyboard(Action<int> onKeyDown, Action<int> onKeyUp) {
            _onKeyDown = onKeyDown;
            _onKeyUp = onKeyUp;
            _proc = (nCode, wParam, lParam) => {
                if (_pause.ToInt32() == 0) {
                    if (nCode >= 0) {
                        var key = Marshal.ReadInt32(lParam);
                        if (wParam == (IntPtr)KEY_DOWN || wParam == (IntPtr)WM_SYSKEY_DOWN) {
                            _onKeyDown(key);
                        } else if (wParam == (IntPtr)KEY_UP || wParam == (IntPtr)WM_SYSKEY_UP) {
                            _onKeyUp(-key);
                        }
                    }
                    return (IntPtr)1;
                }
                return CallNextHookEx(_hook, nCode, wParam, lParam);
            };
        }

        public void StartScan() {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        public void StopScan() {
            UnhookWindowsHookEx(_hook);
        }

        public void PauseScan() {
            _pause = (IntPtr)1;
        }
        
        public void ResumeScan() {
            _pause = IntPtr.Zero;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
    }
}