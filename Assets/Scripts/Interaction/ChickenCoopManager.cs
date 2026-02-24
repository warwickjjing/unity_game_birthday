using System.Collections.Generic;
using UnityEngine;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 닭장의 닭들이 달아난 상태를 관리합니다.
    /// 닭들이 모두 달아나면 계란을 수집할 수 있게 됩니다.
    /// </summary>
    public class ChickenCoopManager : MonoBehaviour
    {
        public static ChickenCoopManager Instance { get; private set; }

        [Header("Chicken References")]
        [Tooltip("닭장에 있는 모든 닭 오브젝트들 (달아나야 할 닭들)")]
        [SerializeField] private List<GameObject> chickens = new List<GameObject>();

        [Header("Escape Settings")]
        [Tooltip("닭들이 달아나야 하는 거리 (닭장에서 이 거리 이상 떨어지면 달아난 것으로 간주)")]
        [SerializeField] private float escapeDistance = 5f;

        [Tooltip("닭장의 중심 위치 (Transform 또는 빈 GameObject, 비어있으면 이 GameObject의 위치 사용)")]
        [SerializeField] private Transform coopCenter;

        [Tooltip("고양이가 닭장에 들어갔을 때 닭들이 달아나는 속도 배율 (기본 속도에 곱해짐)")]
        [SerializeField] private float escapeSpeedMultiplier = 2f;

        [Tooltip("고양이가 닭장에 들어갔을 때 닭들이 달아나는 반경 제한 해제 여부")]
        [SerializeField] private bool removeRadiusLimitOnEscape = true;

        private HashSet<GameObject> _escapedChickens = new HashSet<GameObject>();
        private Vector3 _coopPosition;
        private bool _escapeTriggered = false;

        /// <summary>
        /// 모든 닭들이 달아났는지 여부
        /// </summary>
        public bool AllChickensEscaped => _escapedChickens.Count >= chickens.Count && chickens.Count > 0;

        /// <summary>
        /// 달아난 닭의 수
        /// </summary>
        public int EscapedChickenCount => _escapedChickens.Count;

        /// <summary>
        /// 전체 닭의 수
        /// </summary>
        public int TotalChickenCount => chickens.Count;

        /// <summary>
        /// 닭장의 중심 위치를 반환합니다 (coopCenter가 설정되어 있으면 그것을, 없으면 GameObject 위치를 반환).
        /// </summary>
        public Vector3 CoopPosition => _coopPosition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[ChickenCoopManager] 이미 인스턴스가 존재합니다. 중복 인스턴스를 제거합니다.");
                Destroy(gameObject);
                return;
            }

            // 닭장 중심 위치 설정
            if (coopCenter != null)
            {
                _coopPosition = coopCenter.position;
            }
            else
            {
                _coopPosition = transform.position;
            }
        }

        private void Update()
        {
            // 매 프레임 닭들이 달아났는지 확인
            CheckChickenEscapeStatus();
        }

        /// <summary>
        /// 닭들이 달아났는지 확인합니다.
        /// </summary>
        private void CheckChickenEscapeStatus()
        {
            foreach (var chicken in chickens)
            {
                if (chicken == null) continue;
                if (_escapedChickens.Contains(chicken)) continue;

                float distance = Vector3.Distance(chicken.transform.position, _coopPosition);
                if (distance >= escapeDistance)
                {
                    _escapedChickens.Add(chicken);
                }
            }
        }

        /// <summary>
        /// 고양이를 닭장에 넣었을 때 호출합니다. 닭들을 달아나게 합니다.
        /// </summary>
        public void TriggerChickenEscape()
        {
            if (_escapeTriggered)
            {
                return;
            }

            _escapeTriggered = true;

            foreach (var chicken in chickens)
            {
                if (chicken == null) continue;

                // AnimalWander 컴포넌트를 찾아서 달아나게 설정
                var wander = chicken.GetComponent<BirthdayCakeQuest.Animals.AnimalWander>();
                if (wander != null)
                {
                    // Escape 메서드 호출하여 달아나게 함
                    wander.Escape(escapeSpeedMultiplier, removeRadiusLimitOnEscape);
                }
                else
                {
                    Debug.LogWarning($"[ChickenCoopManager] {chicken.name}에 AnimalWander 컴포넌트가 없습니다!");
                }
            }
        }

        /// <summary>
        /// 닭을 수동으로 달아난 상태로 표시합니다 (테스트용).
        /// </summary>
        public void MarkChickenAsEscaped(GameObject chicken)
        {
            if (chicken != null && chickens.Contains(chicken))
            {
                _escapedChickens.Add(chicken);
            }
        }

        /// <summary>
        /// 모든 닭을 달아난 상태로 표시합니다 (테스트용).
        /// </summary>
        [ContextMenu("Mark All Chickens Escaped (Test)")]
        public void MarkAllChickensEscaped()
        {
            foreach (var chicken in chickens)
            {
                if (chicken != null)
                {
                    _escapedChickens.Add(chicken);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 닭장 중심 위치 시각화
            Vector3 center = coopCenter != null ? coopCenter.position : transform.position;
            
            // 달아나야 하는 거리 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, escapeDistance);
            
            // 닭장 중심 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, 0.5f);
        }
    }
}

