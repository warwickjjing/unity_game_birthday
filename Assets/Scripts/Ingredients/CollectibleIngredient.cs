using UnityEngine;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 월드에 배치되어 플레이어가 수집할 수 있는 재료 오브젝트입니다.
    /// Interactor가 범위 내에서 E키를 누르면 수집됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CollectibleIngredient : MonoBehaviour
    {
        [Header("Ingredient Info")]
        [SerializeField] private IngredientId ingredientId;
        
        [Header("Interaction")]
        [Tooltip("플레이어가 이 거리 안에 있을 때 수집 가능")]
        [SerializeField] private float interactionRadius = 1.5f;

        [Header("Visual")]
        [Tooltip("수집 시 오브젝트를 파괴할지 여부")]
        [SerializeField] private bool destroyOnCollect = true;
        
        [Tooltip("수집 시 재생할 파티클 효과 (선택)")]
        [SerializeField] private GameObject collectEffectPrefab;

        public IngredientId Id => ingredientId;
        public float InteractionRadius => interactionRadius;

        private bool _collected;

        /// <summary>
        /// 이 재료를 수집합니다.
        /// </summary>
        /// <returns>성공적으로 수집되었으면 true</returns>
        public bool TryCollect()
        {
            if (_collected)
                return false;

            var inventory = IngredientInventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("[CollectibleIngredient] IngredientInventory를 찾을 수 없습니다!");
                return false;
            }

            if (!inventory.Collect(ingredientId))
                return false;

            _collected = true;

            // 수집 효과 재생
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            // 오브젝트 처리
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 상호작용 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

