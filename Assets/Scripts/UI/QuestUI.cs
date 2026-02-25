using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 퀘스트 스타일의 재료 체크리스트 UI입니다.
    /// 각 재료를 개별 UI 요소로 표시하고, 수집 시 시각적 피드백을 제공합니다.
    /// 모든 재료 수집 후 다음 퀘스트를 표시합니다.
    /// </summary>
    public sealed class QuestUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("퀘스트 패널 (전체 컨테이너)")]
        [SerializeField] public GameObject questPanel;

        [Tooltip("퀘스트 제목 텍스트")]
        [SerializeField] private TextMeshProUGUI questTitleText;

        [Tooltip("재료 목록 컨테이너 (Vertical Layout Group)")]
        [SerializeField] private Transform ingredientListContainer;

        [Tooltip("다음 퀘스트 표시 영역")]
        [SerializeField] private GameObject nextQuestArea;

        [Tooltip("다음 퀘스트 텍스트")]
        [SerializeField] private TextMeshProUGUI nextQuestText;

        [Header("Quest Item Prefab")]
        [Tooltip("재료 퀘스트 아이템 프리팹 (없으면 런타임에 생성)")]
        [SerializeField] private GameObject questItemPrefab;

        [Header("Display Settings")]
        [Tooltip("재료 이름 한글 매핑")]
        [SerializeField] private IngredientDisplayName[] ingredientNames = new IngredientDisplayName[]
        {
            new IngredientDisplayName { id = IngredientId.Flour, displayName = "밀가루" },
            new IngredientDisplayName { id = IngredientId.Sugar, displayName = "설탕" },
            new IngredientDisplayName { id = IngredientId.Egg, displayName = "계란" },
            new IngredientDisplayName { id = IngredientId.Butter, displayName = "버터" },
            new IngredientDisplayName { id = IngredientId.Strawberry, displayName = "딸기" }
        };

        [Tooltip("퀘스트 제목 (재료 수집 중)")]
        [SerializeField] private string ingredientQuestTitle = "케이크 재료 모으기";

        [Tooltip("다음 퀘스트 텍스트 (모든 재료 수집 후)")]
        [SerializeField] private string nextQuestMessage = "케이크를 들고 소파로 가기";

        [Header("Visual Settings - Colors Only")]
        [Tooltip("수집 완료된 항목 색상")]
        [SerializeField] private Color completedColor = new Color(0.2f, 0.8f, 0.2f);

        [Tooltip("수집 전 항목 색상")]
        [SerializeField] private Color incompleteColor = Color.white;

        [Tooltip("체크 마크 색상")]
        [SerializeField] private Color checkMarkColor = new Color(0.2f, 0.8f, 0.2f);

        [Header("Check Mark Settings")]
        [Tooltip("미완료 체크 마크 문자 (폰트에 없는 경우 [ ] 또는 다른 문자 사용)")]
        [SerializeField] private string uncheckMark = "[ ]";

        [Tooltip("완료 체크 마크 문자 (폰트에 없는 경우 [V] 또는 다른 문자 사용)")]
        [SerializeField] private string checkMark = "[V]";

        [Header("Next Quest Settings")]
        [Tooltip("다음 퀘스트 표시 시 재료 목록 숨기기")]
        [SerializeField] private bool hideIngredientListOnComplete = true;

        [Tooltip("다음 퀘스트로 완전히 교체 (재료 목록 숨기고 제목도 변경)")]
        [SerializeField] private bool replaceQuestContentOnComplete = true;

        [Header("Runtime Settings (프리팹 없을 때만 사용)")]
        [Tooltip("프리팹이 없을 때 런타임 생성 시 사용할 기본 크기 (프리팹 사용 시 무시됨)")]
        [SerializeField] private float defaultCheckMarkSize = 24f;

        private Dictionary<IngredientId, string> _nameMap;
        private Dictionary<IngredientId, QuestItemUI> _questItems;
        private IngredientInventory _inventory;
        private BirthdayCakeQuest.Managers.QuestSequenceManager _questManager;
        private DialogueSystem _dialogueSystem;
        private bool _isInitialized = false;

        private void Awake()
        {
            // 이름 매핑 딕셔너리 생성
            _nameMap = new Dictionary<IngredientId, string>();
            foreach (var item in ingredientNames)
            {
                _nameMap[item.id] = item.displayName;
            }

            _questItems = new Dictionary<IngredientId, QuestItemUI>();

            // 다음 퀘스트 영역 초기에는 숨김
            if (nextQuestArea != null)
            {
                nextQuestArea.SetActive(false);
            }

            // QuestPanel 초기에는 숨김 (대화 끝난 후 표시)
            // 단, 이미 초기화된 경우(씬 재활성화)에는 상태 유지
            if (questPanel != null && !_isInitialized)
            {
                questPanel.SetActive(false);
            }
        }

        private void Start()
        {
            _inventory = IngredientInventory.Instance;
            _questManager = BirthdayCakeQuest.Managers.QuestSequenceManager.Instance;
            _dialogueSystem = DialogueSystem.Instance;

            if (_inventory == null)
            {
                Debug.LogError("[QuestUI] IngredientInventory를 찾을 수 없습니다!");
                return;
            }

            if (_questManager == null)
            {
                Debug.LogError("[QuestUI] QuestSequenceManager를 찾을 수 없습니다!");
                return;
            }

            // 이벤트가 이미 구독되어 있는지 확인 (중복 구독 방지)
            if (!_isInitialized)
            {
                // 이벤트 구독
                _inventory.OnIngredientCollected += OnIngredientCollected;
                _inventory.OnAllCollected += OnAllCollected;
                _questManager.OnQuestChanged += OnQuestChanged;

                // 대화 시스템 이벤트 구독
                if (_dialogueSystem != null)
                {
                    _dialogueSystem.OnDialogueStart += OnDialogueStart;
                    _dialogueSystem.OnDialogueEnd += OnDialogueEnd;
                }

                // 딸기 카운트 업데이트 이벤트 구독
                var strawberryManager = BirthdayCakeQuest.Ingredients.StrawberryCollectionManager.Instance;
                if (strawberryManager != null)
                {
                    strawberryManager.OnStrawberryCountChanged += OnStrawberryCountChanged;
                }
                else
                {
                    Debug.LogWarning("[QuestUI] StrawberryCollectionManager를 찾을 수 없습니다!");
                }
                
                _isInitialized = true;
            }

            // 초기 UI는 OnQuestChanged 이벤트로 생성됨 (게임 시작 대화 후)
            // 하지만 빌드에서 이벤트가 이미 발생했을 수 있으므로, 현재 퀘스트가 있으면 즉시 UI 생성
            StartCoroutine(InitializeQuestUIAfterDelay());
            
            // 씬 복귀 시: 대화가 재생 중이 아니고 퀘스트가 이미 활성화되었다면 QuestPanel 표시
            StartCoroutine(CheckAndShowQuestPanelOnSceneReturn());
        }
        
        /// <summary>
        /// 초기화 후 현재 퀘스트가 있으면 UI를 생성합니다 (빌드에서 이벤트를 놓쳤을 경우 대비).
        /// </summary>
        private System.Collections.IEnumerator InitializeQuestUIAfterDelay()
        {
            // 모든 초기화가 완료되도록 대기
            yield return null;
            yield return null;
            
            // 현재 퀘스트가 있고, UI가 아직 생성되지 않았으면 생성
            if (_questManager != null && _questManager.CurrentActiveIngredient != (IngredientId)(-1))
            {
                if (ingredientListContainer != null && ingredientListContainer.childCount == 0)
                {
                    Debug.Log("[QuestUI] 빌드에서 초기 퀘스트 UI 생성 (이벤트를 놓쳤을 수 있음)");
                    CreateQuestItems();
                    UpdateQuestDisplay();
                    
                    // 대화가 재생 중이 아니면 QuestPanel 표시
                    if (_dialogueSystem == null || !_dialogueSystem.IsPlaying)
                    {
                        if (questPanel != null && !questPanel.activeSelf)
                        {
                            questPanel.SetActive(true);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 씬 복귀 시 QuestPanel을 표시합니다 (대화가 재생 중이 아닐 때).
        /// </summary>
        private System.Collections.IEnumerator CheckAndShowQuestPanelOnSceneReturn()
        {
            // 씬이 완전히 로드되도록 대기
            yield return null;
            yield return null;
            yield return null; // 추가 대기
            
            // Home 씬인지 확인
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isHomeScene = currentSceneName == "Home";
            
            if (!isHomeScene)
            {
                Debug.Log($"[QuestUI] 현재 씬은 {currentSceneName}입니다. QuestPanel은 Home 씬에서만 표시됩니다.");
                yield break;
            }
            
            // 대화가 재생 중이 아니고, 퀘스트 매니저가 있고, 첫 번째 퀘스트가 활성화되었다면
            if (_dialogueSystem != null && !_dialogueSystem.IsPlaying && 
                _questManager != null && _questManager.CurrentActiveIngredient != (IngredientId)(-1))
            {
                if (questPanel != null && !questPanel.activeSelf)
                {
                    questPanel.SetActive(true);
                }
            }
            // 대화가 재생 중이 아니고 퀘스트가 아직 활성화되지 않았다면, 잠시 대기 후 다시 확인
            else if (_dialogueSystem != null && !_dialogueSystem.IsPlaying && 
                     _questManager != null && _questManager.CurrentActiveIngredient == (IngredientId)(-1))
            {
                // 퀘스트가 아직 활성화되지 않았으면 잠시 더 대기
                yield return new WaitForSeconds(0.5f);
                
                if (_questManager.CurrentActiveIngredient != (IngredientId)(-1) && questPanel != null && !questPanel.activeSelf)
                {
                    questPanel.SetActive(true);
                }
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnIngredientCollected -= OnIngredientCollected;
                _inventory.OnAllCollected -= OnAllCollected;
            }

            if (_questManager != null)
            {
                _questManager.OnQuestChanged -= OnQuestChanged;
            }

            if (_dialogueSystem != null)
            {
                _dialogueSystem.OnDialogueStart -= OnDialogueStart;
                _dialogueSystem.OnDialogueEnd -= OnDialogueEnd;
            }

            var strawberryManager = BirthdayCakeQuest.Ingredients.StrawberryCollectionManager.Instance;
            if (strawberryManager != null)
            {
                strawberryManager.OnStrawberryCountChanged -= OnStrawberryCountChanged;
            }
        }

        /// <summary>
        /// 현재 활성 퀘스트의 재료 아이템만 생성합니다.
        /// </summary>
        private void CreateQuestItems()
        {
            if (ingredientListContainer == null)
            {
                return;
            }

            // 재료 목록 표시 (타이틀과 별도)
            ingredientListContainer.gameObject.SetActive(true);
            
            // 기존 아이템 제거
            foreach (Transform child in ingredientListContainer)
            {
                Destroy(child.gameObject);
            }
            _questItems.Clear();

            // 현재 활성 재료만 생성
            if (_questManager == null)
                return;

            var currentIngredient = _questManager.CurrentActiveIngredient;
            if (currentIngredient == (IngredientId)(-1))
                return;

            // 현재 퀘스트의 재료 UI 생성 (간단한 텍스트로)
            GameObject itemObj = CreateSimpleQuestText(currentIngredient);
            if (itemObj != null)
            {
                itemObj.transform.SetParent(ingredientListContainer, false);
            }
        }
        
        /// <summary>
        /// 간단한 퀘스트 텍스트를 생성합니다 (체크박스 없이).
        /// </summary>
        private GameObject CreateSimpleQuestText(IngredientId ingredientId)
        {
            GameObject textObj = new GameObject($"QuestText_{ingredientId}");
            var rectTransform = textObj.AddComponent<RectTransform>();
            
            // Layout Element 추가 (VerticalLayoutGroup과 호환)
            var layoutElement = textObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 50;
            layoutElement.preferredHeight = 50;
            layoutElement.flexibleWidth = 1;
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 26;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.margin = new Vector4(10, 5, 10, 5); // 여백
            // textComponent.fontStyle = FontStyles.Bold;
            
            // 재료 이름 설정
            string ingredientName = GetDisplayName(ingredientId);
            
            // 딸기인 경우 진행도 표시
            if (ingredientId == IngredientId.Strawberry)
            {
                var strawberryManager = BirthdayCakeQuest.Ingredients.StrawberryCollectionManager.Instance;
                if (strawberryManager != null)
                {
                    textComponent.text = $"{ingredientName} ({strawberryManager.CollectedCount}/{strawberryManager.TargetCount})";
                    Debug.Log($"[QuestUI] 딸기 퀘스트 UI 생성: {textComponent.text}");
                }
                else
                {
                    Debug.LogWarning("[QuestUI] StrawberryCollectionManager가 null입니다! 기본 텍스트만 표시합니다.");
                    textComponent.text = $"{ingredientName} (0/5)"; // 기본값 표시
                }
            }
            else
            {
                textComponent.text = ingredientName;
            }
            
            return textObj;
        }

        /// <summary>
        /// 재료 퀘스트 아이템을 생성합니다.
        /// </summary>
        private GameObject CreateQuestItem(IngredientId ingredientId)
        {
            GameObject itemObj;

            if (questItemPrefab != null)
            {
                // 프리팹 사용: Inspector 설정 그대로 사용
                itemObj = Instantiate(questItemPrefab);
            }
            else
            {
                // 프리팹이 없으면 런타임에 생성 (기본값만 사용)
                itemObj = new GameObject($"QuestItem_{ingredientId}");
                var rectTransform = itemObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(300, 40); // 기본값

                // Horizontal Layout Group 추가
                var layoutGroup = itemObj.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 10f; // 기본값

                // 체크 마크 아이콘 (TextMeshProUGUI로 생성)
                GameObject checkIcon = new GameObject("CheckIcon");
                checkIcon.transform.SetParent(itemObj.transform, false);
                var checkRect = checkIcon.AddComponent<RectTransform>();
                checkRect.sizeDelta = new Vector2(defaultCheckMarkSize, defaultCheckMarkSize); // Inspector 설정값 사용
                var checkText = checkIcon.AddComponent<TextMeshProUGUI>();
                checkText.text = uncheckMark; // 초기 텍스트만 설정
                // fontSize, color, alignment는 Inspector에서 설정하거나 기본값 사용
                // Overflow 설정: TextMeshProUGUI의 Overflow Mode를 Truncate 또는 Overflow로 설정 권장

                // 재료 이름 텍스트
                GameObject nameText = new GameObject("NameText");
                nameText.transform.SetParent(itemObj.transform, false);
                var nameRect = nameText.AddComponent<RectTransform>();
                nameRect.sizeDelta = new Vector2(200, 40); // 기본값
                var nameTMP = nameText.AddComponent<TextMeshProUGUI>();
                nameTMP.text = GetDisplayName(ingredientId); // 내용만 설정
                // fontSize, color, alignment는 Inspector에서 설정하거나 기본값 사용
            }

            // QuestItemUI 컴포넌트 추가
            var questItem = itemObj.GetComponent<QuestItemUI>();
            if (questItem == null)
            {
                questItem = itemObj.AddComponent<QuestItemUI>();
            }

            // 색상 정보와 체크 마크 문자 전달 (시각적 설정은 Inspector에서)
            questItem.Initialize(ingredientId, GetDisplayName(ingredientId), incompleteColor, completedColor, checkMarkColor, uncheckMark, checkMark);

            return itemObj;
        }

        /// <summary>
        /// 퀘스트 표시를 업데이트합니다.
        /// </summary>
        private void UpdateQuestDisplay()
        {
            if (_inventory == null || _questManager == null)
                return;

            var currentIngredient = _questManager.CurrentActiveIngredient;
            if (currentIngredient == (IngredientId)(-1))
            {
                ShowNextQuest();
                return;
            }

            // 퀘스트 타이틀 표시 (QuestSequenceManager에서 설정한 제목)
            if (questTitleText != null)
            {
                string questTitle = _questManager.GetCurrentQuestTitle();
                if (string.IsNullOrEmpty(questTitle))
                {
                    // 제목이 없으면 기본 제목 사용
                    questTitle = ingredientQuestTitle;
                }
                questTitleText.text = questTitle;
            }
            
            // 재료 내용 업데이트 (IngredientListContainer에 있는 텍스트)
            if (ingredientListContainer != null && ingredientListContainer.childCount > 0)
            {
                var textComponent = ingredientListContainer.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    string ingredientName = GetDisplayName(currentIngredient);
                    
                    // 딸기인 경우 진행도 업데이트
                    if (currentIngredient == IngredientId.Strawberry)
                    {
                        var strawberryManager = BirthdayCakeQuest.Ingredients.StrawberryCollectionManager.Instance;
                        if (strawberryManager != null)
                        {
                            textComponent.text = $"{ingredientName} ({strawberryManager.CollectedCount}/{strawberryManager.TargetCount})";
                        }
                        else
                        {
                            textComponent.text = ingredientName;
                        }
                    }
                    else
                    {
                        textComponent.text = ingredientName;
                    }
                }
            }
        }

        /// <summary>
        /// 다음 퀘스트를 표시합니다.
        /// </summary>
        private void ShowNextQuest()
        {
            if (replaceQuestContentOnComplete)
            {
                // 재료 목록 완전히 숨기기
                if (ingredientListContainer != null)
                {
                    ingredientListContainer.gameObject.SetActive(false);
                }

                // 제목 텍스트를 다음 퀘스트 메시지로 변경
                if (questTitleText != null)
                {
                    questTitleText.text = nextQuestMessage;
                }

                // NextQuestArea는 사용하지 않음 (제목으로 대체)
                if (nextQuestArea != null)
                {
                    nextQuestArea.SetActive(false);
                }
            }
            else
            {
                // 기존 방식: NextQuestArea 표시
                if (nextQuestArea != null)
                {
                    nextQuestArea.SetActive(true);
                }

                // 텍스트 내용만 설정 (스타일은 Inspector에서)
                if (nextQuestText != null)
                {
                    nextQuestText.text = nextQuestMessage;
                }

                // 재료 목록 처리
                if (ingredientListContainer != null)
                {
                    if (hideIngredientListOnComplete)
                    {
                        // 재료 목록 완전히 숨기기
                        ingredientListContainer.gameObject.SetActive(false);
                    }
                    else
                    {
                        // 재료 목록은 약간 투명하게
                        var canvasGroup = ingredientListContainer.GetComponent<CanvasGroup>();
                        if (canvasGroup == null)
                        {
                            canvasGroup = ingredientListContainer.gameObject.AddComponent<CanvasGroup>();
                        }
                        canvasGroup.alpha = 0.6f;
                    }
                }
            }
        }

        /// <summary>
        /// 다음 퀘스트를 숨깁니다.
        /// </summary>
        private void HideNextQuest()
        {
            if (replaceQuestContentOnComplete)
            {
                // 재료 목록 복원
                if (ingredientListContainer != null)
                {
                    ingredientListContainer.gameObject.SetActive(true);
                }

                // 제목 텍스트 복원
                if (questTitleText != null && _inventory != null)
                {
                    questTitleText.text = $"{ingredientQuestTitle} ({_inventory.CollectedCount}/{_inventory.TotalRequired})";
                }
            }
            else
            {
                // 기존 방식
                if (nextQuestArea != null)
                {
                    nextQuestArea.SetActive(false);
                }

                // 재료 목록 복원
                if (ingredientListContainer != null)
                {
                    ingredientListContainer.gameObject.SetActive(true);
                    
                    var canvasGroup = ingredientListContainer.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 1f;
                    }
                }
            }
        }

        /// <summary>
        /// 대화가 시작되면 QuestPanel을 숨깁니다.
        /// </summary>
        private void OnDialogueStart(DialogueData dialogue)
        {
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 대화가 끝나면 QuestPanel을 표시합니다.
        /// </summary>
        private void OnDialogueEnd()
        {
            // 퀘스트가 활성화된 상태인지 확인
            if (_questManager != null && _questManager.CurrentActiveIngredient != (IngredientId)(-1))
            {
                if (questPanel != null)
                {
                    questPanel.SetActive(true);
                }
            }
            else
            {
                // OnQuestChanged가 곧 호출될 것이므로 대기
                StartCoroutine(WaitForQuestActivation());
            }
        }
        
        /// <summary>
        /// 대화 종료 후 퀘스트 활성화를 대기합니다.
        /// </summary>
        private System.Collections.IEnumerator WaitForQuestActivation()
        {
            // 최대 1초 대기
            float waitTime = 0f;
            float maxWaitTime = 1f;
            
            while (waitTime < maxWaitTime)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
                
                if (_questManager != null && _questManager.CurrentActiveIngredient != (IngredientId)(-1))
                {
                    if (questPanel != null && !questPanel.activeSelf)
                    {
                        questPanel.SetActive(true);
                    }
                    yield break;
                }
            }
        }

        private void OnQuestChanged(IngredientId newQuestIngredient)
        {
            // 퀘스트가 모두 완료된 경우 (다음 퀘스트로 이동)
            if (newQuestIngredient == (IngredientId)(-1))
            {
                ShowNextQuest();
                return;
            }

            // 퀘스트 UI 생성 및 업데이트
            CreateQuestItems();
            UpdateQuestDisplay();
            
            // 대화가 재생 중이 아니면 QuestPanel 표시
            if (_dialogueSystem == null || !_dialogueSystem.IsPlaying)
            {
                if (questPanel != null)
                {
                    if (!questPanel.activeSelf)
                    {
                        questPanel.SetActive(true);
                    }
                }
            }
        }

        private void OnStrawberryCountChanged(int current, int total)
        {
            // 현재 퀘스트가 딸기인 경우에만 업데이트
            if (_questManager != null && _questManager.CurrentActiveIngredient == IngredientId.Strawberry)
            {
                UpdateQuestDisplay();
            }
        }

        private void OnIngredientCollected(IngredientId id)
        {
            UpdateQuestDisplay();
        }

        private void OnAllCollected()
        {
            // 모든 재료 수집 완료 - 다음 퀘스트 표시
            // (OnQuestChanged 이벤트가 발생하지 않는 경우를 위한 백업)
            ShowNextQuest();
        }

        private string GetDisplayName(IngredientId id)
        {
            if (_nameMap.TryGetValue(id, out string name))
                return name;

            return id.ToString(); // 폴백: enum 이름 그대로
        }

        [System.Serializable]
        private struct IngredientDisplayName
        {
            public IngredientId id;
            public string displayName;
        }
    }

    /// <summary>
    /// 개별 퀘스트 아이템 UI 컴포넌트입니다.
    /// </summary>
    public class QuestItemUI : MonoBehaviour
    {
        private IngredientId _ingredientId;
        private TextMeshProUGUI _checkText; // TextMeshProUGUI 사용
        private TextMeshProUGUI _nameText;
        private Color _incompleteColor;
        private Color _completedColor;
        private Color _checkMarkColor;
        private string _uncheckMark;
        private string _checkMark;

        /// <summary>
        /// 퀘스트 아이템을 초기화합니다.
        /// </summary>
        public void Initialize(IngredientId ingredientId, string displayName, Color incompleteColor, Color completedColor, Color checkMarkColor, string uncheckMark, string checkMark)
        {
            _ingredientId = ingredientId;
            _incompleteColor = incompleteColor;
            _completedColor = completedColor;
            _checkMarkColor = checkMarkColor;
            _uncheckMark = uncheckMark;
            _checkMark = checkMark;

            // UI 요소 찾기
            Transform checkIconTransform = transform.Find("CheckIcon");
            if (checkIconTransform != null)
            {
                // TextMeshProUGUI를 먼저 찾기
                _checkText = checkIconTransform.GetComponent<TextMeshProUGUI>();
                
                // TextMeshProUGUI가 없으면 Image를 찾아서 제거하고 TextMeshProUGUI 추가
                if (_checkText == null)
                {
                    var image = checkIconTransform.GetComponent<Image>();
                    if (image != null)
                    {
                        DestroyImmediate(image);
                    }
                    _checkText = checkIconTransform.gameObject.AddComponent<TextMeshProUGUI>();
                    _checkText.text = _uncheckMark; // 초기 텍스트만 설정
                    // fontSize, color, alignment는 Inspector에서 설정된 값 사용
                    // Overflow 설정: TextMeshProUGUI의 Overflow Mode를 Truncate 또는 Overflow로 설정 권장
                }
            }

            _nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

            // 텍스트 내용만 설정 (스타일은 Inspector에서 설정된 값 사용)
            if (_nameText != null)
            {
                _nameText.text = displayName;
                // Overflow 설정: TextMeshProUGUI의 Overflow Mode를 Truncate 또는 Overflow로 설정 권장
            }

            // 초기 상태: 미완료
            SetCompleted(false);
        }

        /// <summary>
        /// 완료 상태를 설정합니다. (색상과 텍스트 내용만 변경, 스타일은 유지)
        /// </summary>
        public void SetCompleted(bool completed)
        {
            if (_checkText != null)
            {
                // 체크 마크 텍스트와 색상만 변경 (fontSize, alignment는 Inspector 설정 유지)
                _checkText.text = completed ? _checkMark : _uncheckMark;
                _checkText.color = completed ? _checkMarkColor : _incompleteColor;
            }

            if (_nameText != null)
            {
                // 색상만 변경 (텍스트 내용, fontSize, alignment는 Inspector 설정 유지)
                _nameText.color = completed ? _completedColor : _incompleteColor;
            }
        }
    }
}



