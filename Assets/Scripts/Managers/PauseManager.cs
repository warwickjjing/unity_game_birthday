using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BirthdayCakeQuest.Player;
using BirthdayCakeQuest.Managers;
using BirthdayCakeQuest.UI;

namespace BirthdayCakeQuest.Managers
{
    /// <summary>
    /// Home 씬에서 Esc로 일시정지 메뉴를 열고, 재개/설정/타이틀로 버튼을 제공합니다.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Pause UI")]
        [Tooltip("일시정지 시 표시할 패널 (재개/설정/타이틀 버튼 포함)")]
        [SerializeField] private GameObject pausePanel;

        [Tooltip("일시정지 패널의 재개 버튼")]
        [SerializeField] private Button resumeButton;

        [Tooltip("일시정지 패널의 설정 버튼 (옵션 패널 표시)")]
        [SerializeField] private Button settingsButton;

        [Tooltip("일시정지 패널의 타이틀로 버튼")]
        [SerializeField] private Button titleButton;

        [Header("Options")]
        [Tooltip("설정 패널 (GameOptionsUI). 설정 버튼 클릭 시 표시")]
        [SerializeField] private GameObject optionsPanel;

        private bool _isPaused;
        private PlayerController _player;

        private void Awake()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (optionsPanel != null)
                optionsPanel.SetActive(false);

            EnsureButtonReferences();
            BindPauseButtons();
        }

        /// <summary>
        /// Inspector에서 버튼이 비어 있으면 패널 자식에서 이름으로 찾습니다.
        /// </summary>
        private void EnsureButtonReferences()
        {
            if (pausePanel == null) return;

            if (resumeButton == null || settingsButton == null || titleButton == null)
            {
                Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn == null) continue;
                    string name = btn.gameObject.name.ToLowerInvariant();
                    if (resumeButton == null && (name.Contains("resume") || name.Contains("return") || name.Contains("재개") || name.Contains("닫기") || name.Contains("게임으로")))
                        resumeButton = btn;
                    else if (settingsButton == null && (name.Contains("setting") || name.Contains("설정") || name.Contains("option")))
                        settingsButton = btn;
                    else if (titleButton == null && (name.Contains("title") || name.Contains("타이틀") || name.Contains("메인으로") || name.Contains("메인")))
                        titleButton = btn;
                }
                // 이름으로 못 찾으면 순서대로: 첫 번째=재개, 두 번째=설정, 세 번째=타이틀
                if (buttons.Length >= 1 && resumeButton == null) resumeButton = buttons[0];
                if (buttons.Length >= 2 && settingsButton == null) settingsButton = buttons[1];
                if (buttons.Length >= 3 && titleButton == null) titleButton = buttons[2];
            }
        }

        /// <summary>
        /// 버튼 클릭 리스너를 연결합니다. 패널을 열 때마다 호출해도 안전합니다.
        /// </summary>
        private void BindPauseButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(Resume);
                resumeButton.interactable = true;
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OpenSettings);
                settingsButton.interactable = true;
            }
            if (titleButton != null)
            {
                titleButton.onClick.RemoveAllListeners();
                titleButton.onClick.AddListener(GoToTitle);
                titleButton.interactable = true;
            }
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "Home")
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isPaused)
                {
                    if (optionsPanel != null && optionsPanel.activeSelf)
                    {
                        var ui = optionsPanel.GetComponent<GameOptionsUI>();
                        if (ui != null) ui.Hide();
                        else optionsPanel.SetActive(false);
                    }
                    else
                        Resume();
                }
                else
                    Pause();
            }
        }

        /// <summary>
        /// 일시정지합니다.
        /// </summary>
        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;
            Time.timeScale = 0f;

            if (_player == null)
                _player = FindObjectOfType<PlayerController>();
            if (_player != null)
                _player.SetPaused(true);

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                BindPauseButtons();
                Canvas canvas = pausePanel.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.sortingOrder < 1000)
                    canvas.sortingOrder = 1000;
                // 대사창 등 다른 UI가 클릭을 가로채지 않도록 패널/캔버스에서 레이캐스트 막기
                var canvasGroup = pausePanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = pausePanel.GetComponentInParent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
            }
        }

        /// <summary>
        /// 재개합니다.
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = 1f;

            if (_player != null)
                _player.SetPaused(false);

            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (optionsPanel != null)
            {
                var ui = optionsPanel.GetComponent<GameOptionsUI>();
                if (ui != null)
                    ui.Hide();
                else
                    optionsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 설정(옵션) 패널을 엽니다.
        /// </summary>
        public void OpenSettings()
        {
            if (optionsPanel == null) return;
            var ui = optionsPanel.GetComponent<GameOptionsUI>();
            if (ui != null)
                ui.Show();
            else
                optionsPanel.SetActive(true);
        }

        /// <summary>
        /// 타이틀(메인) 씬으로 이동합니다.
        /// </summary>
        public void GoToTitle()
        {
            Time.timeScale = 1f;
            _isPaused = false;
            if (_player != null)
                _player.SetPaused(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionsPanel != null)
            {
                var ui = optionsPanel.GetComponent<GameOptionsUI>();
                if (ui != null) ui.Hide();
                optionsPanel.SetActive(false);
            }

            var loader = SceneLoader.Instance;
            if (loader != null)
                loader.LoadTitleScene();
            else
                SceneManager.LoadScene("TitleScene");
        }
    }
}
