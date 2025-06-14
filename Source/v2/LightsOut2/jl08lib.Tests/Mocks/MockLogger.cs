using jl08lib.Logging;
using jl08lib.Tests.Exceptions;

namespace jl08lib.Tests.Mocks
{
    internal class MockLogger : LoggerBase
    {
        protected override void CloseSectionInner(string closeMessage = null, LogLevel logLevel = LogLevel.None)
        {
            return;
        }

        protected override void Log(string message, LogLevel logLevel, bool onlyOnce)
        {
            // use exceptions to allow the tests to expect warnings or errors
            // while allowing unexpected exceptions to still cause failures
            switch (logLevel)
            {
                case LogLevel.Warning:
                    throw new WarningException(message);
                case LogLevel.Error:
                case LogLevel.Critical:
                    throw new ErrorException(message);
                default:
                    return;
            }
        }

        protected override void OpenSectionInner(string openMessage, LogLevel logLevel)
        {
            return;
        }
    }
}