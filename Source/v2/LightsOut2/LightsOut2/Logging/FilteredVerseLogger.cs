using jl08lib.Logging;

namespace LightsOut2.Logging
{
    /// <summary>
    /// A copy of the verse logger that filters messages based on the configured minimum log level
    /// </summary>
    public class FilteredVerseLogger : VerseLogger
    {
        public FilteredVerseLogger(string modName)
            : base(modName) { }

        protected override void Log(string message, LogLevel logLevel, bool onlyOnce)
        {
            if (logLevel >= LightsOut2Settings.MinimumLogLevel)
            {
                base.Log(message, logLevel, onlyOnce);
            }
        }
    }
}