# 🎮 미니게임 시스템 설정 가이드

## 📋 목차
1. [개요](#개요)
2. [UI 설정](#ui-설정)
3. [매니저 설정](#매니저-설정)
4. [재료에 미니게임 연결](#재료에-미니게임-연결)
5. [테스트](#테스트)
6. [문제 해결](#문제-해결)

---

## 개요

재료를 수집할 때 미니게임을 플레이하는 시스템입니다. 현재 **설탕 따르기 미니게임**이 구현되어 있습니다.

---

## UI 설정

### 1단계: MiniGame Canvas 생성

1. **Canvas 생성**
   ```
   Hierarchy → 우클릭 → UI → Canvas
   이름: "MiniGameCanvas"
   ```

2. **Canvas 설정**
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler 추가:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5` (중간)

3. **Background 추가**
   ```
   MiniGameCanvas 우클릭 → UI → Image
   이름: "Background"
   ```
   - Anchor: Stretch (전체 화면)
   - Color: 검은색 (R:0, G:0, B:0, A:200)

### 2단계: SugarMiniGamePanel 생성

1. **Panel 생성**
   ```
   MiniGameCanvas 우클릭 → UI → Panel
   이름: "SugarMiniGamePanel"
   ```
   - Width: `800`
   - Height: `600`
   - 중앙 배치

2. **Title 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
   이름: "Title"
   ```
   - Text: "설탕 계량하기"
   - Font Size: `48`
   - Alignment: Center
   - Position: 상단 (Y: 200)

3. **Instructions 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
   이름: "Instructions"
   ```
   - Text: "마우스를 눌러 설탕을 따르세요!"
   - Font Size: `28`
   - Alignment: Center
   - Position: (Y: 150)

4. **Timer 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Text - TextMeshPro
   이름: "Timer"
   ```
   - Text: "남은 시간: 10.0초"
   - Font Size: `32`
   - Alignment: Center
   - Position: (Y: -200)

5. **Container (설탕 용기) 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Image
   이름: "Container"
   ```
   - Width: `200`
   - Height: `400`
   - Color: 회색 (R:0.7, G:0.7, B:0.7)
   - Position: 중앙

6. **SugarFill (채워지는 설탕) 추가**
   ```
   Container 우클릭 → UI → Image
   이름: "SugarFill"
   ```
   - Anchor: Stretch (부모 크기에 맞춤)
   - Color: 흰색
   - Image Type: `Filled`
   - Fill Method: `Vertical`
   - Fill Origin: `Bottom`
   - Fill Amount: `0`

7. **TargetZone (목표 영역) 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Image
   이름: "TargetZone"
   ```
   - Width: `220` (Container보다 약간 크게)
   - Height: `80` (목표 범위 20% → 400 * 0.2 = 80)
   - Position: Container와 같은 X, Y는 Container 상단 기준 80% 위치
   - Color: 초록색 반투명 (R:0, G:1, B:0, A:100)

8. **ResultPanel 추가**
   ```
   SugarMiniGamePanel 우클릭 → UI → Panel
   이름: "ResultPanel"
   ```
   - Width: `600`
   - Height: `300`
   - 중앙 배치
   - **초기 상태: 비활성 (체크 해제)**

9. **ResultText 추가**
   ```
   ResultPanel 우클릭 → UI → Text - TextMeshPro
   이름: "ResultText"
   ```
   - Text: "성공!"
   - Font Size: `48`
   - Alignment: Center
   - Position: (Y: 50)

10. **RetryButton 추가**
    ```
    ResultPanel 우클릭 → UI → Button - TextMeshPro
    이름: "RetryButton"
    ```
    - Width: `200`
    - Height: `60`
    - Position: (Y: -50)
    - Text: "다시 시도"

11. **ContinueButton 추가**
    ```
    ResultPanel 우클릭 → UI → Button - TextMeshPro
    이름: "ContinueButton"
    ```
    - Width: `200`
    - Height: `60`
    - Position: (Y: -50)
    - Text: "계속하기"

### 3단계: 스크립트 추가

1. **SugarPouringMiniGame 추가**
   - `SugarMiniGamePanel` 선택
   - `Add Component` → `SugarPouringMiniGame`

2. **UI 참조 연결**
   
   Inspector에서 다음을 할당:
   - Sugar Fill Image: `SugarFill`
   - Timer Text: `Timer`
   - Instructions Text: `Instructions`
   - Result Panel: `ResultPanel`
   - Result Text: `ResultText`
   - Retry Button: `RetryButton`
   - Continue Button: `ContinueButton`
   - Target Zone Image: `TargetZone`

3. **게임 설정 조정 (필요 시)**
   - Pouring Speed: `0.3` (기본값)
   - Target Min: `0.8` (80%)
   - Target Max: `1.0` (100%)
   - Time Limit: `10` (초)

---

## 매니저 설정

### 1단계: MiniGameManager 오브젝트 생성

1. **GameObject 생성**
   ```
   Hierarchy → 우클릭 → Create Empty
   이름: "MiniGameManager"
   ```

2. **스크립트 추가**
   - `Add Component` → `MiniGameManager`

3. **참조 연결**
   
   Inspector에서:
   - Mini Game Canvas: `MiniGameCanvas`
   - Sugar Mini Game Panel: `SugarMiniGamePanel`
   - Player Controller: `Player` (Hierarchy에서 드래그)
   - Interactor: `Player` (Hierarchy에서 드래그)
   - Isometric Camera: `Main Camera` (Hierarchy에서 드래그)

### 2단계: Canvas 초기 비활성화

- `MiniGameCanvas`를 Hierarchy에서 선택
- Inspector 상단의 체크박스 **해제** (비활성화)

---

## 재료에 미니게임 연결

### 설탕 재료 설정

1. **설탕 Prefab 또는 GameObject 선택**
   - Hierarchy 또는 Project에서 `Ingredient_Sugar` 선택

2. **CollectibleIngredient 설정**
   
   Inspector에서:
   - **Mini Game** 섹션:
     - Requires Mini Game: ☑️ (체크)
     - Mini Game Type: `Sugar`

3. **다른 재료는 그대로**
   - 다른 재료들은 `Requires Mini Game` 체크 해제 (즉시 수집)

---

## 테스트

### 테스트 순서

1. **Play 버튼 클릭**

2. **설탕 재료로 이동**
   - E키 프롬프트 확인: "Sugar 수집 [E]"

3. **E키 누르기**
   - 미니게임 UI가 나타나야 함
   - 플레이어 이동 불가 (일시정지)
   - 커서 표시됨

4. **마우스 버튼 누르기**
   - 설탕 게이지가 채워지는지 확인
   - 타이머가 감소하는지 확인

5. **목표 범위(80-100%) 내에서 마우스 버튼 놓기**
   - 게이지가 초록색으로 변하는지 확인
   - Enter 키 또는 자동으로 성공 판정

6. **성공 메시지 확인**
   - "성공! 완벽한 계량입니다!" 메시지
   - "계속하기" 버튼 클릭

7. **게임 재개**
   - 설탕이 인벤토리에 추가되었는지 확인
   - 플레이어 이동 가능

---

## 문제 해결

### ❌ UI가 표시되지 않음

**원인**: Canvas가 활성화되지 않음

**해결**:
1. MiniGameManager에 Canvas가 올바르게 연결되었는지 확인
2. Console에서 에러 메시지 확인

---

### ❌ E키를 눌러도 미니게임이 시작되지 않음

**원인**: CollectibleIngredient 설정 문제

**해결**:
1. 설탕 GameObject 선택
2. Inspector에서 `Requires Mini Game` 체크 확인
3. `Mini Game Type`이 `Sugar`로 설정되었는지 확인

---

### ❌ 플레이어가 일시정지되지 않음

**원인**: MiniGameManager에 참조 연결 안 됨

**해결**:
1. MiniGameManager 선택
2. Inspector에서 다음 확인:
   - Player Controller 연결
   - Interactor 연결
   - Isometric Camera 연결

---

### ❌ 마우스 버튼을 눌러도 게이지가 안 채워짐

**원인**: SugarPouringMiniGame 스크립트 설정 문제

**해결**:
1. SugarMiniGamePanel 선택
2. SugarPouringMiniGame 컴포넌트 확인
3. UI 참조가 모두 연결되었는지 확인

---

### ❌ 타이머가 표시되지 않음

**원인**: TextMeshPro Font Asset 문제

**해결**:
1. Timer Text 선택
2. Font Asset이 할당되었는지 확인
3. Text 내용이 있는지 확인

---

### ❌ 게임 종료 후 플레이어가 움직이지 않음

**원인**: SetPaused가 제대로 호출되지 않음

**해결**:
1. Console에서 에러 확인
2. MiniGameManager의 EndMiniGame이 호출되는지 확인
3. Continue 버튼 클릭 이벤트 확인

---

## 난이도 조정

### 쉽게 만들기

SugarPouringMiniGame 설정:
- Pouring Speed: `0.2` (느리게)
- Target Min: `0.7` (70%)
- Target Max: `1.0` (100%)
- Time Limit: `15` (초)

### 어렵게 만들기

SugarPouringMiniGame 설정:
- Pouring Speed: `0.5` (빠르게)
- Target Min: `0.85` (85%)
- Target Max: `0.95` (95%)
- Time Limit: `7` (초)

---

## 추가 기능

### 사운드 추가 (선택사항)

1. **AudioClip 준비**
   - 설탕 따르는 소리 (pouringSound)
   - 성공 소리 (successSound)
   - 실패 소리 (failSound)

2. **AudioSource 추가**
   - SugarMiniGamePanel에 AudioSource 컴포넌트 추가
   - Play On Awake: 체크 해제

3. **SugarPouringMiniGame에 할당**
   - Inspector에서 각 AudioClip 할당

---

## 다음 단계

이 시스템을 기반으로 다른 미니게임을 추가할 수 있습니다:

- 🥚 **계란 배달** (EggCarryingMiniGame)
- 🌾 **밀가루 쌓기** (FlourStackingMiniGame)
- 🧈 **냉장고 미로** (ButterMazeMiniGame)
- 🍓 **딸기 따기** (StrawberryPickingMiniGame)

각 미니게임은 `IMiniGame` 인터페이스를 구현하면 됩니다.

---

**마지막 업데이트**: 2026-02-02  
**버전**: 1.0 (설탕 미니게임)

