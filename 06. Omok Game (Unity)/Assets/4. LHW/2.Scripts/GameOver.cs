using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
  [SerializeField] GameObject _gameOverUI;
  [SerializeField] TMP_Text _gameOverInfoTMP;

  [SerializeField] GameObject _PointUI;
  [SerializeField] kyuSystem kyuSystem;
  [SerializeField] GameObject pointBar;
  [SerializeField] RectTransform pointBlockParent;
  [SerializeField] GameObject pointBlock;

  private Queue<GameObject> _resetPointBlock = new Queue<GameObject>();


  /// <summary>
  /// UI 초기화
  /// </summary>
  void Start()
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336></color> - ");




    ResetGameOverUI();
  }
  /// <summary>
  /// 승패 포인트 계산
  /// </summary>
  /// <param name="judgementWin">승패 여부</param>
  /// <returns></returns>
  public int CalculatePoint(bool judgementWin) => judgementWin ? 10 : -10;

  /// <summary>
  /// 외부 호출 코드. SetGameOver 실행시 GameOver의 모든 기능 순서대로 실행.
  /// </summary>
  /// <param name="winner">승리자</param>
  public void SetGameOver(PlayerState winner)
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetGameOver</color> - SetGameOver 시작 전 승패 포인트: {UserManager.Instance._CurrentUserData.point}");
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetGameOver</color> - SetGameOver 시작");
    bool judgementWin = winner == PlayerState.PlayerA || winner == PlayerState.PlayerB;

    if (GameManager.Instance.gameState == PlayState.SinglePlay || GameManager.Instance.gameState == PlayState.MultiPlay)
    {
      GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetGameOver</color> - SingPlay, MultiPlay 일 때");
      // int point = (winner == Constants.PlayerType.PlayerA) ? 10 : -10;
      int point = CalculatePoint(judgementWin);

      OnGameOverUI();
      JudgementWinLose(judgementWin);
      AddCoin(judgementWin);
      StartKyuSystem(point);
      SetGameOverInfo(point);
    }
    else if (GameManager.Instance.gameState == PlayState.DualPlay)
    {
      GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetGameOver</color> - DualPlay 일 때");
      OnGameOverUI();
      SetDualPlayGameOverInfo(judgementWin);
    }

    UserManager.Instance.LocalBackupUserData();
  }
  /// <summary>
  /// 확인 버튼. Main씬으로 이동 메서드 호출
  /// </summary>
  public void CloseButtonPress()
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>CloseButtonPress</color> - 확인 => Main씬 이동");
    ChangeMainScene();
  }
  private void JudgementWinLose(bool judgementWin)
  {
    kyuSystem.JudgementWinLose(judgementWin);
  }
  /// <summary>
  /// 게임 승리시 코인 획득 함수 호출
  /// </summary>
  /// <param name="judgementWin"></param>
  private void AddCoin(bool judgementWin)
  {
    if (judgementWin)
    {
      GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>AddCoin</color> - 코인 획득");
      int coin = 100;
      UserManager.Instance.AddCoin(coin);
    }
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="point"></param>
  private void StartKyuSystem(int point)
  {
    kyuSystem.AddPoint(point);
    // DB 반영
  }
  /// <summary>
  /// [싱글, 멀티 플레이]
  /// 승패 여부에 따라 text UI 정보 표시
  /// </summary>
  /// <param name="point"></param>
  private void SetGameOverInfo(int point)
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetGameOverInfo</color> - Text 출력");
    _gameOverInfoTMP.text = point >= 0 ?
    $"게임에서 승리했습니다! \n {point}승금 포인트를 얻었습니다!" : $"게임에서 패배했습니다. \n {point}만큼 포인트가 감소했습니다!";
  }
  /// <summary>
  /// [듀얼 플레이]
  /// 승패 여부에 따라 text UI 정보 표시
  /// </summary>
  /// <param name="judgementWin">승패 여부</param>
  private void SetDualPlayGameOverInfo(bool judgementWin)
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SetDualPlayGameOverInfo</color> - Text 출력");
    _gameOverInfoTMP.text = judgementWin ?
    "흑돌이 게임에서 승리했습니다! " : "백돌이 게임에서 승리했습니다!";
  }
  /// <summary>
  /// GameOverUI 비활성화
  /// </summary>
  private void ResetGameOverUI()
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>ResetGameOverUI</color> - GameOverUI 초기화");
    _gameOverUI.SetActive(false);
    pointBar.SetActive(false);

    if (_resetPointBlock != null)
    {
      while (_resetPointBlock.Count > 0)
      {
        GameObject block = _resetPointBlock.Dequeue();
        if (block != null)
          Destroy(block);
      }
    }
  }
  /// <summary>
  /// GameOverUI 활성화
  /// </summary>
  private void OnGameOverUI()
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>OnGameOverUI</color> - GameOver UI 활성화");
    _gameOverUI.SetActive(true);
    if (GameManager.Instance.gameState == PlayState.SinglePlay || GameManager.Instance.gameState == PlayState.MultiPlay)
      pointBar.SetActive(true);
  }
  /// <summary>
  /// Main씬으로 이동
  /// </summary>
  private void ChangeMainScene()
  {
    ///// 수정
    MainManager.Instance?.LoadMainSceneWithUI("Main Menu"); // <<<<<<<<<<<<<<
  }

  /// <summary>
  /// 승패 포인트 Block 생성
  /// </summary>
  /// <param name="point">승급 포인트</param>
  public void SpawnBlocks(int point)
  {
    GameLogger.Log($"<color=#4CAF50>GameOver</color>: <color=#F44336>SpawnBlocks</color> - 승패 Point UI 블럭 소환");

    if (point == 0) return;

    int start = point > 0 ? 1 : -1;

    int end = point / 10;

    for (int i = start; end > 0 ? i <= end : i >= end; i += end > 0 ? 1 : -1)
    {
      float x = GetXPosFromPoint(i, pointBlockParent.rect.width);

      GameObject block = Instantiate(pointBlock, pointBlockParent);
      RectTransform rect = block.GetComponent<RectTransform>();
      rect.anchoredPosition = new Vector2(x, 0);
      block.SetActive(true);

      // // 가로 길이(width)를 x*2로 변경
      // Vector2 size = rect.sizeDelta;
      // size.x = pointBlockParent.rect.width/ 6f;
      // rect.sizeDelta = size;

      _resetPointBlock.Enqueue(block);
    }
  }
  /// <summary>
  /// 승패 포인트 Block 생성 위치. (UI 대응)
  /// </summary>
  /// <param name="point">승급 포인트</param>
  /// <param name="parentWidth">포인트 블럭 생성 위치 UI의 가로 넓이</param>
  /// <returns></returns>
  private float GetXPosFromPoint(int point, float parentWidth)
  {
    point = point > 0 ? 2 * point - 1 : 2 * point + 1;
    return point * (parentWidth / 12f);
  }
}