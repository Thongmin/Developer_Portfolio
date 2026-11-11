using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMenuController : MonoBehaviour
{
  [SerializeField] private TMP_Text _profileTMP;  // 유저 프로필 정보 Text 출력 TMP_Text 변수
  [SerializeField] private TMP_Text _userCoinTMP; // 유저 코인 정보 Text 출력 TMP_Text 변수

  private UserData _currentUserData;  // 현재 유저 데이터를 가지고 있는 변수

  public void OnPlayModeButton(int type)
  {
    GameManager.Instance.gameState = (PlayState)type;
    
    SceneManager.LoadScene(1);
  }

  // 뒤로가기 버튼 함수
  public void OnBackButton()
  {
    ///////////////////////////////////////////////////////// 수정
    MainManager.Instance.ShowMainMenuUI();  // 게임메뉴로 되돌아가기 위해서 게임메뉴UI 보여주는 함수 호출
  }

  // 프로필 업데이트 함수
  public void ProfileUpdate()
  {
    _currentUserData = UserManager.Instance._CurrentUserData; // UserManager 클래스에서 현재 유저데이터를 가져와서 해당 변수에 초기화

    _profileTMP.text = _currentUserData.ranked + "급 " + _currentUserData.nickname; // 유저 정보를 프로필 TextUI에 초기화
    _userCoinTMP.text = "코인 : " + _currentUserData.coin;  // 유저 정보를 코인 TextUI에 초기화

    // TODO: 프로필 사진 추가
  }
}
