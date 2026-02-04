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
        [SerializeField] private GameObject eggMiniGamePanel;
        [SerializeField] private GameObject flourMiniGamePanel;
        [SerializeField] private GameObject butterMiniGamePanel;
        [SerializeField] private GameObject strawberryMiniGamePanel;

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

            // Canvas가 없으면 자동 생성
            if (miniGameCanvas == null)
            {
                miniGameCanvas = MiniGameUIFactory.CreateMiniGameCanvas();
                Debug.Log("[MiniGameManager] Canvas 자동 생성 완료");
            }

            // SugarMiniGamePanel이 없으면 자동 생성
            if (sugarMiniGamePanel == null)
            {
                if (miniGameCanvas != null)
                {
                    sugarMiniGamePanel = MiniGameUIFactory.CreateSugarMiniGamePanel(miniGameCanvas);
                    // SugarPouringMiniGame 컴포넌트 설정
                    MiniGameUIFactory.SetupSugarMiniGameComponent(sugarMiniGamePanel);
                    Debug.Log("[MiniGameManager] SugarMiniGamePanel 자동 생성 완료");
                }
                else
                {
                    Debug.LogError("[MiniGameManager] Canvas가 없어 SugarMiniGamePanel을 생성할 수 없습니다!");
                }
            }
            else
            {
                // 기존 Panel에 컴포넌트가 없으면 추가
                if (sugarMiniGamePanel.GetComponent<SugarPouringMiniGame>() == null)
                {
                    MiniGameUIFactory.SetupSugarMiniGameComponent(sugarMiniGamePanel);
                    Debug.Log("[MiniGameManager] SugarPouringMiniGame 컴포넌트 자동 추가 완료");
                }
            }

            // 다른 미니게임 Panel들 자동 생성
            if (miniGameCanvas != null)
            {
                if (eggMiniGamePanel == null)
                {
                    eggMiniGamePanel = MiniGameUIFactory.CreateEggMiniGamePanel(miniGameCanvas);
                    eggMiniGamePanel.SetActive(false); // 초기 비활성화
                    Debug.Log("[MiniGameManager] EggMiniGamePanel 자동 생성 완료");
                }

                if (flourMiniGamePanel == null)
                {
                    flourMiniGamePanel = MiniGameUIFactory.CreateFlourMiniGamePanel(miniGameCanvas);
                    flourMiniGamePanel.SetActive(false); // 초기 비활성화
                    Debug.Log("[MiniGameManager] FlourMiniGamePanel 자동 생성 완료");
                }

                if (butterMiniGamePanel == null)
                {
                    butterMiniGamePanel = MiniGameUIFactory.CreateSimpleMiniGamePanel(miniGameCanvas, "ButterMiniGamePanel", "냉장고 미로", "버터를 찾아보세요!");
                    butterMiniGamePanel.SetActive(false); // 초기 비활성화
                    Debug.Log("[MiniGameManager] ButterMiniGamePanel 자동 생성 완료");
                }

                if (strawberryMiniGamePanel == null)
                {
                    strawberryMiniGamePanel = MiniGameUIFactory.CreateStrawberryMiniGamePanel(miniGameCanvas);
                    strawberryMiniGamePanel.SetActive(false); // 초기 비활성화
                    Debug.Log("[MiniGameManager] StrawberryMiniGamePanel 자동 생성 완료");
                }
            }
            
            // SugarMiniGamePanel도 초기 비활성화
            if (sugarMiniGamePanel != null)
            {
                sugarMiniGamePanel.SetActive(false);
            }

            // Canvas 초기 비활성화
            if (miniGameCanvas != null)
            {
                miniGameCanvas.gameObject.SetActive(false);
                Debug.Log("[MiniGameManager] MiniGame Canvas 초기 비활성화 완료");
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

            // 모든 패널 먼저 비활성화
            DeactivateAllPanels();

            // 미니게임 생성
            _currentMiniGame = CreateMiniGame(type);

            if (_currentMiniGame == null)
            {
                Debug.LogError($"[MiniGameManager] {type} 미니게임을 생성할 수 없습니다!");
                EndMiniGame(false);
                return;
            }

            // 현재 미니게임 패널만 활성화
            GameObject currentPanel = GetPanelForType(type);
            if (currentPanel != null)
            {
                currentPanel.SetActive(true);
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

            // 모든 패널 비활성화
            DeactivateAllPanels();

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
        /// 모든 미니게임 패널을 비활성화합니다.
        /// </summary>
        private void DeactivateAllPanels()
        {
            if (sugarMiniGamePanel != null)
                sugarMiniGamePanel.SetActive(false);
            if (eggMiniGamePanel != null)
                eggMiniGamePanel.SetActive(false);
            if (flourMiniGamePanel != null)
                flourMiniGamePanel.SetActive(false);
            if (butterMiniGamePanel != null)
                butterMiniGamePanel.SetActive(false);
            if (strawberryMiniGamePanel != null)
                strawberryMiniGamePanel.SetActive(false);
        }

        /// <summary>
        /// 타입에 맞는 패널 GameObject를 반환합니다.
        /// </summary>
        private GameObject GetPanelForType(MiniGameType type)
        {
            switch (type)
            {
                case MiniGameType.Sugar:
                    return sugarMiniGamePanel;
                case MiniGameType.Egg:
                    return eggMiniGamePanel;
                case MiniGameType.Flour:
                    return flourMiniGamePanel;
                case MiniGameType.Butter:
                    return butterMiniGamePanel;
                case MiniGameType.Strawberry:
                    return strawberryMiniGamePanel;
                default:
                    return null;
            }
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
                    if (eggMiniGamePanel != null)
                    {
                        var eggGame = eggMiniGamePanel.GetComponent<EggDeliveryMiniGame>();
                        if (eggGame == null)
                        {
                            Debug.LogError("[MiniGameManager] EggMiniGamePanel에 EggDeliveryMiniGame 컴포넌트가 없습니다!");
                            return null;
                        }
                        return eggGame;
                    }
                    Debug.LogError("[MiniGameManager] EggMiniGamePanel이 할당되지 않았습니다!");
                    return null;

                case MiniGameType.Flour:
                    if (flourMiniGamePanel != null)
                    {
                        var flourGame = flourMiniGamePanel.GetComponent<FlourStackingMiniGame>();
                        if (flourGame == null)
                        {
                            Debug.LogError("[MiniGameManager] FlourMiniGamePanel에 FlourStackingMiniGame 컴포넌트가 없습니다!");
                            return null;
                        }
                        return flourGame;
                    }
                    Debug.LogError("[MiniGameManager] FlourMiniGamePanel이 할당되지 않았습니다!");
                    return null;

                case MiniGameType.Butter:
                    if (butterMiniGamePanel != null)
                    {
                        var butterGame = butterMiniGamePanel.GetComponent<SimpleMiniGame>();
                        if (butterGame == null)
                        {
                            butterGame = butterMiniGamePanel.AddComponent<SimpleMiniGame>();
                        }
                        return butterGame;
                    }
                    Debug.LogError("[MiniGameManager] ButterMiniGamePanel이 할당되지 않았습니다!");
                    return null;

                case MiniGameType.Strawberry:
                    if (strawberryMiniGamePanel != null)
                    {
                        var strawberryGame = strawberryMiniGamePanel.GetComponent<StrawberryPickingMiniGame>();
                        if (strawberryGame == null)
                        {
                            Debug.LogError("[MiniGameManager] StrawberryMiniGamePanel에 StrawberryPickingMiniGame 컴포넌트가 없습니다!");
                            return null;
                        }
                        return strawberryGame;
                    }
                    Debug.LogError("[MiniGameManager] StrawberryMiniGamePanel이 할당되지 않았습니다!");
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

