using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Radon.UI
{
    internal class TimeScaleDisplay : MonoBehaviour
    {
        internal static TimeScaleDisplay Instance { get; private set; }
        private const float FadeSeconds = 0.5f;
        private const float FontSize = 32f;
        private const string FontPath = "Fonts/Noto/NotoSans-SemiBold SDF";
        private static readonly Vector2 _margin = new Vector2(-20f, -20f);

        private Canvas _canvas;
        private CanvasGroup _group;
        private TextMeshProUGUI _text;
        private Coroutine _fade;
        internal static void Create()
        {
            if (Instance != null)
                return;

            GameObject root = new GameObject("Radon Time Scale");
            DontDestroyOnLoad(root);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            CanvasGroup group = root.AddComponent<CanvasGroup>();

            GameObject background = new GameObject("Background");
            background.transform.SetParent(root.transform, false);

            Image image = background.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.4f);

            HorizontalLayoutGroup layout = background.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 4, 4);

            ContentSizeFitter fitter = background.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = _margin;

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(background.transform, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(FontPath);
            if (font != null)
                text.font = font;
            text.fontSize = FontSize;
            text.enableWordWrapping = false;

            Instance = root.AddComponent<TimeScaleDisplay>();
            Instance._canvas = canvas;
            Instance._group = group;
            Instance._text = text;

            root.SetActive(false);
        }
        internal void Show(int percent)
        {
            _text.text = $"Speed: {percent}%";

            StopFade();
            _group.alpha = 1f;
            gameObject.SetActive(true);

            if (percent == 100)
                _fade = StartCoroutine(FadeOut());
        }
        internal void Hide()
        {
            StopFade();
            gameObject.SetActive(false);
        }
        private void LateUpdate()
        {
            _canvas.enabled = Radon.IsPlaying();
        }
        private IEnumerator FadeOut()
        {
            yield return new WaitForSecondsRealtime(2f);

            float elapsed = 0f;
            while (elapsed < FadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _group.alpha = 1f - elapsed / FadeSeconds;
                yield return null;
            }

            _fade = null;
            gameObject.SetActive(false);
        }
        private void StopFade()
        {
            if (_fade == null)
                return;

            StopCoroutine(_fade);
            _fade = null;
        }
    }
}
