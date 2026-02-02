# 🏠 House Generator 완벽 가이드 (2룸 구조)

## 📋 목차
1. [개요](#개요)
2. [사용 방법](#사용-방법)
3. [집 구조 설명](#집-구조-설명)
4. [재료 배치](#재료-배치)
5. [문제 해결](#문제-해결)

---

## 개요

`HouseGenerator`는 **안정적인 2룸 구조의 집**을 자동으로 생성하는 Unity 에디터 도구입니다.

### ✨ 주요 특징
- ✅ **안정적인 구조**: 모든 벽이 명확하게 연결됨
- ✅ **2룸 구조**: 침실 2개 + 거실/주방
- ✅ **가구 자동 배치**: 소파, TV, 침대 등
- ✅ **문 자동 생성**: E키로 열고 닫을 수 있는 상호작용 문
- ✅ **Floating UI**: "문 열기 [E]" 프롬프트 자동 표시
- ✅ **재료 자동 배치**: 5가지 케이크 재료
- ✅ **하얀 대리석 바닥**: 고급스러운 비주얼

---

## 사용 방법

### 1️⃣ 집 생성

Unity 에디터 상단 메뉴에서:

```
Tools → Generate House + Ingredients
```

또는 개별 실행:

```
Tools → Generate House          (집만 생성)
Tools → Place Ingredients       (재료만 배치)
```

### 2️⃣ 생성 결과 확인

Scene Hierarchy에 다음과 같은 구조가 생성됩니다:

```
House
├── LivingRoom (거실/주방)
│   ├── Floor
│   ├── Wall_Left, Wall_Right, Wall_Front_*, Wall_Back_*
│   ├── KitchenCounter
│   └── EntranceDoor 🚪 (입구 문)
│       ├── DoorPivot
│       └── DoorPanel
├── Room1 (침실1)
│   ├── Floor
│   ├── Wall_*
│   ├── Bed
│   └── Room1Door 🚪 (방1 문)
│       ├── DoorPivot
│       └── DoorPanel
├── Room2 (침실2)
│   ├── Floor
│   ├── Wall_*
│   ├── Bed
│   └── Room2Door 🚪 (방2 문)
│       ├── DoorPivot
│       └── DoorPanel
├── Hallway (복도)
│   └── Floor
├── Sofa (소파)
│   ├── Seat
│   ├── Back
│   └── SofaSitPosition
├── TV
│   ├── Stand
│   └── TV_Screen
└── Ingredient_* (재료들)
```

---

## 집 구조 설명

### 📐 레이아웃

```
        입구
         ↓
    ┌─────────┐
    │  현관   │
    └─────────┘
         │
    ┌────┴────┐
    │ 복  도  │
    ├───┬────┤
    │방1 │방2 │
    │🛏️│🛏️│
    ├───┴────┤
    │        │
    │ 거실   │
    │ 🛋️📺│
    │        │
    │ 주방   │
    └────────┘
```

### 📏 크기 (단위: 미터)

| 공간 | 크기 (가로 x 세로) |
|------|-------------------|
| **거실/주방** | 10m x 10m |
| **침실1** | 4m x 4m |
| **침실2** | 4m x 4m |
| **복도** | 2m x 2m |
| **벽 두께** | 0.2m |
| **벽 높이** | 2.5m |

### 🎨 재질

- **바닥**: 하얀 대리석 (광택 있음)
- **벽**: 밝은 회색
- **가구**: 각 가구별 색상 (소파: 파랑, 침대: 흰색 등)

---

## 재료 배치

### 🍰 재료 위치

| 재료 | 위치 | 좌표 |
|------|------|------|
| **밀가루 (Flour)** | 침실1 | (-3, 0.5, 6) |
| **설탕 (Sugar)** | 침실2 | (3, 0.5, 6) |
| **계란 (Egg)** | 거실 왼쪽 | (-3, 0.5, 0) |
| **버터 (Butter)** | 거실 오른쪽 | (3, 0.5, 0) |
| **딸기 (Strawberry)** | 거실 앞쪽 | (0, 0.5, -3) |

### 📦 필요한 Prefab

재료 Prefab들이 다음 경로에 있어야 합니다:

```
Assets/Prefabs/
├── Ingredient_Flour.prefab
├── Ingredient_Sugar.prefab
├── Ingredient_Egg.prefab
├── Ingredient_Butter.prefab
└── Ingredient_Strawberry.prefab
```

---

## 문제 해결

### ❌ "House를 찾을 수 없습니다"

**원인**: 재료를 배치하려는데 House가 없음

**해결**:
1. `Tools → Generate House`를 먼저 실행
2. 또는 `Tools → Generate House + Ingredients`로 한 번에 실행

---

### ❌ "Prefab을 찾을 수 없음" 경고

**원인**: 재료 Prefab 파일이 없음

**해결**:
1. Scene에 이미 있는 재료 오브젝트를 확인
2. 해당 오브젝트를 `Assets/Prefabs/` 폴더로 드래그해서 Prefab 생성
3. 파일 이름이 정확한지 확인 (예: `Ingredient_Flour.prefab`)

---

### ❌ 벽이 연결되지 않음

**원인**: 이전 버전의 코드 사용

**해결**:
1. Scene에서 기존 House 삭제
2. `Tools → Generate House` 다시 실행
3. 최신 코드로 재생성됨

---

### ❌ 바닥이 깜빡임 (Z-fighting)

**원인**: 바닥이 y=0에 정확히 위치

**해결**:
- 이미 해결됨! `FLOOR_Y = 0.001f`로 약간 올려서 생성됩니다.

---

### ❌ 플레이어가 벽을 통과함

**원인**: Collider가 없거나 Physics 레이어 설정 문제

**해결**:
1. 생성된 벽 선택
2. `MeshCollider` 또는 `BoxCollider` 확인
3. 필요 시 수동으로 추가

---

## 코드 커스터마이징

### 집 크기 변경

`Assets/Scripts/Tools/HouseGenerator.cs` 파일에서:

```csharp
// 거실 크기 변경 (CreateLivingRoom 메소드)
CreateFloor(room.transform, "Floor", new Vector3(0, 0, 0), 
    new Vector3(12 * UNIT, 12 * UNIT)); // 10 → 12로 변경

// 침실 크기 변경 (CreateRoom1, CreateRoom2 메소드)
CreateFloor(room.transform, "Floor", Vector3.zero, 
    new Vector3(5 * UNIT, 5 * UNIT)); // 4 → 5로 변경
```

### 벽 색상 변경

`CreateWall` 메소드에서:

```csharp
Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
mat.color = new Color(0.9f, 0.85f, 0.8f); // 베이지색으로 변경
```

### 바닥 재질 변경

`CreateFloor` 메소드에서:

```csharp
mat.color = new Color(0.8f, 0.7f, 0.6f); // 나무 색상
mat.SetFloat("_Smoothness", 0.3f); // 광택 줄이기
```

---

## 🚪 자동 생성된 문

집을 생성하면 다음 문들이 자동으로 생성됩니다:

### 문 위치

| 문 이름 | 위치 | 상호작용 |
|---------|------|----------|
| **EntranceDoor** | 거실 입구 (0, 0, -5) | E키로 열고 닫기 |
| **Room1Door** | 침실1 입구 | E키로 열고 닫기 |
| **Room2Door** | 침실2 입구 | E키로 열고 닫기 |

### 문 기능

- ✅ **E키 상호작용**: 플레이어가 가까이 가면 자동으로 프롬프트 표시
- ✅ **Floating UI**: "문 열기 [E]" 또는 "문 닫기 [E]" 텍스트가 문 위에 떠다님
- ✅ **부드러운 애니메이션**: 90도 회전하며 열고 닫힘
- ✅ **Door.cs 스크립트**: 자동으로 추가됨
- ✅ **WorldSpaceInteractionPrompt**: 자동으로 추가됨
- ✅ **Layer 설정**: "Interactable" Layer로 자동 설정

### 문 상세 정보

자세한 내용은 **[문 상호작용 가이드](Door_Interaction_Guide.md)**를 참고하세요.

---

## 추가 기능 아이디어

### 🪟 창문 추가하기

외벽에 창문 추가 (수동 작업):

1. `GameObject → 3D Object → Quad` 생성
2. 재질: 반투명 파란색
3. 벽에 약간 앞으로 배치 (Z-fighting 방지)

### 💡 조명 추가하기

각 방에 천장 조명 추가:

1. `GameObject → Light → Point Light` 생성
2. 위치: `y = 2.3` (천장 아래)
3. Range: `5`
4. Color: 따뜻한 흰색

---

## 참고 자료

- [Unity Primitives 문서](https://docs.unity3d.com/Manual/PrimitiveObjects.html)
- [Unity Materials 문서](https://docs.unity3d.com/Manual/Materials.html)
- [URP Lit Shader](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/lit-shader.html)

---

**마지막 업데이트**: 2026-02-02  
**버전**: 2.0 (안정적인 2룸 구조)
