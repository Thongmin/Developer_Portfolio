using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayState
{
    /// <summary>
    /// vs AI
    /// </summary>
    SinglePlay,
    /// <summary>
    /// vs Local Player
    /// </summary>
    DualPlay,
    /// <summary>
    /// vs Server Player
    /// </summary>
    MultiPlay,
    /// <summary>
    /// Review Mode
    /// </summary>
    Review
}
/// <summary>
/// �ش� �÷��̾ ���� �÷��̾�����, �İ� �÷��̾�����, AI����, ��Ƽ�÷��̾����� ��Ÿ���� ������
/// 
/// \AI�Ǵ� ��Ƽ�� ���
/// (�浹:AI, �鵹:Player),
/// (�浹:PlayerA, �鵹:AI)
/// �� ���� ������ ���� ��ǻ�Ϳ��� �÷��� ������ ���� ��ġ�� ��ü��
/// </summary>
public enum PlayerState
{
    PlayerA,
    PlayerB,
    AI,
    MultiPlayer
}

public class GameLogic : MonoBehaviour
{
    [SerializeField] Button decideButton;
    [SerializeField] Ruler ruler;
    [SerializeField] GameOver gameOver;
    [SerializeField] GameObject playUIs;
    [SerializeField] GameObject reviewUIs;
    [SerializeField] AISelectionPopup aiSeletionPopup;
    [SerializeField] BoardUIController _boardUICon;
    BasePlayerState firstPlayer;
    BasePlayerState secondPlayer;
    BasePlayerState currentPlayer;

    public Board board;
    public Action<bool> OnTurnChanged;

    IEnumerator Start()
    {
        yield return null;
        //GameManager.Instance.gameState = PlayState.Review; //�׽�Ʈ��
        if (GameManager.Instance.gameState != PlayState.Review)
        {
            GameManager.Instance.currentReviewData.gameState = GameManager.Instance.gameState;
            GameManager.Instance.currentReviewData.reviewKey = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            GameManager.Instance.currentReviewData.winnerPlayer = PlayerState.AI;
            GameManager.Instance.currentReviewData.reviewMoves = new List<Point>();
            GameManager.Instance.currentReviewData.playerColor = "-";
        }
        Init();
    }

    private void Init()
    {
        GameLogger.Log(GameManager.Instance.gameState);
        playUIs.SetActive(true);
        reviewUIs.SetActive(false);


        switch (GameManager.Instance.gameState)
        {

            case PlayState.SinglePlay:
                SelectAIs(); //���� �İ� ���� �˾�
                break;
            case PlayState.DualPlay:
                firstPlayer = new NormalPlayerState(true, PlayerState.PlayerA, decideButton);
                secondPlayer = new NormalPlayerState(false, PlayerState.PlayerB, decideButton);
                _boardUICon.InitProfiles(true, "Black",null ,0);
                _boardUICon.InitProfiles(false, "White", null, 1);

                GameManager.Instance.currentReviewData.playerColor = "";
                ChangeTurn(false);
                break;
            case PlayState.MultiPlay:
                //�̱���
                firstPlayer = new NormalPlayerState(true, PlayerState.PlayerA, decideButton);
                secondPlayer = new MultiPlayerState(false);
                ChangeTurn(false);
                break;
            case PlayState.Review:
                playUIs.SetActive(false);
                reviewUIs.SetActive(true);
                reviewUIs.GetComponent<ReviewSystem>()?.Init(this, ruler);
                break;
        }
    }
    private void SelectAIs()
    {
        aiSeletionPopup.OpenAISeletion(DecideFirst);
    }
    private void DecideFirst(bool isFirst,int aiLevel)
    {
        Debug.Log($"first:{isFirst},AI Level:{aiLevel}");
        if (isFirst)
        {
            firstPlayer = new NormalPlayerState(true, PlayerState.PlayerA, decideButton);
            GameManager.Instance.currentReviewData.playerColor = "흑";
            secondPlayer = new AIPlayerState(false,aiLevel);
        }
        else
        {
            firstPlayer = new AIPlayerState(true, aiLevel);
            secondPlayer = new NormalPlayerState(false, PlayerState.PlayerB, decideButton);
            GameManager.Instance.currentReviewData.playerColor = "백";

        }
        _boardUICon.InitProfiles(isFirst, UserManager.Instance._CurrentUserData.nickname,GameManager.Instance.userProflie, UnityEngine.Random.Range(0, 6));
        _boardUICon.InitProfiles(!isFirst, "AI",null ,6);

        ChangeTurn(false);
    }
    public void ChangeBoard(int row, int col, Constants.PlayerType state)
    {
        board.ChangeBlockState(row, col, state);
    }
    public bool CheckBlockState(int row, int col, Constants.PlayerType state)
    {
        return board.CheckBlockState(row, col, state);
    }
    public bool DecideLastClickBlock(Constants.PlayerType state)
    {
        return board.DecideLastClickBlock(state);
    }
    public void ChangeTurn(bool wasFirst)
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnExit(this);
            //�¸�üũ
            if (!ruler.StartCheckWinner())
            {
                ruler.StartRenjuRules();
            }
            else
            {
                //���� �ʿ�
                // gameOver.SetGameOverUI(wasFirst ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB);
                _boardUICon.StopTimer();
                gameOver.SetGameOver(currentPlayer.GetPlayerState());
                GameManager.Instance.currentReviewData.winnerPlayer = currentPlayer.GetPlayerState();
                UserManager.Instance._CurrentUserData.reviewDataList
                                    .Add(GameManager.Instance.currentReviewData);
                DBManager.Instance.UpdateUserDataAsync(UserManager.Instance._CurrentUserData);
                GameLogger.Log($"데이터 삽입 : {GameManager.Instance.currentReviewData}");
                GameManager.Instance.currentReviewData = new ReviewData();
                return;
            }

        }
        OnTurnChanged?.Invoke(wasFirst);
        currentPlayer = !wasFirst ? firstPlayer : secondPlayer;
        currentPlayer?.OnEnter(this);

    }

    public void TimeOver()
    {
        if (currentPlayer == firstPlayer)
        {
            gameOver.SetGameOver(secondPlayer.GetPlayerState());
            // gameOver.SetGameOverUI(Constants.PlayerType.PlayerB);

        }
        else
        {
            gameOver.SetGameOver(firstPlayer.GetPlayerState());
            // gameOver.SetGameOverUI(Constants.PlayerType.PlayerA);
        }
        OmokAI.RequestAiStop();
    }
    public Constants.PlayerType[,] GetBoardData()
    {
        Constants.PlayerType[,] board = new Constants.PlayerType[15, 15];
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                board[i, j] = this.board.blocks[i, j].State;
            }
        }
        return board;
    }
    
}
