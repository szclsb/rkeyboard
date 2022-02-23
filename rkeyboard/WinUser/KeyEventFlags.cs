﻿using System;

namespace rkeyboard.WinUser {
    [Flags]
    public enum KeyEventFlags {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }
}