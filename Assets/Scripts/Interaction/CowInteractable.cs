using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BirthdayCakeQuest.Interaction;
using BirthdayCakeQuest.UI;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 소와의 상호작용을 처리합니다.
    /// 우유를 주면 5초 후 버터를 생성합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CowInteractable : MonoBehaviour, IInteractable
    {
        [Header("Cow Settings")]
        [Tooltip("우유를 받을 수 있는 거리")]
        [SerializeField] private float interactionDistance = 2f;

        [Tooltip("버터 생성 위치")]
        [SerializeField] private Transform butterSpawnPoint;

        [Tooltip("버터 프리팹 (비어있으면 자동 생성)")]
        [SerializeField] private GameObject butterPrefab;

        [Header("Butter Spawn Settings")]
        [Tooltip("버터 생성까지 대기 시간 (초)")]
        [SerializeField] private float butterSpawnDelay = 5f;

        [Tooltip("버터 생성 위치 오프셋 (소 앞)")]
        [SerializeField] private Vector3 butterSpawnOffset = new Vector3(0, 0, 1.5f);

        [Header("Dialogue")]
        [Tooltip("소에게 우유를 줄 때 표시할 대화 목록")]
        [SerializeField] private List<DialogueData> giveMilkDialogues = new List<DialogueData>();

        [Tooltip("버터가 생성될 때 표시할 대화 목록")]
        [SerializeField] private List<DialogueData> butterSpawnDialogues = new List<DialogueData>();

        private MilkInteractable _nearbyMilk;
        private bool _hasReceivedMilk = false;
        private float _butterSpawnTimer = 0f;
        private bool _isWaitingForButter = false;

        public bool CanInteract => HasMilkNearby() && !_hasReceivedMilk;

        public string GetInteractPrompt()
        {
            if (_hasReceivedMilk)
            {
                if (_isWaitingForButter)
                {
                    float remainingTime = butterSpawnDelay - _butterSpawnTimer;
                    return $"버터 생성 대기 중... {remainingTime:F1}초 [F]";
                }
                return "버터를 받으세요 [F]";
            }

            if (HasMilkNearby())
            {
                return "우유 주기 [F]";
            }

            return "우유가 필요합니다 [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void Awake()
        {
            // WorldSpaceInteractionPrompt 자동 추가
            if (GetComponent<WorldSpaceInteractionPrompt>() == null)
            {
                var prompt = gameObject.AddComponent<WorldSpaceInteractionPrompt>();
                Debug.Log("[CowInteractable] WorldSpaceInteractionPrompt 자동 추가됨");
            }
        }

        private void Update()
        {
            // 버터 생성 대기 중
            if (_isWaitingForButter)
            {
                _butterSpawnTimer += Time.deltaTime;

                if (_butterSpawnTimer >= butterSpawnDelay)
                {
                    SpawnButter();
                    _isWaitingForButter = false;
                    _butterSpawnTimer = 0f;
                }
            }

            // 근처 우유 찾기
            FindNearbyMilk();
        }

        /// <summary>
        /// 근처에 우유가 있는지 확인합니다.
        /// </summary>
        private bool HasMilkNearby()
        {
            return _nearbyMilk != null && _nearbyMilk.IsCarried();
        }

        /// <summary>
        /// 근처의 우유를 찾습니다.
        /// </summary>
        private void FindNearbyMilk()
        {
            MilkInteractable[] allMilk = FindObjectsOfType<MilkInteractable>();
            _nearbyMilk = null;

            foreach (MilkInteractable milk in allMilk)
            {
                if (!milk.IsCarried())
                    continue;

                float distance = Vector3.Distance(transform.position, milk.transform.position);
                if (distance <= interactionDistance)
                {
                    _nearbyMilk = milk;
                    break;
                }
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_hasReceivedMilk && !_isWaitingForButter)
            {
                // 버터가 이미 생성되었는지 확인
                // (ButterCollectible이 있으면 수집 가능)
                Debug.Log("[CowInteractable] 버터를 확인하세요!");
                return;
            }

            if (!HasMilkNearby())
            {
                Debug.Log("[CowInteractable] 우유를 가져와주세요!");
                return;
            }

            // 우유를 소에게 주기
            GiveMilkToCow();
        }

        /// <summary>
        /// 소에게 우유를 줍니다.
        /// </summary>
        private void GiveMilkToCow()
        {
            if (_nearbyMilk == null || !_nearbyMilk.IsCarried())
                return;

            Debug.Log("[CowInteractable] 소에게 우유를 주었습니다!");

            // 우유 제거
            _nearbyMilk.GiveToCow();
            _nearbyMilk = null;

            // 대화 표시 (우유를 줄 때) - 코루틴으로 지연하여 표시
            if (giveMilkDialogues != null && giveMilkDialogues.Count > 0)
            {
                StartCoroutine(ShowGiveMilkDialogueAfterDelay());
            }

            // 버터 생성 시작
            _hasReceivedMilk = true;
            _isWaitingForButter = true;
            _butterSpawnTimer = 0f;
        }

        /// <summary>
        /// 버터를 생성합니다.
        /// </summary>
        private void SpawnButter()
        {
            Vector3 spawnPosition;

            if (butterSpawnPoint != null)
            {
                spawnPosition = butterSpawnPoint.position;
            }
            else
            {
                spawnPosition = transform.position + transform.TransformDirection(butterSpawnOffset);
            }

            // 바닥에 레이캐스트하여 Y 위치 조정
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
            {
                spawnPosition.y = hit.point.y + 0.1f;
            }

            GameObject butterObj;

            if (butterPrefab != null)
            {
                butterObj = Instantiate(butterPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // 버터 오브젝트 자동 생성
                butterObj = new GameObject("Butter");
                butterObj.transform.position = spawnPosition;

                // Collider 추가
                BoxCollider collider = butterObj.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.3f, 0.2f, 0.3f);
                collider.isTrigger = true;

                // ButterCollectible 컴포넌트 추가
                ButterCollectible butterCollectible = butterObj.AddComponent<ButterCollectible>();
                Debug.Log("[CowInteractable] 버터 오브젝트 자동 생성 완료");
            }

            Debug.Log($"[CowInteractable] 버터가 생성되었습니다! 위치: {spawnPosition}");

            // QuestSequenceManager에 버터 오브젝트 등록
            RegisterButterToQuestManager(butterObj);

            // 대화 표시 (버터가 생성될 때) - 대화가 끝난 후에 시작하도록 지연
            if (butterSpawnDialogues != null && butterSpawnDialogues.Count > 0)
            {
                // 대화가 이미 진행 중이면 대화가 끝난 후에 시작하도록 코루틴 사용
                StartCoroutine(ShowButterSpawnDialogueAfterDelay());
            }
        }

        /// <summary>
        /// 우유 주기 대화를 지연시켜 표시합니다.
        /// 다른 대화가 진행 중이면 대화가 끝난 후에 시작합니다.
        /// </summary>
        private IEnumerator ShowGiveMilkDialogueAfterDelay()
        {
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem == null)
            {
                Debug.LogWarning("[CowInteractable] DialogueSystem을 찾을 수 없습니다!");
                yield break;
            }

            // 현재 대화가 진행 중이면 대화가 끝날 때까지 대기
            while (dialogueSystem.IsPlaying)
            {
                yield return null;
            }

            // 추가로 한 프레임 대기 (대화 종료 이벤트가 완전히 처리되도록)
            yield return null;
            yield return null; // 한 프레임 더 대기

            // DialogueUI를 다시 찾기 (씬 전환 후일 수 있음)
            dialogueSystem.FindDialogueUI();
            
            // 대화 표시
            if (giveMilkDialogues != null && giveMilkDialogues.Count > 0)
            {
                dialogueSystem.StartDialogueSequence(giveMilkDialogues);
                Debug.Log($"[CowInteractable] 우유 주기 대화 표시 ({giveMilkDialogues.Count}개)");
            }
        }

        /// <summary>
        /// 버터 생성 대화를 지연시켜 표시합니다.
        /// 다른 대화가 진행 중이면 대화가 끝난 후에 시작합니다.
        /// </summary>
        private IEnumerator ShowButterSpawnDialogueAfterDelay()
        {
            var dialogueSystem = DialogueSystem.Instance;
            if (dialogueSystem == null)
            {
                Debug.LogWarning("[CowInteractable] DialogueSystem을 찾을 수 없습니다!");
                yield break;
            }

            // 현재 대화가 진행 중이면 대화가 끝날 때까지 대기
            while (dialogueSystem.IsPlaying)
            {
                yield return null;
            }

            // 추가로 한 프레임 대기 (대화 종료 이벤트가 완전히 처리되도록)
            yield return null;
            yield return null; // 한 프레임 더 대기

            // DialogueUI를 다시 찾기 (씬 전환 후일 수 있음)
            dialogueSystem.FindDialogueUI();
            
            // 대화 표시
            if (butterSpawnDialogues != null && butterSpawnDialogues.Count > 0)
            {
                dialogueSystem.StartDialogueSequence(butterSpawnDialogues);
                Debug.Log($"[CowInteractable] 버터 생성 대화 표시 ({butterSpawnDialogues.Count}개)");
            }
        }

        /// <summary>
        /// 생성된 버터를 QuestSequenceManager에 등록합니다.
        /// </summary>
        private void RegisterButterToQuestManager(GameObject butterObj)
        {
            var questManager = BirthdayCakeQuest.Managers.QuestSequenceManager.Instance;
            if (questManager == null)
            {
                Debug.LogWarning("[CowInteractable] QuestSequenceManager를 찾을 수 없습니다!");
                return;
            }

            // 버터 퀘스트 찾기
            var butterQuestStep = questManager.GetQuestStepByIngredient(IngredientId.Butter);
            if (butterQuestStep != null)
            {
                // Target Ingredient Objects에 버터 추가
                if (!butterQuestStep.targetIngredientObjects.Contains(butterObj))
                {
                    butterQuestStep.targetIngredientObjects.Add(butterObj);
                    Debug.Log("[CowInteractable] 버터가 QuestSequenceManager에 등록되었습니다!");

                    // 파티클 상태 업데이트 (현재 퀘스트가 버터면 파티클 활성화)
                    questManager.RefreshParticleStates();
                }
            }
            else
            {
                Debug.LogWarning("[CowInteractable] 버터 퀘스트를 찾을 수 없습니다!");
            }
        }
    }
}

