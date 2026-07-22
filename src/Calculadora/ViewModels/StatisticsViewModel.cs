using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;
using Calculadora.Services;

namespace Calculadora.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        public ObservableCollection<DataPoint> DataPoints { get; } = new();

        private string _newX = "0";
        private string _newY = "0";
        private string _selectedRegressionType = "Lineal (y = mx + b)";
        private string _statsSummaryText = "";
        private string _regressionResultText = "";
        private string _lastRegressionExpression = "";

        public string NewX
        {
            get => _newX;
            set { _newX = value; OnPropertyChanged(nameof(NewX)); }
        }

        public string NewY
        {
            get => _newY;
            set { _newY = value; OnPropertyChanged(nameof(NewY)); }
        }

        public string SelectedRegressionType
        {
            get => _selectedRegressionType;
            set { _selectedRegressionType = value; OnPropertyChanged(nameof(SelectedRegressionType)); CalculateStatistics(); }
        }

        public string StatsSummaryText
        {
            get => _statsSummaryText;
            set { _statsSummaryText = value; OnPropertyChanged(nameof(StatsSummaryText)); }
        }

        public string RegressionResultText
        {
            get => _regressionResultText;
            set { _regressionResultText = value; OnPropertyChanged(nameof(RegressionResultText)); }
        }

        public ObservableCollection<string> RegressionTypes { get; } = new()
        {
            "Lineal (y = mx + b)",
            "Exponencial (y = a · e^bx)"
        };

        public ICommand AddPointCommand { get; }
        public ICommand RemovePointCommand { get; }
        public ICommand ClearPointsCommand { get; }
        public ICommand SendToGraphCommand { get; }

        public StatisticsViewModel()
        {
            // Puntos de ejemplo iniciales
            DataPoints.Add(new DataPoint(1, 2.1));
            DataPoints.Add(new DataPoint(2, 3.9));
            DataPoints.Add(new DataPoint(3, 6.1));
            DataPoints.Add(new DataPoint(4, 8.2));
            DataPoints.Add(new DataPoint(5, 10.0));

            AddPointCommand = new RelayCommand<object>(ExecuteAddPoint);
            RemovePointCommand = new RelayCommand<DataPoint>(ExecuteRemovePoint);
            ClearPointsCommand = new RelayCommand<object>(ExecuteClearPoints);
            SendToGraphCommand = new RelayCommand<object>(ExecuteSendToGraph);

            CalculateStatistics();
        }

        private void ExecuteAddPoint(object? parameter)
        {
            if (double.TryParse(NewX, NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(NewY, NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
            {
                DataPoints.Add(new DataPoint(x, y));
                CalculateStatistics();
            }
        }

        private void ExecuteRemovePoint(DataPoint? point)
        {
            if (point != null && DataPoints.Contains(point))
            {
                DataPoints.Remove(point);
                CalculateStatistics();
            }
        }

        private void ExecuteClearPoints(object? parameter)
        {
            DataPoints.Clear();
            CalculateStatistics();
        }

        private void CalculateStatistics()
        {
            if (DataPoints.Count == 0)
            {
                StatsSummaryText = "Sin datos.";
                RegressionResultText = "";
                _lastRegressionExpression = "";
                return;
            }

            var xValues = DataPoints.Select(p => p.X).ToList();
            var yValues = DataPoints.Select(p => p.Y).ToList();

            double meanX = StatisticsModel.Mean(xValues);
            double meanY = StatisticsModel.Mean(yValues);
            double stdX = StatisticsModel.StandardDeviation(xValues);
            double stdY = StatisticsModel.StandardDeviation(yValues);

            StatsSummaryText = $"n = {DataPoints.Count}\n" +
                               $"x̅ = {meanX:G5},  y̅ = {meanY:G5}\n" +
                               $"σx = {stdX:G5},  σy = {stdY:G5}";

            RegressionResult res;
            if (SelectedRegressionType.StartsWith("Exponencial"))
            {
                res = StatisticsModel.ExponentialRegression(DataPoints.ToList());
            }
            else
            {
                res = StatisticsModel.LinearRegression(DataPoints.ToList());
            }

            RegressionResultText = $"{res.Equation}\n\n{res.Summary}";
            _lastRegressionExpression = res.ExpressionForGraph;

            if (!string.IsNullOrWhiteSpace(res.Equation))
            {
                HistoryService.Instance.Add("Estadística", $"Regresión ({DataPoints.Count} pts)", res.Equation);
            }
        }

        private void ExecuteSendToGraph(object? parameter)
        {
            if (!string.IsNullOrWhiteSpace(_lastRegressionExpression))
            {
                EventAggregator.Instance.SendToGraph(_lastRegressionExpression);
            }
        }
    }
}
