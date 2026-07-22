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
            set => SetProperty(ref _displayText, value);
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
            DisplayText = "0";
        }

        private void OnNumberInput(string? input)
        {
            if (string.IsNullOrEmpty(input)) return;

            // En estado Error, cualquier número reinicia la calculadora
            if (_state == CalculatorState.Error)
            {
                Reset();
            }

            // Si empezamos un nuevo número tras un resultado previo, limpiamos el estado del cálculo anterior
            if (_state == CalculatorState.ResultDisplayed)
            {
                _firstOperand = 0;
                _currentOperator = null;
            }

            // Determinar si debemos reemplazar todo el display o concatenar
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

                // Transición de estados
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
                // Estamos concatenando al operando actual
                
                // Regla: No duplicar el punto decimal
                if (input == "." && DisplayText.Contains("."))
                {
                    return;
                }

                // Regla: Evitar ceros a la izquierda redundantes (ej: si hay "0" y escribimos "5", pasa a ser "5")
                if (DisplayText == "0" && input != ".")
                {
                    DisplayText = input;
                    return;
                }

                // Regla: Límite máximo de 15 dígitos (excluyendo el punto y signos para evitar desbordamiento)
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
        }

        private void OnEqualsInput()
        {
            if (_state == CalculatorState.Error) return;
            if (_currentOperator == null) return;

            ExecuteCalculation();

            if (_state != CalculatorState.Error)
            {
                _currentOperator = null; // Reseteamos el operador tras el cálculo final
            }
        }

        private void OnOperationInput(string? opStr)
        {
            if (string.IsNullOrEmpty(opStr)) return;

            // Convertir el string a nuestro OperationType enum
            OperationType selectedOp = opStr switch
            {
                "+" => OperationType.Add,
                "-" => OperationType.Subtract,
                "*" or "×" or "x" => OperationType.Multiply,
                "/" or "÷" => OperationType.Divide,
                "%" => OperationType.Percentage,
                _ => throw new ArgumentException($"Operador inválido: {opStr}")
            };

            // Si está en estado de Error, ignoramos operaciones
            if (_state == CalculatorState.Error) return;

            // El porcentaje se maneja como un modificador contextual inmediato
            if (selectedOp == OperationType.Percentage)
            {
                HandlePercentage();
                return;
            }

            // Para operadores estándar (+, -, *, /)
            if (_state == CalculatorState.SecondOperandInput)
            {
                // Si ya se estaba escribiendo el segundo operando, resolvemos la operación previa (encadenamiento)
                ExecuteCalculation();
                
                // Si la operación intermedia dio error, nos detenemos
                if (_state == CalculatorState.Error) return;
            }

            // Guardamos el primer operando (lo que está en display)
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
                    DisplayText = FormatResult(result);
                    HistoryService.Instance.Add("Básica", expr, DisplayText);
                    _firstOperand = result;
                    _state = CalculatorState.ResultDisplayed;
                }
                catch (DivideByZeroException)
                {
                    DisplayText = "Error";
                    _state = CalculatorState.Error;
                }
            }
        }

        private void HandlePercentage()
        {
            double currentVal = double.TryParse(DisplayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double cv) ? cv : 0;

            if (_state == CalculatorState.SecondOperandInput && _currentOperator != null)
            {
                // Porcentaje contextual: se calcula respecto al primer operando (ej: 200 + 10% -> 10% de 200 = 20)
                double result = _model.Percentage(_firstOperand, currentVal);
                DisplayText = FormatResult(result);
                // Mantenemos el estado en SecondOperandInput para que al presionar '=' se ejecute el cálculo final
            }
            else
            {
                // Porcentaje simple/aislado: divide el número actual por 100
                double result = currentVal / 100.0;
                DisplayText = FormatResult(result);
                _state = CalculatorState.ResultDisplayed;
            }
        }

        private string FormatResult(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return "Error";
            }

            // ToString("G15") mantiene hasta 15 dígitos significativos y corrige artifacts de precisión flotante
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
