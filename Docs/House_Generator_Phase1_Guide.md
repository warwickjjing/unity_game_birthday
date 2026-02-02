# 🏠 집 생성 가이드 - Phase 1: 입구

3D 투어를 기반으로 실제 집 구조를 정확하게 재현합니다.

## 🎯 Phase 1 완료 상태

### ✅ 생성된 공간

1. **외부 입구** - 문 밖 복도
2. **입구 문** - E키로 열고 닫을 수 있는 문
3. **입구 복도** - 밝은 나무 바닥, 좁고 긴 복도
4. **신발장** - 입구 좌측 빌트인 수납
5. **화장실** - 입구 우측 작은 화장실 (변기, 세면대)

---

## 🚀 사용 방법

### 1. 집 생성

Unity 상단 메뉴:
```
Tools → Birthday Cake Quest → Generate House + Ingredients
```

### 2. 생성 결과 확인

Hierarchy에 다음 구조가 생성됩니다:

```
House
├── OutsideEntrance (외부)
│   └── OutsideFloor
│
├── EntranceDoor (입구 문) 🚪
│   ├── DoorFrame
│   └── DoorPivot
│       └── DoorPanel
│
├── EntranceHallway (입구 복도)
│   ├── HallwayFloor (밝은 나무)
│   ├── Wall_Left
│   ├── Wall_Right_Front
│   ├── Wall_Right_Back
│   └── Ceiling
│
├── ShoeStorage (신발장) 👟
│   └── CabinetBody
│
└── EntranceBathroom (화장실) 🚽
    ├── BathroomFloor (타일)
    ├── Walls (4개)
    ├── Toilet
    └── Sink
```

---

## 🔧 입구 문 설정 (중요!)

### Door 컴포넌트 추가

1. **Hierarchy → House → EntranceDoor 선택**

2. **Add Component → Door**

3. **설정**:
   ```
   Door Settings:
   - Open Angle: 90
   - Closed Angle: 0
   - Open Speed: 3
   - Auto Close: ✓ (선택)
   - Auto Close Delay: 3

   Visual:
   - Door Pivot: EntranceDoor/DoorPivot 드래그
   ```

4. **Collider 확인**:
   - Box Collider가 자동 생성되어 있어야 함
   - 없으면 Add Component → Box Collider

---

## 🎮 플레이어 시작 위치

### Player 배치

```
Position: (0, 0, -10)  ← 문 밖에서 시작
Rotation: (0, 0, 0)
```

이렇게 하면 플레이어가:
1. 외부 입구에서 시작
2. 정면의 문으로 다가감
3. E키로 문 열기
4. 복도 진입

---

## 📐 좌표 시스템

```
       Z+ (북쪽)
        ↑
        │
W ←────┼────→ E
        │
        ↓
       Z- (남쪽, 입구)

입구: Z = -5
복도: Z = -2 ~ 2
```

---

## 🎨 색상 가이드

| 요소 | 색상 | RGB |
|------|------|-----|
| 외부 바닥 | 회색 | (0.6, 0.6, 0.6) |
| 복도 바닥 | 밝은 나무 | (0.9, 0.87, 0.8) |
| 문 | 진한 갈색 | (0.35, 0.3, 0.28) |
| 벽 | 흰색 | (0.95, 0.95, 0.95) |
| 신발장 | 흰색 | (0.95, 0.95, 0.95) |
| 화장실 바닥 | 베이지 타일 | (0.85, 0.82, 0.78) |

---

## 🔄 다음 단계 (Phase 2)

다음 공간을 3D 투어로 보여주시면 추가합니다:

- [ ] 복도 끝 (거실로 연결)
- [ ] 입구 좌측 방
- [ ] 거실
- [ ] 주방
- [ ] 안방
- [ ] 안방 화장실/드레스룸
- [ ] 발코니

각 공간을 실제 사진/3D 투어로 보여주시면:
→ HouseGenerator 스크립트에 해당 공간 추가
→ 정확한 크기와 배치 반영

---

## 🐛 문제 해결

### 문제: 문이 열리지 않음

**해결**:
1. EntranceDoor에 Door 컴포넌트가 있는지 확인
2. Door Pivot이 연결되었는지 확인
3. Collider가 있는지 확인
4. 플레이어가 문 근처에 있는지 확인 (2m 이내)

### 문제: 문이 이상한 방향으로 열림

**해결**:
1. Door 컴포넌트:
   - Open Angle: 90 (우측으로 열림)
   - Open Angle: -90 (좌측으로 열림)
2. Door Pivot의 위치 조정

### 문제: 플레이어가 벽을 통과함

**해결**:
모든 벽에 Collider가 있는지 확인 (Primitive Cube는 자동으로 있음)

---

## 📝 커스터마이징

### 복도 길이 조정

`HouseGenerator.cs` → `CreateEntranceHallway()`:

```csharp
floor.transform.localScale = new Vector3(1.5f * SCALE, 0.1f, 4f * SCALE);
                                                          // ↑ 이 값 변경
```

### 화장실 크기 조정

`CreateEntranceBathroom()`:

```csharp
floor.transform.localScale = new Vector3(1.5f, 0.1f, 2f);
                                        // ↑ 너비  ↑ 깊이
```

---

## ✅ Phase 1 체크리스트

생성 후 확인:

- [ ] 외부 입구 바닥 (회색)
- [ ] 입구 문 (갈색)
- [ ] 문에 Door 컴포넌트 추가
- [ ] Door Pivot 연결
- [ ] 복도 바닥 (밝은 나무)
- [ ] 복도 벽 (좌/우)
- [ ] 신발장 (좌측)
- [ ] 화장실 (우측)
- [ ] 화장실 설비 (변기, 세면대)
- [ ] Player 시작 위치 (0, 0, -10)

---

**다음 공간의 3D 투어를 보여주시면 계속 추가하겠습니다!** 📸✨

