# TitleScene Credit 버튼 설정 가이드

## 개요

TitleScene에 크레딧을 볼 수 있는 테스트용 버튼을 추가했습니다.

## Unity Editor에서 설정

### 1. Credit 버튼 생성

1. **TitleScene 열기**
2. **Canvas 선택**
3. **우클릭 → UI → Button - TextMeshPro**
4. **이름: `CreditButton`**
5. **RectTransform 설정:**
   - Anchor: Middle Center
   - Pos X: 0
   - Pos Y: -100 (StartButton 아래)
   - Width: 300
   - Height: 80

6. **Button Text 설정:**
   - Text: "크레딧 보기" 또는 "Credit"

### 2. TitleScreenUI 컴포넌트 설정

1. **Canvas 선택**
2. **Inspector에서 Title Screen UI 컴포넌트 확인**
3. **Credit Button 필드에 `CreditButton` 드래그**

### 3. CreditsUI 확인

Credit 버튼이 작동하려면 CreditsUI가 있어야 합니다:

- **HomeScene에 CreditsUI가 있는 경우**: 자동으로 찾아서 활성화
- **TitleScene에 CreditsUI가 있는 경우**: 자동으로 찾아서 활성화
- **DontDestroyOnLoad로 되어 있는 경우**: 자동으로 찾아서 활성화

## 동작 방식

1. **Credit 버튼 클릭**
2. **타이틀 UI 숨김** (StartButton, QuitButton, TitleText)
3. **CreditsUI 활성화**
4. **CreditsLetterPlayer 또는 CreditsSlidePlayer 시작**
5. **크레딧 재생**

## ReturnButton으로 돌아가기

크레딧이 끝나면 ReturnButton이 표시됩니다. ReturnButton을 클릭하면:
- TitleScene으로 돌아가거나
- TitleScene이 이미 활성화되어 있으면 CreditsUI만 비활성화

ReturnButton의 이벤트를 TitleScene으로 돌아가도록 설정하려면:
1. **ReturnButton 선택**
2. **Button 컴포넌트 → On Click()**
3. **+ 버튼 클릭**
4. **None (Object)**: SceneLoader 드래그
5. **Function**: SceneLoader → LoadTitleScene()

또는 CreditsUI를 비활성화만 하려면:
1. **ReturnButton 선택**
2. **Button 컴포넌트 → On Click()**
3. **+ 버튼 클릭**
4. **None (Object)**: CreditsUI 드래그
5. **Function**: GameObject → SetActive (false)
6. **체크박스 해제** (비활성화)

그리고 TitleScreenUI의 ShowTitleUI()를 호출하여 타이틀 UI를 다시 표시할 수 있습니다.

## 테스트

1. **Play 모드로 전환**
2. **TitleScene에서 Credit 버튼 클릭**
3. **크레딧이 표시되는지 확인**
4. **ReturnButton으로 돌아가기 확인**

