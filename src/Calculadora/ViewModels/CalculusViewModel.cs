using System;
using System.Globalization;
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

        // Propiedades para Integrales
        private string _integralLowerBound = "0";
        private string _integralUpperBound = "1";
        private string _integralMethod = "Simpson";

        // Propiedades para Ecuaciones
        private string _newtonInitialGuess = "1";
        private string _quadA = "1";
        private string _quadB = "0";
        private string _quadC = "-1";

        public string InputExpression
        {
            get => _inputExpression;
            set { _inputExpression = value; OnPropertyChanged(nameof(InputExpression)); ErrorMessage = ""; }
        }

        public string Variable
        {
            get => _variable;
            set { _variable = value; OnPropertyChanged(nameof(Variable)); }
        }

        public string ResultExpression
        {
            get => _resultExpression;
            set { _resultExpression = value; OnPropertyChanged(nameof(ResultExpression)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged(nameof(SelectedTabIndex)); }
        }

        public string IntegralLowerBound
        {
            get => _integralLowerBound;
            set { _integralLowerBound = value; OnPropertyChanged(nameof(IntegralLowerBound)); }
        }

        public string IntegralUpperBound
        {
            get => _integralUpperBound;
            set { _integralUpperBound = value; OnPropertyChanged(nameof(IntegralUpperBound)); }
        }

        public string IntegralMethod
        {
            get => _integralMethod;
            set { _integralMethod = value; OnPropertyChanged(nameof(IntegralMethod)); }
        }

        public string NewtonInitialGuess
        {
            get => _newtonInitialGuess;
            set { _newtonInitialGuess = value; OnPropertyChanged(nameof(NewtonInitialGuess)); }
        }

        public string QuadA { get => _quadA; set { _quadA = value; OnPropertyChanged(nameof(QuadA)); } }
        public string QuadB { get => _quadB; set { _quadB = value; OnPropertyChanged(nameof(QuadB)); } }
        public string QuadC { get => _quadC; set { _quadC = value; OnPropertyChanged(nameof(QuadC)); } }

        // Comandos Simbólicos
        public ICommand DifferentiateCommand { get; }
        public ICommand SimplifyCommand { get; }
        public ICommand ExpandCommand { get; }
        public ICommand SendToGraphCommand { get; }
        public ICommand ClearCommand { get; }

        // Comandos Numéricos
        public ICommand IntegrateCommand { get; }
        public ICommand NewtonRaphsonCommand { get; }
        public ICommand SolveQuadraticCommand { get; }

        public CalculusViewModel()
        {
            DifferentiateCommand = new RelayCommand<object>(ExecuteDifferentiate);
            SimplifyCommand = new RelayCommand<object>(ExecuteSimplify);
            ExpandCommand = new RelayCommand<object>(ExecuteExpand);
            SendToGraphCommand = new RelayCommand<object>(ExecuteSendToGraph);
            ClearCommand = new RelayCommand<object>(ExecuteClear);
            IntegrateCommand = new RelayCommand<object>(ExecuteIntegrate);
            NewtonRaphsonCommand = new RelayCommand<object>(ExecuteNewtonRaphson);
            SolveQuadraticCommand = new RelayCommand<object>(ExecuteSolveQuadratic);
        }

        // ===== SIMBÓLICO =====

        private void ExecuteDifferentiate(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;
            try
            {
                ResultExpression = _adapter.Differentiate(InputExpression, Variable);
                HistoryService.Instance.Add("Cálculo", $"d/d{Variable}({InputExpression})", ResultExpression);
                ErrorMessage = "";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        private void ExecuteSimplify(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;
            try
            {
                ResultExpression = _adapter.Simplify(InputExpression);
                HistoryService.Instance.Add("Cálculo", $"Simplify({InputExpression})", ResultExpression);
                ErrorMessage = "";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        private void ExecuteExpand(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;
            try
            {
                ResultExpression = _adapter.Expand(InputExpression);
                HistoryService.Instance.Add("Cálculo", $"Expand({InputExpression})", ResultExpression);
                ErrorMessage = "";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        // ===== INTEGRALES =====

        private void ExecuteIntegrate(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;
            ErrorMessage = "";

            try
            {
                if (!double.TryParse(IntegralLowerBound, NumberStyles.Float, CultureInfo.InvariantCulture, out double a))
                    throw new ArgumentException("Límite inferior inválido.");
                if (!double.TryParse(IntegralUpperBound, NumberStyles.Float, CultureInfo.InvariantCulture, out double b))
                    throw new ArgumentException("Límite superior inválido.");

                Func<double, double> f = x =>
                {
                    string expr = InputExpression.Replace("x", $"({x.ToString(CultureInfo.InvariantCulture)})");
                    var parser = new ExpressionParser(expr, false);
                    return parser.Parse();
                };

                double result = IntegralMethod == "Trapecio"
                    ? NumericalCalculus.TrapezoidalRule(f, a, b)
                    : NumericalCalculus.SimpsonRule(f, a, b);

                ResultExpression = $"∫[{a},{b}] f(x) dx ≈ {result.ToString("G10", CultureInfo.InvariantCulture)}";
                HistoryService.Instance.Add("Cálculo", $"∫[{a},{b}] ({InputExpression}) dx", result.ToString("G10", CultureInfo.InvariantCulture));
            }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        // ===== ECUACIONES =====

        private void ExecuteNewtonRaphson(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputExpression)) return;
            ErrorMessage = "";

            try
            {
                if (!double.TryParse(NewtonInitialGuess, NumberStyles.Float, CultureInfo.InvariantCulture, out double x0))
                    throw new ArgumentException("Punto inicial inválido.");

                Func<double, double> f = x =>
                {
                    string expr = InputExpression.Replace("x", $"({x.ToString(CultureInfo.InvariantCulture)})");
                    var parser = new ExpressionParser(expr, false);
                    return parser.Parse();
                };

                double root = NumericalCalculus.NewtonRaphson(f, x0);
                ResultExpression = $"Raíz encontrada: x = {root.ToString("G10", CultureInfo.InvariantCulture)}";
                HistoryService.Instance.Add("Cálculo", $"Root({InputExpression} = 0, x0={x0})", root.ToString("G10", CultureInfo.InvariantCulture));
            }
            catch (ConvergenceException ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        private void ExecuteSolveQuadratic(object? parameter)
        {
            ErrorMessage = "";
            try
            {
                double a = double.Parse(QuadA, CultureInfo.InvariantCulture);
                double b = double.Parse(QuadB, CultureInfo.InvariantCulture);
                double c = double.Parse(QuadC, CultureInfo.InvariantCulture);

                var (_, _, description) = NumericalCalculus.SolveQuadratic(a, b, c);
                ResultExpression = description;
                HistoryService.Instance.Add("Cálculo", $"{a}x² + {b}x + {c} = 0", description);
            }
            catch (FormatException) { ErrorMessage = "Coeficientes inválidos."; ResultExpression = ""; }
            catch (Exception ex) { ErrorMessage = ex.Message; ResultExpression = ""; }
        }

        // ===== COMUNES =====

        private void ExecuteSendToGraph(object? parameter)
        {
            string toSend = !string.IsNullOrWhiteSpace(ResultExpression) ? ResultExpression : InputExpression;
            if (!string.IsNullOrWhiteSpace(toSend))
                EventAggregator.Instance.SendToGraph(toSend);
        }

        private void ExecuteClear(object? parameter)
        {
            InputExpression = "";
            ResultExpression = "";
            ErrorMessage = "";
        }
    }
}
