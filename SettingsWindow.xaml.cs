using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace DesktopFolder
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = Settings.Current;
            IconsPerRowTextBox.Text = settings.IconsPerRow.ToString();
            ShowCollapsedNameCheckBox.IsChecked = settings.ShowCollapsedName;
            ShowExpandedNameCheckBox.IsChecked = settings.ShowExpandedName;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = Settings.Current;

            if (int.TryParse(IconsPerRowTextBox.Text, out int iconsPerRow) && iconsPerRow > 0)
            {
                settings.IconsPerRow = iconsPerRow;
            }
            else
            {
                MessageBox.Show("Введите положительное число для количества иконок",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            settings.ShowCollapsedName = ShowCollapsedNameCheckBox.IsChecked == true;
            settings.ShowExpandedName = ShowExpandedNameCheckBox.IsChecked == true;

            settings.Save();
            this.Close();
        }
    }
}