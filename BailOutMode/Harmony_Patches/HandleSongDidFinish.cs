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
    [HarmonyPatch(typeof(StandardLevelGameplayManager), nameof(StandardLevelGameplayManager.HandleSongDidFinish),
    new Type[] { })]
    class StandardLevelGameplayManagerHandleSongDidFinish
    {
        static bool Prefix(StandardLevelGameplayManager __instance, ref StandardLevelGameplayManager.GameState ____gameState)
        {
            //Logger.Trace("In StandardLevelGameplayManager.HandleSongDidFinish()");
            try
            {
                if (BailOutController.instance == null)
                    return true;
                if (BailOutController.instance.numFails > 0)
                {
                    Logger.log.Debug("Fail detected in BailOutController, forcing level failed");
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
