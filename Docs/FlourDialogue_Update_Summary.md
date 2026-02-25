# FlourMiniGame 대화 시스템 업데이트

## 🎉 새로운 기능

### 1. 여러 대화 지원
- 이제 NPC가 여러 문장을 순차적으로 말할 수 있습니다
- Space 키를 눌러 다음 대화로 진행합니다

### 2. 화자 정보 표시
- **화자 이름**: 대화창 좌측 상단에 노란색으로 표시
- **화자 초상화**: 대화창 우측 상단에 이미지로 표시

### 3. 대화 진행 표시
- **Continue Indicator**: 우측 하단에 "▼ Space" 표시
- 타이핑 중에는 숨김, 완료되면 표시

## 📝 변경된 파일

### 1. `Assets/Scripts/MiniGames/FlourDialogueUI.cs`
**새로운 기능:**
- `FlourDialogueData` 클래스 추가 (text, speakerName, speakerPortrait)
- `ShowDialogue(List<FlourDialogueData>)` 메서드 추가
- Space 키로 대화 진행
- 화자 이름/초상화 표시
- Continue Indicator 추가

**UI 구조:**
```
FlourDialoguePanel (하단 0~25%)
├── SpeakerName (좌측 상단, 노란색)
├── SpeakerPortrait (우측 상단, 이미지)
├── DialogueText (중앙, 흰색)
└── ContinueIndicator (우측 하단, "▼ Space")
```

### 2. `Assets/Scripts/MiniGames/FlourNPC.cs`
**새로운 필드:**
- `List<FlourDialogueData> startDialogues` - 시작 대화 목록
- `List<FlourDialogueData> progressDialogues` - 진행 중 대화 목록 (선택)
- `List<FlourDialogueData> completeDialogues` - 완료 대화 목록
- `string npcName` - NPC 이름 (기본값 사용)
- `Sprite npcPortrait` - NPC 초상화 (기본값 사용)

**자동 초기화:**
- 대화가 비어있으면 기본 대화 자동 생성
- `npcName`과 `npcPortrait`를 기본값으로 사용

### 3. `Docs/MiniGame_Scene_Setup_Guide.md`
**추가된 섹션:**
- FlourDialogueUI 상세 설정 가이드
- 대화 시스템 사용 방법 가이드
- 여러 캐릭터 대화 예제

## 🎮 사용 방법

### Inspector에서 설정 (가장 쉬움)

```
NPC 선택 → FlourNPC (Script)

Dialogue - Start (Size: 2)  ← 여기를 클릭해서 증가
├── Element 0
│   ├── Text: "안녕하세요! 저는 마을 주민입니다."
│   ├── Speaker Name: "마을 주민"
│   └── Speaker Portrait: [NPC 이미지 드래그]
└── Element 1
    ├── Text: "포대를 옮겨주시면 밀가루를 드리겠습니다!"
    ├── Speaker Name: "마을 주민"
    └── Speaker Portrait: [동일 이미지]

Dialogue - Complete (Size: 1)
└── Element 0
    ├── Text: "고마워요! 밀가루 한 자루입니다."
    ├── Speaker Name: "마을 주민"
    └── Speaker Portrait: [동일 이미지]

NPC Info (Optional) - 기본값으로 사용됨
├── Npc Name: "마을 주민"
└── Npc Portrait: [NPC 이미지]
```

### 플레이어 조작

```
F 키: NPC와 상호작용 (대화 시작)
Space 키: 다음 대화로 진행
- 타이핑 중: 전체 텍스트 즉시 표시
- 타이핑 완료: 다음 대화로 이동
- 마지막 대화: 대화창 닫힘
```

## 🔧 설정 방법

### 1. FlourDialogueUI 추가 (Canvas에)

```
Canvas 선택 → Add Component → Flour Dialogue UI

자동으로 UI가 생성됩니다!
```

### 2. NPC 대화 설정

```
NPC 선택 → FlourNPC (Script) → Inspector

Dialogue - Start 옆 숫자 클릭하여 증가
각 Element에 대화 내용 입력
```

## 🎨 커스터마이징

### 대화창 스타일 변경

```
Play 모드에서 확인:
Canvas → FlourDialoguePanel (자동 생성됨)

Position/Size 조정:
- Anchor 값 변경으로 위치 조정
- Font Size 조정으로 크기 변경
- Color 조정으로 색상 변경

Play 종료 후 변경 사항은 사라지므로,
원하는 스타일을 찾은 후 수동 생성 방법 사용!
```

### 여러 캐릭터 대화

```csharp
// 코드에서 동적으로 생성
List<FlourDialogueData> dialogues = new List<FlourDialogueData>
{
    new FlourDialogueData
    {
        text = "안녕!",
        speakerName = "캐릭터 A",
        speakerPortrait = spriteA
    },
    new FlourDialogueData
    {
        text = "반가워!",
        speakerName = "캐릭터 B",
        speakerPortrait = spriteB
    }
};
```

## ✅ 하위 호환성

**기존 코드도 그대로 작동합니다!**

```csharp
// 이전 방식 (여전히 사용 가능)
dialogueUI.Show("단순 텍스트");

// 새로운 방식 (권장)
dialogueUI.ShowDialogue(dialogueList);
```

## 🐛 문제 해결

### 대화창이 안 나타남
```
1. Canvas에 FlourDialogueUI 컴포넌트가 있는지 확인
2. Play 모드에서 DialoguePanel이 자동 생성되는지 확인
3. Console에서 "[FlourDialogueUI] 대화 UI 자동 생성 완료" 로그 확인
```

### Space 키가 안 먹힘
```
1. FlourDialogueUI Inspector → Continue Key: Space 확인
2. 다른 스크립트가 Space 키를 먼저 처리하는지 확인
3. Input System이 아닌 Legacy Input 사용 중인지 확인
```

### 초상화가 안 보임
```
1. FlourDialogueData의 speakerPortrait가 null이 아닌지 확인
2. Sprite가 올바르게 할당되었는지 확인
3. SpeakerPortrait GameObject가 활성화되어 있는지 확인
```

## 📚 추가 참고 자료

- `Docs/MiniGame_Scene_Setup_Guide.md` - 전체 설정 가이드
- `Assets/Scripts/MiniGames/FlourDialogueUI.cs` - 소스 코드
- `Assets/Scripts/MiniGames/FlourNPC.cs` - NPC 예제 코드

## 🎉 완료!

이제 FlourMiniGame에서 풍성한 대화를 즐길 수 있습니다!

**테스트 방법:**
1. FlourMiniGameScene 열기
2. NPC 선택 → Inspector에서 대화 설정
3. Play 버튼 클릭
4. F 키로 NPC와 상호작용
5. Space 키로 대화 진행

**즐거운 게임 개발 되세요!** 🎮✨




