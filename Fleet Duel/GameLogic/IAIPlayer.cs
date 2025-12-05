using System.Windows;

namespace Fleet_Duel.GameLogic
{
    public interface IAIPlayer
    {
        Point MakeMove();
        void UpdateAfterShot(Point shot, CellState result);
        void Reset();
    }
}
