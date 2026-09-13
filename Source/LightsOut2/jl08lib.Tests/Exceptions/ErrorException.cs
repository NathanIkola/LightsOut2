using System;

namespace jl08lib.Tests.Exceptions
{
    internal class ErrorException : Exception
    {
        public ErrorException(string message)
            : base(message) { }
    }
}