#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2017 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Text;

namespace Iridium.Logging
{
    public abstract class LoggingProvider : IDisposable
    {
        protected LoggingProvider()
        {
            TimeFormatString = "yyyy.MM.dd HH:mm:ss.ff";
        }

        public virtual LogLevel LogLevelMask { get; set; } = LogLevel.All;
        public virtual LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

        public string TimeFormatString { get; set; }

        public abstract void LogText(DateTime timeStamp, LogLevel logLevel, string s);

        public virtual void LogException(DateTime timeStamp, LogLevel logLevel, Exception e)
        {
            StringBuilder text = new StringBuilder();

            var innerException = e;

            text.AppendLine("===== EXCEPTION =====");

            while (innerException != null)
            {
                text.AppendLine($"{innerException.GetType().Name} : {innerException.Message}");
                text.AppendLine();
                text.AppendLine(innerException.StackTrace);
                text.AppendLine("---------------------");
            
                innerException = innerException.InnerException;
            }
            
            text.AppendLine("=====================");

            LogText(timeStamp, logLevel, text.ToString());
        }

        public virtual string FormatTime(DateTime time)
        {
            return time.ToString(TimeFormatString);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}