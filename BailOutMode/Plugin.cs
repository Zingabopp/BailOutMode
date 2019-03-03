using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using IllusionPlugin;
using Harmony;
using System.Reflection;


namespace BailOutMode
{
    public class Plugin : IPlugin
    {
        public static string PluginName = "BailOutMode";
        public string Name => PluginName;
        public string Version => "0.1.1";
        
        private static bool _isEnabled = false;
        private static bool _showFailText = true;
        private static int _failTextDuration = 5;
        private static int _energyReset = 50;
        public static int _numFails = 0;

        private const string KeyBailOutMode = "BailOutModeEnabled";
        private const string KeyShowFailText = "ShowFailText";
        private const string KeyFailTextDuration = "FailTextDuration";
        private const string KeyEnergyResetAmount = "EnergyResetAmount";
        public const int nrgResetMin = 30;
        public const int nrgResetMax = 100;
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

        public void OnApplicationStart()
        {
            Logger.LogLevel = LogLevel.Warn;
            CheckForUserDataFolder();
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            

            try
            {
                var harmony = HarmonyInstance.Create("com.github.zingabopp.bailoutmode");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
            catch (Exception ex)
            {
                Logger.Exception("This plugin requires Harmony. Make sure you " +
                    "installed the plugin properly, as the Harmony DLL should have been installed with it.", ex);
            }

        }

        private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
        {

            if (newScene.name == "Menu")
            {
                //Code to execute when entering The Menu


            }

            if (newScene.name == "GameCore")
            {
                //Code to execute when entering actual gameplay
                _gameScenesManager.transitionDidFinishEvent += OnSceneTransitionFinish;

                _numFails = 0;
            }


        }

        private void OnSceneTransitionFinish()
        {
            new GameObject("LevelFailedEffectController").AddComponent<LevelFailedEffectController>();
            _gameScenesManager.transitionDidFinishEvent -= OnSceneTransitionFinish;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            //Create GameplayOptions/SettingsUI if using either
            if (scene.name == "Menu")
                UI.BailOutModeUI.CreateUI();

        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
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


        }

        public void OnFixedUpdate()
        {
        }
        #endregion

        public static bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                ModPrefs.SetBool(Plugin.PluginName, KeyBailOutMode, value);
                _isEnabled = value;
            }

        }

        public static bool ShowFailText
        {
            get
            {
                return _showFailText;
            }
            set
            {
                ModPrefs.SetBool(Plugin.PluginName, KeyShowFailText, value);
                _showFailText = value;
            }

        }

        public static int FailTextDuration
        {
            get
            {
                return _failTextDuration;
            }
            set
            {
                ModPrefs.SetInt(Plugin.PluginName, KeyFailTextDuration, value);
                _failTextDuration = value;
            }
        }

        public static int EnergyResetAmount
        {
            get { return _energyReset; }
            set
            {
                if ((value > nrgResetMin))
                {
                    if (value <= nrgResetMax)
                        _energyReset = GetMultipleOfTen(value);
                    else
                        _energyReset = nrgResetMax;

                }
                ModPrefs.SetInt(Plugin.PluginName, KeyEnergyResetAmount, _energyReset);
            }

        }

        private static int GetMultipleOfTen(int value)
        {
            int remainder = value % 10;
            if (remainder == 0)
                return value;
            else
            {
                if ((10 - remainder) > 5)
                    return value - remainder;
                else
                    return value + (10 - remainder);
            }
        }

        private void CheckForUserDataFolder()
        {
            string userDataPath = Environment.CurrentDirectory + "/UserData";
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }
            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyBailOutMode, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyBailOutMode, IsEnabled);
            }
            else
                IsEnabled = ModPrefs.GetBool(Plugin.PluginName, Plugin.KeyBailOutMode, IsEnabled);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyShowFailText, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyShowFailText, ShowFailText);
            }
            else
                ShowFailText = ModPrefs.GetBool(Plugin.PluginName, Plugin.KeyShowFailText, ShowFailText);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyFailTextDuration, "")))
            {
                ModPrefs.SetInt(Plugin.PluginName, Plugin.KeyFailTextDuration, FailTextDuration);
            }
            else
                FailTextDuration = ModPrefs.GetInt(Plugin.PluginName, Plugin.KeyFailTextDuration, FailTextDuration);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyEnergyResetAmount, "")))
            {
                ModPrefs.SetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, EnergyResetAmount);
            }
            else
                EnergyResetAmount = ModPrefs.GetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, EnergyResetAmount);

            Logger.Debug("Settings:\n  IsEnabled={0}\n  ShowFailText={1}\n  FailTextDuration={2}\n  EnergyResetAmount={3}", IsEnabled, ShowFailText, FailTextDuration, EnergyResetAmount);
        }
    }


}
