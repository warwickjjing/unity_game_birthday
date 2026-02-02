using UnityEngine;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.Props
{
    /// <summary>
    /// 플레이어가 상호작용할 수 있는 문입니다.
    /// E키로 열고 닫을 수 있습니다.
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [Tooltip("문이 열리는 방향 (각도)")]
        [SerializeField] private float openAngle = 90f;

        [Tooltip("문이 닫혀있을 때 각도")]
        [SerializeField] private float closedAngle = 0f;

        [Tooltip("문이 열리는 속도")]
        [SerializeField] private float openSpeed = 3f;

        [Tooltip("문이 자동으로 닫힐지 여부")]
        [SerializeField] private bool autoClose = false;

        [Tooltip("자동으로 닫히기까지 대기 시간 (초)")]
        [SerializeField] private float autoCloseDelay = 3f;

        [Header("Lock Settings")]
        [Tooltip("문이 잠겨있는지 여부")]
        [SerializeField] private bool isLocked = false;

        [Tooltip("잠겨있을 때 표시할 메시지")]
        [SerializeField] private string lockedMessage = "Locked";

        [Header("Visual")]
        [Tooltip("문 오브젝트 (회전할 부분) - DoorPivot")]
        [SerializeField] private Transform doorPivot;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioSource audioSource;

        private bool _isOpen = false;
        private bool _isAnimating = false;
        private float _targetAngle;
        private float _autoCloseTimer;
        private BoxCollider _doorCollider; // 문 충돌 제어용

        public bool CanInteract => !_isAnimating;

        public string GetInteractPrompt()
        {
            if (isLocked)
                return lockedMessage;

            return _isOpen ? "Close Door [E]" : "Open Door [E]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            // DoorPivot 자동 찾기
            if (doorPivot == null)
            {
                doorPivot = transform.Find("DoorPivot");
                if (doorPivot == null)
                {
                    Debug.LogWarning("[Door] DoorPivot을 찾을 수 없습니다. transform을 사용합니다.");
                    doorPivot = transform;
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // BoxCollider 찾기 및 초기 설정
            _doorCollider = GetComponent<BoxCollider>();
            if (_doorCollider == null)
            {
                Debug.LogWarning($"[Door] {name}에 BoxCollider가 없습니다. 자동 추가합니다.");
                _doorCollider = gameObject.AddComponent<BoxCollider>();
                _doorCollider.center = new Vector3(0, 1f, 0);
                _doorCollider.size = new Vector3(1f, 2f, 0.1f);
            }

            // 초기 상태: 문이 닫혀있으므로 물리적으로 막음 (isTrigger = false)
            _doorCollider.isTrigger = false;
            Debug.Log($"[Door] {name} 초기화: 닫힌 상태 (충돌 활성)");

            // 자식 오브젝트의 Collider 제거 (문짝, 손잡이 등)
            // DoorPivot은 제외 (구조적 필요)
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child != transform && child.name != "DoorPivot")
                {
                    Collider childCollider = child.GetComponent<Collider>();
                    if (childCollider != null)
                    {
                        Debug.Log($"[Door] {child.name}의 Collider 제거 (물리 충돌 방지)");
                        Destroy(childCollider);
                    }
                }
            }

            // 초기 각도 설정
            _targetAngle = closedAngle;
            doorPivot.localRotation = Quaternion.Euler(0, closedAngle, 0);
        }

        private void Update()
        {
            // 문 애니메이션
            if (_isAnimating)
            {
                AnimateDoor();
            }

            // 자동 닫기
            if (autoClose && _isOpen && !_isAnimating)
            {
                _autoCloseTimer += Time.deltaTime;
                if (_autoCloseTimer >= autoCloseDelay)
                {
                    CloseDoor();
                }
            }
        }

        public void Interact(GameObject interactor)
        {
            if (isLocked)
            {
                Debug.Log($"[Door] 문이 잠겨있습니다!");
                PlaySound(lockedSound);
                return;
            }

            if (_isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        /// <summary>
        /// 문을 엽니다.
        /// </summary>
        public void OpenDoor()
        {
            if (_isOpen || _isAnimating)
                return;

            Debug.Log("[Door] 문 열기");
            _isOpen = true;
            _isAnimating = true;
            _targetAngle = openAngle;
            _autoCloseTimer = 0f;

            // 문이 열릴 때: Trigger로 변경 (통과 가능)
            if (_doorCollider != null)
            {
                _doorCollider.isTrigger = true;
                Debug.Log("[Door] Collider를 Trigger로 변경 (통과 가능)");
            }

            PlaySound(openSound);
        }

        /// <summary>
        /// 문을 닫습니다.
        /// </summary>
        public void CloseDoor()
        {
            if (!_isOpen || _isAnimating)
                return;

            Debug.Log("[Door] 문 닫기");
            _isOpen = false;
            _isAnimating = true;
            _targetAngle = closedAngle;

            // 문이 닫힐 때: 일반 Collider로 변경 (물리적으로 막음)
            if (_doorCollider != null)
            {
                _doorCollider.isTrigger = false;
                Debug.Log("[Door] Collider를 일반으로 변경 (통과 불가)");
            }

            PlaySound(closeSound);
        }

        /// <summary>
        /// 문의 잠금 상태를 설정합니다.
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            Debug.Log($"[Door] 문이 {(locked ? "잠김" : "열림")} 상태로 변경되었습니다.");
        }

        /// <summary>
        /// 문을 잠금 해제합니다 (열쇠로).
        /// </summary>
        public void Unlock()
        {
            SetLocked(false);
        }

        private void AnimateDoor()
        {
            Quaternion targetRotation = Quaternion.Euler(0, _targetAngle, 0);
            doorPivot.localRotation = Quaternion.Slerp(
                doorPivot.localRotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );

            // 목표 각도에 거의 도달했는지 확인
            if (Quaternion.Angle(doorPivot.localRotation, targetRotation) < 0.5f)
            {
                doorPivot.localRotation = targetRotation;
                _isAnimating = false;
                Debug.Log($"[Door] 문이 {(_isOpen ? "열렸" : "닫혔")}습니다.");
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 문이 열리는 범위 시각화
            Gizmos.color = isLocked ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 2f);

            // 열린 상태 표시
            if (doorPivot != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 openDirection = Quaternion.Euler(0, openAngle, 0) * Vector3.forward;
                Gizmos.DrawRay(transform.position, openDirection * 2f);
            }
        }
    }
}

