using System;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class MatrixModelTests
    {
        // ===== CONSTRUCTOR E INDEXER =====

        [Fact]
        public void Constructor_CreatesMatrixWithCorrectDimensions()
        {
            var m = new MatrixModel(3, 4);
            Assert.Equal(3, m.Rows);
            Assert.Equal(4, m.Columns);
        }

        [Fact]
        public void Constructor_FromArray_CopiesDataCorrectly()
        {
            var data = new double[,] { { 1, 2 }, { 3, 4 } };
            var m = new MatrixModel(data);
            Assert.Equal(1, m[0, 0]);
            Assert.Equal(4, m[1, 1]);
        }

        [Fact]
        public void Indexer_SetAndGet_WorksCorrectly()
        {
            var m = new MatrixModel(2, 2);
            m[0, 0] = 5;
            Assert.Equal(5, m[0, 0]);
        }

        [Fact]
        public void Indexer_OutOfRange_ThrowsException()
        {
            var m = new MatrixModel(2, 2);
            Assert.Throws<IndexOutOfRangeException>(() => m[5, 0]);
        }

        [Fact]
        public void Constructor_InvalidDimensions_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new MatrixModel(0, 2));
        }

        // ===== SUMA =====

        [Fact]
        public void Addition_SameDimensions_ReturnsCorrectResult()
        {
            var a = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            var b = new MatrixModel(new double[,] { { 5, 6 }, { 7, 8 } });
            var result = a + b;
            Assert.Equal(6, result[0, 0]);
            Assert.Equal(8, result[0, 1]);
            Assert.Equal(10, result[1, 0]);
            Assert.Equal(12, result[1, 1]);
        }

        [Fact]
        public void Addition_DifferentDimensions_ThrowsException()
        {
            var a = new MatrixModel(2, 2);
            var b = new MatrixModel(2, 3);
            Assert.Throws<InvalidOperationException>(() => a + b);
        }

        // ===== RESTA =====

        [Fact]
        public void Subtraction_SameDimensions_ReturnsCorrectResult()
        {
            var a = new MatrixModel(new double[,] { { 5, 6 }, { 7, 8 } });
            var b = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            var result = a - b;
            Assert.Equal(4, result[0, 0]);
            Assert.Equal(4, result[0, 1]);
        }

        // ===== MULTIPLICACIÓN =====

        [Fact]
        public void Multiplication_CompatibleDimensions_ReturnsCorrectResult()
        {
            // [1 2] × [5 6] = [1*5+2*7  1*6+2*8] = [19 22]
            // [3 4]   [7 8]   [3*5+4*7  3*6+4*8]   [43 50]
            var a = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            var b = new MatrixModel(new double[,] { { 5, 6 }, { 7, 8 } });
            var result = a * b;
            Assert.Equal(19, result[0, 0]);
            Assert.Equal(22, result[0, 1]);
            Assert.Equal(43, result[1, 0]);
            Assert.Equal(50, result[1, 1]);
        }

        [Fact]
        public void Multiplication_IncompatibleDimensions_ThrowsException()
        {
            var a = new MatrixModel(2, 3);
            var b = new MatrixModel(4, 2);
            Assert.Throws<InvalidOperationException>(() => a * b);
        }

        [Fact]
        public void ScalarMultiplication_ReturnsCorrectResult()
        {
            var m = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            var result = 3 * m;
            Assert.Equal(3, result[0, 0]);
            Assert.Equal(6, result[0, 1]);
            Assert.Equal(9, result[1, 0]);
            Assert.Equal(12, result[1, 1]);
        }

        // ===== TRANSPUESTA =====

        [Fact]
        public void Transpose_ReturnsCorrectResult()
        {
            var m = new MatrixModel(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
            var t = m.Transpose();
            Assert.Equal(3, t.Rows);
            Assert.Equal(2, t.Columns);
            Assert.Equal(1, t[0, 0]);
            Assert.Equal(4, t[0, 1]);
            Assert.Equal(2, t[1, 0]);
            Assert.Equal(5, t[1, 1]);
        }

        // ===== TRAZA =====

        [Fact]
        public void Trace_SquareMatrix_ReturnsCorrectSum()
        {
            var m = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            Assert.Equal(5, m.Trace());
        }

        [Fact]
        public void Trace_NonSquareMatrix_ThrowsException()
        {
            var m = new MatrixModel(2, 3);
            Assert.Throws<InvalidOperationException>(() => m.Trace());
        }

        // ===== DETERMINANTE =====

        [Fact]
        public void Determinant_1x1_ReturnsValue()
        {
            var m = new MatrixModel(new double[,] { { 7 } });
            Assert.Equal(7, m.Determinant());
        }

        [Fact]
        public void Determinant_2x2_ReturnsCorrectValue()
        {
            // det([1 2; 3 4]) = 1*4 - 2*3 = -2
            var m = new MatrixModel(new double[,] { { 1, 2 }, { 3, 4 } });
            Assert.Equal(-2, m.Determinant(), 5);
        }

        [Fact]
        public void Determinant_3x3_ReturnsCorrectValue()
        {
            // det([1 2 3; 0 1 4; 5 6 0]) = 1(0-24) - 2(0-20) + 3(0-5) = -24+40-15 = 1
            var m = new MatrixModel(new double[,] { { 1, 2, 3 }, { 0, 1, 4 }, { 5, 6, 0 } });
            Assert.Equal(1, m.Determinant(), 5);
        }

        [Fact]
        public void Determinant_NonSquareMatrix_ThrowsException()
        {
            var m = new MatrixModel(2, 3);
            Assert.Throws<InvalidOperationException>(() => m.Determinant());
        }

        // ===== INVERSA =====

        [Fact]
        public void Inverse_2x2_MultipliedByOriginal_GivesIdentity()
        {
            var m = new MatrixModel(new double[,] { { 4, 7 }, { 2, 6 } });
            var inv = m.Inverse();
            var identity = m * inv;

            // Verificar que es aproximadamente la identidad
            Assert.Equal(1, identity[0, 0], 5);
            Assert.Equal(0, identity[0, 1], 5);
            Assert.Equal(0, identity[1, 0], 5);
            Assert.Equal(1, identity[1, 1], 5);
        }

        [Fact]
        public void Inverse_SingularMatrix_ThrowsException()
        {
            // Singular: rows are linearly dependent
            var m = new MatrixModel(new double[,] { { 1, 2 }, { 2, 4 } });
            Assert.Throws<InvalidOperationException>(() => m.Inverse());
        }

        [Fact]
        public void Inverse_NonSquareMatrix_ThrowsException()
        {
            var m = new MatrixModel(2, 3);
            Assert.Throws<InvalidOperationException>(() => m.Inverse());
        }
    }
}
