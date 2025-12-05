using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Fleet_Duel.GameLogic;

namespace Fleet_Duel.Views
{
    public partial class CellControl : UserControl
    {
        public static readonly DependencyProperty CellStateProperty =
            DependencyProperty.Register("CellState", typeof(CellState), typeof(CellControl),
                new PropertyMetadata(CellState.Empty, OnCellStateChanged));

        public CellState CellState
        {
            get => (CellState)GetValue(CellStateProperty);
            set => SetValue(CellStateProperty, value);
        }

        public static readonly DependencyProperty PreviewStateProperty =
            DependencyProperty.Register("PreviewState", typeof(CellState), typeof(CellControl),
                new PropertyMetadata(CellState.Empty, OnCellStateChanged));

        public CellState PreviewState
        {
            get => (CellState)GetValue(PreviewStateProperty);
            set => SetValue(PreviewStateProperty, value);
        }

        public static readonly DependencyProperty PreviewColorProperty =
            DependencyProperty.Register("PreviewColor", typeof(Color), typeof(CellControl),
                new PropertyMetadata(Colors.Transparent, OnPreviewColorChanged));

        public Color PreviewColor
        {
            get => (Color)GetValue(PreviewColorProperty);
            set => SetValue(PreviewColorProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CellControl),
                new PropertyMetadata(new Point(0, 0)));

        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty IsDarkThemeProperty =
            DependencyProperty.Register("IsDarkTheme", typeof(bool), typeof(CellControl),
                new PropertyMetadata(false, OnIsDarkThemeChanged));

        public bool IsDarkTheme
        {
            get => (bool)GetValue(IsDarkThemeProperty);
            set => SetValue(IsDarkThemeProperty, value);
        }

        public CellControl()
        {
            InitializeComponent();
        }

        private static void OnCellStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CellControl)d;
            control.UpdateCellAppearance();
        }

        private static void OnPreviewColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CellControl)d;
            control.UpdateCellAppearance();
        }

        private static void OnIsDarkThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CellControl)d;
            control.UpdateCellAppearance();
        }

        private void UpdateCellAppearance()
        {
            if (cellBorder == null) return;

            // Используем PreviewState для предпросмотра, иначе CellState
            CellState stateToShow = PreviewState != CellState.Empty ? PreviewState : CellState;

            // Используем цвет предпросмотра если задан
            if (PreviewColor != Colors.Transparent && PreviewState != CellState.Empty)
            {
                cellBorder.Background = new SolidColorBrush(PreviewColor);
                cellText.Text = "";
                return;
            }

            switch (stateToShow)
            {
                case CellState.Empty:
                    cellBorder.Background = new SolidColorBrush(
                        IsDarkTheme ? GameSettings.DarkCellColor : GameSettings.LightCellColor);
                    cellText.Text = "";
                    break;
                case CellState.Ship:
                    cellBorder.Background = new SolidColorBrush(
                        IsDarkTheme ? GameSettings.DarkShipColor : GameSettings.LightShipColor);
                    cellText.Text = "";
                    break;
                case CellState.Hit:
                    cellBorder.Background = new SolidColorBrush(GameSettings.HitColor);
                    cellText.Text = "✕";
                    cellText.Foreground = Brushes.White;
                    break;
                case CellState.Miss:
                    cellBorder.Background = new SolidColorBrush(GameSettings.MissColor);
                    cellText.Text = "•";
                    cellText.Foreground = Brushes.Black;
                    break;
                case CellState.Destroyed:
                    cellBorder.Background = new SolidColorBrush(GameSettings.DestroyedColor);
                    cellText.Text = "✕";
                    cellText.Foreground = Brushes.White;
                    break;
            }

            // Сбрасываем PreviewState после отображения
            if (PreviewState != CellState.Empty)
            {
                PreviewState = CellState.Empty;
                PreviewColor = Colors.Transparent;
            }
        }
    }
}
