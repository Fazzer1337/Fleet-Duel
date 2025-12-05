using System;
using System.Windows;
using System.Windows.Controls;
using Fleet_Duel.GameLogic;

namespace Fleet_Duel.Windows
{
    public partial class SettingsWindow : Window
    {
        private GameSettings settings;
        private MainWindow mainWindow;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.settings = mainWindow.CurrentSettings;
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Загружаем уровень сложности
            foreach (ComboBoxItem item in difficultyComboBox.Items)
            {
                if (item.Tag?.ToString() == settings.Difficulty.ToString())
                {
                    difficultyComboBox.SelectedItem = item;
                    break;
                }
            }

            // Загружаем тему
            themeComboBox.SelectedIndex = settings.DarkTheme ? 1 : 0;

            // Загружаем чекбоксы
            hintsCheckBox.IsChecked = settings.ShowShipPlacementHints;
            autoCompleteCheckBox.IsChecked = settings.AutoCompleteDestroyedShips;
            showShipsCheckBox.IsChecked = true; // По умолчанию
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем уровень сложности
            if (difficultyComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string difficulty = selectedItem.Tag?.ToString() ?? "Medium";
                settings.Difficulty = difficulty switch
                {
                    "Easy" => DifficultyLevel.Easy,
                    "Medium" => DifficultyLevel.Medium,
                    "Hard" => DifficultyLevel.Hard,
                    _ => DifficultyLevel.Medium
                };
            }

            // Сохраняем тему
            settings.DarkTheme = themeComboBox.SelectedIndex == 1;

            // Сохраняем чекбоксы
            settings.ShowShipPlacementHints = hintsCheckBox.IsChecked ?? true;
            settings.AutoCompleteDestroyedShips = autoCompleteCheckBox.IsChecked ?? true;

            // Применяем настройки к главному окну
            mainWindow.ApplySettings(settings);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем настройки по умолчанию
            settings = new GameSettings();
            LoadSettings();
        }
    }
}
