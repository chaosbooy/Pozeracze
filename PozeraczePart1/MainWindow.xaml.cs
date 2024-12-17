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
        private bool isFirstTurn;
        private int wallSize;
        const int pieceCategoryCount = 3;

        private Player player1, player2;
        private RadialGradientBrush defaultBackground;

        GameBoard backendGame;

        public event PropertyChangedEventHandler ?PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext to the MainWindow instance.
            backendGame = new GameBoard();
            defaultBackground = new RadialGradientBrush();

            var center = new Point(0.5, 0.5);

            player1 = new Player();
            player2 = new Player();

            firstColor.Color = (Color)ColorConverter.ConvertFromString("#B2BCF0");
            secondColor.Color = (Color)ColorConverter.ConvertFromString("#FFCCE1");

            defaultBackground.Center = center;
            defaultBackground.GradientOrigin = center;

            defaultBackground.GradientStops.Add(new GradientStop(Colors.Gray, 0.4));
            defaultBackground.GradientStops.Add(new GradientStop(Colors.Transparent, 0.5));

        }

        //input to number only
        private void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        //input to number only
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

        //changing game grid size when window size changed
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = (e.NewSize.Width * 0.6 < e.NewSize.Height - 100) ? (e.NewSize.Width * 0.6) : (e.NewSize.Height - 100);
            gameBoard.Height = newSize;
            gameBoard.Width = newSize;
        }

        //setting up everything for the game to start
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


            //Creating player panels
            player1Display.Children.Clear();
            player2Display.Children.Clear();

            player1 = new Player(wallSize - 1);
            player2 = new Player(wallSize - 1);

            player1.Name = firstName.Text.Trim() == "" ? "player 1" : firstName.Text;
            player2.Name = secondName.Text.Trim() == "" ? "player 2" : secondName.Text;

            player1.Color = firstColor.Color.ToString();
            player2.Color = secondColor.Color.ToString();

            var center = new Point(0.5, 0.5);

            for (int i = 1; i <= pieceCategoryCount; i++)
            {
                double gradientPlacement = i / (double)pieceCategoryCount;
                gradientPlacement -= gradientPlacement > 0.95 ? 0.05 : 0;

                RadialGradientBrush player1Backgrounds = new RadialGradientBrush();
                RadialGradientBrush player2Backgrounds = new RadialGradientBrush();

                player1Backgrounds.Center = center;
                player1Backgrounds.GradientOrigin = center;

                player2Backgrounds.Center = center;
                player2Backgrounds.GradientOrigin = center;

                player1Backgrounds.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(player1.Color), gradientPlacement));
                player1Backgrounds.GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                player2Backgrounds.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(player2.Color), gradientPlacement));
                player2Backgrounds.GradientStops.Add(new GradientStop(Colors.Transparent, gradientPlacement + 0.05));

                Label tmp1 = new Label
                {
                    Background = player1Backgrounds,
                    Content = $"{(player1.pieceNumbers[i - 1] > 0 ? player1.pieceNumbers[i - 1] : null)}"
                };

                Label tmp2 = new Label
                {
                    Background = player2Backgrounds,
                    Content = $"{(player2.pieceNumbers[i - 1] > 0 ? player2.pieceNumbers[i - 1] : null)}"
                };

                tmp1.MouseLeftButtonDown += PieceSelected;
                tmp2.MouseLeftButtonDown += PieceSelected;

                player1Display.Children.Add(tmp1);
                player2Display.Children.Add(tmp2);
            }

            lobby.Visibility = Visibility.Collapsed;
            game.Visibility = Visibility.Visible;
            settings.Visibility = Visibility.Collapsed;
            gameBoard.IsEnabled = true;
            isFirstTurn = true;
            Label lab = (Label)player1Display.Children[0];
            lab.Background.Opacity = 0.5;
            pieceHeldId = 1;
        }

        //Going back to lobby
        private void EndGame(object sender, RoutedEventArgs e)
        {
            lobby.Visibility = Visibility.Visible;
            game.Visibility = Visibility.Collapsed;
            settings.Visibility = Visibility.Collapsed;
        }

        //Game has end showing who won 
        private void GameLost()
        {
            settings.Visibility = Visibility.Visible;
            gameBoard.IsEnabled = false;
        }

        //Placing piece, Checking game condition
        private void GameFieldClicked(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            int index = gameBoard.Children.IndexOf(s);
            if (!backendGame.PlacePiece(index / wallSize, index % wallSize, pieceHeldId)) return;

            if (pieceHeldId > 1)
            {
                Label tmp = (Label)player1Display.Children[pieceHeldId - 1];
                tmp.Content = --player1.pieceNumbers[pieceHeldId - 1];
            }
            else if (pieceHeldId < -1)
            {
                Label tmp = (Label)player2Display.Children[Math.Abs(pieceHeldId) - 1];
                tmp.Content = --player2.pieceNumbers[Math.Abs(pieceHeldId) - 1];
            }

            isFirstTurn = !isFirstTurn;

            NumberToBackground(pieceHeldId).Opacity = 1.0;

            if(backendGame.CheckWin())
            {
                winner.Content = $"{(isFirstTurn ? player2.Name : player1.Name)} has won";
                GameLost();
            }
            else if (CheckDraw())
            {
                winner.Content = "Draw";
                GameLost();
            }

            pieceHeldId = isFirstTurn ? 1 : -1;
            NumberToBackground(pieceHeldId).Opacity = 0.5;
        }

        //When player has piece clicked
        private void PieceSelected(object sender, MouseEventArgs e)
        {
            Label s = (Label)sender;

            NumberToBackground(pieceHeldId).Opacity = 1.0;

            if (player2Display.Children.Contains(s) && !isFirstTurn && s.Content.ToString() != "0")
                pieceHeldId = -player2Display.Children.IndexOf(s) - 1;

            else if (player1Display.Children.Contains(s) && isFirstTurn && s.Content.ToString() != "0") 
                pieceHeldId = player1Display.Children.IndexOf(s) + 1;

            NumberToBackground(pieceHeldId).Opacity = 0.5;
        }

        //mouse enters game grid piece
        private void GameBoard_MouseEnter(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            int index = gameBoard.Children.IndexOf(s);
            int number = backendGame.Board[index / wallSize, index % wallSize];
            if (number * pieceHeldId > 0 || (Math.Abs(number) >= Math.Abs(pieceHeldId) && number * pieceHeldId < 0)) return;

            s.Background = NumberToBackground(pieceHeldId);
        }

        //mouse leaves game grid piece
        private void GameBoard_MouseLeave(object sender, RoutedEventArgs e)
        {
            Label s = (Label)sender;

            int index = gameBoard.Children.IndexOf(s);
            int number = backendGame.Board[index / wallSize, index % wallSize];

            s.Background = NumberToBackground(number).Clone();
        }

        //changing id from num to round background
        private RadialGradientBrush NumberToBackground(int num)
        {
            Label tmp = new Label { Background = defaultBackground };
            if (num < 0) tmp =  (Label)player2Display.Children[Math.Abs(num) - 1];
            else if (num > 0) tmp = (Label)player1Display.Children[num - 1];


            return (RadialGradientBrush) tmp.Background;
        }

        //checking draw condition because i forgot to put it in backendGame
        private bool CheckDraw()
        {
            for(int i = 1; i < pieceCategoryCount; i++)
                if (player1.pieceNumbers[i] > 0 || player2.pieceNumbers[i] > 0) return false;

            for(int i = 0; i < wallSize; i++)
            {
                for(int j = 0; j < wallSize; j++)
                {
                    if (backendGame.Board[i, j] == 0) return false;
                }
            }

            return true;
        }
    }
}