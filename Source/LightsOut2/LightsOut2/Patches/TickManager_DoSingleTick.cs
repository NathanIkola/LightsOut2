using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public class TickManager_DoSingleTick
    {
        public static void Prefix()
        {
            OnTick?.Invoke();
            if (DeferredTasks.Count == 0) return;

            Dictionary<Predicate, Action> failedTasks = new Dictionary<Predicate, Action>();
            foreach(KeyValuePair<Predicate, Action> pair in DeferredTasks)
            {
                // if the predicate fails, put it in the failed tasks dictionary to attempt next time
                if (!pair.Key.Invoke())
                    failedTasks.Add(pair.Key, pair.Value);
                // otherwise the predicate passed, so invoke the action
                else
                    pair.Value.Invoke();
            }

            // put any failed tasks back in the array
            DeferredTasks = failedTasks;
        }

        /// <summary>
        /// Adds a task to be performed at a later time, dictated by <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">A function which returns <see langword="true"/> when the task is ready to be performed</param>
        /// <param name="task">The task being deferred</param>
        public static void AddDeferredTask(Predicate predicate, Action task)
        {
            DeferredTasks.Add(predicate, task);
        }

        /// <summary>
        /// The template for the OnTick handler methods
        /// </summary>
        public delegate void TickHandler();

        /// <summary>
        /// The event fired every time a tick happens
        /// </summary>
        public static event TickHandler OnTick;

        /// <summary>
        /// A delegate that acts as a predicate for whether a deferred task can be run
        /// </summary>
        /// <returns><see langword="true"/> if the associated action is ready to run and can be removed from the list</returns>
        public delegate bool Predicate();
        private static Dictionary<Predicate, Action> DeferredTasks = new Dictionary<Predicate, Action>();
    }
}