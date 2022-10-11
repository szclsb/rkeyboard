namespace rkeyboard.Input {
    public interface IKeyboard {
        void EmulateKeyDown(int vkey);
        void EmulateKeyUp(int vkey);
        void StartScan();
        void StopScan();
        void PauseScan();
        void ResumeScan();
    }
}