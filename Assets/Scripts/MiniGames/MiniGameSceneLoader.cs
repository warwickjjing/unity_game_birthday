using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using BirthdayCakeQuest.Utils;
using BirthdayCakeQuest.Ingredients;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 미니게임 Scene 로드를 관리합니다.
    /// </summary>
    public static class MiniGameSceneLoader
    {
        // Scene 이름 상수
        public const string FLOUR_SCENE_NAME = "FlourMiniGameScene";
        public const string SUGAR_SCENE_NAME = "SugarMiniGameScene";

        // Home 씬 카메라 설정 저장
        private static bool _homeCameraOrthographic;
        private static float _homeCameraOrthographicSize;
        private static float _homeCameraFieldOfView;

        /// <summary>
        /// 미니게임 Scene을 로드합니다.
        /// </summary>
        public static void LoadMiniGameScene(MiniGameType type, Action<bool> onComplete)
        {
            string sceneName = GetSceneNameForMiniGame(type);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[MiniGameSceneLoader] {type} 미니게임에 대한 Scene이 정의되지 않았습니다!");
                onComplete?.Invoke(false);
                return;
            }

            // 현재 Scene 이름 저장
            string currentSceneName = SceneManager.GetActiveScene().name;

            // MiniGameResult 싱글톤 생성 (없으면)
            MiniGameResult resultManager = MiniGameResult.Instance;
            if (resultManager == null)
            {
                GameObject resultObj = new GameObject("MiniGameResult");
                resultManager = resultObj.AddComponent<MiniGameResult>();
            }

            // 미니게임 준비
            resultManager.PrepareMiniGame(type, currentSceneName, onComplete);

            Debug.Log($"[MiniGameSceneLoader] {sceneName} Scene 로드 중...");

            // SceneTransitionManager를 사용한 페이드 전환
            var transitionManager = SceneTransitionManager.Instance;
            if (transitionManager == null)
            {
                // SceneTransitionManager가 없으면 생성
                GameObject transitionObj = new GameObject("SceneTransitionManager");
                transitionManager = transitionObj.AddComponent<SceneTransitionManager>();
            }

            // 페이드 전환과 함께 Scene 로드
            transitionManager.StartCoroutine(LoadWithFade(sceneName, transitionManager));
        }

        /// <summary>
        /// 페이드 효과와 함께 Scene을 Additive로 로드합니다.
        /// Home 씬은 유지되고 미니게임 씬만 오버레이됩니다.
        /// </summary>
        private static IEnumerator LoadWithFade(string sceneName, SceneTransitionManager transitionManager)
        {
            // Home 씬의 메인 카메라 설정 저장
            UnityEngine.Camera homeCamera = UnityEngine.Camera.main;
            if (homeCamera != null)
            {
                _homeCameraOrthographic = homeCamera.orthographic;
                _homeCameraOrthographicSize = homeCamera.orthographicSize;
                _homeCameraFieldOfView = homeCamera.fieldOfView;
                Debug.Log($"[MiniGameSceneLoader] Home 카메라 설정 저장 - orthographic: {_homeCameraOrthographic}, FOV: {_homeCameraFieldOfView}");
            }

            // 페이드 아웃
            yield return transitionManager.FadeOut();

            // 다른 미니게임 씬들이 로드되어 있으면 언로드
            Scene[] allScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                allScenes[i] = SceneManager.GetSceneAt(i);
            }

            foreach (Scene sceneToCheck in allScenes)
            {
                // Home 씬과 TitleScene이 아니고, 미니게임 씬이면 언로드
                if (sceneToCheck.name != "Home" && sceneToCheck.name != "TitleScene" && 
                    (sceneToCheck.name == FLOUR_SCENE_NAME || sceneToCheck.name == SUGAR_SCENE_NAME))
                {
                    if (sceneToCheck.isLoaded && sceneToCheck.name != sceneName)
                    {
                        Debug.Log($"[MiniGameSceneLoader] 기존 미니게임 씬 비활성화 및 언로드: {sceneToCheck.name}");
                        
                        // 먼저 모든 루트 오브젝트 비활성화 (UI가 보이지 않도록)
                        GameObject[] rootObjects = sceneToCheck.GetRootGameObjects();
                        foreach (GameObject obj in rootObjects)
                        {
                            if (obj.scene == sceneToCheck)
                            {
                                obj.SetActive(false);
                            }
                        }
                        
                        // 한 프레임 대기 (비활성화가 적용되도록)
                        yield return null;
                        
                        // 씬 언로드
                        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToCheck);
                        yield return asyncUnload;
                        
                        Debug.Log($"[MiniGameSceneLoader] {sceneToCheck.name} 언로드 완료");
                    }
                }
            }

            // Home 씬의 모든 루트 오브젝트 비활성화 (렌더링 및 업데이트 완전 차단)
            Scene homeScene = SceneManager.GetActiveScene();
            GameObject[] homeRootObjects = homeScene.GetRootGameObjects();
            
            Debug.Log($"[MiniGameSceneLoader] Home 씬 오브젝트 {homeRootObjects.Length}개 비활성화");
            foreach (GameObject obj in homeRootObjects)
            {
                // DontDestroyOnLoad 오브젝트는 제외 (SceneTransitionManager 등)
                if (obj.scene == homeScene)
                {
                    // HomeScene의 AudioListener 비활성화 (미니게임 씬의 AudioListener만 활성화)
                    AudioListener homeListener = obj.GetComponent<AudioListener>();
                    if (homeListener != null)
                    {
                        homeListener.enabled = false;
                        Debug.Log($"[MiniGameSceneLoader] Home 씬의 AudioListener 비활성화: {obj.name}");
                    }
                    
                    obj.SetActive(false);
                }
            }

            // Scene을 Additive로 로드 (Home 씬은 유지)
            Debug.Log($"[MiniGameSceneLoader] {sceneName} Additive 로드 시작");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return asyncLoad;

            // 로드된 씬을 Active Scene으로 설정
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
                Debug.Log($"[MiniGameSceneLoader] {sceneName}을 Active Scene으로 설정");
                
                // 미니게임 씬의 모든 UI를 먼저 비활성화 (페이드 인 전까지 숨김)
                GameObject[] rootObjects = loadedScene.GetRootGameObjects();
                foreach (GameObject obj in rootObjects)
                {
                    // Canvas나 UI 관련 오브젝트 찾기
                    var canvas = obj.GetComponent<UnityEngine.Canvas>();
                    if (canvas != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"[MiniGameSceneLoader] {sceneName}의 Canvas 비활성화: {obj.name}");
                    }
                }
            }

            // 한 프레임 대기 (씬이 완전히 로드되도록)
            yield return null;

            // 미니게임 씬의 카메라에 AudioListener 확인 및 추가
            EnsureAudioListenerInMiniGameScene(loadedScene);

            // 페이드 인 전에 UI 다시 활성화
            if (loadedScene.isLoaded)
            {
                GameObject[] rootObjects = loadedScene.GetRootGameObjects();
                foreach (GameObject obj in rootObjects)
                {
                    var canvas = obj.GetComponent<UnityEngine.Canvas>();
                    if (canvas != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"[MiniGameSceneLoader] {sceneName}의 Canvas 활성화: {obj.name}");
                    }
                }
            }

            // 페이드 인
            yield return transitionManager.FadeIn();
        }
        
        /// <summary>
        /// 미니게임 씬에 AudioListener가 있는지 확인하고 없으면 추가합니다.
        /// </summary>
        private static void EnsureAudioListenerInMiniGameScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                Debug.LogWarning("[MiniGameSceneLoader] 씬이 아직 로드되지 않아 AudioListener를 확인할 수 없습니다.");
                return;
            }

            // 씬의 모든 카메라 찾기
            UnityEngine.Camera[] cameras = UnityEngine.Object.FindObjectsOfType<UnityEngine.Camera>();
            UnityEngine.Camera mainCamera = null;

            foreach (var cam in cameras)
            {
                // 미니게임 씬에 속한 카메라만 확인
                if (cam.gameObject.scene == scene)
                {
                    if (cam.CompareTag("MainCamera") || mainCamera == null)
                    {
                        mainCamera = cam;
                    }
                }
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("[MiniGameSceneLoader] 미니게임 씬의 MainCamera를 찾을 수 없습니다!");
                return;
            }

            // AudioListener 확인 및 활성화
            AudioListener audioListener = mainCamera.GetComponent<AudioListener>();
            if (audioListener == null)
            {
                audioListener = mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log($"[MiniGameSceneLoader] {scene.name} 씬의 MainCamera에 AudioListener 추가됨");
            }
            else
            {
                Debug.Log($"[MiniGameSceneLoader] {scene.name} 씬의 MainCamera에 AudioListener가 이미 있습니다.");
            }
            
            // AudioListener가 비활성화되어 있으면 활성화
            if (audioListener != null && !audioListener.enabled)
            {
                audioListener.enabled = true;
                Debug.Log($"[MiniGameSceneLoader] {scene.name} 씬의 AudioListener 활성화됨");
            }
            
            // 카메라가 비활성화되어 있으면 활성화 (AudioListener가 작동하려면 카메라도 활성화되어야 함)
            if (!mainCamera.gameObject.activeInHierarchy)
            {
                mainCamera.gameObject.SetActive(true);
                Debug.Log($"[MiniGameSceneLoader] {scene.name} 씬의 MainCamera 활성화됨");
            }

            // 씬에 AudioListener가 하나만 있는지 확인
            AudioListener[] allListeners = UnityEngine.Object.FindObjectsOfType<AudioListener>();
            int listenerCount = 0;
            foreach (var listener in allListeners)
            {
                if (listener.gameObject.scene == scene && listener.enabled)
                {
                    listenerCount++;
                }
            }

            if (listenerCount > 1)
            {
                Debug.LogWarning($"[MiniGameSceneLoader] {scene.name} 씬에 AudioListener가 {listenerCount}개 있습니다! 하나만 남기고 나머지는 비활성화하세요.");
            }
        }

        /// <summary>
        /// 저장된 Home 카메라 설정을 반환합니다.
        /// </summary>
        public static void GetHomeCameraSettings(out bool orthographic, out float orthographicSize, out float fieldOfView)
        {
            orthographic = _homeCameraOrthographic;
            orthographicSize = _homeCameraOrthographicSize;
            fieldOfView = _homeCameraFieldOfView;
        }

        /// <summary>
        /// 미니게임 타입에 해당하는 Scene 이름을 반환합니다.
        /// </summary>
        private static string GetSceneNameForMiniGame(MiniGameType type)
        {
            switch (type)
            {
                case MiniGameType.Flour:
                    return FLOUR_SCENE_NAME;
                case MiniGameType.Sugar:
                    return SUGAR_SCENE_NAME;
                default:
                    return null; // Panel 방식 미니게임
            }
        }

        /// <summary>
        /// 해당 미니게임이 Scene 방식인지 확인합니다.
        /// </summary>
        public static bool IsSceneBasedMiniGame(MiniGameType type)
        {
            return type == MiniGameType.Flour || type == MiniGameType.Sugar;
        }
    }
}

