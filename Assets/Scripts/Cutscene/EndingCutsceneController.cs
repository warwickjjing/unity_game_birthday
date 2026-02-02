using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Player;

namespace BirthdayCakeQuest.Cutscene
{
    /// <summary>
    /// 엔딩 컷씬을 제어합니다.
    /// 소파 인터랙션으로 Timeline 컷씬을 재생하고,
    /// 컷씬이 끝나면 크레딧과 엔딩 UI를 표시합니다.
    /// </summary>
    public sealed class EndingCutsceneController : MonoBehaviour
    {
        [Header("Triggers")]
        [Tooltip("재료 인벤토리 (케이크 표시용)")]
        [SerializeField] private IngredientInventory inventory;

        [Header("Cutscene")]
        [Tooltip("Timeline을 제어하는 PlayableDirector")]
        [SerializeField] private PlayableDirector director;

        [Header("Optional Components")]
        [Tooltip("플레이어 컨트롤러 (입력 잠금 및 케이크 표시용)")]
        [SerializeField] private PlayerController playerController;
        
        [Tooltip("게임플레이 UI 루트 (컷씬 중 숨김)")]
        [SerializeField] private GameObject gameplayUIRoot;
        
        [Tooltip("엔딩 UI 루트 (컷씬 후 표시)")]
        [SerializeField] private GameObject endingUIRoot;

        [Tooltip("크레딧 UI 루트 (영상 후 표시)")]
        [SerializeField] private GameObject creditsUIRoot;
        
        [Tooltip("VideoPlayer (선택, Timeline에서 제어 가능)")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Cutscene Settings")]
        [Tooltip("VideoPlayer를 스크립트에서 직접 재생할지 여부")]
        [SerializeField] private bool controlVideoFromScript = false;

        [Tooltip("소파 앉기 위치 (플레이어가 이동할 좌표)")]
        [SerializeField] private Transform sofaSitPosition;

        private bool _played;

        private void OnEnable()
        {
            // 모든 재료 수집 시 케이크 표시
            if (inventory != null)
                inventory.OnAllCollected += OnAllIngredientsCollected;

            if (director != null)
                director.stopped += OnDirectorStopped;

            // 크레딧 UI 초기에 숨김
            if (creditsUIRoot != null)
                creditsUIRoot.SetActive(false);
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.OnAllCollected -= OnAllIngredientsCollected;

            if (director != null)
                director.stopped -= OnDirectorStopped;
        }

        private void OnAllIngredientsCollected()
        {
            Debug.Log("[EndingCutsceneController] All ingredients collected! Showing cake.");
            
            // 플레이어에게 케이크 표시
            if (playerController != null)
            {
                playerController.ShowCake();
            }
        }

        /// <summary>
        /// 소파에서 엔딩 컷씬을 시작합니다.
        /// </summary>
        /// <param name="playerTransform">플레이어의 Transform</param>
        public void StartFromSofa(Transform playerTransform)
        {
            if (_played)
            {
                Debug.LogWarning("[EndingCutsceneController] Cutscene already played.");
                return;
            }
            
            _played = true;
            Debug.Log("[EndingCutsceneController] ENDING CUTSCENE STARTED FROM SOFA!");

            // 플레이어를 소파 위치로 이동
            if (sofaSitPosition != null && playerTransform != null)
            {
                playerTransform.position = sofaSitPosition.position;
                playerTransform.rotation = sofaSitPosition.rotation;
            }

            // 플레이어 입력 비활성화
            if (playerController != null)
            {
                playerController.SetInputEnabled(false);
            }

            // 게임플레이 UI 숨김
            if (gameplayUIRoot != null)
            {
                gameplayUIRoot.SetActive(false);
            }

            // 엔딩 UI와 크레딧 숨김 (나중에 표시)
            if (endingUIRoot != null)
            {
                endingUIRoot.SetActive(false);
            }

            if (creditsUIRoot != null)
            {
                creditsUIRoot.SetActive(false);
            }

            // VideoPlayer 제어 (선택)
            if (controlVideoFromScript && videoPlayer != null)
            {
                if (videoPlayer.isPrepared)
                {
                    videoPlayer.Play();
                    Debug.Log("[EndingCutsceneController] VideoPlayer started");
                }
                else if (videoPlayer.clip != null)
                {
                    // 준비되지 않았다면 준비 후 재생
                    videoPlayer.Prepare();
                    videoPlayer.prepareCompleted += OnVideoPrepared;
                }
            }

            // Timeline 재생
            if (director != null)
            {
                director.Play();
            }
            else
            {
                Debug.LogWarning("[EndingCutsceneController] PlayableDirector is not assigned!");
            }
        }

        /// <summary>
        /// 엔딩 컷씬을 재생합니다 (레거시, 이제 StartFromSofa 사용).
        /// </summary>
        [System.Obsolete("Use StartFromSofa instead")]
        public void Play()
        {
            StartFromSofa(playerController?.transform);
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnVideoPrepared;
            source.Play();
            Debug.Log("[EndingCutsceneController] VideoPlayer prepared and playing");
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            Debug.Log("[EndingCutsceneController] Timeline cutscene ended");

            // VideoPlayer 정리
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            // 크레딧 표시 (Signal에서도 호출 가능)
            ShowCredits();
        }

        /// <summary>
        /// 크레딧을 표시합니다.
        /// </summary>
        public void ShowCredits()
        {
            Debug.Log("[EndingCutsceneController] Showing credits");

            if (creditsUIRoot != null)
            {
                creditsUIRoot.SetActive(true);
            }
        }

        /// <summary>
        /// 엔딩 UI를 표시합니다 (크레딧 이후).
        /// </summary>
        public void ShowEndingUI()
        {
            Debug.Log("[EndingCutsceneController] Showing ending UI");

            if (endingUIRoot != null)
            {
                endingUIRoot.SetActive(true);
            }
        }

        /// <summary>
        /// 컷씬을 수동으로 트리거합니다 (테스트용).
        /// </summary>
        [ContextMenu("Play Cutscene (Test)")]
        public void PlayTest()
        {
            _played = false;
            StartFromSofa(playerController?.transform);
        }

        /// <summary>
        /// 컷씬을 리셋합니다.
        /// </summary>
        [ContextMenu("Reset Cutscene")]
        public void ResetCutscene()
        {
            _played = false;
            
            if (director != null)
            {
                director.Stop();
                director.time = 0;
            }

            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }

            if (playerController != null)
            {
                playerController.SetInputEnabled(true);
            }

            if (gameplayUIRoot != null)
            {
                gameplayUIRoot.SetActive(true);
            }

            if (endingUIRoot != null)
            {
                endingUIRoot.SetActive(false);
            }

            Debug.Log("[EndingCutsceneController] Cutscene reset complete");
        }
    }
}
