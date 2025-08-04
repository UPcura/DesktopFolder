using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;
using DesktopFolder;

namespace DesktopFolder
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private FolderManagerWindow? _folderManagerWindow;
        private SettingsWindow? _settingsWindow;

        public static List<FolderWindow> ActiveFolders { get; private set; } = new List<FolderWindow>();

        public MainWindow()
        {
            InitializeComponent();
            this.Hide();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new Icon("folder.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Папки на рабочем столе";

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Менеджер папок...", null, OnFolderManagerClicked);
            _notifyIcon.ContextMenuStrip.Items.Add("Параметры...", null, OnSettingsClicked);
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, OnExitClicked);

            LoadFolders();
        }

        private void OnFolderManagerClicked(object? sender, EventArgs e)
        {
            if (_folderManagerWindow == null || !_folderManagerWindow.IsVisible)
            {
                _folderManagerWindow = new FolderManagerWindow();
                _folderManagerWindow.Show();
            }
            else
            {
                _folderManagerWindow.Activate();
            }
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            SaveFolders();
            _notifyIcon.Dispose();
            foreach (var folder in ActiveFolders.ToList())
            {
                folder.Close();
            }
            System.Windows.Application.Current.Shutdown();
        }

        public static void CreateNewFolder()
        {
            var newFolder = new FolderWindow();
            ActiveFolders.Add(newFolder);
            newFolder.Show();
        }

        public static void DeleteFolder(FolderWindow folder)
        {
            if (ActiveFolders.Contains(folder))
            {
                folder.Close();
                ActiveFolders.Remove(folder);
            }
        }

        private void LoadFolders()
        {
            if (File.Exists("folders.json"))
            {
                try
                {
                    var json = File.ReadAllText("folders.json");
                    var folderData = JsonConvert.DeserializeObject<List<FolderData>>(json);
                    if (folderData == null) return;

                    foreach (var data in folderData)
                    {
                        var folderWindow = new FolderWindow();
                        folderWindow.LoadData(data);
                        ActiveFolders.Add(folderWindow);
                        folderWindow.Show();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось загрузить папки: " + ex.Message);
                }
            }
        }

        public static void SaveFolders()
        {
            var folderData = ActiveFolders.Select(f => f.GetData()).ToList();
            var json = JsonConvert.SerializeObject(folderData, Formatting.Indented);
            File.WriteAllText("folders.json", json);
        }
    }
}