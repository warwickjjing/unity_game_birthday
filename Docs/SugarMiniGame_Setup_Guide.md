# 설탕 미니게임 Scene 설정 가이드

NPC와 대화하고 선택지를 통해 정답 NPC를 찾아 설탕을 획득하는 미니게임입니다.

---

## 📋 목차

1. [씬 생성](#씬-생성)
2. [씬 기본 구조](#씬-기본-구조)
3. [NPC 설정](#npc-설정)
4. [UI 설정](#ui-설정)
5. [대화 설정](#대화-설정)
6. [테스트](#테스트)

---

## 씬 생성

1. **File → New Scene → Empty**
2. **File → Save As**
3. 이름: `SugarMiniGameScene`
4. 위치: `Assets/Scenes/MiniGames/SugarMiniGameScene.unity`

---

## 씬 기본 구조

```
SugarMiniGameScene
├── MainCamera (AudioListener 포함)
├── Canvas (Screen Space - Overlay)
│   ├── EventSystem
│   ├── DialoguePanel (기존 DialogueUI 사용)
│   │   ├── ChoiceButtonParent (VerticalLayoutGroup) ← 새로 추가
│   │   └── ... (기존 DialoguePanel 구조)
│   └── QuitButton
└── SugarMiniGameController (Empty GameObject)
    └── SugarMiniGameScene (Script)
```

---

## NPC 설정

### 1. NPC 오브젝트 생성

씬에 **3~5개의 NPC**를 배치합니다.

```
Hierarchy 우클릭 → Create Empty
이름: NPC_01, NPC_02, NPC_03, NPC_04, NPC_05
```

### 2. NPC 위치 배치

- NPC들을 씬 내 적절한 위치에 배치
- 플레이어가 각 NPC와 상호작용할 수 있도록 충분한 간격 유지

### 3. NPC 컴포넌트 추가

각 NPC 오브젝트에:

1. **Transform 설정**:
   - Position: 원하는 위치
   - Rotation: (0, 0, 0)

2. **Collider 추가**:
   ```
   Add Component → Box Collider
   - Is Trigger: ☑ (체크)
   - Size: (1, 2, 1) (NPC 크기에 맞게 조정)
   ```

3. **SugarNPC 스크립트 추가**:
   ```
   Add Component → SugarNPC (Script)
   ```

4. **WorldSpaceInteractionPrompt 자동 추가됨** (코드에서 자동 처리)

### 4. NPC Inspector 설정

각 NPC의 **SugarNPC (Script)** Inspector:

- **Has Answer**: ☐ (체크하지 않음 - 코드에서 랜덤 설정)
- **Correct Dialogues**: 정답 NPC일 때 대화 (나중에 설정)
- **Wrong Dialogues**: 틀린 NPC일 때 대화 (나중에 설정)

---

## UI 설정

### 1. Canvas 생성

```
Hierarchy 우클릭 → UI → Canvas
이름: Canvas

Inspector:
- Render Mode: Screen Space - Overlay
- Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
```

### 2. EventSystem 생성

```
Canvas 생성 시 자동 생성됨
또는 Hierarchy 우클릭 → UI → Event System
```

### 3. DialoguePanel 설정

#### 방법 1: Home 씬의 DialoguePanel 복사

1. Home 씬 열기
2. Canvas → DialoguePanel 선택
3. **Ctrl+C** (복사)
4. SugarMiniGameScene 열기
5. Canvas 선택
6. **Ctrl+V** (붙여넣기)

#### 방법 2: 새로 생성

```
Canvas 우클릭 → UI → Image
이름: DialoguePanel

Inspector:
- Rect Transform:
  - Anchor: Min (0, 0), Max (1, 0.25) (하단 25%)
  - Position: (0, 0, 0)
- Image:
  - Color: (0, 0, 0, 200) (검은색 반투명)

Add Component → DialogueUI (Script)
```

**DialoguePanel 하위 구조**:
```
DialoguePanel
├── SpeakerName (TextMeshProUGUI)
├── DialogueText (TextMeshProUGUI)
├── ContinueIndicator (TextMeshProUGUI)
├── SpeakerPortrait (Image)
└── ChoiceButtonParent (VerticalLayoutGroup) ← 새로 추가
```

### 4. ChoiceButtonParent 생성

```
DialoguePanel 우클릭 → Create Empty
이름: ChoiceButtonParent

Add Component → Vertical Layout Group
Inspector:
- Spacing: 10
- Padding: Left 20, Right 20, Top 10, Bottom 10
- Child Alignment: Middle Center
- Child Force Expand: Width ☑, Height ☐

Rect Transform:
- Anchor: Min (0.1, 0.05), Max (0.9, 0.2)
- Position: (0, 0, 0)
```

**DialogueUI Inspector 설정**:
- **Choice Button Parent**: ChoiceButtonParent 드래그

### 5. ChoiceButtonPrefab 생성 (선택사항)

```
Hierarchy 우클릭 → UI → Button
이름: ChoiceButtonPrefab

Inspector:
- Rect Transform:
  - Width: 800
  - Height: 60
- Image:
  - Color: (50, 50, 50, 230) (어두운 회색)

Button (Text) 하위:
- Text (TextMeshProUGUI):
  - Font Size: 24
  - Alignment: Center
  - Color: White

프리팹으로 저장:
- Project 창에서 우클릭 → Create → Prefab
- 이름: ChoiceButtonPrefab
- ChoiceButtonPrefab을 드래그하여 저장
```

**DialogueUI Inspector 설정**:
- **Choice Button Prefab**: ChoiceButtonPrefab 드래그

> **참고**: Prefab이 없어도 코드에서 자동 생성됩니다.

### 6. QuitButton 생성

```
Canvas 우클릭 → UI → Button
이름: QuitButton

Inspector:
- Rect Transform:
  - Anchor: Top Right
  - Position: (-50, -50, 0)
  - Width: 150
  - Height: 50
- Button (Text):
  - Text: "나가기"
```

---

## 대화 설정

### 1. 정답 NPC 대화 설정

정답 NPC (설탕을 가지고 있는 NPC)의 **SugarNPC (Script)** Inspector:

**Correct Dialogues**:
- **Size**: 2

**Element 0** (첫 번째 대화):
- **Text**: "어머, 저한테 무슨 일이세요?"
- **Speaker Name**: "NPC"
- **Speaker Portrait**: (선택사항)

**Element 1** (선택지가 있는 대화):
- **Text**: "혹시... 설탕 갖고 계신 분이 맞죠?"
- **Speaker Name**: "아내"
- **Choices**:
  - **Size**: 2

  **Choice 0** (정답 선택지):
  - **Choice Text**: "설탕 주실 수 있나요?"
  - **Is Correct**: ☑ (체크)
  - **Next Dialogues**:
    - **Size**: 1
    - **Element 0**:
      - **Text**: "어머 어떻게 아셨어요?! 여기 있어요~"
      - **Speaker Name**: "NPC"

  **Choice 1** (틀린 선택지):
  - **Choice Text**: "아 죄송해요, 잘못 봤어요"
  - **Is Correct**: ☐ (체크 안 함)
  - **Next Dialogues**:
    - **Size**: 1
    - **Element 0**:
      - **Text**: "아 네, 괜찮아요~"
      - **Speaker Name**: "NPC"

### 2. 틀린 NPC 대화 설정

틀린 NPC들의 **SugarNPC (Script)** Inspector:

**Wrong Dialogues**:
- **Size**: 1

**Element 0**:
- **Text**: "설탕이요? 저는 생선만 팔아요~"
- **Speaker Name**: "NPC"

---

## SugarMiniGameController 설정

```
Hierarchy 우클릭 → Create Empty
이름: SugarMiniGameController

Add Component → SugarMiniGameScene (Script)
```

**Inspector 설정**:
- **NPCs**: NPC_01 ~ NPC_05 모두 드래그 (5개)
- **Auto Find NPCs**: ☑ (체크하면 자동으로 찾음)
- **Quit Button**: QuitButton 드래그

---

## 테스트

1. **Play 버튼 클릭**
2. **NPC와 상호작용** (F키)
3. **선택지 버튼 클릭**
4. **정답 NPC 선택 시 설탕 획득 확인**
5. **미니게임 완료 후 Home 씬으로 복귀 확인**

---

## 주의사항

1. **정답 NPC는 랜덤 설정**: 게임 시작 시 자동으로 1명만 정답으로 설정됩니다.
2. **선택지 버튼**: ChoiceButtonPrefab이 없어도 자동 생성되지만, 프리팹을 사용하면 스타일을 일관되게 유지할 수 있습니다.
3. **DialogueSystem**: DontDestroyOnLoad이므로 Home 씬에서 이미 생성되어 있을 수 있습니다.
4. **AudioListener**: MainCamera에 AudioListener가 있는지 확인하세요.

---

## 문제 해결

### 선택지 버튼이 표시되지 않음
- ChoiceButtonParent가 DialoguePanel 하위에 있는지 확인
- DialogueUI Inspector에서 ChoiceButtonParent가 할당되었는지 확인

### 정답 선택해도 설탕이 획득되지 않음
- DialogueUI의 OnCorrectChoiceSelected 이벤트가 제대로 구독되었는지 확인
- 콘솔 로그 확인: "[SugarMiniGameScene] 설탕 획득!" 메시지 확인

### NPC와 상호작용이 안 됨
- NPC에 Collider가 있고 Is Trigger가 체크되어 있는지 확인
- WorldSpaceInteractionPrompt 컴포넌트가 있는지 확인

