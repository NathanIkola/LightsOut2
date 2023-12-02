using LightsOut2.Debug;
using LightsOut2.Gizmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2.ThingComps
{
    public class StandbyLightComp : StandbyComp
    {
        public override void Initialize(CompProperties props)
        {
            IsEnabled = true;
            base.Initialize(props);
            KeepOnGizmo = new KeepOnGizmo();
            KeepOnGizmo.OnKeepOnChanged += OnKeepOnChangedHandler;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!IsEnabled) yield break;
            yield return KeepOnGizmo;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            bool keepOn = KeepOnGizmo.KeepOn;
            Scribe_Values.Look(ref keepOn, "KeepOn", false);
            KeepOnGizmo.KeepOn = keepOn;
        }

        public override bool IsInStandby
        {
            get { return !KeepOn && base.IsInStandby; }
            set { base.IsInStandby = value; }
        }

        /// <summary>
        /// Handler for the event fired when the keep on status changes
        /// </summary>
        /// <param name="newValue">Whether or not it is being kept on</param>
        private void OnKeepOnChangedHandler(bool newValue)
        {
            RaiseOnStandbyChanged(IsInStandby);
        }

        /// <summary>
        /// The gizmo that allows us to keep a light on
        /// </summary>
        public KeepOnGizmo KeepOnGizmo { get; set; }

        /// <summary>
        /// Whether or not this light is being kept on
        /// </summary>
        public bool KeepOn => KeepOnGizmo.KeepOn;

        public override float Rate
        {
            get
            {
                if (!IsEnabled) return 1f;
                return IsInStandby
                    ? LightsOut2Settings.MinDraw
                    : 1f;
            }
        }
    }
}