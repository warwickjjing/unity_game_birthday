using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace BirthdayCakeQuest.Managers
{
    /// <summary>
    /// 씬 전환을 관리하는 유틸리티 클래스입니다.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
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
    }
}

