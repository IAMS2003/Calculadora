using System;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class ExpressionParserTests
    {
        [Theory]
        [InlineData("2+2", 4)]
        [InlineData("2+3*4", 14)]
        [InlineData("(2+3)*4", 20)]
        [InlineData("10/2-1", 4)]
        [InlineData("2^3", 8)]
        [InlineData("4^0.5", 2)]
        [InlineData("-5+10", 5)]
        [InlineData("2*-3", -6)]
        [InlineData("5!", 120)]
        [InlineData("0!", 1)]
        [InlineData("3!+2", 8)]
        [InlineData("2(3)", 6)]       // Multiplicación implícita: número antes de paréntesis
        [InlineData("3(2+1)", 9)]     // Multiplicación implícita: número antes de expresión entre paréntesis
        [InlineData("(2)(3)", 6)]     // Multiplicación implícita: paréntesis consecutivos — no aplica aquí, se maneja en ParseTerm
        public void Parse_BasicExpressions_ReturnsExpected(string expression, double expected)
        {
            var parser = new ExpressionParser(expression, isDegrees: false);
            double result = parser.Parse();
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("2pi", 6.28318)]      // Multiplicación implícita: número antes de constante
        [InlineData("2e", 5.43656)]        // Multiplicación implícita: número antes de constante e
        public void Parse_ImplicitMultiplicationWithConstants_ReturnsExpected(string expression, double expected)
        {
            var parser = new ExpressionParser(expression, isDegrees: false);
            double result = parser.Parse();
            Assert.Equal(expected, result, 3);
        }

        [Fact]
        public void Parse_TrigonometricFunctions_Radians_ReturnsExpected()
        {
            var parser = new ExpressionParser("sin(pi/2) + cos(pi)", isDegrees: false);
            double result = parser.Parse();
            Assert.Equal(0, result, 5); // sin(pi/2)=1, cos(pi)=-1, 1-1=0
        }

        [Fact]
        public void Parse_TrigonometricFunctions_Degrees_ReturnsExpected()
        {
            var parser = new ExpressionParser("sin(90) + cos(180)", isDegrees: true);
            double result = parser.Parse();
            Assert.Equal(0, result, 5); 
        }

        [Theory]
        [InlineData("log(100)", 2)]
        [InlineData("ln(e)", 1)]
        [InlineData("sqrt(16)", 4)]
        [InlineData("cbrt(27)", 3)]
        public void Parse_LogAndRoots_ReturnsExpected(string expression, double expected)
        {
            var parser = new ExpressionParser(expression, isDegrees: false);
            double result = parser.Parse();
            Assert.Equal(expected, result, 5);
        }

        [Fact]
        public void Parse_InvalidSyntax_ThrowsException()
        {
            var parser = new ExpressionParser("2+*3");
            Assert.Throws<Exception>(() => parser.Parse());
        }

        [Fact]
        public void Parse_DomainError_ThrowsArgumentOutOfRangeException()
        {
            var parser = new ExpressionParser("sqrt(-1)");
            Assert.Throws<ArgumentOutOfRangeException>(() => parser.Parse());
        }

        [Fact]
        public void Parse_DivideByZero_ThrowsDivideByZeroException()
        {
            var parser = new ExpressionParser("10/0");
            Assert.Throws<DivideByZeroException>(() => parser.Parse());
        }
    }
}
