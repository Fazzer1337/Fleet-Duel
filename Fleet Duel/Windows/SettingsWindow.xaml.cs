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
            foreach (ComboBoxItem item in difficultyComboBox.Items)
            {
                if (item.Tag?.ToString() == settings.Difficulty.ToString())
                {
                    difficultyComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in modeComboBox.Items)
            {
                if (item.Tag?.ToString() == settings.Mode.ToString())
                {
                    modeComboBox.SelectedItem = item;
                    break;
                }
            }

            themeComboBox.SelectedIndex = settings.DarkTheme ? 1 : 0;

            hintsCheckBox.IsChecked = settings.ShowShipPlacementHints;
            autoCompleteCheckBox.IsChecked = settings.AutoCompleteDestroyedShips;
            showShipsCheckBox.IsChecked = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

            if (modeComboBox.SelectedItem is ComboBoxItem modeItem)
            {
                string mode = modeItem.Tag?.ToString() ?? "VsAI";
                settings.Mode = mode switch
                {
                    "VsAI" => GameMode.VsAI,
                    "Hotseat" => GameMode.Hotseat,
                    _ => GameMode.VsAI
                };
            }

            settings.DarkTheme = themeComboBox.SelectedIndex == 1;
            settings.ShowShipPlacementHints = hintsCheckBox.IsChecked ?? true;
            settings.AutoCompleteDestroyedShips = autoCompleteCheckBox.IsChecked ?? true;

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
            settings = new GameSettings();
            LoadSettings();
        }
    }
}
