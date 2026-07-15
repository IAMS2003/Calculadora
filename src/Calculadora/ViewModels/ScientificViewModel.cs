using System;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    public class ScientificViewModel : BaseViewModel
    {
        private string _expression = "";
        private string _result = "";
        private bool _isDegrees = true;

        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnPropertyChanged(nameof(Expression));
                EvaluateLive();
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public bool IsDegrees
        {
            get => _isDegrees;
            set
            {
                _isDegrees = value;
                OnPropertyChanged(nameof(IsDegrees));
                OnPropertyChanged(nameof(AngleModeText));
                EvaluateLive();
            }
        }

        public string AngleModeText => IsDegrees ? "DEG" : "RAD";

        public ICommand AppendCommand { get; }
        public ICommand AppendFunctionCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand EvaluateCommand { get; }
        public ICommand ToggleAngleModeCommand { get; }

        public ScientificViewModel()
        {
            AppendCommand = new RelayCommand<string>(ExecuteAppend);
            AppendFunctionCommand = new RelayCommand<string>(ExecuteAppendFunction);
            ClearCommand = new RelayCommand<object>(ExecuteClear);
            BackspaceCommand = new RelayCommand<object>(ExecuteBackspace);
            EvaluateCommand = new RelayCommand<object>(ExecuteEvaluate);
            ToggleAngleModeCommand = new RelayCommand<object>(ExecuteToggleAngleMode);
        }

        private void ExecuteAppend(string? text)
        {
            if (text != null)
                Expression += text;
        }

        private void ExecuteAppendFunction(string? funcName)
        {
            if (funcName != null)
                Expression += $"{funcName}(";
        }

        private void ExecuteClear(object? parameter)
        {
            Expression = "";
            Result = "";
        }

        private void ExecuteBackspace(object? parameter)
        {
            if (Expression.Length > 0)
            {
                Expression = Expression.Substring(0, Expression.Length - 1);
            }
        }

        private void ExecuteToggleAngleMode(object? parameter)
        {
            IsDegrees = !IsDegrees;
        }

        private void EvaluateLive()
        {
            if (string.IsNullOrWhiteSpace(Expression))
            {
                Result = "";
                return;
            }

            try
            {
                var parser = new ExpressionParser(Expression, IsDegrees);
                double calcResult = parser.Parse();
                
                if (double.IsNaN(calcResult) || double.IsInfinity(calcResult))
                {
                    Result = "Error: Indefinido";
                }
                else
                {
                    Result = calcResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                // Live evaluation fails silently (e.g. typing "sin(")
                Result = "...";
            }
        }

        private void ExecuteEvaluate(object? parameter)
        {
            try
            {
                var parser = new ExpressionParser(Expression, IsDegrees);
                double calcResult = parser.Parse();
                
                if (double.IsNaN(calcResult) || double.IsInfinity(calcResult))
                {
                    Result = "Error: Indefinido";
                }
                else
                {
                    Expression = calcResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    Result = "";
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Result = "Error: Dominio";
            }
            catch (DivideByZeroException)
            {
                Result = "Error: División por cero";
            }
            catch (Exception)
            {
                Result = "Error: Sintaxis";
            }
        }
    }
}
