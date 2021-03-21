using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using BS_Utils;

namespace BailOutMode.Harmony_Patches
{
    [HarmonyPatch(typeof(GameEnergyCounter), nameof(GameEnergyCounter.ProcessEnergyChange),
        new Type[] {
        typeof(float)})]
    internal class GameEnergyCounterProcessEnergyChange
    {
        private static bool FailDetected = false;
        internal static void ResetFail()
        {
            FailDetected = false;
        }
        static bool Prefix(GameEnergyCounter __instance, ref float energyChange)
        {
            Plugin.Log?.Warn("In GameEnergyCounter.ProcessEnergyChange()");
            if (BailOutController.instance == null)
            {
#if DEBUG
                Plugin.Log?.Debug($"BailOutController instance is null");
#endif
                return true;
            }
            if (energyChange < 0f && BailOutController.instance.IsEnabled)
            {
                Plugin.Log?.Trace($"Negative energy change detected: {energyChange}");
                if (__instance.energy + energyChange <= Plugin.ZERO_ENERGY)
                {
                    Plugin.Log?.Debug($"Fail detected. Current Energy: {__instance.energy}, Energy Change: {energyChange}");
                    if (FailDetected)
                    {
                        Plugin.Log?.Debug("Resetting energy");
                        energyChange = (Configuration.instance.EnergyResetAmount / 100f) - __instance.energy;
                        ResetFail();
                        return true;
                    }
                    FailDetected = true;
                    if (BS_Utils.Gameplay.ScoreSubmission.Disabled == false
                        || BailOutController.instance.numFails == 0)
                    {
                        Plugin.Log?.Info("First fail detected, disabling score submission");
                        if (BS_Utils.Gameplay.ScoreSubmission.Disabled)
                            Plugin.Log?.Debug($"ScoreSubmission already disabled by {BS_Utils.Gameplay.ScoreSubmission.ModString}");
                    }
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
                    if (!BS_Utils.Gameplay.ScoreSubmission.Disabled)
                        Plugin.Log?.Error($"Told BS_Utils to disable submission, but it seems to still be enabled.");
                    BailOutController.instance.numFails++;
                    // Plugin.log?.Debug($"{__instance.energy} + {energyChange} puts us <= 0");
                    // Plugin.log?.Debug($"Changing energyChange to {energyChange} to raise energy to {Configuration.instance.EnergyResetAmount}");
                    BailOutController.instance.OnLevelFailed();
                }
            }
            else if (!BailOutController.instance.IsEnabled)
                Plugin.Log?.Debug("BailOutController not enabled.");
            else
                Plugin.Log?.Debug($"Energy change: {energyChange}");
            return true;
        }
    }




}
