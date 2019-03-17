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
        #endregion
        #region "Fields"
        public bool isHiding = false;
        public static int numFails = 0;
        public static float failTextFontSize = 15f;

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
        private PlayerSpecificSettings PlayerSettings
        {
            get
            {
                if (_playerSettings == null)
                {
                    _playerSettings = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.playerSpecificSettings;
                    if (_playerSettings == null)
                        Logger.Warning($"Unable to find PlayerSettings");
                }
                return _playerSettings;
            }
        }
        private float PlayerHeight
        {
            get
            {
                if (PlayerSettings != null)
                    return PlayerSettings.playerHeight;
                else
                {
                    Logger.Warning("Unable to find PlayerSettings, using 1.8 for player height");
                    return 1.8f;
                }
            }
        }
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
            //Logger.Trace("BailOutController Awake()");
            _instance = this;
            isHiding = false;
        }

        public void Start()
        {
            //Logger.Trace("BailOutController Start()");
            LevelFailedEffect = GameObject.FindObjectsOfType<LevelFailedTextEffect>().FirstOrDefault();
            if ((GameManager != null) && (EnergyCounter != null) && Plugin.IsEnabled)
            {
                //Logger.Trace("Removing HandleGameEnergyDidReach0");
                EnergyCounter.gameEnergyDidReach0Event -= GameManager.HandleGameEnergyDidReach0;
            }
        }

        private void OnDestroy()
        {
            Logger.Debug("Destroying BailOutController");
        }

        public void ShowLevelFailed()
        {
            //Logger.Trace("BailOutController ShowLevelFailed()");
            BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.PluginName);
            FailText.text = $"Bailed Out {Plugin._numFails} time{(Plugin._numFails != 1 ? "s" : "")}";
            if (!isHiding && Plugin.ShowFailEffect)
            {
                try
                {
                    if (!Plugin.RepeatFailEffect && Plugin._numFails > 1)
                        return; // Don't want to repeatedly show fail effect, stop here.

                    //Logger.Debug("Showing fail effect");
                    LevelFailedEffect.ShowEffect();
                    if (Plugin.FailEffectDuration > 0)
                        StartCoroutine(hideLevelFailed());
                    else
                        isHiding = true; // Fail text never hides, so don't try to keep showing it
                }
                catch (Exception ex)
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

        public TextMeshProUGUI CreateText(string text)
        {
            Canvas _canvas = new GameObject("BailOutFailText").AddComponent<Canvas>();
            _canvas.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _canvas.renderMode = RenderMode.WorldSpace;
            (_canvas.transform as RectTransform).sizeDelta = new Vector2(200f, 50f);
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
                Logger.Error("Could not locate font asset, unable to display text");
                return null;
            }
            textMesh.font = font;
            textMesh.rectTransform.SetParent(parent.transform as RectTransform, false);
            textMesh.text = text;
            textMesh.color = Color.white;

            textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;
            textMesh.alignment = TextAlignmentOptions.Left;
            gameObj.transform.position = StringToVector3(Plugin.CounterPosition);
            FacePosition(textMesh.gameObject.transform, new Vector3(0, PlayerHeight, 0));
            gameObj.SetActive(true);
            return textMesh;
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
                Logger.Exception($"Cannot convert value of {vStr} to a Vector. Needs to be in the format #,#,#", ex);
                return new Vector3(0f, .3f, 2.5f);
            }
        }
    }
}
