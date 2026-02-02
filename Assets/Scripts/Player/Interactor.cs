using UnityEngine;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.Player
{
    /// <summary>
    /// 플레이어 근처의 수집 가능한 재료와 상호작용합니다.
    /// E키를 눌러 재료를 수집할 수 있습니다.
    /// </summary>
    public sealed class Interactor : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("재료 감지 범위")]
        [SerializeField] private float detectionRadius = 2f;
        
        [Tooltip("재료 레이어")]
        [SerializeField] private LayerMask ingredientLayer = ~0;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("UI Feedback (Optional)")]
        [Tooltip("상호작용 가능할 때 표시할 UI")]
        [SerializeField] private GameObject interactionPrompt;

        private CollectibleIngredient _nearestIngredient;
        private Collider[] _detectionBuffer = new Collider[10];

        private void Update()
        {
            DetectNearbyIngredients();
            HandleInteractionInput();
            UpdateInteractionPrompt();
        }

        private void DetectNearbyIngredients()
        {
            _nearestIngredient = null;
            float closestDistance = float.MaxValue;

            // 주변 콜라이더 검색
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRadius,
                _detectionBuffer,
                ingredientLayer
            );

            for (int i = 0; i < hitCount; i++)
            {
                var ingredient = _detectionBuffer[i].GetComponent<CollectibleIngredient>();
                if (ingredient == null)
                    continue;

                float distance = Vector3.Distance(transform.position, ingredient.transform.position);
                
                // 재료의 상호작용 범위 안에 있는지 확인
                if (distance <= ingredient.InteractionRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    _nearestIngredient = ingredient;
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (_nearestIngredient == null)
                return;

            if (Input.GetKeyDown(interactKey))
            {
                if (_nearestIngredient.TryCollect())
                {
                    Debug.Log($"[Interactor] {_nearestIngredient.Id} 수집!");
                    _nearestIngredient = null;
                }
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null)
                return;

            bool shouldShow = _nearestIngredient != null;
            if (interactionPrompt.activeSelf != shouldShow)
            {
                interactionPrompt.SetActive(shouldShow);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 감지 범위 시각화
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 가장 가까운 재료 표시
            if (_nearestIngredient != null && Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _nearestIngredient.transform.position);
            }
        }
    }
}

