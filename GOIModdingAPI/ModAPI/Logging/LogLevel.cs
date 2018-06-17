using System;

namespace ModAPI.Logging
{
    [Flags]
    public enum LogLevel
    {
        Info         = 1 << 0,
        Debug        = 1 << 1,
        Warning      = 1 << 2,
        Error        = 1 << 3,
        Exception    = 1 << 4,
    }
}