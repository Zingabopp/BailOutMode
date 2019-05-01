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
        static void Prefix(StandardLevelGameplayManager __instance, ref StandardLevelGameplayManager.GameState ____gameState, ref Signal ____levelFailedSignal)
        {
            //Logger.Trace("In StandardLevelGameplayManager.HandleSongDidFinish()");
            try {
                bool enabled = false;
                if (BailOutController.Instance != null)
                    enabled = BailOutController.Instance.IsEnabled;
                if (enabled && BailOutController.numFails > 0)
                {
                    //Logger.Trace("Fail detected in BailOutController, setting state to failed");
                    ____gameState = StandardLevelGameplayManager.GameState.Failed;
                    ____levelFailedSignal.Raise();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error in StandardLevelGameplayManagerHandleSongDidFinish", ex);
            }

            return;
        }
    }
}
