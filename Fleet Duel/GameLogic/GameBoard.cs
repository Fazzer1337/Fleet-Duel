using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class GameBoard
    {
        public const int BoardSize = 10;
        private CellState[,] cells;
        public List<Ship> Ships { get; }

        public GameBoard()
        {
            cells = new CellState[BoardSize, BoardSize];
            Ships = new List<Ship>();
            ClearBoard();
        }

        public CellState this[int x, int y]
        {
            get => cells[x, y];
            set => cells[x, y] = value;
        }

        public void ClearBoard()
        {
            for (int i = 0; i < BoardSize; i++)
                for (int j = 0; j < BoardSize; j++)
                    cells[i, j] = CellState.Empty;
            Ships.Clear();
        }

        public bool CanPlaceShip(Ship ship, Point startPos)
        {
            int dx = ship.IsHorizontal ? 1 : 0;
            int dy = ship.IsHorizontal ? 0 : 1;

            for (int i = 0; i < ship.Size; i++)
            {
                int x = (int)startPos.X + i * dx;
                int y = (int)startPos.Y + i * dy;

                if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
                    return false;

                for (int nx = Math.Max(0, x - 1); nx <= Math.Min(BoardSize - 1, x + 1); nx++)
                    for (int ny = Math.Max(0, y - 1); ny <= Math.Min(BoardSize - 1, y + 1); ny++)
                        if (cells[nx, ny] == CellState.Ship)
                            return false;
            }
            return true;
        }

        public bool PlaceShip(Ship ship, Point startPos)
        {
            if (!CanPlaceShip(ship, startPos))
                return false;

            ship.ClearCells();
            int dx = ship.IsHorizontal ? 1 : 0;
            int dy = ship.IsHorizontal ? 0 : 1;

            for (int i = 0; i < ship.Size; i++)
            {
                int x = (int)startPos.X + i * dx;
                int y = (int)startPos.Y + i * dy;

                cells[x, y] = CellState.Ship;
                ship.AddCell(new Point(x, y));
            }

            Ships.Add(ship);
            return true;
        }

        public CellState MakeShot(int x, int y)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
                return CellState.Miss;

            if (cells[x, y] == CellState.Ship)
            {
                cells[x, y] = CellState.Hit;
                var ship = Ships.FirstOrDefault(s => s.Cells.Contains(new Point(x, y)));
                ship?.Hit();

                if (ship?.IsDestroyed ?? false)
                {
                    foreach (var cell in ship.Cells)
                        cells[(int)cell.X, (int)cell.Y] = CellState.Destroyed;
                    return CellState.Destroyed;
                }
                return CellState.Hit;
            }
            else if (cells[x, y] == CellState.Empty)
            {
                cells[x, y] = CellState.Miss;
                return CellState.Miss;
            }

            return cells[x, y];
        }

        public bool AllShipsDestroyed()
        {
            return Ships.All(ship => ship.IsDestroyed);
        }
    }
}