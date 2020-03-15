using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BailOutMode
{

    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static string PluginName = "BailOutMode";
        private static Harmony harmony;

        [Init]
        public void Init(IPA.Logging.Logger logger)
        {
            Logger.log = logger;
            harmony = new Harmony("com.github.zingabopp.bailoutmode");
        }

        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.instance = conf.Generated<Configuration>();
            Logger.log.Debug("Config loaded"); 
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu(PluginName, "BailOutMode.UI.Settings.bsml", Configuration.instance);

        }

        [OnEnable]
        public void OnEnable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= BSEvents_gameSceneActive;
            BS_Utils.Utilities.BSEvents.gameSceneActive += BSEvents_gameSceneActive;
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.log.Error($"Error applying Harmony patches: {ex.Message}");
                Logger.log.Debug(ex.ToString());
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= BSEvents_gameSceneActive;
            if (BailOutController.instance != null)
            {
                GameObject.Destroy(BailOutController.instance);
            }
            try
            {
                harmony.UnpatchAll(harmony.Id);
            }
            catch (Exception ex)
            {
                Logger.log.Error($"Error removing Harmony patches: {ex.Message}");
                Logger.log.Debug(ex.ToString());
            }
        }



        private void BSEvents_gameSceneActive()
        {
            if (BailOutController.instance != null)
            {
                GameObject.Destroy(BailOutController.instance);
            }
            new GameObject("BailOutController").AddComponent<BailOutController>();
        }

        public void MenuLoadedFresh()
        {
            
        }
    }


}
