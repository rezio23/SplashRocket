using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SplashRocket.Models;
using SplashRocket.Services;

namespace SplashRocket
{
    public class EditWorkspaceForm : Form
    {
        private readonly Workspace _workspace;
        private readonly WorkspaceService _workspaceService;
        private ListBox _appsList = null!;

        public EditWorkspaceForm(Workspace workspace, WorkspaceService workspaceService)
        {
            Text = "Edit Workspace";
            Size = new Size(720, 480);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _workspaceService = workspaceService;
            _workspace = new Workspace
            {
                Name = workspace.Name,
                Apps = workspace.Apps
                    .Select(a => new AppItem { Name = a.Name, Path = a.Path, IconPath = a.IconPath })
                    .ToList()
            };

            BuildUI();
            RefreshApps();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Edit Workspace",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 44, 52),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            var addButton = CreateToolbarButton("+ Add", 0);
            addButton.Click += AddButton_Click;

            var editButton = CreateToolbarButton("Edit", 80);
            editButton.Click += EditButton_Click;

            var deleteButton = CreateToolbarButton("Delete", 160);
            deleteButton.Click += DeleteButton_Click;

            _appsList = new ListBox
            {
                Location = new Point(24, 60),
                Size = new Size(656, 300),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None,
                DisplayMember = "Name"
            };

            var saveButton = CreateButton("Save", AppDialog.AccentColor, Color.White);
            saveButton.Location = new Point(580, 380);
            saveButton.Click += SaveButton_Click;

            var cancelButton = CreateButton("Cancel", Color.White, Color.FromArgb(40, 44, 52), true);
            cancelButton.Location = new Point(470, 380);
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(title);
            Controls.Add(addButton);
            Controls.Add(editButton);
            Controls.Add(deleteButton);
            Controls.Add(_appsList);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private Button CreateButton(string text, Color backColor, Color foreColor, bool bordered = false)
        {
            var button = new Button
            {
                Text = text,
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = bordered ? 1 : 0;
            if (bordered)
                button.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 230);
            return button;
        }

        private Button CreateToolbarButton(string text, int x)
        {
            var button = new Button
            {
                Text = text,
                Width = 70,
                Height = 30,
                Location = new Point(610 - x, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppDialog.AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void RefreshApps()
        {
            _appsList.Items.Clear();
            foreach (var app in _workspace.Apps)
            {
                _appsList.Items.Add(app);
            }
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new AppDialog("Add app",
                ("Name", "New app"),
                ("Path", ""),
                ("Icon path", ""));

            dialog.AddBrowseButton("Path", false);
            dialog.AddBrowseButton("Icon path", true);

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var name = dialog.GetValue("Name");
            var path = dialog.GetValue("Path");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path))
                return;

            _workspace.Apps.Add(new AppItem
            {
                Name = name,
                Path = path,
                IconPath = dialog.GetValue("Icon path")
            });
            RefreshApps();
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            if (_appsList.SelectedItem is not AppItem app)
                return;

            using var dialog = new AppDialog("Edit app",
                ("Name", app.Name),
                ("Path", app.Path),
                ("Icon path", app.IconPath));

            dialog.AddBrowseButton("Path", false);
            dialog.AddBrowseButton("Icon path", true);

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var name = dialog.GetValue("Name");
            var path = dialog.GetValue("Path");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path))
                return;

            app.Name = name;
            app.Path = path;
            app.IconPath = dialog.GetValue("Icon path");
            RefreshApps();
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_appsList.SelectedItem is not AppItem app)
                return;

            var result = MessageBox.Show($"Delete '{app.Name}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            _workspace.Apps.Remove(app);
            RefreshApps();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            _workspaceService.Save(_workspace);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
