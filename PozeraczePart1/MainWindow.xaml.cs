using System;
using System.ComponentModel;
using System.Reflection;
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
        private int pieceHeldId;
        private bool firstTurn;
        private int wallSize;
        private int pieceCategoryCount = 3;

        private string _color1;
        private string _color2;

        private RadialGradientBrush[] player1Backgrounds;
        private RadialGradientBrush[] player2Backgrounds;
        private RadialGradientBrush defaultBackground;

        GameBoard backendGame;

        public string Color1
        {
            get => _color1;
            set
            {
                _color1 = value;

                if (player2Backgrounds[0] != null)
                    for (int i = 0; i < pieceCategoryCount; i++)
                    {
                        player1Backgrounds[i].GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(Color1);
                    }

                OnPropertyChanged(nameof(Color1));
            }
        }

        public string Color2
        {
            get => _color2;
            set
            {
                _color2 = value;

                if(player2Backgrounds[0] != null)
                    for (int i = 0; i < pieceCategoryCount; i++)
                    {
                        player2Backgrounds[i].GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(Color2);
                    }

                OnPropertyChanged(nameof(Color2));
            }
        }

        public event PropertyChangedEventHandler ?PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext to the MainWindow instance.
            backendGame = new GameBoard();

            player1Backgrounds = new RadialGradientBrush[pieceCategoryCount];
            player2Backgrounds = new RadialGradientBrush[pieceCategoryCount];
            defaultBackground = new RadialGradientBrush();

            var center = new Point(0.5, 0.5);

            _color1 = "#B2BCF0";
            _color2 = "#FFCCE1";
            Color1 = "#B2BCF0";
            Color2 = "#FFCCE1";

            defaultBackground.Center = center;
            defaultBackground.GradientOrigin = center;

            defaultBackground.GradientStops.Add(new GradientStop(Colors.Gray, 0.4));
            defaultBackground.GradientStops.Add(new GradientStop(Colors.Transparent, 0.5));

            for (int i = 0; i < pieceCategoryCount; i++)
            {
                player1Backgrounds[i] = new RadialGradientBrush();
                player2Backgrounds[i] = new RadialGradientBrush();

                player1Backgrounds[i].Center = center;
                player1Backgrounds[i].GradientOrigin = center;

                player2Backgrounds[i].Center = center;
                player2Backgrounds[i].GradientOrigin = center;
            }

            for (int i = 1; i <= pieceCategoryCount; i++)
            {
                double gradientPlacement = i * 0.33;
                gradientPlacement -= gradientPlacement > 0.95 ? 0.05 : 0;

                player1Backgrounds[i - 1].GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(_color1), gradientPlacement));
                player1Backgrounds[i - 1].GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                player2Backgrounds[i - 1].GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(_color2.Normalize()), gradientPlacement));
                player2Backgrounds[i - 1].GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                Label tmp1 = new Label
                {
                    Background = player1Backgrounds[i - 1],
                    Content = $"{i}"
                };

                Label tmp2 = new Label
                {
                    Background = player2Backgrounds[i - 1],
                    Content = $"-{i}"
                };

                tmp1.MouseLeftButtonDown += PieceSelected;
                tmp2.MouseLeftButtonDown += PieceSelected;

                player1.Children.Add(tmp1);
                player2.Children.Add(tmp2);
            }

            firstTurn = true;
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = (e.NewSize.Width * 0.6 < e.NewSize.Height * 0.8) ? (e.NewSize.Width * 0.6) : (e.NewSize.Height * 0.8);
            gameBoard.Height = newSize;
            gameBoard.Width = newSize;
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
            backendGame = new GameBoard(wallSize);

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
                        Background = defaultBackground
                    };

                    tmp.MouseEnter += GameBoard_MouseEnter;
                    tmp.MouseLeave += GameBoard_MouseLeave;

                    tmp.MouseLeftButtonDown += GameFieldClicked;

                    Grid.SetColumn(tmp, j);
                    Grid.SetRow(tmp, i);

                    gameBoard.Children.Add(tmp);
                }
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

            int index = gameBoard.Children.IndexOf(s);
            if (!backendGame.PlacePiece(index / wallSize, index % wallSize, pieceHeldId)) return;

            firstTurn = !firstTurn;
            pieceHeldId = firstTurn ? 1 : -1;
        }

        private void PieceSelected(object sender, MouseEventArgs e)
        {
            Label s = (Label)sender;

            if (s.Content.ToString()?[0] == '-' && !firstTurn)
                int.TryParse(s.Content.ToString(), out pieceHeldId);

            else if (s.Content.ToString()?[0] != '-' && firstTurn)
                int.TryParse(s.Content.ToString(), out pieceHeldId);
        }

        private void GameBoard_MouseEnter(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            int index = gameBoard.Children.IndexOf(s);
            int number = backendGame.Board[index / wallSize, index % wallSize];
            if (number * pieceHeldId > 0 || (Math.Abs(number) >= Math.Abs(pieceHeldId) && number * pieceHeldId < 0)) return;

            s.Background = NumberToBackground(pieceHeldId - 1);
        }

        private void GameBoard_MouseLeave(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            int index = gameBoard.Children.IndexOf(s);
            int number = backendGame.Board[index / wallSize, index % wallSize];
            if (number * pieceHeldId < 0 || Math.Abs(number) > Math.Abs(pieceHeldId)) return;

            s.Background = NumberToBackground(number);
        }

        private RadialGradientBrush NumberToBackground(int num)
        {
            if(pieceHeldId < 0) return player2Backgrounds[Math.Abs(num)];
            else if (pieceHeldId > 0) return player1Backgrounds[num];

            return defaultBackground;
        }
    }
}