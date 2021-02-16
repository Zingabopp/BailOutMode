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
    class GameEnergyCounterProcessEnergyChange
    {
        static bool Prefix(GameEnergyCounter __instance, ref float energyChange)
        {
            //Logger.Trace("In GameEnergyCounter.ProcessEnergyChange()");
            if (BailOutController.instance == null)
                return true;
            if (energyChange < 0f && BailOutController.instance.IsEnabled)
            {
                //Logger.Trace("Negative energy change detected: {0}", energyChange);
                if (__instance.energy + energyChange <= 1E-05f)
                {
                    // Logger.log?.Debug($"Fail detected. Current Energy: {__instance.energy}, Energy Change: {energyChange}");
                    if (BS_Utils.Gameplay.ScoreSubmission.Disabled == false
                        || BailOutController.instance.numFails == 0)
                    {
                        Logger.log.Info("First fail detected, disabling score submission");
                        if (BS_Utils.Gameplay.ScoreSubmission.Disabled)
                            Logger.log?.Debug($"ScoreSubmission already disabled by {BS_Utils.Gameplay.ScoreSubmission.ModString}");
                    }
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
                    if (!BS_Utils.Gameplay.ScoreSubmission.Disabled)
                        Logger.log.Error($"Told BS_Utils to disable submission, but it seems to still be enabled.");
                    BailOutController.instance.numFails++;
                    // Logger.log?.Debug($"{__instance.energy} + {energyChange} puts us <= 0");
                    energyChange = (Configuration.instance.EnergyResetAmount / 100f) - __instance.energy;
                    // Logger.log?.Debug($"Changing energyChange to {energyChange} to raise energy to {Configuration.instance.EnergyResetAmount}");
                    BailOutController.instance.OnLevelFailed();
                }
            }
            return true;
        }
    }




}
