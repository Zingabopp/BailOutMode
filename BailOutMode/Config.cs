using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;

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
    public class Config
    {
        private static Config _instance;
        internal static Config instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Config();
                return _instance;
            }
        }
        internal BS_Utils.Utilities.Config config;

        private bool _isEnabled = DefaultSettings.IsEnabled;
        private bool _showFailEffect = DefaultSettings.ShowFailEffect;
        private bool _repeatFailEffect = DefaultSettings.RepeatFailEffect;
        private bool _dynamicSettings = DefaultSettings.DynamicSettings;
        private float _counterTextSize = DefaultSettings.CounterTextSize;
        private int _failEffectDuration = DefaultSettings.FailEffectDuration;
        private int _energyReset = DefaultSettings.EnergyResetAmount;
        private string _counterPosition = DefaultSettings.CounterTextPosition.AsString();

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

        [UIValue("IsEnabled")]
        public bool IsEnabled
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

        [UIValue("ShowFailEffect")]
        public bool ShowFailEffect
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

        [UIValue("RepeatFailEffect")]
        public bool RepeatFailEffect
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

        [UIValue("FailEffectDuration")]
        public int FailEffectDuration
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

        [UIValue("EnergyResetAmount")]
        public int EnergyResetAmount
        {
            get { return _energyReset; }
            set
            {
                if ((value >= nrgResetMin))
                {
                    if (value <= nrgResetMax)
                        _energyReset = value;
                    else
                        _energyReset = nrgResetMax;

                }
                config.SetInt(Plugin.PluginName, KeyEnergyResetAmount, _energyReset);
            }

        }

        [UIValue("CounterTextPosition")]
        public string CounterTextPosition
        {
            get
            {
                if (DynamicSettings)
                {
                    _counterPosition = config.GetString(Plugin.PluginName, KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());
                }
                return _counterPosition;
            }
            set 
            {
                if (_counterPosition == value)
                    return;
                _counterPosition = value;
                config.SetString(Plugin.PluginName, KeyCounterTextPosition, value);
            }
        }

        [UIValue("CounterTextSize")]
        public float CounterTextSize
        {
            get
            {
                if (DynamicSettings)
                {
                    float val = config.GetFloat(Plugin.PluginName, KeyCounterTextSize, DefaultSettings.CounterTextSize);
                    if (val <= 0)
                        val = DefaultSettings.CounterTextSize;
                    _counterTextSize = val;
                }
                return _counterTextSize;
            }
            set
            {
                if (_counterTextSize == value)
                    return;
                var oldValue = _counterTextSize;
                if (value > 0)
                    _counterTextSize = value;
                else
                {
                    Logger.log.Error($"Invalid {KeyCounterTextSize}: {value}, must be > 0.");
                    _counterTextSize = DefaultSettings.CounterTextSize;
                }
                if (_counterTextSize == oldValue)
                    return;
                config.SetFloat(Plugin.PluginName, KeyCounterTextSize, _counterTextSize);
            }
        }

        [UIValue("DynamicSettings")]
        public bool DynamicSettings
        {
            get { return _dynamicSettings; }
            set { _dynamicSettings = value; }
        }

        internal void CheckForUserDataFolder()
        {
            /*
            string userDataPath = Environment.CurrentDirectory + "/UserData";
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }
            */
            if ("".Equals(config.GetString(Plugin.PluginName, KeyBailOutMode, "")))
            {
                config.SetBool(Plugin.PluginName, KeyBailOutMode, DefaultSettings.IsEnabled);
            }
            else
                IsEnabled = config.GetBool(Plugin.PluginName, KeyBailOutMode, DefaultSettings.IsEnabled);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyShowFailEffect, "")))
            {
                config.SetBool(Plugin.PluginName, KeyShowFailEffect, DefaultSettings.ShowFailEffect);
            }
            else
                ShowFailEffect = config.GetBool(Plugin.PluginName, KeyShowFailEffect, DefaultSettings.ShowFailEffect);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyRepeatFailEffect, "")))
            {
                config.SetBool(Plugin.PluginName, KeyRepeatFailEffect, DefaultSettings.RepeatFailEffect);
            }
            else
                RepeatFailEffect = config.GetBool(Plugin.PluginName, KeyRepeatFailEffect, DefaultSettings.RepeatFailEffect);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyFailEffectDuration, "")))
            {
                config.SetInt(Plugin.PluginName, KeyFailEffectDuration, DefaultSettings.FailEffectDuration);
            }
            else
                FailEffectDuration = config.GetInt(Plugin.PluginName, KeyFailEffectDuration, DefaultSettings.FailEffectDuration);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyEnergyResetAmount, "")))
            {
                config.SetInt(Plugin.PluginName, KeyEnergyResetAmount, DefaultSettings.EnergyResetAmount);
            }
            else
                EnergyResetAmount = config.GetInt(Plugin.PluginName, KeyEnergyResetAmount, DefaultSettings.EnergyResetAmount);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyCounterTextPosition, "")))
            {
                config.SetString(Plugin.PluginName, KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());
            }
            else
                CounterTextPosition = config.GetString(Plugin.PluginName, KeyCounterTextPosition, DefaultSettings.CounterTextPosition.AsString());

            if ("".Equals(config.GetString(Plugin.PluginName, KeyCounterTextSize, "")))
            {
                config.SetFloat(Plugin.PluginName, KeyCounterTextSize, DefaultSettings.CounterTextSize);
            }
            else
                CounterTextSize = config.GetFloat(Plugin.PluginName, KeyCounterTextSize, DefaultSettings.CounterTextSize);

            if ("".Equals(config.GetString(Plugin.PluginName, KeyDynamicSettings, "")))
            {
                config.SetBool(Plugin.PluginName, KeyDynamicSettings, DefaultSettings.DynamicSettings);
            }
            else
                DynamicSettings = config.GetBool(Plugin.PluginName, KeyDynamicSettings, DefaultSettings.DynamicSettings);

            Logger.log.Debug(string.Format("Settings:\n  IsEnabled={0}\n  ShowFailEffect={1}\n  FailEffectDuration={2}\n  EnergyResetAmount={3}\n  CounterPosition={4}\n  CounterTextSize={5}\n  DynamicSettings={6}",
                IsEnabled, ShowFailEffect, FailEffectDuration, EnergyResetAmount, CounterTextPosition, CounterTextSize, DynamicSettings));
        }

    }
}
