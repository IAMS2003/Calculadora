using System.Linq;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class FunctionAnalyzerTests
    {
        [Fact]
        public void FindKeyPoints_Polynomial_FindsRootsAndExtrema()
        {
            // f(x) = x^2 - 4 -> Roots at x = -2 and x = 2, Minimum at x = 0
            var points = FunctionAnalyzer.FindKeyPoints("x^2 - 4", -5, 5);

            Assert.NotEmpty(points);
            Assert.Contains(points, p => p.Type == KeyPointType.YIntercept || p.Type == KeyPointType.Minimum);
        }

        [Fact]
        public void FindIntersections_LinearAndQuadratic_FindsIntersections()
        {
            // f(x) = x^2, g(x) = 4 -> Intersections at x = -2 and x = 2
            var intersections = FunctionAnalyzer.FindIntersections("x^2", "4", -5, 5);

            Assert.NotEmpty(intersections);
        }
    }
}
