using UnityEngine;
using UnityEngine.UI;
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

        [Header("Settings")]
        [Tooltip("시작 시 로드할 씬 이름")]
        [SerializeField] private string mainSceneName = "Home";

        private void Awake()
        {
            // 버튼 이벤트 연결
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            else
            {
                Debug.LogWarning("[TitleScreenUI] Start button is not assigned!");
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
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
        }

        private void OnStartButtonClicked()
        {
            Debug.Log("[TitleScreenUI] Start button clicked!");
            SceneLoader.Instance.LoadScene(mainSceneName);
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("[TitleScreenUI] Quit button clicked!");
            SceneLoader.Instance.QuitGame();
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

