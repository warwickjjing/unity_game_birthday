using UnityEngine;
using BirthdayCakeQuest.Props;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Tools
{
    /// <summary>
    /// 안정적인 2룸 구조를 자동 생성합니다.
    /// Unity Editor 메뉴에서 Tools → Generate House를 실행하세요.
    /// </summary>
    public class HouseGenerator : MonoBehaviour
    {
        private const float UNIT = 1f;
        private const float WALL_HEIGHT = 2.5f;
        private const float WALL_THICKNESS = 0.2f;
        private const float FLOOR_Y = 0.001f;
        private const float DOOR_WIDTH = 1f;
        private const float DOOR_HEIGHT = 2f;

        /// <summary>
        /// 2룸 집 구조를 생성합니다.
        /// </summary>
        public static GameObject Generate()
        {
            GameObject house = new GameObject("House");
            house.transform.position = Vector3.zero;

            // 거실을 중심(0, 0, 0)으로 배치
            // 방1, 방2는 위쪽에 배치

            // 1. 거실/주방 (중앙, 큰 공간)
            CreateLivingRoom(house.transform);

            // 2. 방1 (왼쪽 위)
            CreateRoom1(house.transform);

            // 3. 방2 (오른쪽 위)
            CreateRoom2(house.transform);

            // 4. 연결 복도 (최소한)
            CreateHallway(house.transform);

            // 5. 가구 (소파, TV)
            CreateFurniture(house.transform);

            return house;
        }

        /// <summary>
        /// 재료들을 각 방에 배치합니다.
        /// </summary>
        public static void PlaceIngredients(Transform houseRoot)
        {
            string[] ingredientPrefabs = new string[]
            {
                "Assets/Prefabs/Ingredient_Flour.prefab",
                "Assets/Prefabs/Ingredient_Sugar.prefab",
                "Assets/Prefabs/Ingredient_Egg.prefab",
                "Assets/Prefabs/Ingredient_Butter.prefab",
                "Assets/Prefabs/Ingredient_Strawberry.prefab"
            };

            // 재료 위치 (간단하게)
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-3 * UNIT, 0.5f, 6 * UNIT),   // 방1 (Flour)
                new Vector3(3 * UNIT, 0.5f, 6 * UNIT),    // 방2 (Sugar)
                new Vector3(-3 * UNIT, 0.5f, 0),          // 거실 왼쪽 (Egg)
                new Vector3(3 * UNIT, 0.5f, 0),           // 거실 오른쪽 (Butter)
                new Vector3(0, 0.5f, -3 * UNIT)           // 거실 앞쪽 (Strawberry)
            };

#if UNITY_EDITOR
            for (int i = 0; i < ingredientPrefabs.Length; i++)
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ingredientPrefabs[i]);
                if (prefab != null)
                {
                    GameObject ingredient = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    ingredient.transform.position = positions[i];
                    ingredient.transform.SetParent(houseRoot);
                }
                else
                {
                    Debug.LogWarning($"[HouseGenerator] ⚠️ Prefab을 찾을 수 없음: {ingredientPrefabs[i]}");
                }
            }
#endif
        }

        // ===========================================
        // 거실/주방 (중앙, 큰 공간)
        // ===========================================

        private static void CreateLivingRoom(Transform parent)
        {
            GameObject room = new GameObject("LivingRoom");
            room.transform.SetParent(parent);
            room.transform.position = Vector3.zero;

            // 바닥 (10m x 10m)
            CreateFloor(room.transform, "Floor", new Vector3(0, 0, 0), new Vector3(10 * UNIT, 10 * UNIT));

            // 외벽 4개
            // 왼쪽 벽
            CreateWall(room.transform, "Wall_Left", new Vector3(-5 * UNIT, WALL_HEIGHT / 2, 0), 
                new Vector3(WALL_THICKNESS, WALL_HEIGHT, 10 * UNIT));
            
            // 오른쪽 벽
            CreateWall(room.transform, "Wall_Right", new Vector3(5 * UNIT, WALL_HEIGHT / 2, 0), 
                new Vector3(WALL_THICKNESS, WALL_HEIGHT, 10 * UNIT));
            
            // 앞쪽 벽 (입구, 중앙에 문 공간)
            CreateWall(room.transform, "Wall_Front_Left", new Vector3(-3 * UNIT, WALL_HEIGHT / 2, -5 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            CreateWall(room.transform, "Wall_Front_Right", new Vector3(3 * UNIT, WALL_HEIGHT / 2, -5 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            
            // 입구 문
            CreateDoor(room.transform, "EntranceDoor", new Vector3(0, 0, -5 * UNIT), Quaternion.identity);
            
            // 뒤쪽 벽 (방들과 연결되는 쪽) - 복도 공간 제외
            CreateWall(room.transform, "Wall_Back_Left", new Vector3(-3 * UNIT, WALL_HEIGHT / 2, 5 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            CreateWall(room.transform, "Wall_Back_Right", new Vector3(3 * UNIT, WALL_HEIGHT / 2, 5 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));

            // 주방 카운터 (간단하게)
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "KitchenCounter";
            counter.transform.SetParent(room.transform);
            counter.transform.localPosition = new Vector3(-3 * UNIT, 0.9f, 3 * UNIT);
            counter.transform.localScale = new Vector3(2 * UNIT, 0.1f, 1 * UNIT);
            
            Material counterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            counterMat.color = new Color(0.3f, 0.25f, 0.2f);
            counter.GetComponent<Renderer>().material = counterMat;
        }

        // ===========================================
        // 방1 (왼쪽 위)
        // ===========================================

        private static void CreateRoom1(Transform parent)
        {
            GameObject room = new GameObject("Room1");
            room.transform.SetParent(parent);
            room.transform.position = new Vector3(-3 * UNIT, 0, 8 * UNIT);

            // 바닥 (4m x 4m)
            CreateFloor(room.transform, "Floor", Vector3.zero, new Vector3(4 * UNIT, 4 * UNIT));

            // 벽 4개 (복도쪽 제외, 문 있음)
            // 왼쪽 벽
            CreateWall(room.transform, "Wall_Left", new Vector3(-2 * UNIT, WALL_HEIGHT / 2, 0), 
                new Vector3(WALL_THICKNESS, WALL_HEIGHT, 4 * UNIT));
            
            // 뒤쪽 벽
            CreateWall(room.transform, "Wall_Back", new Vector3(0, WALL_HEIGHT / 2, 2 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            
            // 앞쪽 벽 (복도로 향함, 문 공간)
            CreateWall(room.transform, "Wall_Front_Left", new Vector3(-1 * UNIT, WALL_HEIGHT / 2, -2 * UNIT), 
                new Vector3(2 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            CreateWall(room.transform, "Wall_Front_Right", new Vector3(1 * UNIT, WALL_HEIGHT / 2, -2 * UNIT), 
                new Vector3(2 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            
            // 방1 문
            CreateDoor(room.transform, "Room1Door", new Vector3(0, 0, -2 * UNIT), Quaternion.identity);
            
            // 오른쪽 벽 (방2와 구분)
            CreateWall(room.transform, "Wall_Right", new Vector3(2 * UNIT, WALL_HEIGHT / 2, 0), 
                new Vector3(WALL_THICKNESS, WALL_HEIGHT, 4 * UNIT));

            // 침대
            CreateBed(room.transform, new Vector3(0, 0.3f, 0));
        }

        // ===========================================
        // 방2 (오른쪽 위)
        // ===========================================

        private static void CreateRoom2(Transform parent)
        {
            GameObject room = new GameObject("Room2");
            room.transform.SetParent(parent);
            room.transform.position = new Vector3(3 * UNIT, 0, 8 * UNIT);

            // 바닥 (4m x 4m)
            CreateFloor(room.transform, "Floor", Vector3.zero, new Vector3(4 * UNIT, 4 * UNIT));

            // 벽 4개 (복도쪽 제외, 문 있음)
            // 오른쪽 벽
            CreateWall(room.transform, "Wall_Right", new Vector3(2 * UNIT, WALL_HEIGHT / 2, 0), 
                new Vector3(WALL_THICKNESS, WALL_HEIGHT, 4 * UNIT));
            
            // 뒤쪽 벽
            CreateWall(room.transform, "Wall_Back", new Vector3(0, WALL_HEIGHT / 2, 2 * UNIT), 
                new Vector3(4 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            
            // 앞쪽 벽 (복도로 향함, 문 공간)
            CreateWall(room.transform, "Wall_Front_Left", new Vector3(-1 * UNIT, WALL_HEIGHT / 2, -2 * UNIT), 
                new Vector3(2 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            CreateWall(room.transform, "Wall_Front_Right", new Vector3(1 * UNIT, WALL_HEIGHT / 2, -2 * UNIT), 
                new Vector3(2 * UNIT, WALL_HEIGHT, WALL_THICKNESS));
            
            // 방2 문
            CreateDoor(room.transform, "Room2Door", new Vector3(0, 0, -2 * UNIT), Quaternion.identity);
            
            // 왼쪽 벽은 방1에서 이미 생성됨

            // 침대
            CreateBed(room.transform, new Vector3(0, 0.3f, 0));
        }

        // ===========================================
        // 복도 (방들과 거실 연결)
        // ===========================================

        private static void CreateHallway(Transform parent)
        {
            GameObject hallway = new GameObject("Hallway");
            hallway.transform.SetParent(parent);
            hallway.transform.position = new Vector3(0, 0, 6 * UNIT);

            // 복도 바닥 (2m x 2m, 작게)
            CreateFloor(hallway.transform, "Floor", Vector3.zero, new Vector3(2 * UNIT, 2 * UNIT));

            // 복도 양쪽 벽은 방들에서 이미 생성됨
        }

        // ===========================================
        // 가구 (소파, TV)
        // ===========================================

        private static void CreateFurniture(Transform parent)
        {
            // 소파 (거실 앞쪽)
            GameObject sofa = new GameObject("Sofa");
            sofa.transform.SetParent(parent);
            sofa.transform.position = new Vector3(0, 0, -2 * UNIT);
            sofa.transform.rotation = Quaternion.Euler(0, 0, 0); // TV를 향함

            // 소파 좌석
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(sofa.transform);
            seat.transform.localPosition = new Vector3(0, 0.4f, 0);
            seat.transform.localScale = new Vector3(3 * UNIT, 0.5f, 1 * UNIT);

            // 소파 등받이
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back";
            back.transform.SetParent(sofa.transform);
            back.transform.localPosition = new Vector3(0, 1f, -0.4f);
            back.transform.localScale = new Vector3(3 * UNIT, 1.2f, 0.2f);

            Material sofaMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sofaMat.color = new Color(0.3f, 0.4f, 0.6f);
            seat.GetComponent<Renderer>().material = sofaMat;
            back.GetComponent<Renderer>().material = sofaMat;

            // 앉기 위치
            GameObject sitPosition = new GameObject("SofaSitPosition");
            sitPosition.transform.SetParent(sofa.transform);
            sitPosition.transform.localPosition = new Vector3(0, 0.5f, 0);

            // TV (거실 뒤쪽 벽)
            GameObject tv = new GameObject("TV");
            tv.transform.SetParent(parent);
            tv.transform.position = new Vector3(0, 1.5f, 3 * UNIT);
            tv.transform.rotation = Quaternion.Euler(0, 180, 0);

            // TV 스탠드
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "Stand";
            stand.transform.SetParent(tv.transform);
            stand.transform.localPosition = new Vector3(0, -1f, 0);
            stand.transform.localScale = new Vector3(2 * UNIT, 0.3f, 0.5f);
            
            Material standMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            standMat.color = new Color(0.2f, 0.2f, 0.2f);
            stand.GetComponent<Renderer>().material = standMat;

            // TV 스크린
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "TV_Screen";
            screen.transform.SetParent(tv.transform);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localRotation = Quaternion.Euler(0, 180, 0);
            screen.transform.localScale = new Vector3(3 * UNIT, 2 * UNIT, 1);
            
            Material screenMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            screenMat.color = Color.black;
            screen.GetComponent<Renderer>().material = screenMat;
        }

        // ===========================================
        // 유틸리티 메소드
        // ===========================================

        private static void CreateFloor(Transform parent, string name, Vector3 position, Vector3 size)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = name;
            floor.transform.SetParent(parent);
            floor.transform.localPosition = position + new Vector3(0, FLOOR_Y, 0);
            floor.transform.localScale = new Vector3(size.x, 0.1f, size.y);

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 1f, 1f); // 하얀 대리석
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetFloat("_Metallic", 0.1f);
            floor.GetComponent<Renderer>().material = mat;
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.95f, 0.95f, 0.95f);
            wall.GetComponent<Renderer>().material = mat;
        }

        private static void CreateBed(Transform parent, Vector3 position)
        {
            GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "Bed";
            bed.transform.SetParent(parent);
            bed.transform.localPosition = position;
            bed.transform.localScale = new Vector3(2f, 0.6f, 2.2f);

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.9f, 0.95f);
            bed.GetComponent<Renderer>().material = mat;
        }

        private static void CreateDoor(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            // 문 루트 오브젝트
            GameObject doorRoot = new GameObject(name);
            doorRoot.transform.SetParent(parent);
            doorRoot.transform.localPosition = position;
            doorRoot.transform.localRotation = rotation;

            // 문 Pivot (회전 중심)
            GameObject doorPivot = new GameObject("DoorPivot");
            doorPivot.transform.SetParent(doorRoot.transform);
            doorPivot.transform.localPosition = new Vector3(-DOOR_WIDTH / 2, 0, 0); // 왼쪽 축

            // 문짝 (Panel)
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "DoorPanel";
            doorPanel.transform.SetParent(doorPivot.transform);
            doorPanel.transform.localPosition = new Vector3(DOOR_WIDTH / 2, DOOR_HEIGHT / 2, 0);
            doorPanel.transform.localScale = new Vector3(DOOR_WIDTH, DOOR_HEIGHT, WALL_THICKNESS / 2);

            Material doorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            doorMat.color = new Color(0.6f, 0.4f, 0.2f); // 나무 색상
            doorPanel.GetComponent<Renderer>().material = doorMat;

            // 문짝의 Collider 제거 (물리 충돌 방지 - 루트의 BoxCollider만 사용)
            Object.DestroyImmediate(doorPanel.GetComponent<Collider>());

            // 문 손잡이
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Handle";
            handle.transform.SetParent(doorPivot.transform);
            handle.transform.localPosition = new Vector3(DOOR_WIDTH * 0.8f, DOOR_HEIGHT / 2, -WALL_THICKNESS / 4);
            handle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            Material handleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            handleMat.color = new Color(0.8f, 0.7f, 0.3f); // 금색 손잡이
            handleMat.SetFloat("_Metallic", 0.8f);
            handleMat.SetFloat("_Smoothness", 0.9f);
            handle.GetComponent<Renderer>().material = handleMat;

            // 손잡이의 Collider 제거 (물리 충돌 방지)
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            // BoxCollider 추가 (상호작용용 - Door 스크립트가 열림/닫힘에 따라 제어)
            BoxCollider collider = doorRoot.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, DOOR_HEIGHT / 2, 0);
            collider.size = new Vector3(DOOR_WIDTH + 0.5f, DOOR_HEIGHT, 0.5f);
            // isTrigger는 Door.cs에서 상태에 따라 제어됨 (닫힘=false, 열림=true)

            // Door 스크립트 추가
            Door doorScript = doorRoot.AddComponent<Door>();
            // doorPivot 할당은 Door.cs의 Awake에서 자동으로 찾음

            // WorldSpaceInteractionPrompt 추가
            WorldSpaceInteractionPrompt prompt = doorRoot.AddComponent<WorldSpaceInteractionPrompt>();

            // Layer 설정
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer == -1)
            {
                Debug.LogWarning("[HouseGenerator] 'Interactable' Layer가 없습니다. Layer를 추가해주세요.");
            }
            else
            {
                doorRoot.layer = interactableLayer;
            }
        }
    }
}
