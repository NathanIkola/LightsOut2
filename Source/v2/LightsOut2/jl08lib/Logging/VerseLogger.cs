using System.Text;

namespace jl08lib.Logging
{
    public class VerseLogger : LoggerBase
    {
        /// <summary>
        /// Initializes the logger for RimWorld
        /// </summary>
        /// <param name="modName">The name of the mod the logger is for</param>
        public VerseLogger(string modName)
        {
            _modName = modName;
        }

        protected override void Log(string message, LogLevel logLevel, bool onlyOnce)
        {
            // craft the message to print
            message = ModPrefix(logLevel) + GetIndent() + " " + message;
            switch (logLevel)
            {
                case LogLevel.Warning:
                    if (onlyOnce) { Verse.Log.WarningOnce(message, message.GetHashCode()); }
                    else { Verse.Log.Warning(message); }
                    break;
                case LogLevel.Error:
                    if (onlyOnce) { Verse.Log.ErrorOnce(message, message.GetHashCode()); }
                    else { Verse.Log.Error(message); }
                    break;
                // critical errors are important, so don't allow logging only once
                case LogLevel.Critical:
                    Verse.Log.Error(message);
                    break;
                // these messages shouldn't log at all
                case LogLevel.None:
                    break;
                // for all other messages, logging once wouldn't really be appropriate
                default:
                    Verse.Log.Message(message);
                    break;
            }
        }

        protected override void OpenSectionInner(string openMessage, LogLevel logLevel)
        {
            Log(openMessage, logLevel, false);
            _indent += 1;
        }

        protected override void CloseSectionInner(string closeMessage = null, LogLevel logLevel = LogLevel.None)
        {
            _indent -= 1;
            if (!Assert(_indent >= 0, "Attempted to close more sections than were opened", false))
            {
                _indent = 0;
            }
            if (!string.IsNullOrWhiteSpace(closeMessage))
            {
                Log(closeMessage, logLevel, false);
            }
        }

        /// <summary>
        /// The name of the mod to log
        /// </summary>
        private readonly string _modName;

        /// <summary>
        /// The level of indentation to use when logging
        /// </summary>
        private int _indent = 0;

        /// <summary>
        /// Returns the mod prefix for the given log level
        /// </summary>
        /// <param name="logLevel">The log level to get the prefix for</param>
        /// <returns>The prefix based on the log level</returns>
        private string ModPrefix(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return $"<color=orange>{LogLevelPrefix(logLevel)}</color>";
                case LogLevel.None:
                    return string.Empty;
                default:
                    return LogLevelPrefix(logLevel);
            }
        }

        /// <summary>
        /// Retrieves the prefix for the given log level
        /// </summary>
        /// <param name="logLevel">The log level to get the prefix for</param>
        /// <returns>The prefix for the given level</returns>
        private string LogLevelPrefix(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return $"[{_modName} - Trace]";
                case LogLevel.Debug:
                    return $"[{_modName} - Debug]";
                case LogLevel.Information:
                    return $"[{_modName} - Info]";
                case LogLevel.Warning:
                    return $"[{_modName} - Warning]";
                case LogLevel.Error:
                    return $"[{_modName} - Error]";
                case LogLevel.Critical:
                    return $"[{_modName} - Critical]";
                case LogLevel.None:
                    return string.Empty;
                default:
                    return $"[{_modName}]";
            }
        }

        /// <summary>
        /// Retrieves the level of indent to use when logging a message
        /// </summary>
        /// <returns>The indent string (4 spaces per level of indentation)</returns>
        private string GetIndent()
        {
            if (_indent <= 0) { return string.Empty; }
            return "".PadRight(_indent * 4, ' ');
        }
    }
}