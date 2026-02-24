using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 설탕 NPC 선택지 기반 미니게임 Scene 컨테이너입니다.
    /// 여러 NPC 중 정답 NPC를 찾아 설탕을 획득하는 게임입니다.
    /// </summary>
    public class SugarMiniGameScene : MonoBehaviour
    {
        [Header("NPCs")]
        [Tooltip("씬에 배치된 NPC들 (자동으로 찾을 수도 있음)")]
        [SerializeField] private SugarNPC[] npcs;

        [Header("UI References")]
        [SerializeField] private Button quitButton;
        [Tooltip("DialogueUI 오브젝트 (비어있으면 자동으로 찾음)")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Game Settings")]
        [Tooltip("NPC를 자동으로 찾을지 여부")]
        [SerializeField] private bool autoFindNPCs = true;

        private bool _sugarObtained = false;
        private bool _isGameActive = false;

        private void Start()
        {
            Debug.Log("[SugarMiniGameScene] 설탕 미니게임 Scene 시작");

            // DialogueSystem 확인 및 생성 (없으면)
            EnsureDialogueSystem();

            // NPC 찾기
            if (autoFindNPCs || npcs == null || npcs.Length == 0)
            {
                npcs = FindObjectsOfType<SugarNPC>();
                Debug.Log($"[SugarMiniGameScene] NPC {npcs.Length}개 발견");
            }

            // 정답 NPC 랜덤 설정 (1명만 정답)
            if (npcs.Length > 0)
            {
                int correctIndex = Random.Range(0, npcs.Length);
                for (int i = 0; i < npcs.Length; i++)
                {
                    npcs[i].SetAsCorrectAnswer(i == correctIndex);
                }
                Debug.Log($"[SugarMiniGameScene] NPC {correctIndex + 1}번이 정답으로 설정됨");
            }
            else
            {
                Debug.LogWarning("[SugarMiniGameScene] NPC를 찾을 수 없습니다! 씬에 SugarNPC를 배치하세요.");
            }

            // DialogueUI 찾기 (정답 선택 이벤트 구독용)
            if (dialogueUI == null)
            {
                // 먼저 현재 씬(SugarMiniGameScene)에서 찾기
                dialogueUI = FindObjectOfType<DialogueUI>();
                
                if (dialogueUI == null)
                {
                    Debug.Log("[SugarMiniGameScene] FindObjectOfType으로 못 찾음, Resources.FindObjectsOfTypeAll 시도...");
                    // 현재 씬에서 못 찾으면 비활성화된 오브젝트도 포함해서 찾기
                    DialogueUI[] allDialogueUIs = Resources.FindObjectsOfTypeAll<DialogueUI>();
                    Debug.Log($"[SugarMiniGameScene] 전체 DialogueUI 개수: {allDialogueUIs.Length}");
                    
                    foreach (var ui in allDialogueUIs)
                    {
                        string sceneName = ui.gameObject.scene.name;
                        Debug.Log($"[SugarMiniGameScene] DialogueUI 발견 - 씬: {sceneName}, 활성화: {ui.gameObject.activeInHierarchy}, 이름: {ui.gameObject.name}");
                        
                        // SugarMiniGameScene에 있는 것만 사용
                        if (sceneName == "SugarMiniGameScene" || sceneName.Contains("SugarMiniGame"))
                        {
                            dialogueUI = ui;
                            Debug.Log($"[SugarMiniGameScene] DialogueUI 찾음! 씬: {sceneName}, 이름: {ui.gameObject.name}");
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log($"[SugarMiniGameScene] FindObjectOfType으로 DialogueUI 찾음 - 씬: {dialogueUI.gameObject.scene.name}, 이름: {dialogueUI.gameObject.name}");
                }
            }

            if (dialogueUI != null)
            {
                dialogueUI.OnCorrectChoiceSelected += OnSugarObtained;
                Debug.Log($"[SugarMiniGameScene] DialogueUI 찾음: {dialogueUI.gameObject.name}, 정답 선택 이벤트 구독");
            }
            else
            {
                Debug.LogError("[SugarMiniGameScene] DialogueUI를 찾을 수 없습니다! SugarMiniGameScene에 DialogueUI 컴포넌트가 있는지 확인하세요.");
            }

            // 종료 버튼 설정
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            _isGameActive = true;
        }

        /// <summary>
        /// DialogueSystem이 있는지 확인하고 없으면 찾거나 대기합니다.
        /// </summary>
        private void EnsureDialogueSystem()
        {
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem == null)
            {
                Debug.LogWarning("[SugarMiniGameScene] DialogueSystem.Instance가 null입니다. 찾는 중...");
                
                // DontDestroyOnLoad 오브젝트에서 찾기
                DialogueSystem[] allSystems = Resources.FindObjectsOfTypeAll<DialogueSystem>();
                foreach (var system in allSystems)
                {
                    if (system != null && system.gameObject != null)
                    {
                        // DontDestroyOnLoad 오브젝트 확인
                        if (system.gameObject.scene.name == "DontDestroyOnLoad" || 
                            system.gameObject.hideFlags == HideFlags.None)
                        {
                            Debug.Log($"[SugarMiniGameScene] DialogueSystem 발견: {system.gameObject.name}");
                            break;
                        }
                    }
                }

                // 여전히 없으면 한 프레임 대기 후 재시도
                StartCoroutine(WaitForDialogueSystem());
            }
            else
            {
                Debug.Log($"[SugarMiniGameScene] DialogueSystem 확인: {dialogueSystem.gameObject.name}");
                // DialogueSystem이 DialogueUI를 찾도록 강제
                dialogueSystem.FindDialogueUI();
            }
        }

        private System.Collections.IEnumerator WaitForDialogueSystem()
        {
            yield return null;
            yield return null; // 2프레임 대기

            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem != null)
            {
                Debug.Log("[SugarMiniGameScene] DialogueSystem 찾음 (지연 후)");
                dialogueSystem.FindDialogueUI();
            }
            else
            {
                Debug.LogError("[SugarMiniGameScene] DialogueSystem을 찾을 수 없습니다! HomeScene에서 DialogueSystem이 생성되었는지 확인하세요.");
            }
        }

        /// <summary>
        /// 정답 선택 시 호출됩니다 (DialogueUI 이벤트).
        /// 대화가 모두 끝난 후 HomeScene으로 복귀합니다.
        /// </summary>
        private void OnSugarObtained()
        {
            if (_sugarObtained || !_isGameActive)
                return;

            _sugarObtained = true;
            Debug.Log("[SugarMiniGameScene] 정답 선택됨! 대화 종료 대기 중...");

            // 설탕을 인벤토리에 추가 (대화 전에 미리 추가)
            var inventory = IngredientInventory.Instance;
            if (inventory != null)
            {
                if (inventory.Collect(IngredientId.Sugar))
                {
                    Debug.Log("[SugarMiniGameScene] 설탕이 인벤토리에 추가되었습니다!");
                }
                else
                {
                    Debug.LogWarning("[SugarMiniGameScene] 설탕을 인벤토리에 추가할 수 없습니다!");
                }
            }
            else
            {
                Debug.LogError("[SugarMiniGameScene] IngredientInventory를 찾을 수 없습니다!");
            }

            // DialogueSystem의 OnDialogueEnd 이벤트 구독하여 대화 종료 대기
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueEnd += OnCorrectDialogueEnd;
                Debug.Log("[SugarMiniGameScene] 대화 종료 이벤트 구독");
            }
            else
            {
                Debug.LogWarning("[SugarMiniGameScene] DialogueSystem을 찾을 수 없습니다. 즉시 복귀합니다.");
                CompleteGame(true);
            }
        }

        /// <summary>
        /// 정답 대화가 모두 끝난 후 호출됩니다.
        /// </summary>
        private void OnCorrectDialogueEnd()
        {
            Debug.Log("[SugarMiniGameScene] 정답 대화 종료! HomeScene으로 복귀합니다.");

            // 이벤트 구독 해제
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueEnd -= OnCorrectDialogueEnd;
            }

            _isGameActive = false;
            // 미니게임 완료 및 HomeScene 복귀
            CompleteGame(true);
        }

        private void CompleteGame(bool success)
        {
            if (!_isGameActive && !success)
                return;

            _isGameActive = false;

            Debug.Log($"[SugarMiniGameScene] 게임 {(success ? "성공" : "실패")}!");

            // 결과 전달 및 Scene 복귀
            var resultManager = MiniGameResult.Instance;
            if (resultManager != null)
            {
                resultManager.SetResultAndReturn(success);
            }
            else
            {
                Debug.LogError("[SugarMiniGameScene] MiniGameResult를 찾을 수 없습니다!");
            }
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("[SugarMiniGameScene] 종료 버튼 클릭");
            CompleteGame(false);
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (dialogueUI != null)
            {
                dialogueUI.OnCorrectChoiceSelected -= OnSugarObtained;
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }

            // DialogueSystem 이벤트 구독 해제
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueEnd -= OnCorrectDialogueEnd;
            }
        }
    }
}
