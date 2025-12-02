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

namespace Fleet_Duel

{
    public partial class MainWindow : Window
    {
        private GameBoard playerBoard;
        private GameBoard enemyBoard;
        private AIPlayer aiPlayer;
        private GameState currentState;
        private Ship currentShip;
        private int currentShipIndex;
        private CellControl[,] playerCells;
        private CellControl[,] enemyCells;
        private DispatcherTimer aiTimer;
        private bool isPlacingShip = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Fleet Duel - Морской бой";
            InitializeGame();
        }

        private void InitializeGame()
        {
            playerBoard = new GameBoard();
            enemyBoard = new GameBoard();

            ShipPlacement.AutoPlaceShips(enemyBoard);

            aiPlayer = new AIPlayer(playerBoard);
            currentState = GameState.PlacingShips;
            currentShipIndex = 0;

            InitializeBoards();
            SetupShipPlacement();
            UpdateStatus();

            aiTimer = new DispatcherTimer();
            aiTimer.Interval = TimeSpan.FromSeconds(0.7);
            aiTimer.Tick += AITimer_Tick;

            startButton.IsEnabled = false;
            rotateButton.IsEnabled = true;
            surrenderButton.IsEnabled = false;
        }

        private void InitializeBoards()
        {
            playerBoardPanel.Children.Clear();  // Используем новое имя
            enemyBoardPanel.Children.Clear();    // Используем новое имя

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
                        Height = 30
                    };

                    playerCell.MouseEnter += PlayerCell_MouseEnter;
                    playerCell.MouseLeave += PlayerCell_MouseLeave;
                    playerCell.MouseDown += PlayerCell_MouseDown;

                    playerBoardPanel.Children.Add(playerCell);  // Используем новое имя
                    playerCells[x, y] = playerCell;

                    var enemyCell = new CellControl
                    {
                        Position = new Point(x, y),
                        Width = 30,
                        Height = 30
                    };

                    enemyBoardPanel.Children.Add(enemyCell);    // Используем новое имя
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
                shipInfoText.Text = $"Разместите {currentShip.Size}-палубный корабль";
                isPlacingShip = true;
            }
            else
            {
                FinishShipPlacement();
            }
        }

        private void FinishShipPlacement()
        {
            isPlacingShip = false;
            shipInfoText.Text = "Все корабли размещены";
            startButton.IsEnabled = true;
            rotateButton.IsEnabled = false;
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
        }

        private void ShowShipPreview(Point position)
        {
            ClearPreview();

            if (!playerBoard.CanPlaceShip(currentShip, position))
                return;

            int dx = currentShip.IsHorizontal ? 1 : 0;
            int dy = currentShip.IsHorizontal ? 0 : 1;

            for (int i = 0; i < currentShip.Size; i++)
            {
                int x = (int)position.X + i * dx;
                int y = (int)position.Y + i * dy;

                if (x >= 0 && x < GameBoard.BoardSize && y >= 0 && y < GameBoard.BoardSize)
                {
                    playerCells[x, y].CellState = CellState.Ship;
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
                    enemyCells[x, y].CellState = enemyBoard[x, y];

                    if (enemyBoard[x, y] == CellState.Ship &&
                        currentState != GameState.PlayerWon &&
                        currentState != GameState.AIWon)
                    {
                        enemyCells[x, y].CellState = CellState.Empty;
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            switch (currentState)
            {
                case GameState.PlacingShips:
                    statusText.Text = "Расставьте корабли";
                    break;
                case GameState.PlayerTurn:
                    statusText.Text = "Ваш ход";
                    break;
                case GameState.AITurn:
                    statusText.Text = "Ход противника";
                    break;
                case GameState.PlayerWon:
                    statusText.Text = "Вы победили!";
                    break;
                case GameState.AIWon:
                    statusText.Text = "Противник победил!";
                    break;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerBoard.Ships.Count != 10)
            {
                MessageBox.Show("Разместите все корабли перед началом игры!");
                return;
            }

            currentState = GameState.PlayerTurn;
            startButton.IsEnabled = false;
            autoPlaceButton.IsEnabled = false;
            rotateButton.IsEnabled = false;
            surrenderButton.IsEnabled = true;

            foreach (var cell in enemyCells)
            {
                cell.MouseDown += EnemyCell_MouseDown;
            }

            UpdateStatus();
        }

        private void AutoPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            playerBoard.ClearBoard();
            if (ShipPlacement.AutoPlaceShips(playerBoard))
            {
                currentShipIndex = 10;
                FinishShipPlacement();
                UpdateBoardDisplay();
                MessageBox.Show("Корабли расставлены автоматически!");
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

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            currentState = GameState.AIWon;
            UpdateStatus();
            MessageBox.Show("Вы сдались. Игра окончена.");
            EndGame();
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
                MessageBox.Show("Поздравляем! Вы победили!");
                EndGame();
            }
        }

        private void AITimer_Tick(object sender, EventArgs e)
        {
            aiTimer.Stop();

            var shot = aiPlayer.MakeMove();
            int x = (int)shot.X;
            int y = (int)shot.Y;

            var result = playerBoard.MakeShot(x, y);
            UpdateBoardDisplay();

            if (result == CellState.Miss || playerBoard.AllShipsDestroyed())
            {
                currentState = GameState.PlayerTurn;
                UpdateStatus();

                if (playerBoard.AllShipsDestroyed())
                {
                    currentState = GameState.AIWon;
                    UpdateStatus();
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

            foreach (var cell in enemyCells)
            {
                cell.MouseDown -= EnemyCell_MouseDown;
            }

            startButton.IsEnabled = false;
            autoPlaceButton.IsEnabled = false;
            rotateButton.IsEnabled = false;
            surrenderButton.IsEnabled = false;
        }
    }
}