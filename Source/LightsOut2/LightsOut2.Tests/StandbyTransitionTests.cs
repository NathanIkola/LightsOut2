using LightsOut2.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightsOut2.Tests
{
    /// <summary>
    /// Tests standby decisions and timing without game startup.
    /// </summary>
    [TestClass]
    public sealed class StandbyTransitionTests
    {
        /// <summary>
        /// A zero delay enters standby on the first requesting tick.
        /// </summary>
        [TestMethod]
        public void ZeroDelayTransitionsImmediately()
        {
            var transition = new StandbyTransition();
            Assert.IsTrue(transition.Tick(true, 0));
            Assert.IsTrue(transition.InStandby);
            Assert.IsFalse(transition.Tick(true, 0));
        }

        /// <summary>
        /// A positive delay counts each desired tick once.
        /// </summary>
        [TestMethod]
        public void DelayDoesNotRestartWhileDesired()
        {
            var transition = new StandbyTransition();
            Assert.IsFalse(transition.Tick(true, 3));
            Assert.IsFalse(transition.Tick(true, 3));
            Assert.IsTrue(transition.Tick(true, 3));
            Assert.IsTrue(transition.InStandby);
        }

        /// <summary>
        /// An active interval cancels a pending delay.
        /// </summary>
        [TestMethod]
        public void ActiveIntervalCancelsPendingStandby()
        {
            var transition = new StandbyTransition();
            Assert.IsFalse(transition.Tick(true, 3));
            Assert.IsFalse(transition.Tick(false, 3));
            Assert.IsFalse(transition.Tick(true, 3));
            Assert.IsFalse(transition.Tick(true, 3));
            Assert.IsTrue(transition.Tick(true, 3));
        }

        /// <summary>
        /// Leaving active standby takes effect on the same tick.
        /// </summary>
        [TestMethod]
        public void LeavingStandbyIsImmediate()
        {
            var transition = new StandbyTransition();
            transition.Tick(true, 0);
            Assert.IsTrue(transition.Tick(false, 12));
            Assert.IsFalse(transition.InStandby);
            Assert.IsFalse(transition.DesiresStandby);
        }

    }
}
