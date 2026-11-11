using UnityEngine;

public class MultiPlayerState : BasePlayerState
{
    //¹Ì±¸Çö
    public MultiPlayerState(bool isFirst) : base(isFirst, PlayerState.MultiPlayer)
    {
    }
    public override void OnEnter(GameLogic gameLogic)
    {
    }

    public override void OnExit(GameLogic gameLogic)
    {
    }

    protected override void HandleMove(GameLogic gameLogic, int row, int col)
    {
    }
}
