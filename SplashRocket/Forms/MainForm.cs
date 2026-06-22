using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SplashRocket.Controls;
using SplashRocket.Helpers;
using SplashRocket.Models;
using SplashRocket.Services;
using SplashRocket.UI;

namespace SplashRocket
{
    public partial class MainForm : Form
    {
        private readonly WorkspaceService _workspaceService;
        private readonly AppLauncherService _launcherService;
        private Workspace _workspace;
        private Panel _appsPanel = null!;
        private Label _statusLabel = null!;
        private Label _emptyLabel = null!;

        public MainForm()
        {
            Text = "SplashRocket";
            Size = new Size(560, 440);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.Background;
            Font = UiTheme.BodyFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SplashRocket");
            Directory.CreateDirectory(dataDir);
            _workspaceService = new WorkspaceService(Path.Combine(dataDir, "workspace.json"));
            _launcherService = new AppLauncherService();
            _workspace = _workspaceService.Load();

            BuildUI();
            RefreshTiles();
        }

        private void BuildUI()
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = UiTheme.Background
            };

            var title = new Label
            {
                Text = "SplashRocket",
                Font = UiTheme.HeaderFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 18)
            };

            _statusLabel = new Label
            {
                Text = "0 apps",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(460, 24)
            };

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(_statusLabel);

            _appsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24),
                BackColor = UiTheme.Background
            };
            _appsPanel.Layout += AppsPanel_Layout;

            _emptyLabel = new Label
            {
                Text = "Workspace is empty. Click Edit Workspace to add apps.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                BackColor = UiTheme.Background
            };

            var runAllButton = UiTheme.CreatePrimaryButton("Run All", 120, 40);
            runAllButton.Location = new Point(24, 14);
            runAllButton.Click += RunAllButton_Click;

            var editButton = UiTheme.CreateSecondaryButton("Edit Workspace", 140, 40);
            editButton.Location = new Point(154, 14);
            editButton.Click += EditButton_Click;

            var exitButton = UiTheme.CreateSecondaryButton("Exit", 100, 40);
            exitButton.Location = new Point(420, 14);
            exitButton.Click += (s, e) => Close();

            footerPanel.Controls.Add(runAllButton);
            footerPanel.Controls.Add(editButton);
            footerPanel.Controls.Add(exitButton);

            Controls.Add(_appsPanel);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);

            AcceptButton = runAllButton;
            CancelButton = exitButton;
        }

        private void RefreshTiles()
        {
            _appsPanel.Controls.Clear();
            _statusLabel.Text = $"{_workspace.Apps.Count} app{(_workspace.Apps.Count == 1 ? "" : "s")}";

            if (_workspace.Apps.Count == 0)
            {
                _appsPanel.Controls.Add(_emptyLabel);
                return;
            }

            foreach (var app in _workspace.Apps)
            {
                var icon = IconHelper.GetIconImage(app.Path, app.IconPath, 48);
                var tile = new AppTile(app, icon);
                tile.Click += (s, e) => LaunchApp(app);
                tile.DoubleClick += (s, e) => LaunchApp(app);
                _appsPanel.Controls.Add(tile);
            }
        }

        private void AppsPanel_Layout(object? sender, LayoutEventArgs e)
        {
            if (_appsPanel.Controls.Count == 0)
                return;

            if (_appsPanel.Controls.Count == 1 && _appsPanel.Controls[0] == _emptyLabel)
            {
                _emptyLabel.Location = new Point(
                    (_appsPanel.ClientSize.Width - _emptyLabel.Width) / 2,
                    (_appsPanel.ClientSize.Height - _emptyLabel.Height) / 2);
                return;
            }

            const int tileWidth = 132;
            const int tileHeight = 112;
            const int margin = 24;

            var availableWidth = _appsPanel.ClientSize.Width - _appsPanel.Padding.Horizontal;
            var columns = Math.Max(1, availableWidth / (tileWidth + margin));
            var totalRowWidth = columns * tileWidth + (columns - 1) * margin;
            var startX = (_appsPanel.ClientSize.Width - totalRowWidth) / 2;
            var startY = _appsPanel.Padding.Top;

            for (int i = 0; i < _appsPanel.Controls.Count; i++)
            {
                var control = _appsPanel.Controls[i];
                var row = i / columns;
                var col = i % columns;
                var x = startX + col * (tileWidth + margin);
                var y = startY + row * (tileHeight + margin);
                control.Location = new Point(x, y);
            }
        }

        private void LaunchApp(AppItem app)
        {
            try
            {
                _launcherService.Run(app.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{app.Name}: {ex.Message}", "Failed to launch",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RunAllButton_Click(object? sender, EventArgs e)
        {
            if (_workspace.Apps.Count == 0)
            {
                MessageBox.Show("Workspace is empty. Click 'Edit Workspace' to add apps.", "No apps",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (sender is Button button)
                button.Enabled = false;

            var failed = new List<string>();
            foreach (var app in _workspace.Apps)
            {
                try
                {
                    _launcherService.Run(app.Path);
                }
                catch (Exception ex)
                {
                    failed.Add($"{app.Name}: {ex.Message}");
                }
            }

            if (sender is Button b)
                b.Enabled = true;

            if (failed.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, failed), "Some apps failed to launch",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            using var editor = new EditWorkspaceForm(_workspace, _workspaceService);
            if (editor.ShowDialog() == DialogResult.OK)
            {
                _workspace = _workspaceService.Load();
                IconHelper.ClearCache();
                RefreshTiles();
            }
        }
    }
}
