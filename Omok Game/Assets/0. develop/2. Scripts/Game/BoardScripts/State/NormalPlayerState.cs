using UnityEngine.UI;
using UnityEngine;

public class NormalPlayerState : BasePlayerState
{
    Button decideButton;
    public NormalPlayerState(bool isFirst,PlayerState state,Button button) : base(isFirst,state)
    {
        decideButton = button;
    }


    public override void OnEnter(GameLogic gameLogic)
    {
        decideButton.onClick.AddListener(() => HandleMove(gameLogic, 0, 0));
    }

    public override void OnExit(GameLogic gameLogic)
    {
        decideButton.onClick.RemoveAllListeners();
    }
    protected override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        int temp = stateNum;
        if (gameLogic.DecideLastClickBlock((Constants.PlayerType)temp)){

            Process_Move(gameLogic, row, col, true);
        }
    }
}
