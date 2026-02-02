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
│   ├── GameplayUI (Empty GameObject)
│   │   └── IngredientChecklist (TextMeshPro)
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
   - Gameplay UI Root: None (나중에 연결)
   - Ending UI Root: None (나중에 연결)
   - Video Player: None (선택)
   - Control Video From Script: false (Timeline에서 제어)

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
   - Interact Key: E
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

### 7.3 IngredientChecklist 생성

1. **GameplayUI 우클릭 → UI → Text - TextMeshPro**
2. 이름: `IngredientChecklist`
3. **RectTransform**:
   - Anchor: Top Left
   - Position: (200, -100, 0)
   - Width: 300
   - Height: 250

4. **TextMeshProUGUI 설정**:
   - Text: (비워둠, 스크립트가 자동 설정)
   - Font Size: 18
   - Color: White
   - Alignment: Left, Top

5. **Add Component → Ingredient Checklist UI**
6. **설정**:
   - Checklist Text: 방금 만든 TextMeshPro 컴포넌트 드래그
   - Check Mark: ✓
   - Uncheck Mark: ☐
   - Completed Color: Green (0, 255, 0)
   - Incomplete Color: White (255, 255, 255)

### 7.4 EndingUI 생성

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

### 7.5 InteractionPrompt (선택)

1. **Canvas 자식으로 Text - TextMeshPro 생성**
2. 이름: `InteractionPrompt`
3. **RectTransform**:
   - Anchor: Bottom Center
   - Position: (0, 100, 0)
   - Width: 200
   - Height: 50

4. **TextMeshProUGUI**:
   - Text: "[E] 수집하기"
   - Font Size: 24
   - Color: Yellow
   - Alignment: Center, Middle

5. **초기 비활성화**

## 8. 컷씬 설정

### 8.1 Cutscene 부모 오브젝트

1. **Hierarchy 우클릭 → Create Empty**
2. 이름: `Cutscene`
3. Position: (0, 0, 0)

### 8.2 PlayableDirector 추가

1. **Cutscene 선택**
2. **Add Component → Playable Director**
3. **Timeline Asset 생성**:
   - Assets에서 우클릭 → Create → Timeline
   - 이름: `EndingCutscene`
   - PlayableDirector의 Playable 슬롯에 드래그

4. **Timeline 창 열기**:
   - Window → Sequencing → Timeline
   - Cutscene 오브젝트 선택 시 Timeline 창에 표시됨

### 8.3 Timeline 구성 (기본)

1. **Camera Track 추가** (선택):
   - Timeline 창에서 Add → Cinemachine Track 또는 Animation Track
   - 카메라 연출 추가 가능

2. **Activation Track 추가** (선택):
   - EndingUI를 제어하기 위한 Activation Track
   - 컷씬 끝에서 EndingUI 활성화

### 8.4 VideoPlayer 설정 (선택)

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

## 9. 최종 연결

### 9.1 EndingCutsceneController 연결

1. **GameSystems → EndingCutsceneController 선택**
2. **모든 참조 연결**:
   - Inventory: GameSystems → IngredientInventory
   - Director: Cutscene → PlayableDirector
   - Player Controller: Player → PlayerController
   - Gameplay UI Root: Canvas/GameplayUI
   - Ending UI Root: Canvas/EndingUI
   - Video Player: Cutscene/VideoPlayerObject (선택)

### 9.2 Interactor 연결 (선택)

1. **Player → Interactor 선택**
2. **Interaction Prompt**: Canvas/InteractionPrompt 연결

## 10. 테스트

### 10.1 기본 동작 테스트

1. **Play 모드 진입**
2. **WASD로 이동 테스트**
3. **Shift로 달리기 테스트**
4. **카메라 추적 확인**

### 10.2 재료 수집 테스트

1. **재료 근처로 이동**
2. **E키로 수집**
3. **UI 체크리스트 업데이트 확인**

### 10.3 엔딩 컷씬 테스트

1. **모든 재료 수집**
2. **자동으로 컷씬 재생 확인**
3. **엔딩 UI 표시 확인**

## 11. 최적화 및 마무리

### 11.1 조명 베이크 (선택)

1. **Window → Rendering → Lighting**
2. **Mixed Lighting → Baked Indirect**
3. **Generate Lighting** 클릭

### 11.2 Occlusion Culling (선택)

1. **Window → Rendering → Occlusion Culling**
2. **Bake** 탭에서 Bake 실행

### 11.3 빌드 설정

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

### 재료 수집이 안 됨

- Ingredient 오브젝트에 Collider가 있는지 확인
- Interactor의 Detection Radius 증가
- IngredientInventory가 씬에 존재하는지 확인

### 컷씬이 재생되지 않음

- PlayableDirector의 Playable에 Timeline Asset이 할당되었는지 확인
- EndingCutsceneController의 모든 참조가 연결되었는지 확인
- Console에서 에러 메시지 확인

---

**축하합니다!** Birthday Cake Quest 씬 구성이 완료되었습니다.

