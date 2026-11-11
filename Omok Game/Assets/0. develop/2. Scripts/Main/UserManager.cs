using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class UserManager : Singleton<UserManager> // UserData를 관리하기 때문에 싱글톤으로 제작
{
  private enum PasswordState { EmptyPassword, InCorrect, ExactMatchPassword }

  private UserData _currentUserData;

  private bool _isSignin = false;

  public void SetIsSignin(bool isSginin)
  {
    _isSignin = isSginin;
  }

  public bool GetIsSignin()
  {
    return _isSignin;
  }
  
  public UserData _CurrentUserData
  {
    get => _currentUserData;
    set
    {
      _currentUserData = value;

      if (_currentUserData != null)
      {
        if (_currentUserData.reviewDataList == null)
        {
          _currentUserData.reviewDataList = new List<ReviewData>();
        }
      }
      else
      {
        GameLogger.LogWarning("UserData가 null 이기 때문에 reviewDataList에 접근 할 수 없습니다.");
      }
    }
  }

  // 매직 스트링
  private const string LAST_UPDATA_KEY = "LastUserData";

  #region 회원가입 함수

  // 회원가입시 중복, 다른 경우에 안내메시지가 들어간 Task<String>값을 반환하는 함수
  public async Task<string> ValidateRegistration(UserData newUser, string confirmPassword)
  {
    // 1. ID 자리수 체크 6~16자
    // 자리수 체크 후 있으면 자리수 체크 안내메시지 반환 후 함수 종료
    if (!IsUserIDRequirement(newUser.userID))
      return "아이디를 6 ~ 16자 이내로 해주세요.";

    // 2. ID 특수문자 체크
    // 주어진 문자열 (newUser.userID)이 뒤의 패턴과 일치하는 부분이 있으면 true 반환
    // @: C# 기능, 이스케이프 처리 하지 않고 문자열 그대로 해석 (\ : \d 등등)
    // [^A-Za-z0-9_]: 영문자(대/소), 숫자, _(언더바) 가 아닌 문자 1개와 일치하는지 (^: 부정, -: 범위)
    // 그래서 허용하지 않은 공백, 특수문자가 포함되어있으면 true 반환
    if (Regex.IsMatch(newUser.userID, @"[^A-Za-z0-9_]"))
      return "아이디에 특수문자가 들어갈 수 없습니다.";

    // 3. ID 중복 체크
    // ID 중복 체크 함수 호출 한 뒤, 중복이면 안내메시지 반환 후 함수 종료
    if (await DBManager.Instance.CheckUserAsync(newUser.userID, DataType.ID))
      return "이미 등록된 아이디 입니다.";

    // 4. 비밀번호 비어있는지, 일치하는지 체크
    // 비밀번호 공백인지 체크, 비밀번호 확인과 서로 일치하는지 체크
    // 각 항목에 문제가 있으면 해당 항목에 해당하는 안내메시지 반환 후 함수 종료
    switch (CheckPassword(newUser.password, confirmPassword))
    {
      case PasswordState.EmptyPassword:
        return "비밀번호를 입력해주세요.";
      case PasswordState.InCorrect:
        return "비밀번호가 일치하지 않습니다.";
    }

    // 5. 닉네임 중복 체크
    if (await DBManager.Instance.CheckUserAsync(newUser.nickname, DataType.NICK)) // 닉네임 중복이 있을 경우 닉네임 중복 안내 enum 값 반환
      return "이미 등록된 닉네임 입니다.";

    // 조건 다 통과시 실행
    await DBManager.Instance.JoinListAsync(newUser.userID, newUser.password, newUser.nickname);

    return "SignUp Success";  // 성공적으로 회원가입 됐다고 enum 값 반환
  }

  // ID 자리수 체크 함수
  private bool IsUserIDRequirement(string userID)
  {
    if (userID.Length < 6 || userID.Length > 16)
      return false;

    return true;
  }

  // 비밀번호 공백 체크 + 비밀번호 서로 같은지 체크 함수
  private PasswordState CheckPassword(string password, string confirmPassword)
  {
    if (password == "" || confirmPassword == "")
      return PasswordState.EmptyPassword;
    else if (password != confirmPassword)
      return PasswordState.InCorrect;

    return PasswordState.ExactMatchPassword;
  }

  #endregion

  // 현재 유저 코인 증가
  public void AddCoin(int add)
  {
    _CurrentUserData.coin += add;
  }

  // 현재 유저 코인 감소
  public void DeductCoin(int deduct)
  {
    _CurrentUserData.coin -= deduct;
  }

  // 비상 저장시 비동기는 안 될 수 있으므로 로컬 저장하는 함수
  public void LocalBackupUserData()
  {
    try
    {
      if (_CurrentUserData == null)
      {
        GameLogger.Log("저장할 사용자 데이터가 Null 입니다.");
        return;
      }

      string json = JsonUtility.ToJson(_CurrentUserData);

      if (string.IsNullOrEmpty(json))
      {
        GameLogger.Log("Json 변환 실패");
        return;
      }

      PlayerPrefs.SetString(LAST_UPDATA_KEY, json);
      PlayerPrefs.Save();

      GameLogger.Log($"로컬 백업 완료 : {_CurrentUserData.userID}");
    }
    catch (Exception e)
    {
      GameLogger.LogError($"로컬 백업 실패 : {e.Message}");
    }
  }

  // 클라이언트가 켜지면 자동으로 로컬 저장된 UserData를 DB에 저장하는 함수
  public async Task PushLocalUserDatatoRemote()
  {
    if (PlayerPrefs.HasKey(LAST_UPDATA_KEY))
    {
      var localDataJson = PlayerPrefs.GetString(LAST_UPDATA_KEY, "");

      if (string.IsNullOrEmpty(localDataJson))
        return;

      UserData data = JsonUtility.FromJson<UserData>(localDataJson);

      if (string.IsNullOrEmpty(data._id) || data._id.Length != 24)
      {
        data._id = null;
      }

      bool isSaveSuccess = false;

      if (data != null)
        isSaveSuccess = await DBManager.Instance.UpdateUserDataAsync(data);
      else
        GameLogger.Log("로컬에 저장한 UserData가 없어서 생략합니다.");

      if (isSaveSuccess)
      {
        GameLogger.Log("마지막으로 저장한 로컬 데이터를 성공적으로 불러 왔습니다.");
        DeleteLocalBackup();
      }
    }
  }

  // Local에 저장한 UserData를 제거하는 함수
  public void DeleteLocalBackup()
  {
    if (PlayerPrefs.HasKey(LAST_UPDATA_KEY))
    {
      PlayerPrefs.DeleteKey(LAST_UPDATA_KEY);
      PlayerPrefs.Save();
      GameLogger.Log("로컬 백업 UserData 삭제 완료!");
    }
  }

  // UserData를 DB에 저장하는 함수
  public async Task SaveUserData()
  {
    bool isSaveSuccess = false;

    if (_CurrentUserData != null)
      isSaveSuccess = await DBManager.Instance.UpdateUserDataAsync(_CurrentUserData);
    else
      GameLogger.Log("UserManager가 들고있는 로그인 한 유저의 UserData가 null 값입니다.");

    if (isSaveSuccess)
    {
      GameLogger.Log("UserData 저장 성공");

      // 현재 UserData는 null로 초기화
      _CurrentUserData = null;
    }
  }
}