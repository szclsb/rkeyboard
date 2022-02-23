using System;
using System.Runtime.InteropServices;

namespace rkeyboard.WinUser {
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}