using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : Singleton<MainManager>
{
  // 매직 스트링
  // MainScene의 각 UI들 이름
  private const string MAIN_MENU = "Main Menu";
  private const string PLAY_MODE_MENU = "Play Mode Menu";
  private const string REPLAY_MENU = "Replay Menu";
  private const string SIGN_IN = "Sign in";
  private const string SIGN_UP = "Sign up";
  private const string RANKING = "Ranking";
  private const string SETTING = "Setting";
  private const string SHOP = "Shop";

  private UIManager _currentUIManager;  // UIManager를 변수로 선언
  [SerializeField]
  private ReplayMenuController _replayMenu;

  private float _gamePlayTime = 0f;      // 로그인 이후로 플레이 하고 있는 시간
  private string _pendingUIname = "";   // 이 다음에 보여줄 UI 이름을 가지고 있는 변수

  public bool isSignin = false;         // 로그인 했는지 체크

  private string _currentUIName = "";  // 현재 보여주고 있는 UI의 이름 가지고 있는 변수

  void Update()
  {
    // 로그인 했으면 시간 누적 (true)
    if (isSignin)
    {
      _gamePlayTime += Time.deltaTime;  // 보정된 시간을 누적함.

      if (_gamePlayTime >= 10f)  // 10초가 지났을때 실행
      {
        UserManager.Instance.AddCoin(1);    // 10초당 1코인 증가

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
          if (_currentUIName == MAIN_MENU || _currentUIName == PLAY_MODE_MENU || _currentUIName == REPLAY_MENU)
            _currentUIManager.UpdateCoinText(_currentUIName); // UI에 초기화

          UserManager.Instance.LocalBackupUserData(); // 10초마다 자동저장
        }

        _gamePlayTime = 0f;
      }
    }
  }

  // 변수 초기화 함수
  public void Init()
  {
    _gamePlayTime = 0;
    isSignin = false;
  }

  // UIManager 변수에 현재 UIManager를 넣음.
  // (로그인 하거나 GameScene에서 MainScene으로 넘어올 때 1회 실행)
  public void SetUIManager(UIManager uiManager)
  {
    _currentUIManager = uiManager;  // 현재 UImanager를 받음.

    if (!string.IsNullOrEmpty(_pendingUIname))  // 보여줄 UI가 있으면 실행
    {
      _currentUIManager.HideAllPanels();
      _currentUIManager.ShowPanel(_pendingUIname);
      _pendingUIname = "";  // 보여줬기 때문에 초기화
    }
    else
    {
      ShowSigninUI();
    }
  }

  public void SetReviewer(ReplayMenuController reviewer)
  {
    _replayMenu = reviewer;  // 리뷰어
    GameLogger.Log("reviewer 등록 완료");
  }

  #region 외부에서 UI 전환을 위해서 호출하는 함수들

  // 로그인 UI 호출 함수
  public void ShowSigninUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(SIGN_IN);
    _currentUIName = SIGN_IN;
  }

  // 회원가입 UI 호출 함수
  public void ShowSignupUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(SIGN_UP);
    _currentUIName = SIGN_UP;
  }

  // 메인메뉴 UI 호출 함수
  public void ShowMainMenuUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(MAIN_MENU);
    _currentUIName = MAIN_MENU;
    _currentUIManager.MainMenuUpdateProfile();
  }

  // 대국하기 UI 호출 함수
  public void ShowPlayModeMenuUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(PLAY_MODE_MENU);
    _currentUIName = PLAY_MODE_MENU;
    _currentUIManager.PlayModeMenuUpdateProfile();
  }

  // 내 기보 UI 호출 함수
  public void ShowReplayMenu()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(REPLAY_MENU);
    _currentUIName = REPLAY_MENU;
    _currentUIManager.ReplayMenuUpdateProfile();

    _replayMenu.ButtonSet();
  }

  // 상점 UI 호출 함수
  public void ShowShopMenuUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(SHOP);
    _currentUIName = SHOP;
  }

  // 랭킹 UI 호출 함수
  public void ShowRankingUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(RANKING);
    _currentUIName = RANKING;
  }

  // 설정 UI 호출 함수
  public void ShowSettingsUI()
  {
    _currentUIManager?.HideAllPanels();
    _currentUIManager?.ShowPanel(SETTING);
    _currentUIName = SETTING;
  }

  #endregion

  #region 씬 전환 관련 함수들

  // GameScene -> MainScene 전환시 호출 함수
  public void LoadMainSceneWithUI(string uiName)
  {
    _pendingUIname = uiName;
    SceneManager.LoadScene(0);
    StartCoroutine(StartLater(() =>
    {
      _currentUIManager.MainMenuUpdateProfile();
    }));
    
    GameLogger.LogWarning($"MainScene으로 전환 후 유저 point = {UserManager.Instance._CurrentUserData.point}");
  }

  // MainScene -> GameScene 전환시 호출 함수
  public void LoadGameScene()
  {
    GameLogger.LogWarning($"GameScene으로 전환 전 유저 point = {UserManager.Instance._CurrentUserData.point}");
    _currentUIManager?.HideAllPanels();
    SceneManager.LoadScene(1);
    GameLogger.LogWarning($"GameScene으로 전환 후 유저 point = {UserManager.Instance._CurrentUserData.point}");
  }

  #endregion

  #region 비 정상적 상황시 Data를 로컬에 저장 함수

  void OnApplicationFocus(bool focus)
  {
    if (!focus)
    {
      UserManager.Instance.LocalBackupUserData();
      GameLogger.Log("로컬 저장 완료 (OnApplicationFocus())");
    }
  }

  void OnApplicationPause(bool pause)
  {
    if (pause)
    {
      UserManager.Instance.LocalBackupUserData();
      GameLogger.Log("로컬 저장 완료 (OnApplicationPause())");
    }
  }

  #endregion
  IEnumerator StartLater(Action act)
  {
    yield return null;
    act?.Invoke();
  }
}
