using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calculadora.Models
{
    public enum KeyPointType
    {
        Root,
        YIntercept,
        Maximum,
        Minimum,
        Intersection
    }

    public class KeyPoint
    {
        public double X { get; }
        public double Y { get; }
        public KeyPointType Type { get; }
        public string Label { get; }

        public KeyPoint(double x, double y, KeyPointType type, string label = "")
        {
            X = x;
            Y = y;
            Type = type;
            Label = string.IsNullOrEmpty(label) ? GetDefaultLabel(type, x, y) : label;
        }

        private static string GetDefaultLabel(KeyPointType type, double x, double y)
        {
            string typeName = type switch
            {
                KeyPointType.Root => "Raíz",
                KeyPointType.YIntercept => "Corte Y",
                KeyPointType.Maximum => "Máx",
                KeyPointType.Minimum => "Mín",
                KeyPointType.Intersection => "Intersección",
                _ => "Punto"
            };
            return $"{typeName} ({x.ToString("0.##", CultureInfo.InvariantCulture)}, {y.ToString("0.##", CultureInfo.InvariantCulture)})";
        }
    }

    public static class FunctionAnalyzer
    {
        private static double Evaluate(string expression, double x)
        {
            string injected = expression.Replace("x", $"({x.ToString(CultureInfo.InvariantCulture)})");
            var parser = new ExpressionParser(injected, false);
            return parser.Parse();
        }

        private static double Derivative(string expression, double x, double h = 1e-5)
        {
            double f1 = Evaluate(expression, x + h);
            double f2 = Evaluate(expression, x - h);
            return (f1 - f2) / (2 * h);
        }

        /// <summary>
        /// Encuentra raíces, corte en Y y extremos locales (máximos/mínimos) en el intervalo [minX, maxX].
        /// </summary>
        public static List<KeyPoint> FindKeyPoints(string expression, double minX, double maxX, int steps = 200)
        {
            var points = new List<KeyPoint>();

            if (string.IsNullOrWhiteSpace(expression)) return points;

            // 1. Corte en Y (x = 0)
            if (minX <= 0 && maxX >= 0)
            {
                try
                {
                    double y0 = Evaluate(expression, 0);
                    if (!double.IsNaN(y0) && !double.IsInfinity(y0))
                    {
                        points.Add(new KeyPoint(0, y0, KeyPointType.YIntercept));
                    }
                }
                catch { }
            }

            // 2. Muestreo en el intervalo
            double stepSize = (maxX - minX) / steps;
            double prevX = minX;
            double prevY = double.NaN;
            double prevDeriv = double.NaN;

            try { prevY = Evaluate(expression, prevX); } catch { }
            try { prevDeriv = Derivative(expression, prevX); } catch { }

            for (int i = 1; i <= steps; i++)
            {
                double currX = minX + i * stepSize;
                double currY = double.NaN;
                double currDeriv = double.NaN;

                try { currY = Evaluate(expression, currX); } catch { }
                try { currDeriv = Derivative(expression, currX); } catch { }

                if (!double.IsNaN(prevY) && !double.IsNaN(currY))
                {
                    // Detección de Raíz (cambio de signo en Y)
                    if (prevY * currY <= 0 && Math.Abs(prevY - currY) < 50)
                    {
                        double rootX = FindRootBisection(expression, prevX, currX);
                        if (!double.IsNaN(rootX))
                        {
                            double rootY = Evaluate(expression, rootX);
                            if (Math.Abs(rootY) < 1e-4 && Math.Abs(rootX) > 1e-4) // Evitar duplicar (0,0) con Corte Y
                            {
                                points.Add(new KeyPoint(rootX, 0, KeyPointType.Root));
                            }
                        }
                    }
                }

                if (!double.IsNaN(prevDeriv) && !double.IsNaN(currDeriv))
                {
                    // Detección de Extremo (cambio de signo en f'(x))
                    if (prevDeriv * currDeriv < 0)
                    {
                        double extremumX = (prevX + currX) / 2.0;
                        try
                        {
                            double extremumY = Evaluate(expression, extremumX);
                            if (!double.IsNaN(extremumY) && !double.IsInfinity(extremumY))
                            {
                                KeyPointType type = (prevDeriv > 0) ? KeyPointType.Maximum : KeyPointType.Minimum;
                                points.Add(new KeyPoint(extremumX, extremumY, type));
                            }
                        }
                        catch { }
                    }
                }

                prevX = currX;
                prevY = currY;
                prevDeriv = currDeriv;
            }

            return points;
        }

        /// <summary>
        /// Encuentra intersecciones entre dos funciones f1(x) y f2(x).
        /// </summary>
        public static List<KeyPoint> FindIntersections(string expr1, string expr2, double minX, double maxX, int steps = 150)
        {
            var intersections = new List<KeyPoint>();
            if (string.IsNullOrWhiteSpace(expr1) || string.IsNullOrWhiteSpace(expr2)) return intersections;

            double stepSize = (maxX - minX) / steps;
            double prevDiff = double.NaN;

            for (int i = 0; i <= steps; i++)
            {
                double x = minX + i * stepSize;
                try
                {
                    double y1 = Evaluate(expr1, x);
                    double y2 = Evaluate(expr2, x);
                    double diff = y1 - y2;

                    if (!double.IsNaN(diff) && !double.IsNaN(prevDiff))
                    {
                        if (prevDiff * diff <= 0)
                        {
                            double ix = (x - stepSize + x) / 2.0;
                            double iy = Evaluate(expr1, ix);
                            if (!double.IsNaN(iy) && !double.IsInfinity(iy))
                            {
                                intersections.Add(new KeyPoint(ix, iy, KeyPointType.Intersection));
                            }
                        }
                    }
                    prevDiff = diff;
                }
                catch { }
            }

            return intersections;
        }

        private static double FindRootBisection(string expression, double a, double b, int maxIter = 20)
        {
            double fa = Evaluate(expression, a);
            for (int i = 0; i < maxIter; i++)
            {
                double mid = (a + b) / 2.0;
                double fmid = Evaluate(expression, mid);

                if (Math.Abs(fmid) < 1e-7) return mid;

                if (fa * fmid < 0)
                {
                    b = mid;
                }
                else
                {
                    a = mid;
                    fa = fmid;
                }
            }
            return (a + b) / 2.0;
        }
    }
}
