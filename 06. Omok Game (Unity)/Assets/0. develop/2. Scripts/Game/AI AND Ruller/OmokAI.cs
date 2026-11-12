using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TODO
/// 1. --탐색 범위 제한--
/// 2. --깊이 제한 조건--
/// 3. --우선순위 정렬--
/// 4. --점수판 평가--
/// 5. --알파 베타 가지치기--
/// 6. --플레이어 순서--
/// 7. 재귀 중단 - 시간제한 조건
/// 8. --렌주룰 적용--
/// 9. --aiLevel--
/// </summary>

public class OmokAI
{
    private const int BOARD_SIZE = 15;  // 보드판 사이즈
    private static int count = 0;       // 전체 재귀 카운트

    private static Constants.PlayerType aiPlayer;
    private static Constants.PlayerType opponentPlayer;
    private static bool isAIFirst;
    private static bool _stopCalculation;

    public static void RequestAiStop()
    {
        _stopCalculation = true;
    }

    private static bool IsEmpty(Constants.PlayerType playerType, Constants.PlayerType player)
    {
        if (player == Constants.PlayerType.PlayerA)         // 자신이 선공이라면
            return playerType == Constants.PlayerType.None; // None만 비어있는 칸으로 인식한다
        else                                                // 후공이라면 None과 Cant 둘 다 빈 칸으로 인식한다.
            return playerType == Constants.PlayerType.None || playerType == Constants.PlayerType.Cant;
    }

    // 현재 상태를 전달하면 다음 최적의 수를 반환
    // 인자 : 타입을 가지고 있는 보드판, ai의 플레이어타입(AorB), ai의 레벨
    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board, Constants.PlayerType _aiPlayer, int aiLevel)
    {
        aiPlayer = _aiPlayer;
        isAIFirst = (aiPlayer == Constants.PlayerType.PlayerA);          // ai가 선공인지 확인
        opponentPlayer = (isAIFirst) ?                                   // 플레이어는 ai가
            Constants.PlayerType.PlayerB : Constants.PlayerType.PlayerA; // 선공이라면 후공, 아니라면 선공

        if (isAIFirst && board[7, 7] == Constants.PlayerType.None)       // ai가 선공이고 보드의 중심점이 비어있다면
            return (7, 7);                                               // ai의 첫 수는 중심점에 둔다.

        int maxDepth = aiLevel + 2;
        _stopCalculation = false;  // 타이머 초기화
        count = 0;

        // 반환할 점수와 포지션 초기값
        float _bestScore = -1000000;
        (int row, int col) movePosition = (-1, -1);

        // 가지치기를 위한 알파-베타 변수
        float _alpha = float.MinValue;
        float _beta = float.MaxValue;

        /// <summary>
        /// 1. 탐색 범위 제한
        /// 2. 깊이0 = 첫 수를 기준으로 점수 평가
        /// 3. 평가된 점수를 정렬
        /// 4. 일정 순위만큼만 MinMax 탐색 실행
        /// 5. 최고 점수로 movePosition 반환
        /// </summary>

        // 이미 놓여진 돌을 기준으로 n칸 범위의 빈 자리를 탐색하여 후보를 제한
        // 각 후보 위치에 대한 우선순위 점수를 저장할 정렬용 리스트
        var scoredMoves = FindScoredMove(board, 2, aiPlayer);
        int maxSerchCount = aiLevel + 2; // 탐색할 최대 순위
        int serchCount = 0;             // 순위 카운터 변수

        Debug.Log("재귀 전 후보 탐색 : " + scoredMoves.Count);

        // 정렬된 후보 리스트로 계산 시작
        foreach (var scoredMove in scoredMoves)
        {
            // 정해진 순위만큼만 반복하고 중단
            if (serchCount >= maxSerchCount)
                break;
            serchCount++;    // 순위 카운터 증가

            // 외부 정지 신호
            if (_stopCalculation)
                throw new OperationCanceledException();

            // 리스트의 좌표값을 row, col로 변환
            int row = scoredMove.position.x;
            int col = scoredMove.position.y;

            if (IsEmpty(board[row, col], aiPlayer))
            {
                var tempBoard = board[row, col];
                board[row, col] = aiPlayer;
                var score = DoMinMax(board, row, col, 0, maxDepth, false, _alpha, _beta, aiLevel);
                board[row, col] = tempBoard;
                if (score > _bestScore)
                {
                    _bestScore = score;
                    movePosition = (row, col);
                }
            }
            Debug.Log($"후보 수: ({row},{col})  /  평가 점수: {scoredMove.score}/{_bestScore}");
        }

        if (movePosition != (-1, -1))
        {
            Debug.Log($"재귀 횟수: {serchCount}/{count}  착수/점수: {movePosition.row},{movePosition.col}/{_bestScore}");
            return (movePosition.row, movePosition.col);
        }

        return null;
    }

    private static float DoMinMax(Constants.PlayerType[,] board, int prevRow, int prevCol, int depth, int maxDepth, bool isMaximizing, float alpha, float beta,
        int aiLevel)
    {
        // 외부 정지 신호
        if (_stopCalculation)
            throw new OperationCanceledException();
        count++;

        if (CheckWin(board, opponentPlayer, prevRow, prevCol) && isMaximizing)  // 이전에 둔 플레이어가 상대이며 해당 돌로 이겼을 경우
            return EvaluateBoard(board) + depth;
        if (CheckWin(board, aiPlayer, prevRow, prevCol) && !isMaximizing) // 이전에 둔 플레이어가 자신이며 해당 돌로 이겼을 경우
            return EvaluateBoard(board) - depth;
        //if (Ruller.StartCheckWinner())    // 무승부의 경우
        //    return 0;

        if (depth >= maxDepth)
        {
            // 깊이 제한에 걸린다면 minmax함수를 종료하고 게임판 평가
            return EvaluateBoard(board); // <-- 게임판 평가 함수 호출
        }

        int maxSearchBranch = aiLevel + 2;

        if (isMaximizing)
        {
            // 우선 순위 탐색을 위한 리스트
            var _scoredMoves = FindScoredMove(board, 2, aiPlayer);
            int searchCount = 0;
            var bestScore = float.MinValue;

            foreach (var scoredMove in _scoredMoves)
            {
                int row = scoredMove.position.x;
                int col = scoredMove.position.y;

                if (isAIFirst && IsForbiddenMove(board, row, col))  // 만약 AI가 선공(PlayerA)이고, 현재 두려는 수가 금지수라면
                    continue;                                       // 이 수는 탐색하지 않고 건너뛴다.

                if (searchCount >= maxSearchBranch) break;
                searchCount++;

                if (IsEmpty(board[row, col], aiPlayer))
                {
                    var tempBoard = board[row, col];
                    board[row, col] = aiPlayer;
                    var score = DoMinMax(board, row, col, depth + 1, maxDepth, false, alpha, beta, aiLevel);
                    board[row, col] = tempBoard;
                    bestScore = Mathf.Max(score, bestScore);
                    //alpha = Mathf.Max(alpha, bestScore); // alpha 값 업데이트
                    //if (beta <= alpha) // 가지치기 조건
                    //    break; // 이 경로는 더 이상 탐색할 필요 없음
                }
            }

            return bestScore;
        }
        else
        {
            // 우선 순위 탐색을 위한 리스트
            var _scoredMoves = FindScoredMove(board, 2, opponentPlayer);
            int searchCount = 0;
            var bestScore = float.MaxValue;

            foreach (var scoredMove in _scoredMoves)
            {
                int row = scoredMove.position.x;
                int col = scoredMove.position.y;

                if (!isAIFirst && IsForbiddenMove(board, row, col)) // 만약 유저가 선공(PlayerA)이고, 현재 두려는 수가 금지수라면
                    continue;                                       // 이 수는 탐색하지 않고 건너뛴다.

                if (searchCount >= maxSearchBranch) break;
                searchCount++;

                if (IsEmpty(board[row, col], opponentPlayer))
                {
                    var tempBoard = board[row, col];
                    board[row, col] = opponentPlayer;
                    var score = DoMinMax(board, row, col, depth + 1, maxDepth, true, alpha, beta, aiLevel);
                    board[row, col] = tempBoard;
                    bestScore = Mathf.Min(score, bestScore);
                    //beta = Mathf.Min(beta, bestScore); // beta 값 업데이트
                    //if (beta <= alpha) // 가지치기 조건
                    //    break; // 이 경로는 더 이상 탐색할 필요 없음
                }
            }

            return bestScore;
        }
    }

    // 후보 제한 함수
    private static List<Vector2Int> FindCandidateMove(Constants.PlayerType[,] board, int searchRange, Constants.PlayerType player)
    {
        var candidateMoves = new HashSet<Vector2Int>(); // 중복 좌표를 막기위한 HashSet

        // 이미 놓인 모든 돌을 순회하며 주변 빈칸을 후보에 추가
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (!IsEmpty(board[row, col], player))
                {
                    // 돌이 놓인 위치를 기준으로 주변 범위를 탐색
                    for (var r = row - searchRange; r <= row + searchRange; r++)
                    {
                        for (var c = col - searchRange; c <= col + searchRange; c++)
                        {
                            // 게임판 경계를 벗어나지 않는지 확인
                            if (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE)
                            {
                                // 빈칸인 경우에만 후보 리스트에 추가
                                if (IsEmpty(board[r, c], player))
                                {
                                    candidateMoves.Add(new Vector2Int(r, c));
                                }
                            }
                        }
                    }
                }
            }
        }

        return new List<Vector2Int>(candidateMoves);
    }

    // 후보 순서 정렬 함수
    private static List<ScoredMove> FindScoredMove(Constants.PlayerType[,] board, int searchRange, Constants.PlayerType player)
    {
        // 현재 상태에 맞는 후보군을 '새로' 생성
        var candidateMoves = FindCandidateMove(board, searchRange, player);

        // 후보 순서를 담을 구조체 리스트
        var scoredMoves = new List<ScoredMove>();
        // 점수 평가 후 정렬
        foreach (var move in candidateMoves)
        {
            float score = SimpleEvaluate(board, move, aiPlayer); // AI의 공격 점수
            score += (SimpleEvaluate(board, move, opponentPlayer) * 1.5f); // 유저의 공격 점수 * //(방어 가중치)

            scoredMoves.Add(new ScoredMove(move, score));
        }
        scoredMoves = scoredMoves.OrderByDescending(m => m.score).ToList();

        return scoredMoves;
    }

    // 점수와 위치를 함께 담기 위한 보조 클래스
    private class ScoredMove
    {
        public Vector2Int position;
        public float score;

        public ScoredMove(Vector2Int pos, float s)
        {
            position = pos;
            score = s;
        }
    }

    #region 게임 진행중 점수 평가

    // 간단한 평가 함수 : 첫 수를 두기 전 우선순위 정렬용
    private static float SimpleEvaluate(Constants.PlayerType[,] board, Vector2Int position, Constants.PlayerType player)
    {
        float score = 0;
        int row = position.x;
        int col = position.y;

        if (!IsEmpty(board[row, col], player))
        {
            return 0; // 이미 돌이 있으면 평가 점수 0점 반환
        }

        // 돌을 놓을 위치를 임시로 설정
        var tempBoard = board[row, col];
        board[row, col] = player;

        score += PatternEvaluator.CheckDirection(board, player, row, col, 1, 0);  // 가로 (오른쪽)
        score += PatternEvaluator.CheckDirection(board, player, row, col, 0, 1);  // 세로 (아래쪽)
        score += PatternEvaluator.CheckDirection(board, player, row, col, 1, 1);  // 대각선 (오른쪽 아래)
        score += PatternEvaluator.CheckDirection(board, player, row, col, 1, -1); // 대각선 (오른쪽 위)


        // 임시로 놓은 돌을 다시 제거 (백트래킹)
        board[row, col] = tempBoard;

        return score;
    }

    // 마무리 평가 함수 : 탐색 노드가 끝난 후 각 노드의 결과 평가용
    private static float EvaluateBoard(Constants.PlayerType[,] board)
    {
        float score;

        // AI와 유저의 패턴 점수를 계산
        float aiScore = PatternEvaluator.CalculatePatterns(board, aiPlayer);
        float userScore = PatternEvaluator.CalculatePatterns(board, opponentPlayer);

        // 두 점수를 합산하여 최종 점수 반환
        // (AI 점수는 양수, 유저 점수는 음수로 처리하여 AI가 최대화하도록 유도)
        score = aiScore - userScore;

        return score;
    }

    private static class PatternEvaluator
    {
        public static float CalculatePatterns(Constants.PlayerType[,] board, Constants.PlayerType player)
        {
            float score = 0;

            // 게임판 전체를 순회
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == player)
                    {
                        // 현재 위치를 기준으로 가로, 세로, 대각선 패턴을 탐색하고 점수 합산
                        score += CheckDirection(board, player, row, col, 1, 0);  // 가로 (오른쪽)
                        score += CheckDirection(board, player, row, col, 0, 1);  // 세로 (아래쪽)
                        score += CheckDirection(board, player, row, col, 1, 1);  // 대각선 (오른쪽 아래)
                        score += CheckDirection(board, player, row, col, 1, -1); // 대각선 (오른쪽 위)
                    }
                }
            }

            return score;
        }

        public static float CheckDirection(Constants.PlayerType[,] board, Constants.PlayerType player, int startRow, int startCol, int rowDir, int colDir)
        {
            int count = 1; // 현재 돌 포함
            int emptyEnds = 0; // 열린 끝의 개수
            float patternScore;

            // 현재 방향으로 연속된 돌의 개수를 센다.
            // 다음 위치
            int nextRow = startRow + rowDir;
            int nextCol = startCol + colDir;
            while (IsWithinBounds(nextRow, nextCol) && board[nextRow, nextCol] == player)
            {
                count++;
                nextRow += rowDir;
                nextCol += colDir;
            }

            // 연속된 돌 뒤에 빈칸이 있는지 확인
            if (IsWithinBounds(nextRow, nextCol) && IsEmpty(board[nextRow, nextCol], player))
            {
                emptyEnds++;
            }

            // 반대 방향으로 연속된 돌의 개수를 센다.
            // 이 위치가 연속된 패턴의 시작점이라고 가정
            // 반대 방향으로 연속된 돌의 개수를 세고, 열린 끝을 확인한다.
            int backCount = 0;
            int prevRow = startRow - rowDir;
            int prevCol = startCol - colDir;
            while (IsWithinBounds(prevRow, prevCol) && board[prevRow, prevCol] == player)
            {
                backCount++;
                prevRow -= rowDir;
                prevCol -= colDir;
            }
            if (IsWithinBounds(prevRow, prevCol) && IsEmpty(board[prevRow, prevCol], player))
            {
                emptyEnds++;
            }
            count += backCount;

            // 패턴에 점수 부여
            patternScore = GetScoreForPattern(count, emptyEnds);


            return patternScore;
        }

        // 패턴의 길이와 열린 끝의 개수에 따라 점수를 반환하는 함수
        private static float GetScoreForPattern(int count, int emptyEnds)
        {
            if (emptyEnds == 2)
            {
                if (count == 5) return 100000; // 5목
                if (count == 4) return 50000;  // 양쪽이 열린 4목 (필승)
                if (count == 3) return 10000;  // 양쪽이 열린 3목 (유력한 승리 위협)
                if (count == 2) return 500;    // 양쪽이 열린 2목
            }
            if (emptyEnds == 1)
            {
                if (count == 5) return 100000; // 5목 (한쪽이 막혀도 승리)
                if (count == 4) return 10000;  // 한쪽이 막힌 4목 (방어 필요)
                if (count == 3) return 500;    // 한쪽이 막힌 3목
                if (count == 2) return 10;     // 한쪽이 막힌 2목
            }
            return 0;
        }

        public static bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
        }
    }
    #endregion


    #region     임시용 CheckWin

    private static bool CheckWin(Constants.PlayerType[,] board, Constants.PlayerType player, int row, int col)
    {
        if (CheckDir(board, player, row, col, 1, 0)
            || CheckDir(board, player, row, col, 0, 1)
            || CheckDir(board, player, row, col, 1, 1)
            || CheckDir(board, player, row, col, 1, -1))
            return true;

        return false;
    }

    private static bool CheckDir(Constants.PlayerType[,] board, Constants.PlayerType player, int startRow, int startCol, int rowDir, int colDir)
    {
        int count = 1; // 현재 돌 포함

        // 현재 방향으로 연속된 돌의 개수를 센다.
        // 다음 위치
        int nextRow = startRow + rowDir;
        int nextCol = startCol + colDir;
        while (PatternEvaluator.IsWithinBounds(nextRow, nextCol) && board[nextRow, nextCol] == player)
        {
            count++;
            nextRow += rowDir;
            nextCol += colDir;
        }

        // 반대 방향으로 연속된 돌의 개수를 센다.
        // 이 위치가 연속된 패턴의 시작점이라고 가정
        // 반대 방향으로 연속된 돌의 개수를 세고, 열린 끝을 확인한다.
        int backCount = 0;
        int prevRow = startRow - rowDir;
        int prevCol = startCol - colDir;
        while (PatternEvaluator.IsWithinBounds(prevRow, prevCol) && board[prevRow, prevCol] == player)
        {
            backCount++;
            prevRow -= rowDir;
            prevCol -= colDir;
        }

        count += backCount;

        if (count >= 5)
            return true;

        return false;
    }

    #endregion


    #region AI용 렌주룰
    private static readonly (int, int)[] directions = new (int, int)[]
    {
        (0,1), (1,0), (1,1),(1,-1)
    };
    private static bool IsForbiddenMove(Constants.PlayerType[,] board, int row, int col)
    {
        // 여기에 3-3, 4-4, 장목(6목 이상)을 판별하는 로직을 구현합니다.
        // 이 위치에 흑돌을 놓았다고 가정하고 검사를 시작합니다.
        var tempBoard = board[row, col];    // 기존 보드 정보 저장
        board[row, col] = Constants.PlayerType.PlayerA; // 흑돌(선공)을 임시로 놓음

        bool isDoubleThreeFour = CheckForDoubleThreeFour(board, row, col);
        bool isOverline = CheckForOverline(board, row, col);

        board[row, col] = tempBoard; // 검사가 끝났으니 돌을 다시 치움

        // 3-3, 4-4, 장목 중 하나라도 해당되면 금지수입니다.
        return isDoubleThreeFour || isOverline;
    }

    /// <summary>
    /// 금지수 검사 함수들
    /// CheckForDoubleThreeFour
    /// CheckForOverline
    /// </summary>
    private static bool CheckForDoubleThreeFour(Constants.PlayerType[,] board, int row, int col)
    {
        int openThreeCount = 0;
        int openFourCount = 0;

        foreach (var dir in directions)
        {
            string line = ExtractLine(board, row, col, dir.Item1, dir.Item2, 5);

            if (IsOpenThree((line))) openThreeCount++;
            if (IsOpenFour(line)) openFourCount++;
        }

        if (openThreeCount >= 2 || openFourCount >= 2)
        {
            return true;
        }
        return false;
    }

    private static string ExtractLine(Constants.PlayerType[,] board, int row, int col, int dRow, int dCol, int length)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        System.Text.StringBuilder sb = new();

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
                switch (board[newRow, newCol])
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
    private static bool IsOpenThree(string line)
    {
        return line.Contains("01110") ||
            line.Contains("010110") ||
            line.Contains("011010");
    }

    // 열린4 패턴 검사
    private static bool IsOpenFour(string line)
    {
        return line.Contains("011110") ||
            line.Contains("0101110") ||
            line.Contains("0111010") ||
            line.Contains("0110110");
    }

    private static bool CheckForOverline(Constants.PlayerType[,] board, int row, int col)
    {
        var state = board[row, col];

        if (state == Constants.PlayerType.PlayerA)
        {
            foreach (var dir in directions)
            {
                if (RenjuRule_Overline_CheckDirection(board, row, col, dir.Item1, dir.Item2, state))
                    return true;
            }
        }

        return false;
    }
    private static bool RenjuRule_Overline_CheckDirection(Constants.PlayerType[,] board, int row, int col, int dRow, int dCol, Constants.PlayerType state)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        int stateCount = 1;
        int noneCount = 0;

        for (int i = 1; i < 6; i++)
        {
            int newRow = row + i * dRow;
            int newCol = col + i * dCol;

            if (newRow < 0 || newRow >= rows ||
                newCol < 0 || newCol >= cols)
            {
                return false;
            }

            if (board[newRow, newCol] == state)
            {
                stateCount++;
            }

            if (board[newRow, newCol] == Constants.PlayerType.None)
            {
                noneCount++;
            }
        }

        if (stateCount >= 5 && noneCount >= 1)
        {
            for (int i = 1; i < 6; i++)
            {
                int newRow = row + i * dRow;
                int newCol = col + i * dCol;
                if (board[newRow, newCol] == Constants.PlayerType.None)
                {
                    board[newRow, newCol] = Constants.PlayerType.Cant;
                    return true;
                }
            }
        }

        return false;
    }


    #endregion


}
