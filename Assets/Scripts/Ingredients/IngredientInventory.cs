using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 수집된 재료를 관리하고, 모든 재료가 수집되었는지 추적합니다.
    /// 싱글톤 패턴으로 전역 접근 가능합니다.
    /// </summary>
    public sealed class IngredientInventory : MonoBehaviour
    {
        public static IngredientInventory Instance { get; private set; }

        [Header("Required Ingredients")]
        [Tooltip("수집해야 할 재료 목록")]
        [SerializeField] private IngredientId[] requiredIngredients = new IngredientId[]
        {
            IngredientId.Flour,
            IngredientId.Sugar,
            IngredientId.Egg,
            IngredientId.Butter,
            IngredientId.Strawberry
        };

        private HashSet<IngredientId> _collectedIngredients = new HashSet<IngredientId>();

        [Header("Events")]
        [Tooltip("모든 재료가 수집되었을 때 발생하는 이벤트 (Inspector에서 연결)")]
        [SerializeField] private UnityEvent onAllIngredientsCollected = new UnityEvent();

        /// <summary>
        /// 모든 재료가 수집되었을 때 발생하는 이벤트 (Inspector에서 연결 가능)
        /// </summary>
        public UnityEvent OnAllIngredientsCollected => onAllIngredientsCollected;

        /// <summary>
        /// 모든 재료가 수집되었을 때 발생하는 C# 이벤트 (코드에서 구독용)
        /// </summary>
        public event Action OnAllCollected;

        /// <summary>
        /// 특정 재료가 수집되었을 때 발생하는 C# 이벤트 (코드에서 구독용)
        /// </summary>
        public event Action<IngredientId> OnIngredientCollected;

        public IReadOnlyCollection<IngredientId> RequiredIngredients => requiredIngredients;
        public IReadOnlyCollection<IngredientId> CollectedIngredients => _collectedIngredients;
        public int TotalRequired => requiredIngredients.Length;
        public int CollectedCount => _collectedIngredients.Count;
        public bool AllCollected => CollectedCount >= TotalRequired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                // 이벤트 구독자들을 모두 해제
                OnAllCollected = null;
                OnIngredientCollected = null;
            }
        }

        /// <summary>
        /// 재료를 수집합니다.
        /// </summary>
        /// <param name="id">수집할 재료 ID</param>
        /// <returns>이미 수집했으면 false, 새로 수집했으면 true</returns>
        public bool Collect(IngredientId id)
        {
            if (_collectedIngredients.Contains(id))
            {
                Debug.LogWarning($"[Inventory] {id} is already collected.");
                return false;
            }

            _collectedIngredients.Add(id);
            Debug.Log($"[Inventory] Collected {id}! ({CollectedCount}/{TotalRequired})");

            // C# 이벤트 호출 (코드 구독용)
            OnIngredientCollected?.Invoke(id);

            if (AllCollected)
            {
                Debug.Log("[Inventory] ALL INGREDIENTS COLLECTED!");
                // UnityEvent 호출 (Inspector 연결용)
                onAllIngredientsCollected?.Invoke();
                // C# 이벤트 호출 (코드 구독용)
                OnAllCollected?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// 특정 재료가 이미 수집되었는지 확인합니다.
        /// </summary>
        public bool IsCollected(IngredientId id)
        {
            return _collectedIngredients.Contains(id);
        }

        /// <summary>
        /// 인벤토리를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            _collectedIngredients.Clear();
            Debug.Log("[Inventory] Reset.");
        }
    }
}

