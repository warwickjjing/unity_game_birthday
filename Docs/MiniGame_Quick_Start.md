# 🎮 미니게임 빠른 시작 가이드

## 설탕 미니게임 테스트하기 (5분)

### 1️⃣ MiniGameManager 생성

```
Hierarchy → Create Empty → "MiniGameManager"
Add Component → MiniGameManager
```

### 2️⃣ MiniGame Canvas 생성

```
Hierarchy → UI → Canvas → "MiniGameCanvas"

Canvas 설정:
- Render Mode: Screen Space - Overlay
- Canvas Scaler → UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080

초기 상태: 비활성화 (체크 해제)
```

### 3️⃣ SugarMiniGamePanel 생성

```
MiniGameCanvas → UI → Panel → "SugarMiniGamePanel"
크기: 800 x 600
```

#### Panel 안에 UI 요소 추가:

1. **Title** (Text - TextMeshPro)
   - Text: "설탕 계량하기"
   - Font Size: 48

2. **Instructions** (Text - TextMeshPro)
   - Text: "마우스를 눌러 설탕을 따르세요!"
   - Font Size: 28

3. **Timer** (Text - TextMeshPro)
   - Text: "남은 시간: 10.0초"
   - Position Y: -200

4. **Container** (Image)
   - 크기: 200 x 400
   - 색상: 회색

5. **SugarFill** (Image, Container의 자식)
   - Anchor: Stretch
   - Image Type: Filled
   - Fill Method: Vertical
   - Fill Origin: Bottom
   - Fill Amount: 0

6. **TargetZone** (Image)
   - 크기: 220 x 80
   - 색상: 초록색 반투명 (Alpha: 100)

7. **ResultPanel** (Panel, 초기 비활성)
   - 크기: 600 x 300
   - 자식으로:
     - **ResultText** (Text - TextMeshPro)
     - **RetryButton** (Button - TextMeshPro): "다시 시도"
     - **ContinueButton** (Button - TextMeshPro): "계속하기"

### 4️⃣ 스크립트 연결

1. **SugarMiniGamePanel 선택**
   - Add Component → `SugarPouringMiniGame`

2. **Inspector에서 참조 연결**:
   - Sugar Fill Image → SugarFill
   - Timer Text → Timer
   - Instructions Text → Instructions
   - Result Panel → ResultPanel
   - Result Text → ResultText
   - Retry Button → RetryButton
   - Continue Button → ContinueButton
   - Target Zone Image → TargetZone

3. **MiniGameManager 선택**
   - Mini Game Canvas → MiniGameCanvas
   - Sugar Mini Game Panel → SugarMiniGamePanel
   - Player Controller → Player (드래그)
   - Interactor → Player (드래그)
   - Isometric Camera → Main Camera (드래그)

### 5️⃣ 설탕 재료 설정

```
Hierarchy에서 Ingredient_Sugar 선택

CollectibleIngredient 설정:
- Requires Mini Game: ✓ (체크)
- Mini Game Type: Sugar
```

### 6️⃣ 테스트!

1. Play 버튼 클릭
2. 설탕 재료로 이동
3. E키 누르기
4. 마우스 버튼 누르고 있기
5. 게이지가 80-100% 사이에서 놓기
6. 성공!

---

## 문제 해결

### UI가 안 보임
- MiniGameCanvas가 비활성화되어 있는지 확인 (정상)
- 게임 중에만 활성화됨

### E키를 눌러도 미니게임이 안 열림
- Ingredient_Sugar의 "Requires Mini Game" 체크 확인
- MiniGameManager에 모든 참조 연결 확인

### 게이지가 안 채워짐
- SugarPouringMiniGame의 UI 참조 확인
- SugarFill Image가 올바르게 연결되었는지 확인

---

**상세 가이드**: [MiniGame_Setup_Guide.md](MiniGame_Setup_Guide.md)

