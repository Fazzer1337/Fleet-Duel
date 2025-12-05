using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class HardAIPlayer : IAIPlayer
    {
        private Random random;
        private GameBoard targetBoard;
        private List<Point> availableShots;

        // Состояние для продвинутой логики
        private Point? firstHit;
        private Point? lastHit;
        private Point? currentDirection;
        private List<Point> triedDirections;
        private bool isSearchingDirection;
        private bool directionFound;
        private int hitsInDirection;

        public HardAIPlayer(GameBoard targetBoard)
        {
            random = new Random();
            this.targetBoard = targetBoard;
            triedDirections = new List<Point>();
            InitializeAvailableShots();
        }

        private void InitializeAvailableShots()
        {
            availableShots = new List<Point>();
            for (int x = 0; x < GameBoard.BoardSize; x++)
                for (int y = 0; y < GameBoard.BoardSize; y++)
                    availableShots.Add(new Point(x, y));

            ResetHuntingState();
        }

        private void ResetHuntingState()
        {
            firstHit = null;
            lastHit = null;
            currentDirection = null;
            triedDirections.Clear();
            isSearchingDirection = false;
            directionFound = false;
            hitsInDirection = 0;
        }

        public Point MakeMove()
        {
            if (availableShots.Count == 0)
                return new Point(-1, -1);

            // Если есть первое попадание и мы ищем направление
            if (firstHit.HasValue && isSearchingDirection && !directionFound)
            {
                // Пробуем направления, которые еще не пробовали
                Point tryShot = TryNextDirection();
                if (tryShot.X >= 0 && tryShot.Y >= 0)
                {
                    return tryShot;
                }
            }

            // Если направление найдено, продолжаем в том же направлении
            if (directionFound && currentDirection.HasValue && lastHit.HasValue)
            {
                Point nextShot = new Point(
                    lastHit.Value.X + currentDirection.Value.X,
                    lastHit.Value.Y + currentDirection.Value.Y
                );

                if (IsValidShot(nextShot) && availableShots.Contains(nextShot))
                {
                    availableShots.Remove(nextShot);
                    return nextShot;
                }
                else
                {
                    // Если нельзя стрелять в текущем направлении, пробуем противоположное
                    return TryOppositeDirection();
                }
            }

            // Иначе - умный случайный выстрел с приоритетом по клеткам с максимальной вероятностью
            return MakeSmartRandomShot();
        }

        private Point TryNextDirection()
        {
            Point[] directions = {
                new Point(1, 0),   // вправо
                new Point(-1, 0),  // влево
                new Point(0, 1),   // вниз
                new Point(0, -1)   // вверх
            };

            foreach (var dir in directions)
            {
                if (!triedDirections.Contains(dir))
                {
                    Point potentialShot = new Point(
                        firstHit.Value.X + dir.X,
                        firstHit.Value.Y + dir.Y
                    );

                    if (IsValidShot(potentialShot) && availableShots.Contains(potentialShot))
                    {
                        triedDirections.Add(dir);
                        availableShots.Remove(potentialShot);
                        currentDirection = dir;
                        return potentialShot;
                    }
                    else
                    {
                        triedDirections.Add(dir);
                    }
                }
            }

            return new Point(-1, -1);
        }

        private Point TryOppositeDirection()
        {
            if (!firstHit.HasValue || !currentDirection.HasValue)
                return new Point(-1, -1);

            Point oppositeDir = new Point(
                -currentDirection.Value.X,
                -currentDirection.Value.Y
            );

            // Проверяем, не пробовали ли уже это направление
            if (triedDirections.Contains(oppositeDir))
            {
                // Уже пробовали, значит корабль уничтожен или ошибка
                return new Point(-1, -1);
            }

            // Начинаем с первой точки попадания в противоположном направлении
            Point oppositeShot = new Point(
                firstHit.Value.X + oppositeDir.X,
                firstHit.Value.Y + oppositeDir.Y
            );

            int attempts = 0;
            while (attempts < 10 && (!IsValidShot(oppositeShot) || !availableShots.Contains(oppositeShot)))
            {
                oppositeShot = new Point(
                    oppositeShot.X + oppositeDir.X,
                    oppositeShot.Y + oppositeDir.Y
                );
                attempts++;
            }

            if (IsValidShot(oppositeShot) && availableShots.Contains(oppositeShot))
            {
                triedDirections.Add(oppositeDir);
                availableShots.Remove(oppositeShot);
                currentDirection = oppositeDir;
                directionFound = true;
                return oppositeShot;
            }

            return new Point(-1, -1);
        }

        private Point MakeSmartRandomShot()
        {
            // Создаем карту вероятностей
            int[,] probabilityMap = new int[GameBoard.BoardSize, GameBoard.BoardSize];

            // Заполняем вероятности на основе известных попаданий и промахов
            foreach (var availableShot in availableShots)
            {
                int x = (int)availableShot.X;
                int y = (int)availableShot.Y;

                // Повышаем вероятность для клеток, где могли бы быть корабли
                probabilityMap[x, y] = random.Next(1, 10);

                // Проверяем соседние клетки на наличие попаданий
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < GameBoard.BoardSize && ny >= 0 && ny < GameBoard.BoardSize)
                        {
                            CellState state = targetBoard.GetCellState(nx, ny);
                            if (state == CellState.Hit)
                            {
                                probabilityMap[x, y] += 50;
                            }
                        }
                    }
                }
            }

            // Находим клетку с максимальной вероятностью
            Point bestShot = new Point(-1, -1);
            int maxProbability = -1;

            for (int x = 0; x < GameBoard.BoardSize; x++)
            {
                for (int y = 0; y < GameBoard.BoardSize; y++)
                {
                    if (probabilityMap[x, y] > maxProbability && availableShots.Contains(new Point(x, y)))
                    {
                        maxProbability = probabilityMap[x, y];
                        bestShot = new Point(x, y);
                    }
                }
            }

            if (bestShot.X >= 0 && bestShot.Y >= 0)
            {
                availableShots.Remove(bestShot);
                return bestShot;
            }

            // Fallback: случайный выстрел
            int index = random.Next(availableShots.Count);
            Point randomShot = availableShots[index];
            availableShots.RemoveAt(index);
            return randomShot;
        }

        private bool IsValidShot(Point point)
        {
            return point.X >= 0 && point.X < GameBoard.BoardSize &&
                   point.Y >= 0 && point.Y < GameBoard.BoardSize;
        }

        public void UpdateAfterShot(Point shot, CellState result)
        {
            if (result == CellState.Hit)
            {
                if (!firstHit.HasValue)
                {
                    // Первое попадание по кораблю
                    firstHit = shot;
                    lastHit = shot;
                    isSearchingDirection = true;
                    directionFound = false;
                    hitsInDirection = 1;
                }
                else if (currentDirection.HasValue)
                {
                    // Попали в том же направлении
                    lastHit = shot;
                    hitsInDirection++;
                    directionFound = true;
                }
            }
            else if (result == CellState.Miss)
            {
                if (isSearchingDirection && !directionFound)
                {
                    // Промах при поиске направления - продолжаем искать
                    // Ничего не делаем, следующий MakeMove попробует следующее направление
                }
                else if (directionFound && currentDirection.HasValue)
                {
                    // Промах после того как нашли направление
                    // Пробуем противоположное направление
                    isSearchingDirection = true;
                    directionFound = false;
                }
            }
            else if (result == CellState.Destroyed)
            {
                // Корабль уничтожен
                ResetHuntingState();

                // Удаляем все клетки вокруг уничтоженного корабля
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Point p = new Point(shot.X + dx, shot.Y + dy);
                        availableShots.Remove(p);
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
