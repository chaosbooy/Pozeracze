using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PozeraczePart1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        readonly Regex regex = new Regex("[^0-9]+");
        private RadialGradientBrush pieceHeld;
        private int pieceHeldId;
        private bool firstTurn;
        private int wallSize;
        private int pieceCategoryCount = 3;

        private string _color1;
        private string _color2;

        private int[,] board;

        public string Color1
        {
            get => _color1;
            set
            {
                _color1 = value;
                OnPropertyChanged(nameof(Color1));
            }
        }

        public string Color2
        {
            get => _color2;
            set
            {
                _color2 = value;
                OnPropertyChanged(nameof(Color2));
            }
        }

        public event PropertyChangedEventHandler ?PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext to the MainWindow instance.
            Color1 = "#B2BCF0";
            Color2 = "#FFCCE1";

            board = new int[0, 0];
            firstTurn = true;
            pieceHeld = new RadialGradientBrush();
        }

        private void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }

            TextBox s = (TextBox)sender;
            wallSize = Convert.ToInt16(s.Text);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(wallInput.Text, out wallSize) || wallSize < 3)
                wallSize = 3;

            //Creating center game board
            board = new int[wallSize, wallSize];
            gameBoard.ColumnDefinitions.Clear();
            gameBoard.RowDefinitions.Clear();
            gameBoard.Children.Clear();
            

            for(int i = 0; i < wallSize; i++)
            {
                gameBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                gameBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for(int i = 0; i < wallSize; i++)
            {
                for(int j = 0; j < wallSize; j++)
                {
                    Label tmp = new Label
                    {
                        Content = $"{i * wallSize + j}"
                    };

                    tmp.MouseEnter += GameBoard_MouseEnter;
                    tmp.MouseLeave += GameBoard_MouseLeave;

                    tmp.MouseLeftButtonDown += GameFieldClicked;

                    Grid.SetColumn(tmp, j);
                    Grid.SetRow(tmp, i);

                    gameBoard.Children.Add(tmp);
                }
            }

            //Creating Player pieces
            player1.Children.Clear();
            player2.Children.Clear();

            RadialGradientBrush player1Gradient = new RadialGradientBrush();
            RadialGradientBrush player2Gradient = new RadialGradientBrush();

            player1Gradient.Center = new Point(0.5, 0.5);
            player1Gradient.GradientOrigin = new Point(0.5, 0.5);

            player2Gradient.Center = new Point(0.5, 0.5);
            player1Gradient.GradientOrigin = new Point(0.5, 0.5);

            for (int i = 1; i <= pieceCategoryCount; i++)
            {
                player1Gradient.GradientStops.Clear();
                player2Gradient.GradientStops.Clear();

                double gradientPlacement = i * 0.33;
                gradientPlacement -= gradientPlacement > 0.95 ? 0.05 : 0;

                player1Gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(Color1.Normalize()), gradientPlacement));
                player1Gradient.GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                player2Gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(Color2.Normalize()), gradientPlacement));
                player2Gradient.GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                if (i == 1) 
                    pieceHeld = player1Gradient.Clone();

                Label tmp1 = new Label
                {
                    Background = player1Gradient.Clone(),
                    Content = $"{i}"
                };

                Label tmp2 = new Label
                {
                    Background = player2Gradient.Clone(),
                    Content = $"{-i}"
                };

                tmp1.MouseLeftButtonDown += PieceSelected;
                tmp2.MouseLeftButtonDown += PieceSelected;

                player1.Children.Add(tmp1);
                player2.Children.Add(tmp2);

            }

            lobby.Visibility = Visibility.Collapsed;
            game.Visibility = Visibility.Visible;
            firstTurn = true;
            pieceHeldId = 1;
        }

        private void EndGame(object sender, RoutedEventArgs e)
        {
            lobby.Visibility = Visibility.Visible;
            game.Visibility = Visibility.Collapsed;
        }

        private void GameFieldClicked(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            firstTurn = !firstTurn;

            pieceHeldId = firstTurn ? 1 : -1;
            Label tmp = firstTurn ? (Label)player1.Children[0] : (Label)player2.Children[0];
            pieceHeld = (RadialGradientBrush)tmp.Background;
        }

        private void PieceSelected(object sender, MouseEventArgs e)
        {
            Label s = (Label)sender;

            if (s.Content.ToString()?[0] == '-' && !firstTurn)
            {
                int.TryParse(s.Content.ToString(), out pieceHeldId);
                pieceHeld = (RadialGradientBrush)s.Background;
            }

            else if (s.Content.ToString()?[0] != '-' && firstTurn)
            {
                int.TryParse(s.Content.ToString(), out pieceHeldId);
                pieceHeld = (RadialGradientBrush)s.Background;
            }
        }

        private void GameBoard_MouseEnter(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            RadialGradientBrush tmp = pieceHeld;
            pieceHeld = (RadialGradientBrush)s.Background;
            s.Background = tmp;
        }

        private void GameBoard_MouseLeave(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            RadialGradientBrush tmp = pieceHeld;
            pieceHeld = (RadialGradientBrush)s.Background;
            s.Background = tmp.Clone();
        }
    }
}