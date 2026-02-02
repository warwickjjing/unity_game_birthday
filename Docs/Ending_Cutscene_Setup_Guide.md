# 엔딩 컷씬 고도화 설정 가이드

이 문서는 집 환경 구축부터 타이틀 화면, 엔딩 크레딧까지 완전한 게임 플로우를 Unity 에디터에서 설정하는 방법을 단계별로 안내합니다.

## 📋 사전 준비

모든 스크립트 파일이 이미 생성되었습니다:
- ✅ IInteractable.cs
- ✅ CakeHolder.cs
- ✅ SofaInteractable.cs
- ✅ CreditsScroller.cs
- ✅ SceneLoader.cs
- ✅ TitleScreenUI.cs
- ✅ EndingSignalReceiver.cs
- ✅ 기존 스크립트 수정 완료

## 1단계: 집 환경 구축 (거실, 소파, 티비)

### 1.1 거실 구조 생성

1. **Hierarchy에서 Environment 선택**
2. **우클릭 → Create Empty → 이름: `LivingRoom`**

### 1.2 거실 바닥

1. **LivingRoom 우클릭 → 3D Object → Plane**
2. **이름: `LivingRoomFloor`**
3. **Transform**:
   - Position: (15, 0, 0) - 기존 환경과 떨어진 곳
   - Scale: (2, 1, 2)

### 1.3 거실 벽 (4개)

**벽 1 (뒤쪽)**
1. **LivingRoom 우클릭 → 3D Object → Cube**
2. **이름: `Wall_Back`**
3. **Transform**:
   - Position: (15, 2.5, 10)
   - Scale: (20, 5, 0.2)

**벽 2 (앞쪽)**
1. **Cube 생성 → 이름: `Wall_Front`**
2. **Transform**:
   - Position: (15, 2.5, -10)
   - Scale: (20, 5, 0.2)

**벽 3 (왼쪽)**
1. **Cube 생성 → 이름: `Wall_Left`**
2. **Transform**:
   - Position: (5, 2.5, 0)
   - Rotation: (0, 90, 0)
   - Scale: (20, 5, 0.2)

**벽 4 (오른쪽)**
1. **Cube 생성 → 이름: `Wall_Right`**
2. **Transform**:
   - Position: (25, 2.5, 0)
   - Rotation: (0, 90, 0)
   - Scale: (20, 5, 0.2)

### 1.4 소파 (Sofa)

**소파 베이스**
1. **LivingRoom 우클릭 → Create Empty → 이름: `Sofa`**
2. **Position: (15, 0, 5)**

**소파 좌석**
1. **Sofa 우클릭 → 3D Object → Cube**
2. **이름: `Sofa_Seat`**
3. **Transform**:
   - Position: (0, 0.5, 0)
   - Scale: (3, 0.5, 1.5)

**소파 등받이**
1. **Sofa 우클릭 → 3D Object → Cube**
2. **이름: `Sofa_Back`**
3. **Transform**:
   - Position: (0, 1.5, -0.5)
   - Scale: (3, 2, 0.3)

**소파 앉기 위치 마커**
1. **Sofa 우클릭 → Create Empty**
2. **이름: `SofaSitPosition`**
3. **Position: (0, 0.5, 0)**
4. **Rotation: (0, 0, 0)**

### 1.5 티비 (TV)

**TV 루트**
1. **LivingRoom 우클릭 → Create Empty → 이름: `TV`**
2. **Position: (15, 1.5, 8)**

**TV 스탠드**
1. **TV 우클릭 → 3D Object → Cube**
2. **이름: `TV_Stand`**
3. **Transform**:
   - Position: (0, -1, 0)
   - Scale: (2, 0.3, 0.5)

**TV 스크린 (Quad)**
1. **TV 우클릭 → 3D Object → Quad**
2. **이름: `TV_Screen`**
3. **Transform**:
   - Position: (0, 0, 0)
   - Rotation: (0, 180, 0)
   - Scale: (3, 2, 1)

---

## 2단계: 재료 Prefab 업데이트

### 2.1 재료별 설정

**Ingredient_Flour (밀가루)**
1. **Project → Assets/Prefabs/Ingredient_Flour 더블클릭**
2. **하위 Cube 선택 → Material**:
   - Albedo Color: 흰색 (255, 255, 255)

**Ingredient_Sugar (설탕)**
1. **Ingredient_Sugar 프리팹 열기**
2. **Material Albedo Color**: 연한 갈색 (210, 180, 140)

**Ingredient_Egg (계란)**
1. **Ingredient_Egg 프리팹 열기**
2. **하위 Cube 삭제 → 3D Object → Sphere 추가**
3. **Material Albedo Color**: 노란색/흰색 (255, 235, 100)

**Ingredient_Butter (버터)**
1. **Ingredient_Butter 프리팹 열기**
2. **Material Albedo Color**: 노란색 (255, 220, 80)

**Ingredient_Strawberry (딸기)**
1. **Ingredient_Strawberry 프리팹 열기**
2. **하위 Cube 삭제 → 3D Object → Capsule 추가**
3. **Material Albedo Color**: 빨간색 (220, 50, 50)

---

## 3단계: 케이크 오브젝트 시스템

### 3.1 케이크 생성

1. **Player GameObject 선택**
2. **우클릭 → Create Empty → 이름: `CakeHolder`**
3. **Transform**:
   - Position: (0.3, 1.2, 0.3) - 손 위치
   - Scale: (1, 1, 1)

4. **CakeHolder 우클릭 → 3D Object → Cylinder**
5. **이름: `CakeBase`**
6. **Transform**:
   - Position: (0, 0, 0)
   - Scale: (0.3, 0.15, 0.3)
7. **Material**: 연한 크림색 (255, 250, 240)

8. **CakeHolder 우클릭 → 3D Object → Cone**
9. **이름: `CakeTopping`**
10. **Transform**:
    - Position: (0, 0.2, 0)
    - Rotation: (180, 0, 0)
    - Scale: (0.15, 0.2, 0.15)
11. **Material**: 빨간색 (딸기) (220, 50, 50)

### 3.2 PlayerController 연결

1. **Player GameObject 선택**
2. **Inspector → Player Controller 컴포넌트**
3. **Cake Holder 필드**: CakeHolder GameObject 드래그

---

## 4단계: 소파 인터랙션 설정

### 4.1 Sofa에 Collider 추가

1. **Hierarchy → Sofa 선택**
2. **Add Component → Box Collider**
3. **Center: (0, 0.5, 0)**
4. **Size: (3, 1, 2)**

### 4.2 SofaInteractable 컴포넌트 추가

1. **Sofa 선택**
2. **Add Component → Sofa Interactable**
3. **설정**:
   - **Cutscene Controller**: EndingCutscene GameObject 드래그
   - **Inventory**: GameSystems/IngredientInventory 드래그
   - **Interact Prompt**: "소파에 앉기 [E]"
   - **Incomplete Prompt**: "케이크를 완성하세요"

### 4.3 EndingCutsceneController 설정

1. **EndingCutscene GameObject 선택**
2. **Inspector → Ending Cutscene Controller**
3. **새 필드 설정**:
   - **Credits UI Root**: (다음 단계에서 생성 후 연결)
   - **Sofa Sit Position**: Sofa/SofaSitPosition 드래그

---

## 5단계: Timeline 확장

### 5.1 Timeline 창 열기

1. **EndingCutscene GameObject 선택**
2. **Window → Sequencing → Timeline**

### 5.2 Cinemachine 설치 (카메라 이동용)

1. **Window → Package Manager**
2. **Packages: Unity Registry**
3. **Cinemachine 검색 → Install**

### 5.3 Virtual Camera 생성

1. **Hierarchy → Main Camera 우클릭**
2. **Cinemachine → Virtual Camera**
3. **이름: `VCam_TV`**
4. **Transform**:
   - Position: TV 화면을 바라보는 위치
   - Look At Target: TV_Screen

### 5.4 Timeline에 Cinemachine Track 추가

1. **Timeline 창에서 빈 공간 우클릭**
2. **Cinemachine Track**
3. **Track 우클릭 → Add Cinemachine Shot**
4. **Virtual Camera 필드**: VCam_TV 드래그
5. **Clip 길이**: 3초 (카메라 이동 시간)

### 5.5 Animation Track 추가 (플레이어 앉기)

1. **Timeline → Add → Animation Track**
2. **Player GameObject를 Track으로 드래그**
3. **우클릭 → Add From Animation Clip** (나중에 애니메이션 추가)
4. **또는** Transform 위치만 조정 (간단)

---

## 6단계: RenderTexture 티비 영상 시스템

### 6.1 RenderTexture 생성

1. **Project → Assets 우클릭 → Create → Folder → 이름: `RenderTextures`**
2. **RenderTextures 폴더 우클릭 → Create → Render Texture**
3. **이름: `TVScreen`**
4. **Inspector 설정**:
   - **Size**: 1920 x 1080
   - **Depth Buffer**: 24 bit
   - **Anti-aliasing**: None

### 6.2 TV Screen Material 생성

1. **Project → Assets/Materials 폴더**
2. **우클릭 → Create → Material → 이름: `TVScreenMaterial`**
3. **Inspector**:
   - **Shader**: Universal Render Pipeline/Unlit
   - **Base Map**: TVScreen RenderTexture 드래그
   - **Surface Type**: Opaque

4. **Hierarchy → TV_Screen 선택**
5. **Material**: TVScreenMaterial 드래그

### 6.3 VideoPlayer 설정 변경

1. **VideoPlayerObject GameObject 선택**
2. **Inspector → Video Player 컴포넌트**
3. **Render Mode**: **Render Texture** (Camera Near Plane에서 변경)
4. **Target Texture**: TVScreen RenderTexture 드래그

---

## 7단계: 엔딩 크레딧 UI

### 7.1 Credits UI Panel 생성

1. **Hierarchy → Canvas 선택**
2. **우클릭 → UI → Panel → 이름: `CreditsUI`**
3. **Rect Transform**:
   - Anchor: Stretch (전체)
   - Left, Top, Right, Bottom: 0

4. **Image 컴포넌트**:
   - **Color**: 검은색 (0, 0, 0, 255)

### 7.2 Credits Text 생성

1. **CreditsUI 우클릭 → UI → Scroll View**
2. **이름: `CreditsScrollView`**
3. **Scroll Rect 컴포넌트**:
   - **Vertical**: ✓
   - **Horizontal**: ☐
   - **Movement Type**: Elastic

4. **CreditsScrollView → Viewport → Content 선택**
5. **우클릭 → UI → Text - TextMeshPro**
6. **이름: `CreditsText`**
7. **Text 내용**:
```
Birthday Cake Quest

게임 제작
[당신의 이름]

특별한 날을 위한 특별한 게임

영상 출연
[주인공 이름]

감사합니다
이 게임을 플레이해 주셔서 감사합니다

© 2026
```

8. **TextMeshPro 설정**:
   - **Font**: 한글 폰트 (이전에 생성한 Noto Sans KR)
   - **Font Size**: 48
   - **Alignment**: Center
   - **Color**: 흰색

9. **Content Rect Transform**:
   - **Width**: 800
   - **Height**: 2000 (텍스트 길이에 맞춤)

### 7.3 Return Button 생성

1. **CreditsUI 우클릭 → UI → Button - TextMeshPro**
2. **이름: `ReturnButton`**
3. **Rect Transform**:
   - **Anchor**: Bottom Center
   - **Pos Y**: 100
   - **Width**: 300
   - **Height**: 80

4. **Text 변경**: "타이틀로 돌아가기"

### 7.4 CreditsScroller 컴포넌트 추가

1. **CreditsUI 선택**
2. **Add Component → Credits Scroller**
3. **설정**:
   - **Scroll Speed**: 50
   - **Duration**: 15 (초)
   - **Wait After Complete**: 2
   - **Credits Text**: Content (RectTransform) 드래그
   - **Return Button**: ReturnButton GameObject 드래그

### 7.5 ReturnButton 이벤트 연결

1. **ReturnButton 선택**
2. **Button 컴포넌트 → On Click()**
3. **+ 버튼 클릭**
4. **None (Object)**: SceneLoader 프리팹/GameObject 드래그 (다음 단계에서 생성)
5. **Function**: SceneLoader → LoadTitleScene()

### 7.6 EndingCutsceneController 연결

1. **EndingCutscene 선택**
2. **Ending Cutscene Controller**:
   - **Credits UI Root**: CreditsUI 드래그

---

## 8단계: 타이틀 화면 씬

### 8.1 새 씬 생성

1. **File → New Scene**
2. **3D (URP)** 선택
3. **File → Save As → Assets/Scenes/TitleScene.unity**

### 8.2 Canvas 생성

1. **Hierarchy 우클릭 → UI → Canvas**
2. **Canvas 설정**:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**:
     - **UI Scale Mode**: Scale With Screen Size
     - **Reference Resolution**: 1920 x 1080

### 8.3 배경 Panel

1. **Canvas 우클릭 → UI → Panel**
2. **이름: `Background`**
3. **Image Color**: 하늘색 또는 원하는 색 (100, 150, 250)

### 8.4 타이틀 텍스트

1. **Canvas 우클릭 → UI → Text - TextMeshPro**
2. **이름: `TitleText`**
3. **Rect Transform**:
   - **Anchor**: Top Center
   - **Pos Y**: -200
   - **Width**: 800
   - **Height**: 200

4. **TextMeshPro**:
   - **Text**: "Birthday Cake Quest"
   - **Font Size**: 80
   - **Alignment**: Center
   - **Color**: 흰색

### 8.5 Start Button

1. **Canvas 우클릭 → UI → Button - TextMeshPro**
2. **이름: `StartButton`**
3. **Rect Transform**:
   - **Anchor**: Middle Center
   - **Pos Y**: -50
   - **Width**: 300
   - **Height**: 80

4. **Text**: "게임 시작"

### 8.6 Quit Button (선택)

1. **Canvas 우클릭 → UI → Button - TextMeshPro**
2. **이름: `QuitButton`**
3. **Rect Transform**:
   - **Pos Y**: -150
   - **Width**: 300
   - **Height**: 80

4. **Text**: "종료"

### 8.7 TitleScreenUI 컴포넌트 추가

1. **Canvas 선택**
2. **Add Component → Title Screen UI**
3. **설정**:
   - **Start Button**: StartButton 드래그
   - **Quit Button**: QuitButton 드래그
   - **Main Scene Name**: "Home"

---

## 9단계: SceneLoader 설정

### 9.1 SceneLoader GameObject 생성 (Home 씬)

1. **Home.unity 씬 열기**
2. **Hierarchy 우클릭 → Create Empty**
3. **이름: `SceneLoader`**
4. **Add Component → Scene Loader**
5. **GameObject → DontDestroyOnLoad 체크 안 함** (스크립트가 자동 처리)

### 9.2 CreditsUI ReturnButton 연결

1. **Home 씬 → CreditsUI → ReturnButton 선택**
2. **Button → On Click()**:
   - **GameObject**: SceneLoader 드래그
   - **Function**: SceneLoader → LoadTitleScene()

---

## 10단계: Build Settings 등록

### 10.1 Build Settings 열기

1. **File → Build Settings**

### 10.2 씬 추가

1. **Add Open Scenes 버튼 클릭** (TitleScene이 열려 있을 때)
2. **TitleScene이 인덱스 0에 있도록 드래그**
3. **Home.unity 씬 열기**
4. **Build Settings → Add Open Scenes**
5. **Home이 인덱스 1에 있도록 확인**

**최종 순서**:
```
0: TitleScene
1: Home
```

---

## 11단계: 최종 연결 및 테스트

### 11.1 Player 설정 확인

1. **Home 씬 → Player 선택**
2. **Player Controller**:
   - **Cake Holder**: CakeHolder GameObject 연결 ✓

### 11.2 Interactor 레이어 설정

1. **Player → Interactor 컴포넌트**
2. **Interaction Layer**: Everything (모든 IInteractable 감지)

### 11.3 재료 배치 (거실 포함)

1. **Ingredients 폴더에서 재료들을 거실 곳곳에 배치**
2. **예시 위치**:
   - Flour: 소파 옆 (14, 0.5, 5)
   - Sugar: TV 앞 (15, 0.5, 7)
   - Egg, Butter, Strawberry: 거실 구석

### 11.4 테스트 플레이

1. **Play 버튼 클릭**
2. **확인 사항**:
   - ✓ 플레이어 이동 (WASD)
   - ✓ 재료 수집 (E키)
   - ✓ 5개 수집 시 케이크 표시
   - ✓ 소파 앞에서 E키 → 엔딩 컷씬
   - ✓ 카메라 티비로 이동
   - ✓ 티비에 영상 재생
   - ✓ 영상 종료 후 크레딧 스크롤
   - ✓ "타이틀로 돌아가기" 버튼 → TitleScene 전환

3. **TitleScene 테스트**:
   - ✓ "게임 시작" 버튼 → Home 씬 로드
   - ✓ Enter 키로도 시작 가능

---

## 🎯 트러블슈팅

### 문제: 소파 인터랙션이 안 됨
**해결**: 
- Sofa에 Collider가 있는지 확인
- SofaInteractable의 Inventory가 연결되었는지 확인
- 모든 재료를 수집했는지 확인

### 문제: 티비에 영상이 안 나옴
**해결**:
- VideoPlayer의 Render Mode가 "Render Texture"인지 확인
- Target Texture가 TVScreen RenderTexture인지 확인
- TV_Screen의 Material이 TVScreenMaterial인지 확인

### 문제: 크레딧이 안 보임
**해결**:
- CreditsUI가 Canvas 하위에 있는지 확인
- CreditsScroller 컴포넌트 설정 확인
- EndingCutsceneController의 Credits UI Root 연결 확인

### 문제: 씬 전환이 안 됨
**해결**:
- Build Settings에 두 씬이 모두 등록되었는지 확인
- TitleScene이 인덱스 0, Home이 인덱스 1인지 확인
- SceneLoader GameObject가 있는지 확인

---

## ✅ 완료 체크리스트

- [ ] 거실 환경 구축 (바닥, 벽, 소파, 티비)
- [ ] 재료 Prefab 색상/형태 변경
- [ ] 케이크 오브젝트 생성 및 PlayerController 연결
- [ ] SofaInteractable 설정
- [ ] Timeline에 Cinemachine Track 추가
- [ ] RenderTexture 티비 시스템 구축
- [ ] 크레딧 UI 및 CreditsScroller 설정
- [ ] TitleScene 생성 및 UI 구성
- [ ] Build Settings에 씬 등록
- [ ] 전체 플로우 테스트

모든 단계를 완료하면 완전한 게임 플로우가 구현됩니다! 🎉

