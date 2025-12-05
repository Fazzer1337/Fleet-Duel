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

            if (isHunting && hitCells.Count > 0)
            {
                Point lastHit = hitCells.Last();
                AddAdjacentTargets(lastHit);

                if (hitCells.Count >= 2)
                {
                    var a = hitCells[hitCells.Count - 2];
                    var b = hitCells[hitCells.Count - 1];
                    bool horizontal = a.Y == b.Y;

                    potentialTargets = potentialTargets
                        .OrderByDescending(p => horizontal ? (p.Y == a.Y ? 1 : 0) : (p.X == a.X ? 1 : 0))
                        .ToList();
                }

                if (potentialTargets.Count > 0)
                {
                    int index = random.Next(potentialTargets.Count);
                    shot = potentialTargets[index];
                    potentialTargets.RemoveAt(index);

                    if (availableShots.Contains(shot))
                    {
                        availableShots.Remove(shot);
                        return shot;
                    }
                }
            }

            var searchCells = availableShots
                .Where(p => (((int)p.X + (int)p.Y) % 2) == 0)
                .ToList();

            if (searchCells.Count == 0)
                searchCells = availableShots.ToList();

            int randIndex = random.Next(searchCells.Count);
            shot = searchCells[randIndex];
            availableShots.Remove(shot);

            return shot;
        }

        private void AddAdjacentTargets(Point center)
        {
            Point[] directions = {
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(0, -1)
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
                    ClearTargetsAroundShip(shot);
                    hitCells.Clear();
                    isHunting = false;
                    potentialTargets.Clear();
                }
            }
            else if (result == CellState.Miss)
            {
                potentialTargets.Remove(shot);
            }
        }

        private void ClearTargetsAroundShip(Point shot)
        {
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
