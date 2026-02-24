using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 포대 오브젝트입니다.
    /// 플레이어가 집고 운반할 수 있습니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FlourBag : MonoBehaviour, IInteractable
    {
        [Header("Bag Settings")]
        [SerializeField] private bool isPickedUp = false;
        [SerializeField] private bool canBePickedUp = true;

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Collider2D _collider;
        private FlourPlayer2D _player;
        private FlourNPC _npc;

        public bool CanInteract 
        { 
            get 
            {
                // 이미 집혔으면 상호작용 불가
                if (isPickedUp || !canBePickedUp)
                    return false;
                    
                // NPC가 있고 퀘스트가 시작되지 않았으면 상호작용 불가
                if (_npc != null && !_npc.IsQuestStarted())
                    return false;
                    
                return true;
            }
        }

        public string GetInteractPrompt()
        {
            if (isPickedUp)
                return null;
                
            // NPC와 대화하지 않았으면 프롬프트 숨김
            if (_npc != null && !_npc.IsQuestStarted())
                return null;

            return "밀가루 포대 집기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider != null)
            {
                _collider.isTrigger = false; // 물리 충돌 필요
            }

            // NPC 찾기
            _npc = FindObjectOfType<FlourNPC>();

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[FlourBag] WorldSpaceInteractionPrompt 자동 추가됨");
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            // 플레이어 찾기
            _player = interactor.GetComponent<FlourPlayer2D>();
            if (_player == null)
            {
                Debug.LogWarning("[FlourBag] FlourPlayer2D를 찾을 수 없습니다!");
                return;
            }

            // 플레이어가 이미 포대를 들고 있으면 무시
            if (_player.IsCarryingBag())
            {
                Debug.Log("[FlourBag] 플레이어가 이미 포대를 들고 있습니다!");
                return;
            }

            // 포대 집기 (플레이어가 PickUpBag에서 처리)
            _player.PickUpBag(this);
        }

        /// <summary>
        /// 포대를 집습니다.
        /// </summary>
        public void PickUp(Transform parentTransform)
        {
            if (isPickedUp)
                return;

            _originalParent = transform.parent;
            _originalPosition = transform.position;
            _originalScale = transform.localScale; // 원래 스케일 저장

            // 부모 설정
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Collider 비활성화
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            // 포대 오브젝트 자체는 숨김 (Sprite로 대체)
            gameObject.SetActive(false);

            isPickedUp = true;

            Debug.Log("[FlourBag] 포대가 집혔습니다. (Sprite로 표시됨)");
        }

        /// <summary>
        /// 포대를 놓습니다.
        /// </summary>
        public void Drop(Vector3 position)
        {
            if (!isPickedUp)
                return;

            // 부모 복원
            transform.SetParent(_originalParent);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = _originalScale; // 원래 스케일 복원

            // Collider 활성화
            if (_collider != null)
            {
                _collider.enabled = true;
            }

            // SpriteRenderer의 Sorting Order 복원
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 0; // 기본값으로 복원
            }

            isPickedUp = false;
            _player = null;

            Debug.Log("[FlourBag] 포대가 놓였습니다.");
        }

        /// <summary>
        /// 포대가 집혔는지 확인합니다.
        /// </summary>
        public bool IsPickedUp()
        {
            return isPickedUp;
        }
    }
}

