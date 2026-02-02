using UnityEngine;

namespace BirthdayCakeQuest.Interaction
{
    /// <summary>
    /// 플레이어와 상호작용 가능한 오브젝트의 인터페이스입니다.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용이 가능한지 여부를 반환합니다.
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// 상호작용 프롬프트 텍스트를 반환합니다.
        /// </summary>
        string GetInteractPrompt();

        /// <summary>
        /// 상호작용을 실행합니다.
        /// </summary>
        /// <param name="interactor">상호작용을 시도한 GameObject (플레이어)</param>
        void Interact(GameObject interactor);

        /// <summary>
        /// 이 인터랙터블의 Transform을 반환합니다.
        /// </summary>
        Transform GetTransform();
    }
}

