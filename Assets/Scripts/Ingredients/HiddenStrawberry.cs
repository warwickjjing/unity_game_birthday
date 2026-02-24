using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 숲에 숨겨진 딸기 오브젝트입니다.
    /// 반짝 파티클 힌트를 표시하고 상호작용으로 수집할 수 있습니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HiddenStrawberry : MonoBehaviour, IInteractable
    {
        [Header("Strawberry Settings")]
        [Tooltip("이 딸기가 수집되었는지 여부")]
        [SerializeField] private bool isCollected = false;

        [Header("Particle Effect")]
        [Tooltip("반짝 파티클 효과 (Inspector에서 직접 할당)")]
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

        private StrawberryCollectionManager _collectionManager;
        private Managers.QuestSequenceManager _questManager;

        public bool CanInteract
        {
            get
            {
                if (isCollected)
                    return false;

                // 순차 퀘스트 체크
                if (_questManager != null && !_questManager.CanCollectIngredient(IngredientId.Strawberry))
                    return false;

                return true;
            }
        }

        public string GetInteractPrompt()
        {
            // 순차 퀘스트 체크
            if (_questManager != null && !_questManager.CanCollectIngredient(IngredientId.Strawberry))
                return null;

            return "딸기 줍기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            // Collider 확인 및 경고
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogError($"[HiddenStrawberry] {gameObject.name}: Collider가 없습니다! Collider를 추가하세요.");
            }
            else
            {
                if (!col.isTrigger)
                {
                    col.isTrigger = true;
                }
                
                if (!col.enabled)
                {
                    Debug.LogWarning($"[HiddenStrawberry] {gameObject.name}: Collider가 비활성화되어 있습니다!");
                }
            }

            // isCollected 초기화 확인
            if (isCollected)
            {
                isCollected = false;
            }

            // CollectibleIngredient 컴포넌트가 있으면 경고 (딸기는 HiddenStrawberry만 사용)
            var collectibleIngredient = GetComponent<CollectibleIngredient>();
            if (collectibleIngredient != null)
            {
                Debug.LogWarning($"[HiddenStrawberry] {gameObject.name}: CollectibleIngredient 컴포넌트가 있습니다. 딸기는 HiddenStrawberry만 사용해야 하므로 CollectibleIngredient를 제거하세요!");
            }

            // 파티클 시스템 찾기 (자동 생성하지 않음 - Inspector에서 할당)
            if (sparkleParticle == null)
            {
                sparkleParticle = GetComponentInChildren<ParticleSystem>();
            }

            // AudioSource 자동 찾기 또는 생성
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0.5f; // 3D 사운드 설정
                }
            }

            // StrawberryCollectionManager 찾기
            _collectionManager = StrawberryCollectionManager.Instance;
            if (_collectionManager == null)
            {
                Debug.LogWarning("[HiddenStrawberry] StrawberryCollectionManager를 찾을 수 없습니다!");
            }

            // QuestSequenceManager 찾기
            _questManager = Managers.QuestSequenceManager.Instance;

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
            }
        }

        private void Start()
        {
            // 파티클 시작
            if (sparkleParticle != null && !sparkleParticle.isPlaying)
            {
                sparkleParticle.Play();
            }
        }

        /// <summary>
        /// 오브젝트를 즉시 시각적으로 숨깁니다 (Renderer, Collider 비활성화)
        /// AudioSource는 사운드 재생을 위해 유지됩니다.
        /// </summary>
        private void HideObjectImmediately()
        {
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

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            isCollected = true;

            // 수집 효과 재생
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            // 수집 사운드 재생 (오브젝트가 Destroy되기 전에 재생)
            if (collectSound != null)
            {
                if (audioSource != null)
                {
                    // AudioSource가 있으면 사용
                    audioSource.PlayOneShot(collectSound);
                }
                else
                {
                    // AudioSource가 없으면 AudioSource.PlayClipAtPoint 사용 (3D 사운드)
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }
            }

            // CollectionManager에 알림
            if (_collectionManager != null)
            {
                _collectionManager.CollectStrawberry(this);
            }

            // 파티클 정지
            if (sparkleParticle != null)
            {
                sparkleParticle.Stop();
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
        }

    }
}

