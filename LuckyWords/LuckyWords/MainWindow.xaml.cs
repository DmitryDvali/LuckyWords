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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LuckyWords
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void playWithComputerButton_Click(object sender, RoutedEventArgs e)
        {
            Computer computerWindow = new Computer();
            this.Close();
            computerWindow.Show();
        }

        private void playWithPersonButton_Click(object sender, RoutedEventArgs e)
        {
            Person personWindow = new Person();
            this.Close();
            personWindow.Show();
        }
    }
}
