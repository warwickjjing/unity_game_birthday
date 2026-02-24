using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;
using System.Collections.Generic;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 설탕 미니게임의 NPC입니다.
    /// 플레이어와 대화하고 선택지를 제공합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SugarNPC : MonoBehaviour, IInteractable
    {
        [Header("NPC Settings")]
        [Tooltip("정답 NPC 여부 (설탕을 가지고 있는 NPC)")]
        [SerializeField] private bool hasAnswer = false;

        [Header("Dialogue")]
        [Tooltip("정답 NPC일 때 표시할 대화 목록 (선택지 포함)")]
        [SerializeField] private List<DialogueData> correctDialogues = new List<DialogueData>();

        [Tooltip("틀린 NPC일 때 표시할 대화 목록")]
        [SerializeField] private List<DialogueData> wrongDialogues = new List<DialogueData>();

        public bool CanInteract => true;

        public string GetInteractPrompt()
        {
            return "대화하기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log($"[SugarNPC] ========== Interact 호출됨 ==========");
            Debug.Log($"[SugarNPC] NPC 이름: {gameObject.name}");
            Debug.Log($"[SugarNPC] hasAnswer: {hasAnswer}");
            Debug.Log($"[SugarNPC] interactor: {interactor?.name}");

            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem == null)
            {
                Debug.LogError("[SugarNPC] DialogueSystem.Instance가 null입니다! DialogueSystem이 씬에 있는지 확인하세요.");
                return;
            }

            Debug.Log($"[SugarNPC] DialogueSystem 찾음: {dialogueSystem.gameObject.name}");

            // DialogueSystem이 DialogueUI를 찾았는지 확인
            dialogueSystem.FindDialogueUI();
            
            // DialogueUI 상태 확인
            var dialogueUI = dialogueSystem.GetDialogueUI();
            if (dialogueUI != null)
            {
                Debug.Log($"[SugarNPC] DialogueUI 확인: {dialogueUI.gameObject.name}, 활성화: {dialogueUI.gameObject.activeInHierarchy}, 씬: {dialogueUI.gameObject.scene.name}");
            }
            else
            {
                Debug.LogWarning("[SugarNPC] DialogueUI를 찾을 수 없습니다!");
            }

            if (hasAnswer)
            {
                // 정답 NPC 대화
                Debug.Log($"[SugarNPC] hasAnswer = true, correctDialogues 사용");
                if (correctDialogues != null && correctDialogues.Count > 0)
                {
                    Debug.Log($"[SugarNPC] 정답 NPC 대화 시작 - 대화 개수: {correctDialogues.Count}");
                    foreach (var dialogue in correctDialogues)
                    {
                        Debug.Log($"[SugarNPC] 대화 내용: {dialogue.text}, 화자: {dialogue.speakerName}, 선택지 개수: {(dialogue.choices != null ? dialogue.choices.Count : 0)}");
                        if (dialogue.choices != null && dialogue.choices.Count > 0)
                        {
                            foreach (var choice in dialogue.choices)
                            {
                                Debug.Log($"[SugarNPC]   - 선택지: {choice.choiceText}, 정답: {choice.isCorrect}");
                            }
                        }
                    }
                    dialogueSystem.StartDialogueSequence(correctDialogues);
                }
                else
                {
                    Debug.LogError($"[SugarNPC] correctDialogues가 비어있습니다! Inspector에서 대화를 추가하세요. (hasAnswer: {hasAnswer})");
                }
            }
            else
            {
                // 틀린 NPC 대화
                Debug.Log($"[SugarNPC] hasAnswer = false, wrongDialogues 사용");
                if (wrongDialogues != null && wrongDialogues.Count > 0)
                {
                    Debug.Log($"[SugarNPC] 틀린 NPC 대화 시작 - 대화 개수: {wrongDialogues.Count}");
                    foreach (var dialogue in wrongDialogues)
                    {
                        Debug.Log($"[SugarNPC] 대화 내용: {dialogue.text}, 화자: {dialogue.speakerName}");
                    }
                    dialogueSystem.StartDialogueSequence(wrongDialogues);
                }
                else
                {
                    Debug.LogError($"[SugarNPC] wrongDialogues가 비어있습니다! Inspector에서 대화를 추가하세요. (hasAnswer: {hasAnswer})");
                }
            }
            
            Debug.Log($"[SugarNPC] ========== Interact 완료 ==========");
        }

        private void Awake()
        {
            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[SugarNPC] WorldSpaceInteractionPrompt 자동 추가됨");
            }
        }

        /// <summary>
        /// 정답 NPC로 설정합니다.
        /// </summary>
        public void SetAsCorrectAnswer(bool isCorrect)
        {
            hasAnswer = isCorrect;
        }

        /// <summary>
        /// 정답 NPC인지 확인합니다.
        /// </summary>
        public bool IsCorrectAnswer()
        {
            return hasAnswer;
        }
    }
}

