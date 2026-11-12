using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class DBManager : Singleton<DBManager>
{
  #region Field Variable

  /// <summary>
  /// MongoDB의 'users' 컬렉션에[ 대한 CRUD(생성, 읽기, 수정, 삭제) 작업을 수행하는 핵심 객체
  /// 제네릭으로 <Personal> 타입을 지정하여, DB의 BSON 문서를 C#의 Personal 클래스로 자동 변환해주는 역할
  /// </summary>
  private IMongoCollection<UserData> _personalCollection;

  /// <summary>
  /// DB가 성공적으로 초기화(연결)되었는지 여부
  /// 외부에서 DB 작업 요청 전에 이 값을 확인하여 안정성 확보
  /// </summary>
  public bool IsInitialized { get; private set; }

  #endregion

  protected override void Awake()
  {
    base.Awake();
    InitializeDatabaseAsync();
  }

  #region Init

  /// <summary>
  /// MongoDB에 비동기적으로 연결하고 초기화를 수행
  /// </summary>
  async void InitializeDatabaseAsync()
  {
    if (IsInitialized) return;

    try
    {
      // 로컬 MongoDB 서버에 연결하기 위한 연결 문자열
      string connectionString = "mongodb://localhost:27017/";
      // MongoClient를 생성 후 서버에 연결
      var client = new MongoClient(connectionString);
      // DB 가져오기
      var db = client.GetDatabase("Omok-18");
      // DB 내부의 users 컬렉션 가져와 Personal 클래스와 매핑
      _personalCollection = db.GetCollection<UserData>("UserDatas");

      // 'ping' 명령을 보내 서버에 성공적으로 연결되었는지 테스트
      // 응답이 올 때까지 await 키워드로 비동기 대기
      // {ping: <아무값>}인데 그냥 1 넣음
      await db.RunCommandAsync((Command<BsonDocument>)"{ping: 1}");

      IsInitialized = true;
      // DB가 성공적으로 연결되면 Local에 저장한 UserData를 불러와 업데이트
      GameLogger.Log("Database connection successful!");
    }
    catch (Exception e)
    {
      // 연결 과정에서 예외가 발생하면 초기화 상태를 false로 하고 에러 로그 발생
      IsInitialized = false;
      GameLogger.LogError($"Database connection failed : {e.Message}");
    }
  }
  
  #endregion

  #region User Account Management

  /// <summary>
  /// 새로운 유저 정보를 받아 DB에 비동기적으로 저장 (회원가입)
  /// </summary>
  /// <param name="id">가입할 유저의 ID</param>
  /// <param name="pw">가입할 유저의 비밀번호</param>
  /// <param name="nickname">가입할 유저의 닉네임</param>
  public async Task JoinListAsync(string id, string pw, string nickname)
  {
    // DB가 초기화 되지 않았다면 작업 중지
    if (!IsInitialized)
    {
      GameLogger.Log("Database is not initialized yet");
      return;
    }

    UserData data = new UserData(id, pw, nickname, 30, 100, UserProfile.Instance.GetSavePath());

    try
    {
      // 'users' 컬렉션에 새로운 유저 데이터를 비동기적으로 삽입
      // C# 드라이버가 personal.oid 필드에 자동으로 ObjectId를 생성하여 할당
      await _personalCollection.InsertOneAsync(data);

      GameLogger.Log($"User '{id}' successfully joined!");
    }
    catch (Exception e)
    {
      // 에러 타입 예시 : 중복된 ID, 네트워크 문제
      GameLogger.LogError($"Failed to join user. Error : {e.Message}");
    }
  }

  /// <summary>
  /// ID와 비밀번호로 유저를 찾아 Personal 객체를 비동기적으로 반환 (로그인)
  /// </summary>
  /// <returns>찾은 Personal 객체. 없으면 null 반환</returns>
  public async Task<UserData> LoginAsync(string id, string password)
  {
    #region Filter 방식

    //// Magic String 발생 가능성 높음
    //var filter = Builders<Personal>.Filter.Eq("ID", id) & Builders<Personal>.Filter.Eq("Password", password);
    //return await _personalCollection.Find(filter).FirstOrDefaultAsync();

    #endregion

    var predicate = ByIdAndPassword(id, password);
    
    await UserManager.Instance.PushLocalUserDatatoRemote();
    
    return await _personalCollection.AsQueryable().FirstOrDefaultAsync(predicate);

    //return await _personalCollection.AsQueryable().FirstOrDefaultAsync(predicate);
  }

  /// <summary>
  /// ID가 DB에 존재하는지 여부를 비동기적으로 확인
  /// </summary>
  /// <returns>유저의 존재 여부를 반환</returns>
  public async Task<bool> CheckUserAsync(string id, DataType type)
  {
    if (!IsInitialized) return false;

    // LoginAsync를 비동기 호출하여 유저 객체의 존재 여부를 확인
    var predicate = ByType(id, type);

    return await _personalCollection.AsQueryable().AnyAsync(predicate);
  }

  /// <summary>
  /// UserData 업데이트 함수
  /// </summary>
  /// <param name="updateData">업데이트할 UserData</param>
  /// <returns></returns>
  public async Task<bool> UpdateUserDataAsync(UserData updateData)
  {
    if (!IsInitialized)
    {
      GameLogger.Log("Database is not initialized yet.");
      return false;
    }

    try
    {
      if (string.IsNullOrEmpty(updateData._id))
      {
        // 새 문서
        updateData._id = ObjectId.GenerateNewId().ToString();
        await _personalCollection.InsertOneAsync(updateData);
      }
      else
      {
        var filter = Builders<UserData>.Filter.Eq(u => u._id, updateData._id);
        var result = await _personalCollection.ReplaceOneAsync(filter, updateData);

        if (result.MatchedCount == 0)
        {
          await _personalCollection.InsertOneAsync(updateData);
        }
      }

      // if (string.IsNullOrEmpty(updateData._id))
      // {
      //   updateData._id = ObjectId.GenerateNewId().ToString();

      //   var filter = Builders<UserData>.Filter.Eq(u => u.userID, updateData.userID);
      //   var option = new ReplaceOptions { IsUpsert = true };

      //   await _personalCollection.ReplaceOneAsync(filter, updateData, option);
      // }
      // else
      // {
      //   var filter = Builders<UserData>.Filter.Eq(u => u._id, updateData._id);
      //   var result = await _personalCollection.ReplaceOneAsync(filter, updateData);

      //   if (result.MatchedCount == 0)
      //   {
      //     GameLogger.LogWarning("업데이트할 문서를 찾을 수 없어 새로 생성합니다.");
      //     await _personalCollection.InsertOneAsync(updateData);
      //   }
      // }

      return true;
    }
    catch (Exception e)
    {
      GameLogger.LogError($"Failed to update UserData. Error Message : {e.Message}");

      return false;
    }
  }

  #endregion

  #region Filter Method

  Expression<Func<UserData, bool>> ByType(string data, DataType type)
  {
    switch (type)
    {
      case DataType.ID:
        return p => p.userID == data;
      case DataType.PW:
        return p => p.password == data;
      case DataType.NICK:
        return p => p.nickname == data;
    }

    return null;
  }

  /// <summary>
  /// DB에서 특정 사용자를 찾기 위한 쿼리 조건(조건 람다식)을 생성
  /// 이 조건은 C# 코드가 DB 쿼리로 변환될 수 있도록 Expression 트리 형태로 정의
  /// </summary>
  /// <returns>ID와 비밀번호가 일치하는 사용자를 찾는 쿼리 조건</returns>
  Expression<Func<UserData, bool>> ByIdAndPassword(string id, string pw)
  {
    return p => p.userID == id && p.password == pw;
  }

  /// <summary>
  /// ID가 일치하는 사용자를 찾기 위한 쿼리 조건
  /// </summary>
  Expression<Func<UserData, bool>> ById(string id)
  {
    return p => p.userID == id;
  }


  /// <summary>
  /// 닉네임이 일치하는 사용자를 찾기 위한 쿼리 조건
  /// </summary>
  Expression<Func<UserData, bool>> ByNick(string nickname)
  {
    return p => p.nickname == nickname;
  }

  #endregion
}