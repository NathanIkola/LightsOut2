using System;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that simplifies having a ticking comp
    /// </summary>
    public abstract class TickingCompBase : ThingComp, IDisposable
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            Dispose();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            Dispose();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            RegisterTick();
        }

        /// <summary>
        /// The function invoked when ticking
        /// </summary>
        protected abstract void Tick();

        /// <summary>
        /// Whether or not it's still valid to be ticking
        /// </summary>
        protected virtual bool IsValidToTick()
        {
            return parent?.Spawned ?? false;
        }

        /// <summary>
        /// Disposes of this comp
        /// </summary>
        public virtual void Dispose()
        {
            UnregisterTick();
        }

        /// <summary>
        /// Registers this comp to receive ticking events from the static ticker instance
        /// </summary>
        private void RegisterTick()
        {
            LightsOut2Mod.StaticTicker.OnTick += ParentTick;
        }

        /// <summary>
        /// Unregisters the tick action for this instance
        /// </summary>
        private void UnregisterTick()
        {
            LightsOut2Mod.StaticTicker.OnTick -= ParentTick;
        }

        /// <summary>
        /// The function that wraps the tick to verify it's still okay
        /// </summary>
        private void ParentTick()
        {
            // if we aren't already in violation, then go ahead and tick
            bool isValid = IsValidToTick();
            if (isValid || !_previouslyViolated) 
            {
                // mark ourselves as in violation 
                _previouslyViolated = !isValid;
                Tick();
            }
            // otherwise it's likely invalid, but only log an error if it's still invalid across two separate ticks
            // (this helps deal with cases where the Tick event runs prior to the Thing's tick event, which means we may observe incorrect state for up to one tick)
            else if (LightsOut2Settings.EnableIntegrityChecks)
            {
                LightsOut2Mod.StaticLogger.Warning($"ThingComp with def '{parent.def.defName}' did not unregister from the static ticker before despawning");
            }
        }

        /// <summary>
        /// Whether or not this comp was previously had an integrity violation
        /// </summary>
        /// <remarks>
        /// We should wait at least one tick to see if there's some state update that
        /// simply hasn't happened yet (e.g., the ticking is processed before the parent Thing's tick runs)
        /// </remarks>
        private bool _previouslyViolated = false;
    }
}