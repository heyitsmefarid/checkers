using System;
using System.Drawing;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Form3 : Form
    {
        private readonly Label _lblWelcome = new();

        public Form3()
        {
            InitializeComponent();
            StyleUi();
            BuildHeader();
        }

        private void BuildHeader()
        {
            _lblWelcome.Left = 18;
            _lblWelcome.Top = 12;
            _lblWelcome.Width = 640;
            _lblWelcome.Height = 30;
            _lblWelcome.Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
            _lblWelcome.ForeColor = Color.Gold;
            _lblWelcome.BackColor = Color.FromArgb(120, 14, 14, 20);
            _lblWelcome.Text = AppSession.CurrentPlayer is null
                ? "Welcome to Checkers"
                : $"Welcome, {AppSession.CurrentPlayer.FullName} (@{AppSession.CurrentPlayer.Username})";
            Controls.Add(_lblWelcome);
            _lblWelcome.BringToFront();
        }

        private void StyleUi()
        {
            Text = "Checkers - Main Menu";
            DoubleBuffered = true;

            btnplay2players.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnvscomputer.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnleaderboards.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            btnsettings.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);

            StyleMenuButton(btnplay2players, Color.FromArgb(33, 150, 243), Size.Empty);
            StyleMenuButton(btnvscomputer, Color.FromArgb(76, 175, 80), Size.Empty);
            StyleMenuButton(btnleaderboards, Color.FromArgb(255, 152, 0), new Size(120, 30));
            StyleMenuButton(btnsettings, Color.FromArgb(96, 125, 139), new Size(120, 30));
        }

        private static void StyleMenuButton(Button button, Color color, Size overrideSize)
        {
            if (!overrideSize.IsEmpty)
            {
                button.Size = overrideSize;
            }

            UiStyling.StyleInteractiveButton(button, color, cornerRadius: 11);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            using var setup = new GameSetupForm(includeDifficulty: false);
            if (setup.ShowDialog(this) != DialogResult.OK)
            {
                Show();
                Activate();
                return;
            }

            using var game = new GameForm(vsComputer: false, bestOf: setup.BestOf);
            game.ShowDialog(this);
            Show();
            Activate();
        }

        private void btnvscomputer_Click(object sender, EventArgs e)
        {
            Hide();
            using var setup = new GameSetupForm(includeDifficulty: true);
            if (setup.ShowDialog(this) != DialogResult.OK)
            {
                Show();
                Activate();
                return;
            }

            using var game = new GameForm(vsComputer: true, bestOf: setup.BestOf, difficulty: setup.Difficulty);
            game.ShowDialog(this);
            Show();
            Activate();
        }

        private void btnleaderboards_Click(object sender, EventArgs e)
        {
            Hide();
            using var leaderboard = new LeaderboardForm();
            leaderboard.ShowDialog(this);
            Show();
            Activate();
        }

        private void btnsettings_Click(object sender, EventArgs e)
        {
            Hide();
            using var settings = new SettingsForm();
            settings.ShowDialog(this);
            Show();
            Activate();
        }
    }
}
