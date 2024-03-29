﻿using System;
using System.Runtime.InteropServices;

namespace rkeyboard.WinUser {
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}