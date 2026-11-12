using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignupController : MonoBehaviour
{
  [SerializeField] private TMP_InputField _idInputField;              // ID InputField
  [SerializeField] private TMP_InputField _nicknameInputField;        // 닉네임 InputField
  [SerializeField] private TMP_InputField _passwordInputField;        // 비밀번호 InputField
  [SerializeField] private TMP_InputField _confirmPasswordInputField; // 비밀번호 확인용 InputField
  [SerializeField] private Image _passwordHideImage;                  // 비밀번호 숨김/보이기 버튼 이미지 변수
  [SerializeField] private Image _confirmPasswordHideImage;           // 비밀번호 확인 숨김/보이기 버튼 이미지 변수
  [SerializeField] private Sprite _hideSprite;                        // 숨김 Sprite 변수
  [SerializeField] private Sprite _showSprite;                        // 보이기 Sprite 변수
  [SerializeField] private GameObject _invalidEntry;                  // 안내메시지 TextUI GameObject 변수

  [SerializeField] private Image _profileImage;                       // 프로필 버튼 이미지 변수
  [SerializeField] private Sprite _defaultButtonSprite;               // 기본 버튼 이미지 Sprite

  private const string SIGNUP_SUCCESSFULL = "SignUp Success";
  private TMP_Text _invalidEntryTMP;                                  // 안내메시지 TextUI의 TMP_Text Component 변수
  private bool _isPasswordHide = false;                               // 비밀번호 보이기 숨김 체크 변수
  private bool _isConfirmPasswordHide = false;                        // 비밀번호 확인용 보이기 숨김 체크 변수


  void Start()
  {
    _invalidEntryTMP = _invalidEntry.GetComponent<TMP_Text>();  // TMP_Text Component로 초기화
    _invalidEntryTMP.text = "";

    if (_profileImage != null)
    {
      _profileImage.sprite = null;
      _profileImage.color = Color.gray;
    }

    UserProfile.OnProfileImageChanged += OnProfileImageChanged;
  }

  // 비밀번호 숨김/보이기 버튼 함수
  public void OnHidePasswordButton()
  {
    _isPasswordHide = !_isPasswordHide; // 버튼 누를 때마다 bool 값을 변경

    if (_isPasswordHide)  // 비밀번호가 보일때
    {
      _passwordHideImage.sprite = _showSprite;
      _passwordInputField.contentType = TMP_InputField.ContentType.Standard;
      _passwordInputField.inputType = TMP_InputField.InputType.Standard;
    }
    else  // 비밀번호가 보이지 않을 때
    {
      _passwordHideImage.sprite = _hideSprite;
      _passwordInputField.contentType = TMP_InputField.ContentType.Password;
      _passwordInputField.inputType = TMP_InputField.InputType.Password;
    }

    _passwordInputField.ForceLabelUpdate();
  }

  public void OnHideConfirmPasswordButton()
  {
    _isConfirmPasswordHide = !_isConfirmPasswordHide; // 버튼 누를 때마다 bool 값을 변경

    if (_isConfirmPasswordHide) // 비밀번호가 보일때
    {
      _confirmPasswordHideImage.sprite = _showSprite; // Sprite 이미지를 보이는 이미지로 변경
      _confirmPasswordInputField.contentType = TMP_InputField.ContentType.Standard; // InputField의 ContentType을 Standard로 변경
      _confirmPasswordInputField.inputType = TMP_InputField.InputType.Standard;     // InputField의 InputType을 Standard로 변경
    }
    else  // 비밀번호가 보이지 않을 때
    {
      _confirmPasswordHideImage.sprite = _hideSprite; // Sprite 이미지를 숨김 이미지로 변경
      _confirmPasswordInputField.contentType = TMP_InputField.ContentType.Password; // InputField의 ContentType을 Password로 변경
      _confirmPasswordInputField.inputType = TMP_InputField.InputType.Password;     // InputField의 InputType을 Password로 변경
    }

    _confirmPasswordInputField.ForceLabelUpdate();  // 바뀐 UI Update
  }

  // 뒤로가기 버튼 함수
  public void OnBackButton()
  {
    _invalidEntry.SetActive(false);       // 안내 메시지 TextUI 비활성화
    ClearInputField();                    // InputField, Text에 있는 값 초기화
    MainManager.Instance.ShowSigninUI();  // 로그인 화면으로 전환
  }


  // 회원가입 완료 버튼 함수
  public async void OnSignupButton()
  {
    _invalidEntry.SetActive(true);  // 안내메시지 TextUI 활성화

    if (_idInputField.text == "")
    {
      _invalidEntryTMP.text = "아이디를 입력해주세요.";
      return;
    }

    if (_nicknameInputField.text == "")
    {
      _invalidEntryTMP.text = "닉네임을 입력해주세요.";
      return;
    }

    if (_passwordInputField.text == "")
    {
      _invalidEntryTMP.text = "비밀번호를 입력해주세요.";
      return;
    }


    UserData userData = new UserData(_idInputField.text, _passwordInputField.text, _nicknameInputField.text); // 현재 입력받은 값들을 새로만든 UserData 클래스 변수에 초기화

    _invalidEntryTMP.text = await UserManager.Instance.ValidateRegistration(userData, _confirmPasswordInputField.text);

    if (_invalidEntryTMP.text == SIGNUP_SUCCESSFULL)
    {
      UserManager.Instance._CurrentUserData = userData;
      UserProfile.Instance.SaveProfileOnSignupComplete(userData.userID);
      UserProfile.Instance.InitProfile();
      _invalidEntry.SetActive(false);       // 안내메시지 TextUI 비활성화
      ClearInputField();                    // InputField, Text 값 초기화
      MainManager.Instance.ShowSigninUI();  // 로그인 화면으로 전환
    }
  }

  // 프로필 설정 함수
  public void OnProfileSettingButton()
  {
    UserProfile.Instance.SelectProfileImageForSignup();
  }

  // Profile 이벤트 등록 함수
  private void OnProfileImageChanged(Texture2D newTexture)
  {
    if (_profileImage != null)
    {
      if (newTexture != null)
      {
        Sprite newSprite = Sprite.Create(
            newTexture,
            new Rect(0, 0, newTexture.width, newTexture.height),
            Vector2.one * 0.5f
        );

        _profileImage.sprite = newSprite;
        _profileImage.color = Color.white;
      }
      else
      {
        // 기본 프로필 이미지 설정
        _profileImage.sprite = _defaultButtonSprite;
        _profileImage.color = Color.gray;
      }
    }
  }

  // UI가 전환 되었을때 InppuField와 Text(안내메시지) 초기화
  private void ClearInputField()
  {
    // string 값이므로 "" 값으로 초기화
    _idInputField.text = "";
    _passwordInputField.text = "";
    _confirmPasswordInputField.text = "";
    _nicknameInputField.text = "";
    _invalidEntryTMP.text = "";
    _profileImage.sprite = _defaultButtonSprite;
    _profileImage.color = Color.gray;
  }

  void OnDestroy()
  {
    UserProfile.OnProfileImageChanged -= OnProfileImageChanged;
  }
}
