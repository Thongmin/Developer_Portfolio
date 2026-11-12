using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayMenuController : MonoBehaviour
{
  [SerializeField] private TMP_Text _playerNameAndRankTMP;
  [SerializeField] private TMP_Text _userCoinTMP;
  [SerializeField] private GameObject noDataText;

  [SerializeField] private Button oriReviewBt;
  [SerializeField] private Transform content;

  [SerializeField] private TMP_Text _winLoseRecordTMP;

  private UserData userData;
  
  private void Start()
  {
    // MainManager에 자신을 등록
    if (MainManager.Instance)
      MainManager.Instance.SetReviewer(this);
     UserProfile.OnProfileImageChanged += OnProfileImageChanged;

    }

    [SerializeField] private Image _profileImage;                       // 프로필 버튼 이미지 변수
  [SerializeField] private Sprite _defaultButtonSprite;               // 기본 버튼 이미지 Sprite


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
    userData = UserManager.Instance._CurrentUserData; // UserManager 클래스에서 현재 유저데이터를 가져와서 해당 변수에 초기화

    _playerNameAndRankTMP.text = userData.ranked + "급 " + userData.nickname; // 유저 정보를 프로필 TextUI에 초기화
    _userCoinTMP.text = "코인 : " + userData.coin;  // 유저 정보를 코인 TextUI에 초기화
    
    var winRate = (float)userData.win / (float)userData.lose;

    if (winRate == 1f)
      winRate = 0.5f;

    winRate = winRate * 100f;

    _winLoseRecordTMP.text = $"승 : {userData.win} / 패 : {userData.lose} / 승률 : {winRate.ToString("F2")}%";

    UserProfile.Instance.LoadUserProfile(UserManager.Instance._CurrentUserData.userID);
  }

  /// <summary>
  /// 버튼 복제 함수
  /// </summary>
  public void ButtonSet()
  {
    var dataList = userData.reviewDataList;
    bool isDataNull = dataList.Count == 0;
    
    noDataText.SetActive(isDataNull);
    
    // 저장된 기보 데이터 없음
    if (isDataNull)
      return;

    // 추가된 기보 데이터 없음
    if (content.childCount == dataList.Count)
      return;

    // 추가 생성만큼만 생성
    for (int i = content.childCount; i < dataList.Count; i++)
    {
      ReviewButton temp = Instantiate(oriReviewBt, content).GetComponent<ReviewButton>();
      
      // 데이터 넘겨주기
      temp.myData = dataList[i];
      temp.TextSet();
    }
  }

  // TODO: 내가 둔 게임들 기보 리스트 및 기보 한 것 보여주기.
  void OnDestroy()
  {
    UserProfile.OnProfileImageChanged -= OnProfileImageChanged;
  }
}
