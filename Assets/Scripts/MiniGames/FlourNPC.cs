using UnityEngine;
using System.Collections.Generic;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.MiniGames;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 미니게임의 NPC입니다.
    /// 퀘스트를 제공하고 진행 상황을 추적합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FlourNPC : MonoBehaviour, IInteractable
    {
        [Header("Dialogue - Start")]
        [SerializeField] private List<FlourDialogueData> startDialogues = new List<FlourDialogueData>();

        [Header("Dialogue - Progress (사용하지 않을 경우 비워두기)")]
        [SerializeField] private List<FlourDialogueData> progressDialogues = new List<FlourDialogueData>();
        
        [Header("Dialogue - Complete")]
        [SerializeField] private List<FlourDialogueData> completeDialogues = new List<FlourDialogueData>();

        [Header("References")]
        [SerializeField] private FlourDialogueUI dialogueUI;
        [SerializeField] private FlourMiniGame2DScene sceneController; // FlourMiniGame2DScene 참조

        [Header("NPC Info (Optional)")]
        [SerializeField] private string npcName = "마을 주민";
        [SerializeField] private Sprite npcPortrait;

        private QuestState _questState = QuestState.NotStarted;
        private int _deliveredBags = 0;
        private const int TARGET_BAGS = 5;
        private bool _completionDialogueShown = false;

        private enum QuestState
        {
            NotStarted,
            InProgress,
            Completed
        }

        public bool CanInteract => true;

        public string GetInteractPrompt()
        {
            return "대화하기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[FlourNPC] WorldSpaceInteractionPrompt 자동 추가됨");
            }

            // 자동으로 찾기
            if (dialogueUI == null)
            {
                dialogueUI = FindObjectOfType<FlourDialogueUI>();
            }

            if (sceneController == null)
            {
                sceneController = FindObjectOfType<FlourMiniGame2DScene>();
            }

            // 기본 대화가 비어있으면 자동 생성
            InitializeDefaultDialogues();
        }

        /// <summary>
        /// 기본 대화를 초기화합니다.
        /// </summary>
        private void InitializeDefaultDialogues()
        {
            if (startDialogues.Count == 0)
            {
                startDialogues.Add(new FlourDialogueData
                {
                    text = "포대 5자루를 창고로 옮겨주면 한 자루를 드리겠어요!",
                    speakerName = npcName,
                    speakerPortrait = npcPortrait
                });
            }

            if (completeDialogues.Count == 0)
            {
                completeDialogues.Add(new FlourDialogueData
                {
                    text = "고마워요! 약속대로 밀가루 한 자루입니다.",
                    speakerName = npcName,
                    speakerPortrait = npcPortrait
                });
            }

            // progressDialogues는 선택 사항이므로 자동 생성하지 않음
        }

        public void Interact(GameObject interactor)
        {
            switch (_questState)
            {
                case QuestState.NotStarted:
                    StartQuest();
                    break;
                case QuestState.InProgress:
                    ShowProgress();
                    break;
                case QuestState.Completed:
                    ShowComplete();
                    break;
            }
        }

        /// <summary>
        /// 퀘스트를 시작합니다.
        /// </summary>
        private void StartQuest()
        {
            _questState = QuestState.InProgress;
            _deliveredBags = 0;

            ShowDialogues(startDialogues);

            Debug.Log("[FlourNPC] 퀘스트 시작!");
        }

        /// <summary>
        /// 진행 상황을 표시합니다.
        /// </summary>
        private void ShowProgress()
        {
            if (progressDialogues.Count > 0)
            {
                // 사용자 정의 진행 대화 표시
                ShowDialogues(progressDialogues);
            }
            else
            {
                // 기본 진행 메시지
                List<FlourDialogueData> defaultProgress = new List<FlourDialogueData>
                {
                    new FlourDialogueData
                    {
                        text = $"포대를 {_deliveredBags}/{TARGET_BAGS} 옮겼어요. 계속해주세요!",
                        speakerName = npcName,
                        speakerPortrait = npcPortrait
                    }
                };
                ShowDialogues(defaultProgress);
            }
        }

        /// <summary>
        /// 완료 메시지를 표시합니다.
        /// </summary>
        private void ShowComplete()
        {
            ShowDialogues(completeDialogues);
        }

        /// <summary>
        /// 대화 목록을 표시합니다.
        /// </summary>
        private void ShowDialogues(List<FlourDialogueData> dialogues)
        {
            if (dialogueUI != null)
            {
                dialogueUI.ShowDialogue(dialogues);
            }
            else
            {
                // Fallback: 콘솔에 출력
                foreach (var dialogue in dialogues)
                {
                    Debug.Log($"[FlourNPC] {dialogue.speakerName}: {dialogue.text}");
                }
            }
        }

        /// <summary>
        /// 포대 배달을 기록합니다.
        /// </summary>
        public void OnBagDelivered()
        {
            if (_questState != QuestState.InProgress)
                return;

            _deliveredBags++;

            if (_deliveredBags >= TARGET_BAGS)
            {
                _questState = QuestState.Completed;
                
                // 완료 대화가 끝난 후 씬 전환을 위해 이벤트 구독
                if (dialogueUI != null && !_completionDialogueShown)
                {
                    _completionDialogueShown = true;
                    dialogueUI.OnDialogueComplete += OnCompletionDialogueFinished;
                }
                
                ShowComplete();
            }
            else
            {
                ShowProgress();
            }

            Debug.Log($"[FlourNPC] 포대 배달: {_deliveredBags}/{TARGET_BAGS}");
        }
        
        /// <summary>
        /// 완료 대화(completeDialogues)가 모두 끝난 후 호출됩니다.
        /// 순서: completeDialogues 종료 → 밀가루 수집 → HomeScene 전환
        /// </summary>
        private void OnCompletionDialogueFinished()
        {
            Debug.Log("[FlourNPC] 완료 대화 종료 - 밀가루 수집 및 씬 전환 시작");
            
            // 1. 이벤트 구독 해제
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueComplete -= OnCompletionDialogueFinished;
            }
            
            // 2. 밀가루를 인벤토리에 추가 (completeDialogues 종료 후)
            var inventory = BirthdayCakeQuest.Ingredients.IngredientInventory.Instance;
            if (inventory != null)
            {
                if (inventory.Collect(BirthdayCakeQuest.Ingredients.IngredientId.Flour))
                {
                    Debug.Log("[FlourNPC] 밀가루가 인벤토리에 추가되었습니다!");
                }
                else
                {
                    Debug.LogWarning("[FlourNPC] 밀가루를 인벤토리에 추가할 수 없습니다! (이미 수집됨?)");
                }
            }
            else
            {
                Debug.LogError("[FlourNPC] IngredientInventory를 찾을 수 없습니다!");
            }
            
            // 3. Scene 컨트롤러에 완료 알림 (밀가루 수집 후 씬 전환)
            if (sceneController != null)
            {
                sceneController.OnQuestCompleted();
            }
            else
            {
                Debug.LogError("[FlourNPC] SceneController를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 퀘스트가 완료되었는지 확인합니다.
        /// </summary>
        public bool IsQuestCompleted()
        {
            return _questState == QuestState.Completed;
        }

        /// <summary>
        /// 퀘스트가 시작되었는지 확인합니다 (NotStarted가 아닌지).
        /// </summary>
        public bool IsQuestStarted()
        {
            return _questState != QuestState.NotStarted;
        }
    }
}

