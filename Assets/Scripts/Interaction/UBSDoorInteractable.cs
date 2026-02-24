using UnityEngine;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// UBS 문을 F키 상호작용 시스템에 연결하는 래퍼 스크립트입니다.
    /// UBS_Operator 또는 UBS_Actuator와 함께 사용합니다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class UBSDoorInteractable : MonoBehaviour, IInteractable
    {
        [Header("UBS Door References")]
        [Tooltip("UBS_Operator 컴포넌트 (문 손잡이 등)")]
        [SerializeField] private UBS_Operator ubsOperator;
        
        [Tooltip("UBS_Actuator 컴포넌트 (실제 문)")]
        [SerializeField] private UBS_Actuator ubsActuator;

        [Header("Settings")]
        [Tooltip("상호작용 가능한지 여부")]
        [SerializeField] private bool canInteract = true;

        private bool _initialized = false;

        public bool CanInteract => canInteract && _initialized;

        public string GetInteractPrompt()
        {
            if (ubsActuator != null)
            {
                // UBS_Actuator의 binaryState로 열림/닫힘 상태 확인
                return ubsActuator.binaryState ? "Close Door [F]" : "Open Door [F]";
            }
            return "Operate [F]";
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            // UBS_Actuator가 있으면 직접 제어
            if (ubsActuator != null)
            {
                ubsActuator.ActivateToggle();
                Debug.Log($"[UBSDoorInteractable] Door toggled via UBS_Actuator");
            }
            // UBS_Operator가 있으면 Operator를 통해 제어
            else if (ubsOperator != null)
            {
                // UBS_Operator의 objectList에 있는 각 오브젝트의 UBS_Actuator를 직접 호출
                if (ubsOperator.objectList != null && ubsOperator.objectList.Count > 0)
                {
                    foreach (var subObject in ubsOperator.objectList)
                    {
                        if (subObject == null) continue;
                        
                        UBS_Actuator actuator = subObject.GetComponent<UBS_Actuator>();
                        if (actuator != null)
                        {
                            switch (ubsOperator.operatorType)
                            {
                                case UBS_Operator.EnumOperatorType.Toggle:
                                    actuator.ActivateToggle();
                                    break;
                                case UBS_Operator.EnumOperatorType.Open:
                                    actuator.ActivateOpen();
                                    break;
                                case UBS_Operator.EnumOperatorType.Close:
                                    actuator.ActivateClose();
                                    break;
                            }
                            Debug.Log($"[UBSDoorInteractable] Door operated via UBS_Operator ({ubsOperator.operatorType}) on {subObject.name}");
                        }
                        else
                        {
                            Debug.LogWarning($"[UBSDoorInteractable] {subObject.name}에 UBS_Actuator 컴포넌트가 없습니다!");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[UBSDoorInteractable] {name}: UBS_Operator의 objectList가 비어있습니다!");
                }
            }
            else
            {
                Debug.LogWarning($"[UBSDoorInteractable] {name}: UBS_Operator 또는 UBS_Actuator가 없습니다!");
            }
        }

        private void Awake()
        {
            // 자동으로 UBS 컴포넌트 찾기
            if (ubsOperator == null)
            {
                ubsOperator = GetComponent<UBS_Operator>();
            }

            if (ubsActuator == null)
            {
                ubsActuator = GetComponent<UBS_Actuator>();
            }

            // 둘 중 하나는 있어야 함
            if (ubsOperator == null && ubsActuator == null)
            {
                Debug.LogWarning($"[UBSDoorInteractable] {name}: UBS_Operator 또는 UBS_Actuator 컴포넌트를 찾을 수 없습니다. " +
                    "이 스크립트는 UBS 문 오브젝트에만 사용할 수 있습니다.");
            }
            else
            {
                _initialized = true;
            }

            // BoxCollider가 Trigger인지 확인
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }
    }
}

