using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Calculadora.ViewModels;

namespace Calculadora.Models
{
    public class DataPoint : BaseViewModel
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(nameof(X)); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(nameof(Y)); }
        }

        public DataPoint(double x, double y)
        {
            _x = x;
            _y = y;
        }
    }

    public class RegressionResult
    {
        public string Equation { get; set; } = "";
        public string ExpressionForGraph { get; set; } = "";
        public double R2 { get; set; }
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public string Summary { get; set; } = "";
    }

    public static class StatisticsModel
    {
        public static double Mean(IEnumerable<double> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? 0 : list.Average();
        }

        public static double StandardDeviation(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count <= 1) return 0;
            double avg = list.Average();
            double sumSquares = list.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (list.Count - 1));
        }

        /// <summary>
        /// Regresión lineal simple: y = m*x + b
        /// </summary>
        public static RegressionResult LinearRegression(List<DataPoint> points)
        {
            if (points == null || points.Count < 2)
            {
                return new RegressionResult { Summary = "Se requieren al menos 2 puntos de datos." };
            }

            int n = points.Count;
            double sumX = points.Sum(p => p.X);
            double sumY = points.Sum(p => p.Y);
            double sumXY = points.Sum(p => p.X * p.Y);
            double sumX2 = points.Sum(p => p.X * p.X);
            double sumY2 = points.Sum(p => p.Y * p.Y);

            double denominator = (n * sumX2 - sumX * sumX);
            if (Math.Abs(denominator) < 1e-12)
            {
                return new RegressionResult { Summary = "Puntos verticales (denominador cero)." };
            }

            double slope = (n * sumXY - sumX * sumY) / denominator;
            double intercept = (sumY - slope * sumX) / n;

            // R^2
            double meanY = sumY / n;
            double ssTot = points.Sum(p => Math.Pow(p.Y - meanY, 2));
            double ssRes = points.Sum(p => Math.Pow(p.Y - (slope * p.X + intercept), 2));
            double r2 = (ssTot > 1e-12) ? Math.Max(0, 1.0 - (ssRes / ssTot)) : 1.0;

            string slopeStr = slope.ToString("G5", CultureInfo.InvariantCulture);
            string interceptSign = intercept >= 0 ? "+" : "-";
            string interceptStr = Math.Abs(intercept).ToString("G5", CultureInfo.InvariantCulture);

            string eq = $"y = {slopeStr}*x {interceptSign} {interceptStr}";
            string expr = $"{slopeStr}*x {interceptSign} {interceptStr}";

            return new RegressionResult
            {
                Equation = eq,
                ExpressionForGraph = expr,
                Slope = slope,
                Intercept = intercept,
                R2 = r2,
                Summary = $"Pendiente (m) = {slopeStr}\nIntersección (b) = {intercept:G5}\nR² = {r2:F4}"
            };
        }

        /// <summary>
        /// Regresión Exponencial: y = a * e^(b*x)
        /// </summary>
        public static RegressionResult ExponentialRegression(List<DataPoint> points)
        {
            var validPoints = points.Where(p => p.Y > 0).ToList();
            if (validPoints.Count < 2)
            {
                return new RegressionResult { Summary = "Se requieren al menos 2 puntos con Y > 0 para regresión exponencial." };
            }

            var transformed = validPoints.Select(p => new DataPoint(p.X, Math.Log(p.Y))).ToList();
            var linRes = LinearRegression(transformed);

            double a = Math.Exp(linRes.Intercept);
            double b = linRes.Slope;

            string aStr = a.ToString("G5", CultureInfo.InvariantCulture);
            string bStr = b.ToString("G5", CultureInfo.InvariantCulture);

            string eq = $"y = {aStr} * exp({bStr}*x)";
            string expr = $"{aStr}*exp({bStr}*x)";

            return new RegressionResult
            {
                Equation = eq,
                ExpressionForGraph = expr,
                Slope = b,
                Intercept = a,
                R2 = linRes.R2,
                Summary = $"a = {aStr}\nb = {bStr}\nR² = {linRes.R2:F4}"
            };
        }
    }
}
