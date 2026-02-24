using System.Collections.Generic;
using UnityEngine;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Managers
{
    [System.Serializable]
    public class DialogueEntry
    {
        [TextArea(2, 5)]
        public string text;
        [Tooltip("화자 이름 (선택 사항)")]
        public string speakerName = "";
        [Tooltip("화자 초상화 (우측 상단에 표시)")]
        public Sprite speakerPortrait;
    }

    [System.Serializable]
    public class IngredientQuestStep
    {
        [Header("Quest Info")]
        public IngredientId ingredientId;
        
        [Header("Quest Title")]
        [Tooltip("퀘스트 UI에 표시될 제목 (예: '숲 속에서 딸기 찾기')")]
        public string questTitle = "";
        
        [Header("Quest Dialogues")]
        [Tooltip("이 퀘스트 시작 시 순서대로 표시될 대화들")]
        public List<DialogueEntry> questDialogues = new List<DialogueEntry>();
        
        [Header("Target Objects")]
        public List<GameObject> targetIngredientObjects = new List<GameObject>();
    }

    public class QuestSequenceManager : MonoBehaviour
    {
        public static QuestSequenceManager Instance { get; private set; }

        [Header("Quest Sequence")]
        [SerializeField] private List<IngredientQuestStep> questSequence = new List<IngredientQuestStep>();

        [Header("Game Start Dialogues")]
        [Tooltip("게임 시작 시 순서대로 표시될 대화들")]
        [SerializeField] private List<DialogueEntry> gameStartDialogues = new List<DialogueEntry>();

        private int _currentQuestIndex = 0;
        private IngredientInventory _inventory;
        private DialogueSystem _dialogueSystem;
        private bool _isFirstQuestActivated = false;
        private bool _waitingForDialogueToActivateNextQuest = false;
        private bool _hasGameStartDialoguePlayedInHome = false; // Home 씬에서 게임 시작 대화가 이미 재생되었는지 (instance 변수)
        
        // 미니게임에서 재료 수집 시 다음 퀘스트 ID를 저장 (HomeScene 전환 후 처리)
        private static int? _pendingNextQuestIndex = null;

        // 퀘스트 변경 이벤트
        public event System.Action<IngredientId> OnQuestChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad 제거: HomeScene에만 존재하도록 변경
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            // HomeScene으로 전환될 때마다 초기화 확인
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName == "Home")
            {
                InitializeForHomeScene();
            }
        }

        private void Start()
        {
            InitializeForHomeScene();
        }

        /// <summary>
        /// 대기 중인 퀘스트를 처리합니다 (외부에서 호출 가능).
        /// </summary>
        public void ProcessPendingQuest()
        {
            if (_pendingNextQuestIndex.HasValue)
            {
                int questIndexToComplete = _pendingNextQuestIndex.Value;
                _pendingNextQuestIndex = null; // 플래그 클리어
                
                Debug.Log($"[QuestSequenceManager] ProcessPendingQuest: 퀘스트 {questIndexToComplete} 완료 및 다음 퀘스트 시작");
                CompleteQuestAndStartNext(questIndexToComplete);
            }
        }

        /// <summary>
        /// HomeScene에서 초기화를 수행합니다.
        /// </summary>
        private void InitializeForHomeScene()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != "Home")
            {
                return; // HomeScene이 아니면 초기화하지 않음
            }

            // 미니게임에서 돌아온 경우 대기 중인 다음 퀘스트가 있는지 먼저 확인
            if (_pendingNextQuestIndex.HasValue)
            {
                int questIndexToComplete = _pendingNextQuestIndex.Value;
                _pendingNextQuestIndex = null; // 플래그 클리어
                
                // 인벤토리와 대화 시스템이 준비되어 있는지 확인
                if (_inventory == null)
                    _inventory = IngredientInventory.Instance;
                if (_dialogueSystem == null)
                    _dialogueSystem = DialogueSystem.Instance;
                
                if (_inventory != null && _dialogueSystem != null)
                {
                    Debug.Log($"[QuestSequenceManager] 미니게임에서 돌아옴: 퀘스트 {questIndexToComplete} 완료 및 다음 퀘스트 시작");
                    CompleteQuestAndStartNext(questIndexToComplete);
                    return;
                }
                else
                {
                    Debug.LogWarning("[QuestSequenceManager] 인벤토리 또는 대화 시스템이 아직 준비되지 않았습니다. 다음 프레임에 재시도합니다.");
                    // 다음 프레임에 다시 시도하기 위해 플래그 복원
                    _pendingNextQuestIndex = questIndexToComplete;
                }
            }

            // 이미 초기화되었는지 확인 (중복 초기화 방지)
            if (_inventory != null && _dialogueSystem != null)
            {
                // 미니게임에서 돌아온 경우 대기 중인 다음 퀘스트가 있는지 먼저 확인
                if (_pendingNextQuestIndex.HasValue)
                {
                    int questIndexToComplete = _pendingNextQuestIndex.Value;
                    _pendingNextQuestIndex = null; // 플래그 클리어
                    
                    Debug.Log($"[QuestSequenceManager] 미니게임에서 돌아옴 (이미 초기화됨): 퀘스트 {questIndexToComplete} 완료 및 다음 퀘스트 시작");
                    CompleteQuestAndStartNext(questIndexToComplete);
                    return;
                }
                
                // 이미 초기화되었고 게임 시작 대화가 재생되었으면 스킵
                if (_hasGameStartDialoguePlayedInHome)
                {
                    // 대화가 이미 재생되었으면 바로 첫 번째 퀘스트 활성화 (아직 활성화되지 않았으면)
                    if (!_isFirstQuestActivated)
                    {
                        ActivateFirstQuest();
                    }
                }
                return;
            }

            _inventory = IngredientInventory.Instance;
            _dialogueSystem = DialogueSystem.Instance;

            if (_inventory == null)
            {
                Debug.LogError("[QuestSequenceManager] IngredientInventory를 찾을 수 없습니다!");
            }
            else
            {
                // 중복 구독 방지
                _inventory.OnIngredientCollected -= OnIngredientCollected;
                _inventory.OnIngredientCollected += OnIngredientCollected;
            }

            if (_dialogueSystem == null)
            {
                Debug.LogError("[QuestSequenceManager] DialogueSystem을 찾을 수 없습니다!");
            }
            else
            {
                // 중복 구독 방지
                _dialogueSystem.OnDialogueEnd -= OnDialogueEnd;
                // 대화 종료 이벤트 구독 (첫 번째 퀘스트 활성화용)
                _dialogueSystem.OnDialogueEnd += OnDialogueEnd;
            }

            // 퀘스트 시퀀스 확인
            if (questSequence == null || questSequence.Count == 0)
            {
                return;
            }

            // 현재 퀘스트 인덱스를 IngredientInventory의 수집 상태를 기반으로 계산
            SyncCurrentQuestIndexFromInventory();

            // 초기 파티클 설정 (첫 번째 퀘스트만 활성화)
            UpdateParticleStates();

            // 게임 시작 대화 재생 여부 결정
            // 이미 재료를 수집했으면 (미니게임에서 돌아온 경우) 게임 시작 대화를 재생하지 않음
            bool hasCollectedAnyIngredient = false;
            if (_inventory != null && questSequence != null && questSequence.Count > 0)
            {
                // 첫 번째 퀘스트의 재료가 이미 수집되었는지 확인
                if (questSequence.Count > 0)
                {
                    var firstQuestIngredient = questSequence[0].ingredientId;
                    hasCollectedAnyIngredient = _inventory.IsCollected(firstQuestIngredient);
                }
            }

            bool shouldPlayGameStartDialogue = !hasCollectedAnyIngredient && 
                                               _dialogueSystem != null && 
                                               gameStartDialogues != null && 
                                               gameStartDialogues.Count > 0;

            if (shouldPlayGameStartDialogue)
            {
                // 처음 HomeScene에 진입하는 경우: 게임 시작 대화 재생
                _hasGameStartDialoguePlayedInHome = true;
                StartCoroutine(StartDialogueAfterDelay());
                Debug.Log("[QuestSequenceManager] 처음 HomeScene 진입: 게임 시작 대화 재생");
            }
            else
            {
                // 미니게임에서 돌아온 경우: 게임 시작 대화 재생하지 않음
                if (hasCollectedAnyIngredient)
                {
                    Debug.Log("[QuestSequenceManager] 미니게임에서 돌아옴: 게임 시작 대화 스킵 (이미 재료 수집됨)");
                }
                else
                {
                    Debug.LogWarning("[QuestSequenceManager] DialogueSystem이 없거나 시작 대화가 비어있습니다.");
                }
                
                // 첫 번째 퀘스트가 아직 활성화되지 않았으면 활성화
                if (!_isFirstQuestActivated)
                {
                    ActivateFirstQuest();
                }
            }
        }

        /// <summary>
        /// HomeScene 로드 시 게임 시작 대화와 첫 번째 퀘스트 대화를 순서대로 표시합니다.
        /// </summary>
        private System.Collections.IEnumerator StartDialogueAfterDelay()
        {
            // 모든 Start()가 완료되도록 2프레임 대기
            yield return null;
            yield return null;

            var startDialogue = new List<DialogueData>();

            // 1. 게임 시작 대화 추가 (gameStartDialogues)
            foreach (var entry in gameStartDialogues)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.text))
                {
                    startDialogue.Add(new DialogueData 
                    { 
                        text = entry.text,
                        speakerName = entry.speakerName,
                        speakerPortrait = entry.speakerPortrait,
                        trigger = DialogueTrigger.GameStart 
                    });
                }
            }

            // 2. 첫 번째 퀘스트 대화 추가 (첫 번째 퀘스트의 questDialogues)
            if (_currentQuestIndex < questSequence.Count)
            {
                var currentQuest = questSequence[_currentQuestIndex];
                if (currentQuest.questDialogues != null && currentQuest.questDialogues.Count > 0)
                {
                    foreach (var entry in currentQuest.questDialogues)
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.text))
                        {
                            startDialogue.Add(new DialogueData 
                            { 
                                text = entry.text,
                                speakerName = entry.speakerName,
                                speakerPortrait = entry.speakerPortrait,
                                trigger = DialogueTrigger.Manual 
                            });
                        }
                    }
                }
            }

            // 3. 모든 대화를 순서대로 표시
            if (startDialogue.Count > 0)
            {
                _dialogueSystem.StartDialogueSequence(startDialogue);
                Debug.Log($"[QuestSequenceManager] 게임 시작 대화 시작 ({startDialogue.Count}개: 게임 시작 {gameStartDialogues?.Count ?? 0}개 + 첫 퀘스트 {(_currentQuestIndex < questSequence.Count ? questSequence[_currentQuestIndex].questDialogues?.Count ?? 0 : 0)}개)");
            }
            else
            {
                Debug.LogWarning("[QuestSequenceManager] 표시할 대화가 없습니다. 첫 번째 퀘스트를 활성화합니다.");
                // 대화가 없으면 바로 첫 번째 퀘스트 활성화
                if (!_isFirstQuestActivated)
                {
                    ActivateFirstQuest();
                }
            }

            // 첫 번째 퀘스트 활성화는 대화가 끝난 후 OnDialogueEnd에서 처리됨
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnIngredientCollected -= OnIngredientCollected;
            }

            if (_dialogueSystem != null)
            {
                _dialogueSystem.OnDialogueEnd -= OnDialogueEnd;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 대화가 끝나면 호출되며, 첫 번째 퀘스트 또는 다음 퀘스트를 활성화합니다.
        /// </summary>
        private void OnDialogueEnd()
        {
            // 첫 번째 퀘스트 활성화
            if (!_isFirstQuestActivated)
            {
                ActivateFirstQuest();
            }
            // 다음 퀘스트 활성화 대기 중인 경우
            else if (_waitingForDialogueToActivateNextQuest)
            {
                _waitingForDialogueToActivateNextQuest = false;
                ActivateNextQuest();
            }
        }

        /// <summary>
        /// 첫 번째 퀘스트를 활성화합니다 (대화 끝난 후).
        /// </summary>
        private void ActivateFirstQuest()
        {
            if (_currentQuestIndex < questSequence.Count)
            {
                _isFirstQuestActivated = true;
                OnQuestChanged?.Invoke(questSequence[_currentQuestIndex].ingredientId);
                Debug.Log($"[QuestSequenceManager] 첫 번째 퀘스트 활성화: {questSequence[_currentQuestIndex].ingredientId}");
            }
        }

        private void OnIngredientCollected(IngredientId id)
        {
            // 수집된 재료에 해당하는 퀘스트 인덱스 찾기
            int completedQuestIndex = -1;
            for (int i = 0; i < questSequence.Count; i++)
            {
                if (questSequence[i].ingredientId == id)
                {
                    completedQuestIndex = i;
                    break;
                }
            }

            if (completedQuestIndex == -1)
            {
                Debug.LogWarning($"[QuestSequenceManager] 수집된 재료 {id}에 해당하는 퀘스트를 찾을 수 없습니다.");
                return;
            }

            // 퀘스트 완료 처리 및 다음 퀘스트 시작
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != "Home")
            {
                // 미니게임 씬에서 수집한 경우: HomeScene 전환 후 처리
                Debug.Log($"[QuestSequenceManager] 미니게임 씬({currentSceneName})에서 재료 수집. 퀘스트 {completedQuestIndex} 완료, 다음 퀘스트 {completedQuestIndex + 1} 시작 예약");
                _pendingNextQuestIndex = completedQuestIndex;
            }
            else
            {
                // HomeScene에서 수집한 경우: 즉시 처리
                CompleteQuestAndStartNext(completedQuestIndex);
            }
        }

        /// <summary>
        /// 퀘스트를 완료하고 다음 퀘스트를 시작합니다.
        /// 예: CompleteQuestAndStartNext(0) → 퀘스트 0 완료, 퀘스트 1 시작, 대화 표시
        /// </summary>
        /// <param name="completedQuestIndex">완료할 퀘스트 인덱스</param>
        public void CompleteQuestAndStartNext(int completedQuestIndex)
        {
            // 인벤토리와 대화 시스템이 준비되어 있는지 확인
            if (_inventory == null)
                _inventory = IngredientInventory.Instance;
            if (_dialogueSystem == null)
                _dialogueSystem = DialogueSystem.Instance;
            
            if (_inventory == null || _dialogueSystem == null)
            {
                Debug.LogWarning($"[QuestSequenceManager] CompleteQuestAndStartNext: 시스템이 준비되지 않았습니다. 인벤토리: {_inventory != null}, 대화시스템: {_dialogueSystem != null}");
                // 다음 프레임에 재시도하기 위해 코루틴 시작
                StartCoroutine(CompleteQuestAndStartNextDelayed(completedQuestIndex));
                return;
            }
            
            CompleteQuestAndStartNextInternal(completedQuestIndex);
        }
        
        /// <summary>
        /// 시스템이 준비될 때까지 대기한 후 퀘스트를 완료하고 다음 퀘스트를 시작합니다.
        /// </summary>
        private System.Collections.IEnumerator CompleteQuestAndStartNextDelayed(int completedQuestIndex)
        {
            // 시스템이 준비될 때까지 대기
            while (_inventory == null || _dialogueSystem == null)
            {
                if (_inventory == null)
                    _inventory = IngredientInventory.Instance;
                if (_dialogueSystem == null)
                    _dialogueSystem = DialogueSystem.Instance;
                yield return null;
            }
            
            Debug.Log($"[QuestSequenceManager] CompleteQuestAndStartNextDelayed: 시스템 준비 완료, 퀘스트 {completedQuestIndex} 처리 시작");
            CompleteQuestAndStartNextInternal(completedQuestIndex);
        }
        
        private void CompleteQuestAndStartNextInternal(int completedQuestIndex)
        {
            if (completedQuestIndex < 0 || completedQuestIndex >= questSequence.Count)
            {
                Debug.LogWarning($"[QuestSequenceManager] 잘못된 퀘스트 인덱스: {completedQuestIndex}");
                return;
            }

            Debug.Log($"[QuestSequenceManager] 퀘스트 {completedQuestIndex} 완료 처리 시작 (재료: {questSequence[completedQuestIndex].ingredientId})");

            // 현재 퀘스트 인덱스를 완료된 퀘스트 다음으로 설정
            _currentQuestIndex = completedQuestIndex + 1;
            
            Debug.Log($"[QuestSequenceManager] 다음 퀘스트 인덱스: {_currentQuestIndex} (총 퀘스트 수: {questSequence.Count})");

            // 파티클 상태 업데이트
            UpdateParticleStates();

            // 다음 퀘스트가 있는지 확인
            if (_currentQuestIndex >= questSequence.Count)
            {
                // 모든 퀘스트 완료
                Debug.Log("[QuestSequenceManager] 모든 퀘스트 완료!");
                OnQuestChanged?.Invoke((IngredientId)(-1));
                return;
            }

            // 다음 퀘스트 대화 표시
            var nextQuest = questSequence[_currentQuestIndex];
            if (nextQuest.questDialogues != null && nextQuest.questDialogues.Count > 0)
            {
                var dialogueList = new List<DialogueData>();
                
                foreach (var entry in nextQuest.questDialogues)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.text))
                    {
                        dialogueList.Add(new DialogueData 
                        { 
                            text = entry.text,
                            speakerName = entry.speakerName,
                            speakerPortrait = entry.speakerPortrait,
                            trigger = DialogueTrigger.IngredientCollected 
                        });
                    }
                }
                
                if (dialogueList.Count > 0)
                {
                    _waitingForDialogueToActivateNextQuest = true;
                    
                    if (_dialogueSystem != null)
                    {
                        _dialogueSystem.FindDialogueUI();
                        StartCoroutine(PlayNextQuestDialogueAfterDelay(dialogueList));
                    }
                    else
                    {
                        Debug.LogError("[QuestSequenceManager] DialogueSystem을 찾을 수 없습니다!");
                        // 대화가 없으면 즉시 다음 퀘스트 활성화
                        ActivateNextQuest();
                    }
                }
                else
                {
                    // 대화가 없으면 즉시 다음 퀘스트 활성화
                    ActivateNextQuest();
                }
            }
            else
            {
                // 대화가 없으면 즉시 다음 퀘스트 활성화
                ActivateNextQuest();
            }
        }
        

        /// <summary>
        /// 다음 퀘스트를 활성화합니다 (대화 없거나 대화 끝난 후).
        /// </summary>
        private void ActivateNextQuest()
        {
            var nextIngredient = _currentQuestIndex < questSequence.Count 
                ? questSequence[_currentQuestIndex].ingredientId 
                : (IngredientId)(-1);
            OnQuestChanged?.Invoke(nextIngredient);
            Debug.Log($"[QuestSequenceManager] 다음 퀘스트 활성화: {nextIngredient}");
        }

        /// <summary>
        /// IngredientInventory의 수집 상태를 기반으로 현재 퀘스트 인덱스를 동기화합니다.
        /// </summary>
        private void SyncCurrentQuestIndexFromInventory()
        {
            if (_inventory == null || questSequence == null || questSequence.Count == 0)
                return;

            // 수집된 재료를 기반으로 현재 퀘스트 인덱스 계산
            for (int i = 0; i < questSequence.Count; i++)
            {
                if (!_inventory.IsCollected(questSequence[i].ingredientId))
                {
                    _currentQuestIndex = i;
                    Debug.Log($"[QuestSequenceManager] 현재 퀘스트 인덱스 동기화: {_currentQuestIndex} ({questSequence[i].ingredientId})");
                    return;
                }
            }

            // 모든 재료를 수집한 경우
            _currentQuestIndex = questSequence.Count;
            Debug.Log("[QuestSequenceManager] 모든 재료 수집 완료");
        }

        /// <summary>
        /// 다음 퀘스트 대화를 재생합니다 (DialogueUI 찾기 시간 확보).
        /// </summary>
        private System.Collections.IEnumerator PlayNextQuestDialogueAfterDelay(List<DialogueData> dialogueList)
        {
            // DialogueUI 찾기 시간 확보
            if (_dialogueSystem != null)
            {
                _dialogueSystem.FindDialogueUI();
                yield return null; // DialogueUI 찾기 완료 대기
                yield return null; // 추가 대기
                
                _dialogueSystem.StartDialogueSequence(dialogueList);
                Debug.Log($"[QuestSequenceManager] 다음 퀘스트 대화 재생 시작 ({dialogueList.Count}개)");
            }
        }

        private void UpdateParticleStates()
        {
            // 모든 재료의 파티클을 비활성화
            for (int i = 0; i < questSequence.Count; i++)
            {
                var step = questSequence[i];
                bool isActive = (i == _currentQuestIndex);

                foreach (var obj in step.targetIngredientObjects)
                {
                    if (obj != null)
                    {
                        // 파티클 시스템 제어
                        var particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
                        foreach (var ps in particleSystems)
                        {
                            if (isActive)
                            {
                                if (!ps.isPlaying)
                                    ps.Play();
                            }
                            else
                            {
                                if (ps.isPlaying)
                                    ps.Stop();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 파티클 상태를 강제로 업데이트합니다 (외부에서 호출 가능).
        /// </summary>
        public void RefreshParticleStates()
        {
            UpdateParticleStates();
        }

        /// <summary>
        /// 특정 재료의 퀘스트 스텝을 찾아 반환합니다.
        /// </summary>
        public IngredientQuestStep GetQuestStepByIngredient(IngredientId id)
        {
            foreach (var step in questSequence)
            {
                if (step.ingredientId == id)
                    return step;
            }
            return null;
        }

        public bool CanCollectIngredient(IngredientId id)
        {
            if (_currentQuestIndex >= questSequence.Count)
                return false;

            return questSequence[_currentQuestIndex].ingredientId == id;
        }

        /// <summary>
        /// 현재 활성화된 퀘스트 스텝 정보를 반환합니다.
        /// </summary>
        public IngredientQuestStep GetCurrentQuestStep()
        {
            if (_currentQuestIndex < questSequence.Count)
                return questSequence[_currentQuestIndex];
            return null;
        }

        /// <summary>
        /// 현재 퀘스트의 제목을 반환합니다.
        /// </summary>
        public string GetCurrentQuestTitle()
        {
            var step = GetCurrentQuestStep();
            if (step != null)
            {
                // questTitle이 설정되어 있으면 그것을 사용
                if (!string.IsNullOrEmpty(step.questTitle))
                    return step.questTitle;
                
                // 없으면 첫 번째 대화를 제목으로 사용
                if (step.questDialogues != null && step.questDialogues.Count > 0)
                {
                    var firstDialogue = step.questDialogues[0];
                    if (firstDialogue != null && !string.IsNullOrEmpty(firstDialogue.text))
                        return firstDialogue.text;
                }
            }
            return "";
        }

        public IngredientId CurrentActiveIngredient
        {
            get
            {
                if (_currentQuestIndex < questSequence.Count)
                    return questSequence[_currentQuestIndex].ingredientId;
                return (IngredientId)(-1);
            }
        }
    }
}


