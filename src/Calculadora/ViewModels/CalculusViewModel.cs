using System;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;
using Calculadora.Services;

namespace Calculadora.ViewModels
{
    public class CalculusViewModel : BaseViewModel
    {
        private readonly SymbolicAdapter _adapter = new SymbolicAdapter();

        private string _inputExpression = "";
        private string _variable = "x";
        private string _resultExpression = "";
        private string _errorMessage = "";
        private int _selectedTabIndex = 0;

        public string InputExpression
        {
            get => _inputExpression;
            set
            {
                _inputExpression = value;
                OnPropertyChanged(nameof(InputExpression));
                ErrorMessage = "";
            }
        }

        public string Variable
        {
            get => _variable;
            set
            {
                _variable = value;
                OnPropertyChanged(nameof(Variable));
            }
        }

        public string ResultExpression
        {
            get => _resultExpression;
            set
            {
                _resultExpression = value;
                OnPropertyChanged(nameof(ResultExpression));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));
            }
        }

        public ICommand DifferentiateCommand { get; }
        public ICommand SimplifyCommand { get; }
        public ICommand ExpandCommand { get; }
        public ICommand SendToGraphCommand { get; }
        public ICommand ClearCommand { get; }

        public CalculusViewModel()
        {
            DifferentiateCommand = new RelayCommand<object>(ExecuteDifferentiate);
            SimplifyCommand = new RelayCommand<object>(ExecuteSimplify);
            ExpandCommand = new RelayCommand<object>(ExecuteExpand);
            SendToGraphCommand = new RelayCommand<object>(ExecuteSendToGraph);
            ClearCommand = new RelayCommand<object>(ExecuteClear);
        }

        private void ExecuteDifferentiate(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;

            try
            {
                string result = _adapter.Differentiate(InputExpression, Variable);
                ResultExpression = result;
                ErrorMessage = "";
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                ResultExpression = "";
            }
        }

        private void ExecuteSimplify(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;

            try
            {
                string result = _adapter.Simplify(InputExpression);
                ResultExpression = result;
                ErrorMessage = "";
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                ResultExpression = "";
            }
        }

        private void ExecuteExpand(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;

            try
            {
                string result = _adapter.Expand(InputExpression);
                ResultExpression = result;
                ErrorMessage = "";
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                ResultExpression = "";
            }
        }

        private void ExecuteSendToGraph(object? parameter)
        {
            string toSend = !string.IsNullOrWhiteSpace(ResultExpression) ? ResultExpression : InputExpression;
            if (!string.IsNullOrWhiteSpace(toSend))
            {
                EventAggregator.Instance.SendToGraph(toSend);
            }
        }

        private void ExecuteClear(object? parameter)
        {
            InputExpression = "";
            ResultExpression = "";
            ErrorMessage = "";
        }
    }
}
