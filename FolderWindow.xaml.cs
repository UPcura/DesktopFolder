using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using DesktopFolder;

namespace DesktopFolder
{
    public partial class FolderWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isExpanded = false;
        public string FolderName { get; set; } = "Новая папка";
        private System.Windows.Point _startPosition;
        private bool _isDragging;
        private System.Windows.Point _mouseAnchorPoint;
        public ObservableCollection<ShortcutItem> Items { get; set; }
        public ICommand LaunchAppCommand { get; }

        // Свойство для привязки
        public int IconsPerRow => Settings.Current.IconsPerRow;

        public FolderWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Items = new ObservableCollection<ShortcutItem>();
            LaunchAppCommand = new RelayCommand(LaunchApplication);
            Settings.Current.OnSettingsChanged += () => {
                UpdateUIFromSettings();
                OnPropertyChanged(nameof(IconsPerRow));
            };
            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            var settings = Settings.Current;
            FolderNameCollapsed.Visibility = settings.ShowCollapsedName ? Visibility.Visible : Visibility.Collapsed;
            FolderNameExpanded.Visibility = settings.ShowExpandedName ? Visibility.Visible : Visibility.Collapsed;

            // Принудительное обновление макета
            InvalidateVisual();
            UpdateLayout();
        }

        public FolderData GetData()
        {
            return new FolderData
            {
                Left = this.Left,
                Top = this.Top,
                Name = this.FolderName,
                ItemPaths = this.Items.Select(i => i.FilePath).ToList()
            };
        }

        public void LoadData(FolderData data)
        {
            this.Left = data.Left;
            this.Top = data.Top;
            this.FolderName = data.Name;
            foreach (var path in data.ItemPaths)
                AddShortcutItem(path);
        }

        private void ToggleState()
        {
            _isExpanded = !_isExpanded;
            if (_isExpanded)
            {
                _startPosition = new System.Windows.Point(this.Left, this.Top);
                CollapsedView.Visibility = Visibility.Collapsed;
                ExpandedView.Visibility = Visibility.Visible;
                this.SizeToContent = SizeToContent.Manual;
                UpdateUIFromSettings();
                this.Height = 300;
                this.Left = _startPosition.X;
                this.Top = _startPosition.Y;
                this.Activate();
                FolderNameExpanded.Focus();
            }
            else
            {
                _startPosition = new System.Windows.Point(this.Left, this.Top);
                CollapsedView.Visibility = Visibility.Visible;
                ExpandedView.Visibility = Visibility.Collapsed;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                this.Left = _startPosition.X;
                this.Top = _startPosition.Y;
            }
        }

        private void DraggableElement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseAnchorPoint = e.GetPosition(this);
            _isDragging = false;
            (sender as UIElement)?.CaptureMouse();
        }

        private void DraggableElement_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as UIElement)?.IsMouseCaptured == true && e.LeftButton == MouseButtonState.Pressed)
            {
                if (!_isDragging)
                {
                    System.Windows.Point currentPos = e.GetPosition(this);
                    if (Math.Abs(currentPos.X - _mouseAnchorPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(currentPos.Y - _mouseAnchorPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                        _isDragging = true;
                }
                if (_isDragging)
                {
                    System.Windows.Point currentMousePosition = e.GetPosition(this);
                    double deltaX = currentMousePosition.X - _mouseAnchorPoint.X;
                    double deltaY = currentMousePosition.Y - _mouseAnchorPoint.Y;
                    this.Left += deltaX;
                    this.Top += deltaY;
                }
            }
        }

        private void DraggableElement_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement)?.ReleaseMouseCapture();
            if (_isDragging)
                SnapToGrid();
            else
                ToggleState();
            _isDragging = false;
        }

        private void CollapsedView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DraggableElement_PreviewMouseLeftButtonDown(sender, e);
        private void CollapsedView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e) => DraggableElement_PreviewMouseMove(sender, e);
        private void CollapsedView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => DraggableElement_PreviewMouseLeftButtonUp(sender, e);

        private void ExpandedHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void SnapToGrid()
        {
            System.Windows.Point iconSpacing = GetDesktopIconSpacing();
            if (iconSpacing.X == 0 || iconSpacing.Y == 0) return;
            double gridX = Math.Round(this.Left / iconSpacing.X);
            double gridY = Math.Round(this.Top / iconSpacing.Y);
            this.Left = gridX * iconSpacing.X;
            this.Top = gridY * iconSpacing.Y;
        }

        private void Window_Activated(object sender, EventArgs e) => this.Topmost = true;
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = false;
            if (_isExpanded)
                ToggleState();
        }

        private void AddShortcutItem(string path)
        {
            if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
            {
                var icon = IconExtractor.GetIcon(path);
                if (icon != null)
                {
                    Items.Add(new ShortcutItem
                    {
                        FilePath = path,
                        Name = Path.GetFileNameWithoutExtension(path),
                        Icon = icon
                    });
                }
            }
        }

        private void LaunchApplication(object? parameter)
        {
            if (parameter is string path)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    ToggleState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка запуска: {ex.Message}");
                }
            }
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var file in files)
                    AddShortcutItem(file);
                if (!_isExpanded && Items.Any())
                    ToggleState();
            }
        }

        #region WinAPI
        private const int LVM_GETITEMSPACING = 0x1000 + 51;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, int lParam);

        private IntPtr GetDesktopListViewHandle()
        {
            IntPtr p = FindWindow("Progman", null);
            p = FindWindowEx(p, IntPtr.Zero, "SHELLDLL_DefView", null);
            p = FindWindowEx(p, IntPtr.Zero, "SysListView32", "FolderView");
            return p;
        }

        private System.Windows.Point GetDesktopIconSpacing()
        {
            IntPtr h = GetDesktopListViewHandle();
            if (h == IntPtr.Zero) return new System.Windows.Point(0, 0);

            int r = SendMessage(h, LVM_GETITEMSPACING, 0, 0);
            if (r == 0) return new System.Windows.Point(0, 0);

            int w = r & 0xFFFF;
            int he = r >> 16;
            return new System.Windows.Point(w, he);
        }
        #endregion
    }
}