using System.Runtime.InteropServices;

namespace ModAPI.UI.Win32Input.Data
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public long x;
        public long y;

        public override string ToString()
        {
            return $"{{X: {x}, Y: {y}}}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Int32POINT
    {
        public int x;
        public int y;

        public override string ToString()
        {
            return $"{{X: {x}, Y: {y}}}";
        }
    }
}