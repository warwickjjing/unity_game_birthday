# QuestUI 설정 가이드

기존 `IngredientChecklistUI`를 `QuestUI`로 교체하는 설정 가이드입니다.

## 📋 목차

1. [기존 UI 제거](#1-기존-ui-제거)
2. [QuestPanel 생성](#2-questpanel-생성)
3. [QuestItem 프리팹 생성 (선택)](#3-questitem-프리팹-생성-선택)
4. [QuestUI 컴포넌트 설정](#4-questui-컴포넌트-설정)
5. [체크박스 UI 구성](#5-체크박스-ui-구성)

---

## 1. 기존 UI 제거

### 1.1 IngredientChecklistText 제거

1. **Hierarchy에서 `Canvas > GameplayUI > IngredientChecklistText` 선택**
2. **Delete 키 누르기** 또는 **우클릭 → Delete**

또는 비활성화만 원하면:
- Inspector에서 체크박스 해제 (비활성화)

---

## 2. QuestPanel 생성

### 2.1 QuestPanel 기본 생성

1. **Hierarchy에서 `Canvas > GameplayUI` 선택** (또는 Canvas 직접 자식으로)
2. **GameplayUI 우클릭 → UI → Panel**
3. 이름: `QuestPanel`

### 2.2 QuestPanel RectTransform 설정

**Inspector에서 RectTransform 설정:**
- **Anchor Presets**: 좌측 상단 고정
  - RectTransform의 좌측 상단 아이콘(□) 클릭
  - 또는 Anchor Presets 창에서 좌측 상단 선택
- **Position**:
  - Pos X: `20` (좌측 여백)
  - Pos Y: `-20` (상단 여백, 음수)
  - Pos Z: `0`
- **Size**:
  - Width: `350`
  - Height: `300`

### 2.3 QuestPanel Image 설정 (선택)

**Inspector에서 Image 컴포넌트:**
- Source Image: None (투명) 또는 반투명 배경 이미지
- Color: 원하는 배경색 (예: 검은색 50% 투명도)

---

## 3. QuestPanel 자식 UI 요소 생성

### 3.1 QuestTitleText 생성

1. **QuestPanel 우클릭 → UI → Text - TextMeshPro**
2. 이름: `QuestTitleText`

**RectTransform 설정:**
- Anchor: Top Center
- Position: X=0, Y=-20, Z=0
- Width: 300, Height: 40

**TextMeshProUGUI 설정:**
- Text: `케이크 재료 모으기` (임시, 스크립트가 자동 변경)
- Font Size: `20` (Inspector에서 조정 가능)
- Alignment: Center, Top
- Color: White (Inspector에서 조정 가능)

### 3.2 IngredientListContainer 생성

1. **QuestPanel 우클릭 → UI → Panel**
2. 이름: `IngredientListContainer`

**RectTransform 설정:**
- Anchor: Stretch (Left, Top, Right, Bottom)
- Left: `20`
- Top: `60`
- Right: `20`
- Bottom: `80`

**Vertical Layout Group 추가:**
- QuestPanel 선택 → Add Component → Vertical Layout Group
- **설정:**
  - Spacing: `10`
  - Child Alignment: Upper Left
  - Child Control Width: ✓
  - Child Control Height: ☐
  - Child Force Expand Width: ☐
  - Child Force Expand Height: ☐

**Image 컴포넌트 (선택):**
- Source Image: None (투명) 또는 배경 이미지
- Color: 투명 또는 반투명

### 3.3 NextQuestArea 생성

1. **QuestPanel 우클릭 → UI → Panel** (선택)
2. 이름: `NextQuestArea`

**RectTransform 설정:**
- Anchor: Bottom Center
- Position: X=0, Y=20, Z=0
- Width: 300, Height: 50

**초기 비활성화:**
- Inspector에서 체크박스 해제 (비활성화)

**NextQuestText 생성:**
1. **NextQuestArea 우클릭 → UI → Text - TextMeshPro**
2. 이름: `NextQuestText`

**RectTransform 설정:**
- Anchor: Stretch (Left, Top, Right, Bottom)
- 모든 마진: 0

**TextMeshProUGUI 설정:**
- Text: `케이크를 들고 소파로 가기` (임시, 스크립트가 자동 변경)
- Font Size: `18` (Inspector에서 조정 가능)
- Alignment: Center, Middle
- Color: Gold 또는 Yellow (Inspector에서 조정 가능)

---

## 4. QuestUI 컴포넌트 설정

### 4.1 QuestUI 컴포넌트 추가

1. **QuestPanel 선택**
2. **Add Component → Quest UI**

### 4.2 QuestUI 필드 연결

**UI References:**
- Quest Panel: `QuestPanel` 드래그
- Quest Title Text: `QuestTitleText` 드래그
- Ingredient List Container: `IngredientListContainer` 드래그
- Next Quest Area: `NextQuestArea` 드래그
- Next Quest Text: `NextQuestText` 드래그
- Quest Item Prefab: None (자동 생성) 또는 [프리팹 생성](#3-questitem-프리팹-생성-선택) 후 드래그

**Display Settings:**
- Ingredient Quest Title: `케이크 재료 모으기`
- Next Quest Message: `케이크를 들고 소파로 가기`
- Ingredient Names: 기본값 유지 (밀가루, 설탕, 계란, 버터, 딸기)

**Visual Settings - Colors Only:**
- Completed Color: 연한 초록색 `(0.2, 0.8, 0.2)`
- Incomplete Color: 흰색 `(1, 1, 1)`
- Check Mark Color: 초록색 `(0.2, 0.8, 0.2)`

**Check Mark Settings:**
- Uncheck Mark: `[ ]` (미완료 표시, 폰트에 없는 경우 다른 문자 사용 가능)
- Check Mark: `[V]` (완료 표시, 폰트에 없는 경우 다른 문자 사용 가능)

**Next Quest Settings:**
- Hide Ingredient List On Complete: ✓ (다음 퀘스트 표시 시 재료 목록 숨기기)

**Runtime Settings:**
- Default Check Mark Size: `24` (프리팹 없을 때만 사용)

---

## 5. 체크박스 UI 구성

### 5.1 방법 A: 프리팹 사용 (권장)

프리팹을 사용하면 Inspector에서 모든 시각적 설정을 미리 구성할 수 있습니다.

#### 5.1.1 QuestItem 프리팹 생성

1. **프로젝트 창에서 `Assets/Prefabs/UI/` 폴더 생성** (없으면)
2. **Hierarchy에서 임시로 UI 구조 만들기:**
   - Canvas 우클릭 → UI → Panel (임시용)
   - Panel 우클릭 → Create Empty → 이름: `QuestItem`

#### 5.1.2 QuestItem 구조 설정

**QuestItem RectTransform:**
- Width: `300`
- Height: `40`

**Horizontal Layout Group 추가:**
- QuestItem 선택 → Add Component → Horizontal Layout Group
- Spacing: `10`
- Child Control Width: ☐
- Child Control Height: ☐
- Child Force Expand Width: ☐
- Child Force Expand Height: ☐

#### 5.1.3 CheckIcon 생성 (체크박스)

**체크박스는 TextMeshProUGUI로 생성합니다!**

1. **QuestItem 우클릭 → UI → Text - TextMeshPro**
2. 이름: `CheckIcon` (정확히 이 이름이어야 함)

**RectTransform 설정:**
- Anchor: Left Center
- Position: X=0, Y=0, Z=0
- Width: `24`
- Height: `24`

**TextMeshProUGUI 설정:**
- Text: `[ ]` (빈 체크박스, 폰트에 없는 경우 다른 문자 사용)
- Font Size: `20` (Inspector에서 조정 가능)
- Alignment: Center, Middle
- Color: White (Inspector에서 조정 가능)
- **Overflow**: Truncate 또는 Overflow (텍스트가 넘칠 경우)

#### 5.1.4 NameText 생성

1. **QuestItem 우클릭 → UI → Text - TextMeshPro**
2. 이름: `NameText` (정확히 이 이름이어야 함)

**RectTransform 설정:**
- Anchor: Left Center
- Position: X=34, Y=0, Z=0 (CheckIcon 오른쪽)
- Width: `200`
- Height: `40`

**TextMeshProUGUI 설정:**
- Text: `재료이름` (임시, 스크립트가 자동 변경)
- Font Size: `18` (Inspector에서 조정 가능)
- Alignment: Left, Middle
- Color: White (Inspector에서 조정 가능)
- **Overflow**: Truncate 또는 Overflow (텍스트가 넘칠 경우)

#### 5.1.5 QuestItemUI 컴포넌트 추가

1. **QuestItem 선택**
2. **Add Component → Quest Item UI**

#### 5.1.6 프리팹으로 저장

1. **Project 창에서 `Assets/Prefabs/UI/` 폴더로 `QuestItem` 드래그**
2. 프리팹 생성 완료
3. **임시로 만든 Canvas/Panel 삭제**

#### 5.1.7 QuestUI에 프리팹 연결

1. **QuestPanel 선택**
2. **Quest UI 컴포넌트에서 Quest Item Prefab 필드에 `QuestItem` 프리팹 드래그**

---

### 5.2 방법 B: 자동 생성 (프리팹 없음)

프리팹을 사용하지 않으면 스크립트가 런타임에 자동으로 생성합니다.

**설정:**
- Quest UI 컴포넌트의 Quest Item Prefab 필드를 None으로 두기
- 스크립트가 기본값으로 자동 생성

**단점:**
- Inspector에서 미리 스타일을 설정할 수 없음
- 게임 실행 후에만 확인 가능

---

## ✅ 완료 체크리스트

- [ ] 기존 `IngredientChecklistText` 제거 또는 비활성화
- [ ] `QuestPanel` 생성 및 위치 설정 (좌측 상단)
- [ ] `QuestTitleText` 생성 및 설정
- [ ] `IngredientListContainer` 생성 및 Vertical Layout Group 추가
- [ ] `NextQuestArea` 생성 및 초기 비활성화
- [ ] `NextQuestText` 생성 및 설정
- [ ] `QuestPanel`에 `Quest UI` 컴포넌트 추가
- [ ] 모든 참조 필드 연결
- [ ] (선택) `QuestItem` 프리팹 생성 및 연결

---

## 🎮 동작 확인

게임 실행 후:
1. 재료 수집 시 체크박스가 "☐"에서 "✓"로 변경
2. 재료 이름 색상이 흰색에서 초록색으로 변경
3. 모든 재료 수집 완료 시 "케이크를 들고 소파로 가기" 메시지 표시
4. 재료 목록이 약간 투명해짐 (완료 시)

---

## 🔧 문제 해결

### 문제 1: 다음 퀘스트가 재료 목록과 같이 표시됨

**해결 방법:**
- Quest UI 컴포넌트의 `Hide Ingredient List On Complete` 옵션을 체크하세요.
- 이 옵션이 활성화되면 모든 재료 수집 완료 시 재료 목록이 완전히 숨겨집니다.

### 문제 2: 체크 마크 문자(☐, ✓)가 표시되지 않음

**원인:** 현재 폰트에 해당 유니코드 문자가 포함되어 있지 않습니다.

**해결 방법:**
1. **Quest UI 컴포넌트 설정:**
   - `Uncheck Mark`: `[ ]` 또는 `[ ]` (대괄호 사용)
   - `Check Mark`: `[V]` 또는 `[X]` (대괄호 + 문자 사용)

2. **또는 다른 문자 사용:**
   - `Uncheck Mark`: `○` (빈 원), `□` (빈 사각형)
   - `Check Mark`: `●` (채워진 원), `■` (채워진 사각형), `✓` (체크 마크)

3. **프리팹의 CheckIcon 텍스트도 동일하게 변경:**
   - `CheckIcon`의 TextMeshProUGUI에서 Text를 `[ ]`로 설정

### 문제 3: 패널 크기에 비해 글자가 넘침

**해결 방법:**

1. **TextMeshProUGUI Overflow 설정:**
   - `CheckIcon` 선택 → TextMeshProUGUI 컴포넌트
   - **Overflow**: `Truncate` 또는 `Overflow` 선택
   - `Truncate`: 넘치는 텍스트 자르기
   - `Overflow`: 넘치는 텍스트 표시 (레이아웃 밖으로 나갈 수 있음)

2. **NameText Overflow 설정:**
   - `NameText` 선택 → TextMeshProUGUI 컴포넌트
   - **Overflow**: `Truncate` 또는 `Overflow` 선택

3. **패널 크기 조정:**
   - `QuestPanel`의 RectTransform에서 Width/Height 증가
   - 또는 `IngredientListContainer`의 마진 조정

4. **폰트 크기 조정:**
   - `CheckIcon` 또는 `NameText`의 Font Size를 줄이기

---

## ❓ FAQ

**Q: 체크박스를 Image로 만들 수 있나요?**  
A: 가능하지만, TextMeshProUGUI가 더 간단합니다. `[ ]`와 `[V]` 같은 문자를 사용하면 별도 이미지 없이도 표시 가능합니다.

**Q: 프리팹 없이 사용할 수 있나요?**  
A: 네, 가능합니다. Quest Item Prefab 필드를 None으로 두면 자동 생성됩니다.

**Q: 체크박스 크기를 변경하고 싶어요.**  
A: 프리팹을 사용하는 경우 `CheckIcon`의 RectTransform Width/Height를 변경하세요. 프리팹 없이 사용하는 경우 `Default Check Mark Size` 값을 변경하세요.

**Q: 폰트 크기를 변경하고 싶어요.**  
A: Inspector에서 `CheckIcon` 또는 `NameText`의 TextMeshProUGUI 컴포넌트에서 Font Size를 변경하세요. 스크립트가 이 값을 존중합니다.

**Q: 체크 마크 문자가 보이지 않아요.**  
A: 현재 폰트에 해당 문자가 없을 수 있습니다. Quest UI 컴포넌트의 `Uncheck Mark`와 `Check Mark` 필드를 `[ ]`와 `[V]`로 변경하세요.

