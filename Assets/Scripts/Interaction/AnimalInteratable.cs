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
        [Header("Movement Settings")]
        [Tooltip("이동 속도")]
        [SerializeField] private float moveSpeed = 2f;
        
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

        private Animator _animator;
        private Vector3 _targetPosition;
        private Vector3 _startPos; // 시작 위치 저장
        private float _wanderTimer = 0f;
        private bool _isMoving = false;
        private float _currentMoveSpeed; // 현재 이동 속도 (walk/run)

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            // 시작 위치 설정
            if (startPosition != null)
            {
                _startPos = startPosition.position;
            }
            else
            {
                _startPos = transform.position;
            }
            
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
            
            // 이동
            direction.Normalize();
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
            
            transform.position = newPosition;
            
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

        private void UpdateAnimation(float speed, bool isMoving)
        {
            if (_animator == null) return;
            
            // Animator Controller가 할당되어 있는지 확인
            if (_animator.runtimeAnimatorController == null)
            {
                return; // Controller가 없으면 애니메이션 업데이트 안함
            }
            
            // Speed 파라미터 설정 (float)
            if (!string.IsNullOrEmpty(speedParameterName))
            {
                try
                {
                    _animator.SetFloat(speedParameterName, isMoving ? speed : 0f);
                }
                catch (System.Exception)
                {
                    // 파라미터가 없으면 무시
                }
            }
            
            // Vert 파라미터 설정 (float, CreatureMover 호환)
            if (!string.IsNullOrEmpty(vertParameterName))
            {
                try
                {
                    _animator.SetFloat(vertParameterName, isMoving ? 1f : 0f);
                }
                catch (System.Exception)
                {
                    // 파라미터가 없으면 무시
                }
            }
            
            // State 파라미터 설정 (float, CreatureMover 호환)
            if (!string.IsNullOrEmpty(stateParameterName))
            {
                try
                {
                    float state = idleStateValue;
                    if (isMoving)
                    {
                        // 속도에 따라 walk/run 결정
                        state = (speed >= runSpeed - 0.01f) ? runStateValue : walkStateValue;
                    }
                    _animator.SetFloat(stateParameterName, state);
                }
                catch (System.Exception)
                {
                    // 파라미터가 없으면 무시
                }
            }
            
            // IsMoving 파라미터 설정 (Bool, 선택적)
            if (!string.IsNullOrEmpty(isMovingParameterName))
            {
                try
                {
                    _animator.SetBool(isMovingParameterName, isMoving);
                }
                catch (System.Exception)
                {
                    // 파라미터가 없으면 무시
                }
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
    }
}