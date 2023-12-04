using LightsOut2.Common;
using LightsOut2.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2.GlowerActuators
{
    /// <summary>
    /// A class that allows LightsOut to actuate a vanilla glower
    /// </summary>
    public class VanillaGlowerActuator : IGlowerActuator
    {
        public override void UpdateLit(ThingComp glower)
        {
            Map map = glower.parent?.Map;
            if (map is null) return;

            if (glower is CompGlower vanillaGlower)
            {
                vanillaGlower.UpdateLit(map);
                return;
            }

            // otherwise try updating lit on a modded glower
            MethodInfo updateLit = GetUpdateLitMethod(glower.GetType());
            DebugLogger.AssertFalse(updateLit is null, $"Failed to find UpdateLit method on type: {glower.GetType()}", true);
            if (updateLit is null) return;
            try
            {
                int paramLength = updateLit.GetParameters().Length;
                if (paramLength == 0)
                    updateLit.Invoke(glower, null);
                else if (paramLength == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                    updateLit.Invoke(glower, new object[] { map });
                else
                    DebugLogger.LogError($"Parameter error on type: {glower.GetType()}\n {updateLit.GetParameters()}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"Having trouble with modded glower: {glower.GetType()}\n {ex}");
            }
        }

        /// <summary>
        /// Retrieves the UpdateLit method for the given <paramref name="type"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check</param>
        /// <returns>The <see cref="MethodInfo"/> associated with the UpdateLit method of this type</returns>
        private MethodInfo GetUpdateLitMethod(Type type)
        {
            if (UpdateLitMethods.ContainsKey(type)) return UpdateLitMethods[type];
            MethodInfo method = type.GetMethod("UpdateLit", Utils.BindingFlags);
            UpdateLitMethods.Add(type, method);
            return method;
        }

        public static Dictionary<Type, MethodInfo> UpdateLitMethods = new Dictionary<Type, MethodInfo>();
    }
}