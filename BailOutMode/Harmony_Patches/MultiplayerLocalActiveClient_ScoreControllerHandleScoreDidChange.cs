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
    /// <summary>
    /// This patches ClassToPatch.MethodToPatch(Parameter1Type arg1, Parameter2Type arg2)
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLocalActiveClient), nameof(MultiplayerLocalActiveClient.ScoreControllerHandleScoreDidChange),
        new Type[] { // List the Types of the method's parameters.
        typeof(int),
        typeof(int)})]
    internal class MultiplayerLocalActiveClient_ScoreControllerHandleScoreDidChange
    {
        MultiplayerLocalActiveClient_ScoreControllerHandleScoreDidChange()
        {
            Plugin.LevelStarted += OnLevelStarted;
        }

        public static int lastRawScore { get; private set; } = -1;
        public static int lastModifiedScore { get; private set; } = -1;

        public static void ResetLastScores()
        {
            lastRawScore = -1;
            lastModifiedScore = -1;
        }

        private static void OnLevelStarted(object s, EventArgs _)
        {
            ResetLastScores();
        }

        static bool Prefix(ref int rawScore, ref int modifiedScore)
        {
            if (BailOutController.instance.numFails > 0)
                return false;
            lastRawScore = rawScore;
            lastModifiedScore = modifiedScore;
            return true;
        }
    }
}