# 상호작용 문제 해결 가이드 (Interaction Troubleshooting Guide)

이 문서는 게임 내 상호작용(문 열기, 재료 수집, 미니게임 등)이 작동하지 않을 때 문제를 진단하고 해결하는 방법을 설명합니다.

---

## 목차

1. [상호작용이 전혀 작동하지 않을 때](#1-상호작용이-전혀-작동하지-않을-때)
2. [E키를 눌러도 반응이 없을 때](#2-e키를-눌러도-반응이-없을-때)
3. [문이 열리지 않거나 통과할 수 없을 때](#3-문이-열리지-않거나-통과할-수-없을-때)
4. [재료를 수집할 수 없을 때](#4-재료를-수집할-수-없을-때)
5. [미니게임이 시작되지 않을 때](#5-미니게임이-시작되지-않을-때)
6. [한글 텍스트가 깨져서 보일 때](#6-한글-텍스트가-깨져서-보일-때)
7. [디버그 로그 활용하기](#7-디버그-로그-활용하기)

---

## 1. 상호작용이 전혀 작동하지 않을 때

### 증상
- 문, 재료 근처에 가도 아무 반응 없음
- 상호작용 프롬프트(E키 안내)가 보이지 않음
- E키를 눌러도 아무 일도 일어나지 않음

### 원인 및 해결방법

#### 1.1 Layer 설정 확인

**확인 사항:**
1. **Project Settings에서 Interactable Layer 생성 확인**
   - `Edit → Project Settings → Tags and Layers`
   - `Layers` 섹션에서 빈 슬롯에 `Interactable` Layer가 있는지 확인
   - 없다면 추가

2. **상호작용 오브젝트의 Layer 설정**
   - Hierarchy에서 문(Door) 또는 재료(CollectibleIngredient) 오브젝트 선택
   - Inspector 창 상단의 `Layer` 드롭다운 확인
   - `Interactable` Layer로 설정되어 있는지 확인
   - 설정되어 있지 않다면 `Interactable`로 변경

3. **Interactor 컴포넌트의 Layer Mask 설정**
   - Hierarchy에서 Player 오브젝트 선택
   - `Interactor` 컴포넌트 찾기
   - `Interaction Layer` 필드 확인
   - `Interactable` Layer가 체크되어 있는지 확인

#### 1.2 Collider 설정 확인

**확인 사항:**
1. **상호작용 오브젝트에 Collider가 있는지 확인**
   - 문(Door): 루트 오브젝트에 `BoxCollider` 필요
   - 재료(CollectibleIngredient): `Collider` 컴포넌트 필요

2. **Collider가 활성화되어 있는지 확인**
   - Collider 컴포넌트의 체크박스가 켜져 있어야 함

3. **Collider 크기 확인**
   - Scene 뷰에서 Collider 범위(녹색 와이어프레임)가 오브젝트를 적절히 감싸고 있는지 확인

#### 1.3 Interactor 설정 확인

**확인 사항:**
1. **Player에 Interactor 컴포넌트가 있는지 확인**
2. **Detection Radius(감지 범위) 설정**
   - 기본값: `2.0`
   - 너무 작으면 가까이 가야만 감지됨
   - Scene 뷰에서 Cyan 색상의 와이어 구(Wire Sphere)로 범위 확인 가능

---

## 2. E키를 눌러도 반응이 없을 때

### 증상
- 상호작용 프롬프트는 보이는데 E키가 작동하지 않음

### 해결방법

#### 2.1 Input 설정 확인
1. Interactor 컴포넌트의 `Interact Key` 확인
2. 기본값은 `E` (KeyCode.E)
3. 키보드 레이아웃에 따라 다른 키로 변경 가능

#### 2.2 IInteractable 컴포넌트 확인
1. 상호작용 오브젝트에 `IInteractable` 인터페이스를 구현한 스크립트가 있는지 확인
   - Door의 경우: `Door.cs`
   - 재료의 경우: `CollectibleIngredient.cs`

2. `CanInteract` 속성이 `true`를 반환하는지 확인
   - Door: `_isAnimating`이 false여야 함
   - Ingredient: `_collected`가 false여야 함

---

## 3. 문이 열리지 않거나 통과할 수 없을 때

### 증상
- E키를 누르면 문이 열리는데, 플레이어가 통과할 수 없음
- 보이지 않는 벽에 막힌 것처럼 행동함

### 원인
문짝(DoorPanel)이나 손잡이(Handle)에 물리 Collider가 남아있어서 플레이어를 막고 있습니다.

### 해결방법

#### 3.1 자동 수정 (권장)
최신 버전의 코드에서는 문제가 자동으로 수정됩니다:
1. `HouseGenerator.cs`: 문 생성 시 DoorPanel과 Handle의 Collider를 자동으로 제거
2. `Door.cs`: Awake()에서 자식 오브젝트의 Collider를 자동으로 제거

**기존 씬의 문 수정:**
1. Scene에 이미 생성된 문이 있다면 삭제
2. HouseGenerator를 다시 실행하여 새 문 생성
3. 또는 아래 수동 수정 방법 사용

#### 3.2 수동 수정
1. Hierarchy에서 Door 오브젝트 확장
2. `DoorPivot` → `DoorPanel` 선택
3. `Collider` 컴포넌트가 있다면 제거 (우클릭 → Remove Component)
4. `DoorPivot` → `Handle` 선택
5. `Collider` 컴포넌트가 있다면 제거

#### 3.3 문 루트의 BoxCollider 확인
1. Door 루트 오브젝트 선택
2. `BoxCollider` 컴포넌트 확인
3. **중요**: `Is Trigger`가 **반드시 체크**되어 있어야 함
4. 체크되어 있지 않다면 체크

**Is Trigger의 역할:**
- `Is Trigger = true`: 감지만 하고 물리적으로 막지 않음 (상호작용용)
- `Is Trigger = false`: 물리적으로 막음 (벽처럼 행동)

---

## 4. 재료를 수집할 수 없을 때

### 증상
- 재료 근처에 가도 상호작용 프롬프트가 보이지 않음
- E키를 눌러도 수집되지 않음

### 해결방법

#### 4.1 CollectibleIngredient 컴포넌트 확인
1. Hierarchy에서 재료 오브젝트 선택
2. `CollectibleIngredient` 스크립트 확인
3. `Ingredient Id` 설정 확인 (Sugar, Egg, Flour 등)

#### 4.2 IngredientInventory 싱글톤 확인
1. Console 창에서 에러 메시지 확인
2. `"IngredientInventory를 찾을 수 없습니다!"` 에러가 있다면:
   - Scene에 IngredientInventory가 있는 GameObject 생성
   - 또는 기존 GameObject에 컴포넌트 추가

#### 4.3 이미 수집된 상태인지 확인
1. 재료는 한 번만 수집 가능
2. 이미 수집했다면 `_collected = true` 상태
3. 게임을 재시작하거나 씬을 다시 로드

---

## 5. 미니게임이 시작되지 않을 때

### 증상
- 재료와 상호작용했는데 미니게임 UI가 나타나지 않음
- 바로 재료가 수집되거나 아무 반응이 없음

### 해결방법

#### 5.1 CollectibleIngredient 설정 확인
1. Hierarchy에서 재료 오브젝트 선택
2. `CollectibleIngredient` 컴포넌트 확인
3. **Mini Game 섹션:**
   - `Requires Mini Game` 체크박스가 **체크**되어 있는지 확인
   - `Mini Game Type`이 올바르게 설정되어 있는지 확인 (예: Sugar)

#### 5.2 MiniGameManager 존재 확인
1. Hierarchy에서 `MiniGameManager` 오브젝트 검색
2. 없다면:
   - 빈 GameObject 생성 (이름: `MiniGameManager`)
   - `MiniGameManager.cs` 컴포넌트 추가

3. MiniGameManager 컴포넌트 설정:
   - `Mini Game Canvas` 참조 설정
   - `Sugar Mini Game Panel` 참조 설정

#### 5.3 미니게임 UI 설정 확인
1. Canvas 오브젝트가 Scene에 있는지 확인
2. Canvas 아래에 미니게임 패널이 있는지 확인
   - 예: `SugarMiniGamePanel`
3. 패널에 `SugarPouringMiniGame.cs` 스크립트가 있는지 확인

#### 5.4 Console 로그 확인
재료와 상호작용했을 때 Console에 다음 메시지가 나타나야 함:
```
[CollectibleIngredient] Interact called for Sugar
[CollectibleIngredient] Requires MiniGame: True
[CollectibleIngredient] Starting mini game: Sugar
```

메시지가 없거나 에러가 있다면 해당 에러 메시지를 읽고 대응

---

## 6. 한글 텍스트가 깨져서 보일 때

### 증상
- "문 열기 [E]"가 "... [E]"로 표시됨
- 상호작용 프롬프트 텍스트가 점(...)으로 보임

### 원인
TextMeshPro의 기본 폰트(LiberationSans SDF)에 한글 글리프(문자)가 포함되어 있지 않습니다.

### 임시 해결 (현재 적용됨)
코드를 영어로 변경하여 임시 해결:
- "문 열기 [E]" → "Open Door [E]"
- "문 닫기 [E]" → "Close Door [E]"
- "재료 수집 [E]" → "Collect [Ingredient] [E]"

### 근본적 해결 (권장)

#### 6.1 한글 폰트 다운로드
1. Google Fonts에서 **Noto Sans KR** 다운로드
   - URL: https://fonts.google.com/noto/specimen/Noto+Sans+KR
2. `.ttf` 또는 `.otf` 파일을 프로젝트에 추가
   - 경로 예: `Assets/Fonts/NotoSansKR-Regular.ttf`

#### 6.2 TextMeshPro Font Asset 생성
1. Unity 메뉴: `Window → TextMeshPro → Font Asset Creator`
2. 설정:
   - **Source Font File**: 다운로드한 한글 폰트 선택
   - **Sampling Point Size**: `Auto Sizing`
   - **Padding**: `5`
   - **Packing Method**: `Fast`
   - **Atlas Resolution**: `2048 x 2048` (또는 더 크게)
   - **Character Set**: `Custom Characters`
   - **Custom Character List**에 사용할 한글 입력:
     ```
     문열기닫기재료수집잠겨있습니다설탕계란밀가루버터딸기
     [E]키로
     ```
   - 또는 `Characters from File`로 `.txt` 파일에서 로드
3. **Generate Font Atlas** 클릭
4. **Save** 버튼으로 Font Asset 저장
   - 경로 예: `Assets/Fonts/NotoSansKR SDF.asset`

#### 6.3 Font Asset 적용

**방법 1: 코드에서 할당 (권장)**

`WorldSpaceInteractionPrompt.cs` 수정:
```csharp
[Header("Font")]
[SerializeField] private TMP_FontAsset koreanFont; // 인스펙터에서 할당

private void CreatePromptUI()
{
    // ... 기존 코드 ...

    _textMesh = textObject.AddComponent<TextMeshPro>();
    _textMesh.text = "상호작용 [E]";
    _textMesh.fontSize = fontSize;
    _textMesh.color = textColor;
    _textMesh.alignment = TextAlignmentOptions.Center;
    
    // 한글 폰트 적용
    if (koreanFont != null)
    {
        _textMesh.font = koreanFont;
    }
    
    // ... 나머지 코드 ...
}
```

**방법 2: TextMeshPro 기본 폰트 변경**

1. `Edit → Project Settings → TextMeshPro → Settings`
2. `Default Font Asset` 항목을 생성한 한글 폰트로 변경

#### 6.4 한글 메시지 복원

Font Asset이 적용되면 다음 파일들의 메시지를 한글로 다시 변경:
- `Door.cs`: "Open Door [E]" → "문 열기 [E]"
- `CollectibleIngredient.cs`: "Collect [X] [E]" → "[X] 수집 [E]"

---

## 7. 디버그 로그 활용하기

### 로그 보는 방법
1. Unity Editor에서 게임 실행
2. `Window → General → Console` 열기
3. 플레이하면서 상호작용 시도
4. Console에 나타나는 메시지 확인

### 주요 디버그 로그 메시지

#### Interactor (Player/Interactor.cs)
```
[Interactor] Found 2 colliders in range
→ 플레이어 주변에서 2개의 상호작용 가능한 오브젝트 감지

[Interactor] EntranceDoor has no IInteractable component
→ EntranceDoor에 IInteractable 컴포넌트가 없음 (문제!)

[Interactor] Nearest interactable: Sugar at distance 1.23m
→ 가장 가까운 상호작용 대상: Sugar, 거리 1.23미터

[Interactor] Interacting with: Collect Sugar [E]
→ E키를 눌러 Sugar와 상호작용 시작
```

#### CollectibleIngredient
```
[CollectibleIngredient] Interact called for Sugar
→ Sugar와 상호작용 시작

[CollectibleIngredient] Requires MiniGame: True
→ 이 재료는 미니게임 필요

[CollectibleIngredient] Starting mini game: Sugar
→ Sugar 미니게임 시작

[CollectibleIngredient] MiniGameManager를 찾을 수 없습니다!
→ 에러! MiniGameManager가 Scene에 없음
```

#### MiniGameManager
```
[MiniGameManager] Instance created and initialized
→ MiniGameManager 싱글톤 생성 완료

[MiniGameManager] Starting Sugar mini game
→ Sugar 미니게임 시작

[MiniGameManager] Pausing player and camera
→ 플레이어 입력/카메라 일시정지
```

#### Door
```
[Door] 문 열기
→ 문 열기 시작

[Door] 문이 열렸습니다.
→ 문 열림 완료

[Door] DoorPanel의 Collider 제거 (물리 충돌 방지)
→ 자동으로 불필요한 Collider 제거
```

### 문제별 진단 팁

| 증상 | 확인할 로그 | 예상 원인 |
|------|------------|----------|
| 상호작용 감지 안됨 | `[Interactor] Found 0 colliders` | Layer 설정 오류 |
| E키 반응 없음 | 로그 전혀 없음 | Interactor 컴포넌트 없음 |
| 미니게임 안뜸 | `"MiniGameManager를 찾을 수 없습니다!"` | MiniGameManager 없음 |
| 문 통과 불가 | `"Collider 제거"` 로그 없음 | 오래된 Door 오브젝트 |

---

## 추가 도움

### Scene 초기화
문제가 계속되면 깨끗한 상태에서 다시 시작:

1. **기존 오브젝트 정리**
   - Hierarchy에서 모든 Door, Ingredient 오브젝트 삭제
   
2. **HouseGenerator 재실행**
   - 새로운 집 구조 생성
   - 최신 코드로 Door와 Ingredient 자동 배치

3. **필수 오브젝트 확인**
   - Player (Interactor 컴포넌트 포함)
   - MiniGameManager
   - IngredientInventory
   - Canvas (미니게임 UI)

### 참고 문서
- [House Generator 가이드](House_Generator_Complete_Guide.md)
- [Mini Game 설정 가이드](MiniGame_Setup_Guide.md)
- [Mini Game 빠른 시작](MiniGame_Quick_Start.md)

---

**작성일**: 2026-02-02
**버전**: 1.0
**대상**: Birthday Cake Quest 프로젝트

