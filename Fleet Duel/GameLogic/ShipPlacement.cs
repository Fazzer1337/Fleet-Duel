using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public static class ShipPlacement
    {
        private static readonly int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        public static bool AutoPlaceShips(GameBoard board)
        {
            board.ClearBoard();
            Random random = new Random();

            foreach (int size in shipSizes)
            {
                Ship ship = new Ship(size);
                bool placed = false;

                for (int attempt = 0; attempt < 100; attempt++)
                {
                    int x = random.Next(GameBoard.BoardSize);
                    int y = random.Next(GameBoard.BoardSize);
                    ship.IsHorizontal = random.Next(2) == 0;

                    if (board.PlaceShip(ship, new Point(x, y)))
                    {
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    board.ClearBoard();
                    return false;
                }
            }
            return true;
        }

        public static List<Ship> CreateStandardFleet()
        {
            List<Ship> fleet = new List<Ship>();
            foreach (int size in shipSizes)
            {
                fleet.Add(new Ship(size));
            }
            return fleet;
        }
    }
}