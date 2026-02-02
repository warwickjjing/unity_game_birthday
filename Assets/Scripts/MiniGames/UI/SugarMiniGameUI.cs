using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames.UI
{
    /// <summary>
    /// 설탕 미니게임 UI 헬퍼 클래스입니다.
    /// UI 요소들을 자동으로 찾고 설정하는 기능을 제공합니다.
    /// </summary>
    public class SugarMiniGameUI : MonoBehaviour
    {
        [Header("Auto-Find References")]
        [SerializeField] private bool autoFindReferences = true;

        [Header("UI Elements")]
        public Image sugarFillImage;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI instructionsText;
        public TextMeshProUGUI titleText;
        public GameObject resultPanel;
        public TextMeshProUGUI resultText;
        public Button retryButton;
        public Button continueButton;
        public Image targetZoneImage;
        public Image containerImage;

        private void Awake()
        {
            if (autoFindReferences)
            {
                FindUIReferences();
            }
        }

        /// <summary>
        /// UI 요소들을 자동으로 찾습니다.
        /// </summary>
        public void FindUIReferences()
        {
            // Transform.Find를 사용하여 자식 오브젝트 찾기
            if (titleText == null)
            {
                Transform titleTransform = transform.Find("Title");
                if (titleTransform != null)
                    titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }

            if (instructionsText == null)
            {
                Transform instructionsTransform = transform.Find("Instructions");
                if (instructionsTransform != null)
                    instructionsText = instructionsTransform.GetComponent<TextMeshProUGUI>();
            }

            if (timerText == null)
            {
                Transform timerTransform = transform.Find("Timer");
                if (timerTransform != null)
                    timerText = timerTransform.GetComponent<TextMeshProUGUI>();
            }

            if (containerImage == null)
            {
                Transform containerTransform = transform.Find("Container");
                if (containerTransform != null)
                {
                    containerImage = containerTransform.GetComponent<Image>();

                    // Container 안의 SugarFill 찾기
                    if (sugarFillImage == null)
                    {
                        Transform fillTransform = containerTransform.Find("SugarFill");
                        if (fillTransform != null)
                            sugarFillImage = fillTransform.GetComponent<Image>();
                    }
                }
            }

            if (targetZoneImage == null)
            {
                Transform targetZoneTransform = transform.Find("TargetZone");
                if (targetZoneTransform != null)
                    targetZoneImage = targetZoneTransform.GetComponent<Image>();
            }

            if (resultPanel == null)
            {
                Transform resultPanelTransform = transform.Find("ResultPanel");
                if (resultPanelTransform != null)
                {
                    resultPanel = resultPanelTransform.gameObject;

                    // ResultPanel 안의 요소들 찾기
                    if (resultText == null)
                    {
                        Transform resultTextTransform = resultPanelTransform.Find("ResultText");
                        if (resultTextTransform != null)
                            resultText = resultTextTransform.GetComponent<TextMeshProUGUI>();
                    }

                    if (retryButton == null)
                    {
                        Transform retryButtonTransform = resultPanelTransform.Find("RetryButton");
                        if (retryButtonTransform != null)
                            retryButton = retryButtonTransform.GetComponent<Button>();
                    }

                    if (continueButton == null)
                    {
                        Transform continueButtonTransform = resultPanelTransform.Find("ContinueButton");
                        if (continueButtonTransform != null)
                            continueButton = continueButtonTransform.GetComponent<Button>();
                    }
                }
            }

            Debug.Log("[SugarMiniGameUI] UI 참조 자동 탐색 완료");
        }

        /// <summary>
        /// SugarPouringMiniGame 컴포넌트에 UI 참조를 연결합니다.
        /// </summary>
        public void ConnectToMiniGame()
        {
            SugarPouringMiniGame miniGame = GetComponent<SugarPouringMiniGame>();
            if (miniGame == null)
            {
                Debug.LogError("[SugarMiniGameUI] SugarPouringMiniGame 컴포넌트를 찾을 수 없습니다!");
                return;
            }

            // Reflection을 사용하여 private 필드에 접근
            var type = miniGame.GetType();
            
            SetPrivateField(miniGame, "sugarFillImage", sugarFillImage);
            SetPrivateField(miniGame, "timerText", timerText);
            SetPrivateField(miniGame, "instructionsText", instructionsText);
            SetPrivateField(miniGame, "resultPanel", resultPanel);
            SetPrivateField(miniGame, "resultText", resultText);
            SetPrivateField(miniGame, "retryButton", retryButton);
            SetPrivateField(miniGame, "continueButton", continueButton);
            SetPrivateField(miniGame, "targetZoneImage", targetZoneImage);

            Debug.Log("[SugarMiniGameUI] UI 참조가 MiniGame에 연결되었습니다.");
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// UI 초기 설정을 수행합니다.
        /// </summary>
        public void SetupUI()
        {
            // 제목 설정
            if (titleText != null)
            {
                titleText.text = "설탕 계량하기";
            }

            // 초기 안내 메시지
            if (instructionsText != null)
            {
                instructionsText.text = "마우스를 눌러 설탕을 따르세요!";
            }

            // Fill 이미지 초기화
            if (sugarFillImage != null)
            {
                sugarFillImage.fillAmount = 0f;
                sugarFillImage.type = Image.Type.Filled;
                sugarFillImage.fillMethod = Image.FillMethod.Vertical;
                sugarFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            }

            // 목표 영역 설정
            if (targetZoneImage != null)
            {
                targetZoneImage.color = new Color(0f, 1f, 0f, 0.3f); // 반투명 초록색
            }

            // 결과 패널 비활성화
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            Debug.Log("[SugarMiniGameUI] UI 초기 설정 완료");
        }
    }
}

