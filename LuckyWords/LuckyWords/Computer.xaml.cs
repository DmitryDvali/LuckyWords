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

        List<Word> potentialWordsList = new List<Word>();
        List<Word> tempWordsList = new List<Word>();
        List<string> computerWords = new List<string>();
        List<Word> computerWordsList = new List<Word>();

        Square[,] squaresArray = new Square[5, 5];

        Word moveWord = new Word();

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
                if (countFilledSquares == 10)
                {
                    ShowWinner();
                }

                ComputerMove();
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

            ComputerMove();
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

        void ComputerMove()
        {
            potentialWordsList.Clear();
            computerWords.Clear();
            computerWordsList.Clear();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (squaresArray[i, j].TextBox.Text != string.Empty)
                    {
                        Word potentialWordBeginning = new Word();
                        potentialWordBeginning.Squares.Add(squaresArray[i, j]);
                        WordBuilding(potentialWordBeginning);
                    }
                }
            }

            using (var context = new Context())
            {
                foreach (var dictWord in context.Dictionary)
                {
                    if (computerWords.Count() >= 1)
                        if (computerWords.Last() != initialWord && !words1ListBox.Items.Contains(computerWords.Last())
                && !words2ListBox.Items.Contains(computerWords.Last()))
                            break;
                    foreach (var potWord in potentialWordsList)
                    {
                        if (computerWords.Count() >= 1)
                            if (computerWords.Last() != initialWord && !words1ListBox.Items.Contains(computerWords.Last())
                    && !words2ListBox.Items.Contains(computerWords.Last()))
                                break;
                        bool allow = false;
                        string pWord = null;

                        foreach (var sq in potWord.Squares)
                        {
                            if (sq.TextBox.Text == string.Empty) pWord += "*";
                            pWord += sq.TextBox.Text;
                        }

                        if (pWord.Length == dictWord.Word.Length)
                        {
                            for (int i = 0; i < pWord.Length; i++)
                            {
                                if (pWord[i] != '*')
                                {
                                    if (pWord.ToLower()[i] != dictWord.Word[i]) { allow = false; break; }
                                    else { allow = true; }
                                }
                                else
                                {
                                    pWord = pWord.Replace('*', dictWord.Word.ToUpper()[i]);
                                }
                            }
                        }

                        if (allow)
                        {
                            computerWords.Add(pWord);
                            computerWordsList.Add(potWord);
                        }
                    }    
                }
            }
            
            var computerMoveWord = computerWordsList.Last();
            var computerMoveWordString = computerWords.Last();

            for (int i = 0; i < computerMoveWordString.Length; i++)
            {
                computerMoveWord.Squares[i].TextBox.Text = computerMoveWordString[i].ToString();
                computerMoveWord.Squares[i].TextBox.IsReadOnly = true;
            }
                
            words2ListBox.Items.Add(computerMoveWordString);
            points2.Content = (computerPoints += computerMoveWordString.Count()).ToString();

            foreach (var sq in Squares)
                sq.TextBox.IsEnabled = true;
        }

        void WordBuilding(Word word)
        {
            if (potentialWordsList.Count() > 5000) return;
            if (word.Squares.Last().X - 1 >= 0)
            {
                if (squaresArray[word.Squares.Last().X - 1, word.Squares.Last().Y].TextBox.Text == string.Empty)
                {
                    var a = new Word();
                    a.Squares.AddRange(word.Squares);
                    a.Squares.Add(squaresArray[word.Squares.Last().X - 1, word.Squares.Last().Y]);
                    potentialWordsList.Add(a);

                    var b = new Word();
                    var wordSquaresNaoborot = word.Squares.Reverse<Square>();
                    b.Squares.Add(squaresArray[word.Squares.Last().X - 1, word.Squares.Last().Y]);
                    b.Squares.AddRange(wordSquaresNaoborot);
                    potentialWordsList.Add(b);
                }
                else
                {
                    if (!word.Squares.Contains(squaresArray[word.Squares.Last().X - 1, word.Squares.Last().Y]))
                    {
                        var tempWord = new Word();
                        tempWord.Squares.AddRange(word.Squares);
                        tempWord.Squares.Add(squaresArray[word.Squares.Last().X - 1, word.Squares.Last().Y]);
                        if (!tempWordsList.Contains(tempWord))
                        {
                            tempWordsList.Add(tempWord);
                            WordBuilding(tempWord);
                        }
                    }
                }
            }

            if (word.Squares.Last().Y + 1 <= 4)
            {
                if (squaresArray[word.Squares.Last().X, word.Squares.Last().Y + 1].TextBox.Text == string.Empty)
                {
                    var a = new Word();
                    a.Squares.AddRange(word.Squares);
                    a.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y + 1]);
                    potentialWordsList.Add(a);

                    var b = new Word();
                    var wordSquaresNaoborot = word.Squares.Reverse<Square>();
                    b.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y + 1]);
                    b.Squares.AddRange(wordSquaresNaoborot);
                    potentialWordsList.Add(b);
                }
                else
                {
                    if (!word.Squares.Contains(squaresArray[word.Squares.Last().X, word.Squares.Last().Y + 1]))
                    {
                        var tempWord = new Word();
                        tempWord.Squares.AddRange(word.Squares);
                        tempWord.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y + 1]);
                        if (!tempWordsList.Contains(tempWord))
                        {
                            tempWordsList.Add(tempWord);
                            WordBuilding(tempWord);
                        }
                    }
                }
            }

            if (word.Squares.Last().X + 1 <= 4)
            {
                if (squaresArray[word.Squares.Last().X + 1, word.Squares.Last().Y].TextBox.Text == string.Empty)
                {
                    var a = new Word();
                    a.Squares.AddRange(word.Squares);
                    a.Squares.Add(squaresArray[word.Squares.Last().X + 1, word.Squares.Last().Y]);
                    potentialWordsList.Add(a);

                    var b = new Word();
                    var wordSquaresNaoborot = word.Squares.Reverse<Square>();
                    b.Squares.Add(squaresArray[word.Squares.Last().X + 1, word.Squares.Last().Y]);
                    b.Squares.AddRange(wordSquaresNaoborot);
                    potentialWordsList.Add(b);
                }
                else
                {
                    if (!word.Squares.Contains(squaresArray[word.Squares.Last().X + 1, word.Squares.Last().Y]))
                    {
                        var tempWord = new Word();
                        tempWord.Squares.AddRange(word.Squares);
                        tempWord.Squares.Add(squaresArray[word.Squares.Last().X + 1, word.Squares.Last().Y]);
                        if (!tempWordsList.Contains(tempWord))
                        {
                            tempWordsList.Add(tempWord);
                            WordBuilding(tempWord);
                        }
                    }
                }
            }

            if (word.Squares.Last().Y - 1 >= 0)
            {
                if (squaresArray[word.Squares.Last().X, word.Squares.Last().Y - 1].TextBox.Text == string.Empty)
                {
                    var a = new Word();
                    a.Squares.AddRange(word.Squares);
                    a.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y - 1]);
                    potentialWordsList.Add(a);

                    var b = new Word();
                    var wordSquaresNaoborot = word.Squares.Reverse<Square>();
                    b.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y - 1]);
                    b.Squares.AddRange(wordSquaresNaoborot);
                    potentialWordsList.Add(b);
                }
                else
                {
                    if (!word.Squares.Contains(squaresArray[word.Squares.Last().X, word.Squares.Last().Y - 1]))
                    {
                        var tempWord = new Word();
                        tempWord.Squares.AddRange(word.Squares);
                        tempWord.Squares.Add(squaresArray[word.Squares.Last().X, word.Squares.Last().Y - 1]);
                        if (!tempWordsList.Contains(tempWord))
                        {
                            tempWordsList.Add(tempWord);
                            WordBuilding(tempWord);
                        }
                    }
                }
            }
        }
    }
}
