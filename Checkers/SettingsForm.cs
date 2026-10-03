namespace Checkers;

public class SettingsForm : Form
{
    public SettingsForm()
    {
        Text = "Game Settings";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(760, 420);
        BackColor = Color.FromArgb(28, 28, 34);
        ForeColor = Color.WhiteSmoke;

        var lblTitle = new Label
        {
            Text = "CHECKERS SETTINGS",
            Left = 18,
            Top = 14,
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.Gold
        };

        var grpAudio = new GroupBox
        {
            Text = "Audio",
            Left = 18,
            Top = 56,
            Width = 350,
            Height = 165,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
        };

        var chkMusic = new CheckBox
        {
            Text = "Enable background music (bgsound.mp3)",
            Left = 18,
            Top = 32,
            Width = 290,
            ForeColor = Color.WhiteSmoke,
            Checked = AppSession.Settings.EnableBackgroundMusic,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        var lblVolume = new Label
        {
            Text = "Volume",
            Left = 18,
            Top = 70,
            Width = 60,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
        };

        var trackVolume = new TrackBar
        {
            Left = 80,
            Top = 62,
            Width = 190,
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 10,
            Value = Math.Clamp(AppSession.Settings.MusicVolumePercent, 0, 100)
        };

        var lblVolumeValue = new Label
        {
            Left = 280,
            Top = 70,
            Width = 50,
            ForeColor = Color.Gold,
            Text = $"{trackVolume.Value}%",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold)
        };
        trackVolume.Scroll += (_, _) => lblVolumeValue.Text = $"{trackVolume.Value}%";

        grpAudio.Controls.Add(chkMusic);
        grpAudio.Controls.Add(lblVolume);
        grpAudio.Controls.Add(trackVolume);
        grpAudio.Controls.Add(lblVolumeValue);

        var grpGameplay = new GroupBox
        {
            Text = "Gameplay",
            Left = 390,
            Top = 56,
            Width = 350,
            Height = 165,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
        };

        var chkHighlight = new CheckBox
        {
            Text = "Highlight valid moves",
            Left = 18,
            Top = 32,
            Width = 220,
            ForeColor = Color.WhiteSmoke,
            Checked = AppSession.Settings.HighlightValidMoves,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        var lblTheme = new Label
        {
            Text = "Board theme",
            Left = 18,
            Top = 58,
            Width = 120,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        var cmbTheme = new ComboBox
        {
            Left = 18,
            Top = 76,
            Width = 160,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };
        cmbTheme.Items.AddRange(Enum.GetNames<BoardTheme>());
        cmbTheme.SelectedItem = AppSession.Settings.BoardTheme.ToString();

        var lblHint = new Label
        {
            Left = 18,
            Top = 112,
            Width = 320,
            Height = 45,
            ForeColor = Color.Silver,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Text = "Changes apply to newly opened game screens."
        };

        grpGameplay.Controls.Add(chkHighlight);
        grpGameplay.Controls.Add(lblTheme);
        grpGameplay.Controls.Add(cmbTheme);
        grpGameplay.Controls.Add(lblHint);

        var grpDatabase = new GroupBox
        {
            Text = "Database (Advanced)",
            Left = 18,
            Top = 236,
            Width = 722,
            Height = 110,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
        };

        var txtConnection = new TextBox
        {
            Left = 16,
            Top = 30,
            Width = 690,
            Text = AppSession.Database.ConnectionString,
            BackColor = Color.FromArgb(246, 246, 248),
            ForeColor = Color.FromArgb(28, 28, 34),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9f, FontStyle.Regular)
        };

        var btnTestDb = new Button
        {
            Text = "Test DB",
            Left = 16,
            Top = 64,
            Width = 90
        };
        StyleActionButton(btnTestDb, Color.FromArgb(56, 126, 203));

        var lblDbResult = new Label
        {
            Left = 120,
            Top = 68,
            Width = 580,
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        btnTestDb.Click += (_, _) =>
        {
            AppSession.Database.UpdateConnectionString(txtConnection.Text);
            if (AppSession.Database.EnsureDatabaseSchema(out var error))
            {
                lblDbResult.ForeColor = Color.LightGreen;
                lblDbResult.Text = "Database connected and schema is ready.";
            }
            else
            {
                lblDbResult.ForeColor = Color.OrangeRed;
                lblDbResult.Text = $"Database error: {error}";
            }
        };

        grpDatabase.Controls.Add(txtConnection);
        grpDatabase.Controls.Add(btnTestDb);
        grpDatabase.Controls.Add(lblDbResult);

        var btnSave = new Button
        {
            Text = "Save Settings",
            Left = 514,
            Top = 360,
            Width = 110,
            Height = 34
        };
        StyleActionButton(btnSave, Color.FromArgb(66, 145, 75));

        var btnClose = new Button
        {
            Text = "Close",
            Left = 630,
            Top = 360,
            Width = 110,
            Height = 34
        };
        StyleActionButton(btnClose, Color.FromArgb(90, 90, 96));

        var lblSaveResult = new Label
        {
            Left = 20,
            Top = 368,
            Width = 480,
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular)
        };

        btnSave.Click += (_, _) =>
        {
            AppSession.Settings.EnableBackgroundMusic = chkMusic.Checked;
            AppSession.Settings.MusicVolumePercent = trackVolume.Value;
            AppSession.Settings.HighlightValidMoves = chkHighlight.Checked;
            AppSession.Settings.BoardTheme = Enum.TryParse<BoardTheme>(cmbTheme.SelectedItem?.ToString(), out var selectedTheme)
                ? selectedTheme
                : BoardTheme.BlackWhite;

            AppSession.Database.UpdateConnectionString(txtConnection.Text);

            bool ok = AppSession.Music.ApplySettings(AppSession.Settings);
            if (ok)
            {
                lblSaveResult.ForeColor = Color.LightGreen;
                lblSaveResult.Text = "Settings saved.";
            }
            else
            {
                lblSaveResult.ForeColor = Color.Orange;
                lblSaveResult.Text = $"Settings saved, but music issue: {AppSession.Music.LastError}";
            }
        };

        btnClose.Click += (_, _) => Close();

        Controls.Add(lblTitle);
        Controls.Add(grpAudio);
        Controls.Add(grpGameplay);
        Controls.Add(grpDatabase);
        Controls.Add(btnSave);
        Controls.Add(btnClose);
        Controls.Add(lblSaveResult);
    }

    private static void StyleActionButton(Button button, Color backColor)
    {
        button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        UiStyling.StyleInteractiveButton(button, backColor, cornerRadius: 9);
    }
}
