using System.Windows.Media;

namespace Fleet_Duel.GameLogic
{
    public class GameSettings
    {
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public bool DarkTheme { get; set; } = false;
        public bool ShowShipPlacementHints { get; set; } = true;
        public bool AutoCompleteDestroyedShips { get; set; } = true;

        // Цвета для тем
        public static Color LightCellColor = Colors.AliceBlue;
        public static Color DarkCellColor = Color.FromRgb(40, 44, 52);
        public static Color LightShipColor = Colors.Gray;
        public static Color DarkShipColor = Color.FromRgb(100, 100, 100);
        public static Color HitColor = Colors.OrangeRed;
        public static Color MissColor = Colors.LightBlue;
        public static Color DestroyedColor = Colors.DarkRed;
    }
}
