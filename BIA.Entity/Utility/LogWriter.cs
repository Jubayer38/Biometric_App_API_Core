using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BIA.Entity.Utility
{
    public class LogWriter
    {
        private readonly ILogger<LogWriter> _logger;
        private readonly string _logDirectory;

        public LogWriter(ILogger<LogWriter> logger, string logPath)
        {
            _logger = logger;
            _logDirectory = logPath;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
       
        public void WriteDailyLog2(string message)
        {
            try
            {
                string fileName = $"{DateTime.Now:yyyy-MM-dd}.txt";
                string filePath = Path.Combine(_logDirectory, fileName);

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

                File.AppendAllText(filePath, logEntry);

                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write log");
            }
        }
    }
}
