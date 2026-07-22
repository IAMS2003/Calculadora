using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class LinearSystemSolverTests
    {
        [Fact]
        public void Solve_2x2System_ReturnsCorrectSolution()
        {
            // 2x + y = 5
            // x + 3y = 10
            // Solution: x = 1, y = 3
            var a = new MatrixModel(new double[,] { { 2, 1 }, { 1, 3 } });
            var b = new double[] { 5, 10 };

            var (x, desc) = LinearSystemSolver.Solve(a, b);

            Assert.NotNull(x);
            Assert.Equal(1.0, x![0], 4);
            Assert.Equal(3.0, x![1], 4);
        }

        [Fact]
        public void Solve_SingularMatrix_ReturnsNull()
        {
            // Linearly dependent
            var a = new MatrixModel(new double[,] { { 1, 2 }, { 2, 4 } });
            var b = new double[] { 3, 6 };

            var (x, desc) = LinearSystemSolver.Solve(a, b);

            Assert.Null(x);
            Assert.Contains("singular", desc.ToLower());
        }
    }
}
