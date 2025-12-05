using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class MediumAIPlayer : IAIPlayer
    {
        private Random random;
        private GameBoard targetBoard;
        private List<Point> availableShots;
        private List<Point> hitCells;
        private bool isHunting;
        private List<Point> potentialTargets;

        public MediumAIPlayer(GameBoard targetBoard)
        {
            random = new Random();
            this.targetBoard = targetBoard;
            hitCells = new List<Point>();
            potentialTargets = new List<Point>();
            InitializeAvailableShots();
        }

        private void InitializeAvailableShots()
        {
            availableShots = new List<Point>();
            for (int x = 0; x < GameBoard.BoardSize; x++)
                for (int y = 0; y < GameBoard.BoardSize; y++)
                    availableShots.Add(new Point(x, y));

            hitCells.Clear();
            potentialTargets.Clear();
            isHunting = false;
        }

        public Point MakeMove()
        {
            if (availableShots.Count == 0)
                return new Point(-1, -1);

            Point shot;

            // Если есть попадания, переходим в режим охоты
            if (isHunting && hitCells.Count > 0)
            {
                // Берем последнее попадание
                Point lastHit = hitCells.Last();

                // Добавляем соседние клетки как потенциальные цели
                AddAdjacentTargets(lastHit);

                // Если есть потенциальные цели, стреляем по ним
                if (potentialTargets.Count > 0)
                {
                    shot = potentialTargets[0];
                    potentialTargets.RemoveAt(0);

                    if (availableShots.Contains(shot))
                    {
                        availableShots.Remove(shot);
                        return shot;
                    }
                }
            }

            // Иначе случайный выстрел
            int index = random.Next(availableShots.Count);
            shot = availableShots[index];
            availableShots.RemoveAt(index);

            return shot;
        }

        private void AddAdjacentTargets(Point center)
        {
            // Добавляем 4 направления
            Point[] directions = {
                new Point(1, 0),   // вправо
                new Point(-1, 0),  // влево
                new Point(0, 1),   // вниз
                new Point(0, -1)   // вверх
            };

            foreach (var dir in directions)
            {
                Point target = new Point(center.X + dir.X, center.Y + dir.Y);

                if (IsValidTarget(target) &&
                    !potentialTargets.Contains(target) &&
                    availableShots.Contains(target))
                {
                    potentialTargets.Add(target);
                }
            }
        }

        private bool IsValidTarget(Point point)
        {
            return point.X >= 0 && point.X < GameBoard.BoardSize &&
                   point.Y >= 0 && point.Y < GameBoard.BoardSize;
        }

        public void UpdateAfterShot(Point shot, CellState result)
        {
            if (result == CellState.Hit || result == CellState.Destroyed)
            {
                hitCells.Add(shot);
                isHunting = true;

                if (result == CellState.Destroyed)
                {
                    // При уничтожении корабля очищаем цели вокруг него
                    ClearTargetsAroundShip(shot);
                    hitCells.Clear();
                    isHunting = false;
                }
            }
            else if (result == CellState.Miss)
            {
                // Удаляем промахнувшуюся цель из потенциальных
                potentialTargets.Remove(shot);
            }
        }

        private void ClearTargetsAroundShip(Point shot)
        {
            // Удаляем все клетки в радиусе 1 от выстрела
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Point target = new Point(shot.X + dx, shot.Y + dy);
                    potentialTargets.Remove(target);
                    availableShots.Remove(target);
                }
            }
        }

        public void Reset()
        {
            InitializeAvailableShots();
        }
    }
}
