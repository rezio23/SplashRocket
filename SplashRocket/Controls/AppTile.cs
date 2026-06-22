using System;
using System.Drawing;
using System.Windows.Forms;
using SplashRocket.Models;
using SplashRocket.UI;

namespace SplashRocket.Controls
{
    public class AppTile : UserControl
    {
        public AppItem AppItem { get; }

        public AppTile(AppItem app, Image icon)
        {
            AppItem = app;
            Size = new Size(132, 112);
            BackColor = UiTheme.Surface;
            Margin = new Padding(12);
            Cursor = Cursors.Hand;

            var picture = new PictureBox
            {
                Image = icon,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(48, 48),
                Location = new Point(42, 12),
                BackColor = Color.Transparent
            };

            var nameLabel = new Label
            {
                Text = app.Name,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Size = new Size(116, 36),
                Location = new Point(8, 66),
                BackColor = Color.Transparent
            };

            var tip = new ToolTip();
            tip.SetToolTip(this, app.Path);
            tip.SetToolTip(picture, app.Path);
            tip.SetToolTip(nameLabel, app.Path);

            Controls.Add(picture);
            Controls.Add(nameLabel);

            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            picture.MouseEnter += OnMouseEnter;
            picture.MouseLeave += OnMouseLeave;
            nameLabel.MouseEnter += OnMouseEnter;
            nameLabel.MouseLeave += OnMouseLeave;
        }

        private void OnMouseEnter(object? sender, EventArgs e)
        {
            BackColor = UiTheme.Hover;
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            BackColor = UiTheme.Surface;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
