using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BailOutMode.Harmony_Patches
{
    [HarmonyPatch(typeof(GameEnergyUIPanel), nameof(GameEnergyUIPanel.RefreshEnergyUI),
        new Type[] {
        typeof(float)})]
    internal class GameEnergyUIPanel_RefreshEnergyUI
    { 
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if(code.opcode == OpCodes.Ldc_R4)
                {
                    if (code.operand?.Equals(Plugin.ZERO_ENERGY) ?? false)
                    {
                        Plugin.Log?.Warn($"Found Zero-Energy If");
                        code.operand = float.MinValue;
                    }
                    else
                        Plugin.Log?.Critical($"Ldc_R4 that wasn't Zero-Energy: {code.operand}");
                }
                yield return code;
            }
        }
    }




}
