using UnityEngine;

namespace BirthdayCakeQuest.Player
{
    /// <summary>
    /// VRM 캐릭터의 이동을 제어합니다.
    /// WASD로 이동, Shift로 달리기가 가능합니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;
        private bool _inputEnabled = true;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            // Animator 자동 검색 (VRM 모델은 자식 오브젝트에 있음)
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (!_inputEnabled)
                return;

            HandleMovement();
            ApplyGravity();
        }

        private void HandleMovement()
        {
            // 입력 받기
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            // 애니메이션용 변수
            bool isMoving = direction.magnitude >= 0.1f;
            float currentSpeed = 0f;

            if (isMoving)
            {
                // 이동 속도 결정 (Shift로 달리기)
                currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

                // 카메라 기준 방향으로 변환
                Vector3 moveDirection = TransformDirectionRelativeToCamera(direction);

                // 이동
                _controller.Move(moveDirection * currentSpeed * Time.deltaTime);

                // 회전
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            // 애니메이션 파라미터 업데이트
            if (animator != null)
            {
                animator.SetBool("IsMoving", isMoving);
                animator.SetFloat("Speed", currentSpeed);
            }
        }

        private Vector3 TransformDirectionRelativeToCamera(Vector3 direction)
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
                return direction;

            // 카메라의 forward/right를 기준으로 방향 변환 (Y축 회전만 고려)
            Vector3 forward = mainCam.transform.forward;
            Vector3 right = mainCam.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * direction.z + right * direction.x;
        }

        private void ApplyGravity()
        {
            // 바닥 체크
            _isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                groundMask
            );

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // 바닥에 붙어있도록
            }

            // 중력 적용
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// 플레이어 입력을 활성화/비활성화합니다.
        /// 컷씬 재생 시 입력을 막을 때 사용합니다.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            
            if (!enabled)
            {
                // 입력 비활성화 시 속도 초기화
                _velocity.x = 0f;
                _velocity.z = 0f;
            }
        }

        public bool IsInputEnabled => _inputEnabled;

        private void OnDrawGizmosSelected()
        {
            // 바닥 체크 범위 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
        }
    }
}

