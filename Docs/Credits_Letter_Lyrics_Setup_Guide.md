# Credits 편지 & 가사 설정 가이드

## 요구사항 정리

### 현재 구조
- `CreditsScrollView` (ScrollRect) - 스크롤바는 이미 비활성화됨
  - `Viewport` 
    - `Content` (여기에 편지 텍스트 표시)
- `LyricsText` (하단에 별도로 배경음에 맞게 표시)

### 구현 목표
1. **CreditsScrollView의 ViewPort > Content에 편지 표시**
   - 편지 텍스트를 Content에 표시
   - 스크롤은 가능하지만 스크롤바는 보이지 않음 (이미 해결됨)

2. **하단에 LyricsText 별도 표시**
   - 배경음악에 맞춰 가사 표시
   - CreditsScrollView와 독립적으로 동작

## Unity Editor에서 설정할 사항

### 1. CreditsScrollView 구조 확인

**Hierarchy 구조:**
```
CreditsUI
├── CreditsScrollView (ScrollRect)
│   ├── Viewport
│   │   └── Content (RectTransform)
│   │       └── LetterText (TextMeshProUGUI) ← 새로 추가
│   ├── Scrollbar Horizontal (비활성화)
│   └── Scrollbar Vertical (비활성화)
└── LyricsText (TextMeshProUGUI) ← 하단에 별도로 배치
```

### 2. LetterText 생성 (Content 내부)

1. **CreditsScrollView > Viewport > Content 선택**
2. **우클릭 → UI → Text - TextMeshPro**
3. **이름: `LetterText`**
4. **RectTransform 설정:**
   - Anchor: Top Center
   - Pos X: 0
   - Pos Y: 0
   - Width: 800~1000
   - Height: 자동 (텍스트 길이에 맞춤)

5. **TextMeshProUGUI 설정:**
   - Font: 한글 폰트
   - Font Size: 36~48
   - Alignment: Center, Top
   - Color: White
   - Text: 편지 내용 (나중에 스크립트에서 설정)

6. **Content RectTransform 설정:**
   - Width: 800~1000
   - Height: LetterText의 높이에 맞춤 (ContentSizeFitter 사용 권장)

### 3. ContentSizeFitter 추가 (Content에)

1. **Content 선택**
2. **Add Component → Content Size Fitter**
3. **설정:**
   - Horizontal Fit: Unconstrained
   - Vertical Fit: Preferred Size

### 4. LyricsText 생성 (CreditsUI 하단)

1. **CreditsUI 선택**
2. **우클릭 → UI → Text - TextMeshPro**
3. **이름: `LyricsText`**
4. **RectTransform 설정:**
   - Anchor: Bottom Center
   - Pos X: 0
   - Pos Y: 100~150 (하단에서 위로)
   - Width: 1200~1600
   - Height: 100~150

5. **TextMeshProUGUI 설정:**
   - Font: 한글 폰트
   - Font Size: 36~42
   - Alignment: Center, Middle
   - Color: White
   - Text: (비워두기, 스크립트에서 설정)

### 5. CreditsLetterPlayer 스크립트 생성

새로운 스크립트 `CreditsLetterPlayer.cs`를 생성하여:
- `CreditsScrollView`의 `Content`에 편지 텍스트 표시
- `LyricsText`에 배경음악에 맞춰 가사 표시
- `CreditsSlidePlayer`와 유사한 구조이지만 편지는 ScrollView에, 가사는 하단에 표시

## 구현할 스크립트 구조

### CreditsLetterPlayer.cs

```csharp
[Header("Music")]
- endingMusic (AudioClip)
- musicStartDelay (float)

[Header("Letter")]
- letterText (TextMeshProUGUI) // Content 내부의 LetterText
- letterContent (RectTransform) // Content RectTransform
- letterSlides (List<CreditSlide>) // 편지 슬라이드 목록

[Header("Lyrics")]
- lyricsText (TextMeshProUGUI) // 하단 LyricsText
- lyricsSlides (List<CreditSlide>) // 가사 슬라이드 목록

[Header("Common")]
- returnButton (GameObject)
```

### 동작 방식

1. **편지 표시:**
   - `letterSlides`의 각 슬라이드를 시간에 맞춰 `letterText`에 표시
   - 편지가 길면 Content의 Height가 자동으로 조정됨 (ContentSizeFitter)
   - 사용자가 스크롤하여 편지를 읽을 수 있음

2. **가사 표시:**
   - `lyricsSlides`의 각 슬라이드를 배경음악에 맞춰 `lyricsText`에 표시
   - 하단에 고정되어 표시
   - 페이드 인/아웃 효과

3. **동기화:**
   - 배경음악 재생 시간 기준으로 편지와 가사 동기화
   - 각 슬라이드에 `startTime`, `endTime` 설정

## Unity Editor 설정 단계

### 1. CreditsScrollView 구조 확인 및 LetterText 추가

1. **Hierarchy에서 CreditsUI > CreditsScrollView > Viewport > Content 선택**
2. **Content 우클릭 → UI → Text - TextMeshPro**
3. **이름: `LetterText`**
4. **RectTransform 설정:**
   - Anchor: Top Center
   - Pos X: 0
   - Pos Y: 0
   - Width: 800~1000
   - Height: 자동 (텍스트 길이에 맞춤)

5. **TextMeshProUGUI 설정:**
   - Font: 한글 폰트
   - Font Size: 42
   - Alignment: Center, Top
   - Color: White
   - Text: (비워두기, 스크립트에서 설정)

6. **Content에 ContentSizeFitter 추가:**
   - Content 선택
   - Add Component → Content Size Fitter
   - Vertical Fit: Preferred Size

### 2. LyricsText 생성 (CreditsUI 하단)

1. **CreditsUI 선택**
2. **우클릭 → UI → Text - TextMeshPro**
3. **이름: `LyricsText`**
4. **RectTransform 설정:**
   - Anchor: Bottom Center
   - Pos X: 0
   - Pos Y: 120
   - Width: 1400
   - Height: 100

5. **TextMeshProUGUI 설정:**
   - Font: 한글 폰트
   - Font Size: 36
   - Alignment: Center, Middle
   - Color: White
   - Text: (비워두기, 스크립트에서 설정)

### 3. CreditsLetterPlayer 컴포넌트 추가

1. **CreditsUI 선택**
2. **Add Component → Credits Letter Player**
3. **Inspector 설정:**

   **Music:**
   - Ending Music: 엔딩 음악 파일 할당
   - Music Start Delay: 0

   **Letter (ScrollView Content):**
   - Letter Text: LetterText (Content 내부의 TextMeshProUGUI) 드래그
   - Letter Content: Content (RectTransform) 드래그
   - Letter Slides: Size 설정 후 각 Element 추가
     - Start Time: 편지 표시 시작 시간 (초)
     - End Time: 편지 표시 종료 시간 (초)
     - Text: 편지 내용
     - Text Color: White
     - Font Size: 42 (0이면 기본값 사용)
     - Fade In Duration: 1
     - Fade Out Duration: 1

   **Lyrics (Bottom):**
   - Lyrics Text: LyricsText 드래그
   - Lyrics Background: (선택사항)
   - Lyrics Slides: Size 설정 후 각 Element 추가
     - Start Time: 가사 표시 시작 시간 (초)
     - End Time: 가사 표시 종료 시간 (초)
     - Text: 가사 내용
     - Text Color: White
     - Font Size: 36 (0이면 기본값 사용)
     - Fade In Duration: 0.5
     - Fade Out Duration: 0.5

   **Common:**
   - Return Button: ReturnButton 드래그

   **Settings:**
   - Default Letter Font Size: 42
   - Default Lyrics Font Size: 36
   - Letter Fade In Duration: 1
   - Letter Fade Out Duration: 1

### 4. CreditsScrollView 스크롤바 확인

1. **CreditsScrollView 선택**
2. **ScrollRect 컴포넌트 확인:**
   - Horizontal Scrollbar: None (비활성화)
   - Vertical Scrollbar: None (비활성화)
   - Vertical: ✅ 체크
   - Horizontal: ❌ 체크 해제

### 5. 테스트

1. **Play 모드로 전환**
2. **EndingCutsceneController의 Play Cutscene (Test) 실행**
3. **편지가 ScrollView에, 가사가 하단에 표시되는지 확인**

## 완료된 작업

1. ✅ **CreditsLetterPlayer.cs 스크립트 생성**
2. ✅ **EndingCutsceneController 수정** (CreditsLetterPlayer 우선 사용)
3. ✅ **가이드 문서 작성**

## 다음 단계 (Unity Editor에서)

1. **UI 구조 설정** (위의 Unity Editor 설정 단계 참고)
2. **CreditsLetterPlayer 컴포넌트 추가 및 설정**
3. **편지/가사 슬라이드 데이터 입력**
4. **테스트 및 조정**

