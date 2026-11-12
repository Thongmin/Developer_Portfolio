using System;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;

public class kyuSystem : MonoBehaviour
{
  private EndGameInfoToCurrentUserData endGameInfoToCurrentUserData = new EndGameInfoToCurrentUserData();
  [SerializeField] GameOver gameover;
  /// <summary>
  /// 승패 포인트 획득 또는 감소 메서드 호출, 승패 포인트 점수 UI 표시
  /// </summary>
  /// <param name="addPoint">점수</param>
  public void AddPoint(int addPoint)
  {
    GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 급수 시스템 시작");
    endGameInfoToCurrentUserData.AddPoint(addPoint);

    if (UserManager.Instance._CurrentUserData == null)
    {
      GameLogger.LogWarning($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - _CurrentUserDat = null");
      return;
    }
    // int point = UserManager.Instance._CurrentUserDat.point;
    GameLogger.LogWarning($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트: {UserManager.Instance._CurrentUserData.point}");

    if (UserManager.Instance._CurrentUserData.point == 0)
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 0 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
    }
    else if (UserManager.Instance._CurrentUserData.point >= Math.Abs(addPoint * 3))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 3 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
      endGameInfoToCurrentUserData.Promotion();
      endGameInfoToCurrentUserData.ResetPoint();
    }
    else if (UserManager.Instance._CurrentUserData.point == Math.Abs(addPoint * 2))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 2 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
    }
    else if (UserManager.Instance._CurrentUserData.point == Math.Abs(addPoint * 1))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 1 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
    }
    else if (UserManager.Instance._CurrentUserData.point == -Math.Abs(addPoint * 1))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 -1 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
    }
    else if (UserManager.Instance._CurrentUserData.point == -Math.Abs(addPoint * 2))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 -2 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
    }
    else if (UserManager.Instance._CurrentUserData.point <= -Math.Abs(addPoint * 3))
    {
      GameLogger.Log($"<color=#4CAF50>kyuSystem</color>: <color=#F44336>AddPoint</color> - 승패 포인트가 -3 일때");
      gameover.SpawnBlocks(UserManager.Instance._CurrentUserData.point);
      endGameInfoToCurrentUserData.Demotion();
      endGameInfoToCurrentUserData.ResetPoint();
    }
  }

  /// <summary>
  /// 승패 판별, 승패에 따른 승리, 패배 점수 획득 메서드 호출.
  /// </summary>
  /// <param name="point">점수</param>
  public void JudgementWinLose(bool judgementWin)
  {
    endGameInfoToCurrentUserData.JudgementWinLose(judgementWin);
  }
}