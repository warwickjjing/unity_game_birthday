# 🚪 문(Door) 상호작용 가이드

## 📋 목차
1. [개요](#개요)
2. [자동 생성된 문](#자동-생성된-문)
3. [Floating UI 프롬프트](#floating-ui-프롬프트)
4. [수동으로 문 추가하기](#수동으로-문-추가하기)
5. [커스터마이징](#커스터마이징)

---

## 개요

집에 자동으로 생성되는 **상호작용 가능한 문**입니다.

### ✨ 주요 기능
- ✅ **E키로 열고 닫기**: 플레이어가 문 근처에서 E키를 누르면 작동
- ✅ **Floating UI 프롬프트**: "문 열기 [E]" 텍스트가 3D 공간에 표시
- ✅ **부드러운 애니메이션**: 문이 90도 회전하며 열림
- ✅ **자동 생성**: HouseGenerator가 입구와 각 방 문을 자동 생성
- ✅ **잠금 기능**: 필요 시 문을 잠글 수 있음

---

## 자동 생성된 문

`Tools → Generate House + Ingredients`를 실행하면 다음 문들이 자동으로 생성됩니다:

### 📍 생성되는 문

| 문 이름 | 위치 | 설명 |
|---------|------|------|
| **EntranceDoor** | 거실 입구 | 집의 정문 |
| **Room1Door** | 침실1 입구 | 첫 번째 침실 문 |
| **Room2Door** | 침실2 입구 | 두 번째 침실 문 |

### 🎮 사용 방법

1. **문 가까이 다가가기**
   - 플레이어가 문 근처(2m 이내)에 가면 자동으로 프롬프트 표시

2. **Floating UI 확인**
   - 문 위에 "문 열기 [E]" 또는 "문 닫기 [E]" 텍스트 표시

3. **E키 누르기**
   - 문이 90도 회전하며 열림 (또는 닫힘)

---

## Floating UI 프롬프트

### 📢 표시 내용

- **문이 닫혔을 때**: "문 열기 [E]"
- **문이 열렸을 때**: "문 닫기 [E]"
- **문이 잠겼을 때**: "문이 잠겨있습니다"

### 🎨 UI 특징

- **빌보드 효과**: 항상 카메라를 향함
- **거리 기반 표시**: 10m 이내에만 표시
- **3D 공간에 배치**: 문 위 2m 높이에 떠있음
- **반투명 배경**: 검은색 배경 (투명도 70%)
- **하얀색 텍스트**: 선명하게 보이는 흰색

---

## 수동으로 문 추가하기

### 방법 1: GameObject로 추가

1. **빈 GameObject 생성**
   ```
   GameObject → Create Empty
   이름: "MyDoor"
   ```

2. **DoorPivot 추가**
   ```
   MyDoor 우클릭 → Create Empty
   이름: "DoorPivot"
   Position: (-0.5, 0, 0)  // 문의 왼쪽 축
   ```

3. **문짝 추가**
   ```
   DoorPivot 우클릭 → 3D Object → Cube
   이름: "DoorPanel"
   Position: (0.5, 1, 0)
   Scale: (1, 2, 0.1)
   ```

4. **스크립트 추가**
   - `MyDoor`에 `Door.cs` 컴포넌트 추가
   - `MyDoor`에 `WorldSpaceInteractionPrompt.cs` 컴포넌트 추가
   - `Door` 스크립트의 `Door Pivot` 필드에 `DoorPivot` 할당

5. **Collider 추가**
   - `MyDoor`에 `BoxCollider` 추가
   - Center: (0, 1, 0)
   - Size: (1.5, 2, 0.5)

6. **Layer 설정**
   - `MyDoor`의 Layer를 `Interactable`로 설정

### 방법 2: Prefab 사용 (추천)

1. **기존 문 복사**
   ```
   Scene Hierarchy에서 EntranceDoor 선택
   Ctrl+D로 복제
   원하는 위치로 이동
   ```

2. **이름 변경**
   ```
   Inspector에서 이름 변경 (예: "BedroomDoor")
   ```

---

## 커스터마이징

### 🎨 문 색상 변경

Door의 `DoorPanel` 선택 후:

```
Inspector → Material → Color
원하는 색상으로 변경
```

### ⚙️ 문 설정 변경

Door 오브젝트 선택 후 Inspector에서:

| 속성 | 설명 | 기본값 |
|------|------|--------|
| **Open Angle** | 문이 열리는 각도 | 90 |
| **Closed Angle** | 문이 닫힌 각도 | 0 |
| **Open Speed** | 열리는 속도 | 3 |
| **Auto Close** | 자동으로 닫힐지 여부 | false |
| **Auto Close Delay** | 자동 닫힘 대기 시간 | 3초 |
| **Is Locked** | 문이 잠겼는지 여부 | false |

### 🔒 문 잠그기

코드에서:

```csharp
// 문 잠그기
door.SetLocked(true);

// 문 잠금 해제
door.Unlock();
```

Inspector에서:
```
Door 컴포넌트 → Lock Settings → Is Locked 체크
```

### 📢 프롬프트 높이 조정

WorldSpaceInteractionPrompt 컴포넌트:

```
Prompt Height: 2.0  // 문 위 2m 높이
Max Visible Distance: 10.0  // 10m까지 표시
```

### 🎵 사운드 추가 (선택사항)

Door 컴포넌트에 사운드 추가:

1. **AudioClip 준비**
   - 문 열리는 소리 (openSound)
   - 문 닫히는 소리 (closeSound)
   - 잠긴 문 소리 (lockedSound)

2. **Inspector에서 할당**
   ```
   Door → Audio (Optional)
   Open Sound: [AudioClip 할당]
   Close Sound: [AudioClip 할당]
   Locked Sound: [AudioClip 할당]
   ```

3. **AudioSource 추가**
   - Door GameObject에 `AudioSource` 컴포넌트 추가
   - Play On Awake: 체크 해제
   - Spatial Blend: 1 (3D)

---

## 문제 해결

### ❌ "E키로 열기" 프롬프트가 안 보임

**원인**: WorldSpaceInteractionPrompt가 없음

**해결**:
1. Door GameObject 선택
2. `Add Component` → `WorldSpaceInteractionPrompt`

---

### ❌ E키를 눌러도 문이 안 열림

**원인 1**: Layer가 Interactable이 아님

**해결**:
```
Door GameObject 선택
Inspector 상단 → Layer → Interactable
```

**원인 2**: Collider가 없음

**해결**:
```
Door GameObject 선택
Add Component → Box Collider
```

**원인 3**: Player에 Interactor가 없음

**해결**:
```
Player GameObject 확인
Interactor 컴포넌트가 있는지 확인
```

---

### ❌ 문이 이상하게 회전함

**원인**: DoorPivot 위치가 잘못됨

**해결**:
```
DoorPivot의 Position을 (-0.5, 0, 0)으로 설정
(문의 왼쪽 끝이 회전 중심)
```

---

### ❌ 프롬프트 텍스트가 뒤집힘

**원인**: 빌보드 회전이 잘못됨

**해결**:
- 이미 수정됨! `WorldSpaceInteractionPrompt.cs`에서 자동으로 처리

---

### ❌ 문이 너무 빨리/느리게 열림

**해결**:
```
Door 컴포넌트 → Open Speed 조정
값이 클수록 빠름 (기본: 3)
```

---

## 고급 기능

### 🔑 열쇠로 문 열기

```csharp
public class Key : MonoBehaviour, IInteractable
{
    public Door targetDoor;

    public void Interact(GameObject interactor)
    {
        if (targetDoor != null)
        {
            targetDoor.Unlock();
            Debug.Log("문이 잠금 해제되었습니다!");
            Destroy(gameObject); // 열쇠 소멸
        }
    }
}
```

### 🎬 Cutscene에서 문 제어

```csharp
// Timeline Signal Receiver에서
public void OpenDoor()
{
    door.OpenDoor();
}

public void CloseDoor()
{
    door.CloseDoor();
}
```

### 🔊 문 열림 이벤트 구독

```csharp
// Door.cs를 수정하여 UnityEvent 추가
using UnityEngine.Events;

[SerializeField] private UnityEvent onDoorOpened;
[SerializeField] private UnityEvent onDoorClosed;

// OpenDoor() 메소드에서
onDoorOpened?.Invoke();

// CloseDoor() 메소드에서
onDoorClosed?.Invoke();
```

---

## 참고 자료

- **관련 스크립트**:
  - `Assets/Scripts/Props/Door.cs`
  - `Assets/Scripts/UI/WorldSpaceInteractionPrompt.cs`
  - `Assets/Scripts/Interaction/IInteractable.cs`
  - `Assets/Scripts/Player/Interactor.cs`

- **관련 가이드**:
  - `Docs/House_Generator_Complete_Guide.md`
  - `Docs/Scene_Setup_Guide.md`

---

**마지막 업데이트**: 2026-02-02  
**버전**: 1.0 (Floating UI 프롬프트 추가)

