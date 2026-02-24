using UnityEngine;
using System.Collections.Generic;

namespace BirthdayCakeQuest.Ingredients
{
    /// <summary>
    /// 딸기 수집을 관리하는 싱글톤 매니저입니다.
    /// 5개의 딸기를 모두 수집하면 완료됩니다.
    /// </summary>
    public class StrawberryCollectionManager : MonoBehaviour
    {
        private static StrawberryCollectionManager _instance;
        public static StrawberryCollectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("StrawberryCollectionManager");
                    _instance = go.AddComponent<StrawberryCollectionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Collection Settings")]
        [Tooltip("수집해야 할 딸기 개수")]
        [SerializeField] private int targetCount = 5;

        [Tooltip("수집 완료 시 자동으로 IngredientInventory에 추가할지 여부")]
        [SerializeField] private bool autoAddToInventory = true;

        private HashSet<HiddenStrawberry> _collectedStrawberries = new HashSet<HiddenStrawberry>();
        private int _collectedCount = 0;

        public int CollectedCount => _collectedCount;
        public int TargetCount => targetCount;
        public bool IsComplete => _collectedCount >= targetCount;

        // 딸기 수집 카운트 변경 이벤트
        public event System.Action<int, int> OnStrawberryCountChanged; // (current, total)

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[StrawberryCollectionManager] Instance created");
        }

        /// <summary>
        /// 딸기를 수집합니다.
        /// </summary>
        public void CollectStrawberry(HiddenStrawberry strawberry)
        {
            if (strawberry == null)
                return;

            if (_collectedStrawberries.Contains(strawberry))
            {
                Debug.LogWarning("[StrawberryCollectionManager] 이미 수집한 딸기입니다!");
                return;
            }

            _collectedStrawberries.Add(strawberry);
            _collectedCount++;

            Debug.Log($"[StrawberryCollectionManager] 딸기 수집! ({_collectedCount}/{targetCount})");

            // 카운트 변경 이벤트 발행
            OnStrawberryCountChanged?.Invoke(_collectedCount, targetCount);

            // 모든 딸기를 수집했는지 확인
            if (IsComplete)
            {
                OnAllStrawberriesCollected();
            }
        }

        /// <summary>
        /// 모든 딸기를 수집했을 때 호출됩니다.
        /// </summary>
        private void OnAllStrawberriesCollected()
        {
            Debug.Log("[StrawberryCollectionManager] 모든 딸기를 수집했습니다!");

            if (autoAddToInventory)
            {
                var inventory = IngredientInventory.Instance;
                if (inventory != null)
                {
                    if (inventory.Collect(IngredientId.Strawberry))
                    {
                        Debug.Log("[StrawberryCollectionManager] 딸기가 인벤토리에 추가되었습니다!");
                    }
                    else
                    {
                        Debug.LogWarning("[StrawberryCollectionManager] 딸기를 인벤토리에 추가할 수 없습니다 (이미 수집됨?)");
                    }
                }
                else
                {
                    Debug.LogError("[StrawberryCollectionManager] IngredientInventory를 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// 수집 상태를 리셋합니다 (테스트용).
        /// </summary>
        [ContextMenu("Reset Collection")]
        public void ResetCollection()
        {
            _collectedStrawberries.Clear();
            _collectedCount = 0;
            Debug.Log("[StrawberryCollectionManager] 수집 상태 리셋");
        }

        private void OnDestroy()
        {
            // 씬이 닫힐 때 정리 작업
            if (_instance == this)
            {
                // 이벤트 구독 해제
                OnStrawberryCountChanged = null;
                
                // 수집 데이터 정리
                _collectedStrawberries?.Clear();
                _collectedCount = 0;
                
                _instance = null;
                Debug.Log("[StrawberryCollectionManager] Instance destroyed and cleaned up");
            }
        }
    }
}

