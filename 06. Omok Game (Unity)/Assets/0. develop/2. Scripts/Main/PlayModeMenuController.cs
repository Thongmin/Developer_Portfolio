using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayModeMenuController : MonoBehaviour
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

  public void OnPlayModeButton(int type)
  {
    GameManager.Instance.gameState = (PlayState)type;

    MainManager.Instance.LoadGameScene();
  }

  // 뒤로가기 버튼 함수
  public void OnBackButton()
  {
    MainManager.Instance.ShowMainMenuUI();  // 게임메뉴로 되돌아가기 위해서 게임메뉴UI 보여주는 함수 호출
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
