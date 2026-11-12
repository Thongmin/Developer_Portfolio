using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SigninController : MonoBehaviour
{
  #region Inspector Variable

  [SerializeField] private TMP_InputField _idInputField;        // ID InputField
  [SerializeField] private TMP_InputField _passwordInputField;  // Password InputField
  [SerializeField] private Image _hideImage;                    // Password 숨김/보이기 버튼 이미지
  [SerializeField] private Sprite _hideSprite;                  // 숨김/보이기 버튼 이미지에 넣을 숨김 Sprite이미지 파일
  [SerializeField] private Sprite _showSprite;                  // 숨김/보이기 버튼 이미지에 넣을 보이기 Sprite이미지 파일
  [SerializeField] private GameObject _invalidEntry;            // 안내메시지 띄워주는 TextUI GameObject
  [SerializeField] private Toggle saveInfoTg;                   // 유저 정보 저장 토글
  [SerializeField] private Toggle autoLoginTg;                  // 자동 로그인 토글

  #endregion

  #region Field Variable

  // PlayerPrefs에 로그인 정보를 저장하고 불러올 때 사용할 키값
  private const string LOGIN_ID_KEY = "Login Id";
  private const string LOGIN_PW_KEY = "Login PW";
  private const string SAVE_INFO_KEY = "Save Info";
  private const string AUTO_LOGIN_KEY = "Auto Login";

  private string loginID, loginPW;

  private TMP_Text _invalidEntryTMP;                            // 안내메시지 띄워주는 TextUI의 TMP_Text Component
  private bool _isHide = false;                                 // 현재 비밀번호가 보이는지 숨기는지 체크하는 변수

  #endregion

  void Start()
  {
    _invalidEntryTMP = _invalidEntry.GetComponent<TMP_Text>();  // TMP_Text Component 넣음.
    _invalidEntryTMP.text = "";

    LoadData();
    LinkToListener();
    LoadSigninInfo();
    StartCoroutine(AutoLoginWhenReady());
  }

  // 비밀번호 숨김/보이기 버튼 함수
  public void OnHidePasswordButton()
  {
    _isHide = !_isHide; // 누르자마자 bool값 바꿔줌.

    if (_isHide)  // 비밀번호를 보이게 할 때
    {
      _hideImage.sprite = _showSprite;                                        // 보이는 이미지 넣는다.
      _passwordInputField.contentType = TMP_InputField.ContentType.Standard;  // InputField의 ContentType을 Standard로 변경
      _passwordInputField.inputType = TMP_InputField.InputType.Standard;      // InputField의 InputType을 Standard로 변경
    }
    else  // 비밀번호를 보이지 않게 할 때
    {
      _hideImage.sprite = _hideSprite;                                        // 숨김 이미지 넣는다.
      _passwordInputField.contentType = TMP_InputField.ContentType.Password;  // InputField의 ContentType을 Password로 변경
      _passwordInputField.inputType = TMP_InputField.InputType.Password;      // InputField의 InputType을 Password로 변경
    }

    _passwordInputField.ForceLabelUpdate(); // UI에 표시되도록 Update
  }

  #region Setting Method

  void LoadData()
  {
    // PlayerPrefsHelper에서 저장된 토글값 불러오기
    // 저장된 값이 없다면(최초 실행 시) false
    saveInfoTg.isOn = PlayerPrefsHelper.GetBool(SAVE_INFO_KEY, false);
    autoLoginTg.isOn = PlayerPrefsHelper.GetBool(AUTO_LOGIN_KEY, false);

    // PlayerPrefs에 로그인 정보가 저장돼있다면 불러오기 (없으면 빈 문자열)
    loginID = PlayerPrefs.GetString(LOGIN_ID_KEY, string.Empty);
    loginPW = PlayerPrefs.GetString(LOGIN_PW_KEY, string.Empty);
  }

  /// <summary>
  /// 각 버튼에 이벤트 리스너를 등록
  /// </summary>
  void LinkToListener()
  {
    // 저장하는 기능 한 줄만 연결하는 경우
    // autoLoginTg.onValueChanged.AddListener(value => PlayerPrefsHelper.SetBool(AUTO_LOGIN_KEY, value));

    autoLoginTg.onValueChanged.AddListener(value => SaveToggleValue(AUTO_LOGIN_KEY, value));
    saveInfoTg.onValueChanged.AddListener(value => SaveToggleValue(SAVE_INFO_KEY, value));
  }

  /// <summary>
  /// 토글 값 변경 시 호출될 이벤트 핸들러 (현재는 저장 기능만)
  /// 저장 이외의 기능이 추가될 장기적 관점에서는 별도 메서드 방식이 더 좋다
  /// </summary>
  void SaveToggleValue(string key, bool value)
  {
    PlayerPrefsHelper.SetBool(key, value);
  }

  // UI가 전환 되었을때 InppuField와 Text(안내메시지) 초기화
  private void ClearInputField()
  {
    // string 값이므로 "" 값으로 초기화
    _idInputField.text = "";
    _passwordInputField.text = "";
    _invalidEntryTMP.text = "";
  }

  #endregion

  #region Sign-in

  /// <summary>
  /// 디바이스(PlayerPrefs)에 저장된 로그인 정보를 불러와 InputField에 표시
  /// </summary>
  void LoadSigninInfo()
  {
    if (!saveInfoTg.isOn)
      return;

    _idInputField.text = loginID;
    _passwordInputField.text = loginPW;
  }

  // 로그인 버튼 함수
  public async void OnSigninButton()
  {
    // InputField의 텍스트 값을 각 변수에 초기화
    var id = _idInputField.text;
    var password = _passwordInputField.text;

    _invalidEntry.SetActive(true);  // 안내 메시지 TextUI 활성화

    UserData curData = await DBManager.Instance.LoginAsync(id, password);

    if (curData != null)
    {
      UserManager.Instance.SetIsSignin(true);

      // 로그인 성공 시, "아이디 저장", "자동 로그인" 등의 기능을 위해 디바이스에 정보 저장
      PlayerPrefs.SetString(LOGIN_ID_KEY, curData.userID);
      PlayerPrefs.SetString(LOGIN_PW_KEY, curData.password);

      UserManager.Instance._CurrentUserData = curData; // 로그인 한 사람의 UserData를 현재 UserData를 관리하는 변수에 저장
      _invalidEntry.SetActive(false);         // 안내 메시지 TextUI 비활성화
      ClearInputField();                      // Text 초기화 함수 호출
      MainManager.Instance.ShowMainMenuUI();  // 로그인 후 GameMenu UI 활성화 함수 호출
      MainManager.Instance.isSignin = true;   // 로그인 체크하는 bool값 변수 true 로 설정

      /////ProfileUpdate();
    }
    else
    {
      _invalidEntryTMP.text = autoLoginTg.isOn ? "아이디 또는 비밀번호를 입력해주세요."
                              : "아이디 또는 비밀번호가 일치하지 않습니다.";
    }
  }

  IEnumerator AutoLoginWhenReady()
  {
    // PersonalManager가 초기화될 때까지 기다림
    /* 추가 설명
    WaitUntil() : 람다식의 최종 결과가 'true'가 될 때까지 기다림
    yield return new WaitUntil(() => !PersonalManager.Instance.IsInitialized);
    -> 람다식의 최종 결과가 'true' --> IsInitialized가 거짓이 될 때까지 기다림

    반대 역할 WaitWhile
    WaitWhile(() => 조건) : 조건이 true인 동안 계속 기다림 -> false가 되면 실행
    */
    yield return new WaitUntil(() => DBManager.Instance.IsInitialized);

    AutoSignIn();
  }

  void AutoSignIn()
  {
    if (!UserManager.Instance.GetIsSignin())
    {
      if (!autoLoginTg.isOn)
        return;

      OnSigninButton();
    }
  }

  #endregion
  
  // 회원가입 버튼 함수
  public void OnSignupButton()
  {
    _invalidEntry.SetActive(false);       // 안내메시지 비활성화
    ClearInputField();                    // Text 초기화 함수 호출
    MainManager.Instance.ShowSignupUI();  // 회원가입 화면으로 전환 해줄 MainManager의 ShowSignupUI() 함수 호출
  }
}
