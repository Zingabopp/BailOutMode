using BailOutMode.Harmony_Patches;
using BeatSaberMarkupLanguage.GameplaySetup;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace BailOutMode
{

    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin instance = null!;
        internal static IPALogger? Log;
        internal static PluginMetadata? PluginMetadata;
        private static Harmony harmony = null!;
        internal const float ZERO_ENERGY = 1E-05f;
        private const string Resource_Settings_Path =         "BailOutMode.UI.Settings.bsml";
        private const string Resource_GameplaySettings_Path = "BailOutMode.UI.GameplaySettings.bsml";
        public static string PluginName = "BailOutMode";
        internal static event EventHandler? LevelStarted;
        internal Zenjector Zenjector;
        private bool gameplayTabEnabled = false;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            instance = this;
            Log = logger;
            harmony = new Harmony("com.github.zingabopp.bailoutmode");
            PluginMetadata = pluginMetadata;
            Zenjector = zenjector;
            zenjector.OnGame<BailOutInstaller>(false);
            //BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += MenuLoadedFresh;
        }

        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.instance = conf.Generated<Configuration>();
            SetGameplaySetupTab(Configuration.instance.EnableGameplayTab);
            Plugin.Log?.Debug("Config loaded"); 
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu(PluginName, Resource_Settings_Path, Configuration.instance);

        }

        [OnEnable]
        public void OnEnable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= BSEvents_gameSceneActive;
            BS_Utils.Utilities.BSEvents.gameSceneActive += BSEvents_gameSceneActive;
            //SetGameplaySetupTab(true);
        }

        [OnDisable]
        public void OnDisable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= BSEvents_gameSceneActive;
            if (BailOutController.instance != null)
            {
                GameObject.Destroy(BailOutController.instance);
            }
        }



        private void BSEvents_gameSceneActive()
        {
            if (BailOutController.instance != null)
            {
                GameObject.Destroy(BailOutController.instance);
            }
            //new GameObject("BailOutController").AddComponent<BailOutController>();
            LevelStarted?.Invoke(this, EventArgs.Empty);
        }

        internal void SetGameplaySetupTab(bool enabled)
        {
            if(enabled != gameplayTabEnabled)
            {
                if (enabled)
                {
                    Plugin.Log?.Debug($"Enabling GameplaySetup tab.");
                    GameplaySetup.instance.AddTab(PluginName, Resource_GameplaySettings_Path, Configuration.instance);
                    gameplayTabEnabled = true;
                }
                else
                {
                    Plugin.Log?.Debug($"Disabling GameplaySetup tab.");
                    GameplaySetup.instance.RemoveTab(PluginName);
                    gameplayTabEnabled = false;
                }
            }
        }
    }


}
