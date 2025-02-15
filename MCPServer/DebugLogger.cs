using System;
using System.IO;
using System.Threading.Tasks;

namespace ModelContextProtocol
{
    internal static class DebugLogger
    {
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "debug_logs.txt");
        private static readonly object LogLock = new object();

        static DebugLogger()
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void Log(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
            lock (LogLock)
            {
                File.AppendAllText(LogPath, timestampedMessage);
            }
        }
    }
}