# UniVRM 설치 및 통합 가이드

이 문서는 Birthday Cake Quest에서 VRM 캐릭터를 사용하기 위한 UniVRM 설치 및 설정 가이드입니다.

## 1. UniVRM 설치

### 방법 A: UnityPackage 다운로드 (권장)

1. **UniVRM 릴리스 페이지 방문**
   - https://github.com/vrm-c/UniVRM/releases

2. **최신 버전 다운로드**
   - Unity 2022.3.x 호환 버전 확인
   - `UniVRM-x.x.x_xxxx.unitypackage` 다운로드

3. **Unity로 임포트**
   - Unity Editor에서 Assets → Import Package → Custom Package
   - 다운로드한 `.unitypackage` 파일 선택
   - Import 창에서 모든 항목 선택 후 `Import` 클릭

4. **설치 확인**
   - `Assets/VRM/` 폴더가 생성되었는지 확인
   - `Assets/VRMShaders/` 폴더가 생성되었는지 확인

5. **셰이더 확인** (중요!)
   - Project 창에서 검색: `t:Shader MToon`
   - 다음 셰이더들이 보여야 함:
     - `VRM/MToon`
     - `VRM10/MToon10`
     - `UniGLTF/UniUnlit`
   - **셰이더가 없으면**: UniVRM UnityPackage를 다시 임포트할 때 **모든 항목 체크** 확인!

### 방법 B: UPM (Unity Package Manager) - 실험적

1. Window → Package Manager 열기
2. 좌측 상단 `+` 버튼 → `Add package from git URL...`
3. 다음 URL 입력:
   ```
   https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.123.0
   ```
   (최신 버전 번호로 교체 필요)

## 2. VRM 파일 임포트

### VRM 파일 준비

1. **VRM 파일 위치**
   - VRM 캐릭터 파일(`.vrm`)을 준비
   - 예: `wife_character.vrm`

2. **프로젝트로 임포트**
   - `Assets/VRM/Models/` 폴더 생성 (선택)
   - VRM 파일을 해당 폴더로 드래그 앤 드롭

3. **자동 변환**
   - Unity가 자동으로 VRM 파일을 처리합니다
   - Import 설정 창이 나타나면 기본 설정으로 `Apply` 클릭

4. **프리팹 생성 확인**
   - VRM 파일과 동일한 위치에 프리팹이 자동 생성됩니다
   - 예: `wife_character.prefab`

## 3. 플레이어 캐릭터 설정

### VRM 프리팹을 플레이어로 만들기

1. **씬에 배치**
   - VRM 프리팹을 `Home.unity` 씬으로 드래그
   - Hierarchy에서 이름을 `Player`로 변경

2. **CharacterController 추가**
   - Player 오브젝트 선택
   - Inspector에서 `Add Component` → `Character Controller`
   - 설정:
     - **Height**: 1.8 (캐릭터 높이에 맞게 조정)
     - **Radius**: 0.3
     - **Center**: (0, 0.9, 0) - 높이의 절반

3. **PlayerController 스크립트 추가**
   - `Add Component` → `Player Controller`
   - 설정:
     - **Walk Speed**: 3
     - **Run Speed**: 6
     - **Rotation Speed**: 10
     - **Ground Check Distance**: 0.2
     - **Ground Mask**: Default (또는 바닥 레이어)

4. **Interactor 스크립트 추가**
   - `Add Component` → `Interactor`
   - 설정:
     - **Detection Radius**: 2
     - **Ingredient Layer**: Everything
     - **Interact Key**: E
     - **Interaction Prompt**: (선택) UI 오브젝트 참조

5. **태그 설정**
   - Inspector 상단에서 Tag를 `Player`로 설정
   - (없으면 Add Tag로 생성)

## 4. VRM 애니메이션 설정 (선택)

### Humanoid 리그 확인

1. **VRM 프리팹 선택**
2. **Inspector → Rig 탭**
3. **Animation Type**: Humanoid로 설정되어 있는지 확인
   - UniVRM이 자동으로 설정해줍니다

### 애니메이션 추가 (선택)

현재 Birthday Cake Quest는 기본 이동만 구현되어 있지만, 추가하려면:

1. **Animator Controller 생성**
   - Assets에서 우클릭 → Create → Animator Controller
   - 이름: `PlayerAnimator`

2. **애니메이션 클립 추가**
   - Idle, Walk, Run 애니메이션 클립 준비
   - Animator Controller에 State 추가

3. **Player에 Animator 추가**
   - Player 오브젝트에 `Animator` 컴포넌트 추가
   - Controller에 생성한 `PlayerAnimator` 할당

4. **PlayerController 수정**
   - 필요시 애니메이션 파라미터 설정 코드 추가

## 5. VRM 머티리얼 설정 (URP)

### URP 머티리얼 변환

VRM 캐릭터가 제대로 보이지 않는다면:

1. **VRM 프리팹 선택**
2. **Renderer 컴포넌트 확인**
3. **Materials 섹션에서 각 머티리얼 클릭**

4. **셰이더 확인/변경**
   - **VRM/MToon**: UniVRM 기본 셰이더 (URP 호환)
   - 문제가 있다면: `Shader` → `VRM10/MToon10` 선택

5. **일괄 변환 (선택)**
   - Edit → Render Pipeline → Universal Render Pipeline
   - → Upgrade Project Materials to URP Materials

## 6. 문제 해결

### VRM 임포트 시 Shader 에러

**증상**: `ArgumentNullException: Value cannot be null. Parameter name: Shader`

**해결**: [VRM 임포트 문제 해결 가이드](VRM_Import_Troubleshooting.md) 참고

**빠른 해결**:
1. UniVRM UnityPackage 재설치 (**모든 항목 체크**)
2. Unity 재시작
3. VRM 파일 우클릭 → Reimport

### VRM이 분홍색으로 보임

**원인**: 머티리얼이 URP와 호환되지 않음

**해결**:
1. VRM 프리팹의 모든 Materials 확인
2. Shader를 `VRM/MToon` 또는 `VRM10/MToon10`으로 변경
3. 또는 위의 "URP 머티리얼 변환" 참고

### VRM 임포트 시 에러

**원인**: UniVRM 버전 호환 문제

**해결**:
1. UniVRM 최신 버전으로 업데이트
2. Unity 2022.3 LTS 호환 버전 확인
3. Console 에러 메시지 확인 후 해당 이슈 검색

### 캐릭터가 바닥을 뚫고 떨어짐

**원인**: CharacterController 설정 문제

**해결**:
1. CharacterController의 **Center** 값 조정
2. **Ground Check Distance** 값 증가 (0.3~0.5)
3. 바닥에 Collider가 있는지 확인

### 애니메이션이 작동하지 않음

**원인**: Animator Controller 미설정 또는 애니메이션 클립 부족

**해결**:
1. VRM 캐릭터에 기본 애니메이션이 없는 경우가 많음
2. 별도로 Humanoid 호환 애니메이션 구매/다운로드 필요
3. 또는 애니메이션 없이 이동만 구현 (현재 설정)

## 7. 추가 리소스

- **UniVRM 공식 문서**: https://vrm.dev/univrm/
- **VRM 포맷 공식 사이트**: https://vrm.dev/
- **Unity VRM 샘플**: https://github.com/vrm-c/UniVRM/tree/master/Assets/VRM_Samples

## 8. 다음 단계

VRM 캐릭터 설정이 완료되었다면:
1. 메인 README의 "씬 구성 가이드" 참고
2. 카메라, UI, 재료, 컷씬 설정 진행
3. Play 모드로 테스트

---

**참고**: UniVRM은 계속 업데이트되므로, 최신 버전의 설치 방법은 공식 GitHub를 참고하세요.

