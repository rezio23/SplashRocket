using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SplashRocket
{
    public class AppDialog : Form
    {
        public static readonly Color AccentColor = Color.FromArgb(97, 175, 239);

        private readonly List<(Label Label, TextBox TextBox)> _fields = new();

        public AppDialog(string title, params (string Label, string DefaultValue)[] fields)
        {
            Text = title;
            Width = 460;
            Height = 140 + fields.Length * 64;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9F);

            int y = 18;
            foreach (var field in fields)
            {
                var label = new Label
                {
                    Text = field.Label,
                    Location = new Point(18, y),
                    AutoSize = true
                };

                var textBox = new TextBox
                {
                    Text = field.DefaultValue,
                    Width = 408,
                    Location = new Point(18, y + 22)
                };

                Controls.Add(label);
                Controls.Add(textBox);
                _fields.Add((label, textBox));

                y += 64;
            }

            var okButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Width = 90,
                Height = 34,
                Location = new Point(336, y + 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            okButton.FlatAppearance.BorderSize = 0;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 90,
                Height = 34,
                Location = new Point(240, y + 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 44, 52)
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 230);
            cancelButton.FlatAppearance.BorderSize = 1;

            Controls.Add(cancelButton);
            Controls.Add(okButton);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public void AddBrowseButton(string fieldLabel, bool image)
        {
            var field = _fields.FirstOrDefault(f => f.Label.Text == fieldLabel);
            if (field.Label == null)
                return;

            var browse = new Button
            {
                Text = "...",
                Width = 34,
                Height = 24,
                Location = new Point(field.TextBox.Right - 34, field.TextBox.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            browse.FlatAppearance.BorderSize = 1;
            browse.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 230);
            browse.Click += (s, e) =>
            {
                using var dialog = image
                    ? new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.ico;*.bmp|All files|*.*" }
                    : new OpenFileDialog { Filter = "All files|*.*" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    field.TextBox.Text = dialog.FileName;
            };

            field.TextBox.Width -= 40;
            Controls.Add(browse);
        }

        public string GetValue(string fieldLabel)
        {
            var field = _fields.FirstOrDefault(f => f.Label.Text == fieldLabel);
            return field.TextBox?.Text ?? string.Empty;
        }
    }
}
