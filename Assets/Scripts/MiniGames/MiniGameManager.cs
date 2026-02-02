using System;
using UnityEngine;
using BirthdayCakeQuest.Player;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 미니게임의 시작/종료를 관리하는 싱글톤 매니저입니다.
    /// 플레이어 입력, 카메라를 일시정지하고 미니게임 UI를 표시합니다.
    /// </summary>
    public class MiniGameManager : MonoBehaviour
    {
        private static MiniGameManager _instance;
        public static MiniGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("MiniGameManager");
                    _instance = go.AddComponent<MiniGameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("References")]
        [SerializeField] private Canvas miniGameCanvas;
        [SerializeField] private GameObject sugarMiniGamePanel;

        [Header("Game Objects")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Interactor interactor;
        [SerializeField] private Camera.IsometricFollowCamera isometricCamera;

        private IMiniGame _currentMiniGame;
        private Action<bool> _currentCallback;
        private bool _isPlaying;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MiniGameManager] Instance created and initialized");
        }

        private void Start()
        {
            // 자동으로 참조 찾기
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                    Debug.Log("[MiniGameManager] PlayerController 자동 연결 완료");
                else
                    Debug.LogWarning("[MiniGameManager] PlayerController를 찾을 수 없습니다!");
            }

            if (interactor == null)
            {
                interactor = FindObjectOfType<Interactor>();
                if (interactor != null)
                    Debug.Log("[MiniGameManager] Interactor 자동 연결 완료");
                else
                    Debug.LogWarning("[MiniGameManager] Interactor를 찾을 수 없습니다!");
            }

            if (isometricCamera == null)
            {
                isometricCamera = FindObjectOfType<Camera.IsometricFollowCamera>();
                if (isometricCamera != null)
                    Debug.Log("[MiniGameManager] IsometricFollowCamera 자동 연결 완료");
                else
                    Debug.LogWarning("[MiniGameManager] IsometricFollowCamera를 찾을 수 없습니다!");
            }

            // Canvas 초기 비활성화
            if (miniGameCanvas != null)
            {
                miniGameCanvas.gameObject.SetActive(false);
                Debug.Log("[MiniGameManager] MiniGame Canvas 초기 비활성화 완료");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] MiniGame Canvas가 할당되지 않았습니다!");
            }

            // SugarMiniGamePanel 확인
            if (sugarMiniGamePanel != null)
            {
                Debug.Log("[MiniGameManager] SugarMiniGamePanel 할당 확인됨");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] SugarMiniGamePanel이 할당되지 않았습니다!");
            }
        }

        /// <summary>
        /// 미니게임을 시작합니다.
        /// </summary>
        /// <param name="type">미니게임 타입</param>
        /// <param name="onComplete">완료 콜백 (성공: true, 실패: false)</param>
        public void StartMiniGame(MiniGameType type, Action<bool> onComplete)
        {
            if (_isPlaying)
            {
                Debug.LogWarning("[MiniGameManager] 이미 미니게임이 진행 중입니다!");
                return;
            }

            Debug.Log($"[MiniGameManager] {type} 미니게임 시작");

            _isPlaying = true;
            _currentCallback = onComplete;

            // 플레이어 및 카메라 일시정지
            PauseGameplay(true);

            // 미니게임 생성
            _currentMiniGame = CreateMiniGame(type);

            if (_currentMiniGame == null)
            {
                Debug.LogError($"[MiniGameManager] {type} 미니게임을 생성할 수 없습니다!");
                EndMiniGame(false);
                return;
            }

            // Canvas 표시
            if (miniGameCanvas != null)
            {
                miniGameCanvas.gameObject.SetActive(true);
            }

            // 미니게임 초기화 및 시작
            _currentMiniGame.Initialize(OnMiniGameComplete);
            _currentMiniGame.StartGame();
        }

        /// <summary>
        /// 미니게임을 종료합니다.
        /// </summary>
        /// <param name="success">성공 여부</param>
        public void EndMiniGame(bool success)
        {
            if (!_isPlaying)
                return;

            Debug.Log($"[MiniGameManager] 미니게임 종료 - {(success ? "성공" : "실패")}");

            _isPlaying = false;

            // 미니게임 정리
            if (_currentMiniGame != null)
            {
                _currentMiniGame.CleanUp();
                _currentMiniGame = null;
            }

            // Canvas 숨기기
            if (miniGameCanvas != null)
            {
                miniGameCanvas.gameObject.SetActive(false);
            }

            // 플레이어 및 카메라 재개
            PauseGameplay(false);

            // 콜백 호출
            _currentCallback?.Invoke(success);
            _currentCallback = null;
        }

        /// <summary>
        /// 미니게임 완료 시 호출되는 콜백
        /// </summary>
        private void OnMiniGameComplete(bool success)
        {
            // 약간의 지연 후 종료 (결과 표시를 위해)
            if (success)
            {
                Invoke(nameof(DelayedEndSuccess), 1.5f);
            }
            else
            {
                // 실패 시 즉시 종료하지 않음 (다시 시도 버튼 표시)
                // EndMiniGame은 RetryButton 또는 UI에서 호출
            }
        }

        private void DelayedEndSuccess()
        {
            EndMiniGame(true);
        }

        /// <summary>
        /// 게임플레이를 일시정지/재개합니다.
        /// </summary>
        private void PauseGameplay(bool pause)
        {
            Debug.Log($"[MiniGameManager] PauseGameplay({pause})");

            if (playerController != null)
            {
                playerController.SetPaused(pause);
                Debug.Log($"[MiniGameManager] PlayerController paused: {pause}");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] PlayerController is null!");
            }

            if (interactor != null)
            {
                interactor.SetPaused(pause);
                Debug.Log($"[MiniGameManager] Interactor paused: {pause}");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] Interactor is null!");
            }

            if (isometricCamera != null)
            {
                isometricCamera.SetPaused(pause);
                Debug.Log($"[MiniGameManager] Camera paused: {pause}");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] IsometricCamera is null!");
            }

            // 커서 표시 설정
            Cursor.visible = pause;
            Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
            Debug.Log($"[MiniGameManager] Cursor visible: {pause}");
        }

        /// <summary>
        /// 타입에 맞는 미니게임 인스턴스를 생성합니다.
        /// </summary>
        private IMiniGame CreateMiniGame(MiniGameType type)
        {
            switch (type)
            {
                case MiniGameType.Sugar:
                    if (sugarMiniGamePanel != null)
                    {
                        var sugarGame = sugarMiniGamePanel.GetComponent<SugarPouringMiniGame>();
                        if (sugarGame == null)
                        {
                            Debug.LogError("[MiniGameManager] SugarMiniGamePanel에 SugarPouringMiniGame 컴포넌트가 없습니다!");
                            return null;
                        }
                        return sugarGame;
                    }
                    Debug.LogError("[MiniGameManager] SugarMiniGamePanel이 할당되지 않았습니다!");
                    return null;

                case MiniGameType.Egg:
                case MiniGameType.Flour:
                case MiniGameType.Butter:
                case MiniGameType.Strawberry:
                    Debug.LogWarning($"[MiniGameManager] {type} 미니게임은 아직 구현되지 않았습니다.");
                    return null;

                default:
                    Debug.LogError($"[MiniGameManager] 알 수 없는 미니게임 타입: {type}");
                    return null;
            }
        }

        /// <summary>
        /// 외부에서 참조를 설정할 수 있도록 합니다.
        /// </summary>
        public void SetReferences(Canvas canvas, GameObject sugarPanel)
        {
            miniGameCanvas = canvas;
            sugarMiniGamePanel = sugarPanel;
        }

        /// <summary>
        /// 현재 미니게임이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsPlaying => _isPlaying;
    }
}

