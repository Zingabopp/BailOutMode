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

namespace BailOutMode
{

    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin instance;
        private const string Resource_Settings_Path =         "BailOutMode.UI.Settings.bsml";
        private const string Resource_GameplaySettings_Path = "BailOutMode.UI.GameplaySettings.bsml";
        public static string PluginName = "BailOutMode";
        private static Harmony harmony;
        internal static PluginMetadata PluginMetadata = null;
        internal static event EventHandler LevelStarted;
        internal Zenjector Zenjector;
        private bool gameplayTabEnabled = false;

        [Init]
        public Plugin(IPA.Logging.Logger logger, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            instance = this;
            Logger.log = logger;
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
            Logger.log.Debug("Config loaded"); 
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu(PluginName, Resource_Settings_Path, Configuration.instance);

        }

        [OnEnable]
        public void OnEnable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= BSEvents_gameSceneActive;
            BS_Utils.Utilities.BSEvents.gameSceneActive += BSEvents_gameSceneActive;
            //SetGameplaySetupTab(true);
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
            //new GameObject("BailOutController").AddComponent<BailOutController>();
            LevelStarted?.Invoke(this, EventArgs.Empty);
        }

        internal void SetGameplaySetupTab(bool enabled)
        {
            if(enabled != gameplayTabEnabled)
            {
                if (enabled)
                {
                    Logger.log?.Debug($"Enabling GameplaySetup tab.");
                    GameplaySetup.instance.AddTab(PluginName, Resource_GameplaySettings_Path, Configuration.instance);
                    gameplayTabEnabled = true;
                }
                else
                {
                    Logger.log?.Debug($"Disabling GameplaySetup tab.");
                    GameplaySetup.instance.RemoveTab(PluginName);
                    gameplayTabEnabled = false;
                }
            }
        }
    }


}
