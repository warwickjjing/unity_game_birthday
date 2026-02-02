using UnityEngine;
using BirthdayCakeQuest.Cutscene;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 거실 소파와의 상호작용을 처리합니다.
    /// 모든 재료를 수집한 후 소파에 앉으면 엔딩 컷씬이 시작됩니다.
    /// </summary>
    public sealed class SofaInteractable : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [Tooltip("엔딩 컷씬 컨트롤러")]
        [SerializeField] private EndingCutsceneController cutsceneController;

        [Tooltip("재료 인벤토리 (조건 체크용)")]
        [SerializeField] private IngredientInventory inventory;

        [Header("Settings")]
        [Tooltip("상호작용 프롬프트 텍스트")]
        [SerializeField] private string interactPrompt = "소파에 앉기 [E]";

        [Tooltip("재료 미완성 시 프롬프트")]
        [SerializeField] private string incompletePrompt = "케이크를 완성하세요";

        private bool _hasTriggered;

        public bool CanInteract
        {
            get
            {
                if (_hasTriggered)
                    return false;

                // 모든 재료를 수집했는지 확인
                return inventory != null && inventory.AllCollected;
            }
        }

        public string GetInteractPrompt()
        {
            if (_hasTriggered)
                return "";

            if (inventory != null && inventory.AllCollected)
                return interactPrompt;
            else
                return incompletePrompt;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
            {
                Debug.LogWarning("[SofaInteractable] Cannot interact - ingredients not collected.");
                return;
            }

            if (_hasTriggered)
            {
                Debug.LogWarning("[SofaInteractable] Already triggered.");
                return;
            }

            _hasTriggered = true;
            Debug.Log("[SofaInteractable] Player sat on sofa, starting ending cutscene!");

            // 엔딩 컷씬 시작
            if (cutsceneController != null)
            {
                cutsceneController.StartFromSofa(interactor.transform);
            }
            else
            {
                Debug.LogError("[SofaInteractable] EndingCutsceneController is not assigned!");
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 시각화
            Gizmos.color = CanInteract ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}

