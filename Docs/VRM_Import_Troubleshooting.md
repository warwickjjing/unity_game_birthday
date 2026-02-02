# VRM 임포트 문제 해결 가이드

## 문제: Shader를 찾을 수 없음 (ArgumentNullException)

### 증상
```
ArgumentNullException: Value cannot be null.
Parameter name: Shader
```

### 원인
VRM의 MToon 셰이더를 찾을 수 없습니다. 다음 중 하나일 수 있습니다:
1. VRMShaders 패키지가 제대로 설치되지 않음
2. URP 설정이 누락됨
3. 패키지 간 버전 불일치

### 해결 방법

#### 방법 1: 완전 재설치 (가장 확실)

1. **현재 VRM 패키지 제거**
   - `Packages` 폴더에서 다음 폴더들 삭제:
     - `com.vrmc.gltf`
     - `com.vrmc.univrm`
     - `com.vrmc.vrm`
   - Unity 재시작

2. **최신 UniVRM UnityPackage 다운로드**
   - https://github.com/vrm-c/UniVRM/releases
   - `UniVRM-0.125.0_xxxx.unitypackage` (또는 최신 버전)

3. **전체 임포트**
   - Assets → Import Package → Custom Package
   - **모든 항목 체크** (특히 Shaders 관련)
   - Import 클릭

4. **Unity 재시작**

5. **VRM 파일 재임포트**
   - VRM 파일 우클릭 → Reimport

#### 방법 2: URP 설정 확인

1. **Graphics Settings 확인**
   - Edit → Project Settings → Graphics
   - Scriptable Render Pipeline Settings 확인

2. **URP Asset이 없으면**:
   - `Assets/Settings/UniversalRP-HighQuality.asset` 사용
   - 또는 새로 생성:
     - Assets 우클릭 → Create → Rendering → URP → Pipeline Asset (Forward Renderer)

3. **Graphics Settings에 할당**
   - Graphics → Scriptable Render Pipeline Settings
   - 생성한 URP Asset 드래그

#### 방법 3: Quality Settings 확인

1. **Edit → Project Settings → Quality**
2. **각 Quality Level**에서:
   - Rendering → Render Pipeline Asset
   - URP Asset 할당

#### 방법 4: MToon 셰이더 수동 확인

1. **Project 창에서 검색**: `t:Shader MToon`
2. **셰이더가 있는지 확인**:
   - `VRM/MToon`
   - `VRM10/MToon10`
   - `UniGLTF/UniUnlit`

3. **셰이더가 없으면**: UniVRM이 제대로 설치되지 않은 것 → 방법 1 수행

## 문제: NotVrm0Exception

### 증상
```
NotVrm0Exception: Exception of type 'VRM.NotVrm0Exception' was thrown.
```

### 원인
VRM 1.0 파일을 VRM 0.x 임포터로 열려고 시도

### 해결 방법

1. **VRM 1.0 메뉴 사용**
   - Unity 상단 메뉴: **VRM1** → **Import VRM 1.0**
   - (VRM0 메뉴 말고 VRM1 메뉴 사용)

2. **또는 자동 인식**
   - Unity 재시작 후 VRM 파일 Reimport
   - 최신 UniVRM은 자동으로 버전 감지

## 문제: 프리팹이 생성되지 않음

### 해결 체크리스트

- [ ] UniVRM이 제대로 설치되었는지 확인 (Unity 메뉴에 VRM0/VRM1 메뉴)
- [ ] VRM 파일을 Reimport 했는지 확인
- [ ] Unity를 재시작했는지 확인
- [ ] Console에 에러가 없는지 확인
- [ ] URP Asset이 설정되었는지 확인

## 권장 설치 순서 (처음부터)

1. **Unity 프로젝트 새로 열기** (2022.3 LTS)
2. **URP 설정**
   - Graphics Settings에 URP Asset 할당
   - Quality Settings에 URP Asset 할당
3. **UniVRM UnityPackage 다운로드 및 설치**
4. **Unity 재시작**
5. **VRM 파일 임포트**

## 추가 리소스

- UniVRM 공식 문서: https://vrm.dev/univrm/
- UniVRM GitHub: https://github.com/vrm-c/UniVRM
- VRM 포맷 공식: https://vrm.dev/

