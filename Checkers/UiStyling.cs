namespace Checkers;

public static class UiStyling
{
    public static void StyleInteractiveButton(Button button, Color baseColor, int cornerRadius = 10)
    {
        Color hoverColor = Lighten(baseColor, 16);
        Color pressedColor = Lighten(baseColor, -14);

        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = baseColor;
        button.ForeColor = Color.White;
        button.Cursor = Cursors.Hand;

        ApplyRoundedCorners(button, cornerRadius);

        button.MouseEnter += (_, _) => button.BackColor = hoverColor;
        button.MouseLeave += (_, _) => button.BackColor = baseColor;
        button.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                button.BackColor = pressedColor;
            }
        };
        button.MouseUp += (_, _) =>
        {
            var clientPos = button.PointToClient(Cursor.Position);
            button.BackColor = button.ClientRectangle.Contains(clientPos) ? hoverColor : baseColor;
        };
    }

    public static void ApplyRoundedCorners(Control control, int radius)
    {
        if (radius <= 0)
        {
            return;
        }

        void UpdateRegion()
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using var path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
            control.Region?.Dispose();
            control.Region = new Region(path);
        }

        control.SizeChanged += (_, _) => UpdateRegion();
        UpdateRegion();
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        int diameter = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();

        path.StartFigure();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static Color Lighten(Color color, int amount)
    {
        int Clamp(int value) => Math.Max(0, Math.Min(255, value));
        return Color.FromArgb(color.A, Clamp(color.R + amount), Clamp(color.G + amount), Clamp(color.B + amount));
    }
}
