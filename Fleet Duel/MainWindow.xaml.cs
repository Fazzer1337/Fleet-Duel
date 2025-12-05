using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Fleet_Duel.GameLogic;
using Fleet_Duel.Views;
using System.Linq;
using Fleet_Duel.Windows;

namespace Fleet_Duel
{
    public partial class MainWindow : Window
    {
        private GameBoard playerBoard;
        private GameBoard enemyBoard;
        private IAIPlayer aiPlayer;
        private GameState currentState;
        private Ship currentShip;
        private int currentShipIndex;
        private CellControl[,] playerCells;
        private CellControl[,] enemyCells;
        private DispatcherTimer aiTimer;
        private bool isPlacingShip = false;
        private GameSettings settings;
        private bool isAutoPlaced;

        public GameSettings CurrentSettings => settings;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Fleet Duel - Морской бой";
            settings = new GameSettings();
            InitializeGame();
            ApplyTheme();
        }

        private void InitializeGame()
        {
            playerBoard = new GameBoard();
            enemyBoard = new GameBoard();
            isAutoPlaced = false;

            while (!ShipPlacement.AutoPlaceShips(enemyBoard))
            {
            }

            aiPlayer = CreateAIPlayer(settings.Difficulty);

            currentState = GameState.PlacingShips;
            currentShipIndex = 0;

            InitializeBoards();
            SetupShipPlacement();
            UpdateStatus();

            aiTimer = new DispatcherTimer();
            aiTimer.Interval = TimeSpan.FromSeconds(settings.Difficulty == DifficultyLevel.Hard ? 1.0 : 0.7);
            aiTimer.Tick += AITimer_Tick;

            UpdateButtonStates();
        }

        private IAIPlayer CreateAIPlayer(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => new EasyAIPlayer(playerBoard),
                DifficultyLevel.Medium => new MediumAIPlayer(playerBoard),
                DifficultyLevel.Hard => new HardAIPlayer(playerBoard),
                _ => new MediumAIPlayer(playerBoard)
            };
        }

        private void InitializeBoards()
        {
            playerBoardPanel.Children.Clear();
            enemyBoardPanel.Children.Clear();

            playerCells = new CellControl[GameBoard.BoardSize, GameBoard.BoardSize];
            enemyCells = new CellControl[GameBoard.BoardSize, GameBoard.BoardSize];

            for (int y = 0; y < GameBoard.BoardSize; y++)
            {
                for (int x = 0; x < GameBoard.BoardSize; x++)
                {
                    var playerCell = new CellControl
                    {
                        Position = new Point(x, y),
                        Width = 30,
                        Height = 30,
                        IsDarkTheme = settings.DarkTheme
                    };

                    playerCell.MouseEnter += PlayerCell_MouseEnter;
                    playerCell.MouseLeave += PlayerCell_MouseLeave;
                    playerCell.MouseDown += PlayerCell_MouseDown;

                    playerBoardPanel.Children.Add(playerCell);
                    playerCells[x, y] = playerCell;

                    var enemyCell = new CellControl
                    {
                        Position = new Point(x, y),
                        Width = 30,
                        Height = 30,
                        IsDarkTheme = settings.DarkTheme
                    };

                    enemyCell.MouseDown += EnemyCell_MouseDown;
                    enemyBoardPanel.Children.Add(enemyCell);
                    enemyCells[x, y] = enemyCell;
                }
            }
            UpdateBoardDisplay();
        }

        private void SetupShipPlacement()
        {
            var fleet = ShipPlacement.CreateStandardFleet();
            if (currentShipIndex < fleet.Count)
            {
                currentShip = fleet[currentShipIndex];
                shipInfoText.Text = $"Разместите {currentShip.Size}-палубный корабль " +
                                   $"({(currentShip.IsHorizontal ? "горизонтальный" : "вертикальный")})";
                isPlacingShip = true;
                isAutoPlaced = false;
            }
            else
            {
                FinishShipPlacement();
            }
        }

        private void FinishShipPlacement()
        {
            isPlacingShip = false;
            shipInfoText.Text = $"Все корабли размещены ({playerBoard.Ships.Count}/10)";
            startButton.IsEnabled = true;
            UpdateButtonStates();
            ClearPreview();
        }

        private void PlayerCell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isPlacingShip || currentShip == null) return;

            var cell = (CellControl)sender;
            ShowShipPreview(cell.Position);
        }

        private void PlayerCell_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPlacingShip) return;
            ClearPreview();
        }

        private void PlayerCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isPlacingShip || currentShip == null) return;

            var cell = (CellControl)sender;

            if (playerBoard.PlaceShip(currentShip, cell.Position))
            {
                currentShipIndex++;
                ClearPreview();
                UpdateBoardDisplay();

                if (currentShipIndex >= 10)
                {
                    FinishShipPlacement();
                }
                else
                {
                    SetupShipPlacement();
                }
            }
            else if (settings.ShowShipPlacementHints)
            {
                var violations = playerBoard.GetPlacementViolations(currentShip, cell.Position);
                string message = "Невозможно разместить корабль здесь!\n";

                if (violations.Any(p => p.X < 0 || p.X >= GameBoard.BoardSize ||
                                       p.Y < 0 || p.Y >= GameBoard.BoardSize))
                {
                    message += "• Корабль выходит за границы поля\n";
                }

                if (violations.Count > 0)
                {
                    message += "• Корабли не должны касаться друг друга";
                }

                MessageBox.Show(message, "Ошибка размещения", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowShipPreview(Point position)
        {
            ClearPreview();

            var violations = playerBoard.GetPlacementViolations(currentShip, position);
            bool canPlace = violations.Count == 0;

            int dx = currentShip.IsHorizontal ? 1 : 0;
            int dy = currentShip.IsHorizontal ? 0 : 1;

            for (int i = 0; i < currentShip.Size; i++)
            {
                int x = (int)position.X + i * dx;
                int y = (int)position.Y + i * dy;

                if (x >= 0 && x < GameBoard.BoardSize && y >= 0 && y < GameBoard.BoardSize)
                {
                    if (canPlace)
                    {
                        playerCells[x, y].PreviewState = CellState.Ship;
                        playerCells[x, y].PreviewColor = Colors.LightGreen;
                    }
                    else if (settings.ShowShipPlacementHints)
                    {
                        playerCells[x, y].PreviewState = CellState.Hit;
                        playerCells[x, y].PreviewColor = Colors.LightCoral;
                    }
                }
            }
        }

        private void ClearPreview()
        {
            UpdateBoardDisplay();
        }

        private void UpdateBoardDisplay()
        {
            for (int x = 0; x < GameBoard.BoardSize; x++)
            {
                for (int y = 0; y < GameBoard.BoardSize; y++)
                {
                    playerCells[x, y].CellState = playerBoard[x, y];

                    if (currentState == GameState.PlacingShips ||
                        currentState == GameState.PlayerTurn ||
                        currentState == GameState.AITurn)
                    {
                        if (enemyBoard[x, y] == CellState.Ship)
                        {
                            enemyCells[x, y].CellState = CellState.Empty;
                        }
                        else
                        {
                            enemyCells[x, y].CellState = enemyBoard[x, y];
                        }
                    }
                    else
                    {
                        enemyCells[x, y].CellState = enemyBoard[x, y];
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            switch (currentState)
            {
                case GameState.PlacingShips:
                    statusText.Text = $"Расставьте корабли ({playerBoard.Ships.Count}/10)";
                    break;
                case GameState.PlayerTurn:
                    statusText.Text = "Ваш ход";
                    break;
                case GameState.AITurn:
                    statusText.Text = $"Ход противника ({settings.Difficulty})";
                    break;
                case GameState.PlayerWon:
                    statusText.Text = "Вы победили!";
                    break;
                case GameState.AIWon:
                    statusText.Text = "Противник победил!";
                    break;
            }
        }

        private void UpdateButtonStates()
        {
            startButton.IsEnabled = playerBoard.Ships.Count == 10 && currentState == GameState.PlacingShips;
            autoPlaceButton.IsEnabled = currentState == GameState.PlacingShips;
            rotateButton.IsEnabled = currentState == GameState.PlacingShips && isPlacingShip;
            surrenderButton.IsEnabled = currentState == GameState.PlayerTurn || currentState == GameState.AITurn;
            settingsButton.IsEnabled = currentState == GameState.PlacingShips;
            newGameButton.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerBoard.Ships.Count != 10)
            {
                MessageBox.Show("Разместите все 10 кораблей перед началом игры!");
                return;
            }

            if (!isAutoPlaced && playerBoard.HasAdjacentShips())
            {
                MessageBox.Show("Корабли не должны касаться друг друга! Проверьте расстановку.",
                              "Ошибка расстановки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currentState = GameState.PlayerTurn;
            UpdateButtonStates();
            UpdateStatus();

            foreach (var cell in enemyCells)
            {
                cell.MouseDown += EnemyCell_MouseDown;
            }
        }

        private void AutoPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            playerBoard.ClearBoard();
            if (ShipPlacement.AutoPlaceShips(playerBoard))
            {
                currentShipIndex = 10;
                FinishShipPlacement();
                UpdateBoardDisplay();
                isAutoPlaced = true;
                MessageBox.Show("Корабли расставлены автоматически!");
            }
            else
            {
                MessageBox.Show("Не удалось автоматически расставить корабли. Попробуйте еще раз.");
            }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentShip != null)
            {
                currentShip.IsHorizontal = !currentShip.IsHorizontal;
                shipInfoText.Text = $"Разместите {currentShip.Size}-палубный корабль " +
                                   $"({(currentShip.IsHorizontal ? "горизонтальный" : "вертикальный")})";
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Fleet_Duel.Windows.SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentState == GameState.PlayerTurn || currentState == GameState.AITurn)
            {
                if (MessageBox.Show("Текущая игра будет потеряна. Начать новую игру?",
                    "Новая игра", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (aiTimer.IsEnabled)
                aiTimer.Stop();

            foreach (var cell in enemyCells)
            {
                if (cell != null)
                    cell.MouseDown -= EnemyCell_MouseDown;
            }

            InitializeGame();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите сдаться?", "Сдача",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                currentState = GameState.AIWon;
                UpdateStatus();
                UpdateBoardDisplay();
                MessageBox.Show("Вы сдались. Игра окончена.");
                EndGame();
            }
        }

        private void EnemyCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentState != GameState.PlayerTurn) return;

            var cell = (CellControl)sender;
            int x = (int)cell.Position.X;
            int y = (int)cell.Position.Y;

            if (enemyBoard[x, y] == CellState.Hit ||
                enemyBoard[x, y] == CellState.Miss ||
                enemyBoard[x, y] == CellState.Destroyed)
                return;

            var result = enemyBoard.MakeShot(x, y);
            UpdateBoardDisplay();

            if (result == CellState.Miss)
            {
                currentState = GameState.AITurn;
                UpdateStatus();
                aiTimer.Start();
            }
            else if (enemyBoard.AllShipsDestroyed())
            {
                currentState = GameState.PlayerWon;
                UpdateStatus();
                UpdateBoardDisplay();
                MessageBox.Show("Поздравляем! Вы победили!");
                EndGame();
            }
        }

        private void AITimer_Tick(object sender, EventArgs e)
        {
            aiTimer.Stop();

            var shot = aiPlayer.MakeMove();
            if (shot.X < 0 || shot.Y < 0)
            {
                currentState = GameState.PlayerTurn;
                UpdateStatus();
                return;
            }

            int x = (int)shot.X;
            int y = (int)shot.Y;

            var result = playerBoard.MakeShot(x, y);
            aiPlayer.UpdateAfterShot(shot, result);

            UpdateBoardDisplay();

            if (result == CellState.Miss || playerBoard.AllShipsDestroyed())
            {
                currentState = GameState.PlayerTurn;
                UpdateStatus();

                if (playerBoard.AllShipsDestroyed())
                {
                    currentState = GameState.AIWon;
                    UpdateStatus();
                    UpdateBoardDisplay();
                    MessageBox.Show("Противник победил!");
                    EndGame();
                }
            }
            else
            {
                aiTimer.Start();
            }
        }

        private void EndGame()
        {
            UpdateBoardDisplay();
            UpdateButtonStates();

            foreach (var cell in enemyCells)
            {
                cell.MouseDown -= EnemyCell_MouseDown;
            }
        }

        public void ApplySettings(GameSettings newSettings)
        {
            this.settings = newSettings;
            ApplyTheme();

            if (currentState == GameState.PlayerTurn || currentState == GameState.AITurn)
            {
                aiPlayer = CreateAIPlayer(settings.Difficulty);
            }
        }

        private void ApplyTheme()
        {
            if (settings.DarkTheme)
            {
                this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                statusText.Foreground = Brushes.White;
                shipInfoText.Foreground = Brushes.White;
            }
            else
            {
                this.Background = Brushes.White;
                statusText.Foreground = Brushes.Black;
                shipInfoText.Foreground = Brushes.Black;
            }

            foreach (var cell in playerCells)
            {
                if (cell != null)
                    cell.IsDarkTheme = settings.DarkTheme;
            }

            foreach (var cell in enemyCells)
            {
                if (cell != null)
                    cell.IsDarkTheme = settings.DarkTheme;
            }
        }
    }
}
