using UnityEngine;

namespace BirthdayCakeQuest.Camera
{
    /// <summary>
    /// 플레이어를 따라가는 3/4 뷰(Isometric 느낌) 카메라입니다.
    /// 고정된 각도에서 플레이어를 추적합니다.
    /// </summary>
    public sealed class IsometricFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("따라갈 대상 (플레이어)")]
        [SerializeField] private Transform target;

        [Header("Camera Settings")]
        [Tooltip("카메라와 플레이어 사이의 오프셋")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -10f);
        
        [Tooltip("카메라 회전 각도 (X축) - 3/4뷰는 45도")]
        [SerializeField] private float angleX = 45f;
        
        [Tooltip("카메라 Y축 회전 (3/4뷰는 0도 또는 45도)")]
        [SerializeField] private float angleY = 0f;

        [Header("Follow Settings")]
        [Tooltip("카메라 이동 속도 (높을수록 즉시 반응)")]
        [SerializeField] private float followSpeed = 5f;
        
        [Tooltip("부드러운 이동 사용 여부")]
        [SerializeField] private bool useSmoothFollow = true;

        [Header("Constraints")]
        [Tooltip("카메라 이동 범위 제한 (선택)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector3 boundsMin = new Vector3(-50f, 0f, -50f);
        [SerializeField] private Vector3 boundsMax = new Vector3(50f, 20f, 50f);

        private Vector3 _currentVelocity;
        private bool _isPaused = false;

        private void LateUpdate()
        {
            if (target == null || _isPaused)
                return;

            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        /// <summary>
        /// 카메라를 일시정지/재개합니다.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        private void UpdateCameraPosition()
        {
            Vector3 targetPosition = target.position + offset;

            // 범위 제한 적용
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, boundsMin.x, boundsMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, boundsMin.y, boundsMax.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, boundsMin.z, boundsMax.z);
            }

            if (useSmoothFollow)
            {
                // 부드러운 이동
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref _currentVelocity,
                    1f / followSpeed
                );
            }
            else
            {
                // 즉시 이동
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    followSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateCameraRotation()
        {
            // 3/4 쿼터뷰를 위한 고정 각도
            // X축 45도 = 위에서 내려다보는 각도
            // Y축 0도 = 정면, 45도 = 대각선 뷰
            Quaternion targetRotation = Quaternion.Euler(angleX, angleY, 0f);
            transform.rotation = targetRotation;
            
            // 회전 잠금 보장 (미니게임 등에서도 유지)
            transform.eulerAngles = new Vector3(angleX, angleY, 0f);
        }

        /// <summary>
        /// 타겟을 변경합니다.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// 오프셋을 변경합니다.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// 카메라를 즉시 타겟 위치로 이동합니다.
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
                return;

            transform.position = target.position + offset;
            _currentVelocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            // 타겟 연결선 표시
            if (target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, target.position);
            }

            // 범위 제한 박스 표시
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = (boundsMin + boundsMax) * 0.5f;
                Vector3 size = boundsMax - boundsMin;
                Gizmos.DrawWireCube(center, size);
            }

            // 오프셋 시각화
            if (target != null)
            {
                Gizmos.color = Color.green;
                Vector3 targetPos = target.position + offset;
                Gizmos.DrawWireSphere(targetPos, 0.5f);
            }
        }
    }
}

