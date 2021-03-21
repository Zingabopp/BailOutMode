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
    [HarmonyPatch(typeof(MultiplayerLocalActivePlayerGameplayManager), nameof(MultiplayerLocalActivePlayerGameplayManager.HandleSongDidFinish),
    new Type[] { })]
    internal class MultiplayerLocalActivePlayerGameplayManagerHandleSongDidFinish
    {
        static bool Prefix(MultiplayerLocalActivePlayerGameplayManager __instance)
        {
            //Logger.Trace("In StandardLevelGameplayManager.HandleSongDidFinish()");
            try
            {
                if (BailOutController.instance == null)
                    return true;
                if (BailOutController.instance.numFails > 0)
                {
                    Plugin.Log?.Debug("Fail detected in BailOutController, forcing level failed");
                    __instance.HandleGameEnergyDidReach0();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"Error in MultiplayerLocalActivePlayerGameplayManagerHandleSongDidFinish: {ex.Message}");
                Plugin.Log?.Debug(ex);
            }

            return false;
        }
    }
}
