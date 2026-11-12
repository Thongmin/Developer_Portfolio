using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Lobby : MonoBehaviour
{
    #region Inspector Variable

    [SerializeField]
    TMP_InputField idInput;
    [SerializeField]
    TMP_InputField pwInput;
    
    [SerializeField]
    Button loginBt, signInBt;
    
    [SerializeField]
    Toggle saveInfoTg, autoLoginTg;

    #endregion

    #region Field Variable

    // PlayerPrefs에 로그인 정보를 저장하고 불러올 때 사용할 키값
    private const string LOGIN_ID_KEY = "Login Id";
    private const string LOGIN_PW_KEY = "Login PW";
    private const string SAVE_INFO_KEY = "Save Info";
    private const string AUTO_LOGIN_KEY = "Auto Login";

    private string loginID, loginPW;
    
    #endregion
    
    private void Start()
    {
        LoadData();
        
        LinkToListener();
        LoadLoginInfo();
        StartCoroutine(AutoLoginWhenReady());
    }

    #region Init Method

    /// <summary>
    /// 각 버튼에 이벤트 리스너를 등록
    /// </summary>
    void LinkToListener()
    {
        loginBt.onClick.AddListener(LoginLogic);
        signInBt.onClick.AddListener(OnClickSignIn);

        // 저장하는 기능 한 줄만 연결하는 경우
        // autoLoginTg.onValueChanged.AddListener(value => PlayerPrefsHelper.SetBool(AUTO_LOGIN_KEY, value));
        
        autoLoginTg.onValueChanged.AddListener(value => SaveToggleValue(AUTO_LOGIN_KEY, value));
        saveInfoTg.onValueChanged.AddListener(value => SaveToggleValue(SAVE_INFO_KEY, value));
    }

    #endregion

    #region Data Method
    
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
    /// 디바이스(PlayerPrefs)에 저장된 로그인 정보를 불러와 InputField에 표시
    /// </summary>
    void LoadLoginInfo()
    {
        if (!saveInfoTg.isOn)
            return;
        
        idInput.text = loginID;
        pwInput.text = loginPW;
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
        
        AutoLogin();
    }
    
    void AutoLogin()
    {
        if (!autoLoginTg.isOn)
            return;
        
        LoginLogic();
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// 로그인 버튼 클릭 시 호출되는 이벤트 핸들러
    /// </summary>
    private async void LoginLogic()
    {
        // 입력 필드가 유효한지 확인 후 id와 pw에 값을 받아온다
        if (!InputFieldHelper(out string id, out string pw))
            return;

        // DB 관리자가 초기화 되었는지 확인
        if (!DBManager.Instance.IsInitialized)
        {
            GameLogger.LogWarning("Database is not ready. Please wait a moment");
            return;
        }

        // DB에 로그인을 비동기적으로 요청하고 결과를 기다린다
        var user = await DBManager.Instance.LoginAsync(id, pw);
        
        // 로그인 결과에 따라 다른 메세지를 출력
        if (user != null)
        {
            GameLogger.Log($"Login Success!\nUser ID : {user.userID}");
            
            // 로그인 성공 시, "아이디 저장", "자동 로그인" 기능 등을 위해 디바이스에 정보 저장
            PlayerPrefs.SetString(LOGIN_ID_KEY, id);
            PlayerPrefs.SetString(LOGIN_PW_KEY, pw);
            
            // TODO : 로그인 성공 후 작업 추가 (씬 전환)
        }
        else
        {
            GameLogger.LogWarning("Login Failed. Please check your ID and Password");
        }
    }
    
    /// <summary>
    /// 회원가입 버튼 클릭 시 호출되는 이벤트 핸들러
    /// </summary>
    private async void OnClickSignIn()
    {
        if (!CanProceedWithAction(out string id, out string pw))
            return;

        // bool isTaken = await PersonalManager.Instance.CheckUserAsync(id);
        //
        // if (isTaken)
        // {
        //     GameLogger.LogWarning($"Sign-in failed. The Id '{id}' is already taken");
        //     return;
        // }

        ///////////////////////////////////////////////////////// 닉네임 받아오기 해야 함
        await DBManager.Instance.JoinListAsync(id, pw, "");
        
        GameLogger.Log("Sing-in request sent. If Successful, you can now log in.");
        
        // TODO : 회원가입 후 실행될 작업 추가
    }
    
    /// <summary>
    /// 토글 값 변경 시 호출될 이벤트 핸들러 (현재는 저장 기능만)
    /// 저장 이외의 기능이 추가될 장기적 관점에서는 별도 메서드 방식이 더 좋다
    /// </summary>
    void SaveToggleValue(string key, bool value)
    {
        PlayerPrefsHelper.SetBool(key, value);
    }

    #endregion
    
    #region Check Method

    /// <summary>
    /// DB 관련 작업을 수행하기 전, 사용자 입력과 DB 초기화 상태를 모두 확인
    /// </summary>
    /// <returns>모든 조건 충족 시 반환</returns>
    bool CanProceedWithAction(out string id, out string pw)
    {
        // 입력 필드가 유효한지 확인 후 id와 pw에 값을 받아온다
        if (!InputFieldHelper(out id, out pw))
            return false;
        
        // DB 관리자가 초기화 되었는지 확인
        if (!DBManager.Instance.IsInitialized)
        {
            GameLogger.LogWarning("Database is not ready. Please wait a moment");
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// ID와 비밀번호 InputField에 값이 모두 입력되었는지 확인하는 헬퍼 메서드
    /// </summary>
    /// <param name="id">입력된 ID를 반환받을 변수</param>
    /// <param name="pw">입력된 비밀번호를 반환받을 변수</param>
    /// <returns>두 필드가 모두 비어있지 않으면 true</returns>
    bool InputFieldHelper(out string id, out string pw)
    {
        id = idInput.text;
        pw = pwInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            GameLogger.LogWarning("ID와 비밀번호를 모두 입력해주세요!");
            return false;
        }

        return true;
    }

    #endregion
}
