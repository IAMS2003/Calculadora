using System;
using System.Globalization;
using System.Text;

namespace Calculadora.Models
{
    /// <summary>
    /// Representa una matriz de números reales (double) con soporte para
    /// operator overloading, indexers y operaciones de álgebra lineal.
    /// </summary>
    public class MatrixModel
    {
        private readonly double[,] _data;

        public int Rows { get; }
        public int Columns { get; }

        // Indexer para acceso con matrix[i, j]
        public double this[int row, int col]
        {
            get
            {
                ValidateIndices(row, col);
                return _data[row, col];
            }
            set
            {
                ValidateIndices(row, col);
                _data[row, col] = value;
            }
        }

        public MatrixModel(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
                throw new ArgumentException("Las dimensiones de la matriz deben ser positivas.");
            Rows = rows;
            Columns = columns;
            _data = new double[rows, columns];
        }

        public MatrixModel(double[,] data)
        {
            Rows = data.GetLength(0);
            Columns = data.GetLength(1);
            _data = (double[,])data.Clone();
        }

        // ===== OPERADORES ARITMÉTICOS =====

        public static MatrixModel operator +(MatrixModel a, MatrixModel b)
        {
            ValidateSameDimensions(a, b, "sumar");
            var result = new MatrixModel(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Columns; j++)
                    result[i, j] = a[i, j] + b[i, j];
            return result;
        }

        public static MatrixModel operator -(MatrixModel a, MatrixModel b)
        {
            ValidateSameDimensions(a, b, "restar");
            var result = new MatrixModel(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Columns; j++)
                    result[i, j] = a[i, j] - b[i, j];
            return result;
        }

        public static MatrixModel operator *(MatrixModel a, MatrixModel b)
        {
            if (a.Columns != b.Rows)
                throw new InvalidOperationException(
                    $"Dimensiones incompatibles para multiplicación: ({a.Rows}×{a.Columns}) × ({b.Rows}×{b.Columns}). " +
                    $"Se requiere que las columnas de A ({a.Columns}) sean iguales a las filas de B ({b.Rows}).");

            var result = new MatrixModel(a.Rows, b.Columns);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Columns; k++)
                        sum += a[i, k] * b[k, j];
                    result[i, j] = sum;
                }
            return result;
        }

        public static MatrixModel operator *(double scalar, MatrixModel m)
        {
            var result = new MatrixModel(m.Rows, m.Columns);
            for (int i = 0; i < m.Rows; i++)
                for (int j = 0; j < m.Columns; j++)
                    result[i, j] = scalar * m[i, j];
            return result;
        }

        public static MatrixModel operator *(MatrixModel m, double scalar) => scalar * m;

        // ===== OPERACIONES DE ÁLGEBRA LINEAL =====

        /// <summary>
        /// Calcula la transpuesta de la matriz.
        /// </summary>
        public MatrixModel Transpose()
        {
            var result = new MatrixModel(Columns, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[j, i] = this[i, j];
            return result;
        }

        /// <summary>
        /// Calcula la traza (suma de la diagonal principal). Requiere matriz cuadrada.
        /// </summary>
        public double Trace()
        {
            ValidateSquare("calcular la traza");
            double sum = 0;
            for (int i = 0; i < Rows; i++)
                sum += this[i, i];
            return sum;
        }

        /// <summary>
        /// Calcula el determinante usando expansión por cofactores (recursivo).
        /// Requiere matriz cuadrada.
        /// </summary>
        public double Determinant()
        {
            ValidateSquare("calcular el determinante");

            if (Rows == 1) return this[0, 0];
            if (Rows == 2) return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];

            double det = 0;
            for (int j = 0; j < Columns; j++)
            {
                int sign = (j % 2 == 0) ? 1 : -1;
                det += sign * this[0, j] * Minor(0, j).Determinant();
            }
            return det;
        }

        /// <summary>
        /// Calcula la matriz inversa usando la matriz adjunta.
        /// Requiere matriz cuadrada con determinante != 0.
        /// </summary>
        public MatrixModel Inverse()
        {
            ValidateSquare("calcular la inversa");

            double det = Determinant();
            if (Math.Abs(det) < 1e-12)
                throw new InvalidOperationException("Matriz singular (det=0), no tiene inversa.");

            var cofactorMatrix = new MatrixModel(Rows, Columns);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    int sign = ((i + j) % 2 == 0) ? 1 : -1;
                    cofactorMatrix[i, j] = sign * Minor(i, j).Determinant();
                }

            // La inversa es la transpuesta de la cofactora dividida por el determinante
            return (1.0 / det) * cofactorMatrix.Transpose();
        }

        /// <summary>
        /// Calcula la submatriz eliminando la fila y columna indicadas (para cofactores).
        /// </summary>
        private MatrixModel Minor(int excludeRow, int excludeCol)
        {
            var result = new MatrixModel(Rows - 1, Columns - 1);
            int ri = 0;
            for (int i = 0; i < Rows; i++)
            {
                if (i == excludeRow) continue;
                int ci = 0;
                for (int j = 0; j < Columns; j++)
                {
                    if (j == excludeCol) continue;
                    result[ri, ci] = this[i, j];
                    ci++;
                }
                ri++;
            }
            return result;
        }

        // ===== FORMATEO =====

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                sb.Append("│ ");
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(this[i, j].ToString("G4", CultureInfo.InvariantCulture).PadLeft(10));
                    if (j < Columns - 1) sb.Append("  ");
                }
                sb.AppendLine(" │");
            }
            return sb.ToString();
        }

        // ===== VALIDACIÓN =====

        private void ValidateIndices(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Columns)
                throw new IndexOutOfRangeException(
                    $"Índice [{row},{col}] fuera de rango para matriz de {Rows}×{Columns}.");
        }

        private void ValidateSquare(string operation)
        {
            if (Rows != Columns)
                throw new InvalidOperationException(
                    $"No se puede {operation} de una matriz no cuadrada ({Rows}×{Columns}).");
        }

        private static void ValidateSameDimensions(MatrixModel a, MatrixModel b, string operation)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new InvalidOperationException(
                    $"Dimensiones incompatibles para {operation}: ({a.Rows}×{a.Columns}) vs ({b.Rows}×{b.Columns}).");
        }
    }
}
