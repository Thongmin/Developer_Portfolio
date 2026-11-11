using Unity.InferenceEngine;
using UnityEngine;

public class Ruler : MonoBehaviour
{
    [SerializeField] Board _board;
    private GameOver _gameOver;
    
    void Start()
    {
        _gameOver = FindFirstObjectByType<GameOver>();
    }
    private (int, int)[] directions = new (int, int)[]
    {
        (0,1), (1,0), (1,1),(1,-1)
    };

    public bool CheckWinner()
    {

        GameLogger.Log("<color=#1976D2>LHW</color>_Ruller.CheckWinner: 승패 확인");
        var blocks = _board.blocks;

        int rows = blocks.GetLength(0);
        int cols = blocks.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var state = blocks[row, col].State;

                if (state == Constants.PlayerType.PlayerA || state == Constants.PlayerType.PlayerB)
                {
                    foreach (var dir in directions)
                    {
                        if (CheckWinnerDirection(blocks, row, col, dir.Item1, dir.Item2, state))
                        {
                            //EndGame(state);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    private bool CheckWinnerDirection(Block[,] blocks, int row, int col, int dRow, int dCol, Constants.PlayerType state)
    {
        int rows = blocks.GetLength(0);
        int cols = blocks.GetLength(1);

        for (int i = 1; i < 5; i++)
        {
            int newRow = row + i * dRow;
            int newCol = col + i * dCol;

            if (newRow < 0 || newRow >= rows ||
                newCol < 0 || newCol >= cols)
            {
                return false;
            }
            if (blocks[newRow, newCol].State != state)
            {
                return false;
            }
        }
        return true;
    }
    private void RenjuRule_OverLineThreeFour()
    {
        var blocks = _board.blocks;

        int rows = blocks.GetLength(0);
        int cols = blocks.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (blocks[row, col].State == Constants.PlayerType.Cant)
                    blocks[row, col].State = Constants.PlayerType.None;
                    
                if (blocks[row, col].State == Constants.PlayerType.None)
                {
                    blocks[row, col].State = Constants.PlayerType.PlayerA;

                    int openThreeCount = 0;
                    int openFourCount = 0;
                    int openOverLineCount = 0;

                    foreach (var dir in directions)
                    {
                        string line = ExtractLine(blocks, row, col, dir.Item1, dir.Item2, 4);

                        if (IsOpenThree((line))) openThreeCount++;
                    }
                    foreach (var dir in directions)
                    {
                        string line = ExtractLine(blocks, row, col, dir.Item1, dir.Item2, 5);

                        if (IsOpenFour(line)) openFourCount++;
                        if (IsOpenOverLine(line)) openOverLineCount++;
                    }

                    blocks[row, col].State = Constants.PlayerType.None;

                    if (openThreeCount >= 2 || openFourCount >= 2 || openOverLineCount >= 1)
                    {
                        GameLogger.Log($"<color=#1976D2>LHW</color>_Ruller.RenjuRule_ThreeFour: 삼삼 또는 사사 확인, 위치: 가로:{row}, 세로{col}");
                        blocks[row, col].State = Constants.PlayerType.Cant;
                    }
                }
            }
        }

    }
    private string ExtractLine(Block[,] blocks, int row, int col, int dRow, int dCol, int length)
    {
        int rows = blocks.GetLength(0);
        int cols = blocks.GetLength(1);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int startRow = row - dRow * length;
        int startCol = col - dCol * length;

        for (int i = 0; i < length * 2 + 2; i++)
        {
            int newRow = startRow + dRow * i;
            int newCol = startCol + dCol * i;

            if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols)
            {
                sb.Append("2"); // 보드 밖
            }
            else
            {
                switch (blocks[newRow, newCol].State)
                {
                    case Constants.PlayerType.None: sb.Append("0"); break;
                    case Constants.PlayerType.PlayerA: sb.Append("1"); break;
                    default: sb.Append("2"); break; // Player2, Cant → 막힘
                }
            }
        }
        return sb.ToString();
    }

    // 열린3 패턴 검사
    private bool IsOpenThree(string line)
    {
        return line.Contains("01110") ||
            line.Contains("010110") ||
            line.Contains("011010");
    }

    // 열린4 패턴 검사
    private bool IsOpenFour(string line)
    {
        return line.Contains("011110") ||
            line.Contains("0101110") ||
            line.Contains("0111010") ||
            line.Contains("0110110");
    }
    private bool IsOpenOverLine(string line)
    {
        return line.Contains("111111");
    }

    private void RenjuRules()
    {
        GameLogger.Log("<color=#1976D2>LHW</color>_Ruller.RenjuRules: 렌주룰 확인 시작");
        RenjuRule_OverLineThreeFour();
    }
    private void EndGame(Constants.PlayerType winner)
    {
        GameLogger.Log($"<color=#1976D2>LHW</color>_Ruller.EndGame: 게임 종료, 우승자: {winner}");

        // GameOverUI 활성화
        //_gameOver.SetGameOverUI(winner);
    }
    
    public void StartRenjuRules()
    {
        RenjuRules();
    }

    public bool StartCheckWinner()
    {
        GameLogger.Log("<color=#1976D2>LHW</color>_Ruller.StartRuller: Ruller 시작");
        return CheckWinner();
    }
    

}