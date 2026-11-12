
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameInfoToCurrentUserData
{
  /// <summary>
  /// 승패에 따른 승리, 패배 점수 획득
  /// </summary>
  public void JudgementWinLose(bool judgement)
  {
    if (UserManager.Instance._CurrentUserData != null)
      _ = judgement ? UserManager.Instance._CurrentUserData.win++ : UserManager.Instance._CurrentUserData.lose++;
    else
    {
      GameLogger.LogWarning("<color=#1976D2>LHW</color>_EndGameInfoToCurrentUserData.JudgementWinLose(): UserManager.Instance._currentUserData == null");
    }
  }
  /// <summary>
  /// 승급 시스템
  /// </summary>
  public void Promotion()
  {
    if (UserManager.Instance._CurrentUserData != null)
    {
      if (UserManager.Instance._CurrentUserData.ranked > 1)
        UserManager.Instance._CurrentUserData.ranked--;
      else if (UserManager.Instance._CurrentUserData.ranked <= 1)
        UserManager.Instance._CurrentUserData.ranked = 1;
    }
    else
    {
      GameLogger.LogWarning("<color=#1976D2>LHW</color>_EndGameInfoToCurrentUserData.Promotion(): UserManager.Instance._currentUserData == null");
    }
  }
  /// <summary>
  /// 강등 시스템
  /// </summary>
  public void Demotion()
  {
    if (UserManager.Instance._CurrentUserData != null)
    {
      if (UserManager.Instance._CurrentUserData.ranked < 30)
        UserManager.Instance._CurrentUserData.ranked++;
      else if (UserManager.Instance._CurrentUserData.ranked >= 30)
        UserManager.Instance._CurrentUserData.ranked = 30;
    }
    else
    {
      GameLogger.LogWarning("<color=#1976D2>LHW</color>_EndGameInfoToCurrentUserData.Demotion(): UserManager.Instance._currentUserData == null");
    }
  }
  /// <summary>
  /// 승급 포인트 획득 또는 감소
  /// </summary>
  /// <param name="addPoint">점수</param>
  public void AddPoint(int addPoint)
  {
    if (UserManager.Instance._CurrentUserData != null)
    {
      var curData = UserManager.Instance._CurrentUserData;
      curData.point += addPoint;
      UserManager.Instance._CurrentUserData = curData;
    }
    else
    {
      GameLogger.LogWarning("<color=#1976D2>LHW</color>_EndGameInfoToCurrentUserData.AddPoint(): UserManager.Instance._currentUserData == null");
    }
  }

  /// <summary>
  /// 승급 포인트 초기화
  /// </summary>
  public void ResetPoint()
  {
    if (UserManager.Instance._CurrentUserData != null)
      UserManager.Instance._CurrentUserData.point = 0;
    else
    {
      GameLogger.LogWarning("<color=#1976D2>LHW</color>_EndGameInfoToCurrentUserData.ResetPoint(): UserManager.Instance._currentUserData == null");
    }
  }
}