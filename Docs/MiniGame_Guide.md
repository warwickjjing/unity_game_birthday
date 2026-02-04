# 🎮 미니게임 만들기 가이드

이 문서는 Birthday Cake Quest 프로젝트에서 새로운 미니게임을 만드는 방법을 설명합니다.

---

## 📋 목차

1. [미니게임 시스템 구조](#1-미니게임-시스템-구조)
2. [설탕 미니게임 예제](#2-설탕-미니게임-예제)
3. [새 미니게임 추가하기](#3-새-미니게임-추가하기)
4. [Unity Editor 설정](#4-unity-editor-설정)
5. [테스트 방법](#5-테스트-방법)

---

## 1. 미니게임 시스템 구조

### 1.1 핵심 컴포넌트

- **MiniGameManager**: 미니게임을 관리하는 싱글톤 매니저
- **IMiniGame**: 모든 미니게임이 구현해야 하는 인터페이스
- **MiniGameType**: 미니게임 종류를 나타내는 Enum

### 1.2 미니게임 플로우

```
플레이어가 재료와 상호작용 (F키)
    ↓
CollectibleIngredient가 미니게임 필요 여부 확인
    ↓
MiniGameManager.StartMiniGame() 호출
    ↓
게임플레이 일시정지 (플레이어, 카메라, 상호작용)
    ↓
미니게임 UI 활성화
    ↓
미니게임 플레이
    ↓
성공/실패 콜백
    ↓
게임플레이 재개
```

---

## 2. 설탕 미니게임 예제

### 2.1 설탕 미니게임 개요

**게임 목표**: 마우스를 누르고 있어서 게이지를 타겟 범위(80-100%)에 1.5초 동안 유지

**게임플레이**:
- 마우스 버튼 또는 스페이스바를 누르고 있으면 게이지 상승
- 놓으면 게이지 하강
- 타겟 범위에 도달하면 성공 카운트 시작
- 1.5초 유지 시 성공

### 2.2 설탕 미니게임 UI 구성

```
SugarMiniGamePanel
├── TitleText (TextMeshPro)
│   └── "Sugar Pouring Mini-Game"
├── GaugeBackground (Image)
│   └── 게이지 배경
├── GaugeFill (Image)
│   └── Fill Type: Filled, Vertical
│   └── Fill Amount: 0~1
├── TargetRange (Image)
│   └── 타겟 범위 표시 (초록색)
├── TimerText (TextMeshPro)
│   └── "Time: 10.0"
├── InstructionText (TextMeshPro)
│   └── "Hold Mouse or Spacebar to pour sugar!"
└── ResultPanel (Panel, 초기 비활성)
    ├── ResultText (TextMeshPro)
    ├── RetryButton (Button)
    └── CloseButton (Button)
```

---

## 3. 새 미니게임 추가하기

### 3.1 단계별 가이드

#### Step 1: MiniGameType에 새 타입 추가

`Assets/Scripts/MiniGames/MiniGameType.cs` 파일을 열고:

```csharp
public enum MiniGameType
{
    Sugar,
    Egg,      // 새로 추가
    Flour,    // 새로 추가
    Butter,   // 새로 추가
    Strawberry // 새로 추가
}
```

#### Step 2: IMiniGame 인터페이스 구현

새 스크립트 생성: `Assets/Scripts/MiniGames/EggMiniGame.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BirthdayCakeQuest.MiniGames;

namespace BirthdayCakeQuest.MiniGames
{
    public class EggMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeButton;

        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 15f;
        [SerializeField] private float successThreshold = 3f;

        private bool _isActive = false;
        private float _timer = 0f;
        private System.Action<bool> _onComplete;

        public void StartMiniGame(System.Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _isActive = true;
            _timer = gameDuration;
            
            // UI 초기화
            if (resultPanel != null)
                resultPanel.SetActive(false);
            
            if (instructionText != null)
                instructionText.text = "조심조심 배달하기!";
        }

        public void EndMiniGame(bool success)
        {
            _isActive = false;
            
            // 결과 표시
            if (resultPanel != null)
                resultPanel.SetActive(true);
            
            if (resultText != null)
                resultText.text = success ? "Success!" : "Failed!";
            
            // 버튼 이벤트
            if (retryButton != null)
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => {
                    resultPanel.SetActive(false);
                    StartMiniGame(_onComplete);
                });
            
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => {
                    if (_onComplete != null)
                        _onComplete(success);
                });
        }

        private void Update()
        {
            if (!_isActive) return;

            // 타이머 업데이트
            _timer -= Time.deltaTime;
            
            if (timerText != null)
                timerText.text = $"Time: {_timer:F1}";

            // 시간 초과
            if (_timer <= 0f)
            {
                EndMiniGame(false);
            }

            // 게임 로직 구현
            // 예: 균형 잡기, 장애물 피하기 등
        }
    }
}
```

#### Step 3: MiniGameManager에 새 미니게임 등록

`Assets/Scripts/MiniGames/MiniGameManager.cs` 파일의 `CreateMiniGame()` 메서드에 추가:

```csharp
private IMiniGame CreateMiniGame(MiniGameType type)
{
    switch (type)
    {
        case MiniGameType.Sugar:
            if (sugarMiniGamePanel != null)
            {
                return sugarMiniGamePanel.GetComponent<IMiniGame>();
            }
            break;
        
        case MiniGameType.Egg:  // 새로 추가
            if (eggMiniGamePanel != null)
            {
                return eggMiniGamePanel.GetComponent<IMiniGame>();
            }
            break;
        
        // ... 다른 미니게임들
    }
    
    return null;
}
```

그리고 MiniGameManager 클래스에 새 Panel 필드 추가:

```csharp
[Header("Mini Game Panels")]
[SerializeField] private GameObject sugarMiniGamePanel;
[SerializeField] private GameObject eggMiniGamePanel;  // 새로 추가
```

#### Step 4: Unity Editor에서 설정

1. **새 Panel 생성**
   ```
   MiniGameCanvas → UI → Panel → "EggMiniGamePanel"
   ```

2. **스크립트 추가**
   ```
   EggMiniGamePanel 선택
   Add Component → Egg Mini Game
   ```

3. **UI 참조 연결**
   - Inspector에서 모든 UI 요소를 드래그 앤 드롭

4. **MiniGameManager에 등록**
   ```
   MiniGameManager 선택
   Egg Mini Game Panel → EggMiniGamePanel 드래그
   ```

5. **재료 오브젝트 설정**
   ```
   Ingredient_Egg 선택
   CollectibleIngredient:
   - Requires Mini Game: ✓
   - Mini Game Type: Egg
   ```

---

## 4. Unity Editor 설정

### 4.1 MiniGameManager 설정

```
Hierarchy → Create Empty → "MiniGameManager"
Add Component → MiniGameManager

Inspector 설정:
- Mini Game Canvas: MiniGameCanvas (드래그)
- Sugar Mini Game Panel: SugarMiniGamePanel (드래그)
- Egg Mini Game Panel: EggMiniGamePanel (드래그)  // 새로 추가
- Player Controller: (자동 연결)
- Interactor: (자동 연결)
- Isometric Camera: (자동 연결)
```

### 4.2 Canvas 설정

```
Hierarchy → UI → Canvas → "MiniGameCanvas"

Canvas 설정:
- Render Mode: Screen Space - Overlay
- Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080

초기 상태: 비활성화 (체크 해제)
```

### 4.3 미니게임 Panel 설정

각 미니게임마다 Panel을 생성하고:

```
MiniGameCanvas → UI → Panel → "[GameName]MiniGamePanel"

RectTransform:
- Anchor: Stretch
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: 반투명 검정 (A: 180)
```

### 4.4 UI 요소 추가

각 미니게임 Panel 아래에 필요한 UI 요소 추가:

- **TextMeshPro**: 타이틀, 타이머, 안내문
- **Image**: 게이지, 배경, 타겟 영역
- **Button**: 재시도, 닫기 버튼
- **Panel**: 결과 표시 패널

---

## 5. 테스트 방법

### 5.1 체크리스트

- [ ] MiniGameManager가 Scene에 있음
- [ ] MiniGameCanvas가 생성되어 있음
- [ ] 미니게임 Panel이 Canvas 아래에 있음
- [ ] IMiniGame 스크립트가 Panel에 추가됨
- [ ] 모든 UI 참조가 연결됨
- [ ] MiniGameManager에 Panel이 등록됨
- [ ] 재료의 "Requires Mini Game"이 체크됨
- [ ] 재료의 Mini Game Type이 올바르게 설정됨

### 5.2 테스트 플로우

1. **Play 버튼 클릭**
2. **플레이어를 재료 근처로 이동**
   - "Collect [재료명] [F]" 프롬프트 확인
3. **F키 누르기**
   - Console 로그 확인:
     ```
     [Interactor] Interacting with: Collect Sugar [F]
     [CollectibleIngredient] Starting mini game: Sugar
     [MiniGameManager] Sugar 미니게임 시작
     ```
4. **미니게임 UI 확인**
   - Canvas가 활성화되어야 함
   - 모든 UI 요소가 보여야 함
5. **미니게임 플레이**
   - 게임 로직 테스트
6. **결과 확인**
   - 성공/실패 시 올바르게 처리되는지 확인

### 5.3 문제 해결

**문제: 미니게임 UI가 나타나지 않음**
- Canvas가 활성화되어 있는지 확인 (초기에는 비활성화)
- MiniGameManager에 Panel이 연결되어 있는지 확인
- Console에서 에러 메시지 확인

**문제: F키를 눌러도 반응 없음**
- 재료의 "Requires Mini Game" 체크 확인
- 재료의 Mini Game Type이 올바른지 확인
- Interactor 범위 안에 있는지 확인

**문제: "MiniGameManager를 찾을 수 없습니다!"**
- Scene에 MiniGameManager GameObject가 있는지 확인
- MiniGameManager 스크립트가 추가되어 있는지 확인

**문제: 미니게임이 끝나도 게임이 재개되지 않음**
- IMiniGame.EndMiniGame()에서 콜백을 호출하는지 확인
- MiniGameManager의 EndMiniGame()이 호출되는지 확인

---

## 6. 미니게임 아이디어

### 6.1 계란 미니게임: "조심조심 배달하기"
- 계란을 들고 장애물을 피해 이동
- 너무 빨리 움직이면 깨짐
- 장애물에 부딪히면 깨짐
- 타이머 + 균형 잡기 요소

### 6.2 밀가루 미니게임: "밀가루 포대 쌓기"
- 떨어지는 밀가루 포대를 좌우로 움직여 받아내기
- 일정 개수 이상 쌓으면 성공
- 또는 리듬게임처럼 타이밍 맞춰 포대 캐치

### 6.3 버터 미니게임: "냉장고 미로 탐험"
- 차가운 냉장고 안 미로를 탐험해서 버터 찾기
- 시간제한 있음
- 얼음 장애물 피하기
- 열쇠로 문 열거나 퍼즐 요소 추가 가능

### 6.4 딸기 미니게임: "딸기 따기 타이밍 게임"
- 정원에서 좋은 딸기만 골라서 따기
- 클릭/탭 타이밍 게임
- 상한 딸기는 피하기
- 또는 간단한 매칭 퍼즐 (같은 색 딸기 3개 모으기)

---

## 7. 참고 문서

- [미니게임 빠른 시작](MiniGame_Quick_Start.md)
- [Unity Editor 설정 가이드](MiniGame_Unity_Setup.md)
- [상호작용 문제 해결](Troubleshooting_Interaction.md)

---

**작성일**: 2026-02-02  
**버전**: 2.0  
**상호작용 키**: F키 (변경됨)

