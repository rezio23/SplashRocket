using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SplashRocket.Helpers;
using SplashRocket.Models;
using SplashRocket.Services;
using SplashRocket.UI;

namespace SplashRocket
{
    public class EditWorkspaceForm : Form
    {
        private readonly Workspace _workspace;
        private readonly WorkspaceService _workspaceService;
        private ListView _appsList = null!;
        private ImageList _imageList = null!;

        public EditWorkspaceForm(Workspace workspace, WorkspaceService workspaceService)
        {
            Text = "Edit Workspace";
            Size = new Size(780, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Background;
            Font = UiTheme.BodyFont;
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
            RefreshList();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Edit Workspace",
                Font = UiTheme.TitleFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 20)
            };

            var addButton = UiTheme.CreateToolbarButton("+ Add", 70, 30);
            addButton.Location = new Point(452, 20);
            addButton.Click += AddButton_Click;

            var editButton = UiTheme.CreateToolbarButton("Edit", 70, 30);
            editButton.Location = new Point(530, 20);
            editButton.Click += EditButton_Click;

            var deleteButton = UiTheme.CreateToolbarButton("Delete", 70, 30);
            deleteButton.Location = new Point(608, 20);
            deleteButton.Click += DeleteButton_Click;

            var upButton = UiTheme.CreateToolbarButton("↑", 34, 30);
            upButton.Location = new Point(686, 20);
            upButton.Click += (s, e) => MoveSelected(-1);

            var downButton = UiTheme.CreateToolbarButton("↓", 34, 30);
            downButton.Location = new Point(722, 20);
            downButton.Click += (s, e) => MoveSelected(1);

            var listPanel = UiTheme.CreateCardPanel();
            listPanel.Location = new Point(24, 60);
            listPanel.Size = new Size(716, 340);

            _imageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
            _imageList.ImageSize = new Size(24, 24);

            _appsList = new ListView
            {
                Location = new Point(1, 1),
                Size = new Size(714, 338),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UiTheme.Surface,
                Font = UiTheme.BodyFont,
                SmallImageList = _imageList,
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _appsList.Columns.Add("Name", 200);
            _appsList.Columns.Add("Path", 490);

            listPanel.Controls.Add(_appsList);

            var saveButton = UiTheme.CreatePrimaryButton("Save", 100, 36);
            saveButton.Location = new Point(640, 420);
            saveButton.Click += SaveButton_Click;

            var cancelButton = UiTheme.CreateSecondaryButton("Cancel", 100, 36);
            cancelButton.Location = new Point(530, 420);
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(title);
            Controls.Add(addButton);
            Controls.Add(editButton);
            Controls.Add(deleteButton);
            Controls.Add(upButton);
            Controls.Add(downButton);
            Controls.Add(listPanel);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void RefreshList()
        {
            _appsList.Items.Clear();
            _imageList.Images.Clear();

            foreach (var app in _workspace.Apps)
            {
                var icon = IconHelper.GetIconImage(app.Path, app.IconPath, 24);
                var key = string.IsNullOrWhiteSpace(app.Path) ? Guid.NewGuid().ToString() : app.Path;
                if (!_imageList.Images.ContainsKey(key))
                    _imageList.Images.Add(key, icon);

                var item = new ListViewItem(app.Name, key);
                item.SubItems.Add(app.Path);
                item.Tag = app;
                _appsList.Items.Add(item);
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

            var name = dialog.GetValue("Name").Trim();
            var path = dialog.GetValue("Path").Trim();
            if (!ValidateApp(name, path))
                return;

            _workspace.Apps.Add(new AppItem
            {
                Name = name,
                Path = path,
                IconPath = dialog.GetValue("Icon path").Trim()
            });
            RefreshList();
            SelectLast();
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            if (_appsList.SelectedItems.Count == 0 || _appsList.SelectedItems[0].Tag is not AppItem app)
                return;

            using var dialog = new AppDialog("Edit app",
                ("Name", app.Name),
                ("Path", app.Path),
                ("Icon path", app.IconPath));

            dialog.AddBrowseButton("Path", false);
            dialog.AddBrowseButton("Icon path", true);

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var name = dialog.GetValue("Name").Trim();
            var path = dialog.GetValue("Path").Trim();
            if (!ValidateApp(name, path))
                return;

            app.Name = name;
            app.Path = path;
            app.IconPath = dialog.GetValue("Icon path").Trim();
            RefreshList();
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_appsList.SelectedItems.Count == 0 || _appsList.SelectedItems[0].Tag is not AppItem app)
                return;

            var result = MessageBox.Show($"Delete '{app.Name}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            _workspace.Apps.Remove(app);
            RefreshList();
        }

        private void MoveSelected(int direction)
        {
            if (_appsList.SelectedItems.Count == 0 || _appsList.SelectedItems[0].Tag is not AppItem app)
                return;

            var index = _workspace.Apps.IndexOf(app);
            var newIndex = index + direction;
            if (newIndex < 0 || newIndex >= _workspace.Apps.Count)
                return;

            _workspace.Apps.RemoveAt(index);
            _workspace.Apps.Insert(newIndex, app);
            RefreshList();
            _appsList.Items[newIndex].Selected = true;
            _appsList.Items[newIndex].EnsureVisible();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            _workspaceService.Save(_workspace);
            IconHelper.ClearCache();
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateApp(string name, string path)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("App name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("App path is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void SelectLast()
        {
            if (_appsList.Items.Count == 0)
                return;
            var last = _appsList.Items.Count - 1;
            _appsList.Items[last].Selected = true;
            _appsList.Items[last].EnsureVisible();
        }
    }
}
