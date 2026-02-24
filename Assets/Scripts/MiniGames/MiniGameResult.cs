using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 미니게임 결과를 Scene 간에 전달하는 싱글톤입니다.
    /// DontDestroyOnLoad로 Scene 전환 시에도 유지됩니다.
    /// </summary>
    public class MiniGameResult : MonoBehaviour
    {
        public static MiniGameResult Instance { get; private set; }

        [Header("Result Data")]
        [SerializeField] private MiniGameType currentMiniGameType;
        [SerializeField] private bool isSuccess;
        [SerializeField] private string returnSceneName;
        
        public Action<bool> OnMiniGameComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 미니게임을 시작하기 전에 호출됩니다.
        /// </summary>
        public void PrepareMiniGame(MiniGameType type, string returnScene, Action<bool> callback)
        {
            currentMiniGameType = type;
            returnSceneName = returnScene;
            OnMiniGameComplete = callback;
            isSuccess = false;

            Debug.Log($"[MiniGameResult] 미니게임 준비: {type}, 복귀 씬: {returnScene}");
        }

        /// <summary>
        /// 미니게임 결과를 설정하고 원래 Scene으로 복귀합니다.
        /// </summary>
        public void SetResultAndReturn(bool success)
        {
            isSuccess = success;
            Debug.Log($"[MiniGameResult] 결과: {(success ? "성공" : "실패")}");

            // 콜백 호출
            OnMiniGameComplete?.Invoke(isSuccess);
            OnMiniGameComplete = null;

            // 원래 Scene으로 복귀 (페이드 전환 포함)
            if (!string.IsNullOrEmpty(returnSceneName))
            {
                Debug.Log($"[MiniGameResult] {returnSceneName} Scene으로 복귀");
                StartCoroutine(ReturnToSceneWithFade(returnSceneName));
            }
        }

        /// <summary>
        /// 페이드 효과와 함께 원래 Scene으로 복귀합니다.
        /// 미니게임 씬을 언로드하고 Home 씬을 다시 활성화합니다.
        /// </summary>
        private System.Collections.IEnumerator ReturnToSceneWithFade(string sceneName)
        {
            var transitionManager = Utils.SceneTransitionManager.Instance;
            if (transitionManager != null)
            {
                // 페이드 아웃
                yield return transitionManager.FadeOut();

                // 현재 미니게임 씬 언로드 (Home 씬은 이미 로드되어 있음)
                string currentSceneName = SceneManager.GetActiveScene().name;
                Debug.Log($"[MiniGameResult] {currentSceneName} 씬 언로드 시작");
                
                // Home 씬을 먼저 Active Scene으로 설정
                Scene homeScene = SceneManager.GetSceneByName(sceneName);
                if (homeScene.isLoaded)
                {
                    SceneManager.SetActiveScene(homeScene);
                    Debug.Log($"[MiniGameResult] {sceneName}을 Active Scene으로 설정");
                }
                
                // Home 씬의 모든 루트 오브젝트 다시 활성화
                GameObject[] homeRootObjects = homeScene.GetRootGameObjects();
                Debug.Log($"[MiniGameResult] Home 씬 오브젝트 {homeRootObjects.Length}개 활성화");
                foreach (GameObject obj in homeRootObjects)
                {
                    obj.SetActive(true);
                    
                    // HomeScene의 AudioListener 활성화
                    AudioListener homeListener = obj.GetComponent<AudioListener>();
                    if (homeListener != null)
                    {
                        homeListener.enabled = true;
                        Debug.Log($"[MiniGameResult] Home 씬의 AudioListener 활성화: {obj.name}");
                    }
                }
                
                // Home 씬의 카메라 설정 복원
                UnityEngine.Camera homeCamera = UnityEngine.Camera.main;
                if (homeCamera != null)
                {
                    MiniGameSceneLoader.GetHomeCameraSettings(
                        out bool orthographic, 
                        out float orthographicSize, 
                        out float fieldOfView
                    );
                    
                    homeCamera.orthographic = orthographic;
                    homeCamera.orthographicSize = orthographicSize;
                    homeCamera.fieldOfView = fieldOfView;
                    
                    Debug.Log($"[MiniGameResult] Home 카메라 설정 복원 - orthographic: {orthographic}, FOV: {fieldOfView}");
                }
                
                // HomeScene 오브젝트들이 완전히 활성화되도록 대기
                yield return null;
                yield return null; // 추가 대기
                
                // QuestSequenceManager가 초기화될 시간 확보 (DontDestroyOnLoad가 아니므로 새 인스턴스 생성)
                var questManager = BirthdayCakeQuest.Managers.QuestSequenceManager.Instance;
                int retryCount = 0;
                while (questManager == null && retryCount < 10)
                {
                    yield return null;
                    questManager = BirthdayCakeQuest.Managers.QuestSequenceManager.Instance;
                    retryCount++;
                }
                
                if (questManager != null)
                {
                    Debug.Log("[MiniGameResult] QuestSequenceManager 초기화 완료");
                    
                    // QuestSequenceManager가 _pendingNextQuestIndex를 처리하도록 강제 초기화
                    // OnEnable을 다시 호출하여 대기 중인 퀘스트 처리
                    questManager.ProcessPendingQuest();
                }
                else
                {
                    Debug.LogWarning("[MiniGameResult] QuestSequenceManager를 찾을 수 없습니다!");
                }
                
                // DialogueSystem이 DialogueUI를 찾을 시간 확보
                var dialogueSystem = BirthdayCakeQuest.UI.DialogueSystem.Instance;
                if (dialogueSystem != null)
                {
                    dialogueSystem.FindDialogueUI();
                    Debug.Log("[MiniGameResult] DialogueSystem이 DialogueUI를 찾았습니다.");
                    // DialogueUI 찾기 및 초기화 완료 대기
                    yield return null;
                    yield return null; // 추가 대기
                }
                else
                {
                    Debug.LogWarning("[MiniGameResult] DialogueSystem을 찾을 수 없습니다!");
                }
                
                // QuestUI 찾아서 패널 표시
                var questUI = UnityEngine.Object.FindObjectOfType<BirthdayCakeQuest.UI.QuestUI>();
                if (questUI != null && questUI.questPanel != null)
                {
                    if (questManager != null && questManager.CurrentActiveIngredient != (BirthdayCakeQuest.Ingredients.IngredientId)(-1))
                    {
                        questUI.questPanel.SetActive(true);
                        Debug.Log("[MiniGameResult] QuestPanel 활성화");
                    }
                }
                
                // 미니게임 씬 언로드
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentSceneName);
                yield return asyncUnload;

                // 한 프레임 더 대기
                yield return null;

                // 페이드 인
                yield return transitionManager.FadeIn();
            }
            else
            {
                // SceneTransitionManager가 없으면 기존 방식 (fallback)
                Debug.LogWarning("[MiniGameResult] SceneTransitionManager가 없습니다. 기본 씬 로드 사용");
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// 현재 미니게임 타입을 반환합니다.
        /// </summary>
        public MiniGameType GetCurrentMiniGameType()
        {
            return currentMiniGameType;
        }

        /// <summary>
        /// 결과를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            currentMiniGameType = MiniGameType.Sugar;
            isSuccess = false;
            returnSceneName = "";
            OnMiniGameComplete = null;
        }
    }
}

