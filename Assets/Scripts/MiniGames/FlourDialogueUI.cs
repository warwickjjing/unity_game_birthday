using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 미니게임 대화 데이터입니다.
    /// </summary>
    [System.Serializable]
    public class FlourDialogueData
    {
        [TextArea(2, 5)]
        public string text;
        public string speakerName = "";
        public Sprite speakerPortrait;
    }

    /// <summary>
    /// 밀가루 미니게임의 대화 UI입니다.
    /// 여러 대화를 순차적으로 표시하고, 화자 정보를 표시합니다.
    /// </summary>
    public class FlourDialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private GameObject continueIndicator;

        [Header("Settings")]
        [SerializeField] private KeyCode continueKey = KeyCode.Space;
        [SerializeField] private bool useTypingEffect = false;
        [SerializeField] private float typingSpeed = 0.05f;

        private Queue<FlourDialogueData> _dialogueQueue = new Queue<FlourDialogueData>();
        private Coroutine _typingCoroutine;
        private bool _isTyping = false;
        private bool _waitingForInput = false;

        // 대화 종료 이벤트
        public event System.Action OnDialogueComplete;

        private void Awake()
        {
            // 자동으로 UI 생성 (없으면)
            if (dialoguePanel == null)
            {
                CreateDialogueUI();
            }

            // 초기에는 숨김
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (_waitingForInput && Input.GetKeyDown(continueKey))
            {
                if (_isTyping)
                {
                    // 타이핑 중이면 스킵
                    if (_typingCoroutine != null)
                    {
                        StopCoroutine(_typingCoroutine);
                        _typingCoroutine = null;
                    }
                    _isTyping = false;
                    if (continueIndicator != null)
                    {
                        continueIndicator.SetActive(true);
                    }
                }
                else
                {
                    // 다음 대화로
                    _waitingForInput = false;
                    ShowNextDialogue();
                }
            }
        }

        /// <summary>
        /// 대화 UI를 자동으로 생성합니다.
        /// </summary>
        private void CreateDialogueUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[FlourDialogueUI] Canvas를 찾을 수 없습니다!");
                return;
            }

            // Dialogue Panel 생성 (하단)
            GameObject panelObj = new GameObject("FlourDialoguePanel");
            panelObj.transform.SetParent(canvas.transform, false);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.25f);
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            dialoguePanel = panelObj;

            // Speaker Portrait (우측 상단)
            GameObject portraitObj = new GameObject("SpeakerPortrait");
            portraitObj.transform.SetParent(panelObj.transform, false);

            speakerPortrait = portraitObj.AddComponent<Image>();
            speakerPortrait.color = Color.white;
            speakerPortrait.preserveAspect = true;

            RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.85f, 0.6f);
            portraitRect.anchorMax = new Vector2(0.98f, 0.98f);
            portraitRect.sizeDelta = Vector2.zero;
            portraitObj.SetActive(false);

            // Speaker Name (좌측 상단)
            GameObject nameObj = new GameObject("SpeakerName");
            nameObj.transform.SetParent(panelObj.transform, false);

            speakerNameText = nameObj.AddComponent<TextMeshProUGUI>();
            speakerNameText.text = "";
            speakerNameText.fontSize = 28;
            speakerNameText.color = Color.yellow;
            speakerNameText.alignment = TextAlignmentOptions.BottomLeft;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.7f);
            nameRect.anchorMax = new Vector2(0.5f, 0.95f);
            nameRect.sizeDelta = Vector2.zero;

            // Dialogue Text (중앙)
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(panelObj.transform, false);

            dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 24;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.enableWordWrapping = true;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.15f);
            textRect.anchorMax = new Vector2(0.95f, 0.65f);
            textRect.sizeDelta = Vector2.zero;

            // Continue Indicator (우측 하단)
            GameObject indicatorObj = new GameObject("ContinueIndicator");
            indicatorObj.transform.SetParent(panelObj.transform, false);

            TextMeshProUGUI indicatorText = indicatorObj.AddComponent<TextMeshProUGUI>();
            indicatorText.text = "▼ Space";
            indicatorText.fontSize = 20;
            indicatorText.color = Color.white;
            indicatorText.alignment = TextAlignmentOptions.Center;

            RectTransform indicatorRect = indicatorObj.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.85f, 0.05f);
            indicatorRect.anchorMax = new Vector2(0.98f, 0.15f);
            indicatorRect.sizeDelta = Vector2.zero;

            continueIndicator = indicatorObj;

            Debug.Log("[FlourDialogueUI] 대화 UI 자동 생성 완료");
        }

        /// <summary>
        /// 단일 대화를 표시합니다 (하위 호환성).
        /// </summary>
        public void Show(string message)
        {
            FlourDialogueData data = new FlourDialogueData
            {
                text = message,
                speakerName = "",
                speakerPortrait = null
            };
            ShowDialogue(new List<FlourDialogueData> { data });
        }

        /// <summary>
        /// 여러 대화를 순차적으로 표시합니다.
        /// </summary>
        public void ShowDialogue(List<FlourDialogueData> dialogues)
        {
            if (dialoguePanel == null || dialogueText == null)
            {
                Debug.LogWarning("[FlourDialogueUI] UI 요소가 설정되지 않았습니다!");
                return;
            }

            if (dialogues == null || dialogues.Count == 0)
            {
                Debug.LogWarning("[FlourDialogueUI] 표시할 대화가 없습니다!");
                return;
            }

            // 기존 대화 초기화
            _dialogueQueue.Clear();
            foreach (var dialogue in dialogues)
            {
                _dialogueQueue.Enqueue(dialogue);
            }

            // 패널 표시 및 첫 대화 시작
            dialoguePanel.SetActive(true);
            ShowNextDialogue();

            Debug.Log($"[FlourDialogueUI] {dialogues.Count}개의 대화 시작");
        }

        /// <summary>
        /// 다음 대화를 표시합니다.
        /// </summary>
        private void ShowNextDialogue()
        {
            if (_dialogueQueue.Count == 0)
            {
                // 모든 대화 종료
                Hide();
                return;
            }

            FlourDialogueData currentDialogue = _dialogueQueue.Dequeue();

            // 화자 이름 설정
            if (speakerNameText != null)
            {
                speakerNameText.text = string.IsNullOrEmpty(currentDialogue.speakerName) 
                    ? "" 
                    : currentDialogue.speakerName;
            }

            // 화자 초상화 설정
            if (speakerPortrait != null)
            {
                if (currentDialogue.speakerPortrait != null)
                {
                    speakerPortrait.sprite = currentDialogue.speakerPortrait;
                    speakerPortrait.gameObject.SetActive(true);
                }
                else
                {
                    speakerPortrait.gameObject.SetActive(false);
                }
            }

            // 대화 텍스트 표시
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }

            if (useTypingEffect)
            {
                _typingCoroutine = StartCoroutine(TypeText(currentDialogue.text));
            }
            else
            {
                dialogueText.text = currentDialogue.text;
                _isTyping = false;
                if (continueIndicator != null)
                {
                    continueIndicator.SetActive(true);
                }
            }

            _waitingForInput = true;
        }

        /// <summary>
        /// 대화를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _dialogueQueue.Clear();
            _isTyping = false;
            _waitingForInput = false;

            Debug.Log("[FlourDialogueUI] 대화 종료");
            
            // 대화 종료 이벤트 발생
            OnDialogueComplete?.Invoke();
        }

        /// <summary>
        /// 타이핑 효과로 텍스트를 표시합니다.
        /// </summary>
        private IEnumerator TypeText(string message)
        {
            _isTyping = true;
            dialogueText.text = "";
            
            foreach (char c in message)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }
        }
    }
}

