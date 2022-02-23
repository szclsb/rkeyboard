using System.Runtime.InteropServices;

namespace rkeyboard.WinUser {
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}