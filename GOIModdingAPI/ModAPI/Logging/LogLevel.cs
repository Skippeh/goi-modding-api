using System;

namespace ModAPI.Logging
{
    [Flags]
    public enum LogLevel
    {
        Debug        = 1 << 0,
        Info         = 1 << 1,
        Warning      = 1 << 2,
        Error        = 1 << 3,
        Exception    = 1 << 4,
    }
}