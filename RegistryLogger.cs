using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BreakTimer
{
    class RegistryLogger
    {
        private const string SubKey = "BreakTimer";
        private const string ValueName = "LogData";
        private const int MaxLogLines = 10;

        public static void Log(string message)
        {
            try
            {
                using (RegistryKey key = GetOrCreateSubKey())
                {
                    Queue<string> logQueue = new Queue<string>(key.GetValue(ValueName) as string[] ?? Array.Empty<string>());

                    logQueue.Enqueue(FormatLogMessage(message));

                    while (logQueue.Count > MaxLogLines)
                    {
                        logQueue.Dequeue(); // Remove the oldest log line
                    }

                    // Save the updated log to the registry
                    key.SetValue(ValueName, logQueue.ToArray(), RegistryValueKind.MultiString);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions or log them to a different location if needed
                Console.WriteLine("Error logging to the registry: " + ex.Message);
            }
        }

        private static RegistryKey GetOrCreateSubKey()
        {
            // Create or get the registry sub-key
            return Registry.CurrentUser.CreateSubKey($"Software\\{SubKey}");
        }

        private static string FormatLogMessage(string message)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        }
    }
}