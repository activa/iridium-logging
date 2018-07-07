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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iridium.Depend;

namespace Iridium.Logging
{
    public class LoggingProviderFile : LoggingProvider
    {
        private DateTime _lastCleanupTime = DateTime.MinValue;
        private readonly object _fileLock = new object();

        public string FileName { get; set; }
        public string LastUsedFileName { get; private set; }
        public bool LogRotation { get; set; }
        public TimeSpan MaxLogAge { get; set; }

        public LoggingProviderFile(string fileName)
        {
            MaxLogAge = TimeSpan.FromDays(7);
            FileName = fileName;
        }

        private string GenerateFileName(DateTime timeStamp)
        {
            return Regex.Replace(FileName, @"\$\((?<tag>[^\)]+)\)", m =>
                 {
                     string tag = m.Groups["tag"].Value.ToLower();

                     switch (tag)
                     {
                         case "day":
                             return timeStamp.Day.ToString("00");
                         case "month":
                             return timeStamp.Month.ToString("00");
                         case "year":
                             return timeStamp.Year.ToString("0000");
                         case "min":
                             return timeStamp.Minute.ToString("00");
                         case "hour":
                             return timeStamp.Hour.ToString("00");
                         case "sec":
                             return timeStamp.Second.ToString("00");
                         case "dow":
                             return timeStamp.DayOfWeek.ToString();
                     }
                     
                     return "";
                }, RegexOptions.Singleline);
        }

        private string GenerateFileName()
        {
            return GenerateFileName(DateTime.Now);
        }

        private void RemoveOldFiles()
        {
            if (!LogRotation)
                return;

            if ((DateTime.Now - _lastCleanupTime).TotalMinutes >= 60)
            {
                _lastCleanupTime = DateTime.Now;
            }
            else
            {
                return;
            }

            TimeSpan ts = TimeSpan.MinValue;
            DateTime date = DateTime.Now.Subtract(MaxLogAge);
            
            if (FileName.ToLower().IndexOf("$(sec)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromSeconds(1);
            else if (FileName.ToLower().IndexOf("$(min)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromMinutes(1);
            else if (FileName.ToLower().IndexOf("$(hour)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromHours(1);
            else if (FileName.ToLower().IndexOf("$(day)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromDays(1);
            else if (FileName.ToLower().IndexOf("$(month)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromDays(28);
            else if (FileName.ToLower().IndexOf("$(year)", StringComparison.Ordinal) >= 0)
                ts = TimeSpan.FromDays(365);
            if (ts != TimeSpan.MinValue)
                for (int i = 0; i < 500; i++)
                {
                    string fn = GenerateFileName(date);

                    FileIO.Delete(fn);
                    
                    date = date.Subtract(ts);
                }
        }

        public override void LogText(DateTime timeStamp, LogLevel logLevel, string s)
        {
            lock (_fileLock)
            {
                string fn = GenerateFileName();

                string timePart = FormatTime(timeStamp);

                s = s.Replace("\r", "").Replace("\n", "\n" + new string(' ',timePart.Length+3)).Replace("\n","\r\n");

                s = $"{timePart} | {s}";

                for (int i = 0; i < 10;i++ )
                    try
                    {
                        FileIO.AppendAllText(fn, s + "\r\n");

                        break;
                    }
                    catch (IOException)
                    {
                        Task.Delay(100).Wait();
                    }

                LastUsedFileName = fn;
                
                RemoveOldFiles();
            }
        }
    }
}