namespace Checkers;

public class LeaderboardForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly Label _lblInfo = new();
    private readonly Label _lblTitle = new();

    public LeaderboardForm()
    {
        Text = "Leaderboard";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(980, 560);
        BackColor = Color.FromArgb(24, 24, 30);
        ForeColor = Color.WhiteSmoke;

        _lblTitle.Text = "CHECKERS LEADERBOARD";
        _lblTitle.ForeColor = Color.Gold;
        _lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        _lblTitle.AutoSize = true;
        _lblTitle.Left = 16;
        _lblTitle.Top = 10;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.FromArgb(32, 32, 42)
        };

        var btnRefresh = new Button
        {
            Text = "Refresh",
            Left = 780,
            Top = 18,
            Width = 84,
            Height = 30
        };
        StyleButton(btnRefresh, Color.FromArgb(56, 126, 203));
        btnRefresh.Click += (_, _) => LoadData();

        var btnClose = new Button
        {
            Text = "Close",
            Left = 872,
            Top = 18,
            Width = 84,
            Height = 30
        };
        StyleButton(btnClose, Color.FromArgb(88, 88, 96));
        btnClose.Click += (_, _) => Close();

        _lblInfo.AutoSize = true;
        _lblInfo.Left = 340;
        _lblInfo.Top = 24;
        _lblInfo.ForeColor = Color.Gainsboro;
        _lblInfo.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        topPanel.Controls.Add(_lblTitle);
        topPanel.Controls.Add(_lblInfo);
        topPanel.Controls.Add(btnRefresh);
        topPanel.Controls.Add(btnClose);

        var gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(24, 24, 30)
        };

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.FromArgb(34, 34, 44);
        _grid.BorderStyle = BorderStyle.None;
        _grid.GridColor = Color.FromArgb(58, 61, 79);
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 34;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 52, 69);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        _grid.DefaultCellStyle.BackColor = Color.FromArgb(39, 41, 53);
        _grid.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 120, 190);
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 47, 61);

        gridPanel.Controls.Add(_grid);
        Controls.Add(gridPanel);
        Controls.Add(topPanel);

        Load += (_, _) => LoadData();
    }

    private static void StyleButton(Button button, Color backColor)
    {
        button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        UiStyling.StyleInteractiveButton(button, backColor, cornerRadius: 9);
    }

    private void LoadData()
    {
        AppSession.Database.RemoveSampleData(out _);

        var data = AppSession.Database.GetLeaderboard(out var message);
        if (data.Columns.Contains("Rank"))
        {
            data.Columns.Remove("Rank");
        }

        data.Columns.Add("Rank", typeof(int));
        data.Columns["Rank"]!.SetOrdinal(0);

        for (int i = 0; i < data.Rows.Count; i++)
        {
            data.Rows[i]["Rank"] = i + 1;
        }

        _grid.DataSource = data;

        if (_grid.Columns.Contains("WinRate"))
        {
            _grid.Columns["WinRate"]!.DefaultCellStyle.Format = "0.00'%'";
        }

        if (_grid.Columns.Contains("BestWinTime"))
        {
            _grid.Columns["BestWinTime"]!.HeaderText = "Best Win vs Computer";
        }

        _lblInfo.Text = string.IsNullOrWhiteSpace(message)
            ? (data.Rows.Count == 0 ? "No players in leaderboard yet." : $"Loaded {data.Rows.Count} players")
            : $"Database error: {message}";
    }
}
