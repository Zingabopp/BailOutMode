using System;
using System.Collections;
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
        public static BailOutController instance { get; private set; }
        private LevelFailedTextEffect _levelFailedEffect;
        private StandardLevelGameplayManager _gameManager;
        private GameEnergyCounter _energyCounter;
        private TextMeshProUGUI _failText;
        private static PlayerSpecificSettings _playerSettings;
        #endregion
        #region "Fields"
        public bool isHiding = false;
        public int numFails = 0;

        public bool IsEnabled
        {
            get
            {
                return Config.instance.IsEnabled && (!isCampaign);
            }
        }

        public TextMeshProUGUI FailText
        {
            get
            {
                if (_failText == null)
                    _failText = CreateText("");
                return _failText;
            }
            set { _failText = value; }
        }

        private LevelFailedTextEffect LevelFailedEffect
        {
            get
            {
                if (_levelFailedEffect == null)
                    _levelFailedEffect = Resources.FindObjectsOfTypeAll<LevelFailedTextEffect>().FirstOrDefault();
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

        private GameEnergyCounter EnergyCounter
        {
            get
            {
                if (_energyCounter == null)
                    _energyCounter = GameObject.FindObjectsOfType<GameEnergyCounter>().FirstOrDefault();
                return _energyCounter;
            }
        }
        private static PlayerSpecificSettings PlayerSettings
        {
            get
            {
                if (_playerSettings == null)
                {
                    _playerSettings = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.playerSpecificSettings;
                    if (_playerSettings == null)
                        Logger.log.Warn($"Unable to find PlayerSettings");
                }
                return _playerSettings;
            }
        }
        private static float PlayerHeight
        {
            get
            {
                if (PlayerSettings != null)
                {
                    //Logger.Debug($"Using PlayerHeight {PlayerSettings.playerHeight}");
                    return PlayerSettings.playerHeight;
                }
                else
                {
                    Logger.log.Warn("Unable to find PlayerSettings, using 1.8 for player height");
                    return 1.8f;
                }
            }
        }

        public static bool InstanceExists
        {
            get { return instance != null; }
        }

        #endregion

        public void Awake()
        {
            //Logger.Trace("BailOutController Awake()");
            instance = this;
            isHiding = false;
        }

        public void Start()
        {
            //Logger.Trace("BailOutController Start()");
            StartCoroutine(Initialize());
            LevelFailedEffect = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
            
        }

        private bool isCampaign = false;

        private IEnumerator Initialize()
        {
            yield return new WaitForSeconds(0.5f);
            //Logger.Trace("Checking for Campaign mode");
            if (GameObject.FindObjectsOfType<MissionGameplaySceneSetup>().Count() > 0)
            {
                Logger.log.Info("Campaign mode detected, BailOutMode unavailable.");
                isCampaign = true;
            }
            else
            {
                isCampaign = false;
            }
            if ((GameManager != null) && (EnergyCounter != null) && IsEnabled)
            {
                Logger.log.Info("BailOutMode enabled");
                //Logger.Trace("Removing HandleGameEnergyDidReach0");
                EnergyCounter.gameEnergyDidReach0Event -= GameManager.HandleGameEnergyDidReach0;
            }
        }

        private void OnDestroy()
        {
            Logger.log.Debug("Destroying BailOutController");
            instance = null;
        }

        public void ShowLevelFailed()
        {
            //Logger.Trace("BailOutController ShowLevelFailed()");
            //BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName); Don't need this here
            UpdateFailText($"Bailed Out {numFails} time{(numFails != 1 ? "s" : "")}");
            if (!isHiding && Config.instance.ShowFailEffect)
            {
                try
                {
                    if (!Config.instance.RepeatFailEffect && numFails > 1)
                        return; // Don't want to repeatedly show fail effect, stop here.

                    //Logger.Debug("Showing fail effect");
                    LevelFailedEffect.ShowEffect();
                    if (Config.instance.FailEffectDuration > 0)
                        StartCoroutine(hideLevelFailed());
                    else
                        isHiding = true; // Fail text never hides, so don't try to keep showing it
                }
                catch (Exception ex)
                {
                    Logger.log.Error($"Exception trying to show the fail Effect: {ex.Message}");
                    Logger.log.Debug(ex);
                }
            }
        }
        private WaitForSeconds failDurationWait = new WaitForSeconds(Config.instance.FailEffectDuration);
        public IEnumerator<WaitForSeconds> hideLevelFailed()
        {
            Logger.log.Trace("BailOutController hideLevelFailed() CoRoutine");
            if (!isHiding)
            {
                Logger.log.Trace($"BailOutController, will hide LevelFailedEffect after {Config.instance.FailEffectDuration}s");
                isHiding = true;
                yield return failDurationWait;
                Logger.log.Trace($"BailOutController, hiding LevelFailedEffect");
                LevelFailedEffect.gameObject.SetActive(false);
                isHiding = false;
            }
            else
                Logger.log.Trace("BailOutController, skipping hideLevel because isHiding is true");
            yield break;
        }

        public static void FacePosition(Transform obj, Vector3 targetPos)
        {
            var rotAngle = Quaternion.LookRotation(StringToVector3(Config.instance.CounterTextPosition) - targetPos);
            obj.rotation = rotAngle;
        }

        public TextMeshProUGUI CreateText(string text)
        {
            Canvas _canvas = new GameObject("BailOutFailText").AddComponent<Canvas>();
            _canvas.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _canvas.renderMode = RenderMode.WorldSpace;
            (_canvas.transform as RectTransform).sizeDelta = new Vector2(0f, 0f);
            return CreateText(_canvas, text, new Vector2(0f, 0f), (_canvas.transform as RectTransform).sizeDelta);
        }

        public TextMeshProUGUI CreateText(Canvas parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject gameObj = parent.gameObject;
            gameObj.SetActive(false);
            TextMeshProUGUI textMesh = gameObj.AddComponent<TextMeshProUGUI>();
            /*
            Teko-Medium SDF No Glow
            Teko-Medium SDF
            Teko-Medium SDF No Glow Fading
            */
            var font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF No Glow"));
            if (font == null)
            {
                Logger.log.Error("Could not locate font asset, unable to display text");
                return null;
            }
            textMesh.font = font;
            textMesh.fontSize = Config.instance.CounterTextSize;
            textMesh.rectTransform.SetParent(parent.transform as RectTransform, false);
            textMesh.text = text;
            textMesh.color = Color.white;
            textMesh.rectTransform.anchorMin = new Vector2(0f, 0f);
            textMesh.rectTransform.anchorMax = new Vector2(0f, 0f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;
            textMesh.alignment = TextAlignmentOptions.Left;
            FacePosition(textMesh.gameObject.transform, new Vector3(0, PlayerHeight, 0));
            gameObj.SetActive(true);
            return textMesh;
        }

        public static void CenterTextMesh(TextMeshProUGUI text)
        {
            text.ForceMeshUpdate();
            var pos = StringToVector3(Config.instance.CounterTextPosition);
            pos.x = pos.x - (text.renderedWidth * text.gameObject.transform.localScale.x) / 2;
            pos.y = pos.y + (text.renderedHeight * text.gameObject.transform.localScale.y);
            FacePosition(text.gameObject.transform, new Vector3(0, PlayerHeight, 0));
            text.transform.position = pos;
            
        }

        public void UpdateFailText(string text)
        {
            FailText.text = text;
            if (Config.instance.DynamicSettings)
                FailText.fontSize = Config.instance.CounterTextSize;
            CenterTextMesh(FailText);
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
                //Logger.Debug("StringToVector3: {0}={1}", vStr, retVal.ToString());
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.log.Error($"Cannot convert value of {vStr} to a Vector. Needs to be in the format #,#,#: {ex.Message}");
                Logger.log.Debug(ex);
                return new Vector3(DefaultSettings.CounterTextPosition.x, DefaultSettings.CounterTextPosition.y, DefaultSettings.CounterTextPosition.z);
            }
        }
    }
}
