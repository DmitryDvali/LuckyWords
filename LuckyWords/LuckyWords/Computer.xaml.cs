using LuckyWords.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LuckyWords
{
    /// <summary>
    /// Логика взаимодействия для Computer.xaml
    /// </summary>
    public partial class Computer : Window
    {
        string initialWord;

        List<Square> Squares = new List<Square>();

        Square[,] squaresArray = new Square[5, 5];

        Word moveWord = new Word() { Squares = new List<Square>() };

        int playerPoints = 0, computerPoints = 0, countFilledSquares = 0;

        bool ableToSelect = false, oneSquareIsSelected = false;
        public Computer()
        {
            InitializeComponent();

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    string textBoxName = "textBox" + i.ToString() + j.ToString();
                    TextBox textBox = (TextBox)FindName(textBoxName);
                    Squares.Add(new Square() { X = i, Y = j, TextBox = textBox });
                    squaresArray[i, j] = new Square() { X = i, Y = j, TextBox = textBox };
                }

            using (Context context = new Context())
            {
                List<string> initialWordsList = (from unit in context.InitialWordsList
                                                 select unit.Word).ToList();
                initialWord = initialWordsList[new Random().Next(initialWordsList.Count() - 1)];
            }

            for (int i = 10; i < 15; i++)
            {
                Squares[i].TextBox.Text = initialWord.Substring(0, 1).ToUpper();
                initialWord = initialWord.Remove(0, 1);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text != string.Empty)
            {
                foreach (Square square in Squares)
                    if (square.TextBox.Text == string.Empty)
                        square.TextBox.IsEnabled = false;
            }
            else
            {
                foreach (Square square in Squares)
                    square.TextBox.IsEnabled = true;
            }
        }

        private void PreviewMouseLeftButtonDownOnTextBox(object sender, MouseButtonEventArgs e)
        {
            Square senderSquare = Squares.First(square => square.TextBox == sender);

            if (!ableToSelect)
                foreach (Square square in Squares)
                    if (square.TextBox.Text != string.Empty && square.TextBox.IsReadOnly == false)
                    {
                        ableToSelect = true;
                        break;
                    }

            if (!oneSquareIsSelected)
                foreach (Square square in Squares)
                    if (square.TextBox.Background == Brushes.Aquamarine)
                    {
                        oneSquareIsSelected = true;
                        break;
                    }

            if ((sender as TextBox).Background == Brushes.Aquamarine)
            {
                moveWord.Squares.Clear();
                ableToSelect = false;
                oneSquareIsSelected = false;
                foreach (Square square in Squares)
                {
                    square.TextBox.Background = Brushes.White;
                    square.TextBox.CaretBrush = Brushes.Black;
                }
                return;
            }

            if (!oneSquareIsSelected && ableToSelect)
            {
                moveWord.Squares.Add(Squares.Find(a => a.TextBox == sender));
                (sender as TextBox).CaretBrush = Brushes.Aquamarine;
                (sender as TextBox).Background = Brushes.Aquamarine;
                return;
            }

            if (oneSquareIsSelected && ableToSelect)
            {
                double distance = Math.Sqrt(Math.Abs(Math.Pow(senderSquare.X - moveWord.Squares.Last().X, 2) + Math.Pow(senderSquare.Y - moveWord.Squares.Last().Y, 2)));
                if (distance == 1)
                {
                    moveWord.Squares.Add(senderSquare);
                    (sender as TextBox).CaretBrush = Brushes.Aquamarine;
                    (sender as TextBox).Background = Brushes.Aquamarine;
                    return;
                }
            }
        }

        private void endMoveButton_Click(object sender, RoutedEventArgs e)
        {
            ableToSelect = false;
            oneSquareIsSelected = false;

            string finishWord = null;
            bool isCorrectWord;

            foreach (Square square in Squares)
            {
                if (square.TextBox.IsEnabled == true && square.TextBox.IsReadOnly == false
                    && square.TextBox.Background != Brushes.Aquamarine)
                {
                    moveWord.Squares.Clear();
                    foreach (Square sq in Squares)
                    {
                        sq.TextBox.Background = Brushes.White;
                        sq.TextBox.CaretBrush = Brushes.Black;
                    }
                    MessageBox.Show("Необходимо использовать добавленную букву.");
                    return;
                }
            }

            foreach (Square square in moveWord.Squares)
                finishWord += square.TextBox.Text;

            using (Context context = new Context())
            {
                var dictItem = context.Dictionary.FirstOrDefault(item => item.Word == finishWord);
                if (dictItem != null)
                    isCorrectWord = true;
                else isCorrectWord = false;
            }

            if (isCorrectWord && !words1ListBox.Items.Contains(finishWord) && !words2ListBox.Items.Contains(finishWord))
            {
                points1.Content = (playerPoints += finishWord.Count()).ToString();
                words1ListBox.Items.Add(finishWord);
                player1Label.FontWeight = FontWeights.Normal;
                player2Label.FontWeight = FontWeights.Bold;

                moveWord.Squares.Clear();
                foreach (Square square in Squares)
                {
                    if (square.TextBox.Background == Brushes.Aquamarine)
                    {
                        square.TextBox.Background = Brushes.White;
                        square.TextBox.CaretBrush = Brushes.Black;
                        square.TextBox.IsReadOnly = true;
                    }
                    square.TextBox.IsEnabled = true;
                }

                countFilledSquares++;
                if (countFilledSquares == 20)
                {
                    ShowWinner();
                }
                return;
            }

            if (words1ListBox.Items.Contains(finishWord) || words2ListBox.Items.Contains(finishWord))
                MessageBox.Show("Это слово уже было использовано.");
            else
                MessageBox.Show("Такого слова нет.");

            moveWord.Squares.Clear();
            RemoveSelectionAndClearSquares();
        }

        private void missMoveButton_Click(object sender, RoutedEventArgs e)
        {
            ableToSelect = false;
            oneSquareIsSelected = false;

            moveWord.Squares.Clear();
            RemoveSelectionAndClearSquares();
        }

        void ShowWinner()
        {
            MessageBoxResult result;
            if (playerPoints > computerPoints)
                result = MessageBox.Show("Вы победили! Начать новую игру?", "Игра окончена.", MessageBoxButton.YesNo);
            else if (playerPoints < computerPoints)
                result = MessageBox.Show("Вы проиграли! Начать новую игру?", "Игра окончена.", MessageBoxButton.YesNo);
            else
                result = MessageBox.Show("Ничья! Начать новую игру?", "Игра окончена.", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                (new MainWindow()).Show();
                Close();
            }
        }

        void RemoveSelectionAndClearSquares()
        {
            foreach (Square square in Squares)
            {
                square.TextBox.Background = Brushes.White;
                square.TextBox.CaretBrush = Brushes.Black;
                if (square.TextBox.IsReadOnly == false)
                    square.TextBox.Text = string.Empty;
            }
        }
    }
}
