# 씬 구성 가이드

이 문서는 Birthday Cake Quest의 Home.unity 씬을 처음부터 구성하는 상세 가이드입니다.

## 전제 조건

- Unity 2022.3 LTS 설치 완료
- URP 패키지 설치 완료
- UniVRM 설치 완료 (Docs/UniVRM_Setup_Guide.md 참고)
- VRM 캐릭터, 집 에셋, 엔딩 영상 준비 완료

## 씬 구조 개요

```
Home Scene
├── GameSystems (Empty GameObject)
│   ├── IngredientInventory
│   └── EndingCutsceneController
├── Player (VRM Prefab)
│   ├── CharacterController
│   ├── PlayerController
│   └── Interactor
├── Main Camera
│   └── IsometricFollowCamera
├── Lighting
│   ├── Directional Light
│   └── Environment (Reflection Probe, Light Probes)
├── Environment (Empty GameObject)
│   ├── House (프리팹 또는 3D 모델)
│   └── Floor
├── Ingredients (Empty GameObject)
│   ├── Ingredient_Flour
│   ├── Ingredient_Sugar
│   ├── Ingredient_Egg
│   ├── Ingredient_Butter
│   └── Ingredient_Strawberry
├── UI (Canvas)
│   ├── GameplayUI (Empty GameObject, 선택)
│   │   └── IngredientChecklistText (TextMeshPro)
│   ├── CreditsUI (Empty GameObject)
│   │   ├── CreditsScrollView (ScrollRect, 선택 - CreditsScroller용)
│   │   ├── MessageText (TextMeshPro, CreditsSlidePlayer용)
│   │   ├── LyricsText (TextMeshPro, CreditsSlidePlayer용)
│   │   └── ReturnButton (Button)
│   ├── EndingUI (Empty GameObject)
│   │   ├── EndingText (TextMeshPro)
│   │   └── RestartButton (Button)
│   └── InteractionPrompt (TextMeshPro) - Optional
└── Cutscene (Empty GameObject)
    ├── PlayableDirector (Timeline)
    ├── CutsceneCamera (선택)
    └── VideoPlayer (선택)
```

## 1. 새 씬 생성 및 기본 설정

### 1.1 씬 생성

1. **File → New Scene** 또는 `Ctrl+N`
2. **3D (URP)** 템플릿 선택
3. **File → Save As** → `Assets/Scenes/Home.unity`

### 1.2 URP 설정 확인

1. **Edit → Project Settings → Graphics**
2. **Scriptable Render Pipeline Settings** 확인
3. 없으면: `Assets`에서 우클릭 → Create → Rendering → URP → Pipeline Asset (Forward Renderer)

## 2. 게임 시스템 설정

### 2.1 GameSystems 오브젝트 생성

1. **Hierarchy 우클릭 → Create Empty**
2. 이름: `GameSystems`
3. Position: (0, 0, 0)

### 2.2 IngredientInventory 추가

1. **GameSystems 선택**
2. **Add Component → Ingredient Inventory**
3. **Required Ingredients** 확인:
   - Size: 5
   - Element 0: Flour
   - Element 1: Sugar
   - Element 2: Egg
   - Element 3: Butter
   - Element 4: Strawberry

### 2.3 EndingCutsceneController 추가

1. **GameSystems 선택**
2. **Add Component → Ending Cutscene Controller**
3. **설정** (일단 None으로 두고 나중에 연결):
   - Inventory: None (나중에 연결)
   - Director: None (나중에 연결)
   - Player Controller: None (나중에 연결)
   - Gameplay UI Root: None (나중에 연결 - Canvas/GameplayUI 또는 Canvas)
   - Ending UI Root: None (나중에 연결)
   - Credits UI Root: None (나중에 연결 - Canvas/CreditsUI)
   - Sofa Sit Position: None (나중에 연결 - 소파 앉기 위치 Transform)
   - Video Player: None (선택)
   - Control Video From Script: false (Timeline에서 제어)
   - Ending Music: None (선택, 엔딩 크레딧 배경음)

## 3. 플레이어 설정

### 3.1 VRM 캐릭터 배치

1. **VRM 프리팹을 Hierarchy로 드래그**
2. 이름: `Player`
3. **Transform 설정**:
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

### 3.2 CharacterController 추가

1. **Player 선택**
2. **Add Component → Character Controller**
3. **설정**:
   - Center: (0, 0.9, 0)
   - Radius: 0.3
   - Height: 1.8
   - Skin Width: 0.08
   - Min Move Distance: 0.001

### 3.3 PlayerController 추가

1. **Add Component → Player Controller**
2. **설정**:
   - Walk Speed: 3
   - Run Speed: 6
   - Rotation Speed: 10
   - Gravity: -9.81
   - Ground Check Distance: 0.2
   - Ground Mask: Everything

### 3.4 Interactor 추가

1. **Add Component → Interactor**
2. **설정**:
   - Detection Radius: 2
   - Ingredient Layer: Everything
   - Interact Key: F (기본값, E키도 호환)
   - Interaction Prompt: None (나중에 연결)

### 3.5 태그 설정

1. **Inspector 상단 Tag 드롭다운**
2. **Player** 선택 (없으면 Add Tag로 생성)

## 4. 카메라 설정

### 4.1 Main Camera 설정

1. **Main Camera 선택**
2. **Add Component → Isometric Follow Camera**
3. **설정**:
   - Target: Player (드래그해서 연결)
   - Offset: (0, 10, -8)
   - Angle X: 45
   - Follow Speed: 5
   - Use Smooth Follow: ✓
   - Use Bounds: ☐ (나중에 필요시 활성화)

4. **Camera 컴포넌트 설정**:
   - Clear Flags: Skybox
   - Field of View: 60
   - Clipping Planes Near: 0.3
   - Clipping Planes Far: 1000

## 5. 환경 설정

### 5.1 바닥 생성

1. **Hierarchy 우클릭 → 3D Object → Plane**
2. 이름: `Floor`
3. **Transform**:
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - Scale: (5, 1, 5) - 크기에 맞게 조정

4. **Material 생성**:
   - Assets에서 우클릭 → Create → Material → `FloorMaterial`
   - Shader: URP/Lit
   - Base Map: 바닥 텍스처 할당
   - Floor 오브젝트에 드래그

### 5.2 집 모델 배치

1. **집 3D 모델/프리팹을 Hierarchy로 드래그**
2. **Parent Empty GameObject 생성**:
   - Hierarchy 우클릭 → Create Empty
   - 이름: `Environment`
   - 집 모델을 Environment 자식으로 이동

## TODO here
3. **Collider 확인**:
   - 집의 벽/바닥에 Collider가 있는지 확인
   - 없으면 Mesh Collider 또는 Box Collider 추가

### 5.3 조명 설정

1. **Directional Light 선택**
2. **설정**:
   - Rotation: (50, -30, 0) - 자연스러운 방향
   - Intensity: 1
   - Color: 약간 따뜻한 색 (255, 244, 214)

## 6. 재료 배치

### 6.1 Ingredients 부모 오브젝트

1. **Hierarchy 우클릭 → Create Empty**
2. 이름: `Ingredients`
3. Position: (0, 0, 0)

### 6.2 재료 오브젝트 생성 (예: Flour)

1. **3D Object → Cube 생성** (또는 3D 모델 사용)
2. 이름: `Ingredient_Flour`
3. **Transform**:
   - Position: 집 안 원하는 위치
   - Rotation: (0, 0, 0)
   - Scale: (0.5, 0.5, 0.5)

4. **Material 적용** (선택):
   - Material 생성 후 적절한 색상 설정
   - Flour는 흰색 등

5. **Collider 확인**:
   - Box Collider가 자동으로 추가됨
   - Is Trigger: ☐ (체크 해제)

6. **Add Component → Collectible Ingredient**
7. **설정**:
   - Ingredient Id: Flour
   - Interaction Radius: 1.5
   - Destroy On Collect: ✓
   - Collect Effect Prefab: None (선택)

8. **Ingredients 오브젝트의 자식으로 이동**

### 6.3 나머지 재료 생성

같은 방식으로 반복:
- `Ingredient_Sugar` (Ingredient Id: Sugar)
- `Ingredient_Egg` (Ingredient Id: Egg)
- `Ingredient_Butter` (Ingredient Id: Butter)
- `Ingredient_Strawberry` (Ingredient Id: Strawberry)

**팁**: 각 재료는 집 안 여러 곳에 흩어서 배치하세요.

## 7. UI 설정

### 7.1 Canvas 생성

1. **Hierarchy 우클릭 → UI → Canvas**
2. Canvas 설정 확인:
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

3. **EventSystem 자동 생성 확인**

### 7.2 GameplayUI 생성

1. **Canvas 자식으로 Empty GameObject 생성**
2. 이름: `GameplayUI`

### 7.3 재료 체크리스트 UI 생성

**방법 A: 퀘스트 스타일 UI 사용 (권장)**

1. **GameplayUI 우클릭 → UI → Panel** (또는 Empty GameObject)
2. 이름: `QuestPanel`
3. **RectTransform**:
   - Anchor: Top Left
   - Position: (20, -20, 0)
   - Width: 350
   - Height: 300
   - Image 컴포넌트 (선택): 반투명 배경 추가 가능

4. **QuestPanel 자식으로 UI 요소 생성**:
   - **제목 텍스트**: QuestPanel 우클릭 → UI → Text - TextMeshPro
     - 이름: `QuestTitleText`
     - RectTransform: Top Center, Position Y: -20
     - Font Size: 20
     - Text: "케이크 재료 모으기"
     - Alignment: Center, Top
   - **재료 목록 컨테이너**: QuestPanel 우클릭 → UI → Panel
     - 이름: `IngredientListContainer`
     - RectTransform: Stretch (Left, Top, Right, Bottom)
     - Margins: Left 20, Top 60, Right 20, Bottom 80
     - **Vertical Layout Group 추가**:
       - Spacing: 10
       - Child Alignment: Upper Left
       - Child Control Width: ✓
       - Child Control Height: ☐
       - Child Force Expand Width: ☐
       - Child Force Expand Height: ☐
   - **다음 퀘스트 영역**: QuestPanel 우클릭 → UI → Panel (선택)
     - 이름: `NextQuestArea`
     - RectTransform: Bottom Center, Position Y: 20
     - Width: 300, Height: 50
     - **초기 비활성화**
     - **다음 퀘스트 텍스트**: NextQuestArea 우클릭 → UI → Text - TextMeshPro
       - 이름: `NextQuestText`
       - Font Size: 18
       - Text: "케이크를 들고 소파로 가기"
       - Alignment: Center, Middle
       - Color: Gold 또는 Yellow

5. **QuestPanel 선택 → Add Component → Quest UI**
6. **Quest UI 컴포넌트 설정**:
   - Quest Panel: QuestPanel 드래그
   - Quest Title Text: QuestTitleText 드래그
   - Ingredient List Container: IngredientListContainer 드래그
   - Next Quest Area: NextQuestArea 드래그
   - Next Quest Text: NextQuestText 드래그
   - Quest Item Prefab: None (자동 생성) 또는 커스텀 프리팹
   - Ingredient Quest Title: "케이크 재료 모으기"
   - Next Quest Message: "케이크를 들고 소파로 가기"
   - Completed Color: 연한 초록색 (0.2, 0.8, 0.2)
   - Incomplete Color: 흰색
   - Check Mark Color: 초록색 (0.2, 0.8, 0.2)

**방법 B: 기존 TextMeshPro 방식 (간단)**

1. **GameplayUI 우클릭 → UI → Text - TextMeshPro**
2. 이름: `IngredientChecklistText`
3. **RectTransform**:
   - Anchor: Top Left
   - Position: (20, -540, 0) 또는 (200, -100, 0)
   - Width: 300
   - Height: 250

4. **TextMeshProUGUI 설정**:
   - Text: (비워둠, 스크립트가 자동 설정)
   - Font Size: 18
   - Color: White
   - Alignment: Left, Top
   - Wrapping: Enabled

5. **Add Component → Ingredient Checklist UI**
6. **설정**:
   - Checklist Text: 방금 만든 TextMeshPro 컴포넌트 드래그
   - Check Mark: ✓ 또는 [V]
   - Uncheck Mark: ☐ 또는 [ ]
   - Completed Color: Green (0, 255, 0)
   - Incomplete Color: White (255, 255, 255)

### 7.4 CreditsUI 생성

1. **Canvas 자식으로 Empty GameObject 생성**
2. 이름: `CreditsUI`
3. **Inspector에서 비활성화** (체크 해제)

4. **CreditsSlidePlayer 설정 (권장)**:
   - **CreditsUI 선택 → Add Component → Credits Slide Player**
   - **MessageText 생성**:
     - CreditsUI 우클릭 → UI → Text - TextMeshPro
     - 이름: `MessageText`
     - RectTransform: 화면 중앙/상단
     - Font Size: 60 (기본값)
     - Color: White
     - Alignment: Center, Middle
   - **LyricsText 생성**:
     - CreditsUI 우클릭 → UI → Text - TextMeshPro
     - 이름: `LyricsText`
     - RectTransform: 화면 하단
     - Font Size: 36 (기본값)
     - Color: White
     - Alignment: Center, Middle
   - **ReturnButton 생성**:
     - CreditsUI 우클릭 → UI → Button - TextMeshPro
     - 이름: `ReturnButton`
     - RectTransform: 화면 하단 중앙
     - Text: "타이틀로 돌아가기"
     - 초기 비활성화
   - **CreditsSlidePlayer 컴포넌트 설정**:
     - Ending Music: 엔딩 음악 파일 할당
     - Message Text: MessageText 드래그
     - Lyrics Text: LyricsText 드래그
     - Return Button: ReturnButton 드래그
     - Slides: Inspector에서 슬라이드 목록 추가 (시작 시간, 종료 시간, 텍스트, 타입 등)

5. **CreditsScroller 설정 (선택, 하위 호환성)**:
   - **CreditsScrollView 생성** (CreditsSlidePlayer 사용 시 비활성화):
     - CreditsUI 우클릭 → UI → Scroll View
     - 이름: `CreditsScrollView`
     - ScrollRect 설정: Vertical만 활성화
     - Content에 TextMeshPro 추가하여 크레딧 텍스트 작성
   - **CreditsUI 선택 → Add Component → Credits Scroller** (CreditsSlidePlayer 사용 시 비활성화)
   - CreditsScroller 컴포넌트 설정:
     - Credits Text: Content의 RectTransform
     - Return Button: ReturnButton (공유 가능)

### 7.5 EndingUI 생성

1. **Canvas 자식으로 Empty GameObject 생성**
2. 이름: `EndingUI`
3. **Inspector에서 비활성화** (체크 해제)

4. **EndingText 생성**:
   - EndingUI 우클릭 → UI → Text - TextMeshPro
   - 이름: `EndingText`
   - RectTransform: 화면 중앙
   - Text: "축하합니다!\n케이크가 완성되었습니다!"
   - Font Size: 48
   - Color: Gold
   - Alignment: Center, Middle

5. **RestartButton 생성** (선택):
   - EndingUI 우클릭 → UI → Button - TextMeshPro
   - 이름: `RestartButton`
   - RectTransform: 화면 하단 중앙
   - Text: "다시 하기"

### 7.6 InteractionPrompt (선택)

1. **Canvas 자식으로 Text - TextMeshPro 생성**
2. 이름: `InteractionPrompt`
3. **RectTransform**:
   - Anchor: Bottom Center
   - Position: (0, 100, 0)
   - Width: 200
   - Height: 50

4. **TextMeshProUGUI**:
   - Text: "[F] 수집하기"
   - Font Size: 24
   - Color: Yellow
   - Alignment: Center, Middle

5. **초기 비활성화**

## 8. 소파 상호작용 설정

### 8.1 SofaInteractable 추가

1. **소파 GameObject 선택** (또는 소파 프리팹)
2. **Add Component → Sofa Interactable**
3. **설정**:
   - Cutscene Controller: GameSystems → EndingCutsceneController 드래그
   - Ingredient Inventory: GameSystems → IngredientInventory 드래그
   - Interact Prompt: "소파에 앉기 [F]" (기본값)

### 8.2 Sofa Sit Position 생성

1. **Hierarchy 우클릭 → Create Empty**
2. 이름: `SofaSitPosition`
3. **Transform 설정**:
   - Position: 소파 위 앉을 위치 (소파 중앙 위 약간)
   - Rotation: 소파를 바라보는 방향
4. **EndingCutsceneController에 연결**:
   - GameSystems → EndingCutsceneController
   - Sofa Sit Position: SofaSitPosition 드래그

## 9. 컷씬 설정

### 9.1 Cutscene 부모 오브젝트

1. **Hierarchy 우클릭 → Create Empty**
2. 이름: `Cutscene`
3. Position: (0, 0, 0)

### 9.2 PlayableDirector 추가

1. **Cutscene 선택**
2. **Add Component → Playable Director**
3. **Timeline Asset 생성**:
   - Assets에서 우클릭 → Create → Timeline
   - 이름: `EndingCutscene`
   - PlayableDirector의 Playable 슬롯에 드래그

4. **Timeline 창 열기**:
   - Window → Sequencing → Timeline
   - Cutscene 오브젝트 선택 시 Timeline 창에 표시됨

### 9.3 Timeline 구성 (기본)

1. **Camera Track 추가** (선택):
   - Timeline 창에서 Add → Cinemachine Track 또는 Animation Track
   - 카메라 연출 추가 가능

2. **Activation Track 추가** (선택):
   - EndingUI를 제어하기 위한 Activation Track
   - 컷씬 끝에서 EndingUI 활성화

### 9.4 VideoPlayer 설정 (선택)

**방법 A: Timeline Video Track 사용 (권장)**

1. **Timeline 창에서 Add → Video Track**
2. **Video Clip 할당**:
   - Assets/Video/ 폴더에 mp4 파일 배치
   - Video Track에 클립 드래그

3. **Target 설정**:
   - Main Camera 또는 별도의 Quad 오브젝트

**방법 B: VideoPlayer 컴포넌트 사용**

1. **Cutscene 자식으로 Empty GameObject 생성**
2. 이름: `VideoPlayerObject`
3. **Add Component → Video Player**
4. **설정**:
   - Source: Video Clip
   - Video Clip: 엔딩 영상 할당
   - Render Mode: Camera Far Plane 또는 Render Texture
   - Target Camera: Main Camera
   - Play On Awake: ☐

5. **EndingCutsceneController에서 참조**:
   - GameSystems → EndingCutsceneController
   - Video Player 슬롯에 드래그

## 10. 최종 연결

### 10.1 EndingCutsceneController 연결

1. **GameSystems → EndingCutsceneController 선택**
2. **모든 참조 연결**:
   - Inventory: GameSystems → IngredientInventory
   - Director: EndingCutscene → PlayableDirector (또는 Cutscene → PlayableDirector)
   - Player Controller: Player → PlayerController
   - Gameplay UI Root: Canvas/GameplayUI (또는 Canvas 직접)
   - Ending UI Root: Canvas/EndingUI
   - Credits UI Root: Canvas/CreditsUI
   - Sofa Sit Position: SofaSitPosition (8.2에서 생성)
   - Video Player: EndingCutscene/VideoPlayerObject (선택)
   - Control Video From Script: false (Timeline에서 제어)
   - Ending Music: 엔딩 크레딧 배경음 파일 (선택)

### 10.2 Interactor 연결 (선택)

1. **Player → Interactor 선택**
2. **Interaction Prompt**: Canvas/InteractionPrompt 연결 (WorldSpaceInteractionPrompt 사용 시 불필요)

### 10.3 SceneLoader 설정 (자동)

SceneLoader는 DontDestroyOnLoad로 자동 생성되며, 씬 전환 시 자동으로 배경음악을 재생합니다. 별도 설정이 필요 없습니다.

## 11. 테스트

### 11.1 기본 동작 테스트

1. **Play 모드 진입**
2. **WASD로 이동 테스트**
3. **Shift로 달리기 테스트**
4. **카메라 추적 확인**

### 11.2 재료 수집 테스트

1. **재료 근처로 이동**
2. **F키로 수집** (E키도 호환)
3. **UI 체크리스트 업데이트 확인**
4. **IngredientChecklistText가 화면에 보이는지 확인**

### 11.3 엔딩 컷씬 테스트

1. **모든 재료 수집**
2. **소파 근처로 이동**
3. **F키로 소파에 앉기**
4. **Timeline 컷씬 재생 확인**
5. **크레딧 UI 표시 확인** (CreditsSlidePlayer 또는 CreditsScroller)
6. **엔딩 음악 재생 확인** (설정한 경우)

## 12. 최적화 및 마무리

### 12.1 조명 베이크 (선택)

1. **Window → Rendering → Lighting**
2. **Mixed Lighting → Baked Indirect**
3. **Generate Lighting** 클릭

### 12.2 Occlusion Culling (선택)

1. **Window → Rendering → Occlusion Culling**
2. **Bake** 탭에서 Bake 실행

### 12.3 빌드 설정

1. **File → Build Settings**
2. **Add Open Scenes** 클릭
3. **Platform**: PC, Mac & Linux Standalone
4. **Build** 또는 **Build And Run**

## 문제 해결

### 캐릭터가 바닥을 뚫고 떨어짐

- Floor에 Collider가 있는지 확인
- CharacterController의 Ground Check Distance 증가

### UI가 보이지 않음

- Canvas의 Render Mode 확인
- EventSystem이 존재하는지 확인
- UI 오브젝트가 활성화되어 있는지 확인
- IngredientChecklistText의 RectTransform 위치 확인 (Y: -540은 화면 밖일 수 있음)
- GameplayUI 또는 Canvas가 비활성화되어 있지 않은지 확인
- EndingCutsceneController의 Gameplay UI Root가 올바르게 연결되었는지 확인

### 재료 수집이 안 됨

- Ingredient 오브젝트에 Collider가 있는지 확인
- Interactor의 Detection Radius 증가
- IngredientInventory가 씬에 존재하는지 확인

### 컷씬이 재생되지 않음

- PlayableDirector의 Playable에 Timeline Asset이 할당되었는지 확인
- PlayableDirector의 Play On Awake가 비활성화되어 있는지 확인 (소파 상호작용으로 시작해야 함)
- EndingCutsceneController의 모든 참조가 연결되었는지 확인 (특히 Director, Sofa Sit Position)
- SofaInteractable이 소파에 추가되어 있고 올바르게 설정되었는지 확인
- 모든 재료를 수집했는지 확인
- Console에서 에러 메시지 확인

### 크레딧이 표시되지 않음

- CreditsUI가 활성화되어 있는지 확인
- CreditsSlidePlayer가 활성화되어 있고 MessageText, LyricsText가 할당되었는지 확인
- CreditsScroller를 사용하는 경우 CreditsScrollView가 활성화되어 있는지 확인
- CreditsSlidePlayer와 CreditsScroller 중 하나만 활성화되어 있어야 함
- EndingCutsceneController의 Credits UI Root가 올바르게 연결되었는지 확인

---

**축하합니다!** Birthday Cake Quest 씬 구성이 완료되었습니다.

