using System;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that simplifies having a ticking comp
    /// </summary>
    public abstract class TickingCompBase : ThingComp, IDisposable
    {
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            LightsOut2Mod.StaticLogger.Trace($"PostDestroy for {parent}");
            base.PostDestroy(mode, previousMap);
            Dispose();
        }

        public override void PostDeSpawn(Map map)
        {
            LightsOut2Mod.StaticLogger.Trace($"PostDeSpawn for {parent}");
            base.PostDeSpawn(map);
            Dispose();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            LightsOut2Mod.StaticLogger.Trace($"PostSpawnSetup for {parent}");
            RegisterTick();
        }

        /// <summary>
        /// The function invoked when ticking
        /// </summary>
        protected abstract void Tick();

        /// <summary>
        /// Whether or not to allow ticking
        /// </summary>
        protected virtual bool AllowTicking()
        {
            return true;
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
            // if the comp doesn't want to tick right now, then just exit
            if (!AllowTicking()) { return; }
            // otherwise go ahead and tick
            Tick();

            // optionally perform integrity checking
            if (LightsOut2Settings.EnableIntegrityChecks)
            {
                // if the parent is currently spawned, there's no problem
                if (parent.Spawned)
                {
                    _hasParentEverSpawned = true;
                }
                // otherwise if the parent WAS spawned and isn't anymore, it should have unregistered
                else if (_hasParentEverSpawned)
                {
                    LightsOut2Mod.StaticLogger.Warning($"ThingComp with def '{parent.def.defName}' did not unregister from the static ticker before despawning");
                }
            }
        }

        /// <summary>
        /// Whether or not this comp's parent has been seen as spawned
        /// </summary>
        private bool _hasParentEverSpawned = false;
    }
}