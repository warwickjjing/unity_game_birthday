using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Player;

namespace BirthdayCakeQuest.Cutscene
{
    /// <summary>
    /// 엔딩 컷씬을 제어합니다.
    /// 모든 재료가 수집되면 Timeline 컷씬을 재생하고,
    /// 컷씬이 끝나면 엔딩 UI를 표시합니다.
    /// </summary>
    public sealed class EndingCutsceneController : MonoBehaviour
    {
        [Header("Triggers")]
        [Tooltip("재료 인벤토리 (이벤트 구독용)")]
        [SerializeField] private IngredientInventory inventory;

        [Header("Cutscene")]
        [Tooltip("Timeline을 제어하는 PlayableDirector")]
        [SerializeField] private PlayableDirector director;

        [Header("Optional Components")]
        [Tooltip("플레이어 컨트롤러 (입력 잠금용)")]
        [SerializeField] private PlayerController playerController;
        
        [Tooltip("게임플레이 UI 루트 (컷씬 중 숨김)")]
        [SerializeField] private GameObject gameplayUIRoot;
        
        [Tooltip("엔딩 UI 루트 (컷씬 후 표시)")]
        [SerializeField] private GameObject endingUIRoot;
        
        [Tooltip("VideoPlayer (선택, Timeline에서 제어 가능)")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Cutscene Settings")]
        [Tooltip("VideoPlayer를 스크립트에서 직접 재생할지 여부")]
        [SerializeField] private bool controlVideoFromScript = false;

        private bool _played;

        private void OnEnable()
        {
            if (inventory != null)
                inventory.OnAllCollected += Play;

            if (director != null)
                director.stopped += OnDirectorStopped;
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.OnAllCollected -= Play;

            if (director != null)
                director.stopped -= OnDirectorStopped;
        }

        /// <summary>
        /// 엔딩 컷씬을 재생합니다.
        /// </summary>
        public void Play()
        {
            if (_played)
            {
                Debug.LogWarning("[EndingCutsceneController] Cutscene already played.");
                return;
            }
            
            _played = true;
            Debug.Log("[EndingCutsceneController] ENDING CUTSCENE STARTED!");

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

            // 엔딩 UI 숨김 (컷씬 후 표시)
            if (endingUIRoot != null)
            {
                endingUIRoot.SetActive(false);
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

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnVideoPrepared;
            source.Play();
            Debug.Log("[EndingCutsceneController] VideoPlayer prepared and playing");
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            Debug.Log("[EndingCutsceneController] Timeline cutscene ended");

            // 엔딩 UI 표시
            if (endingUIRoot != null)
            {
                endingUIRoot.SetActive(true);
            }

            // VideoPlayer 정리
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }

        /// <summary>
        /// 컷씬을 수동으로 트리거합니다 (테스트용).
        /// </summary>
        [ContextMenu("Play Cutscene (Test)")]
        public void PlayTest()
        {
            _played = false;
            Play();
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
