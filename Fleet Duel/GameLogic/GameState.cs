using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fleet_Duel.GameLogic
{
    public enum GameState
    {
        PlacingShips,
        PlayerTurn,
        AITurn,
        Player1Turn,
        Player2Turn,
        PlayerWon,
        AIWon,
        GameOver
    }

    public enum CellState
    {
        Empty,
        Ship,
        Hit,
        Miss,
        Destroyed
    }
}
