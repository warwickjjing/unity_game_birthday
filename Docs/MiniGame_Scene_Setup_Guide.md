# 미니게임 Scene 설정 가이드

밀가루(Flour)와 설탕(Sugar) 미니게임을 별도의 2D Scene에서 실행하는 방법을 설명합니다.

---

## 📋 목차

1. [개요](#개요)
2. [Scene 생성](#scene-생성)
3. [밀가루 미니게임 Scene 설정](#밀가루-미니게임-scene-설정)
4. [설탕 미니게임 Scene 설정](#설탕-미니게임-scene-설정)
5. [Build Settings 추가](#build-settings-추가)
6. [테스트](#테스트)

---

## 개요

### 아키텍처

```
CollectibleIngredient → MiniGameManager → MiniGameSceneLoader
                                              ↓
                                    Scene 전환 (FlourMiniGameScene 또는 SugarMiniGameScene)
                                              ↓
                                    MiniGameResult (결과 전달)
                                              ↓
                                    원래 Scene 복귀 + 콜백 호출
```

### 관련 스크립트

- **MiniGameResult.cs**: 결과 전달용 싱글톤 (DontDestroyOnLoad)
- **MiniGameSceneLoader.cs**: Scene 로드 관리
- **FlourMiniGameScene.cs**: 밀가루 미니게임 Scene 컨트롤러
- **SugarMiniGameScene.cs**: 설탕 미니게임 Scene 컨트롤러
- **MiniGameManager.cs**: Scene 기반 미니게임 지원 추가

---

## Scene 생성

### 1. 밀가루 미니게임 Scene

1. **File → New Scene → Empty**
2. **File → Save As**
3. 이름: `FlourMiniGameScene`
4. 위치: `Assets/Scenes/MiniGames/FlourMiniGameScene.unity`

### 2. 설탕 미니게임 Scene

1. **File → New Scene → Empty**
2. **File → Save As**
3. 이름: `SugarMiniGameScene`
4. 위치: `Assets/Scenes/MiniGames/SugarMiniGameScene.unity`

---

## 밀가루 미니게임 Scene 설정 (2D Top-down)

밀가루 미니게임은 **Top-down 2D 마을**에서 NPC 퀘스트를 통해 포대 5개를 운반하는 게임입니다.

### Scene 구조

```
FlourMiniGameScene
├── Main Camera (Orthographic)
├── Canvas (Screen Space - Overlay)
│   ├── EventSystem
│   ├── FlourDialogueUI (Script - Canvas에 직접 추가)
│   │   └── FlourDialoguePanel (자동 생성)
│   │       ├── SpeakerName (TextMeshPro)
│   │       ├── SpeakerPortrait (Image)
│   │       ├── DialogueText (TextMeshPro)
│   │       └── ContinueIndicator (TextMeshPro)
│   └── QuitButton (Button)
├── Tilemap (Background - 선택 사항)
│   └── Grid + Tilemap Renderer
├── Player
│   ├── Sprite Renderer (픽셀 아트 캐릭터)
│   ├── Rigidbody2D
│   ├── BoxCollider2D
│   ├── Animator
│   └── FlourPlayer2D (Script)
├── NPC
│   ├── Sprite Renderer
│   ├── BoxCollider2D (Trigger)
│   └── FlourNPC (Script)
├── FlourBags (5개)
│   ├── Bag_01
│   │   ├── Sprite Renderer
│   │   ├── BoxCollider2D
│   │   └── FlourBag (Script)
│   └── ... (Bag_02 ~ Bag_05)
├── DeliveryZone
│   ├── Sprite Renderer (창고 이미지)
│   ├── BoxCollider2D (Trigger)
│   └── FlourDeliveryZone (Script)
└── FlourMiniGame2DController
    └── FlourMiniGame2DScene (Script)
```

### 상세 설정

#### 1. Camera 설정 (중요!)

```
Main Camera 선택:
- Projection: Orthographic
- Size: 10 (또는 적절한 값)
- Background: 하늘색 (R: 0.7, G: 0.9, B: 1, A: 1)
```

#### 2. Physics2D 설정

```
Edit → Project Settings → Physics2D:
- Gravity: X: 0, Y: 0 (중력 없음)
```

#### 3. Canvas 생성

```
Hierarchy 우클릭 → UI → Canvas
이름: Canvas
Render Mode: Screen Space - Overlay
```

#### 4. EventSystem 자동 생성 확인

```
Canvas 생성 시 자동으로 생성됨
없으면 수동 생성: Hierarchy 우클릭 → UI → Event System
```

#### 5. QuitButton (종료 버튼)

```
Canvas 우클릭 → UI → Button - TextMeshPro
이름: QuitButton
Anchor: Bottom-Right
Position: X: -100, Y: 50
Width: 150, Height: 60

Button Text:
- Text: "나가기"
- Font Size: 24
- Color: 흰색

Button:
- Normal Color: 빨간색 (R: 0.8, G: 0.2, B: 0.2, A: 1)
```

#### 6. Player 생성

```
Hierarchy 우클릭 → 2D Object → Sprite → Square
이름: Player

Components:
- Sprite Renderer:
  - Sprite: 픽셀 아트 캐릭터 (없으면 기본 사각형 사용)
  - Color: 원하는 색상
- Rigidbody2D:
  - Body Type: Dynamic
  - Gravity Scale: 0
  - Drag: 10
  - Constraints: Freeze Rotation Z
- BoxCollider2D:
  - Size: X: 0.8, Y: 0.8
- Animator:
  - Controller: 8방향 애니메이션 컨트롤러 (선택 사항)
- FlourPlayer2D (Script):
  - Move Speed: 5
  - Interaction Range: 1.5
  - Horizontal Parameter Name: "Horizontal"
  - Vertical Parameter Name: "Vertical"
  - Speed Parameter Name: "Speed"
```

**픽셀 아트 스프라이트 Import 설정:**
```
Project 창에서 스프라이트 선택:
- Texture Type: Sprite (2D and UI)
- Filter Mode: Point (no filter)
- Pixels Per Unit: 16 (또는 적절한 값)
- Compression: None
```

#### 7. NPC 생성

```
Hierarchy 우클릭 → 2D Object → Sprite → Square
이름: NPC

Components:
- Sprite Renderer:
  - Sprite: NPC 스프라이트 (없으면 기본 사각형)
  - Color: 파란색 등
- BoxCollider2D:
  - Is Trigger: ✓
  - Size: X: 1, Y: 1.5
- FlourNPC (Script):
  - Dialogue - Start: (여러 대화 추가 가능)
    - Element 0:
      - Text: "안녕하세요! 마을 주민입니다."
      - Speaker Name: "마을 주민"
      - Speaker Portrait: (선택 사항 - NPC 초상화 이미지)
    - Element 1:
      - Text: "포대 5자루를 창고로 옮겨주면 한 자루를 드리겠어요!"
      - Speaker Name: "마을 주민"
      - Speaker Portrait: (동일)
  - Dialogue - Progress: (선택 사항, 비워둘 수 있음)
  - Dialogue - Complete: (여러 대화 추가 가능)
    - Element 0:
      - Text: "고마워요! 약속대로 밀가루 한 자루입니다."
      - Speaker Name: "마을 주민"
      - Speaker Portrait: (동일)
  - NPC Info (Optional):
    - Npc Name: "마을 주민" (기본값으로 사용됨)
    - Npc Portrait: (선택 사항 - NPC 초상화 이미지)

※ 대화는 Space 키를 눌러 다음으로 진행됩니다.
※ Speaker Portrait는 선택 사항으로, 없으면 이름만 표시됩니다.
```

#### 8. FlourBags 생성 (5개)

각 포대를 개별적으로 생성:

```
Hierarchy 우클릭 → 2D Object → Sprite → Square
이름: Bag_01

Components:
- Sprite Renderer:
  - Sprite: 포대 스프라이트 (없으면 기본 사각형)
  - Color: 갈색/베이지색
- BoxCollider2D:
  - Is Trigger: ☐ (물리 충돌 필요)
  - Size: X: 0.6, Y: 0.8
- FlourBag (Script):
  - Can Be Picked Up: ✓

※ Bag_02 ~ Bag_05도 동일하게 생성
```

#### 9. DeliveryZone 생성

```
Hierarchy 우클릭 → 2D Object → Sprite → Square
이름: DeliveryZone

Components:
- Sprite Renderer:
  - Sprite: 창고 이미지 (없으면 기본 사각형)
  - Color: 회색
- BoxCollider2D:
  - Is Trigger: ✓
  - Size: X: 2, Y: 2
- FlourDeliveryZone (Script):
  - Interaction Range: 1.5
  - Player: Player 드래그
  - NPC: NPC 드래그
```

#### 10. FlourDialogueUI (자동 생성 또는 수동)

**방법 1: 자동 생성 (권장)**
```
FlourDialogueUI 스크립트가 자동으로 UI를 생성합니다.
Canvas에 FlourDialogueUI 컴포넌트만 추가하면 됩니다.

Canvas 선택 → Add Component → FlourDialogueUI (Script)

자동으로 생성되는 UI:
- DialoguePanel (하단 0~25%)
- SpeakerName (좌측 상단, 화자 이름)
- SpeakerPortrait (우측 상단, 화자 초상화)
- DialogueText (중앙, 대화 내용)
- ContinueIndicator (우측 하단, "▼ Space")

Inspector 설정:
- Continue Key: Space (다음 대화로 진행)
- Use Typing Effect: ☐ (타이핑 효과 사용 여부)
- Typing Speed: 0.05 (타이핑 속도)
```

**방법 2: 수동 생성**
```
Canvas 선택 → Add Component → FlourDialogueUI (Script)

Canvas 우클릭 → UI → Image
이름: FlourDialoguePanel
- Anchor: 0, 0 ~ 1, 0.25 (하단 25%)
- Color: 검정색 반투명 (R: 0, G: 0, B: 0, A: 0.8)

FlourDialoguePanel 우클릭 → UI → Image
이름: SpeakerPortrait
- Anchor: 0.85, 0.6 ~ 0.98, 0.98 (우측 상단)
- Preserve Aspect: ✓
- Color: 흰색

FlourDialoguePanel 우클릭 → UI → Text - TextMeshPro
이름: SpeakerName
- Anchor: 0.05, 0.7 ~ 0.5, 0.95 (좌측 상단)
- Font Size: 28
- Alignment: Bottom Left
- Color: 노란색

FlourDialoguePanel 우클릭 → UI → Text - TextMeshPro
이름: DialogueText
- Anchor: 0.05, 0.15 ~ 0.95, 0.65 (중앙)
- Font Size: 24
- Alignment: Top Left
- Color: 흰색
- Word Wrapping: ✓

FlourDialoguePanel 우클릭 → UI → Text - TextMeshPro
이름: ContinueIndicator
- Anchor: 0.85, 0.05 ~ 0.98, 0.15 (우측 하단)
- Font Size: 20
- Alignment: Center
- Text: "▼ Space"
- Color: 흰색

FlourDialogueUI Inspector:
- Dialogue Panel: FlourDialoguePanel 드래그
- Speaker Name Text: SpeakerName 드래그
- Dialogue Text: DialogueText 드래그
- Speaker Portrait: SpeakerPortrait 드래그
- Continue Indicator: ContinueIndicator 드래그
- Continue Key: Space
- Use Typing Effect: ☐ (선택 사항)
- Typing Speed: 0.05
```

#### 11. FlourMiniGame2DController

```
Hierarchy 우클릭 → Create Empty
이름: FlourMiniGame2DController

Add Component → FlourMiniGame2DScene (Script)

Inspector 할당:
- Player: Player 드래그
- NPC: NPC 드래그
- Delivery Zone: DeliveryZone 드래그
- Flour Bags: Bag_01 ~ Bag_05 모두 드래그 (5개)
- Quit Button: QuitButton 드래그
```

#### 12. Tilemap (선택 사항 - 배경용)

```
Hierarchy 우클릭 → 2D Object → Tilemap → Rectangular
이름: Tilemap

Grid 선택:
- Cell Size: X: 1, Y: 1

Tilemap에 타일 할당하여 마을 배경 생성
(또는 간단한 Image로 배경 대체 가능)
```

### 게임 플레이 흐름

1. **게임 시작**: 플레이어가 2D 마을에 등장
2. **NPC와 대화**: NPC 근처에서 [F]키로 퀘스트 수락
3. **포대 집기**: 포대 근처에서 [F]키로 집기
4. **배달**: DeliveryZone 근처에서 [F]키로 배달
5. **반복**: 5개 포대 모두 배달
6. **완료**: NPC와 다시 대화하여 보상 받기 → 미니게임 성공

---

## 설탕 미니게임 Scene 설정

### Scene 구조

```
SugarMiniGameScene
├── Canvas (Screen Space - Overlay)
│   ├── EventSystem
│   ├── Background (Image - 갈색/주황색)
│   ├── TitleText (TextMeshPro)
│   ├── InstructionText (TextMeshPro)
│   ├── ScoreText (TextMeshPro)
│   ├── ConveyorBelt (Image - 컨베이어 벨트)
│   │   ├── SpawnPoint (Empty GameObject)
│   │   ├── TargetZone (Image - 노란색 영역)
│   │   └── EndPoint (Empty GameObject)
│   └── QuitButton (Button)
└── SugarMiniGameController (Empty GameObject)
    └── SugarMiniGameScene (Script)
```

### 상세 설정

#### 1. Canvas & EventSystem
```
FlourMiniGameScene과 동일하게 생성
```

#### 2. Background
```
Canvas 우클릭 → UI → Image
이름: Background
Anchor: Stretch-Stretch
Color: 갈색/주황색 (R: 0.9, G: 0.7, B: 0.5, A: 1)
```

#### 3. TitleText
```
Canvas 우클릭 → UI → Text - TextMeshPro
이름: TitleText
Anchor: Top-Center
Position: X: 0, Y: -50
Width: 800, Height: 80
Font Size: 60
Alignment: Center
Text: "설탕 수집하기"
```

#### 4. InstructionText
```
Canvas 우클릭 → UI → Text - TextMeshPro
이름: InstructionText
Anchor: Top-Center
Position: X: 0, Y: -150
Width: 800, Height: 60
Font Size: 30
Alignment: Center
Text: "설탕이 노란 구역을 지날 때 클릭하세요!"
```

#### 5. ScoreText
```
Canvas 우클릭 → UI → Text - TextMeshPro
이름: ScoreText
Anchor: Top-Left
Position: X: 100, Y: -50
Width: 300, Height: 60
Font Size: 36
Alignment: Left
Text: "점수: 0 / 5"
Color: 검정색
```

#### 6. ConveyorBelt (컨베이어 벨트)
```
Canvas 우클릭 → UI → Image
이름: ConveyorBelt
Anchor: Middle-Center
Position: X: 0, Y: -100
Width: 1200, Height: 150
Color: 어두운 회색 (R: 0.3, G: 0.3, B: 0.3, A: 0.8)
```

#### 7. SpawnPoint (생성 위치)
```
ConveyorBelt 우클릭 → Create Empty
이름: SpawnPoint
Anchor: Middle-Left
Position: X: -600, Y: 0
```

#### 8. TargetZone (목표 영역)
```
ConveyorBelt 우클릭 → UI → Image
이름: TargetZone
Anchor: Middle-Center
Position: X: 0, Y: 0
Width: 100, Height: 150
Color: 노란색 (R: 1, G: 1, B: 0, A: 0.3) - 반투명
```

#### 9. EndPoint (끝 위치)
```
ConveyorBelt 우클릭 → Create Empty
이름: EndPoint
Anchor: Middle-Right
Position: X: 600, Y: 0
```

#### 10. QuitButton
```
FlourMiniGameScene과 동일하게 생성
```

#### 11. SugarMiniGameController
```
Hierarchy 우클릭 → Create Empty
이름: SugarMiniGameController

Add Component → SugarMiniGameScene (Script)

Inspector 할당:
- Title Text: TitleText 드래그
- Instruction Text: InstructionText 드래그
- Score Text: ScoreText 드래그
- Quit Button: QuitButton 드래그
- Conveyor Belt Transform: ConveyorBelt 드래그
- Spawn Point: SpawnPoint 드래그
- Target Zone Transform: TargetZone 드래그
- End Point: EndPoint 드래그
- Target Zone Image: TargetZone의 Image 컴포넌트 드래그

설정값:
- Conveyor Speed: 2
- Spawn Interval: 2
- Perfect Zone Size: 50
- Good Zone Size: 100
- Target Score: 5
- Perfect Color: 초록색 (R: 0, G: 1, B: 0, A: 1)
- Good Color: 노란색 (R: 1, G: 1, B: 0, A: 1)
```

### 설탕 봉지 Prefab (선택 사항)

더 멋진 비주얼을 위해 설탕 봉지 Prefab을 만들 수 있습니다:

```
1. Hierarchy 우클릭 → UI → Image
2. 이름: SugarBag
3. Width: 80, Height: 100
4. Color: 흰색
5. 설탕 봉지 이미지 Sprite 할당 (있으면)
6. Project 창으로 드래그하여 Prefab 생성
7. SugarMiniGameController → Sugar Bag Prefab에 할당
```

---

## Build Settings 추가

**중요!** Scene을 Build Settings에 추가해야 Scene 전환이 작동합니다.

### 방법 1: Build Settings 창에서 추가

1. **File → Build Settings**
2. **Scenes In Build** 영역에서:
3. **Add Open Scenes** 버튼 클릭 (해당 Scene이 열려있을 때)
4. 또는 **Scene 파일을 드래그**하여 추가

### 방법 2: Scene 이름 확인

Build Settings의 Scene 순서:

```
✓ 0. TitleScene (기존)
✓ 1. Home (기존)
✓ 2. EndingScene (기존)
✓ 3. FlourMiniGameScene (추가!)
✓ 4. SugarMiniGameScene (추가!)
```

### Scene 이름 주의사항

**MiniGameSceneLoader.cs**의 Scene 이름과 일치해야 합니다:

```csharp
public const string FLOUR_SCENE_NAME = "FlourMiniGameScene";
public const string SUGAR_SCENE_NAME = "SugarMiniGameScene";
```

Scene 이름이 다르면 수정하세요!

---

## 테스트

### 1. 밀가루 미니게임 테스트

1. **Home Scene 실행**
2. **밀가루 재료와 상호작용**
3. **FlourMiniGameScene으로 전환 확인**
4. **마우스 흔들기**
5. **진행도 바가 채워지는지 확인**
6. **100% 달성 시 Home Scene으로 복귀**
7. **밀가루 수집 확인**

### 2. 설탕 미니게임 테스트

1. **Home Scene 실행**
2. **설탕 재료와 상호작용**
3. **SugarMiniGameScene으로 전환 확인**
4. **설탕 봉지가 왼쪽에서 오른쪽으로 이동하는지 확인**
5. **노란 영역에서 클릭**
6. **점수 증가 확인**
7. **5점 달성 시 Home Scene으로 복귀**
8. **설탕 수집 확인**

### 3. 디버깅 Console 로그

성공 시 로그:

```
[MiniGameManager] Flour Scene 기반 미니게임 시작
[MiniGameSceneLoader] FlourMiniGameScene Scene 로드 중...
[MiniGameResult] 미니게임 준비: Flour, 복귀 씬: Home
[FlourMiniGameScene] 밀가루 미니게임 Scene 시작
[FlourMiniGameScene] 게임 시작
[FlourMiniGameScene] 게임 성공!
[MiniGameResult] 결과: 성공
[MiniGameResult] Home Scene으로 복귀
```

---

## 대화 시스템 사용 방법

### 기본 사용법

FlourDialogueUI는 여러 대화를 순차적으로 표시할 수 있으며, 각 대화마다 화자 이름과 초상화를 설정할 수 있습니다.

### 단일 대화 (간단)

```csharp
// 단순 텍스트만 표시
FlourDialogueUI dialogueUI = FindObjectOfType<FlourDialogueUI>();
dialogueUI.Show("안녕하세요!");
```

### 여러 대화 (권장)

```csharp
// 여러 대화를 리스트로 생성
List<FlourDialogueData> dialogues = new List<FlourDialogueData>
{
    new FlourDialogueData
    {
        text = "안녕하세요! 저는 마을 주민입니다.",
        speakerName = "마을 주민",
        speakerPortrait = npcSprite // Sprite 할당
    },
    new FlourDialogueData
    {
        text = "포대를 옮겨주시면 밀가루를 드리겠습니다.",
        speakerName = "마을 주민",
        speakerPortrait = npcSprite
    },
    new FlourDialogueData
    {
        text = "정말요? 감사합니다!",
        speakerName = "플레이어",
        speakerPortrait = playerSprite // 플레이어 초상화
    }
};

dialogueUI.ShowDialogue(dialogues);
```

### Inspector에서 설정 (가장 쉬움)

```
FlourNPC Inspector:

Dialogue - Start (Size: 2)
├── Element 0
│   ├── Text: "안녕하세요! 저는 마을 주민입니다."
│   ├── Speaker Name: "마을 주민"
│   └── Speaker Portrait: NPC_Portrait (Sprite)
└── Element 1
    ├── Text: "포대 5자루를 창고로 옮겨주면 한 자루를 드리겠어요!"
    ├── Speaker Name: "마을 주민"
    └── Speaker Portrait: NPC_Portrait (Sprite)

Dialogue - Complete (Size: 1)
└── Element 0
    ├── Text: "고마워요! 약속대로 밀가루 한 자루입니다."
    ├── Speaker Name: "마을 주민"
    └── Speaker Portrait: NPC_Portrait (Sprite)
```

### 플레이어 조작

```
Space 키: 다음 대화로 진행
- 타이핑 중이면: 타이핑 스킵 (전체 텍스트 즉시 표시)
- 타이핑 완료 후: 다음 대화로 이동
- 마지막 대화 후: 대화창 자동 닫힘
```

### 고급 사용

**동적으로 대화 생성**

```csharp
public void ShowProgress()
{
    List<FlourDialogueData> progressDialogues = new List<FlourDialogueData>
    {
        new FlourDialogueData
        {
            text = $"포대를 {_deliveredBags}/{TARGET_BAGS} 옮겼어요.",
            speakerName = npcName,
            speakerPortrait = npcPortrait
        },
        new FlourDialogueData
        {
            text = "계속해주세요!",
            speakerName = npcName,
            speakerPortrait = npcPortrait
        }
    };
    
    dialogueUI.ShowDialogue(progressDialogues);
}
```

**여러 캐릭터 대화**

```csharp
List<FlourDialogueData> conversation = new List<FlourDialogueData>
{
    new FlourDialogueData { text = "안녕하세요!", speakerName = "NPC A", speakerPortrait = npcASprite },
    new FlourDialogueData { text = "반갑습니다!", speakerName = "NPC B", speakerPortrait = npcBSprite },
    new FlourDialogueData { text = "저도요!", speakerName = "플레이어", speakerPortrait = playerSprite }
};

dialogueUI.ShowDialogue(conversation);
```

### 타이핑 효과 사용

```
FlourDialogueUI Inspector:
- Use Typing Effect: ✓
- Typing Speed: 0.05 (작을수록 빠름)

타이핑 효과 활성화 시:
- 텍스트가 한 글자씩 나타남
- Space 키로 스킵 가능
```

---

## 문제 해결

### Scene이 로드되지 않음

**원인**: Build Settings에 Scene이 추가되지 않았습니다.

**해결**:
1. File → Build Settings
2. FlourMiniGameScene, SugarMiniGameScene 추가
3. ✓ 체크 확인

### NullReferenceException 발생

**원인**: Inspector에 UI 요소가 할당되지 않았습니다.

**해결**:
1. Scene을 열고
2. Controller 오브젝트 선택
3. Inspector에서 모든 필드 할당 확인
4. 빠진 항목 드래그하여 할당

### 복귀 후 결과가 전달되지 않음

**원인**: MiniGameResult 싱글톤이 DontDestroyOnLoad 설정이 안 됨

**해결**:
- MiniGameResult.cs의 `DontDestroyOnLoad(gameObject);` 확인
- Awake()가 호출되는지 확인

### 미니게임이 Panel로 실행됨

**원인**: MiniGameSceneLoader.IsSceneBasedMiniGame()이 false 반환

**해결**:
```csharp
public static bool IsSceneBasedMiniGame(MiniGameType type)
{
    return type == MiniGameType.Flour || type == MiniGameType.Sugar;
}
```
확인!

---

## 추가 개선 아이디어

### 비주얼 개선

1. **밀가루 미니게임**:
   - 체 이미지를 실제 체 Sprite로 교체
   - 밀가루 파티클을 Particle System으로 업그레이드
   - 배경에 부엌 이미지 추가

2. **설탕 미니게임**:
   - 설탕 봉지 Sprite 추가
   - 컨베이어 벨트 애니메이션 추가
   - 성공/실패 피드백 효과 강화

### 사운드 추가

```csharp
[Header("Audio")]
[SerializeField] private AudioClip shakeSound; // 체 흔드는 소리
[SerializeField] private AudioClip successSound; // 성공 소리
[SerializeField] private AudioClip failSound; // 실패 소리
[SerializeField] private AudioClip bgMusic; // 배경 음악
```

### 난이도 조정

- 밀가루: `targetProgress`, `shakeSensitivity`, `progressDecayRate` 조정
- 설탕: `conveyorSpeed`, `spawnInterval`, `targetScore` 조정

---

## 요약

✅ **MiniGameResult.cs** - 결과 전달 싱글톤 생성  
✅ **MiniGameSceneLoader.cs** - Scene 로드 관리  
✅ **FlourMiniGameScene.cs** - 밀가루 미니게임 Scene 컨트롤러  
✅ **SugarMiniGameScene.cs** - 설탕 미니게임 Scene 컨트롤러  
✅ **MiniGameManager.cs** - Scene 기반 미니게임 지원 추가  
✅ **FlourMiniGameScene.unity** - 밀가루 미니게임 Scene 생성  
✅ **SugarMiniGameScene.unity** - 설탕 미니게임 Scene 생성  
✅ **Build Settings** - Scene 추가  

**이제 밀가루와 설탕 미니게임이 멋진 별도 Scene에서 실행됩니다!** 🎉

