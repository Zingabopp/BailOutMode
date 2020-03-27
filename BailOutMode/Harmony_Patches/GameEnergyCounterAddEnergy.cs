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
    [HarmonyPatch(typeof(GameEnergyCounter), nameof(GameEnergyCounter.AddEnergy),
        new Type[] {
        typeof(float)})]
    class GameEnergyCounterAddEnergy
    {
        static bool Prefix(GameEnergyCounter __instance, ref float value)
        {
            //Logger.Trace("In GameEnergyCounter.AddEnergy()");
            bool enabled = false;
            if (BailOutController.instance == null)
                return true;
            enabled = BailOutController.instance.IsEnabled;
            if (enabled && value < 0f)
            {
                //Logger.Trace("Negative energy change detected: {0}", value);
                if (__instance.energy + value <= 0)
                {
                    //Logger.Debug("Fail detected. Current Energy: {0}, Energy Change: {1}", __instance.energy, value);
                    if (BS_Utils.Gameplay.ScoreSubmission.Disabled == false
                        || BailOutController.instance.numFails == 0)
                    {
                        Logger.log.Info("First fail detected, disabling score submission");
                        if (BailOutController.instance.numFails > 0)
                            Logger.log.Error($"Fail Counter is {BailOutController.instance.numFails}, but score submission wasn't disabled. This should not happen");
                        BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
                    }
                    BailOutController.instance.numFails++;
                    //Logger.Debug($"{__instance.energy} + {value} puts us <= 0");
                    value = (Configuration.instance.EnergyResetAmount / 100f) - __instance.energy;
                    //Logger.Debug("Changing value to {0} to raise energy to {1}", value, Plugin.EnergyResetAmount);
                    BailOutController.instance.ShowLevelFailed();
                }
            }
            return true;
        }
    }




}
