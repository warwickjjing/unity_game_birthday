# 닭 달아나기 및 계란 수집 시스템 설정 가이드

닭들이 달아난 후에만 계란을 수집할 수 있도록 설정하는 방법입니다.

## 개요

1. **ChickenCoopManager**: 닭장의 닭들이 달아난 상태를 추적합니다.
2. **CollectibleIngredient**: 조건부 상호작용 기능이 추가되었습니다 (`Requires Chickens Escaped` 옵션).
3. **AnimalWander.Escape()**: 닭들이 달아나도록 하는 메서드입니다.

## Unity Inspector 설정

### 1. ChickenCoopManager 설정

1. **빈 GameObject 생성**
   - Hierarchy에서 우클릭 → `Create Empty`
   - 이름을 `ChickenCoopManager`로 변경

2. **ChickenCoopManager 컴포넌트 추가**
   - `ChickenCoopManager` GameObject 선택
   - `Add Component` → `Chicken Coop Manager` 추가

3. **설정 항목**
   - **Chickens**: 닭장에 있는 모든 닭 오브젝트들을 리스트에 추가
     - `+` 버튼을 눌러 닭 오브젝트들을 드래그 앤 드롭
   - **Escape Distance**: 닭들이 달아난 것으로 간주할 거리 (기본값: 5)
     - 닭장 중심에서 이 거리 이상 떨어지면 달아난 것으로 간주
   - **Coop Center**: 닭장의 중심 위치 (Transform 또는 빈 GameObject)
     - 비어있으면 `ChickenCoopManager` GameObject의 위치 사용
   - **Escape Speed Multiplier**: 달아날 때 속도 배율 (기본값: 2)
   - **Remove Radius Limit On Escape**: 달아날 때 반경 제한 해제 여부 (기본값: true)

### 2. 계란 오브젝트 설정

1. **계란 오브젝트 찾기**
   - Hierarchy에서 `Ingredient_Egg` GameObject 찾기

2. **CollectibleIngredient 컴포넌트 확인**
   - `Ingredient_Egg` GameObject 선택
   - `Collectible Ingredient` 컴포넌트가 있는지 확인 (없으면 추가)

3. **설정 항목**
   - **Ingredient Id**: `Egg` 선택
   - **Requires Mini Game**: 미니게임 필요 여부 (기존 설정 유지)
   - **Requires Chickens Escaped**: ✅ **체크** (닭들이 달아난 후에만 수집 가능)
   - **Blocked Message**: 조건이 만족되지 않았을 때 표시할 메시지 (선택사항, 기본값: "아직 수집할 수 없습니다")

### 3. 고양이 상호작용 설정 (선택사항)

고양이를 닭장에 넣었을 때 닭들이 달아나도록 하려면:

1. **고양이 상호작용 스크립트 수정**
   - 고양이를 닭장에 넣는 상호작용이 있는 스크립트를 찾아서
   - 다음 코드를 추가:

```csharp
// 고양이를 닭장에 넣었을 때
var coopManager = ChickenCoopManager.Instance;
if (coopManager != null)
{
    coopManager.TriggerChickenEscape();
}
```

또는 Unity Inspector에서:
- 고양이 상호작용 이벤트에 `ChickenCoopManager`의 `TriggerChickenEscape()` 메서드를 연결

## 테스트

### 수동 테스트

1. **모든 닭을 달아난 상태로 표시**
   - `ChickenCoopManager` GameObject 선택
   - Inspector에서 우클릭 → `Mark All Chickens Escaped (Test)` 선택
   - 이제 계란을 수집할 수 있어야 합니다.

2. **게임 플레이 테스트**
   - 게임을 실행하고 고양이를 닭장에 넣기
   - 닭들이 달아나는지 확인
   - 닭들이 모두 달아난 후 계란을 수집할 수 있는지 확인

## 디버깅

### 콘솔 로그 확인

- `[ChickenCoopManager]` 로그: 닭들이 달아나는 상태 추적
- `[CollectibleIngredient]` 로그: 계란 수집 조건 확인

### Gizmos 확인

- Scene 뷰에서 `ChickenCoopManager` GameObject 선택
- 빨간색 원: 닭들이 달아나야 하는 거리
- 노란색 원: 닭장 중심 위치

## 문제 해결

### 계란을 수집할 수 없음

1. `ChickenCoopManager`가 씬에 있는지 확인
2. `CollectibleIngredient`의 `Requires Chickens Escaped`가 체크되어 있는지 확인
3. 콘솔에서 `AllChickensEscaped` 상태 확인

### 닭들이 달아나지 않음

1. `ChickenCoopManager`의 `Chickens` 리스트에 닭들이 모두 추가되어 있는지 확인
2. 각 닭에 `AnimalWander` 컴포넌트가 있는지 확인
3. `TriggerChickenEscape()`가 호출되는지 확인

### 닭들이 너무 느리게 달아남

1. `Escape Speed Multiplier` 값을 증가 (예: 3 또는 4)
2. 각 닭의 `AnimalWander`에서 `Walk Speed`와 `Run Speed` 값 확인

