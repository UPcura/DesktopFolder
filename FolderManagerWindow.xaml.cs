using System.Windows;
using MessageBox = System.Windows.MessageBox;


namespace DesktopFolder
{
    public partial class FolderManagerWindow : Window
    {
        public FolderManagerWindow()
        {
            InitializeComponent();
            RefreshList();
        }

        private void RefreshList()
        {
            FoldersListBox.ItemsSource = null;
            // ИЗМЕНЕНИЕ: Источник данных - это коллекция FolderWindow
            FoldersListBox.ItemsSource = MainWindow.ActiveFolders;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CreateNewFolder();
            RefreshList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // ИЗМЕНЕНИЕ: Проверяем, что выбранный элемент - это FolderWindow
            if (FoldersListBox.SelectedItem is FolderWindow selectedFolder)
            {
                MainWindow.DeleteFolder(selectedFolder);
                RefreshList();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите папку для удаления.");
            }
        }
    }
}