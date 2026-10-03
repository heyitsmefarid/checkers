using System;
using System.Drawing;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            textBox3.PasswordChar = '●';
            AcceptButton = btnEnterSignUp;
            CancelButton = btnback;
            StyleUi();
        }

        private void StyleUi()
        {
            Text = "Checkers - Sign Up";
            DoubleBuffered = true;

            label1.BackColor = Color.FromArgb(165, 18, 18, 22);
            label2.BackColor = Color.FromArgb(165, 18, 18, 22);
            label3.BackColor = Color.FromArgb(165, 18, 18, 22);
            label4.BackColor = Color.FromArgb(185, 18, 18, 22);

            label1.ForeColor = Color.WhiteSmoke;
            label2.ForeColor = Color.WhiteSmoke;
            label3.ForeColor = Color.WhiteSmoke;
            label4.ForeColor = Color.Gold;

            label1.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            label2.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            label3.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            label4.Font = new Font("Segoe UI", 20f, FontStyle.Bold);

            foreach (var textBox in new[] { txtusername, txtfullname, textBox3 })
            {
                textBox.BackColor = Color.FromArgb(248, 248, 250);
                textBox.ForeColor = Color.FromArgb(30, 30, 35);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Font = new Font("Segoe UI", 11f, FontStyle.Regular);
            }

            StyleButton(btnEnterSignUp, Color.FromArgb(33, 150, 243));
            StyleButton(btnback, Color.FromArgb(108, 117, 125));
        }

        private static void StyleButton(Button button, Color color)
        {
            button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            UiStyling.StyleInteractiveButton(button, color, cornerRadius: 10);
        }

        private void btnEnterSignUp_Click(object sender, EventArgs e)
        {
            bool success = AppSession.Auth.Register(txtusername.Text, txtfullname.Text, textBox3.Text, out var message);
            if (!success)
            {
                MessageBox.Show(message, "Sign Up Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Registration successful. You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
