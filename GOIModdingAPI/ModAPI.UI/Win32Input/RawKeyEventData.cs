namespace ModAPI.UI.Win32Input
{
    internal class RawKeyEventData
    {
        public WM Message;
        public int WindowsKeyCode;
        public int NativeKeyCode;
        private bool intercept;

        public bool Intercept
        {
            get => intercept;
            set => intercept = intercept || value; // Only allow setting value to true.
        }
    }
}