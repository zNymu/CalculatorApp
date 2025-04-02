using System.Windows;
using CalculatorApp.ViewModels;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new CalculatorViewModel();
            KeyDown += (s, e) => ((CalculatorViewModel)DataContext).KeyboardCommand.Execute(e.Key);
        }
    }
}