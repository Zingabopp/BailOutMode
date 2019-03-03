using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BS_Utils;
using static BailOutMode.ReflectionUtil;

namespace BailOutMode
{
    class LevelFailedEffectController : MonoBehaviour
    {
        #region "Fields with get/setters"
        private static LevelFailedEffectController _instance;
        private static LevelFailedTextEffect _levelFailedText;
        private static StandardLevelGameplayManager _gameManager;
        private static GameEnergyCounter _energyCounter;
        #endregion
        #region "Fields"
        public static bool isHiding = false;

        private static LevelFailedTextEffect LevelFailedText
        {
            get
            {
                if (_levelFailedText == null)
                    _levelFailedText = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
                return _levelFailedText;
            }
            set { _levelFailedText = value; }
        }
        private static StandardLevelGameplayManager GameManager
        {
            get
            {
                if (_gameManager == null)
                    _gameManager = GameObject.FindObjectsOfType<StandardLevelGameplayManager>().FirstOrDefault();
                return _gameManager;
            }
        }
        private static GameEnergyCounter EnergyCounter
        {
            get
            {
                if (_energyCounter == null)
                    _energyCounter = GameObject.FindObjectsOfType<GameEnergyCounter>().FirstOrDefault();
                return _energyCounter;
            }
        }
        public static LevelFailedEffectController Instance
        {
            get
            {
                if (_instance == null)
                {
                    Logger.Debug("LevelFailedEffectContoller instance is null, creating new one");
                    _instance = new GameObject("LevelFailedEffectController").AddComponent<LevelFailedEffectController>();
                }
                return _instance;
            }
        }
        #endregion

        public void Awake()
        {
            Logger.Trace("LevelFailedEffectController Awake()");
            _instance = this;
            isHiding = false;
        }

        public void Start()
        {
            Logger.Trace("LevelFailedEffectController Start()");
            LevelFailedText = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
            if ((GameManager != null) && (EnergyCounter != null) && Plugin.IsEnabled)
            {
                Logger.Trace("Removing HandleGameEnergyDidReach0");
                EnergyCounter.gameEnergyDidReach0Event -= GameManager.HandleGameEnergyDidReach0;
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
            }

        }


        public void ShowLevelFailed()
        {
            Logger.Trace("LevelFailedEffectController ShowLevelFailed()");
            if (!isHiding)
            {
                LevelFailedText.gameObject.SetActive(true);
                LevelFailedText.ShowEffect();
                if (Plugin.FailTextDuration > 0)
                    StartCoroutine(hideLevelFailed());
                else
                    isHiding = true; // Fail text never hides, so don't try to keep showing it

            }
        }

        public IEnumerator<WaitForSeconds> hideLevelFailed()
        {
            Logger.Trace("LevelFailedEffectController hideLevelFailed() CoRoutine");
            if (!isHiding)
            {
                Logger.Trace($"LevelFailedEffectController, will hide LevelFailedTextEffect after {Plugin.FailTextDuration}s");
                isHiding = true;
                yield return new WaitForSeconds(Plugin.FailTextDuration);
                Logger.Trace($"LevelFailedEffectController, hiding LevelFailedTextEffect");
                LevelFailedText.gameObject.SetActive(false);
                isHiding = false;
            }
            else
                Logger.Trace("LevelFailedEffectController, skipping hideLevel because isHiding is true");
            yield break;
        }
    }
}
