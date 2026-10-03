namespace Checkers;

public class AIPlayer
{
    private readonly Random _random = new();

    public Move? ChooseMove(Game game, DifficultyLevel difficulty, PieceColor aiColor)
    {
        var validMoves = game.GetValidMoves();
        if (validMoves.Count == 0)
        {
            return null;
        }

        return difficulty switch
        {
            DifficultyLevel.Easy => validMoves[_random.Next(validMoves.Count)].Clone(),
            DifficultyLevel.Normal => ChooseNormalMove(validMoves, game.Board, aiColor),
            DifficultyLevel.Hard => ChooseMinimaxMove(game.Board, aiColor, depth: 5),
            _ => validMoves[_random.Next(validMoves.Count)].Clone()
        };
    }

    private Move ChooseNormalMove(List<Move> validMoves, Board board, PieceColor aiColor)
    {
        var captures = validMoves.Where(m => m.IsCapture).ToList();
        if (captures.Count > 0)
        {
            return captures.OrderByDescending(c => c.CapturedPositions.Count).First().Clone();
        }

        var kingPromotion = validMoves
            .Where(m =>
            {
                var piece = board.Squares[m.FromRow, m.FromCol];
                if (piece is null || piece.IsKing)
                {
                    return false;
                }

                return piece.Color == PieceColor.Red ? m.ToRow == 0 : m.ToRow == 7;
            })
            .ToList();

        if (kingPromotion.Count > 0)
        {
            return kingPromotion[_random.Next(kingPromotion.Count)].Clone();
        }

        return validMoves
            .OrderBy(m => Math.Abs(3 - m.ToRow) + Math.Abs(3 - m.ToCol))
            .First()
            .Clone();
    }

    private Move ChooseMinimaxMove(Board board, PieceColor aiColor, int depth)
    {
        var moves = board.GetValidMoves(aiColor);
        Move bestMove = moves[0];
        int bestScore = int.MinValue;

        foreach (var move in moves)
        {
            var nextBoard = board.Clone();
            nextBoard.ApplyMove(move);
            int score = Minimax(nextBoard, depth - 1, false, aiColor, int.MinValue, int.MaxValue);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove.Clone();
    }

    private int Minimax(Board board, int depth, bool maximizing, PieceColor aiColor, int alpha, int beta)
    {
        PieceColor current = maximizing ? aiColor : OpponentOf(aiColor);
        var moves = board.GetValidMoves(current);

        if (depth == 0 || moves.Count == 0)
        {
            return EvaluateBoard(board, aiColor, moves.Count == 0 ? current : (PieceColor?)null);
        }

        if (maximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in moves)
            {
                var child = board.Clone();
                child.ApplyMove(move);
                int eval = Minimax(child, depth - 1, false, aiColor, alpha, beta);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEval;
        }

        int minEval = int.MaxValue;
        foreach (var move in moves)
        {
            var child = board.Clone();
            child.ApplyMove(move);
            int eval = Minimax(child, depth - 1, true, aiColor, alpha, beta);
            minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);
            if (beta <= alpha)
            {
                break;
            }
        }
        return minEval;
    }

    private static int EvaluateBoard(Board board, PieceColor aiColor, PieceColor? noMovesFor)
    {
        if (noMovesFor.HasValue)
        {
            if (noMovesFor.Value == aiColor)
            {
                return -10000;
            }

            return 10000;
        }

        int score = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = board.Squares[row, col];
                if (piece is null)
                {
                    continue;
                }

                int value = piece.IsKing ? 5 : 3;
                value += (piece.Color == PieceColor.Red ? (7 - row) : row) / 2;

                if (piece.Color == aiColor)
                {
                    score += value;
                }
                else
                {
                    score -= value;
                }
            }
        }

        score += board.GetValidMoves(aiColor).Count;
        score -= board.GetValidMoves(OpponentOf(aiColor)).Count;

        return score;
    }

    private static PieceColor OpponentOf(PieceColor color) => color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
}
