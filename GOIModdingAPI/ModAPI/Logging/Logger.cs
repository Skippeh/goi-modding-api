using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ModAPI.Logging
{
    public sealed class Logger
    {
        public LogOptions Options { get; private set; }

        private readonly Stack<ConsoleColor> colorStack = new Stack<ConsoleColor>();

        private StreamWriter logFile;

        private readonly Dictionary<LogLevel, ConsoleColor> logColors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Info] = ConsoleColor.Gray,
            [LogLevel.Debug] = ConsoleColor.Yellow,
            [LogLevel.Warning] = ConsoleColor.DarkYellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Exception] = ConsoleColor.Red
        };

        public Logger(LogOptions options)
        {
            Options = options;

            if (Options.LogDirectory != null)
            {
                if (!Directory.Exists(Options.LogDirectory))
                    Directory.CreateDirectory(Options.LogDirectory);

                string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.txt";
                logFile = File.AppendText(Path.Combine(Options.LogDirectory, fileName));
            }
        }

        ~Logger()
        {
            logFile.Dispose();
            logFile = null;
        }
        
        public void LogInfo(object message)
        {
            Log(message, LogLevel.Info);
        }

        public void LogDebug(object message)
        {
            Log(message, LogLevel.Debug);
        }

        public void LogWarning(object message)
        {
            Log(message, LogLevel.Warning);
        }

        public void LogError(object message)
        {
            Log(message, LogLevel.Error);
        }

        public void LogException(Exception exception)
        {
            LogException(exception, null);
        }

        public void LogException(Exception exception, object message)
        {
            string strMessage = exception.ToString();

            if (message != null)
                strMessage = message + "\n" + strMessage;
            
            Log(strMessage, LogLevel.Exception);
        }

        private void Log(object message, LogLevel logLevel)
        {
            lock (this)
            {
                string strMessage = message.ToString();

                if (Options.LogToConsole)
                {
                    //PushConsoleColor(logColors[logLevel]);
                    Console.WriteLine(strMessage);
                    //PopConsoleColor();
                }

                if (Options.LogDirectory != null)
                {
                    logFile.WriteLine(strMessage);
                    logFile.Flush();
                }
            }
        }

        private void PushConsoleColor(ConsoleColor color)
        {
            colorStack.Push(Console.ForegroundColor);
            Console.ForegroundColor = color;
        }

        private void PopConsoleColor()
        {
            if (colorStack.Count == 0)
                return;
            
            Console.ForegroundColor = colorStack.Pop();
        }
    }
}