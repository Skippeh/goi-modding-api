namespace ModAPI.Logging
{
    public sealed class LogOptions
    {
        public bool LogToConsole { get; set; } = true;
        public string LogDirectory { get; set; } = null;
    }
}