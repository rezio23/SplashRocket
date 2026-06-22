using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SplashRocket.Helpers;
using SplashRocket.UI;

namespace SplashRocket
{
    public class AppDialog : Form
    {
        private readonly List<(Label Label, TextBox TextBox)> _fields = new();
        private PictureBox _iconPreview = null!;
        private readonly string _defaultName;

        public AppDialog(string title, params (string Label, string DefaultValue)[] fields)
        {
            Text = title;
            Width = 520;
            Height = 170 + fields.Length * 64;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Surface;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = UiTheme.BodyFont;

            var nameField = fields.FirstOrDefault(f => f.Label == "Name");
            _defaultName = nameField.DefaultValue;

            int y = 18;
            foreach (var field in fields)
            {
                var labelText = field.Label == "Icon path" ? "Custom icon (optional)" : field.Label;
                var label = new Label
                {
                    Text = labelText,
                    Location = new Point(18, y),
                    AutoSize = true,
                    ForeColor = UiTheme.TextPrimary
                };

                var textBox = new TextBox
                {
                    Text = field.DefaultValue,
                    Width = 400,
                    Location = new Point(18, y + 22),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = UiTheme.Surface,
                    ForeColor = UiTheme.TextPrimary
                };

                Controls.Add(label);
                Controls.Add(textBox);
                _fields.Add((label, textBox));

                y += 64;
            }

            _iconPreview = new PictureBox
            {
                Size = new Size(48, 48),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(430, 18),
                BackColor = Color.Transparent,
                Visible = false
            };
            Controls.Add(_iconPreview);

            UpdateIconPreview();

            var okButton = UiTheme.CreatePrimaryButton("Save", 90, 34);
            okButton.Location = new Point(396, y + 6);
            okButton.DialogResult = DialogResult.OK;

            var cancelButton = UiTheme.CreateSecondaryButton("Cancel", 90, 34);
            cancelButton.Location = new Point(300, y + 6);
            cancelButton.DialogResult = DialogResult.Cancel;

            Controls.Add(cancelButton);
            Controls.Add(okButton);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public void AddBrowseButton(string fieldLabel, bool image)
        {
            var field = _fields.FirstOrDefault(f => f.Label.Text == fieldLabel ||
                (fieldLabel == "Icon path" && f.Label.Text == "Custom icon (optional)"));
            if (field.Label == null)
                return;

            var browse = new Button
            {
                Text = "...",
                Width = 34,
                Height = 24,
                Location = new Point(field.TextBox.Right - 34, field.TextBox.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Background,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.BodyFont
            };
            browse.FlatAppearance.BorderSize = 1;
            browse.FlatAppearance.BorderColor = UiTheme.Border;
            browse.Click += (s, e) =>
            {
                using var dialog = image
                    ? new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.ico;*.bmp|All files|*.*" }
                    : new OpenFileDialog { Filter = "All files|*.*" };
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                field.TextBox.Text = dialog.FileName;

                if (fieldLabel == "Path")
                {
                    var nameField = _fields.FirstOrDefault(f => f.Label.Text == "Name");
                    if (nameField.Label != null &&
                        (string.IsNullOrWhiteSpace(nameField.TextBox.Text) || nameField.TextBox.Text == _defaultName))
                    {
                        nameField.TextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                    }
                }

                UpdateIconPreview();
            };

            field.TextBox.Width -= 40;
            Controls.Add(browse);
        }

        public string GetValue(string fieldLabel)
        {
            var field = _fields.FirstOrDefault(f => f.Label.Text == fieldLabel ||
                (fieldLabel == "Icon path" && f.Label.Text == "Custom icon (optional)"));
            return field.TextBox?.Text ?? string.Empty;
        }

        private void UpdateIconPreview()
        {
            var pathField = _fields.FirstOrDefault(f => f.Label.Text == "Path").TextBox;
            var iconField = _fields.FirstOrDefault(f => f.Label.Text == "Custom icon (optional)").TextBox;
            if (pathField == null)
                return;

            var image = IconHelper.GetIconImage(pathField.Text, iconField?.Text, 48);
            _iconPreview.Image = image;
            _iconPreview.Visible = true;
        }
    }
}
