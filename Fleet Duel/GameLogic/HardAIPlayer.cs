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

        private Point? firstHit;
        private Point? lastHit;
        private Point? currentDirection;
        private List<Point> triedDirections;
        private bool isSearchingDirection;
        private bool directionFound;
        private int hitsInDirection;
        private int cheatCounter;

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
            cheatCounter = 0;
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

            cheatCounter++;
            if (cheatCounter >= 5)
            {
                cheatCounter = 0;
                Point cheatShot = FindHiddenShipCell();
                if (cheatShot.X >= 0 && cheatShot.Y >= 0 && availableShots.Contains(cheatShot))
                {
                    availableShots.Remove(cheatShot);
                    return cheatShot;
                }
            }

            if (firstHit.HasValue && isSearchingDirection && !directionFound)
            {
                Point tryShot = TryNextDirection();
                if (tryShot.X >= 0 && tryShot.Y >= 0)
                {
                    return tryShot;
                }
            }

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
                    Point opposite = TryOppositeDirection();
                    if (opposite.X >= 0 && opposite.Y >= 0)
                        return opposite;
                }
            }

            return MakeSmartRandomShot();
        }

        private Point TryNextDirection()
        {
            Point[] directions = {
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(0, -1)
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

            if (triedDirections.Contains(oppositeDir))
                return new Point(-1, -1);

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
            int[,] probabilityMap = new int[GameBoard.BoardSize, GameBoard.BoardSize];

            foreach (var shot in availableShots)
            {
                int x = (int)shot.X;
                int y = (int)shot.Y;

                if (((x + y) % 2) == 0)
                    probabilityMap[x, y] += 2;

                probabilityMap[x, y] += CountStretchLength(x, y, 1, 0);
                probabilityMap[x, y] += CountStretchLength(x, y, -1, 0);
                probabilityMap[x, y] += CountStretchLength(x, y, 0, 1);
                probabilityMap[x, y] += CountStretchLength(x, y, 0, -1);

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
                                probabilityMap[x, y] += 40;
                            }
                        }
                    }
                }
            }

            Point bestShot = new Point(-1, -1);
            int maxProbability = -1;

            foreach (var p in availableShots)
            {
                int x = (int)p.X;
                int y = (int)p.Y;
                if (probabilityMap[x, y] > maxProbability)
                {
                    maxProbability = probabilityMap[x, y];
                    bestShot = p;
                }
            }

            if (bestShot.X >= 0 && bestShot.Y >= 0)
            {
                availableShots.Remove(bestShot);
                return bestShot;
            }

            int index = random.Next(availableShots.Count);
            Point randomShot = availableShots[index];
            availableShots.RemoveAt(index);
            return randomShot;
        }

        private int CountStretchLength(int x, int y, int dx, int dy)
        {
            int length = 0;
            int cx = x + dx;
            int cy = y + dy;

            while (cx >= 0 && cx < GameBoard.BoardSize &&
                   cy >= 0 && cy < GameBoard.BoardSize)
            {
                var state = targetBoard.GetCellState(cx, cy);
                if (state == CellState.Empty || state == CellState.Ship)
                {
                    length++;
                    cx += dx;
                    cy += dy;
                }
                else
                {
                    break;
                }
            }

            return length;
        }

        private bool IsValidShot(Point point)
        {
            return point.X >= 0 && point.X < GameBoard.BoardSize &&
                   point.Y >= 0 && point.Y < GameBoard.BoardSize;
        }

        private Point FindHiddenShipCell()
        {
            for (int x = 0; x < GameBoard.BoardSize; x++)
            {
                for (int y = 0; y < GameBoard.BoardSize; y++)
                {
                    if (targetBoard.GetCellState(x, y) == CellState.Ship)
                    {
                        Point p = new Point(x, y);
                        if (availableShots.Contains(p))
                            return p;
                    }
                }
            }
            return new Point(-1, -1);
        }

        public void UpdateAfterShot(Point shot, CellState result)
        {
            if (result == CellState.Hit)
            {
                if (!firstHit.HasValue)
                {
                    firstHit = shot;
                    lastHit = shot;
                    isSearchingDirection = true;
                    directionFound = false;
                    hitsInDirection = 1;
                }
                else if (currentDirection.HasValue)
                {
                    lastHit = shot;
                    hitsInDirection++;
                    directionFound = true;
                }
            }
            else if (result == CellState.Miss)
            {
                if (isSearchingDirection && !directionFound)
                {
                }
                else if (directionFound && currentDirection.HasValue)
                {
                    isSearchingDirection = true;
                    directionFound = false;
                }
            }
            else if (result == CellState.Destroyed)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Point p = new Point(shot.X + dx, shot.Y + dy);
                        availableShots.Remove(p);
                    }
                }

                ResetHuntingState();
            }
        }

        public void Reset()
        {
            InitializeAvailableShots();
        }
    }
}
