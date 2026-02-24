using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 2D 밀가루 미니게임 Scene 컨트롤러입니다.
    /// NPC 퀘스트를 통해 포대 5개를 운반하는 게임을 관리합니다.
    /// </summary>
    public class FlourMiniGame2DScene : MonoBehaviour
    {
        [Header("Game Objects")]
        [SerializeField] private FlourPlayer2D player;
        [SerializeField] private FlourNPC npc;
        [SerializeField] private FlourDeliveryZone deliveryZone;
        [SerializeField] private GameObject[] flourBags; // 5개

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Button quitButton;

        [Header("Initial Dialogue")]
        [Tooltip("씬 진입 시 자동으로 표시할 대화 목록 (비어있으면 표시 안 함)")]
        [SerializeField] private List<FlourDialogueData> initialDialogues = new List<FlourDialogueData>();

        private int _deliveredBags = 0;
        private const int TARGET_BAGS = 5;
        private bool _hasShownInitialDialogue = false; // 인스턴스 변수로 변경 (씬마다 독립적)

        private void Start()
        {
            Debug.Log("[FlourMiniGame2DScene] 2D 밀가루 미니게임 Scene 시작");

            // 자동으로 찾기
            if (player == null)
            {
                player = FindObjectOfType<FlourPlayer2D>();
            }

            if (npc == null)
            {
                npc = FindObjectOfType<FlourNPC>();
            }

            if (deliveryZone == null)
            {
                deliveryZone = FindObjectOfType<FlourDeliveryZone>();
            }

            // 배달 이벤트 구독
            if (deliveryZone != null)
            {
                deliveryZone.OnDelivery += HandleBagDelivered;
            }

            // 종료 버튼 설정
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
                quitButton.onClick.AddListener(OnQuitButtonClicked);
                if (!quitButton.gameObject.activeSelf)
                    quitButton.gameObject.SetActive(true);
                if (!quitButton.interactable)
                    quitButton.interactable = true;
            }
            else
            {
                // 자동으로 찾기
                quitButton = FindObjectOfType<UnityEngine.UI.Button>();
                if (quitButton != null)
                {
                    quitButton.onClick.RemoveListener(OnQuitButtonClicked);
                    quitButton.onClick.AddListener(OnQuitButtonClicked);
                }
            }

            // NPC에 Scene 컨트롤러 참조 전달
            if (npc != null)
            {
                // FlourNPC는 이미 FindObjectOfType으로 찾으므로 자동 연결됨
            }

            // 씬 진입 시 초기 대화 표시
            StartCoroutine(ShowInitialDialogueAfterDelay());
        }

        /// <summary>
        /// 씬 진입 시 초기 대화를 표시합니다.
        /// </summary>
        private IEnumerator ShowInitialDialogueAfterDelay()
        {
            // 씬이 완전히 로드되도록 대기
            yield return null;
            yield return null;
            yield return null; // 추가 대기

            Debug.Log($"[FlourMiniGame2DScene] 초기 대화 체크 시작 - _hasShownInitialDialogue: {_hasShownInitialDialogue}, initialDialogues.Count: {(initialDialogues != null ? initialDialogues.Count : 0)}");

            // 이미 표시했으면 스킵
            if (_hasShownInitialDialogue)
            {
                Debug.Log("[FlourMiniGame2DScene] 이미 초기 대화를 표시했으므로 스킵");
                yield break;
            }

            // 초기 대화가 설정되어 있으면 표시
            if (initialDialogues == null || initialDialogues.Count == 0)
            {
                Debug.LogWarning("[FlourMiniGame2DScene] initialDialogues가 비어있습니다! Inspector에서 대화를 추가하세요.");
                yield break;
            }

            Debug.Log($"[FlourMiniGame2DScene] FlourDialogueUI 찾기 시작...");
            
            // 먼저 현재 씬(FlourMiniGame)에서 찾기
            FlourDialogueUI dialogueUI = FindObjectOfType<FlourDialogueUI>();
            
            if (dialogueUI == null)
            {
                Debug.Log("[FlourMiniGame2DScene] FindObjectOfType으로 못 찾음, Resources.FindObjectsOfTypeAll 시도...");
                // 현재 씬에서 못 찾으면 비활성화된 오브젝트도 포함해서 찾기
                FlourDialogueUI[] allDialogueUIs = Resources.FindObjectsOfTypeAll<FlourDialogueUI>();
                Debug.Log($"[FlourMiniGame2DScene] 전체 FlourDialogueUI 개수: {allDialogueUIs.Length}");
                
                foreach (var ui in allDialogueUIs)
                {
                    string sceneName = ui.gameObject.scene.name;
                    Debug.Log($"[FlourMiniGame2DScene] FlourDialogueUI 발견 - 씬: {sceneName}, 활성화: {ui.gameObject.activeInHierarchy}");
                    
                    // FlourMiniGame 씬에 있는 것만 사용
                    if (sceneName == "FlourMiniGameScene" || sceneName.Contains("FlourMiniGame"))
                    {
                        dialogueUI = ui;
                        Debug.Log($"[FlourMiniGame2DScene] FlourDialogueUI 찾음! 씬: {sceneName}");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log($"[FlourMiniGame2DScene] FindObjectOfType으로 FlourDialogueUI 찾음 - 씬: {dialogueUI.gameObject.scene.name}");
            }
            
            if (dialogueUI != null)
            {
                Debug.Log($"[FlourMiniGame2DScene] 대화 표시 시작 - 대화 개수: {initialDialogues.Count}");
                dialogueUI.ShowDialogue(initialDialogues);
                _hasShownInitialDialogue = true;
                Debug.Log($"[FlourMiniGame2DScene] 초기 대화 표시 완료 ({initialDialogues.Count}개)");
            }
            else
            {
                Debug.LogError("[FlourMiniGame2DScene] FlourDialogueUI를 찾을 수 없습니다! FlourMiniGameScene에 FlourDialogueUI가 있는지 확인하세요.");
            }
        }

        /// <summary>
        /// 포대 배달을 처리합니다.
        /// </summary>
        private void HandleBagDelivered()
        {
            _deliveredBags++;
            Debug.Log($"[FlourMiniGame2DScene] 포대 배달: {_deliveredBags}/{TARGET_BAGS}");

            if (_deliveredBags >= TARGET_BAGS)
            {
                // 퀘스트 완료는 NPC에서 처리
                Debug.Log("[FlourMiniGame2DScene] 모든 포대 배달 완료!");
            }
        }

        /// <summary>
        /// 퀘스트 완료 시 호출됩니다.
        /// </summary>
        public void OnQuestCompleted()
        {
            Debug.Log("[FlourMiniGame2DScene] 퀘스트 완료! 미니게임 성공!");

            // MiniGameResult에 결과 전달
            var resultManager = MiniGameResult.Instance;
            if (resultManager != null)
            {
                resultManager.SetResultAndReturn(true);
            }
            else
            {
                Debug.LogError("[FlourMiniGame2DScene] MiniGameResult를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 종료 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[FlourMiniGame2DScene] 종료 버튼 클릭 - 미니게임 실패");

            // MiniGameResult에 실패 결과 전달
            var resultManager = MiniGameResult.Instance;
            if (resultManager != null)
            {
                resultManager.SetResultAndReturn(false);
            }
            else
            {
                Debug.LogError("[FlourMiniGame2DScene] MiniGameResult를 찾을 수 없습니다!");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (deliveryZone != null)
            {
                deliveryZone.OnDelivery -= HandleBagDelivered;
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }
        }
    }
}

