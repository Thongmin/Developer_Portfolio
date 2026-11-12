using UnityEngine;
using System.Threading.Tasks;
public class AIPlayerState : BasePlayerState
{
    Constants.PlayerType[,] board;
    Constants.PlayerType aiPlayerType;
    int aiLevel;
    public AIPlayerState(bool isFirst, int aiLevel=1) : base(isFirst, PlayerState.AI)
    {
        board = new Constants.PlayerType[15, 15];
        this.aiLevel = aiLevel<4?aiLevel:3;
    }
    public override async void OnEnter(GameLogic gameLogic)
    {
        aiPlayerType = isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        board = gameLogic.GetBoardData();

        var bestMove = await Task.Run(() =>
            OmokAI.GetBestMove(board, aiPlayerType, aiLevel)
        );
        if (bestMove.HasValue)
        {
            HandleMove(gameLogic, bestMove.Value.row, bestMove.Value.col);
        }
    }

    public override void OnExit(GameLogic gameLogic)
    {
    }

    protected override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        Process_Move(gameLogic, row, col, false);
    }

}
