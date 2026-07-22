using System;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class NumericalCalculusTests
    {
        // ===== INTEGRACIÓN: TRAPECIO =====

        [Fact]
        public void TrapezoidalRule_LinearFunction_ExactResult()
        {
            // ∫[0,1] 2x dx = 1
            double result = NumericalCalculus.TrapezoidalRule(x => 2 * x, 0, 1);
            Assert.Equal(1.0, result, 5);
        }

        [Fact]
        public void TrapezoidalRule_XSquared_ApproximatesCorrectly()
        {
            // ∫[0,1] x² dx = 1/3
            double result = NumericalCalculus.TrapezoidalRule(x => x * x, 0, 1);
            Assert.Equal(1.0 / 3.0, result, 3);
        }

        [Fact]
        public void TrapezoidalRule_ReversedBounds_NegatesResult()
        {
            double forward = NumericalCalculus.TrapezoidalRule(x => x, 0, 1);
            double reversed = NumericalCalculus.TrapezoidalRule(x => x, 1, 0);
            Assert.Equal(-forward, reversed, 5);
        }

        [Fact]
        public void TrapezoidalRule_SameBounds_ReturnsZero()
        {
            double result = NumericalCalculus.TrapezoidalRule(x => x * x, 3, 3);
            Assert.Equal(0, result, 10);
        }

        // ===== INTEGRACIÓN: SIMPSON =====

        [Fact]
        public void SimpsonRule_XSquared_HighPrecision()
        {
            // ∫[0,1] x² dx = 1/3 — Simpson es exacta para polinomios ≤ grado 3
            double result = NumericalCalculus.SimpsonRule(x => x * x, 0, 1);
            Assert.Equal(1.0 / 3.0, result, 8);
        }

        [Fact]
        public void SimpsonRule_SinX_ApproximatesCorrectly()
        {
            // ∫[0,π] sin(x) dx = 2
            double result = NumericalCalculus.SimpsonRule(x => Math.Sin(x), 0, Math.PI);
            Assert.Equal(2.0, result, 5);
        }

        [Fact]
        public void SimpsonRule_ExpX_ApproximatesCorrectly()
        {
            // ∫[0,1] e^x dx = e - 1 ≈ 1.71828
            double result = NumericalCalculus.SimpsonRule(x => Math.Exp(x), 0, 1);
            Assert.Equal(Math.E - 1, result, 5);
        }

        [Fact]
        public void SimpsonRule_InvalidSubintervals_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => NumericalCalculus.SimpsonRule(x => x, 0, 1, 0));
        }

        // ===== NEWTON-RAPHSON =====

        [Fact]
        public void NewtonRaphson_XSquaredMinus4_FindsRoot2()
        {
            // x² - 4 = 0 → x = ±2
            double root = NumericalCalculus.NewtonRaphson(x => x * x - 4, 1);
            Assert.Equal(2.0, root, 8);
        }

        [Fact]
        public void NewtonRaphson_XSquaredMinus4_NegativeGuess_FindsNegativeRoot()
        {
            double root = NumericalCalculus.NewtonRaphson(x => x * x - 4, -1);
            Assert.Equal(-2.0, root, 8);
        }

        [Fact]
        public void NewtonRaphson_CosX_FindsPiOver2()
        {
            // cos(x) = 0 → x = π/2
            double root = NumericalCalculus.NewtonRaphson(x => Math.Cos(x), 1);
            Assert.Equal(Math.PI / 2, root, 5);
        }

        [Fact]
        public void NewtonRaphson_NoConvergence_ThrowsConvergenceException()
        {
            // x² + 1 = 0 no tiene raíces reales
            Assert.Throws<ConvergenceException>(() =>
                NumericalCalculus.NewtonRaphson(x => x * x + 1, 0, maxIterations: 100));
        }

        // ===== FÓRMULA CUADRÁTICA =====

        [Fact]
        public void SolveQuadratic_TwoRealRoots_ReturnsCorrectRoots()
        {
            // x² - 5x + 6 = 0 → x = 2, x = 3
            var (r1, r2, _) = NumericalCalculus.SolveQuadratic(1, -5, 6);
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.Equal(3, r1!.Value, 5);
            Assert.Equal(2, r2!.Value, 5);
        }

        [Fact]
        public void SolveQuadratic_DoubleRoot_ReturnsSameRoot()
        {
            // x² - 4x + 4 = 0 → x = 2 (doble)
            var (r1, r2, _) = NumericalCalculus.SolveQuadratic(1, -4, 4);
            Assert.NotNull(r1);
            Assert.Equal(r1, r2);
            Assert.Equal(2, r1!.Value, 5);
        }

        [Fact]
        public void SolveQuadratic_NoRealRoots_ReturnsNulls()
        {
            // x² + 1 = 0 → sin raíces reales
            var (r1, r2, description) = NumericalCalculus.SolveQuadratic(1, 0, 1);
            Assert.Null(r1);
            Assert.Null(r2);
            Assert.Contains("complejas", description);
        }

        [Fact]
        public void SolveQuadratic_LinearEquation_ReturnsSingleRoot()
        {
            // 0x² + 2x - 6 = 0 → x = 3
            var (r1, _, _) = NumericalCalculus.SolveQuadratic(0, 2, -6);
            Assert.NotNull(r1);
            Assert.Equal(3, r1!.Value, 5);
        }
    }
}
