using System;
using System.Runtime.InteropServices;

namespace rkeyboard.WinUser {
    public static class WinInput {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();
    }
}