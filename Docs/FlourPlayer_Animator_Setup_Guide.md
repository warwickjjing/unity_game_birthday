# FlourPlayer2D Animator 설정 가이드

## 📋 목차
1. [애니메이션 클립 준비](#애니메이션-클립-준비)
2. [Animator Controller 생성](#animator-controller-생성)
3. [Parameters 설정](#parameters-설정)
4. [State 생성](#state-생성)
5. [Transition 설정](#transition-설정)
6. [Blend Tree 고급 설정](#blend-tree-고급-설정)

---

## 🎬 애니메이션 클립 준비

### 필요한 애니메이션 클립

```
Assets/Animations/FlourPlayer/
├── Idle.anim       (정지)
├── Walk.anim       (걷기)
└── Run.anim        (달리기)
```

### 애니메이션 클립 생성 방법

#### 1. Animation 창 열기
```
Window → Animation → Animation
```

#### 2. Player 선택 후 클립 생성
```
1. Hierarchy에서 Player 선택
2. Animation 창에서 "Create" 버튼
3. 저장 위치: Assets/Animations/FlourPlayer/
4. 이름: Idle (정지 애니메이션)
```

#### 3. 스프라이트 추가
```
Animation 창:
1. "Add Property" → Sprite Renderer → Sprite
2. 타임라인에 키프레임 추가
3. 각 키프레임에 스프라이트 할당

Idle: 1~2 프레임 (정지)
Walk: 4~8 프레임 (걷기 사이클)
Run: 6~12 프레임 (빠른 걷기 사이클)
```

---

## 🎮 Animator Controller 생성

### 1. Controller 생성

```
Project 창:
1. Assets/Animations/FlourPlayer/ 폴더에서 우클릭
2. Create → Animator Controller
3. 이름: FlourPlayerController
```

### 2. Player에 할당

```
Hierarchy → Player 선택

Inspector:
- Animator 컴포넌트
  - Controller: FlourPlayerController (드래그)
  - Avatar: None (2D는 필요 없음)
  - Apply Root Motion: ☐ (체크 해제)
```

---

## ⚙️ Parameters 설정

Animator 창에서 Parameters 탭:

### Parameter 추가

```
Parameters:
├── Speed (Float) - 이동 속도 (0~10)
├── Horizontal (Float) - 좌우 입력 (-1~1)
└── Vertical (Float) - 상하 입력 (-1~1)
```

#### Speed Parameter
```
이름: Speed
타입: Float
기본값: 0

용도: 
- 0: Idle (정지)
- 0.1~5: Walk (걷기)
- 5~: Run (달리기)
```

#### Horizontal Parameter (선택 사항)
```
이름: Horizontal
타입: Float
기본값: 0

용도: Blend Tree에서 방향별 애니메이션 제어
```

#### Vertical Parameter (선택 사항)
```
이름: Vertical
타입: Float
기본값: 0

용도: Blend Tree에서 방향별 애니메이션 제어
```

---

## 🎭 State 생성

Animator 창의 Layers → Base Layer:

### State 추가

```
빈 공간 우클릭:

1. Create State → Empty
   → 이름: Idle
   → Motion: Idle.anim

2. Create State → Empty
   → 이름: Walk
   → Motion: Walk.anim

3. Create State → Empty
   → 이름: Run
   → Motion: Run.anim
```

### State 배치 (권장)

```
┌─────────────────────────────┐
│                             │
│    [Entry] ──→ [Idle]       │
│                  ↓ ↑         │
│                [Walk]        │
│                  ↓ ↑         │
│                [Run]         │
│                             │
└─────────────────────────────┘
```

### Entry State 설정

```
[Entry] 우클릭 → Set StateMachine Default State
→ Idle 선택

또는:
Idle State를 주황색으로 만들기 (기본 상태)
```

---

## 🔄 Transition 설정

### 1. Idle → Walk

```
Idle State 우클릭 → Make Transition → Walk

Transition 선택 후 Inspector:

조건 (Conditions):
├── Speed Greater 0.1

설정 (Settings):
├── Has Exit Time: ☐ (체크 해제)
├── Transition Duration: 0.1
└── Interruption Source: Current State
```

### 2. Walk → Idle

```
Walk State 우클릭 → Make Transition → Idle

조건:
├── Speed Less 0.1

설정:
├── Has Exit Time: ☐
├── Transition Duration: 0.1
└── Interruption Source: Current State
```

### 3. Walk → Run

```
Walk State 우클릭 → Make Transition → Run

조건:
├── Speed Greater 5.0

설정:
├── Has Exit Time: ☐
├── Transition Duration: 0.2
└── Interruption Source: Current State
```

### 4. Run → Walk

```
Run State 우클릭 → Make Transition → Walk

조건:
├── Speed Less 5.0

설정:
├── Has Exit Time: ☐
├── Transition Duration: 0.2
└── Interruption Source: Current State
```

### 5. Idle → Run (빠른 시작)

```
Idle State 우클릭 → Make Transition → Run

조건:
├── Speed Greater 5.0

설정:
├── Has Exit Time: ☐
├── Transition Duration: 0.1
└── Interruption Source: Current State
```

### 6. Run → Idle (급정지)

```
Run State 우클릭 → Make Transition → Idle

조건:
├── Speed Less 0.1

설정:
├── Has Exit Time: ☐
├── Transition Duration: 0.1
└── Interruption Source: Current State
```

---

## 📊 Transition 다이어그램

```
        Speed > 5.0
    ┌─────────────────┐
    │                 ↓
  [Idle] ←──→ [Walk] ←──→ [Run]
    ↑         ↑   ↓         ↓
    │    0.1  │   │  5.0    │
    └─────────┴───┴─────────┘
      Speed < 0.1
```

### 전체 Transition 요약

| From | To | 조건 | 설명 |
|------|-----|------|------|
| **Idle** | Walk | Speed > 0.1 | 걷기 시작 |
| **Idle** | Run | Speed > 5.0 | 빠르게 달리기 시작 |
| **Walk** | Idle | Speed < 0.1 | 정지 |
| **Walk** | Run | Speed > 5.0 | 달리기로 전환 |
| **Run** | Walk | Speed < 5.0 | 걷기로 전환 |
| **Run** | Idle | Speed < 0.1 | 정지 |

---

## 🎯 Blend Tree 고급 설정 (선택 사항)

8방향 이동 애니메이션이 필요한 경우:

### Blend Tree 생성

```
Animator 창:
1. Walk State 삭제
2. 빈 공간 우클릭 → Create State → From New Blend Tree
3. 이름: Walk_BlendTree
```

### Blend Tree 설정

```
Walk_BlendTree 더블클릭:

Blend Type: 2D Freeform Directional

Parameters:
├── Horizontal (X축)
└── Vertical (Y축)

Motions 추가:
├── Walk_Right   (Pos X:  1, Pos Y:  0)
├── Walk_Left    (Pos X: -1, Pos Y:  0)
├── Walk_Up      (Pos X:  0, Pos Y:  1)
├── Walk_Down    (Pos X:  0, Pos Y: -1)
├── Walk_UpRight (Pos X:  1, Pos Y:  1)
├── Walk_UpLeft  (Pos X: -1, Pos Y:  1)
├── Walk_DownRight (Pos X: 1, Pos Y: -1)
└── Walk_DownLeft (Pos X: -1, Pos Y: -1)
```

**참고:** 이미 스프라이트 Flip을 사용하고 있다면 4방향만 있어도 충분합니다!

---

## ⚡ FlourPlayer2D 스크립트 연동

이미 `FlourPlayer2D.cs`에서 자동으로 연동됩니다:

```csharp
// HandleMovement() 메서드에서 자동 설정
_animator.SetFloat("Horizontal", _moveInput.x);
_animator.SetFloat("Vertical", _moveInput.y);
_animator.SetFloat("Speed", _moveInput.magnitude);
```

### Speed 값 계산

```
정지:      Speed = 0
걷기:      Speed = 0~1 (정규화된 입력)
달리기:    Speed는 자동 계산 안 됨 (추가 구현 필요)
```

### 달리기 구현 (선택 사항)

`FlourPlayer2D.cs`에 Shift 키로 달리기 추가:

```csharp
[Header("Movement Settings")]
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private float runSpeed = 10f; // 추가
[SerializeField] private float interactionRange = 1.5f;

private void HandleMovement()
{
    // 입력 받기
    _moveInput.x = Input.GetKey(KeyCode.A) ? -1f : (Input.GetKey(KeyCode.D) ? 1f : 0f);
    _moveInput.y = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);

    // 대각선 이동 정규화
    if (_moveInput.magnitude > 1f)
    {
        _moveInput.Normalize();
    }

    // 달리기 판단 (Shift 키)
    bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    float currentSpeed = isRunning ? runSpeed : moveSpeed;

    // Rigidbody2D로 이동
    if (_rigidbody != null)
    {
        _rigidbody.velocity = _moveInput * currentSpeed;
    }

    // 스프라이트 좌우 반전
    if (_spriteRenderer != null && _moveInput.x != 0)
    {
        _spriteRenderer.flipX = _moveInput.x < 0;
    }

    // 애니메이션 파라미터 업데이트
    if (_animator != null)
    {
        if (!string.IsNullOrEmpty(horizontalParameterName))
        {
            _animator.SetFloat(horizontalParameterName, _moveInput.x);
        }
        if (!string.IsNullOrEmpty(verticalParameterName))
        {
            _animator.SetFloat(verticalParameterName, _moveInput.y);
        }
        if (!string.IsNullOrEmpty(speedParameterName))
        {
            // Speed 값을 실제 속도에 맞게 설정
            float animSpeed = _moveInput.magnitude * (isRunning ? 10f : 5f);
            _animator.SetFloat(speedParameterName, animSpeed);
        }
    }
}
```

---

## ✅ 테스트 체크리스트

Play 버튼을 눌러서 확인:

- [ ] 정지 시 Idle 애니메이션 재생
- [ ] WASD 이동 시 Walk 애니메이션 재생
- [ ] Shift + WASD 이동 시 Run 애니메이션 재생 (구현한 경우)
- [ ] 왼쪽 이동 시 스프라이트 반전
- [ ] 오른쪽 이동 시 스프라이트 정상
- [ ] Animator 창에서 Speed 값 변화 확인
- [ ] State 전환이 부드러운가?

---

## 🐛 문제 해결

### 문제 1: 애니메이션이 재생 안 됨
```
해결:
- Animator Controller가 Player에 할당되었는지 확인
- Animation Clips이 State에 할당되었는지 확인
- Sprite Renderer가 있는지 확인
```

### 문제 2: State 전환이 안 됨
```
해결:
- Transition의 Conditions 확인
- Has Exit Time 체크 해제 확인
- Speed 값이 올바르게 설정되는지 로그 확인
```

### 문제 3: 애니메이션이 너무 빠름/느림
```
해결:
각 State 선택 → Inspector:
- Speed: 1.0 (기본값)
- 값을 올리면 빠름, 낮추면 느림
```

### 문제 4: Idle로 돌아가지 않음
```
해결:
- Walk → Idle Transition 확인
- 조건: Speed Less 0.1
- Has Exit Time: ☐ 체크 해제
```

### 문제 5: 달리기가 작동 안 함
```
해결:
- Speed 값이 5.0 이상인지 확인
- Shift 키 입력이 감지되는지 확인
- animSpeed 계산 로직 확인
```

---

## 📚 추가 학습 자료

### Unity 공식 문서
- [Animator Controller](https://docs.unity3d.com/Manual/class-AnimatorController.html)
- [Animation Parameters](https://docs.unity3d.com/Manual/AnimationParameters.html)
- [State Machine Transitions](https://docs.unity3d.com/Manual/class-Transition.html)
- [Blend Trees](https://docs.unity3d.com/Manual/class-BlendTree.html)

### 권장 워크플로우
```
1. 스프라이트 준비 → Import 설정
2. Animation Clips 생성 (Idle, Walk, Run)
3. Animator Controller 생성
4. Parameters 추가 (Speed)
5. States 생성 및 Clips 할당
6. Transitions 설정
7. 테스트 및 조정
8. 필요시 Blend Tree로 고도화
```

---

**참고:** 이 가이드는 FlourPlayer2D에 최적화되어 있지만, 다른 2D 캐릭터에도 적용 가능합니다!

