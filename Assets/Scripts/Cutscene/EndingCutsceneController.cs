using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Video;
using BirthdayCakeQuest.Ingredients;
using BirthdayCakeQuest.Player;
using BirthdayCakeQuest.Managers;

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

        [Header("TV Screen Settings")]
        [Tooltip("TV 화면에 비디오를 재생할 RenderTexture (TVScreen)")]
        [SerializeField] private RenderTexture tvScreenTexture;

        [Header("Cutscene Settings")]
        [Tooltip("VideoPlayer를 스크립트에서 직접 재생할지 여부")]
        [SerializeField] private bool controlVideoFromScript = false;

        [Tooltip("소파 앉기 위치 (플레이어가 이동할 좌표)")]
        [SerializeField] private Transform sofaSitPosition;

        [Header("Camera Transition")]
        [Tooltip("TV를 바라보는 카메라 위치 (Transform 또는 빈 GameObject)")]
        [SerializeField] private Transform tvCameraPosition;

        [Tooltip("카메라 전환 시간 (초)")]
        [SerializeField] private float cameraTransitionDuration = 2f;

        [Tooltip("카메라 전환 사용 여부")]
        [SerializeField] private bool useCameraTransition = true;

        [Header("Ending Music")]
        [Tooltip("엔딩 크레딧 배경음 (파일 할당 시 자동 재생)")]
        [SerializeField] private AudioClip endingMusic;

        [Header("Sit Animation")]
        [Tooltip("소파에 앉는 애니메이션 클립")]
        [SerializeField] private AnimationClip sitDownAnimation;

        [Tooltip("앉는 애니메이션 재생 여부")]
        [SerializeField] private bool playSitAnimation = true;

        [Tooltip("Animator의 앉기 파라미터 이름 (기본값: IsSitting)")]
        [SerializeField] private string sitAnimationParameterName = "IsSitting";

        [Tooltip("앉는 애니메이션 대기 시간 (초, 0이면 애니메이션 길이 사용)")]
        [SerializeField] private float sitAnimationWaitTime = 0f;

        [Tooltip("소파 앉기 시작 위치 (앉는 애니메이션이 시작되는 위치, 소파 앞에서 앉는 위치로 조정)")]
        [SerializeField] private Transform sofaFrontPosition;

        [Tooltip("TV 위치 (앉을 때 TV 방향으로 회전하기 위한 참조)")]
        [SerializeField] private Transform tvPosition;

        private bool _played;
        private BirthdayCakeQuest.Camera.IsometricFollowCamera _followCamera;
        private UnityEngine.Camera _mainCamera;
        private Coroutine _cameraTransitionCoroutine;

        private void Awake()
        {
            // 메인 카메라와 FollowCamera 찾기
            _mainCamera = UnityEngine.Camera.main;
            if (_mainCamera != null)
            {
                _followCamera = _mainCamera.GetComponent<BirthdayCakeQuest.Camera.IsometricFollowCamera>();
            }

            // VideoPlayer를 TV 화면에 재생하도록 설정
            SetupVideoPlayerForTV();
        }

        /// <summary>
        /// VideoPlayer를 TV 화면에 재생하도록 설정합니다.
        /// </summary>
        private void SetupVideoPlayerForTV()
        {
            if (videoPlayer == null)
                return;

            // RenderTexture가 할당되어 있으면 TV 화면에 재생
            if (tvScreenTexture != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = tvScreenTexture;
                Debug.Log("[EndingCutsceneController] VideoPlayer configured for TV screen (RenderTexture mode)");
            }
            else
            {
                Debug.LogWarning("[EndingCutsceneController] TVScreen RenderTexture not assigned. Video will not play on TV screen.");
            }
        }

        private void OnEnable()
        {
            // TitleScene에서는 엔딩 컷씬 관련 초기화를 건너뜀
            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                Debug.Log("[EndingCutsceneController] TitleScene에서는 엔딩 컷씬 초기화를 건너뜁니다.");
                // 크레딧 UI는 확실히 숨김
                if (creditsUIRoot != null)
                    creditsUIRoot.SetActive(false);
                return;
            }

            // 모든 재료 수집 시 케이크 표시
            if (inventory != null)
                inventory.OnAllCollected += OnAllIngredientsCollected;

            if (director != null)
                director.stopped += OnDirectorStopped;

            // 크레딧 UI 초기에 숨김 및 리셋
            if (creditsUIRoot != null)
            {
                creditsUIRoot.SetActive(false);
                
                // CreditsScroller를 찾아서 리셋
                var creditsScroller = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsScroller>();
                if (creditsScroller != null)
                {
                    creditsScroller.ResetCredits();
                    Debug.Log("[EndingCutsceneController] CreditsScroller 리셋 완료");
                }
            }
        }

        private void OnDisable()
        {
            // null 체크 강화 (Unity Editor에서 발생할 수 있는 문제 방지)
            try
            {
                if (inventory != null)
                    inventory.OnAllCollected -= OnAllIngredientsCollected;

                if (director != null)
                    director.stopped -= OnDirectorStopped;
            }
            catch (System.Exception e)
            {
                // Unity Editor에서 발생할 수 있는 예외는 무시
                if (Application.isPlaying)
                {
                    Debug.LogWarning($"[EndingCutsceneController] OnDisable error: {e.Message}");
                }
            }
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

            // 메인 음악 정지 (엔딩 컷씬 시작 시)
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.StopBackgroundMusic();
            }

            // 플레이어 입력 비활성화 및 CharacterController 비활성화
            if (playerController != null)
            {
                playerController.SetInputEnabled(false);
                
                // CharacterController 비활성화 (Timeline이 Transform을 제어하도록)
                var characterController = playerController.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                    Debug.Log("[EndingCutsceneController] CharacterController disabled for Timeline control");
                }
            }

            // sitDownAnimation이 null이면 자동으로 찾기
            if (playSitAnimation && sitDownAnimation == null)
            {
                sitDownAnimation = FindSitAnimationClip();
                if (sitDownAnimation != null)
                {
                    Debug.Log($"[EndingCutsceneController] 자동으로 애니메이션 클립 찾음: {sitDownAnimation.name}");
                }
                else
                {
                    Debug.LogWarning("[EndingCutsceneController] 앉는 애니메이션 클립을 찾을 수 없습니다!");
                }
            }

            Debug.Log($"[Check용] playSitAnimation: {playSitAnimation}, sitDownAnimation: {(sitDownAnimation != null ? sitDownAnimation.name : "null")}, playerTransform: {(playerTransform != null ? playerTransform.name : "null")}");

            // 앉는 애니메이션 재생 후 컷씬 시작
            // sitDownAnimation이 null이어도 sofaFrontPosition이 있으면 앉는 애니메이션 재생
            if (playSitAnimation && playerTransform != null && sofaFrontPosition != null)
            {
                StartCoroutine(PlaySitAnimationAndStartCutscene(playerTransform));
            }
            else
            {
                // 앉는 애니메이션 없이 바로 시작
                if (sofaSitPosition != null && playerTransform != null)
                {
                    // CharacterController를 일시적으로 비활성화하여 위치 설정
                    var characterController = playerTransform.GetComponent<CharacterController>();
                    if (characterController != null)
                    {
                        characterController.enabled = false;
                    }
                    
                    playerTransform.position = sofaSitPosition.position;
                    playerTransform.rotation = sofaSitPosition.rotation;
                }
                StartCutsceneAfterPositioning();
            }
        }

        /// <summary>
        /// 앉는 애니메이션을 재생하고 컷씬을 시작합니다.
        /// </summary>
        private IEnumerator PlaySitAnimationAndStartCutscene(Transform playerTransform)
        {
            // sofaFrontPosition이 필수임을 확인
            if (sofaFrontPosition == null)
            {
                Debug.LogError("[EndingCutsceneController] Sofa Front Position이 할당되지 않았습니다!");
                StartCutsceneAfterPositioning();
                yield break;
            }

            // CharacterController를 일시적으로 비활성화하여 위치 설정
            var characterController = playerTransform.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // 소파 앞 위치로 이동 (서 있는 위치)
            playerTransform.position = sofaFrontPosition.position;
            
            // TV 방향으로 회전
            if (tvPosition != null)
            {
                Vector3 directionToTV = (tvPosition.position - playerTransform.position).normalized;
                directionToTV.y = 0f;
                if (directionToTV != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTV);
                    playerTransform.rotation = lookRotation;
                    Debug.Log($"[EndingCutsceneController] Player rotated towards TV");
                }
            }
            else if (tvCameraPosition != null)
            {
                // tvCameraPosition을 사용하여 TV 방향 계산
                Vector3 directionToTV = (tvCameraPosition.position - playerTransform.position).normalized;
                directionToTV.y = 0f;
                if (directionToTV != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTV);
                    playerTransform.rotation = lookRotation;
                    Debug.Log($"[EndingCutsceneController] Player rotated towards TV (using tvCameraPosition)");
                }
            }
            else
            {
                playerTransform.rotation = sofaFrontPosition.rotation;
            }
            
            Debug.Log($"[EndingCutsceneController] Player moved to sit start position: {sofaFrontPosition.position}");

            // Animator 가져오기
            Animator animator = null;
            if (playerController != null)
            {
                animator = playerController.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = playerTransform.GetComponentInChildren<Animator>();
                }
            }

            // 앉는 애니메이션 재생
            if (animator != null)
            {
                // IsSitting 파라미터 설정 (Timeline이 제어하기 전에 설정)
                animator.SetBool(sitAnimationParameterName, true);
                Debug.Log($"[EndingCutsceneController] Set {sitAnimationParameterName} parameter to true");
                
                // 대기 시간 결정 (sitDownAnimation이 null이어도 기본값 사용)
                float waitTime = sitAnimationWaitTime > 0f ? sitAnimationWaitTime : 
                                 (sitDownAnimation != null ? sitDownAnimation.length : 2.233f); // Stand To Sit 기본 길이
                
                Debug.Log($"[EndingCutsceneController] Waiting for sit animation ({waitTime} seconds)");
                yield return new WaitForSeconds(waitTime);
                
                // 앉는 애니메이션은 sofaFrontPosition에서 재생됨
                // Timeline이 재생되기 전까지 sofaFrontPosition에 머물도록 함
                Debug.Log($"[EndingCutsceneController] Sit animation completed at front position: {playerTransform.position}");
            }
            else
            {
                Debug.LogWarning("[EndingCutsceneController] Animator not found!");
                yield return new WaitForSeconds(1f);
            }

            // 컷씬 시작 (카메라 전환 포함)
            StartCutsceneAfterPositioning();
        }

        /// <summary>
        /// 위치 설정 후 컷씬을 시작합니다.
        /// </summary>
        private void StartCutsceneAfterPositioning()
        {
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

            // 퀘스트 UI 숨김
            var questUI = FindObjectOfType<BirthdayCakeQuest.UI.QuestUI>();
            if (questUI != null && questUI.questPanel != null)
            {
                questUI.questPanel.SetActive(false);
                Debug.Log("[EndingCutsceneController] QuestUI hidden");
            }

            // VideoPlayer 미리 준비 (Timeline 시작 전)
            if (controlVideoFromScript && videoPlayer != null && videoPlayer.clip != null)
            {
                // VideoPlayer를 TV 화면에 재생하도록 설정
                if (tvScreenTexture != null)
                {
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    videoPlayer.targetTexture = tvScreenTexture;
                    Debug.Log("[EndingCutsceneController] VideoPlayer RenderTexture configured for TV screen");
                }

                // VideoPlayer 준비 중에는 보이지 않게 설정 (GameObject 비활성화)
                // Prepare()를 위해서는 활성화되어 있어야 하므로, Prepare() 후에 비활성화
                if (videoPlayer.gameObject != null)
                {
                    videoPlayer.gameObject.SetActive(true); // Prepare를 위해 활성화 필요
                }

                if (!videoPlayer.isPrepared)
                {
                    Debug.Log("[EndingCutsceneController] VideoPlayer 준비 중... (화면에 보이지 않음)");
                    videoPlayer.Prepare();
                    // Prepare 완료를 기다리는 코루틴 시작
                    StartCoroutine(WaitForVideoPlayerAndStartCameraTransition());
                    return;
                }
                else
                {
                    Debug.Log("[EndingCutsceneController] VideoPlayer 이미 준비됨");
                    // 이미 준비되어 있으면 비활성화 (재생 전까지 보이지 않게)
                    if (videoPlayer.gameObject != null)
                    {
                        videoPlayer.gameObject.SetActive(false);
                    }
                }
            }

            // 카메라를 TV 쪽으로 부드럽게 이동
            if (useCameraTransition && tvCameraPosition != null && _mainCamera != null)
            {
                // FollowCamera 비활성화 (카메라 제어를 스크립트로 전환)
                if (_followCamera != null)
                {
                    _followCamera.enabled = false;
                }

                // 카메라 전환 코루틴 시작
                if (_cameraTransitionCoroutine != null)
                {
                    StopCoroutine(_cameraTransitionCoroutine);
                }
                _cameraTransitionCoroutine = StartCoroutine(TransitionCameraToTV());
            }
            else
            {
                // 카메라 전환 없이 바로 Timeline 재생
                StartTimeline();
            }
        }

        /// <summary>
        /// VideoPlayer 준비 완료를 기다린 후 카메라 전환을 시작합니다.
        /// </summary>
        private IEnumerator WaitForVideoPlayerAndStartCameraTransition()
        {
            // VideoPlayer 준비 완료 대기
            float timeout = 10f; // 최대 10초 대기
            float elapsed = 0f;
            
            while (!videoPlayer.isPrepared && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (videoPlayer.isPrepared)
            {
                Debug.Log("[EndingCutsceneController] VideoPlayer 준비 완료");
                // 준비 완료 후 비활성화 (재생 전까지 보이지 않게)
                if (videoPlayer.gameObject != null)
                {
                    videoPlayer.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("[EndingCutsceneController] VideoPlayer 준비 시간 초과! 그래도 진행합니다.");
            }
            
            // 카메라 전환 시작
            if (useCameraTransition && tvCameraPosition != null && _mainCamera != null)
            {
                // FollowCamera 비활성화
                if (_followCamera != null)
                {
                    _followCamera.enabled = false;
                }

                // 카메라 전환 코루틴 시작
                if (_cameraTransitionCoroutine != null)
                {
                    StopCoroutine(_cameraTransitionCoroutine);
                }
                _cameraTransitionCoroutine = StartCoroutine(TransitionCameraToTV());
            }
            else
            {
                // 카메라 전환 없이 바로 Timeline 재생
                StartTimeline();
            }
        }

        /// <summary>
        /// 카메라를 TV 위치로 부드럽게 전환합니다.
        /// </summary>
        private IEnumerator TransitionCameraToTV()
        {
            if (_mainCamera == null || tvCameraPosition == null)
            {
                Debug.LogWarning("[EndingCutsceneController] Cannot transition camera - missing references");
                StartTimeline();
                yield break;
            }

            Vector3 startPosition = _mainCamera.transform.position;
            Quaternion startRotation = _mainCamera.transform.rotation;
            Vector3 targetPosition = tvCameraPosition.position;
            Quaternion targetRotation = tvCameraPosition.rotation;

            float elapsedTime = 0f;

            while (elapsedTime < cameraTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / cameraTransitionDuration);
                
                // 부드러운 전환을 위한 EaseInOut 곡선
                t = t * t * (3f - 2f * t); // Smoothstep

                _mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                _mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            // 최종 위치 보정
            _mainCamera.transform.position = targetPosition;
            _mainCamera.transform.rotation = targetRotation;

            Debug.Log("[EndingCutsceneController] Camera transition complete");

            // 카메라 전환 완료 후 Timeline 시작
            StartTimeline();
        }

        /// <summary>
        /// Timeline과 VideoPlayer를 시작합니다.
        /// </summary>
        private void StartTimeline()
        {
            // Timeline 재생 전에 플레이어 위치를 sofaFrontPosition에 고정
            // (Timeline이 Transform을 제어하지 않도록)
            if (playerController != null && sofaFrontPosition != null)
            {
                Transform playerTransform = playerController.transform;
                if (playerTransform != null)
                {
                    // CharacterController가 비활성화되어 있으므로 직접 위치 설정 가능
                    playerTransform.position = sofaFrontPosition.position;
                    Debug.Log($"[EndingCutsceneController] Player position fixed at front position before Timeline: {sofaFrontPosition.position}");
                }
            }

            // VideoPlayer를 TV 화면에 재생하도록 설정 (Timeline 시작 전에 다시 확인)
            if (videoPlayer != null && tvScreenTexture != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = tvScreenTexture;
                Debug.Log("[EndingCutsceneController] VideoPlayer RenderTexture configured for TV screen");
            }

            // VideoPlayer 재생 (controlVideoFromScript가 true일 때만 스크립트에서 제어)
            if (controlVideoFromScript && videoPlayer != null)
            {
                // VideoPlayer GameObject 활성화 (재생을 위해)
                if (videoPlayer.gameObject != null && !videoPlayer.gameObject.activeSelf)
                {
                    videoPlayer.gameObject.SetActive(true);
                }

                // 이미 준비되어 있어야 함 (StartCutsceneAfterPositioning에서 준비됨)
                if (videoPlayer.isPrepared)
                {
                    videoPlayer.Play();
                    Debug.Log("[EndingCutsceneController] VideoPlayer started from script (화면에 표시됨)");
                }
                else
                {
                    Debug.LogWarning("[EndingCutsceneController] VideoPlayer가 아직 준비되지 않았습니다!");
                    // 그래도 재생 시도
                    if (videoPlayer.clip != null)
                    {
                        videoPlayer.Prepare();
                        videoPlayer.prepareCompleted += OnVideoPrepared;
                    }
                    else
                    {
                        Debug.LogWarning("[EndingCutsceneController] VideoPlayer has no clip assigned!");
                    }
                }
            }
            else if (videoPlayer != null && !controlVideoFromScript)
            {
                Debug.Log("[EndingCutsceneController] VideoPlayer will be controlled by Timeline");
            }

            // Timeline 재생 (선택적, Timeline을 사용하지 않으면 null이어도 됨)
            if (director != null)
            {
                director.Play();
                
                // Timeline 재생 직후 플레이어 위치를 다시 고정
                // (Timeline의 Transform Track이 플레이어를 제어하지 않도록)
                StartCoroutine(FixPlayerPositionAfterTimelineStart());
            }
        }

        /// <summary>
        /// Timeline 재생 직후 플레이어 위치를 고정합니다.
        /// </summary>
        private IEnumerator FixPlayerPositionAfterTimelineStart()
        {
            // Timeline이 시작된 직후 위치 고정
            yield return null; // 한 프레임 대기
            
            if (playerController != null && sofaFrontPosition != null)
            {
                Transform playerTransform = playerController.transform;
                if (playerTransform != null)
                {
                    playerTransform.position = sofaFrontPosition.position;
                    Debug.Log($"[EndingCutsceneController] Player position fixed after Timeline start: {sofaFrontPosition.position}");
                }
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
            // VideoPlayer GameObject 활성화 (재생을 위해)
            if (source.gameObject != null && !source.gameObject.activeSelf)
            {
                source.gameObject.SetActive(true);
            }
            source.Play();
            Debug.Log("[EndingCutsceneController] VideoPlayer prepared and playing (화면에 표시됨)");
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            Debug.Log("[EndingCutsceneController] Timeline cutscene ended");

            // VideoPlayer 정리
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            // CharacterController 다시 활성화 (컷씬 종료 후)
            if (playerController != null)
            {
                var characterController = playerController.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = true;
                    Debug.Log("[EndingCutsceneController] CharacterController re-enabled");
                }
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

            // 엔딩 음악 재생 (파일이 할당된 경우)
            if (endingMusic != null)
            {
                // SceneLoader를 통해 엔딩 음악 재생
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.PlayEndingMusic(endingMusic);
                }
            }

            if (creditsUIRoot != null)
            {
                creditsUIRoot.SetActive(true);
                
                // CreditsLetterPlayer 우선 확인 (편지 + 가사)
                var letterPlayer = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsLetterPlayer>(false);
                
                if (letterPlayer != null && letterPlayer.enabled && letterPlayer.gameObject.activeInHierarchy)
                {
                    // 다른 크레딧 시스템 비활성화
                    var slidePlayer = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsSlidePlayer>(false);
                    if (slidePlayer != null)
                    {
                        slidePlayer.enabled = false;
                        Debug.Log("[EndingCutsceneController] CreditsSlidePlayer 비활성화 (CreditsLetterPlayer 사용)");
                    }
                    
                    var creditsScroller = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsScroller>(false);
                    if (creditsScroller != null)
                    {
                        creditsScroller.enabled = false;
                        Debug.Log("[EndingCutsceneController] CreditsScroller 비활성화 (CreditsLetterPlayer 사용)");
                    }
                    
                    letterPlayer.StartCredits();
                    Debug.Log("[EndingCutsceneController] CreditsLetterPlayer 시작");
                }
                // CreditsSlidePlayer 확인 (슬라이드 형식)
                else
                {
                    var slidePlayer = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsSlidePlayer>(false);
                    
                    if (slidePlayer != null && slidePlayer.enabled && slidePlayer.gameObject.activeInHierarchy)
                    {
                        // CreditsScroller 명시적으로 비활성화 (설정된 대로 실행)
                        var creditsScroller = creditsUIRoot.GetComponentInChildren<BirthdayCakeQuest.UI.CreditsScroller>(false);
                        if (creditsScroller != null)
                        {
                            creditsScroller.enabled = false;
                            Debug.Log("[EndingCutsceneController] CreditsScroller 비활성화 (CreditsSlidePlayer 사용)");
                        }
                        
                        slidePlayer.StartSlides();
                        Debug.Log("[EndingCutsceneController] CreditsSlidePlayer 시작");
                    }
                    else
                    {
                        // CreditsSlidePlayer가 없거나 비활성화된 경우 경고만 출력
                        Debug.LogWarning("[EndingCutsceneController] CreditsLetterPlayer 또는 CreditsSlidePlayer를 찾을 수 없거나 비활성화되어 있습니다. 크레딧을 표시할 수 없습니다.");
                    }
                }
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
                
                // CharacterController 다시 활성화
                var characterController = playerController.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = true;
                }
                
                // 앉는 애니메이션 파라미터 리셋
                Animator animator = playerController.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = playerController.GetComponentInChildren<Animator>();
                }
                if (animator != null && !string.IsNullOrEmpty(sitAnimationParameterName))
                {
                    foreach (AnimatorControllerParameter param in animator.parameters)
                    {
                        if (param.name == sitAnimationParameterName && param.type == AnimatorControllerParameterType.Bool)
                        {
                            animator.SetBool(sitAnimationParameterName, false);
                            break;
                        }
                    }
                }
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

        /// <summary>
        /// 앉는 애니메이션 클립을 자동으로 찾습니다.
        /// </summary>
        private AnimationClip FindSitAnimationClip()
        {
            // 1. "mixamo.com" 이름으로 찾기
            AnimationClip[] allClips = Resources.FindObjectsOfTypeAll<AnimationClip>();
            
            foreach (AnimationClip clip in allClips)
            {
                // "mixamo.com" 또는 "Stand To Sit" 포함하는 클립 찾기
                if (clip.name == "mixamo.com" || 
                    clip.name.Contains("Stand To Sit") || 
                    clip.name.Contains("StandToSit") ||
                    clip.name.Contains("Sit"))
                {
                    Debug.Log($"[EndingCutsceneController] 애니메이션 클립 발견: {clip.name} (길이: {clip.length}초)");
                    return clip;
                }
            }
            
            // 2. "Stand To Sit.fbx"에서 직접 추출 시도
            Object[] assets = Resources.LoadAll("Animations", typeof(AnimationClip));
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    if (clip.name.Contains("Stand") || clip.name.Contains("Sit") || clip.name.Contains("mixamo"))
                    {
                        Debug.Log($"[EndingCutsceneController] Resources에서 애니메이션 클립 발견: {clip.name}");
                        return clip;
                    }
                }
            }
            
            Debug.LogWarning("[EndingCutsceneController] 앉는 애니메이션 클립을 찾을 수 없습니다. Project 창에서 'Stand To Sit.fbx'를 확장하여 'mixamo.com' 클립을 찾아 할당하세요.");
            return null;
        }
    }
}
