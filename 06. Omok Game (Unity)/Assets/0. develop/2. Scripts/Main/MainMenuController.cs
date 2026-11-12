using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
  [SerializeField] private TMP_Text _playerNameAndRankTMP;  // 유저 프로필 정보 Text 출력 TMP_Text 변수 
  [SerializeField] private TMP_Text _userCoinTMP; // 유저 코인 정보 Text 출력 TMP_Text 변수

  [SerializeField] private Image _profileImage;                       // 프로필 버튼 이미지 변수
  [SerializeField] private Sprite _defaultButtonSprite;               // 기본 버튼 이미지 Sprite

  [SerializeField] private TMP_Text _winLoseRecordTMP;

  void Start()
  {
    UserProfile.OnProfileImageChanged += OnProfileImageChanged;
  }

  // 대국하기 버튼 함수
  public void OnPlayMenuButton()
  {
    MainManager.Instance.ShowPlayModeMenuUI();  // PlayMenu UI 전환 함수 호출
  }

  // 내 기보 버튼 함수
  public void OnReplayButton()
  {
    MainManager.Instance.ShowReplayMenu();  // Replay UI 전환 함수 호출
  }

  // 랭킹 버튼 함수
  public void OnRankingButton()
  {
    // TODO: 현재 사용자들 랭크 표시
  }

  // 상점 버튼 함수
  public void OnStoreMenuButton()
  {
    // TODO: 코인을 이용한 상점에서 구매 목록
  }

  // 설정 버튼 함수
  public void OnSettingButton()
  {
    // TODO: 설정 기능
  }

  // 로그아웃 버튼 함수
  public async void OnLogoutButton()
  {
    UserManager.Instance.SetIsSignin(false);    // isSignin 변수를 false로 초기화
    await UserManager.Instance.SaveUserData();  // DB에 저장
    UserProfile.Instance.InitProfile();         // CurrentProfile 초기화
    MainManager.Instance.Init();                // 플레이 타임, 로그인 체크 변수 초기화
    UserManager.Instance.DeleteLocalBackup();   // 혹여나 Local에 UserData가 저장되어있으면 삭제
    MainManager.Instance.ShowSigninUI();        // 초기 로그인 화면으로 돌아가도록 로그인 UI 함수 호출
  }

  // 프로필 사진을 UI에 뿌리는 이벤트 함수
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
        _profileImage.sprite = null;
        _profileImage.color = Color.gray;
      }
    }
        GameManager.Instance.userProflie = _profileImage.sprite;
  }

  // 프로필 업데이트 함수
  public void ProfileUpdate()
  {
    var userData = UserManager.Instance._CurrentUserData; // UserManager 클래스에서 현재 유저데이터를 가져와서 해당 변수에 초기화

    _playerNameAndRankTMP.text = userData.ranked + "급 " + userData.nickname; // 유저 정보를 프로필 TextUI에 초기화
    _userCoinTMP.text = "코인 : " + userData.coin;  // 유저 정보를 코인 TextUI에 초기화

    var winRate = (float)userData.win / (float)userData.lose;

    if (winRate == 1f)
      winRate = 0.5f;

    winRate = winRate * 100f;

    _winLoseRecordTMP.text = $"승 : {userData.win} / 패 : {userData.lose} / 승률 : {winRate.ToString("F2")}%";

    UserProfile.Instance.LoadUserProfile(UserManager.Instance._CurrentUserData.userID);
  }

  void OnDestroy()
  {
    UserProfile.OnProfileImageChanged -= OnProfileImageChanged;
  }
}