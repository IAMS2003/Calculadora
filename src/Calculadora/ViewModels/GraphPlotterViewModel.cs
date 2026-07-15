using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Calculadora.Commands;

namespace Calculadora.ViewModels
{
    public class GraphPlotterViewModel : BaseViewModel
    {
        public ObservableCollection<string> Expressions { get; } = new ObservableCollection<string>();

        private string _newExpression = "";
        public string NewExpression
        {
            get => _newExpression;
            set
            {
                _newExpression = value;
                OnPropertyChanged(nameof(NewExpression));
            }
        }

        public ICommand AddExpressionCommand { get; }
        public ICommand RemoveExpressionCommand { get; }

        public GraphPlotterViewModel()
        {
            // Initial example function
            Expressions.Add("sin(x)");
            
            AddExpressionCommand = new RelayCommand<object>(ExecuteAddExpression, CanExecuteAddExpression);
            RemoveExpressionCommand = new RelayCommand<string>(ExecuteRemoveExpression);
        }

        private bool CanExecuteAddExpression(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NewExpression) && Expressions.Count < 5;
        }

        private void ExecuteAddExpression(object? parameter)
        {
            Expressions.Add(NewExpression);
            NewExpression = "";
            OnPropertyChanged(nameof(Expressions)); // Force update for WPF binding
        }

        private void ExecuteRemoveExpression(string? expr)
        {
            if (expr != null && Expressions.Contains(expr))
            {
                Expressions.Remove(expr);
                OnPropertyChanged(nameof(Expressions)); // Force update for WPF binding
            }
        }
    }
}
