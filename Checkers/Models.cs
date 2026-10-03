namespace Checkers;

public enum PieceColor
{
    Red,
    Black
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}

public enum OpponentType
{
    Player,
    Computer
}

public enum MatchResult
{
    Win,
    Loss,
    Draw
}

public class Player
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsComputer { get; set; }
    public PieceColor Color { get; set; }
}

public class PlayerAccount
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
}

public class BoardPosition
{
    public int Row { get; set; }
    public int Col { get; set; }

    public BoardPosition()
    {
    }

    public BoardPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

public class Piece
{
    public PieceColor Color { get; set; }
    public bool IsKing { get; set; }

    public Piece(PieceColor color, bool isKing = false)
    {
        Color = color;
        IsKing = isKing;
    }

    public Piece Clone() => new(Color, IsKing);
}

public class Move
{
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
    public List<BoardPosition> CapturedPositions { get; set; } = [];

    public bool IsCapture => CapturedPositions.Count > 0;

    public Move Clone()
    {
        return new Move
        {
            FromRow = FromRow,
            FromCol = FromCol,
            ToRow = ToRow,
            ToCol = ToCol,
            CapturedPositions = CapturedPositions.Select(c => new BoardPosition(c.Row, c.Col)).ToList()
        };
    }

    public bool Matches(Move other)
    {
        if (FromRow != other.FromRow || FromCol != other.FromCol || ToRow != other.ToRow || ToCol != other.ToCol)
        {
            return false;
        }

        if (CapturedPositions.Count != other.CapturedPositions.Count)
        {
            return false;
        }

        for (int i = 0; i < CapturedPositions.Count; i++)
        {
            if (CapturedPositions[i].Row != other.CapturedPositions[i].Row || CapturedPositions[i].Col != other.CapturedPositions[i].Col)
            {
                return false;
            }
        }

        return true;
    }
}
