using UnityEngine;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 2D 밀가루 미니게임의 플레이어 컨트롤러입니다.
    /// WASD 또는 방향키로 이동하고 F키로 상호작용합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class FlourPlayer2D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float interactionRange = 1.5f;

        [Header("Animation")]
        [SerializeField] private string horizontalParameterName = "Horizontal";
        [SerializeField] private string verticalParameterName = "Vertical";
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private string carryingParameterName = "IsCarrying"; // 포대를 들고 있는지 상태

        [Header("Carry Settings")]
        [SerializeField] private Transform carryPosition;
        [SerializeField] private Vector3 carryOffset = new Vector3(0, 0.6f, -0.1f); // 포대 위치 (Y: 위쪽, Z: 뒤쪽)
        // bagScale과 bagPositionSmoothing은 Animator로 전환하면서 더 이상 사용하지 않음

        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _moveInput;
        private FlourBag _carriedBag;
        private Transform _playerTransform;
        private bool _wasRunning = false;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerTransform = transform;

            // Rigidbody2D 설정
            if (_rigidbody != null)
            {
                _rigidbody.gravityScale = 0f; // 중력 없음
                _rigidbody.drag = 5f; // 마찰 (낮춰서 더 즉각적인 반응)
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정
            }

            // Carry Position 자동 생성 (없으면)
            if (carryPosition == null)
            {
                GameObject carryObj = new GameObject("CarryPosition");
                carryObj.transform.SetParent(transform);
                carryPosition = carryObj.transform;
            }
            
            // Carry Offset 강제 설정 (Inspector 값과 무관하게)
            carryOffset = new Vector3(0, 0.6f, -0.1f);
            
            // Carry Position 초기 위치 설정
            if (carryPosition != null)
            {
                carryPosition.localPosition = carryOffset;
            }
        }

        private void Update()
        {
            HandleInput();
            HandleInteraction();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        /// <summary>
        /// 입력을 처리합니다.
        /// </summary>
        private void HandleInput()
        {
            // 입력 받기 (WASD 또는 방향키)
            float horizontal = 0f;
            float vertical = 0f;

            // 좌우 입력 (A/D 또는 Left/Right Arrow)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal = 1f;

            // 상하 입력 (W/S 또는 Up/Down Arrow)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                vertical = 1f;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                vertical = -1f;

            _moveInput.x = horizontal;
            _moveInput.y = vertical;

            // 대각선 이동 정규화
            if (_moveInput.magnitude > 1f)
            {
                _moveInput.Normalize();
            }

            // 스프라이트 좌우 반전 (좌우 이동 시)
            if (_spriteRenderer != null && _moveInput.x != 0)
            {
                _spriteRenderer.flipX = _moveInput.x < 0;
            }
        }

        /// <summary>
        /// 물리 기반 이동을 적용합니다.
        /// </summary>
        private void ApplyMovement()
        {
            // 달리기 감지 (Shift 키)
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            // 달리기 상태 변경 시 디버그 로그
            if (isRunning != _wasRunning)
            {
                _wasRunning = isRunning;
            }

            // Rigidbody2D로 이동 (FixedUpdate에서 즉시 적용)
            if (_rigidbody != null)
            {
                Vector2 targetVelocity = _moveInput * currentSpeed;
                _rigidbody.velocity = targetVelocity;
            }

            // 애니메이션 파라미터 업데이트
            UpdateAnimation(isRunning);
        }

        /// <summary>
        /// 애니메이션 파라미터를 업데이트합니다.
        /// </summary>
        private void UpdateAnimation(bool isRunning)
        {
            if (_animator != null)
            {
                if (!string.IsNullOrEmpty(horizontalParameterName))
                {
                    _animator.SetFloat(horizontalParameterName, _moveInput.x);
                }
                if (!string.IsNullOrEmpty(verticalParameterName))
                {
                    _animator.SetFloat(verticalParameterName, _moveInput.y);
                }
                if (!string.IsNullOrEmpty(speedParameterName))
                {
                    // Speed 파라미터를 0~10 범위로 설정
                    float animSpeed = _moveInput.magnitude * (isRunning ? 10f : 5f);
                    _animator.SetFloat(speedParameterName, animSpeed);
                }
                if (!string.IsNullOrEmpty(carryingParameterName))
                {
                    // 포대를 들고 있는지 상태 전달
                    _animator.SetBool(carryingParameterName, _carriedBag != null);
                }
            }
        }


        /// <summary>
        /// F키 입력을 처리하여 상호작용합니다.
        /// </summary>
        private void HandleInteraction()
        {
            if (!Input.GetKeyDown(KeyCode.F))
                return;

            // 근처의 IInteractable 찾기
            IInteractable nearestInteractable = FindNearestInteractable();

            if (nearestInteractable != null && nearestInteractable.CanInteract)
            {
                nearestInteractable.Interact(gameObject);
            }
        }

        /// <summary>
        /// 근처의 가장 가까운 IInteractable을 찾습니다.
        /// </summary>
        private IInteractable FindNearestInteractable()
        {
            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            // Physics2D로 근처 Collider2D 찾기
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);
            
            foreach (Collider2D col in colliders)
            {
                // 자기 자신 제외
                if (col.gameObject == gameObject)
                    continue;
                
                // IInteractable 찾기
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    interactable = col.GetComponentInParent<IInteractable>();
                }
                
                if (interactable != null && interactable.CanInteract)
                {
                    float distance = Vector2.Distance(transform.position, interactable.GetTransform().position);
                    if (distance < nearestDistance)
                    {
                        nearest = interactable;
                        nearestDistance = distance;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 포대를 집습니다.
        /// </summary>
        public void PickUpBag(FlourBag bag)
        {
            if (_carriedBag != null)
            {
                Debug.LogWarning("[FlourPlayer2D] 이미 포대를 들고 있습니다!");
                return;
            }

            if (bag == null)
            {
                Debug.LogWarning("[FlourPlayer2D] 포대가 null입니다!");
                return;
            }

            _carriedBag = bag;
            bag.PickUp(carryPosition != null ? carryPosition : transform);

            // Animator에 상태 전달
            if (_animator != null && !string.IsNullOrEmpty(carryingParameterName))
            {
                _animator.SetBool(carryingParameterName, true);
            }
        }

        /// <summary>
        /// 포대를 놓습니다.
        /// </summary>
        public void DropBag()
        {
            if (_carriedBag == null)
                return;

            FlourBag bag = _carriedBag;
            _carriedBag = null;

            Vector3 dropPosition = transform.position + new Vector3(0, 0.5f, 0);
            bag.Drop(dropPosition);

            // Animator에 상태 전달
            if (_animator != null && !string.IsNullOrEmpty(carryingParameterName))
            {
                _animator.SetBool(carryingParameterName, false);
            }
        }

        /// <summary>
        /// 현재 포대를 들고 있는지 확인합니다.
        /// </summary>
        public bool IsCarryingBag()
        {
            return _carriedBag != null;
        }

        /// <summary>
        /// 현재 들고 있는 포대를 반환합니다.
        /// </summary>
        public FlourBag GetCarriedBag()
        {
            return _carriedBag;
        }
    }
}

