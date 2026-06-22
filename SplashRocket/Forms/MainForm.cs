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
        private List<Workspace> _workspaces;

        private ListBox _workspaceList = null!;
        private FlowLayoutPanel _appsPanel = null!;
        private Label _workspaceTitle = null!;
        private Workspace? _selectedWorkspace;

        private static readonly Color SidebarColor = Color.FromArgb(40, 44, 52);
        public static readonly Color AccentColor = Color.FromArgb(97, 175, 239);
        private static readonly Color CardColor = Color.White;
        private static readonly Color HoverColor = Color.FromArgb(240, 242, 245);

        public MainForm()
        {
            Text = "Barahh Launcher";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);

            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _workspaceService = new WorkspaceService(Path.Combine(dataDir, "workspace.json"));
            _launcherService = new AppLauncherService();
            _workspaces = _workspaceService.Load();

            BuildUI();
            RefreshWorkspaces();
        }

        private void BuildUI()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 240,
                IsSplitterFixed = true,
                BackColor = SidebarColor
            };

            // Sidebar
            var sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SidebarColor
            };

            var title = new Label
            {
                Text = "Workspaces",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, 18)
            };

            var addWorkspaceBtn = CreateSidebarButton("+ New workspace", 18, 56);
            addWorkspaceBtn.Click += AddWorkspace_Click;

            _workspaceList = new ListBox
            {
                Location = new Point(12, 96),
                Size = new Size(216, split.Height - 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = SidebarColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ItemHeight = 34,
                DisplayMember = "Name"
            };
            _workspaceList.SelectedIndexChanged += WorkspaceList_SelectedIndexChanged;
            _workspaceList.DrawMode = DrawMode.OwnerDrawFixed;
            _workspaceList.DrawItem += WorkspaceList_DrawItem;

            sidebar.Controls.Add(_workspaceList);
            sidebar.Controls.Add(addWorkspaceBtn);
            sidebar.Controls.Add(title);

            split.Panel1.Controls.Add(sidebar);

            // Main area
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(24)
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            _workspaceTitle = new Label
            {
                Text = "Select a workspace",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 44, 52),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            var addAppBtn = CreatePrimaryButton("+ Add app", 24);
            addAppBtn.Click += AddApp_Click;
            addAppBtn.Left = headerPanel.Width - addAppBtn.Width - 24;
            addAppBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var runAllBtn = CreatePrimaryButton("▶ Run all", 24);
            runAllBtn.Click += RunAll_Click;
            runAllBtn.Left = addAppBtn.Left - runAllBtn.Width - 12;
            runAllBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            headerPanel.Controls.Add(addAppBtn);
            headerPanel.Controls.Add(runAllBtn);
            headerPanel.Controls.Add(_workspaceTitle);

            _appsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24, 12, 24, 24),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            mainPanel.Controls.Add(_appsPanel);
            mainPanel.Controls.Add(headerPanel);

            split.Panel2.Controls.Add(mainPanel);

            Controls.Add(split);
        }

        private Button CreateSidebarButton(string text, int x, int y)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 204,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Button CreatePrimaryButton(string text, int y)
        {
            var button = new Button
            {
                Text = text,
                Height = 38,
                AutoSize = true,
                Padding = new Padding(16, 0, 16, 0),
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void WorkspaceList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0)
                return;

            var workspace = _workspaceList.Items[e.Index] as Workspace;
            if (workspace == null)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var bounds = e.Bounds;
            using var brush = new SolidBrush(selected ? AccentColor : SidebarColor);
            e.Graphics.FillRectangle(brush, bounds);

            using var textBrush = new SolidBrush(Color.White);
            var format = new StringFormat { LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(workspace.Name, _workspaceList.Font, textBrush, new RectangleF(bounds.X + 12, bounds.Y, bounds.Width - 24, bounds.Height), format);

            e.DrawFocusRectangle();
        }

        private void RefreshWorkspaces()
        {
            _workspaceList.Items.Clear();
            foreach (var workspace in _workspaces)
            {
                _workspaceList.Items.Add(workspace);
            }

            if (_workspaces.Count > 0)
            {
                _workspaceList.SelectedIndex = 0;
            }
            else
            {
                _selectedWorkspace = null;
                RefreshApps();
            }
        }

        private void WorkspaceList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedWorkspace = _workspaceList.SelectedItem as Workspace;
            RefreshApps();
        }

        private void RefreshApps()
        {
            _appsPanel.Controls.Clear();

            if (_selectedWorkspace == null)
            {
                _workspaceTitle.Text = "Select a workspace";
                _appsPanel.Controls.Add(CreateEmptyLabel("Create a workspace to get started."));
                return;
            }

            _workspaceTitle.Text = _selectedWorkspace.Name;

            if (_selectedWorkspace.Apps.Count == 0)
            {
                _appsPanel.Controls.Add(CreateEmptyLabel("This workspace is empty. Click 'Add app' to add one."));
                return;
            }

            foreach (var app in _selectedWorkspace.Apps)
            {
                _appsPanel.Controls.Add(CreateAppCard(app));
            }
        }

        private Label CreateEmptyLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 20, 0, 0)
            };
        }

        private Panel CreateAppCard(AppItem app)
        {
            var card = new Panel
            {
                Width = 160,
                Height = 180,
                Margin = new Padding(12),
                BackColor = CardColor,
                Cursor = Cursors.Hand
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(220, 224, 230), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var iconBox = new PictureBox
            {
                Size = new Size(64, 64),
                Location = new Point(48, 24),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = GetAppIcon(app)
            };

            var nameLabel = new Label
            {
                Text = app.Name,
                Location = new Point(12, 104),
                Size = new Size(136, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 44, 52)
            };

            var pathLabel = new Label
            {
                Text = app.Path,
                Location = new Point(12, 128),
                Size = new Size(136, 40),
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                AutoEllipsis = true
            };

            card.Controls.Add(pathLabel);
            card.Controls.Add(nameLabel);
            card.Controls.Add(iconBox);

            card.MouseEnter += (s, e) => card.BackColor = HoverColor;
            card.MouseLeave += (s, e) => card.BackColor = CardColor;
            card.Click += (s, e) => LaunchApp(app);
            iconBox.Click += (s, e) => LaunchApp(app);
            nameLabel.Click += (s, e) => LaunchApp(app);
            pathLabel.Click += (s, e) => LaunchApp(app);

            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit");
            editItem.Click += (s, e) => EditApp(app);
            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteApp(app);
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            card.ContextMenuStrip = contextMenu;

            return card;
        }

        private Image GetAppIcon(AppItem app)
        {
            if (!string.IsNullOrWhiteSpace(app.IconPath) && File.Exists(app.IconPath))
            {
                try
                {
                    return Image.FromFile(app.IconPath);
                }
                catch
                {
                    // fall through to default
                }
            }

            if (File.Exists(app.Path))
            {
                try
                {
                    using var icon = Icon.ExtractAssociatedIcon(app.Path);
                    if (icon != null)
                        return icon.ToBitmap();
                }
                catch
                {
                    // fall through to default
                }
            }

            return CreateDefaultIcon();
        }

        private Image CreateDefaultIcon()
        {
            var bitmap = new Bitmap(64, 64);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(220, 224, 230));
            g.FillEllipse(brush, 8, 8, 48, 48);
            using var pen = new Pen(AccentColor, 2);
            g.DrawEllipse(pen, 8, 8, 48, 48);
            return bitmap;
        }

        private void LaunchApp(AppItem app)
        {
            try
            {
                _launcherService.Run(app.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not launch {app.Name}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunAll_Click(object? sender, EventArgs e)
        {
            if (_selectedWorkspace == null || _selectedWorkspace.Apps.Count == 0)
                return;

            var failed = new List<string>();
            foreach (var app in _selectedWorkspace.Apps)
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
                MessageBox.Show(string.Join(Environment.NewLine, failed), "Some apps failed to launch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddWorkspace_Click(object? sender, EventArgs e)
        {
            using var dialog = new AppDialog("New workspace", ("Name", "My workspace"));
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var name = dialog.GetValue("Name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            _workspaces.Add(new Workspace { Name = name, Apps = new List<AppItem>() });
            _workspaceService.Save(_workspaces);
            RefreshWorkspaces();

            _workspaceList.SelectedIndex = _workspaces.Count - 1;
        }

        private void AddApp_Click(object? sender, EventArgs e)
        {
            if (_selectedWorkspace == null)
            {
                MessageBox.Show("Create and select a workspace first.", "No workspace", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

            _selectedWorkspace.Apps.Add(new AppItem
            {
                Name = name,
                Path = path,
                IconPath = dialog.GetValue("Icon path")
            });

            _workspaceService.Save(_workspaces);
            RefreshApps();
        }

        private void EditApp(AppItem app)
        {
            using var dialog = new AppDialog("Edit app",
                ("Name", app.Name),
                ("Path", app.Path),
                ("Icon path", app.IconPath));

            dialog.AddBrowseButton("Path", false);
            dialog.AddBrowseButton("Icon path", true);

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            app.Name = dialog.GetValue("Name");
            app.Path = dialog.GetValue("Path");
            app.IconPath = dialog.GetValue("Icon path");

            _workspaceService.Save(_workspaces);
            RefreshApps();
        }

        private void DeleteApp(AppItem app)
        {
            var result = MessageBox.Show($"Delete '{app.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            _selectedWorkspace?.Apps.Remove(app);
            _workspaceService.Save(_workspaces);
            RefreshApps();
        }
    }

    public class AppDialog : Form
    {
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
                BackColor = MainForm.AccentColor,
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
                using var dialog = image ? new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.ico;*.bmp|All files|*.*" } : new OpenFileDialog { Filter = "All files|*.*" };
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
