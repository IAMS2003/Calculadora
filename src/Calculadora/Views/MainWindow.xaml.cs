using System.Windows;
using Calculadora.ViewModels;

namespace Calculadora.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new NavigationViewModel();
        }
    }
}
