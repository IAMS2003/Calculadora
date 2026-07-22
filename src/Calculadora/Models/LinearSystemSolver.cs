using System;
using System.Globalization;
using System.Text;

namespace Calculadora.Models
{
    public static class LinearSystemSolver
    {
        /// <summary>
        /// Resuelve un sistema de ecuaciones lineales A * X = B usando eliminación de Gauss-Jordan.
        /// </summary>
        public static (double[]? solution, string description) Solve(MatrixModel a, double[] b)
        {
            if (a.Rows != a.Columns)
                return (null, "La matriz de coeficientes debe ser cuadrada (N×N).");

            if (a.Rows != b.Length)
                return (null, "El vector de constantes B debe coincidir con el número de filas de A.");

            int n = a.Rows;
            double[,] aug = new double[n, n + 1];

            // Construir matriz aumentada [A | B]
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = a[i, j];
                aug[i, n] = b[i];
            }

            // Eliminación de Gauss-Jordan con pivoteo parcial
            for (int p = 0; p < n; p++)
            {
                // Buscar pivote máximo
                int maxRow = p;
                for (int i = p + 1; i < n; i++)
                {
                    if (Math.Abs(aug[i, p]) > Math.Abs(aug[maxRow, p]))
                        maxRow = i;
                }

                // Intercambiar filas
                for (int k = 0; k <= n; k++)
                {
                    double temp = aug[p, k];
                    aug[p, k] = aug[maxRow, k];
                    aug[maxRow, k] = temp;
                }

                if (Math.Abs(aug[p, p]) < 1e-12)
                {
                    return (null, "El sistema no tiene solución única (matriz singular o dependiente).");
                }

                // Normalizar fila pivote
                double pivot = aug[p, p];
                for (int j = p; j <= n; j++)
                    aug[p, j] /= pivot;

                // Eliminar en otras filas
                for (int i = 0; i < n; i++)
                {
                    if (i != p)
                    {
                        double factor = aug[i, p];
                        for (int j = p; j <= n; j++)
                            aug[i, j] -= factor * aug[p, j];
                    }
                }
            }

            // Extraer solución
            double[] x = new double[n];
            var sb = new StringBuilder();
            sb.AppendLine("Solución del Sistema AX = B:");

            for (int i = 0; i < n; i++)
            {
                x[i] = aug[i, n];
                sb.AppendLine($"x{i + 1} = {x[i].ToString("G6", CultureInfo.InvariantCulture)}");
            }

            return (x, sb.ToString().TrimEnd());
        }
    }
}
