using System;

namespace jl08lib.Logging
{
    /// <summary>
    /// The various log levels
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// The most detailed messages
        /// </summary>
        Trace = 0,

        /// <summary>
        /// For debugging and development
        /// </summary>
        Debug = 1,

        /// <summary>
        /// General mod flow
        /// </summary>
        Information = 2,

        /// <summary>
        /// Abnormal or unexpected events
        /// </summary>
        Warning = 3,

        /// <summary>
        /// For errors and exceptions that cannot be handled within one operation
        /// </summary>
        Error = 4,

        /// <summary>
        /// For failures that require immediate attention
        /// </summary>
        Critical = 5,

        /// <summary>
        /// Specifies that no messages should be written
        /// </summary>
        None = 6,
    }

    /// <summary>
    /// An interface for logging in mods
    /// </summary>
    public abstract class LoggerBase
    {
        /// <summary>
        /// Logs a message with the Trace level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Trace(string message)
        {
            Log(message, LogLevel.Trace, false);
        }

        /// <summary>
        /// Logs a message with the Debug level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Debug(string message)
        {
            Log(message, LogLevel.Debug, false);
        }

        /// <summary>
        /// Logs a message with the Information level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Information(string message)
        {
            Log(message, LogLevel.Information, false);
        }

        /// <summary>
        /// Logs a message with the Warning level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Warning(string message)
        {
            Log(message, LogLevel.Warning, false);
        }
        
        /// <summary>
        /// Logs a warning, optionally only once
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="onlyOnce">Whether or not the message should be logged only once</param>
        public virtual void Warning(string message, bool onlyOnce)
        {
            Log(message, LogLevel.Warning, onlyOnce);
        }

        /// <summary>
        /// Logs a message with the Error level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Error(string message)
        {
            Log(message, LogLevel.Error, false);
        }

        /// <summary>
        /// Logs an error, optionally only once
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="onlyOnce">Whether or not the message should be logged only once</param>
        public virtual void Error(string message, bool onlyOnce)
        {
            Log(message, LogLevel.Error, onlyOnce);
        }

        /// <summary>
        /// Logs a message with the Critical level
        /// </summary>
        /// <param name="message">The message to log</param>
        public virtual void Critical(string message)
        {
            Log(message, LogLevel.Critical, false);
        }

        /// <summary>
        /// Asserts that the given truth value is true
        /// </summary>
        /// <param name="expr">The expression to check for truth</param>
        /// <param name="message">The message to log if truth is false explaining what failed</param>
        /// <param name="onlyOnce">Whether or not to only log this once</param>
        /// <returns>The value of expr</returns>
        public bool Assert(bool expr, string message, bool onlyOnce = false)
        {
            if (expr) { return true; }
            Error($"Assertion failed: {message}", onlyOnce);
            return false;
        }

        /// <summary>
        /// Asserts that the given object is not null
        /// </summary>
        /// <param name="obj">The object to check for null</param>
        /// <param name="objName">The name of the object being checked</param>
        /// <param name="onlyOnce">Whether or not to only log this once</param>
        /// <returns>True if the object is NOT null, false if the object is null</returns>
        public bool AssertNonNull<TObjType>(TObjType obj, string objName = null, bool onlyOnce = false)
        {
            if (obj != null) { return true; }

            // verify that we have a name to log
            if (string.IsNullOrWhiteSpace(objName))
            {
                objName = typeof(TObjType).Name;
            }

            string message = $"{objName} was null";
            return Assert(false, message, onlyOnce);
        }

        /// <summary>
        /// Opens a section for related log entries to be grouped within
        /// </summary>
        /// <param name="openMessage">The message to label the opening of the section</param>
        /// <param name="logLevel">The level to log the message at</param>
        /// <param name="closeMessage">The message to show when closing the section</param>
        public OpenedSection OpenSection(string openMessage, LogLevel logLevel, string closeMessage = null)
        {
            OpenSectionInner(openMessage, logLevel);
            return new OpenedSection(this, closeMessage, logLevel);
        }

        /// <summary>
        /// Closes the opened section
        /// </summary>
        /// <param name="openedSection">The opened section to close</param>
        public virtual void CloseSection(OpenedSection openedSection)
        {
            openedSection.Dispose();
        }

        /// <summary>
        /// Opens a section for related log entries to be grouped within
        /// </summary>
        /// <param name="openMessage">The message to label the opening of the section</param>
        /// <param name="logLevel">The level to log the message at</param>
        protected abstract void OpenSectionInner(string openMessage, LogLevel logLevel);

        /// <summary>
        /// Closes the last section that was opened
        /// </summary>
        protected abstract void CloseSectionInner(string closeMessage = null, LogLevel logLevel = LogLevel.None);

        /// <summary>
        /// Logs a message with the given log level 
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="logLevel">The log level</param>
        /// <param name="onlyOnce">Whether or not to only log once</param>
        protected abstract void Log(string message, LogLevel logLevel, bool onlyOnce);

        /// <summary>
        /// An opened section that can be disposed to automatically close a section
        /// </summary>
        public struct OpenedSection : IDisposable
        {
            /// <summary>
            /// Instantiates the opened section with the info needed to close it when disposed
            /// </summary>
            /// <param name="loggerBase">The logger to close</param>
            /// <param name="closeMessage">The message to show after closing the section</param>
            /// <param name="logLevel">The level to log the section close as</param>
            public OpenedSection(LoggerBase loggerBase, string closeMessage = null, LogLevel logLevel = LogLevel.None)
            {
                _loggerBase = loggerBase;
                _closeMessage = closeMessage;
                _logLevel = logLevel;
            }

            /// <summary>
            /// Closes the opened section
            /// </summary>
            public void Dispose()
            {
                _loggerBase?.CloseSectionInner(_closeMessage, _logLevel);
                _loggerBase = null;
                _closeMessage = null;
                _logLevel = LogLevel.None;
            }

            /// <summary>
            /// The logger to close the section on upon disposal
            /// </summary>
            private LoggerBase _loggerBase;

            /// <summary>
            /// The message to show after closing the section
            /// </summary>
            private string _closeMessage;

            /// <summary>
            /// The level to log the message at
            /// </summary>
            private LogLevel _logLevel;
        }
    }
}