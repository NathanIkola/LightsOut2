using Verse;

namespace LightsOut2.Debug
{
    /// <summary>
    /// A static class that holds all of the logging functionality needed by the mod
    /// </summary>
    public static class DebugLogger
    {
        /// <summary>
        /// Determines whether or not a message should be logged
        /// </summary>
        /// <returns><see langword="true"/> in debug mode, <see langword="false"/> otherwise</returns>
        private static bool ShouldLog()
        {
            return LightsOut2Settings.ShowDebugMessages;
        }

        /// <summary>
        /// Adds the debug header to a message
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <returns>The message with the debug header prepended</returns>
        private static string AddDebugHeader(string message)
        {
            return "<color=orange>[LightsOut 2 - Debug]</color> " + message;
        }

        /// <summary>
        /// Logs an informational message to the console.
        /// Should be used to log things that are good to
        /// know, but are not problematic
        /// </summary>
        /// <param name="message">The message to print</param>
        public static void LogInfo(string message)
        {
            Log.Message(AddDebugHeader(message));
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// Should be used to log things that could be
        /// issues, but aren't causing any fatal problems
        /// </summary>
        /// <param name="message">The message to print</param>
        public static void LogWarning(string message)
        {
            Log.Warning(AddDebugHeader(message));
        }

        /// <summary>
        /// Logs an error message to the console.
        /// Should be used to log when something has
        /// gone terribly, terribly wrong.
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="onlyOnce">If <see langword="true"/>, logs the error only one time</param>
        public static void LogError(string message, bool onlyOnce = false)
        {
            if (!onlyOnce)
                Log.Error(AddDebugHeader(message));
            else
                Log.ErrorOnce(AddDebugHeader(message), message.GetHashCode());
        }

        /// <summary>
        /// Logs an error message if <paramref name="expression"/> is <see langword="false"/>
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <param name="failMessage">The message to output if the assertion fails</param>
        /// <param name="onlyOnce">Only log the error one time</param>
        public static void Assert(bool expression, string failMessage, bool onlyOnce = false)
        {
            if (expression || !ShouldLog()) return;
            LogError(failMessage, onlyOnce);
        }

        /// <summary>
        /// Logs an error message if <paramref name="expression"/> is <see langword="true"/>
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <param name="failMessage">The message to output if the assertion fails</param>
        /// <param name="onlyOnce">Only log the error one time</param>
        public static void AssertFalse(bool expression, string failMessage, bool onlyOnce = false)
        {
            Assert(!expression, failMessage, onlyOnce);
        }
    }
}
