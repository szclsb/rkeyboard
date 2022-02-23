using System;

namespace rkeyboard.WinUser {
    [Flags]
    public enum InputType {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }
}