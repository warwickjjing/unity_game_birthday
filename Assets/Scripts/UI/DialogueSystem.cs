using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BirthdayCakeQuest.UI
{
    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("선택지 텍스트")]
        public string choiceText;
        [Tooltip("정답 여부 (설탕 미니게임 등에서 사용)")]
        public bool isCorrect = false;
        [Tooltip("이 선택지를 선택한 후 표시할 대화 목록 (최대 3단계 깊이 권장)")]
        [SerializeReference] // 순환 참조 문제 해결
        public List<DialogueData> nextDialogues = new List<DialogueData>();
    }

    [Serializable]
    public class DialogueData
    {
        [TextArea(3, 10)]
        public string text;
        public string speakerName = "";
        [Tooltip("화자의 초상화 이미지 (우측 상단에 표시됨)")]
        public Sprite speakerPortrait;
        public DialogueTrigger trigger = DialogueTrigger.Manual;
        
        [Header("Choices (Optional)")]
        [Tooltip("선택지가 있으면 버튼이 표시됩니다 (최대 2단계 깊이 권장)")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public bool hasChoices => choices != null && choices.Count > 0;
    }

    public enum DialogueTrigger
    {
        Manual,              // 수동 트리거
        GameStart,           // 게임 시작 시
        IngredientCollected, // 재료 수집 시
        AllIngredientsCollected // 모든 재료 수집 시
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("UI Reference")]
        [Tooltip("DialogueUI 오브젝트를 여기에 드래그하세요 (씬 전환 시 자동으로 다시 찾습니다)")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Dialogue Data")]
        [SerializeField] private List<DialogueData> dialogues = new List<DialogueData>();

        public event Action<DialogueData> OnDialogueStart;
        public event Action OnDialogueEnd;

        private Queue<DialogueData> _dialogueQueue = new Queue<DialogueData>();
        private bool _isPlaying = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // 씬 로드 이벤트 구독
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬이 로드될 때마다 DialogueUI 다시 찾기 (약간의 지연 후)
            StartCoroutine(FindDialogueUIDelayed());
        }

        private System.Collections.IEnumerator FindDialogueUIDelayed()
        {
            // 한 프레임 대기 (씬이 완전히 로드되도록)
            yield return null;
            FindDialogueUI();
        }

        private void Start()
        {
            // 초기 DialogueUI 찾기
            FindDialogueUI();
        }

        public void FindDialogueUI()
        {
            // Inspector에 할당된 참조가 유효한지 확인
            if (dialogueUI != null && dialogueUI.gameObject != null)
            {
                // 현재 활성 씬의 DialogueUI인지 확인
                Scene activeScene = SceneManager.GetActiveScene();
                if (dialogueUI.gameObject.scene == activeScene)
                {
                    return;
                }
                else
                {
                    // 다른 씬의 DialogueUI면 무효화
                    dialogueUI = null;
                }
            }

            // 참조가 없거나 무효하면 새로 찾기
            // 현재 활성 씬 찾기
            Scene currentActiveScene = SceneManager.GetActiveScene();
            // Resources.FindObjectsOfTypeAll 사용 (비활성화된 오브젝트도 찾음)
            DialogueUI[] allDialogueUIs = Resources.FindObjectsOfTypeAll<DialogueUI>();
            DialogueUI foundUI = null;
            
            // 1단계: 현재 활성 씬의 DialogueUI 우선 찾기
            foreach (var ui in allDialogueUIs)
            {
                if (ui == null || ui.gameObject == null)
                    continue;

                string sceneName = ui.gameObject.scene.name;
                bool isActive = ui.gameObject.activeInHierarchy;
                bool isLoaded = ui.gameObject.scene.isLoaded;
                
                // 현재 활성 씬에 있는 것 우선 선택 (비활성화되어 있어도 OK)
                if (ui.gameObject.scene == currentActiveScene && isLoaded)
                {
                    // 에디터 전용 오브젝트나 프리팹이 아닌 실제 씬 오브젝트만
                    if (!ui.gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy) &&
                        !ui.gameObject.hideFlags.HasFlag(HideFlags.DontSave))
                    {
                        foundUI = ui;
                        break;
                    }
                }
            }

            // 2단계: 활성 씬에서 못 찾으면 다른 로드된 씬에서 찾기 (비활성화되어 있어도 OK)
            if (foundUI == null)
            {
                foreach (var ui in allDialogueUIs)
                {
                    if (ui == null || ui.gameObject == null)
                        continue;

                    string sceneName = ui.gameObject.scene.name;
                    bool isLoaded = ui.gameObject.scene.isLoaded;
                    
                    // 로드된 씬의 DialogueUI만 선택
                    if (isLoaded && !string.IsNullOrEmpty(sceneName))
                    {
                        // 에디터 전용 오브젝트나 프리팹이 아닌 실제 씬 오브젝트만
                        if (!ui.gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy) &&
                            !ui.gameObject.hideFlags.HasFlag(HideFlags.DontSave))
                        {
                            // SugarMiniGameScene 우선 선택
                            if (sceneName.Contains("SugarMiniGame") || sceneName.Contains("FlourMiniGame"))
                            {
                                foundUI = ui;
                                break;
                            }
                            // 그 외에는 첫 번째로 찾은 것 선택
                            else if (foundUI == null)
                            {
                                foundUI = ui;
                            }
                        }
                    }
                }
            }

            if (foundUI != null)
            {
                dialogueUI = foundUI;
            }
        }

        /// <summary>
        /// 현재 DialogueUI 참조를 반환합니다 (디버깅용).
        /// </summary>
        public DialogueUI GetDialogueUI()
        {
            return dialogueUI;
        }

        public void TriggerDialogue(DialogueTrigger trigger)
        {
            var triggeredDialogues = dialogues.FindAll(d => d.trigger == trigger);
            if (triggeredDialogues.Count > 0)
            {
                StartDialogueSequence(triggeredDialogues);
            }
        }

        public void StartDialogueSequence(List<DialogueData> dialogueList)
        {
            // DialogueUI 다시 찾기 (씬 전환 후일 수 있음)
            FindDialogueUI();

            if (dialogueUI == null)
            {
                return;
            }

            // DialogueUI가 비활성화되어 있으면 활성화
            if (!dialogueUI.gameObject.activeInHierarchy)
            {
                dialogueUI.gameObject.SetActive(true);
            }
            
            // DialoguePanel도 활성화 확인
            if (dialogueUI.DialoguePanel != null && !dialogueUI.DialoguePanel.activeSelf)
            {
                dialogueUI.DialoguePanel.SetActive(true);
            }

            _dialogueQueue.Clear();
            foreach (var dialogue in dialogueList)
            {
                _dialogueQueue.Enqueue(dialogue);
            }
            PlayNextDialogue();
        }

        public void PlayNextDialogue()
        {
            if (_dialogueQueue.Count > 0)
            {
                var dialogue = _dialogueQueue.Dequeue();
                _isPlaying = true;
                OnDialogueStart?.Invoke(dialogue);
            }
            else
            {
                _isPlaying = false;
                OnDialogueEnd?.Invoke();
            }
        }

        public bool IsPlaying => _isPlaying;
    }
}

