using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 수집해야 할 재료 목록과 수집 상태를 UI로 표시합니다.
    /// TextMeshPro를 사용하여 체크리스트를 표시합니다.
    /// </summary>
    public sealed class IngredientChecklistUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("체크리스트를 표시할 TextMeshPro 컴포넌트")]
        [SerializeField] private TextMeshProUGUI checklistText;

        [Header("Display Settings")]
        [Tooltip("재료 이름 한글 매핑")]
        [SerializeField] private IngredientDisplayName[] ingredientNames = new IngredientDisplayName[]
        {
            new IngredientDisplayName { id = IngredientId.Flour, displayName = "밀가루" },
            new IngredientDisplayName { id = IngredientId.Sugar, displayName = "설탕" },
            new IngredientDisplayName { id = IngredientId.Egg, displayName = "계란" },
            new IngredientDisplayName { id = IngredientId.Butter, displayName = "버터" },
            new IngredientDisplayName { id = IngredientId.Strawberry, displayName = "딸기" }
        };

        [Header("Visual Settings")]
        [Tooltip("수집 완료 시 체크 마크")]
        [SerializeField] private string checkMark = "✓";
        
        [Tooltip("수집 전 체크 마크")]
        [SerializeField] private string uncheckMark = "☐";
        
        [Tooltip("수집 완료된 항목 색상")]
        [SerializeField] private Color completedColor = Color.green;
        
        [Tooltip("수집 전 항목 색상")]
        [SerializeField] private Color incompleteColor = Color.white;

        private Dictionary<IngredientId, string> _nameMap;
        private IngredientInventory _inventory;

        private void Awake()
        {
            // 이름 매핑 딕셔너리 생성
            _nameMap = new Dictionary<IngredientId, string>();
            foreach (var item in ingredientNames)
            {
                _nameMap[item.id] = item.displayName;
            }

            if (checklistText == null)
            {
                Debug.LogError("[IngredientChecklistUI] checklistText is not assigned!");
            }
        }

        private void Start()
        {
            _inventory = IngredientInventory.Instance;
            
            if (_inventory == null)
            {
                Debug.LogError("[IngredientChecklistUI] IngredientInventory not found!");
                return;
            }

            // 이벤트 구독
            _inventory.OnIngredientCollected += OnIngredientCollected;
            _inventory.OnAllCollected += OnAllCollected;

            // 초기 UI 업데이트
            UpdateChecklistDisplay();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnIngredientCollected -= OnIngredientCollected;
                _inventory.OnAllCollected -= OnAllCollected;
            }
        }

        private void OnIngredientCollected(IngredientId id)
        {
            UpdateChecklistDisplay();
        }

        private void OnAllCollected()
        {
            UpdateChecklistDisplay();
            Debug.Log("[IngredientChecklistUI] All ingredients collected!");
        }

        private void UpdateChecklistDisplay()
        {
            if (checklistText == null || _inventory == null)
                return;

            string text = "<b>케이크 재료 체크리스트</b>\n";
            text += $"({_inventory.CollectedCount}/{_inventory.TotalRequired})\n\n";

            foreach (var ingredient in _inventory.RequiredIngredients)
            {
                bool isCollected = _inventory.IsCollected(ingredient);
                string mark = isCollected ? checkMark : uncheckMark;
                string colorHex = isCollected 
                    ? ColorUtility.ToHtmlStringRGB(completedColor)
                    : ColorUtility.ToHtmlStringRGB(incompleteColor);
                
                string displayName = GetDisplayName(ingredient);
                
                text += $"<color=#{colorHex}>{mark} {displayName}</color>\n";
            }

            checklistText.text = text;
        }

        private string GetDisplayName(IngredientId id)
        {
            if (_nameMap.TryGetValue(id, out string name))
                return name;
            
            return id.ToString(); // 폴백: enum 이름 그대로
        }

        /// <summary>
        /// UI를 강제로 업데이트합니다.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateChecklistDisplay();
        }

        [System.Serializable]
        private struct IngredientDisplayName
        {
            public IngredientId id;
            public string displayName;
        }
    }
}

