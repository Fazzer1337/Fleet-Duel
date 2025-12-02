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
        private GameBoard playerBoard;
        private List<Point> availableShots;

        public AIPlayer(GameBoard targetBoard)
        {
            random = new Random();
            playerBoard = targetBoard;
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

            int index = random.Next(availableShots.Count);
            Point shot = availableShots[index];
            availableShots.RemoveAt(index);
            return shot;
        }

        public void Reset()
        {
            InitializeAvailableShots();
        }
    }
}