using UnityEngine;

public abstract class BasePlayerState
{
    protected bool isFirstPlayer;
    protected int stateNum;
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);

    protected abstract void HandleMove(GameLogic gameLogic, int row, int col); //마커 표식

    protected PlayerState playerState;
    public BasePlayerState(bool isFirstPlayer, PlayerState state)
    {
        this.isFirstPlayer = isFirstPlayer;
        stateNum = isFirstPlayer ? 1 : 2;
        this.playerState = state;
    }


    //게임 결과 처리
    protected void Process_Move(GameLogic gameLogic
        , int row, int col, bool isNomal)
    {
        if (gameLogic.CheckBlockState(row, col, (Constants.PlayerType)stateNum))
        {
            if (!isNomal)
            {
                gameLogic.ChangeBoard(row, col, (Constants.PlayerType)stateNum);
                GameManager.Instance.currentReviewData.reviewMoves.Add(new Point(row, col));

            }
            gameLogic.ChangeTurn(isFirstPlayer);

        }

    }
    public PlayerState GetPlayerState()
    {
        return playerState;
    }
}
