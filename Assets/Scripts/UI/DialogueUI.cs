using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace BirthdayCakeQuest.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Image speakerPortrait; // 캐릭터 초상화 이미지

        // 외부 접근용 프로퍼티
        public GameObject DialoguePanel => dialoguePanel;

        [Header("Choice UI")]
        [Tooltip("선택지 버튼이 생성될 부모 오브젝트 (VerticalLayoutGroup 권장)")]
        [SerializeField] private Transform choiceButtonParent;
        [Tooltip("선택지 버튼 프리팹 (Button + TextMeshProUGUI)")]
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Settings")]
        [SerializeField] private KeyCode continueKey = KeyCode.Space;
        [SerializeField] private KeyCode skipKey = KeyCode.Return;

        private DialogueSystem _dialogueSystem;
        private bool _waitingForInput = false;
        private bool _showingChoices = false;
        private List<GameObject> _currentChoiceButtons = new List<GameObject>();

        private void Awake()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // TitleScene에서는 DialogueSystem이 필요 없으므로 경고를 출력하지 않음
            // 자신의 씬이 TitleScene인지 확인
            if (gameObject.scene.name == "TitleScene" || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene")
            {
                return;
            }
            
            // Awake에서 이벤트 구독 (Start보다 먼저 실행됨)
            _dialogueSystem = DialogueSystem.Instance;
            if (_dialogueSystem != null)
            {
                _dialogueSystem.OnDialogueStart += ShowDialogue;
                _dialogueSystem.OnDialogueEnd += HideDialogue;
            }
        }

        private void Start()
        {
            // 자신의 씬이 활성 씬이 아니면 dialoguePanel 비활성화
            Scene activeScene = SceneManager.GetActiveScene();
            if (gameObject.scene != activeScene)
            {
                if (dialoguePanel != null && dialoguePanel.activeSelf)
                {
                    dialoguePanel.SetActive(false);
                }
            }

            // Awake에서 연결되지 않았으면 Start에서 재시도
            if (_dialogueSystem == null)
            {
                _dialogueSystem = DialogueSystem.Instance;
                if (_dialogueSystem != null)
                {
                    _dialogueSystem.OnDialogueStart += ShowDialogue;
                    _dialogueSystem.OnDialogueEnd += HideDialogue;
                }
            }
        }

        private void OnEnable()
        {
            // 씬이 활성화될 때 자신의 씬이 활성 씬인지 확인
            Scene activeScene = SceneManager.GetActiveScene();
            if (gameObject.scene != activeScene)
            {
                if (dialoguePanel != null && dialoguePanel.activeSelf)
                {
                    dialoguePanel.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (_dialogueSystem != null)
            {
                _dialogueSystem.OnDialogueStart -= ShowDialogue;
                _dialogueSystem.OnDialogueEnd -= HideDialogue;
            }
        }

        private void Update()
        {
            // 선택지가 표시 중이면 키 입력 무시
            if (_showingChoices)
                return;

            if (_waitingForInput)
            {
                if (Input.GetKeyDown(continueKey) || Input.GetKeyDown(skipKey))
                {
                    _waitingForInput = false;
                    _dialogueSystem.PlayNextDialogue();
                }
            }
        }

        private void ShowDialogue(DialogueData dialogue)
        {
            // 현재 활성 씬의 DialogueUI인지 확인
            Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (gameObject.scene != activeScene)
            {
                // 다른 씬의 DialogueUI면 무시
                return;
            }

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // 화자 이름 표시
            if (speakerNameText != null)
            {
                speakerNameText.text = string.IsNullOrEmpty(dialogue.speakerName) ? "" : dialogue.speakerName;
                speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(dialogue.speakerName));
            }

            // 대화 텍스트 표시
            if (dialogueText != null)
                dialogueText.text = dialogue.text;

            // 캐릭터 초상화 표시
            if (speakerPortrait != null)
            {
                if (dialogue.speakerPortrait != null)
                {
                    speakerPortrait.sprite = dialogue.speakerPortrait;
                    speakerPortrait.gameObject.SetActive(true);
                }
                else
                {
                    // 초상화가 없으면 숨김
                    speakerPortrait.gameObject.SetActive(false);
                }
            }

            // 선택지가 있으면 버튼 표시, 없으면 계속 표시
            if (dialogue.hasChoices && dialogue.choices.Count > 0)
            {
                if (continueIndicator != null)
                    continueIndicator.SetActive(false);
                
                ShowChoices(dialogue.choices);
            }
            else
            {
                if (continueIndicator != null)
                    continueIndicator.SetActive(true);
                
                _waitingForInput = true;
            }
        }

        private void ShowChoices(List<DialogueChoice> choices)
        {
            _showingChoices = true;
            _waitingForInput = false;

            // 기존 선택지 버튼 제거
            ClearChoices();

            if (choiceButtonParent == null)
            {
                _showingChoices = false;
                return;
            }

            // VerticalLayoutGroup 확인 및 Spacing 설정
            VerticalLayoutGroup layoutGroup = choiceButtonParent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = 100f; // 버튼 간격 100으로 설정
            }

            // 선택지 버튼 생성
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                GameObject buttonObj;
                
                if (choiceButtonPrefab != null)
                {
                    buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                }
                else
                {
                    // 프리팹이 없으면 자동 생성
                    buttonObj = new GameObject("ChoiceButton");
                    buttonObj.transform.SetParent(choiceButtonParent, false);
                    
                    RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
                    buttonRect.sizeDelta = new Vector2(400, 60);
                    
                    Image buttonImage = buttonObj.AddComponent<Image>();
                    buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                    
                    Button button = buttonObj.AddComponent<Button>();
                    
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(buttonObj.transform, false);
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(10, 5);
                    textRect.offsetMax = new Vector2(-10, -5);
                    
                    TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                    buttonText.text = choice.choiceText;
                    buttonText.fontSize = 24;
                    buttonText.alignment = TextAlignmentOptions.Center;
                    buttonText.color = Color.white;
                    buttonText.raycastTarget = false;
                }

                // VerticalLayoutGroup이 없으면 수동으로 위치 설정
                if (layoutGroup == null)
                {
                    RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        buttonRect.anchoredPosition = new Vector2(0, -i * 100f); // 100 간격으로 배치
                    }
                }

                // 버튼 텍스트 설정
                TextMeshProUGUI buttonTextComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonTextComponent != null)
                {
                    buttonTextComponent.text = choice.choiceText;
                }

                // 버튼 클릭 이벤트
                Button buttonComponent = buttonObj.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.RemoveAllListeners(); // 기존 리스너 제거
                    buttonComponent.onClick.AddListener(() => OnChoiceSelected(choice));
                }

                // 버튼 활성화
                if (!buttonObj.activeSelf)
                {
                    buttonObj.SetActive(true);
                }

                _currentChoiceButtons.Add(buttonObj);
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            // 선택지 버튼 숨기기
            ClearChoices();
            _showingChoices = false;

            // 정답 여부를 DialogueSystem에 알림 (이벤트로 처리)
            if (choice.isCorrect)
            {
                // 정답 선택 시 이벤트 발생 (SugarMiniGameScene에서 구독)
                OnCorrectChoiceSelected?.Invoke();
            }

            // 다음 대화 표시
            if (choice.nextDialogues != null && choice.nextDialogues.Count > 0)
            {
                _dialogueSystem.StartDialogueSequence(choice.nextDialogues);
            }
            else
            {
                // 다음 대화가 없으면 대화 종료
                _dialogueSystem.PlayNextDialogue();
            }
        }

        private void ClearChoices()
        {
            foreach (var button in _currentChoiceButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            _currentChoiceButtons.Clear();
        }

        // 정답 선택 이벤트 (SugarMiniGameScene에서 구독)
        public event System.Action OnCorrectChoiceSelected;

        private void HideDialogue()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // 초상화도 숨김
            if (speakerPortrait != null)
                speakerPortrait.gameObject.SetActive(false);

            // 선택지 버튼 제거
            ClearChoices();

            _waitingForInput = false;
            _showingChoices = false;
        }

        public static GameObject CreateDialogueUI(Canvas parentCanvas)
        {
            // 대화 패널 생성 (화면 하단)
            GameObject panelObj = new GameObject("DialoguePanel");
            panelObj.transform.SetParent(parentCanvas.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.25f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);

            // 화자 이름 텍스트
            GameObject speakerObj = new GameObject("SpeakerName");
            speakerObj.transform.SetParent(panelObj.transform, false);
            RectTransform speakerRect = speakerObj.AddComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0.05f, 0.7f);
            speakerRect.anchorMax = new Vector2(0.3f, 0.95f);
            speakerRect.offsetMin = Vector2.zero;
            speakerRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerText.fontSize = 32;
            speakerText.alignment = TextAlignmentOptions.BottomLeft;
            speakerText.color = Color.yellow;

            // 대화 텍스트
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(panelObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.2f);
            textRect.anchorMax = new Vector2(0.95f, 0.7f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI dialogueTextTMP = textObj.AddComponent<TextMeshProUGUI>();
            dialogueTextTMP.fontSize = 28;
            dialogueTextTMP.alignment = TextAlignmentOptions.TopLeft;
            dialogueTextTMP.color = Color.white;

            // 계속 표시 아이콘
            GameObject indicatorObj = new GameObject("ContinueIndicator");
            indicatorObj.transform.SetParent(panelObj.transform, false);
            RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.9f, 0.05f);
            indicatorRect.anchorMax = new Vector2(0.95f, 0.15f);
            indicatorRect.offsetMin = Vector2.zero;
            indicatorRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI indicatorText = indicatorObj.AddComponent<TextMeshProUGUI>();
            indicatorText.text = "▼";
            indicatorText.fontSize = 24;
            indicatorText.alignment = TextAlignmentOptions.Center;
            indicatorText.color = Color.white;

            // DialogueUI 컴포넌트 추가
            DialogueUI dialogueUI = panelObj.AddComponent<DialogueUI>();
            dialogueUI.dialoguePanel = panelObj;
            dialogueUI.speakerNameText = speakerText;
            dialogueUI.dialogueText = dialogueTextTMP;
            dialogueUI.continueIndicator = indicatorObj;

            panelObj.SetActive(false);
            return panelObj;
        }
    }
}

