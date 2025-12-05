using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class EasyAIPlayer : IAIPlayer
    {
        private Random random;
        private GameBoard targetBoard;
        private List<Point> availableShots;

        public EasyAIPlayer(GameBoard targetBoard)
        {
            random = new Random();
            this.targetBoard = targetBoard;
            InitializeAvailableShots();
        }

        private void InitializeAvailableShots()
        {
            availableShots = new List<Point>();
            for (int x = 0; x < GameBoard.BoardSize; x++)
                for (int y = 0; y < GameBoard.BoardSize; y++)
                    availableShots.Add(new Point(x, y));
        }

        public Point MakeMove()
        {
            if (availableShots.Count == 0)
                return new Point(-1, -1);

            // Просто случайный выстрел
            int index = random.Next(availableShots.Count);
            Point shot = availableShots[index];
            availableShots.RemoveAt(index);
            return shot;
        }

        public void UpdateAfterShot(Point shot, CellState result)
        {
            // Easy AI не учится на выстрелах
        }

        public void Reset()
        {
            InitializeAvailableShots();
        }
    }
}
