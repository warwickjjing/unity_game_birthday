using UnityEngine;
using UnityEngine.AI;

namespace BirthdayCakeQuest.Animals
{
    /// <summary>
    /// 동물이 랜덤하게 이동하도록 하는 스크립트입니다.
    /// 반경을 설정하면 그 안에서만 움직이고, 설정하지 않으면 전범위에서 움직입니다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimalWander : MonoBehaviour
    {
        [Tooltip("회전 속도")]
        [SerializeField] private float rotationSpeed = 5f;

        [Tooltip("목표 지점까지의 거리 (이 거리 안이면 도착으로 간주)")]
        [SerializeField] private float arrivalDistance = 1f;

        [Tooltip("새 목표 지점을 선택하는 간격 (초)")]
        [SerializeField] private float wanderInterval = 3f;

        [Header("Movement Area (반경 제한)")]
        [Tooltip("반경 제한 사용 여부 (체크 해제 시 전범위 이동)")]
        [SerializeField] private bool useRadiusLimit = false;

        [Tooltip("시작 위치를 중심으로 한 이동 반경 (useRadiusLimit이 true일 때만 사용)")]
        [SerializeField] private float wanderRadius = 10f;

        [Tooltip("시작 위치 (비어있으면 현재 위치를 시작 위치로 사용)")]
        [SerializeField] private Transform startPosition;

        [Header("Animation")]
        [Tooltip("Animator의 Speed 파라미터 이름 (float)")]
        [SerializeField] private string speedParameterName = "Speed";

        [Tooltip("Animator의 State 파라미터 이름 (float, CreatureMover 호환)")]
        [SerializeField] private string stateParameterName = "State";

        [Tooltip("Animator의 Vert 파라미터 이름 (float, CreatureMover 호환)")]
        [SerializeField] private string vertParameterName = "Vert";

        [Tooltip("IsMoving 파라미터 이름 (Bool, 사용 안 하면 비워두기)")]
        [SerializeField] private string isMovingParameterName = "";

        [Header("State Values (CreatureMover 호환)")]
        [Tooltip("Idle 상태 값 (float)")]
        [SerializeField] private float idleStateValue = 0f;

        [Tooltip("Walk 상태 값 (float)")]
        [SerializeField] private float walkStateValue = 1f;

        [Tooltip("Run 상태 값 (float)")]
        [SerializeField] private float runStateValue = 2f;

        [Header("Move Speed (for animation)")]
        [Tooltip("걷기 속도")]
        [SerializeField] private float walkSpeed = 2f;

        [Tooltip("뛰기 속도")]
        [SerializeField] private float runSpeed = 4f;

        [Tooltip("뛰기 허용 여부")]
        [SerializeField] private bool allowRun = true;

        [Tooltip("뛰기 확률 (0~1)")]
        [SerializeField, Range(0f, 1f)] private float runChance = 0.2f;

        [Header("Collision Detection")]
        [Tooltip("벽 충돌 감지 거리")]
        [SerializeField] private float wallCheckDistance = 1f;

        [Tooltip("벽 충돌 감지 레이어 (벽이 있는 레이어)")]
        [SerializeField] private LayerMask wallLayer = ~0; // 기본값: 모든 레이어

        private Animator _animator;
        private Rigidbody _rigidbody;
        private Vector3 _targetPosition;
        private Vector3 _startPos; // 시작 위치 저장
        private float _wanderTimer = 0f;
        private bool _isMoving = false;
        private float _currentMoveSpeed; // 현재 이동 속도 (walk/run)
        private bool _isEscaping = false; // 달아나는 상태인지 여부
        private float _originalWalkSpeed; // 원래 걷기 속도
        private float _originalRunSpeed; // 원래 뛰기 속도
        private bool _originalUseRadiusLimit; // 원래 반경 제한 사용 여부

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();

            // Rigidbody가 있으면 설정
            if (_rigidbody != null)
            {
                // 회전은 스크립트에서 제어하므로 freeze
                _rigidbody.freezeRotation = true;
            }

            // 시작 위치 설정
            if (startPosition != null)
            {
                _startPos = startPosition.position;
            }
            else
            {
                _startPos = transform.position;
            }

            // 원래 속도 저장
            _originalWalkSpeed = walkSpeed;
            _originalRunSpeed = runSpeed;
            _originalUseRadiusLimit = useRadiusLimit;

            // 초기 목표 지점 설정
            SetNewTarget();
        }

        private void Start()
        {
            // Start에서도 시작 위치 확인 (Awake 이후에 위치가 변경될 수 있음)
            if (startPosition == null && _startPos == Vector3.zero)
            {
                _startPos = transform.position;
            }
        }

        private void Update()
        {
            // 목표 지점까지 이동
            if (_isMoving)
            {
                MoveTowardsTarget();
            }

            // 일정 시간마다 새 목표 지점 선택
            _wanderTimer += Time.deltaTime;
            if (_wanderTimer >= wanderInterval)
            {
                _wanderTimer = 0f;
                SetNewTarget();
            }
        }

        private void SetNewTarget()
        {
            Vector3 centerPos;
            float radius;

            if (useRadiusLimit)
            {
                // 반경 제한 사용: 시작 위치를 중심으로 반경 내에서만 이동
                centerPos = _startPos;
                radius = wanderRadius;
            }
            else
            {
                // 반경 제한 없음: 현재 위치를 중심으로 큰 범위에서 이동
                centerPos = transform.position;
                radius = 1000f; // 매우 큰 값 (실질적으로 제한 없음)
            }

            // 랜덤한 위치 선택
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            _targetPosition = centerPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // 반경 제한이 있을 경우, 시작 위치로부터의 거리 확인
            if (useRadiusLimit)
            {
                Vector3 toTarget = _targetPosition - _startPos;
                toTarget.y = 0f;
                if (toTarget.magnitude > wanderRadius)
                {
                    // 반경을 벗어나면 반경 내로 제한
                    toTarget = toTarget.normalized * wanderRadius;
                    _targetPosition = _startPos + toTarget;
                }
            }

            // 바닥에 레이캐스트하여 Y 좌표 조정
            if (Physics.Raycast(_targetPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                _targetPosition.y = hit.point.y;
            }
            else
            {
                // 레이캐스트 실패 시 현재 Y 좌표 유지
                _targetPosition.y = transform.position.y;
            }

            // 이번 목적지로 갈 때 걷기/뛰기 결정
            _currentMoveSpeed = walkSpeed;
            if (allowRun && Random.value < runChance)
            {
                _currentMoveSpeed = runSpeed;
            }

            _isMoving = true;
        }

        private void MoveTowardsTarget()
        {
            Vector3 direction = (_targetPosition - transform.position);
            direction.y = 0f; // Y축 무시

            float distance = direction.magnitude;

            // 목표 지점에 도달했는지 확인
            if (distance <= arrivalDistance)
            {
                _isMoving = false;
                UpdateAnimation(0f, false);
                return;
            }

            // 반경 제한이 있을 경우, 시작 위치로부터 너무 멀어지지 않도록 체크
            if (useRadiusLimit)
            {
                Vector3 toCurrent = transform.position - _startPos;
                toCurrent.y = 0f;
                if (toCurrent.magnitude > wanderRadius)
                {
                    // 반경을 벗어났으면 시작 위치 방향으로 이동
                    Vector3 backToCenter = (_startPos - transform.position);
                    backToCenter.y = 0f;
                    direction = backToCenter.normalized;
                }
            }

            // 벽 충돌 감지 (추가)
            direction.Normalize();
            Vector3 rayStart = transform.position;
            rayStart.y += 0.5f; // 동물의 중심 높이에서 레이캐스트

            // 이동 방향으로 레이캐스트하여 벽 감지
            if (Physics.Raycast(rayStart, direction, out RaycastHit hit, wallCheckDistance, wallLayer))
            {
                // 벽이 감지되면 새로운 목표 지점 설정
                SetNewTarget();
                return; // 이번 프레임은 이동하지 않음
            }

            // 이동
            Vector3 newPosition = transform.position + direction * _currentMoveSpeed * Time.deltaTime;

            // 반경 제한이 있을 경우, 새 위치가 반경 내인지 확인
            if (useRadiusLimit)
            {
                Vector3 toNewPos = newPosition - _startPos;
                toNewPos.y = 0f;
                if (toNewPos.magnitude > wanderRadius)
                {
                    // 반경을 벗어나면 반경 경계로 제한
                    newPosition = _startPos + toNewPos.normalized * wanderRadius;
                    newPosition.y = transform.position.y; // Y는 유지
                }
            }

            // 새 위치에서도 벽 충돌 확인 (이중 체크)
            Vector3 checkStart = newPosition;
            checkStart.y += 0.5f;
            if (Physics.Raycast(checkStart, direction, out RaycastHit hit2, 0.5f, wallLayer))
            {
                // 새 위치에도 벽이 있으면 이동하지 않고 새 목표 설정
                SetNewTarget();
                return;
            }

            // Rigidbody가 있고 kinematic이 아니면 velocity만 사용, 아니면 transform.position 직접 설정
            if (_rigidbody != null && !_rigidbody.isKinematic)
            {
                // direction 정규화 확인
                if (direction.magnitude > 0.01f)
                {
                    direction.Normalize();
                }
                
                // 수평 이동만 velocity로 제어 (Y축은 중력이 처리)
                Vector3 horizontalVelocity = direction * _currentMoveSpeed;
                
                // Y축 velocity는 중력에 맡기되, 위로 너무 빠르게 올라가면 제한
                float currentYVelocity = _rigidbody.velocity.y;
                if (currentYVelocity > 2f) // 위로 너무 빠르게 올라가면 제한
                {
                    currentYVelocity = 2f;
                }
                
                // velocity 설정 (수평 이동 + 제한된 Y축 velocity)
                _rigidbody.velocity = new Vector3(horizontalVelocity.x, currentYVelocity, horizontalVelocity.z);
                
                // 바닥 감지하여 Y축 위치와 velocity 조정
                if (Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit groundHit, 2f))
                {
                    float groundY = groundHit.point.y;
                    float currentY = transform.position.y;
                    float distanceToGround = currentY - groundY;
                    
                    // Collider의 하단 높이 계산
                    float colliderBottomOffset = 0f;
                    CapsuleCollider capsuleCol = GetComponent<CapsuleCollider>();
                    if (capsuleCol != null)
                    {
                        colliderBottomOffset = capsuleCol.center.y - (capsuleCol.height / 2f);
                    }
                    else
                    {
                        Collider col = GetComponent<Collider>();
                        if (col != null)
                        {
                            colliderBottomOffset = -col.bounds.extents.y;
                        }
                    }
                    
                    // 목표 Y 위치: 바닥 + Collider 하단 오프셋
                    float targetY = groundY - colliderBottomOffset;
                    
                    // 바닥에 가까우면 (0.3 유닛 이내) 위치 강제 조정
                    if (distanceToGround > 0.3f)
                    {
                        // 떠있으면 강제로 내림
                        Vector3 pos = transform.position;
                        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 5f); // 부드럽게 내림
                        transform.position = pos;
                        _rigidbody.velocity = new Vector3(horizontalVelocity.x, -1f, horizontalVelocity.z);
                    }
                    // 바닥에 매우 가까우면 (0.15 유닛 이내) Y축 velocity를 0으로
                    else if (distanceToGround < 0.15f && distanceToGround > -0.05f)
                    {
                        _rigidbody.velocity = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
                        // 미세 조정으로 바닥에 정확히 붙임
                        if (Mathf.Abs(distanceToGround) > 0.05f)
                        {
                            Vector3 pos = transform.position;
                            pos.y = targetY;
                            transform.position = pos;
                        }
                    }
                    // 바닥 위에 있지만 떨어지고 있으면 속도 조정
                    else if (distanceToGround > 0.15f && currentYVelocity < 0)
                    {
                        // 떨어지는 속도를 약간 줄임
                        _rigidbody.velocity = new Vector3(horizontalVelocity.x, currentYVelocity * 0.7f, horizontalVelocity.z);
                    }
                }
                else
                {
                    // 바닥을 감지하지 못하면 중력에 맡김
                    _rigidbody.velocity = new Vector3(horizontalVelocity.x, _rigidbody.velocity.y, horizontalVelocity.z);
                }
            }
            else
            {
                transform.position = newPosition;
            }

            // 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // 애니메이션 업데이트
            UpdateAnimation(_currentMoveSpeed, true);
        }

        /// <summary>
        /// Animator에 특정 파라미터가 존재하는지 확인합니다.
        /// </summary>
        private bool HasParameter(string paramName, AnimatorControllerParameterType type)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return false;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.name == paramName && param.type == type)
                    return true;
            }
            return false;
        }

        private void UpdateAnimation(float speed, bool isMoving)
        {
            if (_animator == null) return;

            // Animator Controller가 할당되어 있는지 확인
            if (_animator.runtimeAnimatorController == null)
            {
                return; // Controller가 없으면 애니메이션 업데이트 안함
            }

            // Speed 파라미터 설정 (float)
            if (!string.IsNullOrEmpty(speedParameterName) && HasParameter(speedParameterName, AnimatorControllerParameterType.Float))
            {
                _animator.SetFloat(speedParameterName, isMoving ? speed : 0f);
            }

            // Vert 파라미터 설정 (float, CreatureMover 호환)
            if (!string.IsNullOrEmpty(vertParameterName) && HasParameter(vertParameterName, AnimatorControllerParameterType.Float))
            {
                _animator.SetFloat(vertParameterName, isMoving ? 1f : 0f);
            }

            // State 파라미터 설정 (float, CreatureMover 호환)
            if (!string.IsNullOrEmpty(stateParameterName) && HasParameter(stateParameterName, AnimatorControllerParameterType.Float))
            {
                float state = idleStateValue;
                if (isMoving)
                {
                    // 속도에 따라 walk/run 결정
                    state = (speed >= runSpeed - 0.01f) ? runStateValue : walkStateValue;
                }
                _animator.SetFloat(stateParameterName, state);
            }

            // IsMoving 파라미터 설정 (Bool, 선택적)
            if (!string.IsNullOrEmpty(isMovingParameterName) && HasParameter(isMovingParameterName, AnimatorControllerParameterType.Bool))
            {
                _animator.SetBool(isMovingParameterName, isMoving);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 시작 위치 시각화
            Vector3 center = startPosition != null ? startPosition.position : _startPos;
            if (center == Vector3.zero)
            {
                center = transform.position;
            }

            if (useRadiusLimit)
            {
                // 반경 제한 범위 시각화 (파란색 원)
                Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // 반투명 청록색
                Gizmos.DrawWireSphere(center, wanderRadius);

                // 시작 위치 표시
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(center, 0.5f);
            }
            else
            {
                // 반경 제한 없음 표시 (현재 위치 중심)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 1f);
            }

            // 목표 지점 시각화
            if (_targetPosition != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_targetPosition, 0.5f);
                Gizmos.DrawLine(transform.position, _targetPosition);
            }
        }

        /// <summary>
        /// 동물을 달아나게 합니다 (고양이 등 위험 요소에 대한 반응).
        /// 반경 제한을 해제하고 속도를 증가시킵니다.
        /// </summary>
        /// <param name="speedMultiplier">속도 배율 (기본 속도에 곱해짐)</param>
        /// <param name="removeRadiusLimit">반경 제한을 해제할지 여부</param>
        public void Escape(float speedMultiplier = 2f, bool removeRadiusLimit = true)
        {
            if (_isEscaping)
            {
                return;
            }

            _isEscaping = true;
            
            // 반경 제한 해제
            if (removeRadiusLimit)
            {
                useRadiusLimit = false;
            }

            // 속도 증가
            walkSpeed = _originalWalkSpeed * speedMultiplier;
            runSpeed = _originalRunSpeed * speedMultiplier;
            runChance = 1f; // 항상 뛰도록 설정

        }

        /// <summary>
        /// 달아나기 상태를 해제하고 원래 상태로 복원합니다.
        /// </summary>
        public void StopEscaping()
        {
            if (!_isEscaping)
                return;

            _isEscaping = false;
            useRadiusLimit = _originalUseRadiusLimit;
            walkSpeed = _originalWalkSpeed;
            runSpeed = _originalRunSpeed;
            runChance = 0.2f; // 원래 확률로 복원

        }
    }
}