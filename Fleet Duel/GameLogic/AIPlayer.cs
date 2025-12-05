using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class AIPlayer
    {
        private Random random;
        private GameBoard targetBoard;
        private List<Point> availableShots;
        private List<Point> hitCells;
        private Point? lastHit;
        private bool isHuntingMode;

        public AIPlayer(GameBoard targetBoard)
        {
            random = new Random();
            this.targetBoard = targetBoard;
            hitCells = new List<Point>();
            InitializeAvailableShots();
        }

        private void InitializeAvailableShots()
        {
            availableShots = new List<Point>();
            for (int x = 0; x < GameBoard.BoardSize; x++)
                for (int y = 0; y < GameBoard.BoardSize; y++)
                    availableShots.Add(new Point(x, y));

            hitCells.Clear();
            lastHit = null;
            isHuntingMode = false;
        }

        public Point MakeMove()
        {
            if (availableShots.Count == 0)
                return new Point(-1, -1);

            Point shot;

            // Если есть подбитые клетки и мы в режиме "охоты"
            if (isHuntingMode && lastHit.HasValue)
            {
                shot = GetSmartShot();
                if (shot.X >= 0 && shot.Y >= 0)
                {
                    availableShots.Remove(shot);
                    return shot;
                }
            }

            // Случайный выстрел
            int index = random.Next(availableShots.Count);
            shot = availableShots[index];
            availableShots.RemoveAt(index);

            return shot;
        }

        private Point GetSmartShot()
        {
            // Получаем возможные направления от последнего попадания
            List<Point> possibleShots = new List<Point>();
            Point lastHitValue = lastHit.Value;

            // Проверяем все 4 направления
            Point[] directions = {
                new Point(1, 0),   // вправо
                new Point(-1, 0),  // влево
                new Point(0, 1),   // вниз
                new Point(0, -1)   // вверх
            };

            foreach (var dir in directions)
            {
                Point shot = new Point(lastHitValue.X + dir.X, lastHitValue.Y + dir.Y);
                if (IsValidShot(shot) && availableShots.Contains(shot))
                {
                    possibleShots.Add(shot);
                }
            }

            // Если есть возможные выстрелы, выбираем случайный
            if (possibleShots.Count > 0)
            {
                return possibleShots[random.Next(possibleShots.Count)];
            }

            // Если нет возможных выстрелов от последнего попадания, ищем другие подбитые клетки
            foreach (var hit in hitCells)
            {
                foreach (var dir in directions)
                {
                    Point shot = new Point(hit.X + dir.X, hit.Y + dir.Y);
                    if (IsValidShot(shot) && availableShots.Contains(shot))
                    {
                        possibleShots.Add(shot);
                    }
                }
            }

            if (possibleShots.Count > 0)
            {
                return possibleShots[random.Next(possibleShots.Count)];
            }

            return new Point(-1, -1);
        }

        private bool IsValidShot(Point point)
        {
            return point.X >= 0 && point.X < GameBoard.BoardSize &&
                   point.Y >= 0 && point.Y < GameBoard.BoardSize;
        }

        // Метод для обновления состояния AI после выстрела
        public void UpdateAfterShot(Point shot, CellState result)
        {
            if (result == CellState.Hit || result == CellState.Destroyed)
            {
                hitCells.Add(shot);
                lastHit = shot;
                isHuntingMode = true;

                if (result == CellState.Destroyed)
                {
                    // Если корабль уничтожен, выходим из режима охоты
                    isHuntingMode = false;
                    lastHit = null;

                    // Удаляем клетки вокруг уничтоженного корабля из доступных выстрелов
                    // (они уже будут отмечены как промахи в GameBoard.MarkDestroyedShip)
                    RemoveNeighborCells(shot);
                }
            }
        }

        private void RemoveNeighborCells(Point shot)
        {
            // Удаляем все клетки вокруг выстрела из доступных
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Point neighbor = new Point(shot.X + dx, shot.Y + dy);
                    if (IsValidShot(neighbor))
                    {
                        availableShots.Remove(neighbor);
                    }
                }
            }
        }

        public void Reset()
        {
            InitializeAvailableShots();
        }
    }
}
