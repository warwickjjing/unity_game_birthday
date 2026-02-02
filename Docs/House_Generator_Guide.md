# 🏠 집 자동 생성 가이드

이 문서는 도면 기반 집 자동 생성 도구 사용법을 설명합니다.

## 📋 준비사항

### ✅ 필수 Prefab
다음 재료 Prefab이 있어야 합니다:
- `Assets/Prefabs/Ingredient_Flour.prefab`
- `Assets/Prefabs/Ingredient_Sugar.prefab`
- `Assets/Prefabs/Ingredient_Egg.prefab`
- `Assets/Prefabs/Ingredient_Butter.prefab`
- `Assets/Prefabs/Ingredient_Strawberry.prefab`

### ✅ URP 설정
- Universal Render Pipeline Asset이 설정되어 있어야 합니다

---

## 🚀 사용 방법

### Option 1: 한 번에 모두 생성 (추천) ⭐

1. Unity 상단 메뉴에서:
   ```
   Tools → Birthday Cake Quest → Generate House + Ingredients
   ```

2. 확인 대화상자에서 **"생성"** 클릭

3. 완료! 🎉
   - Hierarchy에 **"House"** GameObject 생성됨
   - 거실, 침실 2개, 화장실 2개, 발코니, 주방이 자동 생성
   - 재료 5개가 각 방에 배치됨

### Option 2: 단계별 생성

#### Step 1: 집만 생성
```
Tools → Birthday Cake Quest → Generate House
```

#### Step 2: 재료 배치
```
Tools → Birthday Cake Quest → Place Ingredients
```

---

## 🏗️ 생성되는 구조

```
House
├── LivingRoom (거실)
│   ├── Floor (주황색 마루)
│   ├── Wall_North, South, East, West (벽 4개)
│   ├── Sofa (소파)
│   │   ├── Seat
│   │   ├── Back
│   │   └── SofaSitPosition
│   └── TV (티비)
│       ├── Stand
│       └── TV_Screen
│
├── Bedroom_Left (왼쪽 침실)
│   ├── Floor (베이지색)
│   └── Walls (벽 4개)
│
├── Bedroom_Right (오른쪽 침실)
│   ├── Floor (베이지색)
│   └── Walls (벽 4개)
│
├── Bathroom_Upper (위쪽 화장실)
│   ├── Floor (흰색 타일)
│   └── Walls (벽 4개)
│
├── Bathroom_Lower (아래쪽 화장실)
│   ├── Floor (흰색 타일)
│   └── Walls (벽 4개)
│
├── Balcony (발코니)
│   ├── Floor (회색)
│   └── Railing (난간)
│
└── Kitchen (주방)
    ├── Counter (싱크대)
    └── Cooktop (쿡탑)
```

---

## 🍰 재료 배치 위치

| 재료 | 위치 | 좌표 (대략) |
|------|------|-------------|
| 🌾 Flour (밀가루) | 왼쪽 침실 | (-15, 0.5, 15) |
| 🍚 Sugar (설탕) | 오른쪽 침실 | (15, 0.5, 15) |
| 🥚 Egg (계란) | 거실 왼쪽 | (-7.5, 0.5, 0) |
| 🧈 Butter (버터) | 주방 | (12, 0.5, 0) |
| 🍓 Strawberry (딸기) | 거실 중앙 | (0, 0.5, -4.5) |

---

## 🔧 생성 후 설정

### 1. 소파 인터랙션 추가

```
1. Hierarchy → House → LivingRoom → Sofa 선택
2. Add Component → Sofa Interactable
3. 설정:
   - Cutscene Controller: EndingCutscene 드래그
   - Inventory: GameSystems/IngredientInventory 드래그
   - Interact Prompt: "소파에 앉기 [E]"
```

### 2. 플레이어 배치

```
1. Player GameObject 선택
2. Position: (0, 0, -10) - 거실 남쪽 입구
3. Rotation: (0, 0, 0)
```

### 3. 카메라 설정

```
1. Main Camera 선택
2. Isometric Follow Camera 컴포넌트:
   - Target: Player
   - Offset: (0, 15, -12)
   - Angle X: 50
```

### 4. EndingCutscene 설정

```
1. EndingCutscene GameObject 선택
2. Ending Cutscene Controller:
   - Sofa Sit Position: House/LivingRoom/Sofa/SofaSitPosition 드래그
```

---

## 📏 집 크기 조정

집이 너무 크거나 작다면 `HouseGenerator.cs` 수정:

```csharp
// 라인 11
private const float SCALE = 1.5f; // 이 값을 조정 (기본: 1.5)
```

- `SCALE = 1.0f`: 작은 집
- `SCALE = 1.5f`: 중간 (기본)
- `SCALE = 2.0f`: 큰 집

수정 후 다시 생성하세요.

---

## 🎨 방 색상 커스터마이징

### 거실 바닥 색상 변경
`HouseGenerator.cs` → `CreateLivingRoom()` 메서드:

```csharp
floorMat.color = new Color(0.8f, 0.6f, 0.3f); // RGB 값 변경
```

### 침실 바닥 색상 변경
`CreateBedroom()` 메서드:

```csharp
floorMat.color = new Color(0.9f, 0.85f, 0.7f); // 베이지색
```

---

## ⚠️ 문제 해결

### 문제: 재료가 배치되지 않음
**원인**: Prefab 파일을 찾을 수 없음

**해결**:
1. Project → Assets/Prefabs/ 폴더 확인
2. 재료 Prefab 5개가 있는지 확인
3. 파일명이 정확한지 확인:
   - `Ingredient_Flour.prefab`
   - `Ingredient_Sugar.prefab`
   - `Ingredient_Egg.prefab`
   - `Ingredient_Butter.prefab`
   - `Ingredient_Strawberry.prefab`

### 문제: 집이 검은색/마젠타색으로 보임
**원인**: URP 설정 누락

**해결**:
1. `Edit → Project Settings → Graphics`
2. `Scriptable Render Pipeline Settings`에 URP Asset 할당
3. 집 다시 생성

### 문제: 소파 인터랙션이 안 됨
**원인**: SofaInteractable 컴포넌트 미추가

**해결**:
1. House → LivingRoom → Sofa 선택
2. Add Component → Sofa Interactable
3. 필요한 필드 연결 (위 "생성 후 설정" 참고)

---

## 🎯 다음 단계

집 생성 후:

1. ✅ 집 구조 생성됨
2. ✅ 재료 배치됨
3. ⬜ Player 배치 및 설정
4. ⬜ Camera 설정
5. ⬜ Sofa 인터랙션 설정
6. ⬜ EndingCutscene 연결
7. ⬜ Timeline 설정 (TV 영상)
8. ⬜ 크레딧 UI 설정

**전체 가이드**: [`Docs/Ending_Cutscene_Setup_Guide.md`](Ending_Cutscene_Setup_Guide.md)

---

## 🛠️ 고급 기능

### 커스텀 재료 배치 위치

`HouseGenerator.cs` → `PlaceIngredients()` 메서드에서 `positions` 배열 수정:

```csharp
Vector3[] positions = new Vector3[]
{
    new Vector3(-10 * SCALE, 0.5f, 10 * SCALE),  // Flour 위치
    new Vector3(10 * SCALE, 0.5f, 10 * SCALE),   // Sugar 위치
    new Vector3(-5 * SCALE, 0.5f, 0),            // Egg 위치
    new Vector3(8 * SCALE, 0.5f, 0),             // Butter 위치
    new Vector3(0, 0.5f, -3 * SCALE)             // Strawberry 위치
};
```

### 추가 방 생성

`Generate()` 메서드에 새 방 추가:

```csharp
CreateBedroom(house.transform, "Bedroom_Extra", new Vector3(20, 0, 0), new Vector3(5, 2.5f, 4));
```

---

## 📚 참고 문서

- [엔딩 컷씬 설정 가이드](Ending_Cutscene_Setup_Guide.md)
- [씬 설정 가이드](Scene_Setup_Guide.md)
- [UniVRM 설정 가이드](UniVRM_Setup_Guide.md)

---

## ✨ 팁

- 💡 집을 생성한 후 Scene 뷰에서 `F` 키를 눌러 House에 포커스
- 💡 재료 위치가 마음에 안 들면 Inspector에서 직접 이동 가능
- 💡 벽 색상, 바닥 색상은 생성 후에도 Material에서 변경 가능
- 💡 Prefab으로 저장하면 다른 씬에서도 재사용 가능

---

**Happy Building! 🏠✨**

