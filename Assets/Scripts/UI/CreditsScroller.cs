using System.Collections;
using UnityEngine;
using TMPro;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 엔딩 크레딧을 자동으로 스크롤합니다.
    /// </summary>
    public sealed class CreditsScroller : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("스크롤 속도 (단위/초)")]
        [SerializeField] private float scrollSpeed = 50f;

        [Tooltip("스크롤 지속 시간 (초)")]
        [SerializeField] private float duration = 10f;

        [Tooltip("크레딧 종료 후 대기 시간 (초)")]
        [SerializeField] private float waitAfterComplete = 2f;

        [Header("References")]
        [Tooltip("스크롤될 RectTransform")]
        [SerializeField] private RectTransform creditsText;

        [Tooltip("종료 버튼 (크레딧 후 표시)")]
        [SerializeField] private GameObject returnButton;

        private bool _isScrolling;
        private float _startY;
        private float _targetY;

        private void Awake()
        {
            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!_isScrolling)
            {
                StartScrolling();
            }
        }

        /// <summary>
        /// 크레딧 스크롤을 시작합니다.
        /// </summary>
        public void StartScrolling()
        {
            if (_isScrolling)
                return;

            _isScrolling = true;

            if (creditsText != null)
            {
                _startY = creditsText.anchoredPosition.y;
                _targetY = _startY + (scrollSpeed * duration);
                StartCoroutine(ScrollRoutine());
            }
            else
            {
                Debug.LogWarning("[CreditsScroller] Credits text is not assigned!");
            }
        }

        private IEnumerator ScrollRoutine()
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // 부드러운 스크롤
                float newY = Mathf.Lerp(_startY, _targetY, progress);
                creditsText.anchoredPosition = new Vector2(
                    creditsText.anchoredPosition.x,
                    newY
                );

                yield return null;
            }

            // 최종 위치 설정
            creditsText.anchoredPosition = new Vector2(
                creditsText.anchoredPosition.x,
                _targetY
            );

            Debug.Log("[CreditsScroller] Credits scroll complete!");

            // 대기 후 버튼 표시
            yield return new WaitForSeconds(waitAfterComplete);

            if (returnButton != null)
            {
                returnButton.SetActive(true);
            }

            _isScrolling = false;
        }

        /// <summary>
        /// 크레딧을 리셋합니다.
        /// </summary>
        public void ResetCredits()
        {
            _isScrolling = false;
            StopAllCoroutines();

            if (creditsText != null)
            {
                creditsText.anchoredPosition = new Vector2(
                    creditsText.anchoredPosition.x,
                    _startY
                );
            }

            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }
        }
    }
}

