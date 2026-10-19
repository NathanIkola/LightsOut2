using LightsOut2.Core;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace LightsOut2.StandbyInfluencers
{
    /// <summary>
    /// Keeps a building active while a pawn is using a job that targets it.
    /// </summary>
    public sealed class InUseInfluencer : StandbyInfluencerBase
    {
        /// <summary>
        /// Creates an in-use influencer for the supplied building.
        /// </summary>
        /// <param name="parent">The building whose use state controls standby.</param>
        public InUseInfluencer(ThingWithComps parent)
            : base(parent) { }

        /// <summary>
        /// Returns whether no tracked job is currently using the parent building.
        /// </summary>
        public override bool WantsToBeInStandby => _usingJobs.Count == 0;

        /// <summary>
        /// Starts tracking a job that has arrived at or was already at its use position.
        /// </summary>
        /// <param name="driver">The job driver using the parent building.</param>
        public void NotifyJobArrived(JobDriver driver)
        {
            if (driver == null || driver.job?.targetA.Thing != _parent || !_usingJobs.Add(driver))
            {
                return;
            }

            driver.AddFinishAction(condition => NotifyJobEnded(driver));
        }

        /// <summary>
        /// Stops tracking a job that has finished using the parent building.
        /// </summary>
        /// <param name="driver">The completed job driver.</param>
        public void NotifyJobEnded(JobDriver driver)
        {
            if (driver != null)
            {
                _usingJobs.Remove(driver);
            }
        }

        /// <summary>
        /// Clears tracked jobs when this influencer is disposed.
        /// </summary>
        public override void Dispose()
        {
            _usingJobs.Clear();
            base.Dispose();
        }

        /// <summary>
        /// The jobs that have reached their use position for the parent building.
        /// </summary>
        private readonly HashSet<JobDriver> _usingJobs = new HashSet<JobDriver>();
    }
}
