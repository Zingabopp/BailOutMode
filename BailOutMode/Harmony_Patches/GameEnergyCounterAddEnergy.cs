using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using BS_Utils;
using static BailOutMode.ReflectionUtil;

namespace BailOutMode.Harmony_Patches
{
    [HarmonyPatch(typeof(GameEnergyCounter), "AddEnergy",
        new Type[] {
        typeof(float)})]
    class GameEnergyCounterAddEnergy
    {
        static bool Prefix(GameEnergyCounter __instance, ref float value)
        {
            if (Plugin.IsEnabled && value < 0f)
            {
                Logger.Trace("Negative energy change detected: {0}", value);
                if (__instance.energy + value <= 0)
                {
                    Logger.Debug("Fail detected. Current Energy: {0}, Energy Change: {1}", __instance.energy, value);
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
                    Plugin._numFails++;
                    Logger.Debug($"{__instance.energy} + {value} puts us <= 0");
                    value = (Plugin.EnergyResetAmount / 100f) - __instance.energy;
                    Logger.Debug("Changing value to {0} to raise energy to {1}", value, Plugin.EnergyResetAmount);
                    if (Plugin.ShowFailText)
                    {
                        try
                        {
                            Logger.Debug("Trying to show LevelFailedText");
                            LevelFailedEffectController.Instance.ShowLevelFailed();
                        }
                        catch (Exception ex)
                        {
                            Logger.Exception("Exception trying to show the fail text", ex);
                        }
                    }
                }
            }
            return true;
        }
    }

   
}
