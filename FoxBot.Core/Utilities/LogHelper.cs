using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.Core.Utilities
{
    public class LogHelper
    {
        private readonly Func<LogMessage, Task> _logger;
        private static readonly LogSeverity _debug = LogSeverity.Debug;
        private static readonly LogSeverity _info = LogSeverity.Info;
        private static readonly LogSeverity _critical = LogSeverity.Critical;

        public LogHelper(Func<LogMessage, Task> logger)
        {
            _logger = logger;
        }

        public Task LogAsync(LogMessage logMessage) => _logger.Invoke(logMessage);
        public Task LogAsync(string message, string logSource, LogSeverity logSeverity, Exception error = null) => _logger.Invoke(new LogMessage(logSeverity, logSource, message, error));
        public Task DebugAsync(string message, string logSource) => _logger.Invoke(new LogMessage(_debug, logSource, message));
        public Task InfoAsync(string message, string logSource) => _logger.Invoke(new LogMessage(_info, logSource, message));
        public Task CriticalAsync(string message, string logSource, Exception error = null) => _logger.Invoke(new LogMessage(_critical, logSource, message, error));
    }
}
