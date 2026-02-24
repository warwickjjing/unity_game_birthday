using UnityEngine;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 소에게 우유를 준 후 생성되는 버터 오브젝트입니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ButterCollectible : MonoBehaviour, IInteractable
    {
        [Header("Particle Effect")]
        [Tooltip("힌트용 반짝 파티클 효과 (항상 재생, Inspector에서 직접 할당)")]
        [SerializeField] private ParticleSystem sparkleParticle;

        [Header("Butter Settings")]
        [Tooltip("수집 시 오브젝트를 파괴할지 여부")]
        [SerializeField] private bool destroyOnCollect = true;

        [Tooltip("수집 시 재생할 파티클 효과 (선택)")]
        [SerializeField] private GameObject collectEffectPrefab;

        [Header("Audio")]
        [Tooltip("수집 시 재생할 사운드 (선택)")]
        [SerializeField] private AudioClip collectSound;

        [Tooltip("AudioSource (비어있으면 자동으로 찾거나 생성)")]
        [SerializeField] private AudioSource audioSource;

        private bool _collected = false;

        public bool CanInteract => !_collected;

        public string GetInteractPrompt()
        {
            return "버터 줍기 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            // Collider가 Trigger인지 확인
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }

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

            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[ButterCollectible] WorldSpaceInteractionPrompt 자동 추가됨");
            }

            // 반짝 파티클 시작 (이미 할당되어 있으면)
            if (sparkleParticle != null && !sparkleParticle.isPlaying)
            {
                sparkleParticle.Play();
            }
        }

        private void OnEnable()
        {
            // 오브젝트가 활성화될 때 파티클 재생
            if (sparkleParticle != null && !_collected && !sparkleParticle.isPlaying)
            {
                sparkleParticle.Play();
            }
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 파티클 정지
            if (sparkleParticle != null && sparkleParticle.isPlaying)
            {
                sparkleParticle.Stop();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            Debug.Log("[ButterCollectible] 버터 수집!");

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

            // IngredientInventory에 추가
            var inventory = IngredientInventory.Instance;
            if (inventory != null)
            {
                if (inventory.Collect(IngredientId.Butter))
                {
                    Debug.Log("[ButterCollectible] 버터가 인벤토리에 추가되었습니다!");
                }
                else
                {
                    Debug.LogWarning("[ButterCollectible] 버터를 인벤토리에 추가할 수 없습니다 (이미 수집됨?)");
                }
            }
            else
            {
                Debug.LogError("[ButterCollectible] IngredientInventory를 찾을 수 없습니다!");
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
    }
}

