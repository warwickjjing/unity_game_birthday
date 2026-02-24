using System.Linq;
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

            // 방법 1: Collider 기반 검색 (기존 방식)
            // Trigger Collider도 확실히 감지하도록 QueryTriggerInteraction.Collide 사용
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRadius,
                _detectionBuffer,
                interactionLayer,
                QueryTriggerInteraction.Collide // Trigger Collider도 감지
            );

            for (int i = 0; i < hitCount; i++)
            {
                if (_detectionBuffer[i] == null)
                    continue;

                var interactable = _detectionBuffer[i].GetComponent<IInteractable>();
                
                // GetComponentInParent도 시도 (자식 오브젝트에 컴포넌트가 있을 수 있음)
                if (interactable == null)
                {
                    interactable = _detectionBuffer[i].GetComponentInParent<IInteractable>();
                }
                
                if (interactable == null)
                    continue;

                if (!interactable.CanInteract)
                    continue;

                Transform interactableTransform = interactable.GetTransform();
                if (interactableTransform == null)
                    continue;

                float distance = Vector3.Distance(transform.position, interactableTransform.position);
                
                // 상호작용 범위 안에 있는지 확인
                if (distance <= detectionRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    _nearestInteractable = interactable;
                }
            }

            // 방법 2: Collider가 없는 IInteractable도 검색 (보조 방식)
            // 모든 IInteractable을 찾아서 거리 체크
            IInteractable[] allInteractables = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IInteractable)
                .Cast<IInteractable>()
                .ToArray();

            foreach (var interactable in allInteractables)
            {
                if (interactable == null || !interactable.CanInteract)
                    continue;

                Transform interactableTransform = interactable.GetTransform();
                if (interactableTransform == null)
                    continue;

                float distance = Vector3.Distance(transform.position, interactableTransform.position);
                
                // 상호작용 범위 안에 있고, 더 가까운 경우
                if (distance <= detectionRadius && distance < closestDistance)
                {
                    // 이미 Collider로 감지된 경우는 제외 (중복 방지)
                    bool alreadyDetected = false;
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (_detectionBuffer[i] != null)
                        {
                            var detectedInteractable = _detectionBuffer[i].GetComponent<IInteractable>();
                            if (detectedInteractable == null)
                            {
                                detectedInteractable = _detectionBuffer[i].GetComponentInParent<IInteractable>();
                            }
                            
                            if (detectedInteractable == interactable)
                            {
                                alreadyDetected = true;
                                break;
                            }
                        }
                    }

                    if (!alreadyDetected)
                    {
                        closestDistance = distance;
                        _nearestInteractable = interactable;
                    }
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (_nearestInteractable == null)
                return;

            // F키 입력 확인
            if (Input.GetKeyDown(interactKey))
            {
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

