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
    [HarmonyPatch(typeof(StandardLevelGameplayManager), "HandleSongDidFinish",
    new Type[] { })]
    class StandardLevelGameplayManagerHandleSongDidFinish
    {
        static bool Prefix(StandardLevelGameplayManager __instance, ref StandardLevelGameplayManager.GameState ____gameState)
        {
            //Logger.Trace("In StandardLevelGameplayManager.HandleSongDidFinish()");
            try
            {
                bool enabled = false;
                if (BailOutController.instance == null)
                    return true;
                enabled = BailOutController.instance.IsEnabled;
                if (enabled && BailOutController.instance.numFails > 0)
                {
                    Logger.log.Debug("Fail detected in BailOutController, setting state to failed");
                    __instance.HandleGameEnergyDidReach0();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.log.Error($"Error in StandardLevelGameplayManagerHandleSongDidFinish: {ex.Message}");
                Logger.log.Debug(ex);
            }

            return true;
        }
    }
}
