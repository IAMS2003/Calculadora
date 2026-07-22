using System;
using System.Globalization;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;
using Calculadora.Services;

namespace Calculadora.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        private readonly CalculatorModel _model;
        private string _displayText = "0";
        private string _expressionText = "";
        private string _liveResultText = "";
        private string _lastFullExpression = "";
        private double _firstOperand;
        private OperationType? _currentOperator;
        private CalculatorState _state = CalculatorState.Start;

        private enum CalculatorState
        {
            Start,
            FirstOperandInput,
            OperatorSelected,
            SecondOperandInput,
            ResultDisplayed,
            Error
        }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                SetProperty(ref _displayText, value);
                UpdateLiveResult();
            }
        }

        public string ExpressionText
        {
            get => _expressionText;
            set => SetProperty(ref _expressionText, value);
        }

        public string LiveResultText
        {
            get => _liveResultText;
            set => SetProperty(ref _liveResultText, value);
        }

        public ICommand NumberInputCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }

        public CalculatorViewModel()
        {
            _model = new CalculatorModel();
            NumberInputCommand = new RelayCommand<string>(OnNumberInput);
            OperationCommand = new RelayCommand<string>(OnOperationInput);
            EqualsCommand = new RelayCommand(OnEqualsInput);
            ClearCommand = new RelayCommand(Reset);
            Reset();
        }

        private void Reset()
        {
            _firstOperand = 0;
            _currentOperator = null;
            _state = CalculatorState.Start;
            _lastFullExpression = "";
            ExpressionText = "";
            LiveResultText = "";
            DisplayText = "0";
        }

        private void OnNumberInput(string? input)
        {
            if (string.IsNullOrEmpty(input)) return;

            if (_state == CalculatorState.Error)
            {
                Reset();
            }

            if (_state == CalculatorState.ResultDisplayed)
            {
                _firstOperand = 0;
                _currentOperator = null;
                ExpressionText = "";
            }

            bool shouldReplaceDisplay = _state == CalculatorState.Start || 
                                        _state == CalculatorState.OperatorSelected || 
                                        _state == CalculatorState.ResultDisplayed;

            if (shouldReplaceDisplay)
            {
                if (input == ".")
                {
                    DisplayText = "0.";
                }
                else
                {
                    DisplayText = input;
                }

                if (_state == CalculatorState.OperatorSelected)
                {
                    _state = CalculatorState.SecondOperandInput;
                }
                else
                {
                    _state = CalculatorState.FirstOperandInput;
                }
            }
            else
            {
                if (input == "." && DisplayText.Contains("."))
                {
                    return;
                }

                if (DisplayText == "0" && input != ".")
                {
                    DisplayText = input;
                    return;
                }

                int digitCount = 0;
                foreach (char c in DisplayText)
                {
                    if (char.IsDigit(c)) digitCount++;
                }

                if (digitCount >= 15)
                {
                    return;
                }

                DisplayText += input;
            }

            UpdateLiveResult();
        }

        private void OnEqualsInput()
        {
            if (_state == CalculatorState.Error) return;
            if (_currentOperator == null) return;

            ExecuteCalculation();

            if (_state != CalculatorState.Error)
            {
                _currentOperator = null;
            }
        }

        private void OnOperationInput(string? opStr)
        {
            if (string.IsNullOrEmpty(opStr)) return;

            OperationType selectedOp = opStr switch
            {
                "+" => OperationType.Add,
                "-" => OperationType.Subtract,
                "*" or "×" or "x" => OperationType.Multiply,
                "/" or "÷" => OperationType.Divide,
                "%" => OperationType.Percentage,
                _ => throw new ArgumentException($"Operador inválido: {opStr}")
            };

            if (_state == CalculatorState.Error) return;

            if (selectedOp == OperationType.Percentage)
            {
                HandlePercentage();
                return;
            }

            if (_state == CalculatorState.SecondOperandInput)
            {
                ExecuteCalculation();
                if (_state == CalculatorState.Error) return;
            }

            if (double.TryParse(DisplayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
            {
                _firstOperand = parsedValue;
            }
            else
            {
                _firstOperand = 0;
            }

            _currentOperator = selectedOp;
            _state = CalculatorState.OperatorSelected;
            ExpressionText = $"{FormatResult(_firstOperand)} {GetOperatorSymbol(_currentOperator.Value)}";
            LiveResultText = "";
        }

        private void ExecuteCalculation()
        {
            if (_currentOperator == null) return;

            if (double.TryParse(DisplayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double secondOperand))
            {
                try
                {
                    double result = _model.Calculate(_firstOperand, secondOperand, _currentOperator.Value);
                    string expr = $"{FormatResult(_firstOperand)} {GetOperatorSymbol(_currentOperator.Value)} {FormatResult(secondOperand)}";
                    _lastFullExpression = expr;
                    ExpressionText = $"{expr} =";
                    DisplayText = FormatResult(result);
                    LiveResultText = "";
                    HistoryService.Instance.Add("Básica", expr, DisplayText);
                    _firstOperand = result;
                    _state = CalculatorState.ResultDisplayed;
                }
                catch (DivideByZeroException)
                {
                    DisplayText = "Error";
                    LiveResultText = "";
                    _state = CalculatorState.Error;
                }
            }
        }

        private void HandlePercentage()
        {
            double currentVal = double.TryParse(DisplayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double cv) ? cv : 0;

            if (_state == CalculatorState.SecondOperandInput && _currentOperator != null)
            {
                double result = _model.Percentage(_firstOperand, currentVal);
                DisplayText = FormatResult(result);
            }
            else
            {
                double result = currentVal / 100.0;
                DisplayText = FormatResult(result);
                _state = CalculatorState.ResultDisplayed;
            }

            UpdateLiveResult();
        }

        private void UpdateLiveResult()
        {
            if (_state == CalculatorState.SecondOperandInput && _currentOperator != null)
            {
                if (double.TryParse(DisplayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double secondOperand))
                {
                    try
                    {
                        double result = _model.Calculate(_firstOperand, secondOperand, _currentOperator.Value);
                        ExpressionText = $"{FormatResult(_firstOperand)} {GetOperatorSymbol(_currentOperator.Value)} {DisplayText}";
                        LiveResultText = $"= {FormatResult(result)}";
                    }
                    catch
                    {
                        LiveResultText = "";
                    }
                }
            }
            else if (_state == CalculatorState.OperatorSelected && _currentOperator != null)
            {
                ExpressionText = $"{FormatResult(_firstOperand)} {GetOperatorSymbol(_currentOperator.Value)}";
                LiveResultText = "";
            }
        }

        private string FormatResult(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return "Error";
            }

            return value.ToString("G15", CultureInfo.InvariantCulture);
        }

        private string GetOperatorSymbol(OperationType op)
        {
            return op switch
            {
                OperationType.Add => "+",
                OperationType.Subtract => "-",
                OperationType.Multiply => "×",
                OperationType.Divide => "÷",
                _ => ""
            };
        }
    }
}
