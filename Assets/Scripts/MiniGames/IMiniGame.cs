using System;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 모든 미니게임이 구현해야 하는 인터페이스입니다.
    /// </summary>
    public interface IMiniGame
    {
        /// <summary>
        /// 미니게임을 초기화합니다.
        /// </summary>
        /// <param name="onComplete">게임 완료 시 호출될 콜백 (성공: true, 실패: false)</param>
        void Initialize(Action<bool> onComplete);

        /// <summary>
        /// 미니게임을 시작합니다.
        /// </summary>
        void StartGame();

        /// <summary>
        /// 미니게임을 종료합니다.
        /// </summary>
        /// <param name="success">성공 여부</param>
        void EndGame(bool success);

        /// <summary>
        /// 미니게임 리소스를 정리합니다.
        /// </summary>
        void CleanUp();
    }
}

