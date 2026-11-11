using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
  // 매직 스트링
  // MainScene의 각 UI들 이름
  private const string MAIN_MENU = "Main Menu";
  private const string PLAY_MODE_MENU = "Play Mode Menu";
  private const string REPLAY_MENU = "Replay Menu";
  private const string SIGNIN = "Sign in";
  private const string SIGNUP = "Sign up";
  private const string RANKING = "Ranking";
  private const string SETTING = "Setting";
  private const string SHOP = "Shop";

  // 클래스 자체를 Inspector에 보이도록 해서 UI의 이름과 CanvasGroup 컨포넌트를 가진 UI를 추가
  [System.Serializable]
  public class UIPanel
  {
    public string panelName;
    public GameObject uiObject;
  }

  // 위 클래스를 배열로 만들어서 여러개 등록 가능하게 함.
  [SerializeField] private UIPanel[] _uiPanels;

  // 키값과 Value값을 한 쌍으로 저장하는 자료구조
  private Dictionary<string, GameObject> _panelDictionary;

  private void Awake()
  {
    // UI Object들이 있는 List의 값들을 키값과 매칭되도록 하는 Dictinary에 저장
    _panelDictionary = new Dictionary<string, GameObject>();
    foreach (UIPanel panel in _uiPanels)
    {
      _panelDictionary[panel.panelName] = panel.uiObject;
    }
  }

  private void Start()
  {
    // UI 초기화
    InitializeUI();

    // MainManager에 자신을 등록
    if (MainManager.Instance != null)
    {
      MainManager.Instance.SetUIManager(this);
    }
    else
    {
      ShowPanel("Main Menu");
    }
  }

  // 통합 UI 초기화 함수 (최초 1회)
  private void InitializeUI()
  {
    ActiveAllPanels();
    HideAllPanels();
  }

  /// <summary>
  /// uiName에 해당하는 CanvasGroup의 값을 변경하여 보여주는 함수
  /// </summary>
  /// <param name="uiName">UI이름</param>
  public void ShowPanel(string uiName)
  {
    // CanvasGroup.TryGetValue(string(키값), out value)
    // string 키 값을 받아와 해당하는 키값의 Value 값이 있으면 true, 없으면 false를 반환
    if (_panelDictionary.TryGetValue(uiName, out GameObject panel))
    {
      CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();  // 해당 UI Object에 들어있는 CanvasGroup을 변수에 저장

      canvasGroup.alpha = 1f;            // 화면에 보여지는 것을 결정하는 변수 (0: 안보임, 1: 보임)
      canvasGroup.interactable = true;   // 그룹내 버튼, 슬라이드, 토글 등 UI 상호작용 관련 변수 (true: 상호작용 됨, false: 상호작용 안됨)
      canvasGroup.blocksRaycasts = true; // 그룹내 Raycast 활성화 관련 변수 (true: Raycast가 막힘, false: Raycast가 통과됨)
    }
    else
    {
      GameLogger.Log("{uiName}에 해당하는 UI 객체가 없습니다.");
    }
  }

  /// <summary>
  /// uiName에 해당하는 CanvasGroup의 값을 변경하여 숨기는 함수
  /// </summary>
  /// <param name="uiName">UI이름</param>
  public void HidePanel(string uiName)
  {
    if (_panelDictionary.TryGetValue(uiName, out GameObject panel))
    {
      CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

      canvasGroup.alpha = 0f;
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }
    else
    {
      GameLogger.Log("{uiName}에 해당하는 UI 객체가 없습니다.");
    }
  }

  // 배열에 들어있는 모든 UI의 CanvasGroup의 값을 숨김처리 하는 함수
  public void HideAllPanels()
  {
    foreach (var panel in _panelDictionary.Values)
    {
      CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

      canvasGroup.alpha = 0f;
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }
  }

  // 모든 UI 오브젝트 다 활성화 하는 함수 (최초 1회)
  public void ActiveAllPanels()
  {
    foreach (var panel in _uiPanels)
    {
      panel.uiObject.SetActive(true);
    }
  }

  // UI Text에 현재 코인 업데이트 하는 함수
  public void UpdateCoinText(string uiName)
  {
    if (uiName != "")
    {
      if (_panelDictionary.TryGetValue(uiName, out GameObject panel))
      {
        var text = panel.GetComponentInChildren<TMP_Text>();

        text.text = "코인 : " + UserManager.Instance._CurrentUserData.coin;
      }
    }
  }

  #region UI 전환시 프로필 업데이트 하는 함수들

  // Main Menu UI로 전환시 프로필 업데이트
  public void MainMenuUpdateProfile()
  {
    if (_panelDictionary.TryGetValue(MAIN_MENU, out GameObject panel))
      panel.GetComponent<MainMenuController>().ProfileUpdate();
  }

  // Play Mode Menu로 전환시 프로필 업데이트
  public void PlayModeMenuUpdateProfile()
  {
    if (_panelDictionary.TryGetValue(PLAY_MODE_MENU, out GameObject panel))
      panel.GetComponent<PlayModeMenuController>().ProfileUpdate();
  }

  // Replay Menu로 전환시 프로필 업데이트
  public void ReplayMenuUpdateProfile()
  {
    if (_panelDictionary.TryGetValue(REPLAY_MENU, out GameObject panel))
      panel.GetComponent<ReplayMenuController>().ProfileUpdate();
  }

  #endregion
}
