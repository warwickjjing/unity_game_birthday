# Sugar MiniGame 대화 흐름 설정 가이드

## 대화 흐름 구조

```
NPC 상호작용
  ↓
"설탕을 가지고 계신 분이 맞죠?" (Element 0)
  ↓
선택지 2개:
  ├─ "설탕을 달라고 한다" (Element 0)
  │   ├─ hasAnswer = true → 정답 Dialogue 순서대로 → HomeScene 복귀
  │   └─ hasAnswer = false → 실패 Dialogue 순서대로
  │
  └─ "죄송해요 제가 잘못 봤나봐요" (Element 1)
      └─ Quit Dialogue 순서대로
```

## Unity Editor에서 설정 방법

### 1. SugarNPC Inspector 설정

**Sugar_NPCS_0** (정답 NPC) 예시:

#### Correct Dialogues 설정

**Element 0: 초기 질문**
- `Text`: "혹시.. 설탕을 가지고 계신 분이 맞죠?"
- `Speaker Name`: "영란영"
- `Speaker Portrait`: (초상화 이미지)
- **Choices (Optional) > Size**: 2

  **Choices > Element 0: "설탕을 달라고 한다"**
  - `Choice Text`: "설탕을 달라고 한다"
  - `Is Correct`: ✅ **true** (체크)
  - `Next Dialogues > Size`: (정답 대화 개수만큼)
    - `Element 0`: "어머 어떻게 아셨어요?! 여기 있어요~"
    - `Element 1`: "감사합니다!"
    - (마지막 대화에서 HomeScene 복귀 처리)

  **Choices > Element 1: "죄송해요 제가 잘못 봤나봐요"**
  - `Choice Text`: "죄송해요 제가 잘못 봤나봐요"
  - `Is Correct`: ❌ **false** (체크 해제)
  - `Next Dialogues > Size`: (Quit 대화 개수만큼)
    - `Element 0`: "아 네, 괜찮아요~"
    - (마지막 대화에서 게임 종료 또는 씬 복귀)

#### Wrong Dialogues 설정 (틀린 NPC용)

**Element 0: 초기 질문**
- `Text`: "혹시.. 설탕을 가지고 계신 분이 맞죠?"
- `Speaker Name`: "영란영"
- **Choices (Optional) > Size**: 2

  **Choices > Element 0: "설탕을 달라고 한다"**
  - `Choice Text`: "설탕을 달라고 한다"
  - `Is Correct`: ❌ **false** (체크 해제)
  - `Next Dialogues > Size`: (실패 대화 개수만큼)
    - `Element 0`: "설탕이요? 저는 생선만 팔아요~"
    - `Element 1`: "다른 분을 찾아보세요"

  **Choices > Element 1: "죄송해요 제가 잘못 봤나봐요"**
  - `Choice Text`: "죄송해요 제가 잘못 봤나봐요"
  - `Is Correct`: ❌ **false** (체크 해제)
  - `Next Dialogues > Size`: (Quit 대화 개수만큼)
    - `Element 0`: "아 네, 괜찮아요~"

### 2. 대화 구조 예시 (정답 NPC)

#### Correct Dialogues > Element 0
```
Text: "혹시.. 설탕을 가지고 계신 분이 맞죠?"
Speaker Name: "영란영"
Choices:
  - Element 0:
      Choice Text: "설탕을 달라고 한다"
      Is Correct: true
      Next Dialogues:
        - Element 0: "어머 어떻게 아셨어요?! 여기 있어요~"
        - Element 1: "감사합니다!"
  
  - Element 1:
      Choice Text: "죄송해요 제가 잘못 봤나봐요"
      Is Correct: false
      Next Dialogues:
        - Element 0: "아 네, 괜찮아요~"
```

### 3. HomeScene 복귀 처리

정답을 선택하고 모든 대화가 끝나면 `SugarMiniGameScene.cs`의 `OnSugarObtained()` 메서드가 호출되어:
1. 설탕을 `IngredientInventory`에 추가
2. `MiniGameResult.SetResultAndReturn(true)` 호출
3. HomeScene으로 복귀

이 처리는 `DialogueUI.cs`의 `OnChoiceSelected`에서 `choice.isCorrect == true`일 때 `OnCorrectChoiceSelected` 이벤트를 발생시키고, `SugarMiniGameScene.cs`가 이를 구독하여 처리합니다.

## ChoiceButtonParent 위치 설정

선택지 버튼이 대화 패널 하단 중앙에 표시되도록 설정:

### Unity Editor에서 설정

1. **Hierarchy에서 `ChoiceButtonParent` 선택**
2. **Inspector에서 `RectTransform` 설정:**
   - **Anchor Presets**: Bottom Center 클릭 (또는 Alt+Shift로 Pivot도 함께 설정)
   - **Pos X**: 0
   - **Pos Y**: 60~80 (대화 텍스트 아래)
   - **Width**: 400~500
   - **Height**: 자동 (VerticalLayoutGroup이 관리)

3. **VerticalLayoutGroup 컴포넌트 확인:**
   - `Spacing`: 100 (코드에서 자동 설정되지만, 수동으로도 설정 가능)
   - `Child Alignment`: Middle Center
   - `Child Control Size`: Width ✅, Height ✅
   - `Child Force Expand**: Width ✅, Height ❌

4. **ContentSizeFitter 추가 (선택사항):**
   - `Vertical Fit`: Preferred Size
   - 버튼 개수에 따라 높이가 자동 조정됨

### 문제 해결

**버튼이 화면 중앙 위쪽에 있는 경우:**
- `ChoiceButtonParent`의 `RectTransform` Anchor를 **Bottom Center**로 변경
- `Pos Y`를 **양수 값** (60~80)으로 설정

**버튼이 보이지 않는 경우:**
- `ChoiceButtonParent`가 `DialoguePanel`의 자식인지 확인
- `DialoguePanel`이 활성화되어 있는지 확인
- `ChoiceButtonParent`의 `RectTransform` 크기가 0이 아닌지 확인

## 주의사항

1. **Has Answer 체크박스**: 정답 NPC는 `Has Answer`를 ✅ 체크, 틀린 NPC는 ❌ 체크 해제
2. **Is Correct 필드**: 각 선택지의 `Is Correct`는 정답 선택지만 ✅ 체크
3. **Next Dialogues**: 선택지 선택 후 표시될 대화들을 순서대로 추가
4. **버튼 간격**: `ChoiceButtonParent`의 `VerticalLayoutGroup` Spacing이 100으로 자동 설정됨
5. **버튼 위치**: `ChoiceButtonParent`의 Anchor를 Bottom Center로 설정하여 대화 패널 하단에 배치

