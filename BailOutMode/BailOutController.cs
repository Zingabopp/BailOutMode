using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

namespace BailOutMode
{
    class BailOutController : MonoBehaviour
    {
        #region "Fields with get/setters"
        private static BailOutController _instance;
        private LevelFailedTextEffect _levelFailedEffect;
        private StandardLevelGameplayManager _gameManager;
        private GameEnergyCounter _energyCounter;
        private TextMeshProUGUI _failText;
        private PlayerSpecificSettings _playerSettings;
        private StandardLevelSceneSetup _standardLevelSceneSetup;
        #endregion
        #region "Fields"
        public bool isHiding = false;
        public static int numFails = 0;
        public static float failTextFontSize = 20f;

        public TextMeshProUGUI FailText
        {
            get
            {
                if (_failText == null)
                    _failText = CreateFailText("");
                return _failText;
            }
            set { _failText = value; }
        }

        private LevelFailedTextEffect LevelFailedEffect
        {
            get
            {
                if (_levelFailedEffect == null)
                    _levelFailedEffect = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
                return _levelFailedEffect;
            }
            set { _levelFailedEffect = value; }
        }
        private StandardLevelGameplayManager GameManager
        {
            get
            {
                if (_gameManager == null)
                    _gameManager = GameObject.FindObjectsOfType<StandardLevelGameplayManager>().FirstOrDefault();
                return _gameManager;
            }
        }

        private StandardLevelSceneSetup standardLevelSceneSetup
        {
            get
            {
                if (_standardLevelSceneSetup == null)
                    _standardLevelSceneSetup = GameObject.FindObjectsOfType<StandardLevelSceneSetup>().FirstOrDefault();
                return _standardLevelSceneSetup;
            }
        }
        private GameEnergyCounter EnergyCounter
        {
            get
            {
                if (_energyCounter == null)
                    _energyCounter = GameObject.FindObjectsOfType<GameEnergyCounter>().FirstOrDefault();
                return _energyCounter;
            }
        }
        private PlayerSpecificSettings PlayerSettings
        {
            get
            {
                if (_playerSettings == null)
                {
                    _playerSettings = standardLevelSceneSetup?.standardLevelSceneSetupData?.gameplayCoreSetupData?.playerSpecificSettings;
                    if (_playerSettings != null)
                    {
                        Logger.Debug("Found PlayerSettings");
                    }
                    else
                        Logger.Warning($"Unable to find PlayerSettings");
                }
                else
                    Logger.Trace("PlayerSettings already exists, don't need to find it");
                return _playerSettings;
            }
        }
        private float PlayerHeight;
        public static BailOutController Instance
        {
            get
            {
                if (_instance == null)
                {
                    Logger.Debug("BailOutController instance is null, creating new one");
                    _instance = new GameObject("BailOutController").AddComponent<BailOutController>();
                }
                return _instance;
            }
        }
        public static bool InstanceExists
        {
            get { return _instance != null; }
        }

        #endregion

        public void Awake()
        {
            Logger.Trace("BailOutController Awake()");
            _instance = this;
            isHiding = false;
        }

        public void Start()
        {
            Logger.Trace("BailOutController Start()");
            LevelFailedEffect = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
            if ((GameManager != null) && (EnergyCounter != null) && Plugin.IsEnabled)
            {
                Logger.Trace("Removing HandleGameEnergyDidReach0");
                EnergyCounter.gameEnergyDidReach0Event -= GameManager.HandleGameEnergyDidReach0;
            }
            if (PlayerSettings != null)
                PlayerHeight = PlayerSettings.playerHeight;
            else
                PlayerHeight = 1.8f;
        }

        private void OnDestroy()
        {
            Logger.Debug("Destroying BailOutController");
        }


        public void ShowLevelFailed()
        {
            Logger.Trace("BailOutController ShowLevelFailed()");
            BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
            FailText.text = $"Bailed Out {Plugin._numFails} time{(Plugin._numFails != 1 ? "s" : "")}";
            if (!isHiding && Plugin.ShowFailEffect)
            {
                try
                {
                    LevelFailedEffect.gameObject.SetActive(true);
                    LevelFailedEffect.ShowEffect();
                    if (Plugin.FailEffectDuration > 0)
                        StartCoroutine(hideLevelFailed());
                    else
                        isHiding = true; // Fail text never hides, so don't try to keep showing it
                } catch (Exception ex)
                {
                    Logger.Exception("Exception trying to show the fail Effect", ex);
                }
            }
        }

        public IEnumerator<WaitForSeconds> hideLevelFailed()
        {
            Logger.Trace("BailOutController hideLevelFailed() CoRoutine");
            if (!isHiding)
            {
                Logger.Trace($"BailOutController, will hide LevelFailedEffect after {Plugin.FailEffectDuration}s");
                isHiding = true;
                yield return new WaitForSeconds(Plugin.FailEffectDuration);
                Logger.Trace($"BailOutController, hiding LevelFailedEffect");
                LevelFailedEffect.gameObject.SetActive(false);
                isHiding = false;
            }
            else
                Logger.Trace("BailOutController, skipping hideLevel because isHiding is true");
            yield break;
        }

        public static void FacePosition(Transform obj, Vector3 targetPos)
        {
            var rotAngle = Quaternion.LookRotation(obj.position - targetPos);
            obj.rotation = rotAngle;
        }

        public TextMeshProUGUI CreateFailText(string text, float yOffset = 0)
        {
            var textGO = new GameObject();
            textGO.transform.position = StringToVector3(Plugin.CounterPosition);
            textGO.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            textGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            var textCanvas = textGO.AddComponent<Canvas>();
            textCanvas.renderMode = RenderMode.WorldSpace;
            (textCanvas.transform as RectTransform).sizeDelta = new Vector2(200f, 50f);
            
            FacePosition(textCanvas.transform, new Vector3(0, PlayerHeight, 0));
            TextMeshProUGUI textMeshProUGUI = new GameObject("BailOutFailText").AddComponent<TextMeshProUGUI>();
            
            RectTransform rectTransform = textMeshProUGUI.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(400f, 20f);
            rectTransform.Translate(new Vector3(0, yOffset, 0));
            textMeshProUGUI.text = text;
            textMeshProUGUI.fontSize = failTextFontSize;
            textMeshProUGUI.alignment = TextAlignmentOptions.Center;
            textMeshProUGUI.ForceMeshUpdate();
            textMeshProUGUI.rectTransform.SetParent(textCanvas.transform, false);
            return textMeshProUGUI;
        }

        public static Vector3 StringToVector3(string vStr)
        {
            string[] sAry = vStr.Split(',');
            try
            {
                Vector3 retVal = new Vector3(
                    float.Parse(sAry[0]),
                    float.Parse(sAry[1]),
                    float.Parse(sAry[2]));
                Logger.Debug("StringToVector3: {0}={1}", vStr, retVal.ToString());
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.Exception($"Cannot convert value of {vStr} to a Vector. Needs to be in the format #,#,#", ex);
                return new Vector3(0f, .3f, 2.5f);
            }
        }
    }
}
