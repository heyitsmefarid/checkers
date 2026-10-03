using System;
using System.Drawing;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtpassword.PasswordChar = '●';
            AcceptButton = btnlogin;
            StyleUi();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!AppSession.Database.EnsureDatabaseSchema(out var error))
            {
                MessageBox.Show(
                    $"Database connection failed. You can still open Settings to fix it.\n\n{error}",
                    "Database Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void StyleUi()
        {
            Text = "Checkers - Login";
            DoubleBuffered = true;

            var labelBack = Color.FromArgb(165, 18, 18, 22);

            foreach (var label in new[] { label1, label2, label3 })
            {
                label.BackColor = labelBack;
                label.ForeColor = Color.WhiteSmoke;
                label.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            }

            label3.ForeColor = Color.Gold;
            label3.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            foreach (var textBox in new[] { txtusername, txtpassword })
            {
                textBox.Font = new Font("Segoe UI", 11f, FontStyle.Regular);
                textBox.BackColor = Color.FromArgb(248, 248, 250);
                textBox.ForeColor = Color.FromArgb(30, 30, 35);
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }

            StylePrimaryButton(btnlogin, Color.FromArgb(33, 150, 243));
            StylePrimaryButton(btnsignup, Color.FromArgb(0, 150, 136));
        }

        private static void StylePrimaryButton(Button button, Color color)
        {
            button.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            UiStyling.StyleInteractiveButton(button, color, cornerRadius: 10);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var account = AppSession.Auth.Login(txtusername.Text, txtpassword.Text, out var message);
            if (account is null)
            {
                MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSession.CurrentPlayer = account;

            Hide();
            using (var menu = new Form3())
            {
                menu.ShowDialog();
            }

            Close();
        }

        private void btnsignup_Click(object sender, EventArgs e)
        {
            Hide();
            using (var signup = new Form2())
            {
                signup.ShowDialog();
            }
            Show();
            Activate();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void txtpassword_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtusername_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
