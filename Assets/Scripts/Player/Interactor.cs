using UnityEngine;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.Player
{
    /// <summary>
    /// 플레이어 근처의 상호작용 가능한 오브젝트와 상호작용합니다.
    /// F키를 눌러 재료를 수집하거나 다른 오브젝트와 상호작용할 수 있습니다.
    /// </summary>
    public sealed class Interactor : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("상호작용 오브젝트 감지 범위")]
        [SerializeField] private float detectionRadius = 2f;
        
        [Tooltip("상호작용 가능한 오브젝트 레이어")]
        [SerializeField] private LayerMask interactionLayer = ~0;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;

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

            for (int i = 0; i < hitCount; i++)
            {
                var interactable = _detectionBuffer[i].GetComponent<IInteractable>();
                
                if (interactable == null || !interactable.CanInteract)
                    continue;

                Transform interactableTransform = interactable.GetTransform();
                float distance = Vector3.Distance(transform.position, interactableTransform.position);
                
                // 상호작용 범위 안에 있는지 확인
                if (distance <= detectionRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    _nearestInteractable = interactable;
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (_nearestInteractable == null)
                return;

            // F키와 E키 모두 확인 (호환성)
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log($"[Interactor] 상호작용 키 입력 감지 ({interactKey}) - 상호작용 시작: {_nearestInteractable.GetInteractPrompt()}");
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

