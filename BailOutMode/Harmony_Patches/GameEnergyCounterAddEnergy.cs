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
        public static LevelFailedEffectController failController;

        static bool Prefix(GameEnergyCounter __instance, ref float value)
        {
            if (Plugin.IsEnabled && value < 0f)
            {
                //Console.WriteLine($"{value} < 0");
                if (__instance.energy + value <= 0)
                {
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
                    Plugin._numFails++;
                    //Console.WriteLine($"{__instance.energy} + {value} puts us <= 0");
                    value = (Plugin.EnergyResetAmount / 100f) - __instance.energy;
                    if (Plugin.ShowFailText)
                    {
                        try
                        {
                            if (failController == null)
                            {
                                failController = GameObject.FindObjectsOfType<LevelFailedEffectController>().FirstOrDefault();
                                if (failController == null)
                                {
                                    failController = new GameObject("LevelFailedEffectController").AddComponent<LevelFailedEffectController>();
                                }
                            }

                            if (failController != null)
                                failController.ShowLevelFailed();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                        }
                    }


                }
            }
            return true;
        }
    }

    class LevelFailedEffectController : MonoBehaviour
    {
        public static LevelFailedEffectController Instance;
        public static bool isHiding = false;
        private static LevelFailedTextEffect levelFailedText;
        public void Start()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
            isHiding = false;
            levelFailedText = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
        }

        public void ShowLevelFailed()
        {
            if (!isHiding)
            {
                levelFailedText.gameObject.SetActive(true);
                GameObject.FindObjectsOfType<LevelFailedTextEffect>().ToList().ForEach(tfx => {

                    tfx.ShowEffect();
                    if (Plugin.FailTextDuration > 0)
                        StartCoroutine(hideLevelFailed());
                    else
                        isHiding = true; // Fail text never hides, so don't try to keep showing it
                });
            }
        }

        public IEnumerator<WaitForSeconds> hideLevelFailed()
        {
            if (!isHiding)
            {
                isHiding = true;
                yield return new WaitForSeconds(Plugin.FailTextDuration);
                GameObject.FindObjectsOfType<LevelFailedTextEffect>().ToList().ForEach(f => { f.gameObject.SetActive(false); });
                isHiding = false;
            }
            yield return new WaitForSeconds(0);
        }
    }
}
