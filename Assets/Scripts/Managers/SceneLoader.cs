using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

namespace BirthdayCakeQuest.Managers
{
    /// <summary>
    /// 씬 전환을 관리하는 유틸리티 클래스입니다.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;

        [Header("Background Music")]
        [Tooltip("타이틀 씬 배경음 (파일 할당 시 자동 재생)")]
        [SerializeField] private AudioClip titleMusic;
        
        [Tooltip("홈 씬 배경음 (파일 할당 시 자동 재생)")]
        [SerializeField] private AudioClip homeMusic;
        
        [Tooltip("배경음 볼륨 (0~1), 옵션에서 저장 시 PlayerPrefs로 덮어씀")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
        
        [Tooltip("음악 전환 페이드 시간 (초)")]
        [SerializeField] private float musicFadeDuration = 1f;

        private const string PrefsBGMVolume = "BGMVolume";
        private const string PrefsSFXVolume = "SFXVolume";
        private const string PrefsFullscreen = "Fullscreen";

        private AudioSource _musicSource;
        private bool _isFirstSceneLoad = true; // 첫 씬 로드 여부

        /// <summary>
        /// 싱글톤 인스턴스. 씬에 배치된 SceneLoader 중 Title/Home Music이 할당된 것을 우선 사용합니다.
        /// (Plan 추가 후 다른 스크립트가 Instance를 먼저 쓰면 빈 인스턴스가 만들어져 BGM이 None이 되는 일을 막기 위함.)
        /// </summary>
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 비활성 오브젝트 포함해 전부 찾고, titleMusic이 할당된 것을 우선 사용 (씬에 설정해 둔 것)
                    SceneLoader[] all = Object.FindObjectsOfType<SceneLoader>(true);
                    SceneLoader withClips = null;
                    foreach (var loader in all)
                    {
                        if (loader == null) continue;
                        if (loader.titleMusic != null || loader.homeMusic != null)
                        {
                            withClips = loader;
                            break;
                        }
                    }
                    SceneLoader existing = withClips != null ? withClips : (all.Length > 0 ? all[0] : null);
                    if (existing != null)
                    {
                        _instance = existing;
                        return _instance;
                    }
                    GameObject go = new GameObject("SceneLoader");
                    _instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 저장된 옵션 불러오기
            if (PlayerPrefs.HasKey(PrefsBGMVolume))
                musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsBGMVolume));

            // 배경음용 AudioSource 생성
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;

            // 첫 씬 로드 여부 확인 (현재 활성 씬이 첫 씬인지)
            Scene currentScene = SceneManager.GetActiveScene();
            _isFirstSceneLoad = true;

            // 씬 로드 완료 시점 훅 (Home 진입 시 UBS UI Canvas 자동 활성화 등)
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 첫 씬이 TitleScene이면 즉시 음악 재생 (OnSceneLoaded는 씬 전환 시에만 호출됨)
            if (currentScene.name == "TitleScene")
            {
                StartCoroutine(PlayTitleMusicOnStart());
            }
        }

        /// <summary>
        /// Start에서 TitleScene 음악을 재생합니다 (첫 로드 시).
        /// </summary>
        private System.Collections.IEnumerator PlayTitleMusicOnStart()
        {
            // 한 프레임 대기하여 초기화 완료
            yield return null;
            
            if (titleMusic != null && !_musicSource.isPlaying)
            {
                _musicSource.clip = titleMusic;
                _musicSource.volume = musicVolume;
                _musicSource.Play();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 배경음 자동 재생
            if (scene.name == "TitleScene")
            {
                // TitleScene 로드 시 미니게임 씬들 언로드
                StartCoroutine(UnloadMiniGameScenesOnTitleScene());
                // TitleScene의 Canvas와 EventSystem이 활성화되어 있는지 확인 (EventSystem 제거 전에 실행)
                StartCoroutine(EnsureTitleSceneUI());
                
                // TitleScene의 AudioListener 확인 및 활성화
                StartCoroutine(EnsureTitleSceneAudioListener());
                
                PlayMusic(titleMusic, !_isFirstSceneLoad); // 첫 로드가 아니면 페이드 적용
                
                // TitleScene에서는 EventSystem 정리 전에 EnsureTitleSceneUI가 완료되도록 대기
                // EnsureSingleEventSystem은 EnsureTitleSceneUI 이후에 실행되도록 순서 보장
            }
            else if (scene.name == "Home")
            {
                StartCoroutine(EnableHomeUBSCanvasesNextFrame());
                PlayMusic(homeMusic, !_isFirstSceneLoad); // 첫 로드가 아니면 페이드 적용
                // QuestSequenceManager가 instance 변수를 사용하므로 플래그 리셋 불필요
                // HomeScene 로드 시마다 새로운 인스턴스가 생성되어 자동으로 플래그가 false로 초기화됨
            }
            
            // 중복된 AudioListener 제거 (하나만 남기기)
            StartCoroutine(EnsureSingleAudioListenerNextFrame());
            
            // 중복된 EventSystem 제거 (하나만 남기기) - TitleScene은 보호됨
            StartCoroutine(EnsureSingleEventSystemNextFrame());
            
            // 첫 씬 로드 완료 표시
            _isFirstSceneLoad = false;
        }

        /// <summary>
        /// TitleScene 로드 시 미니게임 씬들을 언로드합니다.
        /// </summary>
        private System.Collections.IEnumerator UnloadMiniGameScenesOnTitleScene()
        {
            // 한 프레임 대기 (씬이 완전히 로드되도록)
            yield return null;

            // 모든 로드된 씬 확인
            Scene[] allScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                allScenes[i] = SceneManager.GetSceneAt(i);
            }

            foreach (Scene sceneToCheck in allScenes)
            {
                // 미니게임 씬이면 언로드
                if (sceneToCheck.name == "FlourMiniGameScene" || sceneToCheck.name == "SugarMiniGameScene")
                {
                    if (sceneToCheck.isLoaded)
                    {
                        Debug.Log($"[SceneLoader] TitleScene 로드: 미니게임 씬 언로드 - {sceneToCheck.name}");
                        
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
                        
                        Debug.Log($"[SceneLoader] {sceneToCheck.name} 언로드 완료");
                    }
                }
            }
        }

        /// <summary>
        /// TitleScene의 AudioListener를 확인하고 활성화합니다.
        /// </summary>
        private System.Collections.IEnumerator EnsureTitleSceneAudioListener()
        {
            yield return null; // 한 프레임 대기
            
            Scene titleScene = SceneManager.GetSceneByName("TitleScene");
            if (!titleScene.isLoaded)
            {
                Debug.LogWarning("[SceneLoader] TitleScene이 아직 로드되지 않았습니다.");
                yield break;
            }
            
            // TitleScene의 MainCamera 찾기
            UnityEngine.Camera[] cameras = UnityEngine.Object.FindObjectsOfType<UnityEngine.Camera>();
            UnityEngine.Camera titleCamera = null;
            
            foreach (var cam in cameras)
            {
                if (cam.gameObject.scene == titleScene)
                {
                    if (cam.CompareTag("MainCamera") || titleCamera == null)
                    {
                        titleCamera = cam;
                    }
                }
            }
            
            if (titleCamera != null)
            {
                AudioListener audioListener = titleCamera.GetComponent<AudioListener>();
                if (audioListener == null)
                {
                    audioListener = titleCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("[SceneLoader] TitleScene의 MainCamera에 AudioListener 추가됨");
                }
                else
                {
                    if (!audioListener.enabled)
                    {
                        audioListener.enabled = true;
                        Debug.Log("[SceneLoader] TitleScene의 AudioListener 활성화됨");
                    }
                }
                
                // 카메라가 비활성화되어 있으면 활성화
                if (!titleCamera.gameObject.activeInHierarchy)
                {
                    titleCamera.gameObject.SetActive(true);
                    Debug.Log("[SceneLoader] TitleScene의 MainCamera 활성화됨");
                }
            }
            else
            {
                Debug.LogWarning("[SceneLoader] TitleScene의 MainCamera를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// TitleScene의 Canvas와 EventSystem이 활성화되어 있는지 확인합니다.
        /// </summary>
        private System.Collections.IEnumerator EnsureTitleSceneUI()
        {
            // 한 프레임 대기 (씬이 완전히 로드되도록)
            yield return null;

            // TitleScene의 Canvas 확인 및 활성화
            Canvas[] allCanvases = Object.FindObjectsOfType<Canvas>(true);
            foreach (var canvas in allCanvases)
            {
                if (canvas != null && canvas.gameObject.scene.name == "TitleScene")
                {
                    if (!canvas.gameObject.activeSelf)
                    {
                        canvas.gameObject.SetActive(true);
                        Debug.Log($"[SceneLoader] TitleScene Canvas 활성화: {canvas.gameObject.name}");
                    }
                    if (!canvas.enabled)
                    {
                        canvas.enabled = true;
                        Debug.Log($"[SceneLoader] TitleScene Canvas 컴포넌트 활성화: {canvas.gameObject.name}");
                    }
                    
                    // GraphicRaycaster 확인 및 추가
                    var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    if (raycaster == null)
                    {
                        raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                        Debug.Log($"[SceneLoader] TitleScene Canvas에 GraphicRaycaster 추가: {canvas.gameObject.name}");
                    }
                    if (!raycaster.enabled)
                    {
                        raycaster.enabled = true;
                        Debug.Log($"[SceneLoader] TitleScene Canvas GraphicRaycaster 활성화: {canvas.gameObject.name}");
                    }
                }
            }

            // TitleScene의 EventSystem 확인 및 활성화
            EventSystem[] allEventSystems = Object.FindObjectsOfType<EventSystem>(true);
            EventSystem titleEventSystem = null;
            
            // TitleScene의 EventSystem 찾기
            foreach (var eventSystem in allEventSystems)
            {
                if (eventSystem != null && eventSystem.gameObject.scene.name == "TitleScene")
                {
                    titleEventSystem = eventSystem;
                    break;
                }
            }
            
            // TitleScene의 EventSystem이 있으면 활성화
            if (titleEventSystem != null)
            {
                if (!titleEventSystem.gameObject.activeSelf)
                {
                    titleEventSystem.gameObject.SetActive(true);
                    Debug.Log($"[SceneLoader] TitleScene EventSystem 활성화: {titleEventSystem.gameObject.name}");
                }
                if (!titleEventSystem.enabled)
                {
                    titleEventSystem.enabled = true;
                    Debug.Log($"[SceneLoader] TitleScene EventSystem 컴포넌트 활성화: {titleEventSystem.gameObject.name}");
                }
                EventSystem.current = titleEventSystem;
                Debug.Log($"[SceneLoader] EventSystem.current 설정: {titleEventSystem.gameObject.name}");
            }
            else
            {
                // TitleScene에 EventSystem이 없으면 생성 (하나만)
                // 다른 씬의 EventSystem이 있는지 확인하고, 있으면 제거
                foreach (var es in allEventSystems)
                {
                    if (es != null && es.gameObject.scene.name != "TitleScene")
                    {
                        Debug.Log($"[SceneLoader] 다른 씬의 EventSystem 제거: {es.gameObject.name} (씬: {es.gameObject.scene.name})");
                        Object.Destroy(es.gameObject);
                    }
                }
                
                GameObject eventSystemObj = new GameObject("EventSystem");
                EventSystem newEventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                SceneManager.MoveGameObjectToScene(eventSystemObj, SceneManager.GetSceneByName("TitleScene"));
                EventSystem.current = newEventSystem;
                Debug.Log("[SceneLoader] TitleScene에 EventSystem 생성 및 current 설정 완료");
            }
        }

        private IEnumerator EnableHomeUBSCanvasesNextFrame()
        {
            // 씬 오브젝트들이 완전히 깨어난 다음 프레임에 처리
            yield return null;

            // 1) UBS_UI 컴포넌트가 들고 있는 Canvas 강제 활성화
            // UBS_UI는 글로벌 네임스페이스에 존재(Assets/UBS/Assets/Scripts/UBS_UI.cs)
            var ubsUIs = Object.FindObjectsOfType<UBS_UI>(true);
            foreach (var ui in ubsUIs)
            {
                if (ui == null) continue;
                if (!ui.gameObject.activeSelf) ui.gameObject.SetActive(true);
                ui.enabled = true;
                if (ui.canvas != null) ui.canvas.enabled = true;
            }

            // 2) "UBS Environment" 루트 아래에 있는 Canvas들도 전부 켜주기 (구조/이름이 달라도 안전)
            var ubsEnvRoot = GameObject.Find("UBS Environment");
            if (ubsEnvRoot != null)
            {
                var canvases = ubsEnvRoot.GetComponentsInChildren<Canvas>(true);
                foreach (var c in canvases)
                {
                    if (c == null) continue;
                    if (!c.gameObject.activeSelf) c.gameObject.SetActive(true);
                    c.enabled = true;
                }
            }

            Debug.Log("[SceneLoader] Home 진입: UBS Environment Canvas 활성화 처리 완료");
        }

        /// <summary>
        /// 씬을 이름으로 로드합니다.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 씬을 인덱스로 로드합니다.
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            Debug.Log($"[SceneLoader] Loading scene index: {sceneIndex}");
            SceneManager.LoadScene(sceneIndex);
        }

        /// <summary>
        /// 타이틀 씬으로 돌아갑니다.
        /// </summary>
        public void LoadTitleScene()
        {
            LoadScene(0); // Build Settings에서 0번 인덱스가 타이틀
        }

        /// <summary>
        /// 메인 게임 씬을 로드합니다.
        /// </summary>
        public void LoadMainScene()
        {
            LoadScene(1); // Build Settings에서 1번 인덱스가 메인
        }

        /// <summary>
        /// 현재 씬을 다시 로드합니다.
        /// </summary>
        public void ReloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[SceneLoader] Reloading scene: {currentScene}");
            SceneManager.LoadScene(currentScene);
        }

        /// <summary>
        /// 게임을 종료합니다.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[SceneLoader] Quitting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 페이드 효과와 함께 씬을 로드합니다 (향후 구현).
        /// </summary>
        public void LoadSceneWithFade(string sceneName, float fadeDuration = 1f)
        {
            StartCoroutine(LoadSceneWithFadeRoutine(sceneName, fadeDuration));
        }

        private IEnumerator LoadSceneWithFadeRoutine(string sceneName, float fadeDuration)
        {
            // TODO: 페이드 효과 구현 (향후)
            yield return new WaitForSeconds(fadeDuration * 0.5f);
            
            LoadScene(sceneName);
            
            yield return new WaitForSeconds(fadeDuration * 0.5f);
        }

        /// <summary>
        /// 배경음을 재생합니다 (페이드 인).
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="useFade">페이드 효과 사용 여부 (첫 로드 시 false)</param>
        private void PlayMusic(AudioClip clip, bool useFade = true)
        {
            if (clip == null)
            {
                // 파일이 할당되지 않았으면 재생하지 않음 (기존 동작 유지)
                return;
            }

            // 같은 음악이면 재생하지 않음
            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            if (useFade)
            {
                StartCoroutine(FadeMusic(clip));
            }
            else
            {
                // 첫 로드 시 즉시 재생 (페이드 없음)
                if (_musicSource.isPlaying)
                {
                    _musicSource.Stop();
                }
                _musicSource.clip = clip;
                _musicSource.volume = musicVolume;
                _musicSource.Play();
            }
        }

        /// <summary>
        /// 음악을 페이드 아웃 후 새 음악으로 전환합니다.
        /// </summary>
        private IEnumerator FadeMusic(AudioClip newClip)
        {
            // 현재 음악 페이드 아웃
            if (_musicSource.isPlaying)
            {
                float startVolume = _musicSource.volume;
                float elapsed = 0f;

                while (elapsed < musicFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
                    yield return null;
                }

                _musicSource.Stop();
            }

            // 새 음악 재생 (페이드 인)
            _musicSource.clip = newClip;
            _musicSource.volume = 0f;
            _musicSource.Play();

            float elapsed2 = 0f;
            while (elapsed2 < musicFadeDuration)
            {
                elapsed2 += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed2 / musicFadeDuration);
                yield return null;
            }

            _musicSource.volume = musicVolume;
        }

        /// <summary>
        /// BGM 볼륨 설정 (0~1). PlayerPrefs에 저장하며 즉시 적용합니다.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
                _musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(PrefsBGMVolume, musicVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 현재 BGM 볼륨 (0~1)을 반환합니다.
        /// </summary>
        public float GetMusicVolume()
        {
            return musicVolume;
        }

        /// <summary>
        /// 옵션 화면에서 볼륨 확인용으로, BGM이 재생 중이 아니면 타이틀 BGM을 잠깐 재생합니다.
        /// (Inspector에 Title Music이 할당되어 있어야 함)
        /// </summary>
        public void EnsureBGMPlayingForOptionsPreview()
        {
            if (_musicSource == null) return;
            if (_musicSource.isPlaying) return;
            if (titleMusic == null) return;
            _musicSource.clip = titleMusic;
            _musicSource.volume = musicVolume;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        /// <summary>
        /// 엔딩 음악을 재생합니다 (크레딧용).
        /// </summary>
        public void PlayEndingMusic(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            PlayMusic(clip);
        }

        /// <summary>
        /// 현재 재생 중인 배경음을 정지합니다 (페이드 아웃).
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                StartCoroutine(FadeOutMusic());
            }
        }

        /// <summary>
        /// 배경음을 페이드 아웃하며 정지합니다.
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            if (!_musicSource.isPlaying)
                yield break;

            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.volume = musicVolume; // 볼륨 초기화
            Debug.Log("[SceneLoader] Background music stopped");
        }

        /// <summary>
        /// 씬에 AudioListener가 정확히 하나만 있도록 보장합니다 (다음 프레임에 실행).
        /// </summary>
        private IEnumerator EnsureSingleAudioListenerNextFrame()
        {
            // 씬 오브젝트들이 완전히 깨어난 다음 프레임에 처리
            yield return null;
            EnsureSingleAudioListener();
        }

        /// <summary>
        /// 씬에 AudioListener가 정확히 하나만 있도록 보장합니다.
        /// </summary>
        private void EnsureSingleAudioListener()
        {
            AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>(true);
            
            if (listeners.Length <= 1)
                return;
            
            // 현재 활성 씬 확인
            Scene activeScene = SceneManager.GetActiveScene();
            bool isMiniGameSceneActive = activeScene.name == "FlourMiniGameScene" || activeScene.name == "SugarMiniGameScene";
            bool isTitleSceneActive = activeScene.name == "TitleScene";
            
            // Main Camera에 붙어있는 AudioListener를 우선적으로 유지
            AudioListener mainCameraListener = null;
            AudioListener firstListener = listeners[0];
            AudioListener miniGameListener = null;
            AudioListener titleSceneListener = null;
            
            foreach (var listener in listeners)
            {
                if (listener != null && listener.gameObject != null)
                {
                    // TitleScene의 AudioListener는 보호
                    if (listener.gameObject.scene.name == "TitleScene")
                    {
                        if (titleSceneListener == null)
                        {
                            titleSceneListener = listener;
                        }
                        continue;
                    }
                    
                    // 미니게임 씬의 AudioListener는 보호
                    if (isMiniGameSceneActive && 
                        (listener.gameObject.scene.name == "FlourMiniGameScene" || 
                         listener.gameObject.scene.name == "SugarMiniGameScene"))
                    {
                        if (miniGameListener == null)
                        {
                            miniGameListener = listener;
                        }
                        continue;
                    }
                    
                    UnityEngine.Camera cam = listener.GetComponent<UnityEngine.Camera>();
                    if (cam != null && cam.CompareTag("MainCamera"))
                    {
                        mainCameraListener = listener;
                        break;
                    }
                }
            }
            
            // TitleScene이 활성화되어 있으면 TitleScene의 AudioListener를 유지
            AudioListener listenerToKeep = null;
            if (isTitleSceneActive && titleSceneListener != null)
            {
                listenerToKeep = titleSceneListener;
                // TitleScene의 AudioListener 활성화
                if (!listenerToKeep.enabled)
                {
                    listenerToKeep.enabled = true;
                    Debug.Log($"[SceneLoader] TitleScene의 AudioListener 활성화: {listenerToKeep.gameObject.name}");
                }
            }
            else if (isMiniGameSceneActive && miniGameListener != null)
            {
                listenerToKeep = miniGameListener;
                // 미니게임 씬의 AudioListener 활성화
                if (!listenerToKeep.enabled)
                {
                    listenerToKeep.enabled = true;
                    Debug.Log($"[SceneLoader] 미니게임 씬의 AudioListener 활성화: {listenerToKeep.gameObject.name}");
                }
            }
            else
            {
                // Main Camera의 AudioListener가 있으면 그것을 유지, 없으면 첫 번째 것을 유지
                listenerToKeep = mainCameraListener != null ? mainCameraListener : firstListener;
            }
            
            // 나머지 AudioListener 제거 (TitleScene과 미니게임 씬의 것은 제외)
            int removedCount = 0;
            foreach (var listener in listeners)
            {
                if (listener != null && listener != listenerToKeep)
                {
                    // TitleScene의 AudioListener는 제거하지 않음
                    if (listener.gameObject.scene.name == "TitleScene")
                    {
                        // 비활성화만 함 (TitleScene이 활성화되어 있지 않으면)
                        if (!isTitleSceneActive)
                        {
                            listener.enabled = false;
                        }
                        continue;
                    }
                    
                    // 미니게임 씬의 AudioListener는 제거하지 않음
                    if (isMiniGameSceneActive && 
                        (listener.gameObject.scene.name == "FlourMiniGameScene" || 
                         listener.gameObject.scene.name == "SugarMiniGameScene"))
                    {
                        // 비활성화만 함
                        listener.enabled = false;
                        continue;
                    }
                    
                    Debug.Log($"[SceneLoader] Removing duplicate AudioListener from {listener.gameObject.name}");
                    Object.Destroy(listener);
                    removedCount++;
                }
            }
            
            if (removedCount > 0)
            {
                Debug.Log($"[SceneLoader] Removed {removedCount} duplicate AudioListener(s). {listeners.Length - removedCount} remaining.");
            }
        }

        /// <summary>
        /// 씬에 EventSystem이 정확히 하나만 있도록 보장합니다 (다음 프레임에 실행).
        /// </summary>
        private IEnumerator EnsureSingleEventSystemNextFrame()
        {
            // TitleScene의 경우 EnsureTitleSceneUI가 완료될 때까지 대기
            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                yield return new WaitForSeconds(0.15f); // EnsureTitleSceneUI가 완료될 시간 확보
            }
            else
            {
                // 씬 오브젝트들이 완전히 깨어난 다음 프레임에 처리
                yield return null;
            }
            EnsureSingleEventSystem();
        }

        /// <summary>
        /// 씬에 EventSystem이 정확히 하나만 있도록 보장합니다.
        /// </summary>
        private void EnsureSingleEventSystem()
        {
            EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>(true);
            
            if (eventSystems.Length <= 1)
                return;
            
            // 현재 활성 씬 확인
            Scene activeScene = SceneManager.GetActiveScene();
            
            // TitleScene의 EventSystem은 항상 보호
            if (activeScene.name == "TitleScene")
            {
                EventSystem titleEventSystem = null;
                foreach (var es in eventSystems)
                {
                    if (es != null && es.gameObject.scene == activeScene)
                    {
                        titleEventSystem = es;
                        break;
                    }
                }
                
                // TitleScene의 EventSystem이 있으면 다른 EventSystem만 제거
                if (titleEventSystem != null)
                {
                    int removedCount = 0;
                    foreach (var es in eventSystems)
                    {
                        if (es != null && es != titleEventSystem)
                        {
                            Debug.Log($"[SceneLoader] Removing duplicate EventSystem from {es.gameObject.name} (TitleScene EventSystem 보호)");
                            Object.Destroy(es.gameObject);
                            removedCount++;
                        }
                    }
                    
                    if (removedCount > 0)
                    {
                        Debug.Log($"[SceneLoader] Removed {removedCount} duplicate EventSystem(s). TitleScene EventSystem 보호됨.");
                    }
                }
                return;
            }
            
            // 활성 씬의 EventSystem을 우선적으로 유지
            EventSystem eventSystemToKeep = null;
            for (int i = 0; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != null && eventSystems[i].gameObject.scene == activeScene)
                {
                    eventSystemToKeep = eventSystems[i];
                    break;
                }
            }
            
            // 활성 씬의 EventSystem이 없으면 첫 번째를 유지
            if (eventSystemToKeep == null)
            {
                eventSystemToKeep = eventSystems[0];
            }
            
            // 나머지 EventSystem 제거
            int removedCount2 = 0;
            for (int i = 0; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != null && eventSystems[i] != eventSystemToKeep)
                {
                    Debug.Log($"[SceneLoader] Removing duplicate EventSystem from {eventSystems[i].gameObject.name}");
                    Object.Destroy(eventSystems[i].gameObject);
                    removedCount2++;
                }
            }
            
            if (removedCount2 > 0)
            {
                Debug.Log($"[SceneLoader] Removed {removedCount2} duplicate EventSystem(s). {eventSystems.Length - removedCount2} remaining.");
            }
        }
    }
}

