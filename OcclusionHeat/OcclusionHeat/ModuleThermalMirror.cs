using UnityEngine;
using System.Linq;

namespace ThermalMirror
{
    /// <summary>
    /// A PartModule to forcefully make a part's temperature an exact mirror
    /// of a target part's temperature. This version uses a robust "StartsWith" search
    /// to be immune to in-flight part renaming.
    /// </summary>
    public class ModuleThermalMirror : PartModule
    {
        [KSPField(isPersistant = false)]
        public string targetPartName = "ShuttleOrbiter";

        private Part linkedPart;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            TryLinkPart();
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || part == null) return;

            if (linkedPart == null)
            {
                TryLinkPart();
            }

            if (linkedPart != null)
            {
                if (linkedPart.State == PartStates.DEAD)
                {
                    linkedPart = null;
                    return;
                }

                part.temperature = linkedPart.temperature;
                part.skinTemperature = linkedPart.skinTemperature;
                part.thermalMass = linkedPart.thermalMass;
                part.skinThermalMass = linkedPart.skinThermalMass;
            }
        }

        private void TryLinkPart()
        {
            if (vessel == null || targetPartName == "none" || !vessel.loaded) return;

            // --- THE DEFINITIVE FIX IS HERE ---
            // We now check if the part's name STARTS WITH our target name. This is immune
            // to KSP adding suffixes like "(STS-4A)" to the part name.
            linkedPart = vessel.parts.FirstOrDefault(p =>
                p.name.Trim().StartsWith(targetPartName.Trim(), System.StringComparison.InvariantCultureIgnoreCase));

            if (linkedPart != null)
            {
                // This message will now appear, confirming a successful link.
                UnityEngine.Debug.Log($"[ThermalMirror] {part.name} successfully linked to target part '{linkedPart.name}' using target name '{targetPartName}'.");
            }
            // No need for the failure log anymore, as this method is far more reliable.
        }

        public override string GetInfo()
        {
            if (targetPartName != "none")
            {
                return $"Mirrors the exact thermal state of the part named '{targetPartName}'.";
            }
            return "Does nothing unless a targetPartName is set.";
        }
    }
}