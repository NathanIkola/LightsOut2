using System;

namespace jl08lib.Tests.Exceptions
{
    internal class WarningException : Exception
    {
        public WarningException(string message)
            : base(message) { }
    }
}