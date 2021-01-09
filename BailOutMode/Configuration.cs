using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using BeatSaberMarkupLanguage.Attributes;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BailOutMode
{
    public struct DefaultSettings
    {
        public const bool IsEnabled = true;
        public const bool EnableGameplayTab = true;
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
    internal class Configuration
    {
        internal static Configuration instance;
        private float _counterTextSize = DefaultSettings.CounterTextSize;
        private int _energyReset = DefaultSettings.EnergyResetAmount;
        private bool _enableGameplayTab = DefaultSettings.EnableGameplayTab;
        public const int nrgResetMin = 30;
        public const int nrgResetMax = 100;

        [UIValue("IsEnabled")]
        public virtual bool IsEnabled { get; set; } = DefaultSettings.IsEnabled;

        [UIValue("EnableGameplayTab")]
        public virtual bool EnableGameplayTab
        {
            get => _enableGameplayTab;
            set
            {
                if (_enableGameplayTab == value)
                    return;
                _enableGameplayTab = value;
                Plugin.instance.SetGameplaySetupTab(value);
            }
        }
        [UIValue("ShowFailEffect")]
        public virtual bool ShowFailEffect { get; set; } = DefaultSettings.ShowFailEffect;

        [UIValue("RepeatFailEffect")]
        public virtual bool RepeatFailEffect { get; set; } = DefaultSettings.RepeatFailEffect;

        [UIValue("FailEffectDuration")]
        public virtual int FailEffectDuration { get; set; } = DefaultSettings.FailEffectDuration;

        [UIValue("EnergyResetAmount")]
        public virtual int EnergyResetAmount
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
            }

        }

        [UIValue("CounterTextPosition")]
        public virtual string CounterTextPosition { get; set; } = DefaultSettings.CounterTextPosition.AsString();

        [UIValue("CounterTextSize")]
        public virtual float CounterTextSize
        {
            get
            {
                return _counterTextSize;
            }
            set
            {
                if (_counterTextSize == value)
                    return;
                float oldValue = _counterTextSize;
                if (value > 0)
                    _counterTextSize = value;
                else
                {
                    Logger.log.Error($"Invalid CounterTextSize value: {value}, must be > 0.");
                    _counterTextSize = DefaultSettings.CounterTextSize;
                }
                if (_counterTextSize == oldValue)
                    return;
            }
        }
    }
}
