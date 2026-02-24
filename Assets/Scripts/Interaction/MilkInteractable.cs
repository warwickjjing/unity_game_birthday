using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.Props;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 냉장고에서 우유를 가져갈 수 있는 상호작용 오브젝트입니다.
    /// 냉장고 문이 열려있을 때만 프롬프트가 표시됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MilkInteractable : MonoBehaviour, IInteractable
    {
        [Header("Refrigerator Door")]
        [Tooltip("냉장고 문 (Door 컴포넌트가 있는 오브젝트, 비어있으면 자동으로 찾음)")]
        [SerializeField] private Door refrigeratorDoor;

        [Tooltip("냉장고 문 오브젝트 (refrigeratorDoor가 없을 때 사용)")]
        [SerializeField] private GameObject refrigeratorDoorObject;

        [Header("Milk Settings")]
        [Tooltip("우유를 들고 있을 때의 위치 (플레이어의 자식 오브젝트)")]
        [SerializeField] private Transform carryPosition;

        [Tooltip("우유를 들고 있을 때 플레이어 기준 위치 (carryPosition이 없을 때 사용)")]
        [SerializeField] private Vector3 carryOffset = new Vector3(0.3f, 1.2f, 0.3f);

        [Tooltip("우유를 들고 있는지 여부")]
        [SerializeField] private bool isBeingCarried = false;

        private Transform _playerTransform;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _originalParent;
        private Collider _milkCollider;
        private Rigidbody _rigidbody;

        public bool CanInteract => !isBeingCarried && IsRefrigeratorOpen();

        public string GetInteractPrompt()
        {
            if (isBeingCarried)
            {
                return "우유 내려놓기 [F]";
            }

            if (!IsRefrigeratorOpen())
            {
                return "냉장고 문을 열어주세요 [F]";
            }

            return "우유 가져가기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            _milkCollider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            // 냉장고 문 자동 찾기
            if (refrigeratorDoor == null)
            {
                if (refrigeratorDoorObject != null)
                {
                    refrigeratorDoor = refrigeratorDoorObject.GetComponent<Door>();
                }
                else
                {
                    // 주변에서 Door 찾기
                    Door[] doors = FindObjectsOfType<Door>();
                    foreach (Door door in doors)
                    {
                        float distance = Vector3.Distance(transform.position, door.transform.position);
                        if (distance < 5f) // 5미터 이내
                        {
                            refrigeratorDoor = door;
                            break;
                        }
                    }
                }
            }

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[MilkInteractable] WorldSpaceInteractionPrompt 자동 추가됨");
            }
        }

        private void Start()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void Update()
        {
            // 우유를 들고 있을 때 플레이어를 따라가기
            if (isBeingCarried && _playerTransform != null)
            {
                if (carryPosition != null)
                {
                    transform.position = carryPosition.position;
                    transform.rotation = carryPosition.rotation;
                }
                else
                {
                    transform.position = _playerTransform.position + _playerTransform.TransformDirection(carryOffset);
                    transform.rotation = _playerTransform.rotation;
                }
            }
        }

        /// <summary>
        /// 냉장고 문이 열려있는지 확인합니다.
        /// </summary>
        private bool IsRefrigeratorOpen()
        {
            if (refrigeratorDoor == null)
                return true; // 문이 없으면 항상 열려있다고 가정

            return refrigeratorDoor.IsOpen;
        }

        public void Interact(GameObject interactor)
        {
            if (isBeingCarried)
            {
                DropMilk();
            }
            else
            {
                if (!IsRefrigeratorOpen())
                {
                    Debug.Log("[MilkInteractable] 냉장고 문을 먼저 열어주세요!");
                    return;
                }

                PickUpMilk(interactor);
            }
        }

        /// <summary>
        /// 우유를 집습니다.
        /// </summary>
        private void PickUpMilk(GameObject interactor)
        {
            if (isBeingCarried)
                return;

            _playerTransform = interactor.transform;
            isBeingCarried = true;

            // 원래 상태 저장
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _originalParent = transform.parent;

            // 플레이어의 자식으로 설정
            transform.SetParent(_playerTransform);

            // Collider 비활성화
            if (_milkCollider != null)
            {
                _milkCollider.enabled = false;
            }

            // Rigidbody 비활성화
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }

            Debug.Log("[MilkInteractable] 우유를 들었습니다!");
        }

        /// <summary>
        /// 우유를 내려놓습니다.
        /// </summary>
        private void DropMilk()
        {
            if (!isBeingCarried)
                return;

            isBeingCarried = false;

            // 원래 부모로 복원
            transform.SetParent(_originalParent);

            // 위치 조정 (플레이어 앞에 내려놓기)
            if (_playerTransform != null)
            {
                Vector3 dropPosition = _playerTransform.position + _playerTransform.forward * 1f;
                
                // 바닥에 레이캐스트하여 Y 위치 조정
                RaycastHit hit;
                if (Physics.Raycast(dropPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
                {
                    dropPosition.y = hit.point.y + 0.1f;
                }

                transform.position = dropPosition;
            }
            else
            {
                transform.position = _originalPosition;
            }

            transform.rotation = _originalRotation;

            // Collider 활성화
            if (_milkCollider != null)
            {
                _milkCollider.enabled = true;
            }

            // Rigidbody 활성화
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
            }

            _playerTransform = null;

            Debug.Log("[MilkInteractable] 우유를 내려놓았습니다!");
        }

        /// <summary>
        /// 우유를 소에게 주었는지 확인합니다 (CowInteractable에서 호출).
        /// </summary>
        public bool IsCarried()
        {
            return isBeingCarried;
        }

        /// <summary>
        /// 우유를 소에게 주었을 때 호출됩니다 (CowInteractable에서 호출).
        /// </summary>
        public void GiveToCow()
        {
            if (!isBeingCarried)
                return;

            isBeingCarried = false;
            transform.SetParent(_originalParent);
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;

            // 우유 오브젝트 비활성화 (소가 마셨다고 가정)
            gameObject.SetActive(false);

            Debug.Log("[MilkInteractable] 우유를 소에게 주었습니다!");
        }
    }
}

