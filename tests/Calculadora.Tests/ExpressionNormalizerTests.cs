using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class ExpressionNormalizerTests
    {
        [Theory]
        [InlineData("3x", "3*x")]
        [InlineData("2pi", "2*pi")]
        [InlineData("2sin(x)", "2*sin(x)")]
        [InlineData("2(3+1)", "2*(3+1)")]
        [InlineData("(2)(3)", "(2)*(3)")]
        [InlineData(")x", ")*x")]
        [InlineData(")2", ")*2")]
        [InlineData(")(", ")*(")]
        [InlineData("5!3", "5!*3")]
        [InlineData("5!x", "5!*x")]
        [InlineData("5!(2)", "5!*(2)")]
        [InlineData("3x+2y", "3*x+2*y")]
        [InlineData("2x^2+3x+1", "2*x^2+3*x+1")]
        public void Normalize_ImplicitMultiplication_InsertsAsterisk(string input, string expected)
        {
            string result = ExpressionNormalizer.Normalize(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("sin(x)", "sin(x)")]
        [InlineData("cos(2*x)", "cos(2*x)")]
        [InlineData("3+2", "3+2")]
        [InlineData("3*x", "3*x")]
        [InlineData("x^2", "x^2")]
        [InlineData("", "")]
        public void Normalize_AlreadyExplicit_NoChange(string input, string expected)
        {
            string result = ExpressionNormalizer.Normalize(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("3 x + 2", "3*x+2")]
        [InlineData("sin ( x )", "sin(x)")]
        public void Normalize_WithSpaces_RemovesAndNormalizes(string input, string expected)
        {
            string result = ExpressionNormalizer.Normalize(input);
            Assert.Equal(expected, result);
        }
    }
}
