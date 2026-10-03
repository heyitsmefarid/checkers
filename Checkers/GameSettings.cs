namespace Checkers;

public enum BoardTheme
{
    BlackWhite,
    ClassicWood,
    Ocean
}

public class GameSettings
{
    public bool EnableBackgroundMusic { get; set; } = true;
    public int MusicVolumePercent { get; set; } = 45;
    public bool HighlightValidMoves { get; set; } = true;
    public BoardTheme BoardTheme { get; set; } = BoardTheme.BlackWhite;
}
