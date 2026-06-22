using System.Drawing;
using System.Windows.Forms;

namespace SplashRocket.UI
{
    public static class UiTheme
    {
        public static Color Accent { get; } = Color.FromArgb(123, 154, 204);
        public static Color AccentHover { get; } = Color.FromArgb(105, 137, 186);
        public static Color Background { get; } = Color.FromArgb(252, 246, 245);
        public static Color Surface { get; } = Color.White;
        public static Color TextPrimary { get; } = Color.FromArgb(40, 44, 52);
        public static Color TextSecondary { get; } = Color.FromArgb(120, 126, 138);
        public static Color Border { get; } = Color.FromArgb(220, 224, 230);
        public static Color Hover { get; } = Color.FromArgb(240, 245, 252);

        public static Font HeaderFont { get; } = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static Font TitleFont { get; } = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static Font BodyFont { get; } = new Font("Segoe UI", 9F);
        public static Font BodyBoldFont { get; } = new Font("Segoe UI", 9F, FontStyle.Bold);

        public static Button CreatePrimaryButton(string text, int width = 120, int height = 40)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = BodyBoldFont,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            AttachHover(button, Accent, AccentHover);
            return button;
        }

        public static Button CreateSecondaryButton(string text, int width = 120, int height = 40)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface,
                ForeColor = TextPrimary,
                Font = BodyBoldFont,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Border;
            AttachHover(button, Surface, Hover);
            return button;
        }

        public static Button CreateToolbarButton(string text, int width = 80, int height = 30)
        {
            var button = CreatePrimaryButton(text, width, height);
            button.Font = BodyFont;
            return button;
        }

        public static Panel CreateCardPanel()
        {
            return new Panel
            {
                BackColor = Surface,
                BorderStyle = BorderStyle.None
            };
        }

        private static void AttachHover(Button button, Color normal, Color hover)
        {
            button.MouseEnter += (s, e) => button.BackColor = hover;
            button.MouseLeave += (s, e) => button.BackColor = normal;
        }
    }
}
