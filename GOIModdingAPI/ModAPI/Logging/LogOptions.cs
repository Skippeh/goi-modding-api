﻿namespace ModAPI.Logging
{
    public sealed class LogOptions
    {
        public bool LogToConsole { get; set; }
        public string LogDirectory { get; set; }
        public LogLevel MinLogLevel { get; internal set; }
    }
}