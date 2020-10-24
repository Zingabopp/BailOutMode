using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace BailOutMode.Harmony_Patches
{
    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.prevFrameRawScore), MethodType.Getter)]
    public class ScoreController_prevFrameRawScore
    {
        static bool Prefix(ref int ____prevFrameRawScore, ref int __result)
        {
            int lastRawScore = MultiplayerLocalActiveClient_ScoreControllerHandleScoreDidChange.lastRawScore;
            if (BailOutController.instance.numFails > 0 && lastRawScore >= 0)
            {
#if DEBUG
                Logger.log?.Debug($"Multiplayer Bailout detected. Overriding raw score '{____prevFrameRawScore}' with '{lastRawScore}'");
#endif
                __result = lastRawScore;
                return false;
            }
            return true;                
        }
    }

    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.prevFrameModifiedScore), MethodType.Getter)]
    public class ScoreController_prevprevFrameModifiedScore
    {
        static bool Prefix(ref int ____prevFrameRawScore, ref float ____gameplayModifiersScoreMultiplier, ref int __result)
        {
            int lastModifiedScore = MultiplayerLocalActiveClient_ScoreControllerHandleScoreDidChange.lastModifiedScore;
            if (BailOutController.instance.numFails > 0 && lastModifiedScore >= 0)
            {
                int modifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(____prevFrameRawScore, ____gameplayModifiersScoreMultiplier);
#if DEBUG
                Logger.log?.Debug($"Multiplayer Bailout detected. Overriding modified score '{modifiedScore}' with '{lastModifiedScore}'");
#endif
                __result = lastModifiedScore;
                return false;
            }
            return true;
        }
    }
}