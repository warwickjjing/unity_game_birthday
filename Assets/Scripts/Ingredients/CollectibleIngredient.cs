using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.MiniGames;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 월드에 배치되어 플레이어가 수집할 수 있는 재료 오브젝트입니다.
    /// Interactor가 범위 내에서 F키를 누르면 수집됩니다.
    /// 미니게임이 설정된 경우 미니게임을 먼저 플레이해야 합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CollectibleIngredient : MonoBehaviour, IInteractable
    {
        [Header("Ingredient Info")]
        [SerializeField] private IngredientId ingredientId;

        [Header("Mini Game")]
        [Tooltip("이 재료를 수집하기 위해 미니게임이 필요한가요?")]
        [SerializeField] private bool requiresMiniGame = false;

        [Tooltip("재생할 미니게임 타입")]
        [SerializeField] private MiniGameType miniGameType = MiniGameType.Sugar;

        [Header("Interaction")]
        [Tooltip("플레이어가 이 거리 안에 있을 때 수집 가능")]
        [SerializeField] private float interactionRadius = 1.5f;

        [Header("Conditional Collection")]
        [Tooltip("닭들이 모두 달아난 후에만 수집 가능한가요?")]
        [SerializeField] private bool requiresChickensEscaped = false;

        [Tooltip("조건이 만족되지 않았을 때 표시할 메시지")]
        [SerializeField] private string blockedMessage = "아직 수집할 수 없습니다";

        [Header("Particle Effect")]
        [Tooltip("힌트용 반짝 파티클 효과 (항상 재생, Inspector에서 직접 할당)")]
        [SerializeField] private ParticleSystem sparkleParticle;

        [Header("Visual")]
        [Tooltip("수집 시 오브젝트를 파괴할지 여부")]
        [SerializeField] private bool destroyOnCollect = true;

        [Tooltip("수집 시 재생할 파티클 효과 (선택)")]
        [SerializeField] private GameObject collectEffectPrefab;

        [Header("Audio")]
        [Tooltip("수집 시 재생할 사운드 (선택)")]
        [SerializeField] private AudioClip collectSound;

        [Tooltip("AudioSource (비어있으면 자동으로 찾거나 생성)")]
        [SerializeField] private AudioSource audioSource;

        public IngredientId Id => ingredientId;
        public float InteractionRadius => interactionRadius;

        private bool _collected;
        private Managers.QuestSequenceManager _questManager;

        private void Awake()
        {
            // AudioSource 자동 찾기 또는 생성
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }

            // 3D 사운드 설정
            if (audioSource != null)
            {
                audioSource.spatialBlend = 0.5f; // 3D 사운드
            }

            // QuestSequenceManager 찾기
            _questManager = Managers.QuestSequenceManager.Instance;

            // 반짝 파티클은 조건부로 활성화 (Awake에서는 비활성화)
            if (sparkleParticle != null && sparkleParticle.isPlaying)
            {
                sparkleParticle.Stop();
            }
            // // 반짝 파티클 시작 (이미 할당되어 있으면)
            // if (sparkleParticle != null && !sparkleParticle.isPlaying)
            // {
            //     sparkleParticle.Play();
            // }
        }

        private void OnEnable()
        {
            // 파티클 상태 업데이트
            UpdateParticleState();
        }

        private void Start()
        {
            // Start에서도 파티클 상태 업데이트
            UpdateParticleState();
            
            // QuestSequenceManager의 퀘스트 변경 이벤트 구독
            if (_questManager != null)
            {
                _questManager.OnQuestChanged += OnQuestChanged;
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (_questManager != null)
            {
                _questManager.OnQuestChanged -= OnQuestChanged;
            }
        }

        /// <summary>
        /// 퀘스트가 변경되었을 때 호출됩니다.
        /// </summary>
        private void OnQuestChanged(IngredientId newIngredientId)
        {
            // 퀘스트가 변경되면 파티클 상태 업데이트
            UpdateParticleState();
        }

        /// <summary>
        /// 파티클 상태를 조건에 따라 업데이트합니다.
        /// </summary>
        private void UpdateParticleState()
        {
            if (sparkleParticle == null || _collected)
                return;

            bool shouldPlay = ShouldShowParticle();

            if (shouldPlay && !sparkleParticle.isPlaying)
            {
                sparkleParticle.Play();
            }
            else if (!shouldPlay && sparkleParticle.isPlaying)
            {
                sparkleParticle.Stop();
            }
        }

        /// <summary>
        /// 파티클을 표시해야 하는지 확인합니다.
        /// </summary>
        private bool ShouldShowParticle()
        {
            // QuestManager가 없으면 표시하지 않음
            if (_questManager == null)
                return false;

            // 순차 퀘스트 체크: 현재 퀘스트가 이 재료일 때만 표시
            if (!_questManager.CanCollectIngredient(ingredientId))
            {
                return false;
            }

            // 달걀의 경우: 닭들이 모두 달아난 후에만 표시
            if (ingredientId == IngredientId.Egg)
            {
                var coopManager = Interaction.ChickenCoopManager.Instance;
                if (coopManager != null && coopManager.AllChickensEscaped)
                {
                    return true;
                }
                return false;
            }

            // 다른 재료의 경우: 퀘스트 조건만 확인 (이미 위에서 확인함)
            // CanCollectIngredient가 true면 현재 퀘스트이므로 표시
            return true;
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 파티클 정지
            if (sparkleParticle != null && sparkleParticle.isPlaying)
            {
                sparkleParticle.Stop();
            }
        }

        /// <summary>
        /// 조건이 만족되었는지 확인합니다.
        /// </summary>
        private bool IsConditionMet()
        {
            if (!requiresChickensEscaped)
                return true;

            var coopManager = Interaction.ChickenCoopManager.Instance;
            if (coopManager == null)
            {
                return false;
            }

            return coopManager.AllChickensEscaped;
        }

        // IInteractable 구현
        public bool CanInteract
        {
            get
            {
                if (_collected)
                    return false;

                if (!IsConditionMet())
                    return false;

                // 순차 퀘스트 체크
                if (_questManager != null && !_questManager.CanCollectIngredient(ingredientId))
                    return false;

                return true;
            }
        }

        public string GetInteractPrompt()
        {
            // 순차 퀘스트 체크 (프롬프트 자체를 숨김)
            if (_questManager != null && !_questManager.CanCollectIngredient(ingredientId))
                return null;

            if (!CanInteract)
            {
                return $"{blockedMessage} [F]";
            }

            // 달걀의 경우 특별한 프롬프트
            if (ingredientId == IngredientId.Egg)
            {
                return "달걀 줍기 [F]";
            }

            return $"Collect {ingredientId} [F]";
        }

        public void Interact(GameObject interactor)
        {
            // 딸기는 HiddenStrawberry를 사용해야 하므로 CollectibleIngredient로 수집하지 않음
            if (ingredientId == IngredientId.Strawberry)
            {
                return;
            }

            if (!CanInteract)
            {
                return;
            }

            if (requiresMiniGame)
            {
                StartMiniGame();
            }
            else
            {
                TryCollect();
            }
        }

        /// <summary>
        /// 미니게임을 시작합니다.
        /// </summary>
        private void StartMiniGame()
        {
            var manager = MiniGameManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.StartMiniGame(miniGameType, (success) =>
            {
                if (success)
                {
                    TryCollect();
                }
            });
        }

        public Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// 이 재료를 수집합니다.
        /// </summary>
        /// <returns>성공적으로 수집되었으면 true</returns>
        public bool TryCollect()
        {
            if (_collected)
                return false;

            var inventory = IngredientInventory.Instance;
            if (inventory == null)
            {
                return false;
            }

            if (!inventory.Collect(ingredientId))
                return false;

            _collected = true;

            // 수집 효과 재생
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            // 수집 사운드 재생
            if (collectSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
                else
                {
                    // AudioSource가 없으면 AudioSource.PlayClipAtPoint 사용 (3D 사운드)
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }
            }

            // 오브젝트 즉시 시각적으로 숨기기 (사운드는 계속 재생)
            HideObjectImmediately();

            // 오브젝트 처리 (사운드 재생을 위해 약간의 지연)
            if (destroyOnCollect)
            {
                // 사운드가 재생될 시간을 주기 위해 약간 지연
                if (collectSound != null && audioSource != null)
                {
                    Destroy(gameObject, collectSound.length + 0.1f);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        /// <summary>
        /// 오브젝트를 즉시 시각적으로 숨깁니다 (Renderer, Collider 비활성화)
        /// AudioSource는 사운드 재생을 위해 유지됩니다.
        /// </summary>
        private void HideObjectImmediately()
        {
            // 반짝 파티클 정지
            if (sparkleParticle != null && sparkleParticle.isPlaying)
            {
                sparkleParticle.Stop();
            }

            // 모든 Renderer 비활성화
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            // Collider 비활성화
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // WorldSpaceInteractionPrompt 비활성화
            var prompt = GetComponent<WorldSpaceInteractionPrompt>();
            if (prompt != null)
            {
                prompt.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 상호작용 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

