using System;

namespace ModAPI.UI.Win32Input
{
    [Flags]
    internal enum KF : int
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        EXTENDED = 0x0100,
        DLGMODE = 0x0800,
        MENUMODE = 0x1000,
        ALTDOWN = 0x2000,
        REPEAT = 0x4000,
        UP = 0x8000
        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo
    }
}