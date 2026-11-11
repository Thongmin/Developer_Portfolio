using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public enum DataType {ID, PW, NICK}

[System.Serializable]
public class UserData
{
  // 서버 저장용 ObjectId
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string _id;
  // 유저의 정보만 관리하는 클래스
  public string userID;           // 아이디
  public string password;         // 비밀번호
  public string nickname;         // 닉네임
  public string profileImageName; // 프로필 사진 이름 or 경로
  public int ranked;              // 급수
  public int win;                 // 승리 수
  public int lose;                // 패배 수
  public int point;               // 승급 포인트
  public int coin;                // 코인
  
  public List<ReviewData> reviewDataList;
  
  // 급수, 코인은 데이터가 없으면 (회원가입한 유저) 디폴트 값.
  public UserData(string userID, string password, string nickname, int ranked = 30,  int coin = 100, string profileImageName = "",int win = 0, int lose = 0, int point = 0)
  {
    this.userID = userID;
    this.password = password;
    this.nickname = nickname;
    this.ranked = ranked;
    this.win = win;
    this.lose = lose;
    this.point = point;
    this.profileImageName = profileImageName;
    this.coin = coin;
  }
}

[System.Serializable]
public class Point
{
  public int x;
  public int y;

  public Point(int _x, int _y)
  {
    x = _x;
    y = _y;
  }
}

[System.Serializable]
public class ReviewData
{
  public string reviewKey;
  public List<Point> reviewMoves = new List<Point>();
  public PlayState gameState;
  public PlayerState winnerPlayer;
    public string playerColor;
}