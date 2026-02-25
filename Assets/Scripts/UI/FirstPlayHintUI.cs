using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// Home 씬 진입 시 한 번만 조작 안내를 표시합니다. PlayerPrefs "HasSeenHint"로 스킵합니다.
    /// </summary>
    public class FirstPlayHintUI : MonoBehaviour
    {
        private const string PrefsKey = "HasSeenHint";

        [Header("Content")]
        [Tooltip("표시할 안내 텍스트. 비어 있으면 기본 문구 사용")]
        [SerializeField] private string hintMessage = "";

        [Tooltip("안내 문구를 표시할 TextMeshPro (비어 있으면 자동 검색)")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Behaviour")]
        [Tooltip("확인 버튼 (클릭 시 패널 닫고 다시 안내 안 함). 없으면 자동 숨김만 사용")]
        [SerializeField] private Button closeButton;

        [Tooltip("자동으로 숨기기까지 대기 시간(초). 0이면 자동 숨김 없음")]
        [SerializeField] private float autoHideSeconds = 6f;

        [Tooltip("표시할 패널 (비어 있으면 이 오브젝트)")]
        [SerializeField] private GameObject hintPanel;

        private static readonly string DefaultMessage = "조작법\nWASD: 이동\nShift: 달리기\nE: 수집 / 문 열기\nF: 상호작용 (미니게임 등)";

        private void Start()
        {
            if (hintPanel == null)
                hintPanel = gameObject;

            if (messageText == null)
                messageText = GetComponentInChildren<TextMeshProUGUI>(true);

            bool alreadySeen = PlayerPrefs.GetInt(PrefsKey, 0) != 0;
            if (alreadySeen || SceneManager.GetActiveScene().name != "Home")
            {
                if (hintPanel != null)
                    hintPanel.SetActive(false);
                return;
            }

            if (messageText != null)
                messageText.text = string.IsNullOrEmpty(hintMessage) ? DefaultMessage : hintMessage;

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(MarkSeenAndHide);
            }

            if (hintPanel != null)
                hintPanel.SetActive(true);

            if (autoHideSeconds > 0f)
                Invoke(nameof(MarkSeenAndHide), autoHideSeconds);
        }

        private void MarkSeenAndHide()
        {
            CancelInvoke(nameof(MarkSeenAndHide));
            PlayerPrefs.SetInt(PrefsKey, 1);
            PlayerPrefs.Save();
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }
    }
}
