using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using BirthdayCakeQuest.Managers;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 타이틀 화면 UI를 관리합니다.
    /// </summary>
    public sealed class TitleScreenUI : MonoBehaviour
    {
        [Header("Buttons")]
        [Tooltip("게임 시작 버튼")]
        [SerializeField] private Button startButton;

        [Tooltip("게임 종료 버튼 (선택)")]
        [SerializeField] private Button quitButton;

        [Tooltip("크레딧 보기 버튼 (테스트용)")]
        [SerializeField] private Button creditButton;

        [Tooltip("설정(옵션) 버튼")]
        [SerializeField] private Button settingsButton;

        [Tooltip("옵션 패널 (GameOptionsUI가 붙은 GameObject). 설정 버튼 클릭 시 표시")]
        [SerializeField] private GameObject optionsPanel;

        [Header("Settings")]
        [Tooltip("시작 시 로드할 씬 이름")]
        [SerializeField] private string mainSceneName = "Home";
        [Header("UI Elements")]
        [Tooltip("타이틀 화면 UI 요소들 (게임 시작 시 숨김)")]
        [SerializeField] private GameObject titleText;

        private Coroutine _rehideRoutine;

        private void OnEnable()
        {
            // 타이틀 씬에서만: UBS_UI 등이 다음 프레임에 다시 Canvas를 켜는 경우가 있어서
            // 1프레임 뒤에 한 번 더 강제로 숨기는 방어 로직을 실행
            if (Application.isPlaying && SceneManager.GetActiveScene().name == "TitleScene")
            {
                if (_rehideRoutine != null)
                    StopCoroutine(_rehideRoutine);

                _rehideRoutine = StartCoroutine(RehideAfterFrame());
            }
        }

        private void OnDisable()
        {
            if (_rehideRoutine != null)
            {
                StopCoroutine(_rehideRoutine);
                _rehideRoutine = null;
            }
        }

        private void Start()
        {
            // 타이틀 씬에서는 타이틀 UI를 표시해야 함
            if (Application.isPlaying)
            {
                // 버튼 이벤트 즉시 재연결 (확실하게)
                if (startButton != null)
                {
                    startButton.onClick.RemoveListener(OnStartButtonClicked);
                    startButton.onClick.AddListener(OnStartButtonClicked);
                }
                
                if (creditButton != null)
                {
                    creditButton.onClick.RemoveListener(OnCreditButtonClicked);
                    creditButton.onClick.AddListener(OnCreditButtonClicked);
                }
                if (settingsButton != null)
                {
                    settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
                    settingsButton.onClick.AddListener(OnSettingsButtonClicked);
                }
                
                // 즉시 타이틀 UI 표시 (버튼 활성화 지연 최소화)
                ShowTitleUI();
                
                // TitleScene에 있는 게임 씬 오브젝트들 비활성화
                HideGameSceneObjects();
                
                // Canvas와 GraphicRaycaster 즉시 확인
                EnsureCanvasAndRaycaster();
                
                // EventSystem 즉시 확인 (코루틴 전에)
                EnsureEventSystemExists();
                
                // 버튼 상태 확인 (비동기로 실행하여 Start 지연 최소화)
                StartCoroutine(CheckButtonStateAfterFrame());
            }
        }
        
        /// <summary>
        /// Canvas와 GraphicRaycaster를 확인하고 활성화합니다.
        /// </summary>
        private void EnsureCanvasAndRaycaster()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            
            if (canvas != null)
            {
                if (!canvas.gameObject.activeSelf)
                {
                    canvas.gameObject.SetActive(true);
                }
                if (!canvas.enabled)
                {
                    canvas.enabled = true;
                }
                
                // GraphicRaycaster 확인
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                if (!raycaster.enabled)
                {
                    raycaster.enabled = true;
                }
            }
        }
        
        /// <summary>
        /// EventSystem이 존재하는지 확인하고 활성화합니다. (생성은 SceneLoader에 맡김)
        /// </summary>
        private void EnsureEventSystemExists()
        {
            // TitleScene에서 EventSystem 찾기
            EventSystem[] allEventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>(true);
            EventSystem titleEventSystem = null;
            foreach (var es in allEventSystems)
            {
                if (es != null && es.gameObject.scene.name == "TitleScene")
                {
                    titleEventSystem = es;
                    break;
                }
            }
            
            if (titleEventSystem != null)
            {
                EventSystem.current = titleEventSystem;
                if (!titleEventSystem.gameObject.activeSelf)
                    titleEventSystem.gameObject.SetActive(true);
                if (!titleEventSystem.enabled)
                    titleEventSystem.enabled = true;
            }
        }
        
        /// <summary>
        /// EventSystem을 찾을 때까지 재시도합니다.
        /// </summary>
        private System.Collections.IEnumerator EnsureEventSystemWithRetry()
        {
            // EventSystem이 이미 있으면 즉시 종료
            if (EventSystem.current != null)
            {
                yield break;
            }

            int retryCount = 0;
            const int maxRetries = 10; // 최대 10번 재시도 (약 0.5초, 지연 최소화)
            
            while (retryCount < maxRetries)
            {
                // TitleScene에서 EventSystem 찾기
                EventSystem[] allEventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>(true);
                EventSystem titleEventSystem = null;
                foreach (var es in allEventSystems)
                {
                    if (es != null && es.gameObject.scene.name == "TitleScene")
                    {
                        titleEventSystem = es;
                        break;
                    }
                }
                
                if (titleEventSystem != null)
                {
                    // EventSystem.current 설정
                    EventSystem.current = titleEventSystem;
                    
                    if (!titleEventSystem.gameObject.activeSelf)
                        titleEventSystem.gameObject.SetActive(true);
                    if (!titleEventSystem.enabled)
                        titleEventSystem.enabled = true;
                    
                    // EventSystem이 설정되었으므로 버튼 이벤트도 다시 확인
                    if (startButton != null)
                    {
                        startButton.onClick.RemoveListener(OnStartButtonClicked);
                        startButton.onClick.AddListener(OnStartButtonClicked);
                    }
                    
                    yield break;
                }
                
                retryCount++;
                yield return new WaitForSeconds(0.05f); // 0.05초마다 재시도
            }
            
            // 최종 시도: EventSystem이 없으면 생성
            GameObject eventSystemObj = new GameObject("EventSystem");
            EventSystem newEventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(eventSystemObj, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EventSystem.current = newEventSystem;
            
            // 버튼 이벤트 재연결
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }

        /// <summary>
        /// 버튼 상태를 확인합니다.
        /// </summary>
        private System.Collections.IEnumerator CheckButtonStateAfterFrame()
        {
            // 버튼은 이미 ShowTitleUI()에서 활성화되었으므로, EventSystem만 확인
            yield return null; // 1프레임만 대기 (지연 최소화)

            if (startButton != null)
            {
                // 버튼이 비활성화되어 있으면 활성화 (방어 로직)
                if (!startButton.gameObject.activeSelf)
                {
                    startButton.gameObject.SetActive(true);
                }
                if (!startButton.enabled)
                {
                    startButton.enabled = true;
                }
                if (!startButton.interactable)
                {
                    startButton.interactable = true;
                }
                
                // EventSystem 확인 (빠른 확인)
                if (EventSystem.current == null)
                {
                    // EventSystem 다시 찾기
                    EventSystem[] allEventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>(true);
                    foreach (var es in allEventSystems)
                    {
                        if (es != null && es.gameObject.scene.name == "TitleScene")
                        {
                            EventSystem.current = es;
                            break;
                        }
                    }
                }
                
                // 버튼의 Image 컴포넌트 Raycast Target 확인
                var buttonImage = startButton.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null && !buttonImage.raycastTarget)
                {
                    buttonImage.raycastTarget = true;
                }
            }

            // EventSystem 확인 및 활성화 (재시도 포함, 하지만 빠르게)
            yield return StartCoroutine(EnsureEventSystemWithRetry());

            // Canvas 확인 및 GraphicRaycaster 확인
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            
            if (canvas != null)
            {
                // GraphicRaycaster 확인
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                else if (!raycaster.enabled)
                {
                    raycaster.enabled = true;
                }
            }
        }

        private System.Collections.IEnumerator RehideAfterFrame()
        {
            // UBS_UI.Start() 등이 실행된 이후에 한 번 더 숨김 처리
            yield return null;
            HideGameSceneObjects();

            // 한 번 더(EndOfFrame) - 일부 UI가 렌더 직전에 켜지는 케이스 방어
            yield return new WaitForEndOfFrame();
            HideGameSceneObjects();

            _rehideRoutine = null;
        }

        /// <summary>
        /// TitleScene에 있는 게임 씬 오브젝트들을 비활성화합니다.
        /// </summary>
        private void HideGameSceneObjects()
        {
            // 안전장치: 타이틀 씬에서만 실행
            if (SceneManager.GetActiveScene().name != "TitleScene")
                return;

            // CreditsUI 비활성화
            GameObject creditsUI = GameObject.Find("CreditsUI");
            if (creditsUI != null)
            {
                creditsUI.SetActive(false);
            }

            // EndingCutscene 비활성화
            GameObject endingCutscene = GameObject.Find("EndingCutscene");
            if (endingCutscene != null)
            {
                endingCutscene.SetActive(false);
            }

            // GameSystems 비활성화
            GameObject gameSystems = GameObject.Find("GameSystems");
            if (gameSystems != null)
            {
                gameSystems.SetActive(false);
            }

            // Stand to Sit[Timeline] 비활성화 (엔딩 컷씬용 Timeline)
            GameObject standToSitTimeline = GameObject.Find("Stand to Sit [Timeline]");
            if (standToSitTimeline == null)
            {
                standToSitTimeline = GameObject.Find("Stand to Sit");
            }
            if (standToSitTimeline != null)
            {
                standToSitTimeline.SetActive(false);
            }

            // UBS Environment / UBS UI 비활성화 (타이틀에서 안내문/오버레이 뜨는 것 방지)
            HideUBSUI();
        }

        /// <summary>
        /// TitleScene에서 UBS(Environment) 관련 UI가 남아서 Game 탭에 보이는 문제를 방지합니다.
        /// (DontDestroyOnLoad로 남아있는 오브젝트 포함)
        /// </summary>
        private void HideUBSUI()
        {
            // 1) "UBS Environment" 루트가 있으면 통째로 비활성화 (가장 확실)
            var ubsEnv = GameObject.Find("UBS Environment");
            if (ubsEnv != null)
            {
                // 먼저 하위 Canvas들을 강제로 끔 (SetActive(false)로도 충분하지만,
                // 외부에서 다시 켜는 경우를 대비)
                var envCanvases = ubsEnv.GetComponentsInChildren<Canvas>(true);
                foreach (var c in envCanvases)
                {
                    if (c == null) continue;
                    c.enabled = false;
                    if (c.gameObject.activeSelf)
                        c.gameObject.SetActive(false);
                }

                ubsEnv.SetActive(false);
            }

            // 2) UBS_UI 컴포넌트가 들고 있는 Canvas가 있으면 비활성화
            // UBS_UI는 글로벌 네임스페이스에 있음 (Assets/UBS/Assets/Scripts/UBS_UI.cs)
            var ubsUIs = FindObjectsOfType<UBS_UI>(true);
            foreach (var ui in ubsUIs)
            {
                if (ui == null) continue;

                // 타이틀 Canvas(본인)와 같은 오브젝트면 건드리지 않음
                if (ui.gameObject == gameObject) continue;

                if (ui.canvas != null)
                    ui.canvas.enabled = false;

                ui.enabled = false;
                if (ui.gameObject.activeSelf)
                    ui.gameObject.SetActive(false);
            }

            // 3) 이름에 UBS가 포함된 Canvas들도 방어적으로 끄기
            // (UBS_UI가 참조를 못 잡았거나, 다른 구조로 남아있는 경우)
            var canvases = FindObjectsOfType<Canvas>(true);
            foreach (var c in canvases)
            {
                if (c == null) continue;

                // 타이틀 Canvas 자체는 유지
                if (c.gameObject == gameObject) continue;
                if (c.transform.IsChildOf(transform)) continue;

                bool looksLikeUBS =
                    (c.gameObject.name != null && c.gameObject.name.Contains("UBS")) ||
                    (c.transform.root != null && c.transform.root.name != null && c.transform.root.name.Contains("UBS"));

                if (looksLikeUBS)
                {
                    c.enabled = false;
                    if (c.gameObject.activeSelf)
                        c.gameObject.SetActive(false);
                }
            }
        }

        public void ShowTitleUI()
        {
            if (titleText != null) titleText.SetActive(true);
            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
                startButton.enabled = true;
                startButton.interactable = true;
            }
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(true);
                quitButton.enabled = true;
                quitButton.interactable = true;
            }
            if (creditButton != null)
            {
                creditButton.gameObject.SetActive(true);
                creditButton.enabled = true;
                creditButton.interactable = true;
            }
            if (settingsButton != null)
            {
                settingsButton.gameObject.SetActive(true);
                settingsButton.enabled = true;
                settingsButton.interactable = true;
            }
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        public void HideTitleUI()
        {
            if (titleText != null) titleText.SetActive(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
            if (quitButton != null) quitButton.gameObject.SetActive(false);
            if (creditButton != null) creditButton.gameObject.SetActive(false);
            if (settingsButton != null) settingsButton.gameObject.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void Awake()
        {
            // 버튼 이벤트 연결
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            if (creditButton != null)
            {
                creditButton.onClick.AddListener(OnCreditButtonClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }

            if (creditButton != null)
            {
                creditButton.onClick.RemoveListener(OnCreditButtonClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }
        }

        private void OnSettingsButtonClicked()
        {
            if (optionsPanel != null)
            {
                var optionsUI = optionsPanel.GetComponent<GameOptionsUI>();
                if (optionsUI != null)
                    optionsUI.Show();
                else
                    optionsPanel.SetActive(true);
            }
        }

        private void OnStartButtonClicked()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(mainSceneName);
            }
        }

        private void OnQuitButtonClicked()
        {
            SceneLoader.Instance.QuitGame();
        }

        /// <summary>
        /// 크레딧 보기 버튼 클릭 (테스트용).
        /// </summary>
        private void OnCreditButtonClicked()
        {
            Debug.Log("[TitleScreenUI] Credit 버튼 클릭 - 크레딧 표시");

            // 타이틀 UI 숨기기
            HideTitleUI();

            // CreditsUI 찾기 (모든 씬에서)
            GameObject creditsUI = null;

            // 1. TitleScene에서 찾기
            creditsUI = GameObject.Find("CreditsUI");
            
            // 2. DontDestroyOnLoad 오브젝트에서 찾기
            if (creditsUI == null)
            {
                CreditsLetterPlayer[] allPlayers = Resources.FindObjectsOfTypeAll<CreditsLetterPlayer>();
                foreach (var player in allPlayers)
                {
                    if (player != null && player.gameObject != null)
                    {
                        creditsUI = player.transform.root.gameObject;
                        if (creditsUI.name == "CreditsUI" || creditsUI.name.Contains("Credits"))
                        {
                            break;
                        }
                    }
                }
            }

            // 3. HomeScene에서 찾기 (씬이 로드되어 있다면)
            if (creditsUI == null)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.name == "Home" && scene.isLoaded)
                    {
                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        foreach (var obj in rootObjects)
                        {
                            if (obj.name == "CreditsUI")
                            {
                                creditsUI = obj;
                                break;
                            }
                        }
                        if (creditsUI != null) break;
                    }
                }
            }

            if (creditsUI != null)
            {
                creditsUI.SetActive(true);
                Debug.Log($"[TitleScreenUI] CreditsUI 활성화: {creditsUI.name}, 씬: {creditsUI.scene.name}");

                // CreditsUI의 모든 자식 오브젝트도 활성화
                foreach (Transform child in creditsUI.transform)
                {
                    if (!child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(true);
                        Debug.Log($"[TitleScreenUI] CreditsUI 자식 활성화: {child.name}");
                    }
                }

                // 한 프레임 대기 후 컴포넌트 찾기 (활성화 후 초기화 시간 필요)
                StartCoroutine(StartCreditsAfterDelay(creditsUI));
            }
            else
            {
                Debug.LogError("[TitleScreenUI] CreditsUI를 찾을 수 없습니다! HomeScene에 CreditsUI가 있는지 확인하세요.");
            }
        }

        /// <summary>
        /// CreditsUI 활성화 후 크레딧을 시작합니다.
        /// </summary>
        private System.Collections.IEnumerator StartCreditsAfterDelay(GameObject creditsUI)
        {
            yield return null; // 한 프레임 대기

            // CreditsLetterPlayer 찾기 및 시작
            var letterPlayer = creditsUI.GetComponentInChildren<CreditsLetterPlayer>(true);
            if (letterPlayer != null)
            {
                // 컴포넌트가 비활성화되어 있으면 활성화
                if (!letterPlayer.enabled)
                {
                    letterPlayer.enabled = true;
                    Debug.Log("[TitleScreenUI] CreditsLetterPlayer 활성화");
                }
                
                // GameObject도 활성화
                if (!letterPlayer.gameObject.activeSelf)
                {
                    letterPlayer.gameObject.SetActive(true);
                }

                // Slides 확인
                letterPlayer.GetSlideCounts(out int letterCount, out int lyricsCount);
                Debug.Log($"[TitleScreenUI] CreditsLetterPlayer - 편지 슬라이드: {letterCount}개, 가사 슬라이드: {lyricsCount}개");
                
                if (!letterPlayer.HasSlides())
                {
                    Debug.LogWarning("[TitleScreenUI] CreditsLetterPlayer에 슬라이드가 없습니다! Inspector에서 Letter Slides와 Lyrics Slides를 설정하세요.");
                    Debug.LogWarning("[TitleScreenUI] CreditsUI의 CreditsLetterPlayer 컴포넌트를 확인하고, Letter Slides와 Lyrics Slides에 데이터를 추가하세요.");
                }
                else
                {
                    letterPlayer.StartCredits();
                    Debug.Log("[TitleScreenUI] CreditsLetterPlayer 시작");
                }
                yield break;
            }
            else
            {
                Debug.Log("[TitleScreenUI] CreditsLetterPlayer를 찾지 못함, CreditsSlidePlayer 시도...");
            }

            // CreditsSlidePlayer 찾기 및 시작
            var slidePlayer = creditsUI.GetComponentInChildren<CreditsSlidePlayer>(true);
            if (slidePlayer != null)
            {
                // 컴포넌트가 비활성화되어 있으면 활성화
                if (!slidePlayer.enabled)
                {
                    slidePlayer.enabled = true;
                    Debug.Log("[TitleScreenUI] CreditsSlidePlayer 활성화");
                }
                
                // GameObject도 활성화
                if (!slidePlayer.gameObject.activeSelf)
                {
                    slidePlayer.gameObject.SetActive(true);
                }

                // Slides 확인
                int slideCount = slidePlayer.GetSlideCount();
                Debug.Log($"[TitleScreenUI] CreditsSlidePlayer - 슬라이드: {slideCount}개");
                
                if (!slidePlayer.HasSlides())
                {
                    Debug.LogWarning("[TitleScreenUI] CreditsSlidePlayer에 슬라이드가 없습니다! Inspector에서 Slides를 설정하세요.");
                    Debug.LogWarning("[TitleScreenUI] CreditsUI의 CreditsSlidePlayer 컴포넌트를 확인하고, Slides에 데이터를 추가하세요.");
                }
                else
                {
                    slidePlayer.StartSlides();
                    Debug.Log("[TitleScreenUI] CreditsSlidePlayer 시작");
                }
                yield break;
            }

            // 둘 다 찾지 못한 경우
            Debug.LogWarning("[TitleScreenUI] CreditsLetterPlayer 또는 CreditsSlidePlayer를 찾을 수 없습니다!");
            Debug.LogWarning($"[TitleScreenUI] CreditsUI 자식 오브젝트 확인:");
            foreach (Transform child in creditsUI.transform)
            {
                Debug.LogWarning($"[TitleScreenUI]   - {child.name} (활성화: {child.gameObject.activeSelf})");
            }
        }

        /// <summary>
        /// Enter 키로도 시작할 수 있도록 합니다.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnStartButtonClicked();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnQuitButtonClicked();
            }
        }
    }
}

