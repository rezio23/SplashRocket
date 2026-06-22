using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SplashRocket.Models;
using SplashRocket.Services;

namespace SplashRocket
{
    public partial class MainForm : Form
    {
        private readonly WorkspaceService _workspaceService;
        private readonly AppLauncherService _launcherService;
        private Workspace _workspace;

        public MainForm()
        {
            Text = "Barahh Launcher";
            Size = new Size(360, 360);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _workspaceService = new WorkspaceService(Path.Combine(dataDir, "workspace.json"));
            _launcherService = new AppLauncherService();
            _workspace = _workspaceService.Load();

            BuildUI();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Barahh Launcher",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 44, 52),
                AutoSize = true,
                Location = new Point(82, 32)
            };

            var runButton = CreateButton("Run", AppDialog.AccentColor, Color.White);
            runButton.Location = new Point(80, 90);
            runButton.Click += RunButton_Click;

            var editButton = CreateButton("Edit Workspace", AppDialog.AccentColor, Color.White);
            editButton.Location = new Point(80, 152);
            editButton.Click += EditButton_Click;

            var cancelButton = CreateButton("Cancel", Color.White, Color.FromArgb(40, 44, 52), true);
            cancelButton.Location = new Point(80, 214);
            cancelButton.Click += (s, e) => Close();

            Controls.Add(title);
            Controls.Add(runButton);
            Controls.Add(editButton);
            Controls.Add(cancelButton);

            AcceptButton = runButton;
            CancelButton = cancelButton;
        }

        private Button CreateButton(string text, Color backColor, Color foreColor, bool bordered = false)
        {
            var button = new Button
            {
                Text = text,
                Width = 200,
                Height = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = bordered ? 1 : 0;
            if (bordered)
                button.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 230);
            return button;
        }

        private void RunButton_Click(object? sender, EventArgs e)
        {
            if (_workspace.Apps.Count == 0)
            {
                MessageBox.Show("Workspace is empty. Click 'Edit Workspace' to add apps.", "No apps",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
            }
        }
    }
}
