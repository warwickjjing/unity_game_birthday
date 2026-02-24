using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.Animals;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Managers;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 고양이를 들고 다니고 닭장에 넣을 수 있는 상호작용 오브젝트입니다.
    /// 달걀 퀘스트일 때만 상호작용 가능합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CatInteractable : MonoBehaviour, IInteractable
    {
        [Header("Cat Settings")]
        [Tooltip("고양이를 들고 있는지 여부")]
        [SerializeField] private bool isBeingCarried = false;

        [Tooltip("고양이를 들고 있을 때의 위치 (플레이어의 자식 오브젝트, 비어있으면 자동으로 플레이어 자식으로 설정)")]
        [SerializeField] private Transform carryPosition;

        [Header("Coop Interaction")]
        [Tooltip("닭장 매니저 (고양이를 넣으면 닭들이 달아남, 비어있으면 자동으로 찾음)")]
        [SerializeField] private ChickenCoopManager coopManager;

        [Tooltip("닭장에 넣을 수 있는 거리")]
        [SerializeField] private float coopInteractionDistance = 3f;

        [Tooltip("고양이를 들고 있을 때 플레이어 기준 위치 (carryPosition이 없을 때 사용)")]
        [SerializeField] private Vector3 carryOffset = new Vector3(0.3f, 1.2f, 0.3f);

        private Transform _playerTransform;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _originalParent;
        private Collider _catCollider;
        private Rigidbody _rigidbody;
        
        private AnimalWander _animalWander;
        private QuestSequenceManager _questManager;

        /// <summary>
        /// 달걀 퀘스트일 때만 상호작용 가능합니다.
        /// </summary>
        public bool CanInteract
        {
            get
            {
                // 달걀 퀘스트가 활성화되어 있는지 확인
                if (_questManager == null)
                {
                    _questManager = QuestSequenceManager.Instance;
                }

                if (_questManager != null)
                {
                    // 현재 활성화된 퀘스트가 달걀인지 확인
                    return _questManager.CurrentActiveIngredient == IngredientId.Egg;
                }

                return false;
            }
        }

        public string GetInteractPrompt()
        {
            // 달걀 퀘스트가 아닐 때는 프롬프트를 표시하지 않음
            if (!CanInteract)
            {
                return null;
            }

            if (isBeingCarried)
            {
                // 닭장 근처인지 확인
                if (IsNearCoop())
                {
                    return "닭장에 넣기 [F]";
                }
                return "고양이 내려놓기 [F]";
            }
            return "안기 [F]";
        }

        public void Interact(GameObject interactor)
        {
            // 달걀 퀘스트가 아닐 때는 상호작용 불가
            if (!CanInteract)
            {
                return;
            }

            if (isBeingCarried)
            {
                // 닭장 근처인지 확인
                if (IsNearCoop())
                {
                    // 닭장에 넣기
                    TryPlaceInCoop(interactor);
                }
                else
                {
                    // 닭장이 멀면 그냥 내려놓기
                    DropCat();
                }
            }
            else
            {
                // 고양이 들기
                PickUpCat(interactor);
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            _catCollider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _animalWander = GetComponent<AnimalWander>();
        }

        private void Start()
        {
            // 닭장 매니저 자동 찾기
            if (coopManager == null)
            {
                coopManager = ChickenCoopManager.Instance;
            }

            // QuestSequenceManager 찾기
            if (_questManager == null)
            {
                _questManager = QuestSequenceManager.Instance;
            }

            // WorldSpaceInteractionPrompt가 없으면 자동으로 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[CatInteractable] WorldSpaceInteractionPrompt 자동 추가됨");
            }
        }

        private void PickUpCat(GameObject player)
        {
            _playerTransform = player.transform;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _originalParent = transform.parent;

            // 플레이어의 자식으로 설정
            if (carryPosition != null)
            {
                transform.SetParent(carryPosition);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                // carryPosition이 없으면 플레이어의 자식으로 직접 설정
                transform.SetParent(_playerTransform);
                transform.localPosition = carryOffset;
                transform.localRotation = Quaternion.identity;
            }

            isBeingCarried = true;
            
            // Collider 비활성화 (플레이어와 충돌 방지)
            if (_catCollider != null)
            {
                _catCollider.enabled = false;
            }

            // Rigidbody 비활성화 (물리 시뮬레이션 중지)
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }

            if (_animalWander != null) {
                _animalWander.enabled = false;
            }

            Debug.Log("[CatInteractable] 고양이를 들었습니다.");
        }

        private void TryPlaceInCoop(GameObject player)
        {
            // 닭장 매니저 찾기
            if (coopManager == null)
            {
                coopManager = ChickenCoopManager.Instance;
            }

            if (coopManager == null)
            {
                Debug.LogWarning("[CatInteractable] ChickenCoopManager를 찾을 수 없습니다!");
                // 고양이를 내려놓기만 함
                DropCat();
                return;
            }

            // 닭장과의 거리 확인
            float distance = Vector3.Distance(_playerTransform.position, coopManager.CoopPosition);
            if (distance > coopInteractionDistance)
            {
                Debug.Log($"[CatInteractable] 닭장이 너무 멉니다. (거리: {distance}, 필요 거리: {coopInteractionDistance})");
                // 고양이를 내려놓기만 함
                DropCat();
                return;
            }

            // 고양이를 닭장에 넣기
            transform.SetParent(_originalParent);
            
            // 닭장 위치로 이동
            Vector3 coopPosition = coopManager.CoopPosition;
            transform.position = coopPosition;
            transform.rotation = _originalRotation;

            // Collider 다시 활성화
            if (_catCollider != null)
            {
                _catCollider.enabled = true;
            }

            // Rigidbody 다시 활성화
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }

            // AnimalWander 다시 활성화 (선택사항 - 닭장에 넣은 후에도 움직이게 하려면)
            // if (_animalWander != null)
            // {
            //     _animalWander.enabled = true;
            // }

            isBeingCarried = false;
            _playerTransform = null;

            // 닭들이 달아나게 함
            coopManager.TriggerChickenEscape();

            Debug.Log("[CatInteractable] 고양이를 닭장에 넣었습니다! 닭들이 달아나기 시작합니다.");
        }

        /// <summary>
        /// 고양이를 내려놓습니다 (어디서든 가능).
        /// </summary>
        private void DropCat()
        {
            // 플레이어 앞에 내려놓기 (원래 위치가 아닌 현재 플레이어 앞)
            Vector3 dropPosition = _playerTransform.position + _playerTransform.forward * 1f;
            
            // 바닥에 레이캐스트하여 Y 좌표 조정
            if (Physics.Raycast(dropPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
            {
                dropPosition.y = hit.point.y;
            }
            else
            {
                dropPosition.y = _originalPosition.y;
            }

            transform.SetParent(_originalParent);
            transform.position = dropPosition;
            transform.rotation = _originalRotation;

            // Collider 다시 활성화
            if (_catCollider != null)
            {
                _catCollider.enabled = true;
            }

            // Rigidbody 다시 활성화 (물리 시뮬레이션 사용)
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
                _rigidbody.freezeRotation = true; // 회전은 AnimalWander에서 제어
            }

            // AnimalWander 다시 활성화
            if (_animalWander != null)
            {
                _animalWander.enabled = true;
            }

            isBeingCarried = false;
            _playerTransform = null;

            Debug.Log("[CatInteractable] 고양이를 내려놓았습니다.");
        }

        /// <summary>
        /// 닭장 근처에 있는지 확인합니다.
        /// </summary>
        private bool IsNearCoop()
        {
            if (!isBeingCarried || _playerTransform == null)
                return false;

            if (coopManager == null)
            {
                coopManager = ChickenCoopManager.Instance;
            }

            if (coopManager == null)
                return false;

            float distance = Vector3.Distance(_playerTransform.position, coopManager.CoopPosition);
            return distance <= coopInteractionDistance;
        }

        private void OnDrawGizmosSelected()
        {
            // 닭장 상호작용 범위 시각화
            if (coopManager != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(coopManager.CoopPosition, coopInteractionDistance);
            }
        }
    }
}

