using UnityEngine;

namespace BirthdayCakeQuest.Props
{
    /// <summary>
    /// 플레이어가 들고 있는 케이크 오브젝트를 관리합니다.
    /// </summary>
    public sealed class CakeHolder : MonoBehaviour
    {
        [Header("Cake Object")]
        [Tooltip("케이크 프리팹 또는 하위 오브젝트")]
        [SerializeField] private GameObject cakeObject;

        [Header("Position")]
        [Tooltip("케이크가 표시될 상대 위치 (플레이어 기준)")]
        [SerializeField] private Vector3 holdPosition = new Vector3(0.3f, 1.2f, 0.3f);

        [Tooltip("케이크의 상대 회전")]
        [SerializeField] private Vector3 holdRotation = new Vector3(0f, 0f, 0f);

        [Tooltip("케이크의 스케일")]
        [SerializeField] private Vector3 holdScale = new Vector3(0.3f, 0.3f, 0.3f);

        private bool _isShowing;

        private void Awake()
        {
            // 초기에는 숨김
            if (cakeObject != null)
            {
                cakeObject.SetActive(false);
            }
        }

        /// <summary>
        /// 케이크를 표시합니다.
        /// </summary>
        public void ShowCake()
        {
            if (_isShowing)
                return;

            _isShowing = true;

            if (cakeObject != null)
            {
                cakeObject.transform.localPosition = holdPosition;
                cakeObject.transform.localRotation = Quaternion.Euler(holdRotation);
                cakeObject.transform.localScale = holdScale;
                cakeObject.SetActive(true);

                Debug.Log("[CakeHolder] Cake is now visible!");
            }
            else
            {
                Debug.LogWarning("[CakeHolder] Cake object is not assigned!");
            }
        }

        /// <summary>
        /// 케이크를 숨깁니다.
        /// </summary>
        public void HideCake()
        {
            _isShowing = false;

            if (cakeObject != null)
            {
                cakeObject.SetActive(false);
            }
        }

        /// <summary>
        /// 케이크가 표시 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsShowing => _isShowing;
    }
}

