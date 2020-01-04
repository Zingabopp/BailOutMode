using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPA;
using Harmony;
using System.Reflection;


namespace BailOutMode
{

    public class Plugin : IBeatSaberPlugin
    {
        public static string PluginName = "BailOutMode";

        private GameScenesManager _scenesManager;
        public GameScenesManager _gameScenesManager
        {
            get
            {
                if (_scenesManager == null)
                {
                    _scenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
                }
                return _scenesManager;
            }
        }

        public void Init(IPA.Logging.Logger logger)
        {
            Logger.log = logger;
        }

        public void OnApplicationStart()
        {
            if (Config.instance.config == null)
            {
                Config.instance.config = new BS_Utils.Utilities.Config("BailOutMode");
            }
            Config.instance.CheckForUserDataFolder();
            BS_Utils.Utilities.BSEvents.menuSceneLoadedFresh += MenuLoadedFresh;
            try
            {
                var harmony = HarmonyInstance.Create("com.github.zingabopp.bailoutmode");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.log.Error($"Error applying Harmony patches: {ex.Message}");
                Logger.log.Debug(ex.ToString());
            }

        }

        private void OnSceneTransitionFinish(ScenesTransitionSetupDataSO arg1, Zenject.DiContainer arg2)
        {
            //Logger.Debug("OnSceneTransitionFinished: Creating new BailOutController");
            new GameObject("BailOutController").AddComponent<BailOutController>();
            _gameScenesManager.transitionDidFinishEvent -= OnSceneTransitionFinish;
        }

        public void OnApplicationQuit()
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void MenuLoadedFresh()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu(PluginName, "BailOutMode.UI.Settings.bsml", Config.instance);
        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Logger.log.Debug($"In scene {newScene.name} from {oldScene.name}");
            if (BailOutController.instance != null)
            {
                GameObject.Destroy(BailOutController.instance);
                Logger.log.Debug("Found controller onActiveSceneChanged, destroyed it");
            }
            if (newScene.name == "MenuCore")
            {
                //Code to execute when entering The Menu
                //var boCtrl = new GameObject("BailOutController").AddComponent<BailOutController>();
                //boCtrl.FailText.text = "Testing";

            }

            if (newScene.name == "GameCore")
            {
                //Code to execute when entering actual gameplay
                _gameScenesManager.transitionDidFinishEvent += OnSceneTransitionFinish;
            }
        }

        #region "Unused"
        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
            /*
            if(UnityEngine.Input.GetKeyDown(KeyCode.U))
            {
                Console.WriteLine($"GameMode: {BS_Utils.Gameplay.Gamemode.GameMode}");
            }
            */
        }

        public void OnFixedUpdate()
        {
        }
        #endregion
    }


}
