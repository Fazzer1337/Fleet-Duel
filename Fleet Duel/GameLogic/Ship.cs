using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public class Ship
    {
        public int Size { get; }
        public bool IsHorizontal { get; set; }
        public List<Point> Cells { get; }
        public int Hits { get; private set; }
        public bool IsDestroyed => Hits >= Size;

        public Ship(int size)
        {
            Size = size;
            Cells = new List<Point>();
            IsHorizontal = true;
        }

        public void AddCell(Point point) => Cells.Add(point);
        public void ClearCells() => Cells.Clear();

        public void Hit()
        {
            if (Hits < Size)
                Hits++;
        }
    }
}
