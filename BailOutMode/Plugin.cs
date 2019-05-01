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
    public struct DefaultSettings
    {
        public const bool IsEnabled = true;
        public const bool ShowFailEffect = true;
        public const bool RepeatFailEffect = false;
        public const bool DynamicSettings = false;
        public const int EnergyResetAmount = 50;
        public const float CounterTextSize = 15f;
        public const int FailEffectDuration = 3;

        public struct CounterTextPosition
        {
            public const float x = 0;
            public const float y = 0;
            public const float z = 2f;
            public static string AsString()
            {
                return $"{x},{y},{z}";
            }
        }
        
    }
    public class Plugin : IBeatSaberPlugin
    {
        public static string PluginName = "BailOutMode";
        public string Name => PluginName;
        public string Version => "1.3.0";

        private static bool _isEnabled = DefaultSettings.IsEnabled;
        private static bool _showFailEffect = DefaultSettings.ShowFailEffect;
        private static bool _repeatFailEffect = DefaultSettings.RepeatFailEffect;
        private static bool _dynamicSettings = DefaultSettings.DynamicSettings;
        private static float _counterTextSize = DefaultSettings.CounterTextSize;
        private static int _failEffectDuration = DefaultSettings.FailEffectDuration;
        private static int _energyReset = DefaultSettings.EnergyResetAmount;
        private static string _counterPosition = DefaultSettings.CounterTextPosition.AsString();
        public static BS_Utils.Utilities.Config config;

        private const string KeyBailOutMode = "BailOutModeEnabled";
        private const string KeyShowFailEffect = "ShowFailEffect";
        private const string KeyRepeatFailEffect = "RepeatFailEffect";
        private const string KeyFailEffectDuration = "FailEffectDuration";
        private const string KeyEnergyResetAmount = "EnergyResetAmount";
        private const string KeyCounterTextPosition = "FailCounterPosition";
        private const string KeyCounterTextSize = "FailCounterTextSize";
        private const string KeyDynamicSettings = "DynamicSettings";
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

            customUIExists = IPA.Loader.PluginManager.AllPlugins.FirstOrDefault(c => c.Metadata.Name == "Custom UI") != null;
            bsUtilsExists = IPA.Loader.PluginManager.AllPlugins.FirstOrDefault(c => c.Metadata.Name == "BS_Utils") != null;

            if (!bsUtilsExists)
            {
                Logger.Error($"Missing critical dependency: Beat Saber Utils, unable to start");
                return;
            }
            if (!customUIExists)
            {
                Logger.Warning($"Missing dependency: Beat Saber CustomUI, settings will not be available in game.");
            }
            if(config == null)
            {
                config = new BS_Utils.Utilities.Config("BailOutMode");
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
                BailOutController.numFails = 0;
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
                return _isEnabled;
            }
            set
            {
                config.SetBool(Plugin.PluginName, KeyBailOutMode, value);
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
                config.SetBool(Plugin.PluginName, KeyShowFailEffect, value);
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
                config.SetBool(Plugin.PluginName, KeyRepeatFailEffect, value);
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
                config.SetInt(Plugin.PluginName, KeyFailEffectDuration, value);
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
                config.SetInt(Plugin.PluginName, KeyEnergyResetAmount, _energyReset);
            }

        }

        public static string CounterTextPosition
        {
            get
            {
                if(DynamicSettings)
                {
                    _counterPosition = config.GetString(PluginName, KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());
                }
                return _counterPosition;
            }
            set { _counterPosition = value; }
        }

        public static float CounterTextSize
        {
            get
            {
                if (DynamicSettings)
                {
                    float val = config.GetFloat(Plugin.PluginName, Plugin.KeyCounterTextSize, DefaultSettings.CounterTextSize);
                    if (val <= 0)
                        val = DefaultSettings.CounterTextSize;
                    _counterTextSize = val;
                }
                return _counterTextSize;
            }
            set
            {
                if(value > 0)
                    _counterTextSize = value;
                else
                {
                    Logger.Error($"Invalid {KeyCounterTextSize}: {value}, must be > 0.");
                    _counterTextSize = DefaultSettings.CounterTextSize;
                }
            }
        }

        public static bool DynamicSettings
        {
            get { return _dynamicSettings; }
            set { _dynamicSettings = value; }
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
            /*
            string userDataPath = Environment.CurrentDirectory + "/UserData";
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }
            */
            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyBailOutMode, "")))
            {
                config.SetBool(Plugin.PluginName, Plugin.KeyBailOutMode, DefaultSettings.IsEnabled);
            }
            else
                IsEnabled = config.GetBool(Plugin.PluginName, Plugin.KeyBailOutMode, DefaultSettings.IsEnabled);
            
            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyShowFailEffect, "")))
            {
                config.SetBool(Plugin.PluginName, Plugin.KeyShowFailEffect, DefaultSettings.ShowFailEffect);
            }
            else
                ShowFailEffect = config.GetBool(Plugin.PluginName, Plugin.KeyShowFailEffect, DefaultSettings.ShowFailEffect);

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyRepeatFailEffect, "")))
            {
                config.SetBool(Plugin.PluginName, Plugin.KeyRepeatFailEffect, DefaultSettings.RepeatFailEffect);
            }
            else
                RepeatFailEffect = config.GetBool(Plugin.PluginName, Plugin.KeyRepeatFailEffect, DefaultSettings.RepeatFailEffect);

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyFailEffectDuration, "")))
            {
                config.SetInt(Plugin.PluginName, Plugin.KeyFailEffectDuration, DefaultSettings.FailEffectDuration);
            }
            else
                FailEffectDuration = config.GetInt(Plugin.PluginName, Plugin.KeyFailEffectDuration, DefaultSettings.FailEffectDuration);

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyEnergyResetAmount, "")))
            {
                config.SetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, DefaultSettings.EnergyResetAmount);
            }
            else
                EnergyResetAmount = config.GetInt(Plugin.PluginName, Plugin.KeyEnergyResetAmount, DefaultSettings.EnergyResetAmount);

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, "")))
            {
                config.SetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());
            }
            else
                CounterTextPosition = config.GetString(Plugin.PluginName, Plugin.KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyCounterTextSize, "")))
            {
                config.SetFloat(Plugin.PluginName, Plugin.KeyCounterTextSize, DefaultSettings.CounterTextSize);
            }
            else
                CounterTextSize = config.GetFloat(Plugin.PluginName, Plugin.KeyCounterTextSize, DefaultSettings.CounterTextSize);

            if ("".Equals(config.GetString(Plugin.PluginName, Plugin.KeyDynamicSettings, "")))
            {
                config.SetBool(Plugin.PluginName, Plugin.KeyDynamicSettings, DefaultSettings.DynamicSettings);
            }
            else
                DynamicSettings = config.GetBool(Plugin.PluginName, Plugin.KeyDynamicSettings, DefaultSettings.DynamicSettings);

            Logger.Debug("Settings:\n  IsEnabled={0}\n  ShowFailEffect={1}\n  FailEffectDuration={2}\n  EnergyResetAmount={3}\n  CounterPosition={4}\n CounterTextSize={5}\n DynamicSettings={6}",
                IsEnabled, ShowFailEffect, FailEffectDuration, EnergyResetAmount, CounterTextPosition, CounterTextSize, DynamicSettings);
        }

        
    }


}
