# TextMeshPro 한글 폰트 설정 가이드

이 가이드는 Unity TextMeshPro에서 한글을 표시하기 위한 폰트 설정 방법을 설명합니다.

## 🎯 목표

기본 LiberationSans 폰트는 한글을 지원하지 않아 모든 한글이 □(네모)로 표시됩니다.
한글을 지원하는 폰트를 추가하여 게임 UI에서 한글을 정상적으로 표시합니다.

---

## 📥 1단계: 한글 폰트 다운로드

### 추천 무료 한글 폰트

#### 옵션 1: Noto Sans KR (권장)
- **다운로드**: https://fonts.google.com/noto/specimen/Noto+Sans+KR
- **라이선스**: OFL (상업적 사용 가능)
- **특징**: Google에서 만든 전문적인 한글 폰트

**다운로드 방법**:
1. 위 링크 접속
2. 우측 상단 **"Get font"** 또는 **"Download family"** 클릭
3. ZIP 파일 압축 해제
4. `NotoSansKR-Regular.ttf` (또는 원하는 굵기) 파일 찾기

#### 옵션 2: 나눔고딕
- **다운로드**: https://hangeul.naver.com/font
- **라이선스**: OFL (상업적 사용 가능)
- **특징**: 네이버에서 제공하는 깔끔한 한글 폰트

---

## 📂 2단계: Unity 프로젝트에 폰트 추가

1. Unity 프로젝트에서 **폰트 폴더 생성**:
   - Project 창에서 **Assets 우클릭 → Create → Folder**
   - 폴더 이름: `Fonts`

2. **폰트 파일 복사**:
   - 다운로드한 `.ttf` 파일을 `Assets/Fonts/` 폴더로 드래그

3. **폰트 파일 확인**:
   - `Assets/Fonts/NotoSansKR-Regular.ttf` (예시) 파일이 보여야 합니다

---

## ⚙️ 3단계: TextMeshPro 폰트 에셋 생성

### 3-1. Font Asset Creator 열기

**Window → TextMeshPro → Font Asset Creator** 선택

### 3-2. 설정 입력

Font Asset Creator 창에서 다음과 같이 설정:

| 항목 | 값 | 설명 |
|------|-----|------|
| **Source Font File** | `NotoSansKR-Regular` | 추가한 한글 폰트 선택 |
| **Sampling Point Size** | `Auto Sizing` | 자동 크기 조정 |
| **Padding** | `5` | 글자 간 여백 |
| **Packing Method** | `Optimum` | 최적화 패킹 |
| **Atlas Resolution** | `2048 x 2048` | 텍스처 해상도 (한글은 글자가 많아 큰 크기 필요) |
| **Character Set** | `Unicode Range (Hex)` | 유니코드 범위로 지정 |
| **Character Sequence (Hex)** | 아래 참조 ↓ | 한글 유니코드 범위 |
| **Render Mode** | `SDFAA` | 고품질 렌더링 |
| **Get Kerning Pairs** | ✓ 체크 | 자간 정보 포함 |

### 3-3. Character Sequence (중요!)

**Character Sequence (Hex)** 필드에 다음 값을 복사해서 붙여넣으세요:

**⚠️ 중요: 쉼표 뒤에 공백이 없어야 합니다!**

```
20-7E,AC00-D7A3,2713,25A1
```

**또는 공백으로 구분** (추천):

```
20-7E AC00-D7A3 2713 25A1
```

**각 범위 설명**:
- `20-7E`: 기본 영문, 숫자, 기호
- `AC00-D7A3`: 한글 완성형 (가-힣)
- `2713`: ✓ (체크 마크)
- `25A1`: □ (빈 체크박스)

### 3-4. 폰트 생성

1. **Generate Font Atlas** 버튼 클릭
2. 생성 완료까지 **1-3분 정도** 대기 (한글은 글자가 많아 시간이 걸립니다)
3. 완료되면 아래에 미리보기가 표시됩니다
4. **Save** 버튼 클릭
5. 저장 위치: `Assets/Fonts/` 폴더
6. 파일 이름: `NotoSansKR-Regular SDF` (자동 생성됨)

---

## 🎨 4단계: UI에 폰트 적용

### 방법 1: 개별 텍스트에 적용

1. **Hierarchy**에서 **IngredientChecklistText** 선택
2. **Inspector**의 **TextMeshPro - Text (UI)** 컴포넌트 찾기
3. **Font Asset** 필드에서:
   - 현재: `LiberationSans SDF`
   - 변경: `NotoSansKR-Regular SDF` (생성한 폰트) 선택

### 방법 2: 기본 폰트로 설정 (선택사항)

모든 새 텍스트에 자동 적용하려면:

1. **Edit → Project Settings → TextMesh Pro → Settings**
2. **Default Font Asset** 필드를 `NotoSansKR-Regular SDF`로 변경

---

## ✅ 5단계: 확인

Play 모드로 실행하면 이제 한글이 정상적으로 표시됩니다:

```
케이크 재료 체크리스트
(0/5)

☐ 밀가루
☐ 설탕
☐ 계란
☐ 버터
☐ 딸기
```

---

## 🐛 문제 해결

### 문제 1: "FormatException: Input string was not in a correct format" 오류

**증상**: Character Sequence 입력 시 에러 발생

**해결책**:
- **쉼표 뒤의 공백을 제거**하세요
  - ❌ 잘못된 예: `20-7E, AC00-D7A3` (쉼표 뒤 공백 있음)
  - ✅ 올바른 예: `20-7E,AC00-D7A3` (쉼표 뒤 공백 없음)
  - ✅ 또는: `20-7E AC00-D7A3` (공백으로만 구분)

### 문제 2: "Atlas is Full" 오류

**해결책**:
- Font Asset Creator에서 **Atlas Resolution**을 `4096 x 4096`으로 증가
- 또는 **Character Sequence**를 줄여서 필요한 글자만 포함

### 문제 3: 폰트가 흐릿하거나 픽셀화됨

**해결책**:
- Font Asset Creator에서:
  - **Sampling Point Size**: `Auto Sizing` 사용
  - **Render Mode**: `SDFAA` 또는 `SDFAA_HINTED` 사용
  - **Atlas Resolution**: 더 큰 크기 (4096 x 4096) 시도

### 문제 4: 특정 한글 글자가 여전히 □로 표시됨

**해결책**:
- **Character Sequence**에 해당 글자의 유니코드 범위 추가
- 또는 `AC00-D7AF`로 범위 확장 (더 많은 한글 포함)

### 문제 5: 생성 시간이 너무 오래 걸림

**해결책**:
- **Atlas Resolution**을 `1024 x 1024`로 줄이기 (품질은 낮아짐)
- 또는 게임에 실제로 사용되는 글자만 포함
  - 예: `재료 수집 케이크 밀가루 설탕 계란 버터 딸기 체크리스트`

---

## 💡 팁

### 최적화된 Character Sequence

게임에서 사용되는 한글만 포함하려면:

**전체 한글 (권장)**:
```
20-7E AC00-D7A3 2713 25A1
```

**특정 글자만 포함** (고급, Atlas 크기 줄이기):
```
20-7E 2713 25A1 AC00 AC01 AC04 AC10 AC11 AC13 AC15 AC16 AC19 AC1A AC1C AC1D AE00 AE01 AE08 AE09 AE30 AE38 AE40 AE4C B098 B099 B0A8 B2E4 B300 B354 B358 B370 B380 B784 B798 B799 B7EC B7F4 B808 B809 B828 B85C B85D B8E8 B8F0 B958 BB34 BB38 BC14 BC15 BC18 BC29 BC30 BC31 BC84 BCC0 BCC4 BD80 BE44 BE45 C0AC C11C C120 C124 C131 C138 C18C C18D C19C C1A1 C220 C288 C2B5 C2A4 C2E4 C300 C368 C544 C548 C5B4 C5B8 C5C4 C5C8 C5D0 C5F0 C5FC C608 C640 C644 C6B0 C6C3 C6D0 C704 C77C C774 C788 C790 C791 C798 C7AC C804 C810 C81C C838 C871 C88B C900 C911 C990 C99D CA0C CB48 CC28 CC29 CC44 CC45 CC48 CCAD CCB4 CE58 CE74 D06C D0DC D0E0 D0E4 D2B8 D2F0 D30C D310 D504 D53C D544 D55C D568 D574 D578 D5A5 D5C8 D5D8 D5D9 D5E4 D5EC D5F7 D611 D638 D655 D658 D6C4 D734 D788
```

(특정 글자만 포함하는 것은 복잡하므로, **전체 한글 범위를 권장**합니다)

### 여러 굵기 사용

- `NotoSansKR-Regular.ttf`: 일반
- `NotoSansKR-Bold.ttf`: 굵게
- `NotoSansKR-Light.ttf`: 가늘게

각각에 대해 별도의 Font Asset을 생성하여 사용 가능합니다.

---

## 📚 참고 자료

- [TextMeshPro 공식 문서](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)
- [Noto Sans KR](https://fonts.google.com/noto/specimen/Noto+Sans+KR)
- [한글 유니코드 표](https://www.unicode.org/charts/PDF/UAC00.pdf)

---

완료했습니다! 이제 게임에서 한글이 정상적으로 표시될 것입니다! 🎉

