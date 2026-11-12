using System;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Networking;

public class UserProfile : Singleton<UserProfile>
{
  // 이벤트 - UI 업데이트용
  public static event Action<Texture2D> OnProfileImageChanged;

  // 저장 경로
  private string SavePath => Path.Combine(Application.persistentDataPath, "ProfileImages");
  private string OriginalPath => Path.Combine(SavePath, "Original");
  private string ProcessedPath => Path.Combine(SavePath, "Processed");

  // 현재 프로필 데이터
  private Texture2D currentProfileTexture;
  
  // 회원가입용 임시 데이터
  private Texture2D tempProfileTexture; // 임시 프로필 이미지
  private string tempOriginalPath; // 임시 원본 경로
  private bool hasSelectedProfile = false; // 프로필 선택 여부

  private SignupController signupController;

  void Start()
  {
    Initialize();
  }

  void Initialize()
  {
    // 회원가입 스크립트 가져오기
    signupController = GetComponent<SignupController>();

    // 폴더 생성
    Directory.CreateDirectory(SavePath);
    Directory.CreateDirectory(OriginalPath);
    Directory.CreateDirectory(ProcessedPath);

    // File Browser 설정
    FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".jpeg", ".png"));

    // // 기존 프로필 로드 (로그인된 사용자가 있을 때만)
    // LoadSavedProfile();
  }

  // 현재 프로필 초기화
  public void InitProfile()
  {
    currentProfileTexture = null;
    OnProfileImageChanged?.Invoke(null);
  }

  // ===== 회원가입용 프로필 이미지 선택 (저장하지 않고 미리보기만) =====
  public void SelectProfileImageForSignup()
  {
    StartCoroutine(SelectImageForSignupCoroutine());
  }

  IEnumerator SelectImageForSignupCoroutine()
  {
    yield return FileBrowser.WaitForLoadDialog(
        FileBrowser.PickMode.Files,
        false,
        null,
        null,
        "Select Profile Image",
        "Select"
    );

    if (FileBrowser.Success && FileBrowser.Result.Length > 0)
    {
      yield return LoadImageForPreview(FileBrowser.Result[0]);
    }
  }

  // ===== 이미지 미리보기 로드 (저장하지 않음) =====
  IEnumerator LoadImageForPreview(string path)
  {
    Debug.Log($"이미지 로드 시작: {path}");
    
    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path))
    {
      yield return www.SendWebRequest();

      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Failed to load image: {www.error}");
        yield break;
      }

      var original = DownloadHandlerTexture.GetContent(www);
      Debug.Log($"원본 이미지 로드 성공: {original.width}x{original.height}");

      // 가공 (170x170 정사각형)
      var processed = ProcessTexture(original, 170);
      Debug.Log($"이미지 가공 완료: {processed.width}x{processed.height}");

      // 임시 데이터로 저장 (파일로 저장하지 않음)
      tempProfileTexture = processed;
      tempOriginalPath = path;
      hasSelectedProfile = true;

      // UI 업데이트 이벤트 발생 (미리보기)
      OnProfileImageChanged?.Invoke(processed);

      // 메모리 정리
      if (original != processed) Destroy(original);

      Debug.Log("프로필 이미지 선택 완료 (미리보기)");
    }
  }

  // ===== 회원가입 완료 시 프로필 저장 =====
  public void SaveProfileOnSignupComplete(string userID)
  {
    if (!hasSelectedProfile || tempProfileTexture == null)
    {
      Debug.Log("선택된 프로필 이미지가 없어 저장하지 않습니다.");
      return;
    }

    try
    {
      // 1. 원본 저장
      SaveOriginalWithUserID(userID, tempOriginalPath);

      // 2. 가공본 저장
      SaveProcessedWithUserID(userID, tempProfileTexture);

      // 3. 현재 프로필로 설정
      currentProfileTexture = tempProfileTexture;

      // 4. 임시 데이터 정리
      ClearTempData();

      Debug.Log($"프로필 이미지 저장 완료: {userID}");
    }
    catch (Exception e)
    {
      Debug.LogError($"프로필 저장 실패: {e.Message}");
      ClearTempData();
    }
  }

  // ===== 임시 데이터 정리 =====
  private void ClearTempData()
  {
    tempProfileTexture = null;
    tempOriginalPath = "";
    hasSelectedProfile = false;
  }

  // ===== 회원가입 취소 시 임시 데이터 정리 =====
  public void CancelProfileSelection()
  {
    if (tempProfileTexture != null)
    {
      Destroy(tempProfileTexture);
    }
    
    ClearTempData();
    OnProfileImageChanged?.Invoke(null);
    
    Debug.Log("프로필 선택 취소");
  }

  // ===== 기존 프로필 이미지 선택 (즉시 저장) =====
  public void SelectProfileImage()
  {
    StartCoroutine(SelectImageCoroutine());
  }

  IEnumerator SelectImageCoroutine()
  {
    yield return FileBrowser.WaitForLoadDialog(
        FileBrowser.PickMode.Files,
        false,
        null,
        null,
        "Select Profile Image",
        "Select"
    );

    if (FileBrowser.Success && FileBrowser.Result.Length > 0)
    {
      yield return ProcessImage(FileBrowser.Result[0]);
    }
  }

  // ===== 이미지 처리 (즉시 저장) =====
  IEnumerator ProcessImage(string path)
  {
    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path))
    {
      yield return www.SendWebRequest();

      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Failed to load image: {www.error}");
        yield break;
      }

      var original = DownloadHandlerTexture.GetContent(www);

      // 현재 사용자 ID 가져오기
      string userID = GetCurrentUserID();

      // 2. 원본 저장
      SaveOriginalWithUserID(userID, path);

      // 3. 가공 (170x170 정사각형)
      var processed = ProcessTexture(original, 170);

      // 4. 가공본 저장
      SaveProcessedWithUserID(userID, processed);

      // 5. 현재 텍스처 업데이트
      currentProfileTexture = processed;

      // UI 업데이트 이벤트 발생
      OnProfileImageChanged?.Invoke(processed);

      // 메모리 정리
      if (original != processed) Destroy(original);
    }
  }

  // ===== 텍스처 가공 (정사각형 크롭 + 리사이즈) =====
  Texture2D ProcessTexture(Texture2D source, int targetSize)
  {
    // 1. 정사각형 크롭
    int size = Mathf.Min(source.width, source.height);
    int x = (source.width - size) / 2;
    int y = (source.height - size) / 2;

    Color[] pixels = source.GetPixels(x, y, size, size);

    // 2. 임시 정사각형 텍스처
    Texture2D square = new Texture2D(size, size);
    square.SetPixels(pixels);
    square.Apply();

    // 3. 170x170으로 리사이즈
    RenderTexture rt = RenderTexture.GetTemporary(targetSize, targetSize);
    Graphics.Blit(square, rt);

    RenderTexture previous = RenderTexture.active;
    RenderTexture.active = rt;

    Texture2D result = new Texture2D(targetSize, targetSize);
    result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
    result.Apply();

    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(rt);
    Destroy(square);

    return result;
  }

  // ===== 원본 저장 (userID 사용) =====
  void SaveOriginalWithUserID(string userID, string sourcePath)
  {
    string extension = Path.GetExtension(sourcePath);
    string fileName = $"{userID}_original{extension}";
    string savePath = Path.Combine(OriginalPath, fileName);

    // 기존 파일이 있다면 삭제
    if (File.Exists(savePath))
    {
      File.Delete(savePath);
    }

    // 파일 복사
    if (File.Exists(sourcePath))
    {
      File.Copy(sourcePath, savePath, true);
      Debug.Log($"Original saved: {savePath}");
    }
  }

  // ===== 가공본 저장 (userID 사용) =====
  void SaveProcessedWithUserID(string userID, Texture2D texture)
  {
    string fileName = $"{userID}_processed.png";
    string savePath = Path.Combine(ProcessedPath, fileName);

    // 기존 파일이 있다면 삭제
    if (File.Exists(savePath))
    {
      File.Delete(savePath);
    }

    byte[] data = texture.EncodeToPNG();
    File.WriteAllBytes(savePath, data);

    // PlayerPrefs에 경로 저장
    PlayerPrefs.SetString($"ProfilePath_{userID}", savePath);
    PlayerPrefs.Save();

    Debug.Log($"Processed saved: {savePath}");
  }

  // ===== 현재 사용자 ID 가져오기 =====
  private string GetCurrentUserID()
  {
    // UserManager가 있다면 현재 사용자 ID 사용
    if (UserManager.Instance?._CurrentUserData != null)
    {
      return UserManager.Instance._CurrentUserData.userID;
    }
    return "default";
  }

  // ===== 저장된 프로필 로드 =====
  void LoadSavedProfile()
  {
    string userID = GetCurrentUserID();
    string savedPath = PlayerPrefs.GetString($"ProfilePath_{userID}", "");

    if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
    {
      byte[] data = File.ReadAllBytes(savedPath);
      Texture2D texture = new Texture2D(170, 170);

      if (texture.LoadImage(data))
      {
        currentProfileTexture = texture;
        OnProfileImageChanged?.Invoke(texture);
      }
    }
  }

  // ===== 특정 사용자 프로필 로드 =====
  public void LoadUserProfile(string userID)
  {
    string savedPath = PlayerPrefs.GetString($"ProfilePath_{userID}", "");

    if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
    {
      byte[] data = File.ReadAllBytes(savedPath);
      Texture2D texture = new Texture2D(170, 170);

      if (texture.LoadImage(data))
      {
        currentProfileTexture = texture;
        OnProfileImageChanged?.Invoke(texture);
        Debug.Log($"사용자 프로필 로드 완료: {userID}");
      }
    }
    else
    {
      // 프로필이 없으면 기본 상태로
      currentProfileTexture = null;
      OnProfileImageChanged?.Invoke(null);
    }
  }

  // ===== 프로필 제거 =====
  public void RemoveProfile()
  {
    string userID = GetCurrentUserID();
    RemoveUserProfile(userID);
  }

  public void RemoveUserProfile(string userID)
  {
    currentProfileTexture = null;

    PlayerPrefs.DeleteKey($"ProfilePath_{userID}");
    PlayerPrefs.Save();

    // 파일 삭제
    string[] originalFiles = Directory.GetFiles(OriginalPath, $"{userID}_*");
    string[] processedFiles = Directory.GetFiles(ProcessedPath, $"{userID}_*");

    foreach (string file in originalFiles)
    {
      File.Delete(file);
    }

    foreach (string file in processedFiles)
    {
      File.Delete(file);
    }

    OnProfileImageChanged?.Invoke(null);
    Debug.Log($"프로필 삭제 완료: {userID}");
  }

  // ===== Getter =====
  public Texture2D GetCurrentProfile() => currentProfileTexture;
  public Texture2D GetTempProfile() => tempProfileTexture;
  public bool HasSelectedProfile() => hasSelectedProfile;
  
  // 프로필 이미지 경로 가져오기
  public string GetProfileImagePath(string userID)
  {
    return PlayerPrefs.GetString($"ProfilePath_{userID}", "");
  }
  
  // 현재 사용자 프로필 경로
  public string GetCurrentProfilePath()
  {
    string userID = GetCurrentUserID();
    return GetProfileImagePath(userID);
  }
  
  // 저장 폴더 경로들
  public string GetSavePath() => SavePath;
  public string GetProcessedPath() => ProcessedPath;
  public string GetOriginalPath() => OriginalPath;
}
