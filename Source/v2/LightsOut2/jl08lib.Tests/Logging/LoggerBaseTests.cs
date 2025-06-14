using jl08lib.Logging;
using jl08lib.Tests.Exceptions;
using jl08lib.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jl08lib.Tests.Logging
{
    [TestClass]
    public class LoggerBaseTests
    {
        /// <summary>
        /// Ensures that the assertion statement correctly logs an error when passed in a false statement
        /// </summary>
        [TestMethod]
        public void Assert_False_Fails()
        {
            LoggerBase logger = new MockLogger();

            bool result = true;
            Assert.ThrowsException<ErrorException>(() =>
                result = logger.Assert(false, "False", false)
            );
        }

        /// <summary>
        /// Ensures that the assertion statement correctly passes an assert
        /// </summary>
        [TestMethod]
        public void Assert_True_Passes()
        {
            LoggerBase logger = new MockLogger();

            bool result = logger.Assert(true, "True", false);

            Assert.IsTrue(result, "Assertion should return the result of the input expression");
        }
    }
}