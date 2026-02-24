using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BirthdayCakeQuest.Utils
{
    /// <summary>
    /// Scene 전환 시 부드러운 페이드 효과를 제공하는 싱글톤입니다.
    /// DontDestroyOnLoad로 Scene 전환 시에도 유지됩니다.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        private Canvas _fadeCanvas;
        private Image _fadeImage;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupFadeCanvas();
        }

        private void SetupFadeCanvas()
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            _fadeCanvas = canvasObj.AddComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.sortingOrder = 9999; // 최상위 레이어

            // CanvasScaler 추가 (해상도 대응)
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // GraphicRaycaster 추가
            canvasObj.AddComponent<GraphicRaycaster>();

            // Fade Image 생성
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            _fadeImage = imageObj.AddComponent<Image>();
            _fadeImage.color = fadeColor;

            // RectTransform 설정 (전체 화면)
            RectTransform rect = _fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // CanvasGroup 추가 (알파 제어용)
            _canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            // 초기에는 보이지 않게
            _fadeImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 화면을 페이드 아웃합니다 (어두워짐).
        /// </summary>
        public IEnumerator FadeOut()
        {
            if (_fadeImage == null || _canvasGroup == null)
            {
                Debug.LogWarning("[SceneTransitionManager] Fade Canvas가 설정되지 않았습니다!");
                yield break;
            }

            _fadeImage.gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                _canvasGroup.alpha = alpha;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            Debug.Log("[SceneTransitionManager] Fade Out 완료");
        }

        /// <summary>
        /// 화면을 페이드 인합니다 (밝아짐).
        /// </summary>
        public IEnumerator FadeIn()
        {
            if (_fadeImage == null || _canvasGroup == null)
            {
                Debug.LogWarning("[SceneTransitionManager] Fade Canvas가 설정되지 않았습니다!");
                yield break;
            }

            _fadeImage.gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                _canvasGroup.alpha = alpha;
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _fadeImage.gameObject.SetActive(false);
            Debug.Log("[SceneTransitionManager] Fade In 완료");
        }

        /// <summary>
        /// 즉시 페이드를 완료합니다 (알파를 0으로).
        /// </summary>
        public void ClearFade()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_fadeImage != null)
            {
                _fadeImage.gameObject.SetActive(false);
            }
        }
    }
}

