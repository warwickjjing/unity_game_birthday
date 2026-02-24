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
        [SerializeField] private string interactPrompt = "소파에 앉기 [F]";

        [Tooltip("재료 미완성 시 프롬프트")]
        [SerializeField] private string incompletePrompt = "케이크를 완성하세요";

        private bool _hasTriggered;

        private void Awake()
        {
            // inventory가 Inspector에서 할당되지 않았으면 자동으로 할당
            if (inventory == null)
            {
                inventory = IngredientInventory.Instance;
                if (inventory != null)
                {
                    Debug.Log("[SofaInteractable] IngredientInventory를 자동으로 할당했습니다.");
                }
                else
                {
                    Debug.LogWarning("[SofaInteractable] IngredientInventory.Instance를 찾을 수 없습니다!");
                }
            }
        }

        private void Start()
        {
            // Start에서도 inventory 확인 (Awake에서 Instance가 아직 생성되지 않았을 수 있음)
            if (inventory == null)
            {
                inventory = IngredientInventory.Instance;
                if (inventory != null)
                {
                    Debug.Log("[SofaInteractable] Start에서 IngredientInventory를 할당했습니다.");
                }
                else
                {
                    // IngredientInventory를 찾을 수 없으면 코루틴으로 재시도
                    StartCoroutine(FindInventoryDelayed());
                }
            }
        }

        /// <summary>
        /// IngredientInventory를 찾을 때까지 재시도합니다.
        /// </summary>
        private System.Collections.IEnumerator FindInventoryDelayed()
        {
            int retryCount = 0;
            while (inventory == null && retryCount < 30) // 최대 30프레임 (약 0.5초) 재시도
            {
                yield return null;
                inventory = IngredientInventory.Instance;
                retryCount++;
            }
        }

        public bool CanInteract
        {
            get
            {
                if (_hasTriggered)
                {
                    return false;
                }

                // inventory가 null이면 Instance를 다시 찾기
                if (inventory == null)
                {
                    inventory = IngredientInventory.Instance;
                }
                
                var inv = inventory ?? IngredientInventory.Instance;
                bool canInteract = inv != null && inv.AllCollected;
                
                return canInteract;
            }
        }

        public string GetInteractPrompt()
        {
            if (_hasTriggered)
                return "";

            // inventory가 null이면 Instance를 다시 찾기
            if (inventory == null)
            {
                inventory = IngredientInventory.Instance;
            }
            
            var inv = inventory ?? IngredientInventory.Instance;
            if (inv != null && inv.AllCollected)
            {
                return interactPrompt;
            }
            else
            {
                return incompletePrompt;
            }
        }

        public void Interact(GameObject interactor)
        {
            // 디버그: 상호작용 시도 시 상태 확인
            var inv = inventory ?? IngredientInventory.Instance;

            if (!CanInteract)
            {
                var invCheck = inventory ?? IngredientInventory.Instance;
                return;
            }

            if (_hasTriggered)
            {
                return;
            }

            _hasTriggered = true;

            // 엔딩 컷씬 시작
            if (cutsceneController != null)
            {
                cutsceneController.StartFromSofa(interactor.transform);
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

