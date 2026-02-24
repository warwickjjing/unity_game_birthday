using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.MiniGames;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 포대 배달 지점입니다.
    /// 플레이어가 포대를 들고 F키를 누르면 배달됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FlourDeliveryZone : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float interactionRange = 1.5f;

        [Header("References")]
        [SerializeField] private FlourPlayer2D player;
        [SerializeField] private FlourNPC npc;

        [Header("Bag Stacking")]
        [Tooltip("배달된 포대를 쌓을 프리팹")]
        [SerializeField] private GameObject bagPrefab;

        [Tooltip("포대 쌓기 시작 위치 (비어있으면 이 오브젝트의 위치 사용)")]
        [SerializeField] private Transform stackStartPosition;

        [Tooltip("포대 하나당 높이 오프셋")]
        [SerializeField] private float bagHeightOffset = 0.5f;

        [Tooltip("포대 위치 랜덤 오프셋 (자연스러운 쌓기)")]
        [SerializeField] private float randomOffsetRange = 0.1f;

        [Tooltip("포대 회전 랜덤 범위 (도)")]
        [SerializeField] private float randomRotationRange = 15f;

        [Tooltip("포대 떨어지는 애니메이션 사용")]
        [SerializeField] private bool useDropAnimation = true;

        [Tooltip("포대 떨어지는 속도")]
        [SerializeField] private float dropAnimationSpeed = 5f;

        private int _stackedBagCount = 0;
        private List<GameObject> _stackedBags = new List<GameObject>();

        public event Action OnDelivery;

        public bool CanInteract
        {
            get
            {
                if (player == null)
                    return false;

                // 플레이어가 포대를 들고 있어야 함
                if (!player.IsCarryingBag())
                    return false;

                // 거리 확인
                float distance = Vector2.Distance(transform.position, player.transform.position);
                return distance <= interactionRange;
            }
        }

        public string GetInteractPrompt()
        {
            if (CanInteract)
            {
                return "포대 배달하기 [F]";
            }
            return null;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[FlourDeliveryZone] WorldSpaceInteractionPrompt 자동 추가됨");
            }

            // 자동으로 찾기
            if (player == null)
            {
                player = FindObjectOfType<FlourPlayer2D>();
            }

            if (npc == null)
            {
                npc = FindObjectOfType<FlourNPC>();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            // 플레이어가 포대를 들고 있는지 확인
            FlourBag bag = player.GetCarriedBag();
            if (bag == null)
            {
                Debug.LogWarning("[FlourDeliveryZone] 포대를 찾을 수 없습니다!");
                return;
            }

            // 포대 제거 (플레이어가 들고 있는 것)
            player.DropBag();
            Destroy(bag.gameObject);

            // 포대 프리팹 인스턴스화하여 쌓기
            if (bagPrefab != null)
            {
                Vector3 stackPosition = GetStackPosition();
                
                if (useDropAnimation)
                {
                    // 애니메이션으로 포대 떨어뜨리기
                    StartCoroutine(DropBagAnimation(stackPosition));
                }
                else
                {
                    // 즉시 생성
                    CreateStackedBag(stackPosition);
                }
            }

            // 배달 이벤트 발생
            OnDelivery?.Invoke();

            // NPC에 알림
            if (npc != null)
            {
                npc.OnBagDelivered();
            }

            Debug.Log($"[FlourDeliveryZone] 포대 배달 완료! (쌓인 포대: {_stackedBagCount}개)");
        }

        /// <summary>
        /// 포대를 쌓을 위치를 계산합니다.
        /// </summary>
        private Vector3 GetStackPosition()
        {
            Vector3 basePosition;
            
            if (stackStartPosition != null)
            {
                basePosition = stackStartPosition.position;
            }
            else
            {
                basePosition = transform.position;
            }

            // 높이 계산
            float height = _stackedBagCount * bagHeightOffset;
            
            // 랜덤 오프셋 추가 (자연스러운 쌓기)
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-randomOffsetRange, randomOffsetRange),
                0,
                UnityEngine.Random.Range(-randomOffsetRange, randomOffsetRange)
            );

            return basePosition + Vector3.up * height + randomOffset;
        }

        /// <summary>
        /// 쌓인 포대를 생성합니다.
        /// </summary>
        private GameObject CreateStackedBag(Vector3 position)
        {
            // 랜덤 회전 추가
            Quaternion randomRotation = Quaternion.Euler(
                0,
                UnityEngine.Random.Range(-randomRotationRange, randomRotationRange),
                UnityEngine.Random.Range(-randomRotationRange, randomRotationRange)
            );

            GameObject stackedBag = Instantiate(bagPrefab, position, randomRotation);
            
            // FlourBag 컴포넌트 비활성화 (쌓인 포대는 상호작용 불가)
            var flourBag = stackedBag.GetComponent<FlourBag>();
            if (flourBag != null)
            {
                flourBag.enabled = false;
            }

            // Collider 비활성화 (물리 충돌 방지)
            var collider = stackedBag.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // SpriteRenderer의 Sorting Order 설정 (쌓일수록 더 위에 보이게)
            var spriteRenderer = stackedBag.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _stackedBagCount + 10; // 기본값보다 높게 설정
            }

            _stackedBags.Add(stackedBag);
            _stackedBagCount++;

            return stackedBag;
        }

        /// <summary>
        /// 포대가 떨어지는 애니메이션을 재생합니다.
        /// </summary>
        private IEnumerator DropBagAnimation(Vector3 targetPosition)
        {
            // 시작 위치 (위에서)
            Vector3 startPosition = targetPosition + Vector3.up * 2f;
            GameObject stackedBag = Instantiate(bagPrefab, startPosition, Quaternion.identity);

            // FlourBag 컴포넌트 비활성화
            var flourBag = stackedBag.GetComponent<FlourBag>();
            if (flourBag != null)
            {
                flourBag.enabled = false;
            }

            // Collider 비활성화
            var collider = stackedBag.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // SpriteRenderer의 Sorting Order 설정 (쌓일수록 더 위에 보이게)
            var spriteRenderer = stackedBag.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _stackedBagCount + 10; // 기본값보다 높게 설정
            }

            // 랜덤 회전 추가
            Quaternion randomRotation = Quaternion.Euler(
                0,
                UnityEngine.Random.Range(-randomRotationRange, randomRotationRange),
                UnityEngine.Random.Range(-randomRotationRange, randomRotationRange)
            );
            stackedBag.transform.rotation = randomRotation;

            // 부드럽게 떨어뜨리기
            float elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * dropAnimationSpeed;
                stackedBag.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
                yield return null;
            }

            // 최종 위치 설정
            stackedBag.transform.position = targetPosition;

            _stackedBags.Add(stackedBag);
            _stackedBagCount++;
        }

        /// <summary>
        /// 쌓인 포대들을 모두 제거합니다 (게임 재시작 등에 사용).
        /// </summary>
        public void ClearStack()
        {
            foreach (var bag in _stackedBags)
            {
                if (bag != null)
                {
                    Destroy(bag);
                }
            }
            _stackedBags.Clear();
            _stackedBagCount = 0;
            Debug.Log("[FlourDeliveryZone] 쌓인 포대 모두 제거됨");
        }
    }
}

