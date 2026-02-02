using UnityEngine;
using UnityEditor;
using BirthdayCakeQuest.Tools;

namespace BirthdayCakeQuest.Editor
{
    /// <summary>
    /// HouseGenerator를 Unity 에디터 메뉴에서 실행할 수 있게 합니다.
    /// </summary>
    public static class HouseGeneratorEditor
    {
        [MenuItem("Tools/Generate House")]
        public static void GenerateHouse()
        {
            Debug.Log("[HouseGeneratorEditor] 🏠 2룸 집 생성 시작...");

            // 기존 House 삭제
            GameObject existingHouse = GameObject.Find("House");
            if (existingHouse != null)
            {
                Object.DestroyImmediate(existingHouse);
                Debug.Log("[HouseGeneratorEditor] 기존 House 삭제됨");
            }

            // 새 집 생성
            GameObject house = HouseGenerator.Generate();

            // Scene에 저장
            EditorUtility.SetDirty(house);
            Selection.activeGameObject = house;

            Debug.Log("[HouseGeneratorEditor] ✅ 2룸 집 생성 완료!");
        }

        [MenuItem("Tools/Place Ingredients")]
        public static void PlaceIngredients()
        {
            Debug.Log("[HouseGeneratorEditor] 🍰 재료 배치 시작...");

            GameObject house = GameObject.Find("House");
            if (house == null)
            {
                Debug.LogError("[HouseGeneratorEditor] ❌ House를 찾을 수 없습니다! 먼저 'Generate House'를 실행하세요.");
                return;
            }

            // 기존 재료 삭제
            foreach (Transform child in house.transform)
            {
                if (child.name.StartsWith("Ingredient_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // 재료 배치
            HouseGenerator.PlaceIngredients(house.transform);

            EditorUtility.SetDirty(house);
            Debug.Log("[HouseGeneratorEditor] ✅ 재료 배치 완료!");
        }

        [MenuItem("Tools/Generate House + Ingredients")]
        public static void GenerateAll()
        {
            GenerateHouse();
            PlaceIngredients();
        }
    }
}
