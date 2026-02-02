# 미니게임 Unity Editor 설정 가이드

이 문서는 미니게임 시스템을 Unity Editor에서 설정하는 방법을 단계별로 설명합니다.

---

## 목차

1. [MiniGameManager 오브젝트 생성](#1-minigamemanager-오브젝트-생성)
2. [Canvas 및 UI 설정](#2-canvas-및-ui-설정)
3. [Sugar 미니게임 UI 설정](#3-sugar-미니게임-ui-설정)
4. [참조 연결 확인](#4-참조-연결-확인)
5. [재료 오브젝트 설정](#5-재료-오브젝트-설정)
6. [테스트 방법](#6-테스트-방법)

---

## 1. MiniGameManager 오브젝트 생성

### 1.1 새 GameObject 생성
```
Hierarchy 우클릭 → Create Empty
이름을 "MiniGameManager"로 변경
```

### 1.2 MiniGameManager 컴포넌트 추가
```
Inspector에서 Add Component 클릭
"MiniGameManager" 검색 후 추가
```

### 1.3 DontDestroyOnLoad 확인
- MiniGameManager는 자동으로 DontDestroyOnLoad 설정됨
- 씬 전환 시에도 유지됨

---

## 2. Canvas 및 UI 설정

### 2.1 Canvas 생성
```
Hierarchy 우클릭 → UI → Canvas
이름을 "MiniGameCanvas"로 변경
```

### 2.2 Canvas 설정
Inspector에서 다음 설정 확인:

**Canvas 컴포넌트:**
- Render Mode: `Screen Space - Overlay`
- Pixel Perfect: ☑ (체크)

**Canvas Scaler 컴포넌트:**
- UI Scale Mode: `Scale With Screen Size`
- Reference Resolution: `1920 x 1080`
- Match: `0.5` (Width와 Height 중간)

**Graphic Raycaster 컴포넌트:**
- Ignore Reversed Graphics: ☑
- Blocking Objects: `None`

### 2.3 Canvas 초기 상태
- Canvas GameObject의 체크박스를 **해제**하여 초기에 비활성화
- MiniGameManager가 자동으로 활성화/비활성화 제어

---

## 3. Sugar 미니게임 UI 설정

### 3.1 Panel 생성
```
MiniGameCanvas 우클릭 → UI → Panel
이름을 "SugarMiniGamePanel"로 변경
```

### 3.2 SugarMiniGamePanel 설정

**RectTransform:**
- Anchor: Stretch (양쪽 끝)
- Left: 0, Top: 0, Right: 0, Bottom: 0
- 전체 화면을 덮도록 설정

**Image 컴포넌트:**
- Color: 반투명 검정 (R:0, G:0, B:0, A:180)
- 배경 반투명 효과

### 3.3 UI 요소 추가

SugarMiniGamePanel 아래에 다음 UI 요소들을 생성하세요:

#### A. 타이틀 텍스트
```
SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
이름: "TitleText"
```
- Text: "Sugar Pouring Mini-Game"
- Font Size: 60
- Alignment: Center, Top
- Color: White
- Position: (0, -100, 0) 상단 중앙

#### B. 게이지 배경
```
SugarMiniGamePanel 우클릭 → UI → Image
이름: "GaugeBackground"
```
- Width: 100, Height: 400
- Position: 화면 중앙 약간 왼쪽
- Color: Dark Gray (R:50, G:50, B:50)

#### C. 게이지 Fill
```
GaugeBackground 우클릭 → UI → Image
이름: "GaugeFill"
```
- Anchor: Bottom Left → Top Right (Stretch)
- Color: Yellow (R:255, G:255, B:0)
- Image Type: `Filled`
- Fill Method: `Vertical`
- Fill Amount: 0

#### D. 타겟 영역 표시
```
GaugeBackground 우클릭 → UI → Image
이름: "TargetRange"
```
- Width: 120, Height: 80
- Position: 게이지 중간 어딘가
- Color: Green (투명도 100)
- 타겟 범위를 나타냄

#### E. 타이머 텍스트
```
SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
이름: "TimerText"
```
- Text: "Time: 10.0"
- Font Size: 40
- Alignment: Center
- Position: 화면 상단 우측

#### F. 안내 텍스트
```
SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
이름: "InstructionText"
```
- Text: "Hold Mouse or Spacebar to pour sugar!"
- Font Size: 30
- Alignment: Center
- Position: 화면 하단

#### G. 결과 패널 (ResultPanel)
```
SugarMiniGamePanel 우클릭 → UI → Panel
이름: "ResultPanel"
```
- 초기에는 비활성화
- 성공/실패 시 표시됨

**ResultPanel 아래 요소들:**

1. **ResultText** (TextMeshPro)
   - Text: "Success!" 또는 "Failed!"
   - Font Size: 80

2. **RetryButton** (Button - TextMeshPro)
   - Text: "Retry"
   - Width: 200, Height: 60

3. **CloseButton** (Button - TextMeshPro)
   - Text: "Close"
   - Width: 200, Height: 60

### 3.4 SugarPouringMiniGame 스크립트 추가

```
SugarMiniGamePanel 선택
Inspector → Add Component
"SugarPouringMiniGame" 검색 후 추가
```

### 3.5 SugarPouringMiniGame 스크립트 참조 연결

Inspector에서 다음 항목들을 드래그 앤 드롭으로 연결:

**UI References:**
- Gauge Fill: `GaugeFill` Image
- Timer Text: `TimerText` TextMeshPro
- Instruction Text: `InstructionText` TextMeshPro
- Result Panel: `ResultPanel` GameObject
- Result Text: `ResultText` TextMeshPro
- Retry Button: `RetryButton` Button
- Close Button: `CloseButton` Button

**Settings (기본값 확인):**
- Game Duration: `10` (10초)
- Fill Speed: `0.3` (게이지 상승 속도)
- Decrease Speed: `0.2` (게이지 하강 속도)
- Target Min: `0.4` (타겟 하한선)
- Target Max: `0.6` (타겟 상한선)
- Success Threshold: `1.5` (1.5초 유지 시 성공)

---

## 4. 참조 연결 확인

### 4.1 MiniGameManager 참조 연결

Hierarchy에서 `MiniGameManager` 선택 후 Inspector 확인:

**References:**
- Mini Game Canvas: `MiniGameCanvas` (드래그 앤 드롭)
- Sugar Mini Game Panel: `SugarMiniGamePanel` (드래그 앤 드롭)

**Game Objects (자동 연결됨, 확인만):**
- Player Controller: (자동 연결)
- Interactor: (자동 연결)
- Isometric Camera: (자동 연결)

> **참고**: Player Controller, Interactor, Isometric Camera는 Start()에서 자동으로 찾아서 연결됩니다. 수동으로 연결하고 싶다면 드래그 앤 드롭하세요.

---

## 5. 재료 오브젝트 설정

### 5.1 Sugar 재료 찾기

Hierarchy에서 `Sugar` 또는 `CollectibleIngredient` 검색

### 5.2 CollectibleIngredient 컴포넌트 설정

Inspector에서 다음 설정:

**Ingredient Info:**
- Ingredient Id: `Sugar`

**Mini Game:**
- Requires Mini Game: ☑ (체크)
- Mini Game Type: `Sugar`

**Interaction:**
- Interaction Radius: `1.5`

**Visual:**
- Destroy On Collect: ☑ (체크)

### 5.3 Layer 설정

- Layer: `Interactable`
- 만약 Interactable Layer가 없다면 생성:
  ```
  Edit → Project Settings → Tags and Layers
  Layers 섹션에서 빈 슬롯에 "Interactable" 추가
  ```

### 5.4 Collider 확인

Sugar 오브젝트에 Collider가 있는지 확인:
- BoxCollider, SphereCollider, CapsuleCollider 중 하나
- Is Trigger: ☑ (체크)

---

## 6. 테스트 방법

### 6.1 게임 실행 전 체크리스트

- [ ] MiniGameManager 오브젝트가 Scene에 있음
- [ ] MiniGameCanvas가 생성되어 있음
- [ ] SugarMiniGamePanel이 Canvas 아래에 있음
- [ ] SugarPouringMiniGame 스크립트가 Panel에 추가됨
- [ ] 모든 UI 참조가 연결됨
- [ ] MiniGameManager에 Canvas와 Panel이 연결됨
- [ ] Sugar 재료의 Requires Mini Game이 체크됨
- [ ] Sugar 재료의 Layer가 Interactable임

### 6.2 Console 로그 확인

게임 실행 시 다음 로그가 나타나야 합니다:

```
[MiniGameManager] Instance created and initialized
[MiniGameManager] PlayerController 자동 연결 완료
[MiniGameManager] Interactor 자동 연결 완료
[MiniGameManager] IsometricFollowCamera 자동 연결 완료
[MiniGameManager] MiniGame Canvas 초기 비활성화 완료
[MiniGameManager] SugarMiniGamePanel 할당 확인됨
```

만약 경고 메시지가 나타난다면 해당 항목을 수동으로 연결하세요.

### 6.3 상호작용 테스트

1. **플레이어를 Sugar 근처로 이동**
   - "Collect Sugar [E]" 프롬프트가 표시되어야 함

2. **E키를 눌러 상호작용**
   ```
   Console 로그:
   [Interactor] Interacting with: Collect Sugar [E]
   [CollectibleIngredient] Interact called for Sugar
   [CollectibleIngredient] Requires MiniGame: True
   [CollectibleIngredient] Starting mini game: Sugar
   [MiniGameManager] Sugar 미니게임 시작
   ```

3. **미니게임 UI 확인**
   - Canvas가 활성화되어야 함
   - SugarMiniGamePanel이 표시되어야 함
   - 타이틀, 게이지, 타이머 등 모든 UI가 보여야 함

4. **미니게임 플레이**
   - 마우스 버튼 또는 스페이스바를 홀드하여 게이지 채우기
   - 타이머가 카운트다운되어야 함
   - 타겟 범위에 1.5초 유지 시 성공

5. **결과 확인**
   - 성공 시: "Success!" 메시지 + 재료 수집
   - 실패 시: "Failed!" 메시지 + Retry/Close 버튼

### 6.4 문제 해결

**문제: 미니게임 UI가 나타나지 않음**
- Canvas가 활성화되어 있는지 확인 (초기에는 비활성화되어야 함)
- MiniGameManager에 Canvas가 연결되어 있는지 확인
- Console에서 에러 메시지 확인

**문제: E키를 눌러도 반응 없음**
- [상호작용 문제 해결 가이드](Troubleshooting_Interaction.md) 참고

**문제: "MiniGameManager를 찾을 수 없습니다!" 에러**
- Scene에 MiniGameManager GameObject가 있는지 확인
- MiniGameManager 스크립트가 추가되어 있는지 확인

**문제: 게이지가 움직이지 않음**
- SugarPouringMiniGame 스크립트의 UI 참조 확인
- Gauge Fill의 Image Type이 "Filled"로 설정되어 있는지 확인

---

## 추가 미니게임 설정

Sugar 외에 다른 재료의 미니게임을 추가하려면:

1. 새로운 Panel 생성 (예: `EggMiniGamePanel`)
2. 미니게임 스크립트 작성 (IMiniGame 인터페이스 구현)
3. MiniGameManager의 `CreateMiniGame()` 메서드에 case 추가
4. MiniGameManager Inspector에 새 Panel 참조 추가
5. 재료 오브젝트의 Mini Game Type 설정

---

## 참고 문서

- [상호작용 문제 해결 가이드](Troubleshooting_Interaction.md)
- [미니게임 시스템 가이드](MiniGame_Setup_Guide.md)
- [미니게임 빠른 시작](MiniGame_Quick_Start.md)

---

**작성일**: 2026-02-02
**버전**: 1.0
**대상**: Birthday Cake Quest 프로젝트

