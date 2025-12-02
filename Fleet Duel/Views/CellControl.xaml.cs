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

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CellControl),
                new PropertyMetadata(new Point(0, 0)));

        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public CellControl()
        {
            InitializeComponent();
        }

        private static void OnCellStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CellControl)d;
            control.UpdateCellText();
        }

        private void UpdateCellText()
        {
            if (cellText == null) return;

            switch (CellState)
            {
                case CellState.Hit:
                    cellText.Text = "✕";
                    cellText.Foreground = Brushes.White;
                    break;
                case CellState.Miss:
                    cellText.Text = "•";
                    cellText.Foreground = Brushes.Black;
                    break;
                case CellState.Destroyed:
                    cellText.Text = "✕";
                    cellText.Foreground = Brushes.White;
                    break;
                default:
                    cellText.Text = "";
                    break;
            }
        }
    }
}