namespace Checkers;

public class GameSetupForm : Form
{
    private readonly ComboBox _cmbBestOf = new();
    private readonly ComboBox _cmbDifficulty = new();

    public int BestOf { get; private set; } = 1;
    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Easy;

    public GameSetupForm(bool includeDifficulty)
    {
        Text = "Match Setup";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(320, includeDifficulty ? 220 : 170);
        BackColor = Color.FromArgb(28, 28, 34);
        ForeColor = Color.WhiteSmoke;

        var lblTitle = new Label
        {
            Text = "NEW MATCH",
            Left = 20,
            Top = 14,
            Width = 260,
            Height = 28,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.Gold
        };

        var lblBestOf = new Label
        {
            Text = "Best Of",
            Left = 20,
            Top = 58,
            Width = 100,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke
        };

        _cmbBestOf.Left = 140;
        _cmbBestOf.Top = 54;
        _cmbBestOf.Width = 150;
        _cmbBestOf.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbBestOf.FlatStyle = FlatStyle.Flat;
        _cmbBestOf.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        _cmbBestOf.Items.AddRange(["1", "3", "5"]);
        _cmbBestOf.SelectedIndex = 0;

        Controls.Add(lblTitle);
        Controls.Add(lblBestOf);
        Controls.Add(_cmbBestOf);

        if (includeDifficulty)
        {
            var lblDifficulty = new Label
            {
                Text = "Difficulty",
                Left = 20,
                Top = 98,
                Width = 100,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                ForeColor = Color.WhiteSmoke
            };

            _cmbDifficulty.Left = 140;
            _cmbDifficulty.Top = 94;
            _cmbDifficulty.Width = 150;
            _cmbDifficulty.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbDifficulty.FlatStyle = FlatStyle.Flat;
            _cmbDifficulty.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _cmbDifficulty.Items.AddRange(Enum.GetNames<DifficultyLevel>());
            _cmbDifficulty.SelectedItem = DifficultyLevel.Normal.ToString();

            Controls.Add(lblDifficulty);
            Controls.Add(_cmbDifficulty);
        }

        var btnStart = new Button
        {
            Text = "Start",
            Left = 40,
            Top = includeDifficulty ? 152 : 108,
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.OK
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            Left = 170,
            Top = includeDifficulty ? 152 : 108,
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.Cancel
        };

        StyleButton(btnStart, Color.FromArgb(33, 150, 243));
        StyleButton(btnCancel, Color.FromArgb(96, 125, 139));

        btnStart.Click += (_, _) =>
        {
            BestOf = int.Parse(_cmbBestOf.SelectedItem?.ToString() ?? "1");
            Difficulty = Enum.TryParse<DifficultyLevel>(_cmbDifficulty.SelectedItem?.ToString(), out var diff)
                ? diff
                : DifficultyLevel.Easy;
        };

        Controls.Add(btnStart);
        Controls.Add(btnCancel);

        AcceptButton = btnStart;
        CancelButton = btnCancel;
    }

    private static void StyleButton(Button button, Color color)
    {
        button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        UiStyling.StyleInteractiveButton(button, color, cornerRadius: 10);
    }
}
