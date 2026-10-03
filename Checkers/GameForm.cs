namespace Checkers;

public class GameForm : Form
{
    private readonly Game _game = new();
    private readonly AIPlayer _ai = new();
    private readonly bool _vsComputer;
    private readonly DifficultyLevel? _difficulty;
    private readonly int _bestOf;
    private readonly string _redPlayerName;
    private readonly string _blackPlayerName;

    private readonly Button[,] _boardButtons = new Button[8, 8];
    private readonly Label _lblTurn = new();
    private readonly Label _lblStatus = new();
    private readonly Label _lblSeries = new();
    private readonly Label _lblCaptured = new();
    private readonly Label _lblTimer = new();
    private readonly ListBox _lstMoves = new();

    private readonly System.Diagnostics.Stopwatch _roundStopwatch = new();
    private readonly System.Windows.Forms.Timer _roundTimer = new() { Interval = 1000 };

    private Image? _whitePieceImage;
    private Image? _whiteKingImage;
    private Image? _blackPieceImage;
    private Image? _blackKingImage;

    private BoardPosition? _selected;
    private List<Move> _selectedMoves = [];
    private bool _gameLocked;

    private int _seriesWins;
    private int _seriesLosses;
    private int _redCapturedCount;
    private int _blackCapturedCount;

    public GameForm(bool vsComputer, int bestOf, DifficultyLevel? difficulty = null)
    {
        _vsComputer = vsComputer;
        _bestOf = bestOf;
        _difficulty = difficulty;

        var currentUsername = AppSession.CurrentPlayer?.Username;
        _redPlayerName = string.IsNullOrWhiteSpace(currentUsername) ? "Player 1" : currentUsername;
        _blackPlayerName = vsComputer ? "Computer" : "Player 2";

        Text = vsComputer ? $"Checkers - VS Computer ({difficulty})" : "Checkers - 2 Players";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(910, 620);

        _roundTimer.Tick += (_, _) => UpdateTimerLabel();

        LoadPieceImages();
        BuildLayout();

        _game.StartNew();
        ResetRoundMetrics();
        UpdateBoardUI();
    }

    private void LoadPieceImages()
    {
        string baseDir = AppContext.BaseDirectory;

        _whitePieceImage = TryLoadImage(baseDir, "white.jpg");
        _whiteKingImage = TryLoadImage(baseDir, "whiteking.jpg");
        _blackPieceImage = TryLoadImage(baseDir, "black.jpg");
        _blackKingImage = TryLoadImage(baseDir, "blackking.jpg");

        if (_whitePieceImage is null || _whiteKingImage is null || _blackPieceImage is null || _blackKingImage is null)
        {
            MessageBox.Show(
                "One or more piece images were not found in the app folder.\n" +
                "Expected files: white.jpg, whiteking.jpg, black.jpg, blackking.jpg\n" +
                $"Path checked: {baseDir}",
                "Piece Image Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static Image? TryLoadImage(string baseDir, string fileName)
    {
        string path = System.IO.Path.Combine(baseDir, fileName);
        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }

    private void BuildLayout()
    {
        BackColor = Color.FromArgb(24, 24, 28);
        ForeColor = Color.WhiteSmoke;

        var boardPanel = new TableLayoutPanel
        {
            Left = 20,
            Top = 20,
            Width = 560,
            Height = 560,
            RowCount = 8,
            ColumnCount = 8,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            BackColor = Color.Black
        };

        for (int i = 0; i < 8; i++)
        {
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
        }

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var btn = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    Tag = new BoardPosition(row, col),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    TextImageRelation = TextImageRelation.Overlay
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += BoardButton_Click;
                _boardButtons[row, col] = btn;
                boardPanel.Controls.Add(btn, col, row);
            }
        }

        _lblTurn.Left = 600;
        _lblTurn.Top = 25;
        _lblTurn.Width = 280;
        _lblTurn.Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold);
        _lblTurn.ForeColor = Color.White;

        _lblStatus.Left = 600;
        _lblStatus.Top = 55;
        _lblStatus.Width = 280;
        _lblStatus.Height = 50;
        _lblStatus.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        _lblStatus.ForeColor = Color.Gainsboro;

        _lblSeries.Left = 600;
        _lblSeries.Top = 105;
        _lblSeries.Width = 280;
        _lblSeries.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        _lblSeries.ForeColor = Color.WhiteSmoke;

        _lblCaptured.Left = 600;
        _lblCaptured.Top = 132;
        _lblCaptured.Width = 280;
        _lblCaptured.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        _lblCaptured.ForeColor = Color.Gainsboro;

        _lblTimer.Left = 600;
        _lblTimer.Top = 156;
        _lblTimer.Width = 280;
        _lblTimer.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        _lblTimer.ForeColor = Color.LightSkyBlue;

        _lstMoves.Left = 600;
        _lstMoves.Top = 186;
        _lstMoves.Width = 280;
        _lstMoves.Height = 254;
        _lstMoves.Font = new Font("Consolas", 9.5f, FontStyle.Regular);
        _lstMoves.BackColor = Color.FromArgb(36, 36, 40);
        _lstMoves.ForeColor = Color.WhiteSmoke;
        _lstMoves.BorderStyle = BorderStyle.None;

        var btnResign = new Button { Text = "Resign", Left = 695, Top = 458, Width = 180, Height = 36 };
        StyleActionButton(btnResign, Color.FromArgb(244, 67, 54));
        btnResign.Click += (_, _) => HandleResign();

        Controls.Add(boardPanel);
        Controls.Add(_lblTurn);
        Controls.Add(_lblStatus);
        Controls.Add(_lblSeries);
        Controls.Add(_lblCaptured);
        Controls.Add(_lblTimer);
        Controls.Add(_lstMoves);
        Controls.Add(btnResign);
    }

    private void BoardButton_Click(object? sender, EventArgs e)
    {
        if (_gameLocked)
        {
            return;
        }

        if (sender is not Button btn || btn.Tag is not BoardPosition pos)
        {
            return;
        }

        if (_vsComputer && _game.CurrentTurn == PieceColor.Black)
        {
            return;
        }

        var piece = _game.Board.Squares[pos.Row, pos.Col];

        if (_selected is not null)
        {
            var candidateMoves = _selectedMoves
                .Where(m => m.ToRow == pos.Row && m.ToCol == pos.Col)
                .OrderByDescending(m => m.CapturedPositions.Count)
                .ToList();

            var selectedMove = candidateMoves.FirstOrDefault();
            if (selectedMove is not null)
            {
                ApplyAndContinue(selectedMove);
                return;
            }
        }

        if (piece is null || piece.Color != _game.CurrentTurn)
        {
            _selected = null;
            _selectedMoves.Clear();
            UpdateBoardUI();
            return;
        }

        _selected = pos;
        _selectedMoves = _game.GetValidMovesFrom(pos.Row, pos.Col)
            .OrderByDescending(m => m.CapturedPositions.Count)
            .ToList();
        UpdateBoardUI();
    }

    private async void ApplyAndContinue(Move move)
    {
        PieceColor moverColor = _game.CurrentTurn;

        if (!_game.TryApplyMove(move))
        {
            return;
        }

        RegisterCapture(moverColor, move.CapturedPositions.Count);

        _selected = null;
        _selectedMoves.Clear();

        AddMoveToHistory(move, moverColor, isAi: false);
        UpdateBoardUI();

        if (HandleGameEndIfNeeded())
        {
            return;
        }

        if (_vsComputer && _game.CurrentTurn == PieceColor.Black)
        {
            _gameLocked = true;
            await Task.Delay(300);
            var aiMove = _ai.ChooseMove(_game, _difficulty ?? DifficultyLevel.Easy, PieceColor.Black);
            if (aiMove is not null)
            {
                _game.TryApplyMove(aiMove);
                RegisterCapture(PieceColor.Black, aiMove.CapturedPositions.Count);
                AddMoveToHistory(aiMove, PieceColor.Black, isAi: true);
                UpdateBoardUI();
                HandleGameEndIfNeeded();
            }
            _gameLocked = false;
        }
    }

    private void AddMoveToHistory(Move move, PieceColor moverColor, bool isAi)
    {
        string actor = isAi
            ? _blackPlayerName
            : moverColor == PieceColor.Red ? _redPlayerName : _blackPlayerName;

        string moveText = $"{_game.MoveCount}. {actor}: ({move.FromRow},{move.FromCol}) -> ({move.ToRow},{move.ToCol})";
        if (move.IsCapture)
        {
            moveText += $" x{move.CapturedPositions.Count}";
        }

        _lstMoves.Items.Add(moveText);
        _lstMoves.TopIndex = _lstMoves.Items.Count - 1;
    }

    private bool HandleGameEndIfNeeded()
    {
        if (_game.IsDraw())
        {
            MessageBox.Show("Draw by no-capture rule.", "Match Draw", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FinishGame(MatchResult.Draw);
            return true;
        }

        var winner = _game.GetWinner();
        if (!winner.HasValue)
        {
            return false;
        }

        bool userWon = winner.Value == PieceColor.Red;

        if (_vsComputer)
        {
            MessageBox.Show(userWon ? "You won this round!" : "Computer won this round.", "Round Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FinishGame(userWon ? MatchResult.Win : MatchResult.Loss);
        }
        else
        {
            MessageBox.Show($"{winner.Value} wins this round.", "Round Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FinishGame(userWon ? MatchResult.Win : MatchResult.Loss);
        }

        return true;
    }

    private void FinishGame(MatchResult result, bool promptBeforeNextRound = false)
    {
        _roundStopwatch.Stop();
        _roundTimer.Stop();

        if (result == MatchResult.Win)
        {
            _seriesWins++;
        }
        else if (result == MatchResult.Loss)
        {
            _seriesLosses++;
        }

        if (AppSession.CurrentPlayer is not null)
        {
            int? durationSeconds = _vsComputer && result == MatchResult.Win
                ? (int)Math.Round(_roundStopwatch.Elapsed.TotalSeconds)
                : null;

            AppSession.Database.RecordMatch(
                AppSession.CurrentPlayer.PlayerId,
                _vsComputer ? OpponentType.Computer : OpponentType.Player,
                _vsComputer ? _difficulty : null,
                result,
                _game.MoveCount,
                durationSeconds,
                out _);
        }

        int neededWins = (_bestOf / 2) + 1;

        if (_seriesWins >= neededWins || _seriesLosses >= neededWins)
        {
            var finalResult = _seriesWins > _seriesLosses ? MatchResult.Win : (_seriesWins < _seriesLosses ? MatchResult.Loss : MatchResult.Draw);

            if (AppSession.CurrentPlayer is not null)
            {
                AppSession.Database.RecordSeries(AppSession.CurrentPlayer.PlayerId, _bestOf, _seriesWins, _seriesLosses, finalResult, out _);
            }

            if (!promptBeforeNextRound)
            {
                MessageBox.Show($"Series Complete\nScore: {_seriesWins}-{_seriesLosses}\nResult: {finalResult}", "Series Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StartNextGame(resetSeries: true);
                return;
            }

            var playNewSeries = MessageBox.Show(
                $"Series Complete\nScore: {_seriesWins}-{_seriesLosses}\nResult: {finalResult}\n\nPlay another series?",
                "Series Result",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (playNewSeries == DialogResult.Yes)
            {
                StartNextGame(resetSeries: true);
            }
            else
            {
                Close();
            }

            return;
        }

        if (!promptBeforeNextRound)
        {
            MessageBox.Show($"Next round starts now.\nSeries score: {_seriesWins}-{_seriesLosses}", "Series Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
            StartNextGame(resetSeries: false);
            return;
        }

        var playAgain = MessageBox.Show(
            $"Series score: {_seriesWins}-{_seriesLosses}\n\nDo you want to play the next round?",
            "Round Finished",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (playAgain == DialogResult.Yes)
        {
            StartNextGame(resetSeries: false);
        }
        else
        {
            Close();
        }
    }

    private void StartNextGame(bool resetSeries)
    {
        if (resetSeries)
        {
            _seriesWins = 0;
            _seriesLosses = 0;
            _lstMoves.Items.Clear();
        }

        _selected = null;
        _selectedMoves.Clear();
        _game.StartNew();
        ResetRoundMetrics();
        UpdateBoardUI();
    }

    private void HandleResign()
    {
        if (MessageBox.Show("Do you want to resign this round?", "Resign", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            MessageBox.Show("You resigned this round.", "Resigned", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FinishGame(MatchResult.Loss, promptBeforeNextRound: true);
        }
    }

    private void UpdateBoardUI()
    {
        (Color darkSquareColor, Color lightSquareColor) = GetBoardThemeColors(AppSession.Settings.BoardTheme);

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var button = _boardButtons[row, col];
                bool darkSquare = (row + col) % 2 == 1;
                button.BackColor = darkSquare ? darkSquareColor : lightSquareColor;
                button.Enabled = darkSquare;

                var piece = _game.Board.Squares[row, col];
                if (piece is null)
                {
                    button.Text = string.Empty;
                    button.Image = null;
                    button.BackgroundImage = null;
                }
                else
                {
                    button.Text = string.Empty;
                    button.Image = null;
                    button.BackgroundImage = GetPieceImage(piece);

                    if (button.BackgroundImage is null)
                    {
                        button.Text = piece.IsKing ? "K" : "?";
                        button.ForeColor = piece.Color == PieceColor.Red ? Color.WhiteSmoke : Color.Black;
                    }
                }
            }
        }

        if (_selected is not null && AppSession.Settings.HighlightValidMoves)
        {
            _boardButtons[_selected.Row, _selected.Col].BackColor = Color.FromArgb(255, 193, 7);
            foreach (var move in _selectedMoves)
            {
                _boardButtons[move.ToRow, move.ToCol].BackColor = Color.FromArgb(76, 175, 80);
            }
        }

        var turnName = _game.CurrentTurn == PieceColor.Red ? _redPlayerName : _blackPlayerName;
        _lblTurn.Text = $"Turn: {turnName} ({_game.CurrentTurn})";
        _lblStatus.Text = _vsComputer
            ? $"Mode: VS Computer ({_difficulty})\n{_redPlayerName} vs {_blackPlayerName}"
            : $"Mode: Local 2 Players\n{_redPlayerName} vs {_blackPlayerName}";
        _lblSeries.Text = $"Best of {_bestOf} | Wins: {_seriesWins}  Losses: {_seriesLosses}";
        _lblCaptured.Text = $"Captured | {_redPlayerName}: {_redCapturedCount}   {_blackPlayerName}: {_blackCapturedCount}";
        UpdateTimerLabel();
    }

    private void ResetRoundMetrics()
    {
        _redCapturedCount = 0;
        _blackCapturedCount = 0;
        _roundStopwatch.Restart();
        _roundTimer.Start();
        UpdateTimerLabel();
    }

    private void RegisterCapture(PieceColor moverColor, int capturedCount)
    {
        if (capturedCount <= 0)
        {
            return;
        }

        if (moverColor == PieceColor.Red)
        {
            _redCapturedCount += capturedCount;
        }
        else
        {
            _blackCapturedCount += capturedCount;
        }
    }

    private void UpdateTimerLabel()
    {
        var elapsed = _roundStopwatch.Elapsed;
        _lblTimer.Text = $"Round Time: {elapsed:mm\\:ss}";
    }

    private static (Color dark, Color light) GetBoardThemeColors(BoardTheme theme)
    {
        return theme switch
        {
            BoardTheme.ClassicWood => (Color.FromArgb(92, 64, 51), Color.FromArgb(235, 210, 180)),
            BoardTheme.Ocean => (Color.FromArgb(19, 71, 102), Color.FromArgb(207, 232, 245)),
            _ => (Color.FromArgb(30, 30, 30), Color.FromArgb(245, 245, 245))
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _roundTimer.Stop();
        _roundTimer.Dispose();
        _whitePieceImage?.Dispose();
        _whiteKingImage?.Dispose();
        _blackPieceImage?.Dispose();
        _blackKingImage?.Dispose();
        base.OnFormClosed(e);
    }

    private Image? GetPieceImage(Piece piece)
    {
        // Red side uses the requested white images.
        if (piece.Color == PieceColor.Red)
        {
            return piece.IsKing ? _whiteKingImage : _whitePieceImage;
        }

        return piece.IsKing ? _blackKingImage : _blackPieceImage;
    }

    private static void StyleActionButton(Button button, Color baseColor)
    {
        button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        UiStyling.StyleInteractiveButton(button, baseColor, cornerRadius: 9);
    }
}
