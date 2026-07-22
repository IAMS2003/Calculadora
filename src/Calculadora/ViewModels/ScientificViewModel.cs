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
        private bool _isEvaluated = false;
        private string _lastResultStr = "";

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
            if (text == null) return;

            if (_isEvaluated)
            {
                _isEvaluated = false;
                // Si se presiona un operador (+ - * / ^), encadenar con el resultado anterior
                if (text == "+" || text == "-" || text == "*" || text == "/" || text == "^" || text == "%")
                {
                    Expression = _lastResultStr + text;
                    return;
                }
                else
                {
                    Expression = "";
                }
            }

            Expression += text;
        }

        private void ExecuteAppendFunction(string? funcName)
        {
            if (funcName == null) return;

            if (_isEvaluated)
            {
                _isEvaluated = false;
                Expression = "";
            }

            Expression += $"{funcName}(";
        }

        private void ExecuteClear(object? parameter)
        {
            Expression = "";
            Result = "";
            _isEvaluated = false;
            _lastResultStr = "";
        }

        private void ExecuteBackspace(object? parameter)
        {
            if (_isEvaluated)
            {
                _isEvaluated = false;
            }

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

            if (_isEvaluated) return;

            try
            {
                var parser = new ExpressionParser(Expression, IsDegrees);
                double calcResult = parser.Parse();
                
                if (double.IsNaN(calcResult) || double.IsInfinity(calcResult))
                {
                    Result = "= Indefinido";
                }
                else
                {
                    Result = $"= {calcResult.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                }
            }
            catch
            {
                Result = "";
            }
        }

        private void ExecuteEvaluate(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(Expression)) return;

            try
            {
                var parser = new ExpressionParser(Expression, IsDegrees);
                double calcResult = parser.Parse();
                
                if (double.IsNaN(calcResult) || double.IsInfinity(calcResult))
                {
                    Result = "= Indefinido";
                }
                else
                {
                    string oldExpr = Expression;
                    string resStr = calcResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    Calculadora.Services.HistoryService.Instance.Add("Científica", oldExpr, resStr);
                    
                    _lastResultStr = resStr;
                    Result = $"= {resStr}";
                    _isEvaluated = true;
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
