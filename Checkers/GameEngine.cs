using System.Text.Json;

namespace Checkers;

public class Board
{
    public Piece?[,] Squares { get; private set; } = new Piece?[8, 8];

    public void Initialize()
    {
        Squares = new Piece?[8, 8];

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    Squares[row, col] = new Piece(PieceColor.Black);
                }
            }
        }

        for (int row = 5; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    Squares[row, col] = new Piece(PieceColor.Red);
                }
            }
        }
    }

    public Board Clone()
    {
        var board = new Board();
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                board.Squares[row, col] = Squares[row, col]?.Clone();
            }
        }

        return board;
    }

    public int CountPieces(PieceColor color) => Squares.Cast<Piece?>().Count(p => p?.Color == color);

    public List<Move> GetValidMoves(PieceColor turn)
    {
        List<Move> captures = [];
        List<Move> nonCaptures = [];

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = Squares[row, col];
                if (piece is null || piece.Color != turn)
                {
                    continue;
                }

                captures.AddRange(GetCaptureMovesFrom(row, col));
                nonCaptures.AddRange(GetNormalMovesFrom(row, col));
            }
        }

        return [.. captures, .. nonCaptures];
    }

    public List<Move> GetValidMovesFrom(int row, int col, PieceColor turn)
    {
        return GetValidMoves(turn).Where(m => m.FromRow == row && m.FromCol == col).ToList();
    }

    public void ApplyMove(Move move)
    {
        var piece = Squares[move.FromRow, move.FromCol] ?? throw new InvalidOperationException("No piece at source.");

        Squares[move.FromRow, move.FromCol] = null;
        Squares[move.ToRow, move.ToCol] = piece;

        foreach (var captured in move.CapturedPositions)
        {
            Squares[captured.Row, captured.Col] = null;
        }

        if (!piece.IsKing)
        {
            if (piece.Color == PieceColor.Red && move.ToRow == 0)
            {
                piece.IsKing = true;
            }
            else if (piece.Color == PieceColor.Black && move.ToRow == 7)
            {
                piece.IsKing = true;
            }
        }
    }

    private List<Move> GetNormalMovesFrom(int row, int col)
    {
        var piece = Squares[row, col];
        if (piece is null)
        {
            return [];
        }

        List<Move> moves = [];

        if (piece.IsKing)
        {
            foreach (var (dr, dc) in GetDirections(piece))
            {
                int toRow = row + dr;
                int toCol = col + dc;

                while (IsInBounds(toRow, toCol) && Squares[toRow, toCol] is null)
                {
                    moves.Add(new Move
                    {
                        FromRow = row,
                        FromCol = col,
                        ToRow = toRow,
                        ToCol = toCol
                    });

                    toRow += dr;
                    toCol += dc;
                }
            }

            return moves;
        }

        foreach (var (dr, dc) in GetDirections(piece))
        {
            int toRow = row + dr;
            int toCol = col + dc;
            if (IsInBounds(toRow, toCol) && Squares[toRow, toCol] is null)
            {
                moves.Add(new Move
                {
                    FromRow = row,
                    FromCol = col,
                    ToRow = toRow,
                    ToCol = toCol
                });
            }
        }

        return moves;
    }

    private List<Move> GetCaptureMovesFrom(int row, int col)
    {
        var piece = Squares[row, col];
        if (piece is null)
        {
            return [];
        }

        List<Move> captures = [];
        ExploreCaptures(Clone(), row, col, row, col, piece.Color, piece.IsKing, [], captures);
        return captures;
    }

    private void ExploreCaptures(
        Board board,
        int startRow,
        int startCol,
        int currentRow,
        int currentCol,
        PieceColor color,
        bool isKing,
        List<BoardPosition> captured,
        List<Move> results)
    {
        bool foundNextCapture = false;

        if (isKing)
        {
            foundNextCapture = ExploreKingCaptures(board, startRow, startCol, currentRow, currentCol, color, captured, results);
        }
        else
        {
            foreach (var (dr, dc) in GetDirections(color, isKing: false))
            {
                int enemyRow = currentRow + dr;
                int enemyCol = currentCol + dc;
                int landRow = currentRow + 2 * dr;
                int landCol = currentCol + 2 * dc;

                if (!IsInBounds(enemyRow, enemyCol) || !IsInBounds(landRow, landCol))
                {
                    continue;
                }

                var enemyPiece = board.Squares[enemyRow, enemyCol];
                if (enemyPiece is null || enemyPiece.Color == color || board.Squares[landRow, landCol] is not null)
                {
                    continue;
                }

                foundNextCapture = true;

                var nextBoard = board.Clone();
                var movingPiece = nextBoard.Squares[currentRow, currentCol]!;
                nextBoard.Squares[currentRow, currentCol] = null;
                nextBoard.Squares[enemyRow, enemyCol] = null;
                nextBoard.Squares[landRow, landCol] = movingPiece;

                var nextCaptured = captured.Select(c => new BoardPosition(c.Row, c.Col)).ToList();
                nextCaptured.Add(new BoardPosition(enemyRow, enemyCol));

                bool becomesKing = !movingPiece.IsKing && ((movingPiece.Color == PieceColor.Red && landRow == 0) || (movingPiece.Color == PieceColor.Black && landRow == 7));
                movingPiece.IsKing = movingPiece.IsKing || becomesKing;

                ExploreCaptures(nextBoard, startRow, startCol, landRow, landCol, color, movingPiece.IsKing, nextCaptured, results);
            }
        }

        if (!foundNextCapture && captured.Count > 0)
        {
            results.Add(new Move
            {
                FromRow = startRow,
                FromCol = startCol,
                ToRow = currentRow,
                ToCol = currentCol,
                CapturedPositions = captured
            });
        }
    }

    private bool ExploreKingCaptures(
        Board board,
        int startRow,
        int startCol,
        int currentRow,
        int currentCol,
        PieceColor color,
        List<BoardPosition> captured,
        List<Move> results)
    {
        bool found = false;

        foreach (var (dr, dc) in GetDirections(color, isKing: true))
        {
            int scanRow = currentRow + dr;
            int scanCol = currentCol + dc;

            while (IsInBounds(scanRow, scanCol) && board.Squares[scanRow, scanCol] is null)
            {
                scanRow += dr;
                scanCol += dc;
            }

            if (!IsInBounds(scanRow, scanCol))
            {
                continue;
            }

            var enemyPiece = board.Squares[scanRow, scanCol];
            if (enemyPiece is null || enemyPiece.Color == color)
            {
                continue;
            }

            int landRow = scanRow + dr;
            int landCol = scanCol + dc;

            while (IsInBounds(landRow, landCol) && board.Squares[landRow, landCol] is null)
            {
                found = true;

                var nextBoard = board.Clone();
                var movingPiece = nextBoard.Squares[currentRow, currentCol]!;
                nextBoard.Squares[currentRow, currentCol] = null;
                nextBoard.Squares[scanRow, scanCol] = null;
                nextBoard.Squares[landRow, landCol] = movingPiece;

                var nextCaptured = captured.Select(c => new BoardPosition(c.Row, c.Col)).ToList();
                nextCaptured.Add(new BoardPosition(scanRow, scanCol));

                ExploreCaptures(nextBoard, startRow, startCol, landRow, landCol, color, isKing: true, nextCaptured, results);

                landRow += dr;
                landCol += dc;
            }
        }

        return found;
    }

    private static IEnumerable<(int dr, int dc)> GetDirections(Piece piece) => GetDirections(piece.Color, piece.IsKing);

    private static IEnumerable<(int dr, int dc)> GetDirections(PieceColor color, bool isKing)
    {
        if (isKing)
        {
            return [(-1, -1), (-1, 1), (1, -1), (1, 1)];
        }

        return color == PieceColor.Red ? [(-1, -1), (-1, 1)] : [(1, -1), (1, 1)];
    }

    private static bool IsInBounds(int row, int col) => row is >= 0 and < 8 && col is >= 0 and < 8;

    public bool HasAnyMoves(PieceColor color) => GetValidMoves(color).Count > 0;
}

public class GameStateSnapshot
{
    public string[] Rows { get; set; } = [];
    public PieceColor Turn { get; set; }
    public int MoveCount { get; set; }
    public int NoCaptureCounter { get; set; }
}

public class Game
{
    private readonly Stack<GameStateSnapshot> _undo = new();
    private readonly Stack<GameStateSnapshot> _redo = new();

    public Board Board { get; private set; } = new();
    public PieceColor CurrentTurn { get; private set; } = PieceColor.Red;
    public int MoveCount { get; private set; }
    public int NoCaptureCounter { get; private set; }

    public void StartNew()
    {
        Board.Initialize();
        CurrentTurn = PieceColor.Red;
        MoveCount = 0;
        NoCaptureCounter = 0;
        _undo.Clear();
        _redo.Clear();
    }

    public List<Move> GetValidMoves() => Board.GetValidMoves(CurrentTurn);

    public List<Move> GetValidMovesFrom(int row, int col) => Board.GetValidMovesFrom(row, col, CurrentTurn);

    public bool TryApplyMove(Move move)
    {
        var validMove = GetValidMoves().FirstOrDefault(m => m.Matches(move));
        if (validMove is null)
        {
            return false;
        }

        _undo.Push(CaptureSnapshot());
        _redo.Clear();

        Board.ApplyMove(validMove);
        MoveCount++;

        if (validMove.IsCapture)
        {
            NoCaptureCounter = 0;
        }
        else
        {
            NoCaptureCounter++;
        }

        CurrentTurn = CurrentTurn == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
        return true;
    }

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        _redo.Push(CaptureSnapshot());
        RestoreSnapshot(_undo.Pop());
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        _undo.Push(CaptureSnapshot());
        RestoreSnapshot(_redo.Pop());
        return true;
    }

    public PieceColor? GetWinner()
    {
        if (Board.CountPieces(PieceColor.Red) == 0)
        {
            return PieceColor.Black;
        }

        if (Board.CountPieces(PieceColor.Black) == 0)
        {
            return PieceColor.Red;
        }

        if (!Board.HasAnyMoves(CurrentTurn))
        {
            return CurrentTurn == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
        }

        return null;
    }

    public bool IsDraw() => NoCaptureCounter >= 40;

    public void SaveToFile(string filePath)
    {
        var snapshot = CaptureSnapshot();
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public bool LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var json = File.ReadAllText(filePath);
        var snapshot = JsonSerializer.Deserialize<GameStateSnapshot>(json);
        if (snapshot is null || snapshot.Rows.Length != 8)
        {
            return false;
        }

        RestoreSnapshot(snapshot);
        _undo.Clear();
        _redo.Clear();
        return true;
    }

    private GameStateSnapshot CaptureSnapshot()
    {
        string[] rows = new string[8];

        for (int row = 0; row < 8; row++)
        {
            char[] rowChars = new char[8];
            for (int col = 0; col < 8; col++)
            {
                var piece = Board.Squares[row, col];
                rowChars[col] = piece switch
                {
                    null => '.',
                    { Color: PieceColor.Red, IsKing: false } => 'r',
                    { Color: PieceColor.Red, IsKing: true } => 'R',
                    { Color: PieceColor.Black, IsKing: false } => 'b',
                    _ => 'B'
                };
            }
            rows[row] = new string(rowChars);
        }

        return new GameStateSnapshot
        {
            Rows = rows,
            Turn = CurrentTurn,
            MoveCount = MoveCount,
            NoCaptureCounter = NoCaptureCounter
        };
    }

    private void RestoreSnapshot(GameStateSnapshot snapshot)
    {
        var board = new Board();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                char c = snapshot.Rows[row][col];
                board.Squares[row, col] = c switch
                {
                    'r' => new Piece(PieceColor.Red),
                    'R' => new Piece(PieceColor.Red, true),
                    'b' => new Piece(PieceColor.Black),
                    'B' => new Piece(PieceColor.Black, true),
                    _ => null
                };
            }
        }

        Board = board;
        CurrentTurn = snapshot.Turn;
        MoveCount = snapshot.MoveCount;
        NoCaptureCounter = snapshot.NoCaptureCounter;
    }
}
