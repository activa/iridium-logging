using System;

namespace Iridium.Logging
{
    public class LoggingProviderDelegate : LoggingProvider
    {
        private bool _logTime = false;
        private readonly Action<DateTime, LogLevel, string> _action;

        public LoggingProviderDelegate(Action<DateTime, LogLevel, string> action)
        {
            _action = action;
        }

        public LoggingProviderDelegate(Action<LogLevel, string> action, bool logTime = false)
        {
            _logTime = logTime;

            _action = (time, level, s) =>
            {
                if (_logTime)
                {
                    string formattedTime = FormatTime(time);

                    action(level, formattedTime + " | " + s);

                }
                else
                    action(level, s);
            };
        }

        public override void LogText(DateTime timeStamp, LogLevel logLevel, string s)
        {
            _action(timeStamp, logLevel, s);
        }
    }
}