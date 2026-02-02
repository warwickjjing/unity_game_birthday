using UnityEngine;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.Player
{
    /// <summary>
    /// 플레이어 근처의 상호작용 가능한 오브젝트와 상호작용합니다.
    /// E키를 눌러 재료를 수집하거나 다른 오브젝트와 상호작용할 수 있습니다.
    /// </summary>
    public sealed class Interactor : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("상호작용 오브젝트 감지 범위")]
        [SerializeField] private float detectionRadius = 2f;
        
        [Tooltip("상호작용 가능한 오브젝트 레이어")]
        [SerializeField] private LayerMask interactionLayer = ~0;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("UI Feedback (Optional)")]
        [Tooltip("상호작용 가능할 때 표시할 UI")]
        [SerializeField] private GameObject interactionPrompt;

        private IInteractable _nearestInteractable;
        private Collider[] _detectionBuffer = new Collider[20];
        private bool _isPaused = false;

        private void Update()
        {
            if (_isPaused)
                return;

            DetectNearbyInteractables();
            HandleInteractionInput();
            UpdateInteractionPrompt();
        }

        /// <summary>
        /// Interactor를 일시정지/재개합니다.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;

            // 일시정지 시 프롬프트 숨기기
            if (paused && interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void DetectNearbyInteractables()
        {
            _nearestInteractable = null;
            float closestDistance = float.MaxValue;

            // 주변 콜라이더 검색
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRadius,
                _detectionBuffer,
                interactionLayer
            );

            // 디버그: 감지된 콜라이더 수 로그
            if (hitCount > 0)
            {
                Debug.Log($"[Interactor] Found {hitCount} colliders in range");
            }

            for (int i = 0; i < hitCount; i++)
            {
                var interactable = _detectionBuffer[i].GetComponent<IInteractable>();
                
                if (interactable == null)
                {
                    Debug.Log($"[Interactor] {_detectionBuffer[i].name} has no IInteractable component");
                    continue;
                }
                
                if (!interactable.CanInteract)
                {
                    Debug.Log($"[Interactor] {_detectionBuffer[i].name} CanInteract is false");
                    continue;
                }

                Transform interactableTransform = interactable.GetTransform();
                float distance = Vector3.Distance(transform.position, interactableTransform.position);
                
                // 상호작용 범위 안에 있는지 확인
                if (distance <= detectionRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    _nearestInteractable = interactable;
                    Debug.Log($"[Interactor] Nearest interactable: {_detectionBuffer[i].name} at distance {distance:F2}m");
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (_nearestInteractable == null)
                return;

            if (Input.GetKeyDown(interactKey))
            {
                Debug.Log($"[Interactor] Interacting with: {_nearestInteractable.GetInteractPrompt()}");
                _nearestInteractable.Interact(gameObject);
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null)
                return;

            bool shouldShow = _nearestInteractable != null;
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

            // 가장 가까운 상호작용 오브젝트 표시
            if (_nearestInteractable != null && Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Transform target = _nearestInteractable.GetTransform();
                if (target != null)
                {
                    Gizmos.DrawLine(transform.position, target.position);
                }
            }
        }

        /// <summary>
        /// 현재 가장 가까운 상호작용 가능한 오브젝트를 반환합니다.
        /// </summary>
        public IInteractable NearestInteractable => _nearestInteractable;
    }
}

