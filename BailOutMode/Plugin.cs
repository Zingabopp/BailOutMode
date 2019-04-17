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
using IPA;
using Harmony;
using System.Reflection;


namespace BailOutMode
{
    public class Plugin : IBeatSaberPlugin
    {
        public static string PluginName = "BailOutMode";
        public string Name => PluginName;
        public string Version => "1.2.0";

        private static bool _isEnabled = false;
        private static bool _showFailEffect = true;
        private static bool _repeatFailEffect = false;
        private static int _failEffectDuration = 5;
        private static int _energyReset = 50;
        private static string _counterPosition = "0,1.2,2.5";
        public static int _numFails = 0;

        private const string KeyBailOutMode = "BailOutModeEnabled";
        private const string KeyShowFailEffect = "ShowFailEffect";
        private const string KeyRepeatFailEffect = "RepeatFailEffect";
        private const string KeyFailEffectDuration = "FailEffectDuration";
        private const string KeyEnergyResetAmount = "EnergyResetAmount";
        private const string KeyCounterTextPosition = "FailCounterPosition";
        public const int nrgResetMin = 30;
        public const int nrgResetMax = 100;
        bool bsUtilsExists;
        bool customUIExists;
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
            Logger.LogLevel = LogLevel.Info;

            customUIExists = IPA.Loader.PluginManager.AllPlugins.FirstOrDefault(c => c.Metadata.Name == "BeatSaberCustomUI") != null;
            bsUtilsExists = IPA.Loader.PluginManager.AllPlugins.FirstOrDefault(c => c.Metadata.Name == "BS_Utils") != null;

            if (!bsUtilsExists)
            {
                Logger.Error($"Missing critical dependency: Beat Saber Utils, unable to start");
                return;
            }
            if (!customUIExists)
            {
                Logger.Warning($"Missing dependency: Beat Saber CustomUI, settings will not be available in game.");
                //return;
            }

            CheckForUserDataFolder();
            
            
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


        private void OnSceneTransitionFinish()
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
            //Create GameplayOptions/SettingsUI if using either
            if (customUIExists && scene.name == "MenuCore")
                UI.BailOutModeUI.CreateUI();
        }

        public void OnSceneUnloaded(Scene scene)
        {
            
        }

        public void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Logger.Trace($"In scene {newScene.name} from {oldScene.name}");
            if (BailOutController.InstanceExists)
            {
                GameObject.Destroy(BailOutController.Instance);
                Logger.Debug("Found controller onActiveSceneChanged, destroyed it");
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
                _numFails = 0;
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

        public static bool IsEnabled
        {
            get
            {
                if (BS_Utils.Gameplay.Gamemode.GameMode == "Campaign")
                    return false;
                return _isEnabled;
            }
            set
            {
                ModPrefs.SetBool(Plugin.PluginName, KeyBailOutMode, value);
                _isEnabled = value;
            }

        }

        public static bool ShowFailEffect
        {
            get
            {
                return _showFailEffect;
            }
            set
            {
                ModPrefs.SetBool(Plugin.PluginName, KeyShowFailEffect, value);
                _showFailEffect = value;
            }

        }

        public static bool RepeatFailEffect
        {
            get
            {
                return _repeatFailEffect;
            }
            set
            {
                ModPrefs.SetBool(Plugin.PluginName, KeyRepeatFailEffect, value);
                _repeatFailEffect = value;
            }

        }

        public static int FailEffectDuration
        {
            get
            {
                return _failEffectDuration;
            }
            set
            {
                ModPrefs.SetInt(Plugin.PluginName, KeyFailEffectDuration, value);
                _failEffectDuration = value;
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

        public static string CounterPosition
        {
            get { return _counterPosition; }
            set { _counterPosition = value; }
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

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyShowFailEffect, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyShowFailEffect, ShowFailEffect);
            }
            else
                ShowFailEffect = ModPrefs.GetBool(Plugin.PluginName, Plugin.KeyShowFailEffect, ShowFailEffect);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyRepeatFailEffect, "")))
            {
                ModPrefs.SetBool(Plugin.PluginName, Plugin.KeyRepeatFailEffect, RepeatFailEffect);
            }
            else
                RepeatFailEffect = ModPrefs.GetBool(Plugin.PluginName, Plugin.KeyRepeatFailEffect, RepeatFailEffect);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyFailEffectDuration, "")))
            {
                ModPrefs.SetInt(Plugin.PluginName, Plugin.KeyFailEffectDuration, FailEffectDuration);
            }
            else
                FailEffectDuration = ModPrefs.GetInt(Plugin.PluginName, Plugin.KeyFailEffectDuration, FailEffectDuration);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyEnergyResetAmount, "")))
            {
                ModPrefs.SetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, EnergyResetAmount);
            }
            else
                EnergyResetAmount = ModPrefs.GetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, EnergyResetAmount);

            if ("".Equals(ModPrefs.GetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, "")))
            {
                ModPrefs.SetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, CounterPosition);
            }
            else
                CounterPosition = ModPrefs.GetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, CounterPosition);

            Logger.Debug("Settings:\n  IsEnabled={0}\n  ShowFailEffect={1}\n  FailEffectDuration={2}\n  EnergyResetAmount={3}\n  CounterPosition={4}",
                IsEnabled, ShowFailEffect, FailEffectDuration, EnergyResetAmount, CounterPosition);
        }

        
    }


}
