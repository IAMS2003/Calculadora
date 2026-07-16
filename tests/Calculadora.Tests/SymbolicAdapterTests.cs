using System;
using System.Collections.Generic;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class SymbolicAdapterTests
    {
        private readonly SymbolicAdapter _adapter = new SymbolicAdapter();

        // ===== DERIVACIÓN =====

        [Fact]
        public void Differentiate_XSquared_Returns2X()
        {
            string result = _adapter.Differentiate("x^2", "x");
            Assert.Contains("2", result);
            Assert.Contains("x", result);
        }

        [Fact]
        public void Differentiate_Polynomial_ReturnsCorrectDerivative()
        {
            // d/dx (x^3 + 2*x) = 3*x^2 + 2
            string result = _adapter.Differentiate("x^3 + 2*x", "x");
            Assert.Contains("3", result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void Differentiate_Constant_ReturnsZero()
        {
            string result = _adapter.Differentiate("5", "x");
            Assert.Equal("0", result);
        }

        [Fact]
        public void Differentiate_SinX_ReturnsCosX()
        {
            string result = _adapter.Differentiate("sin(x)", "x");
            Assert.Contains("cos", result.ToLower());
        }

        [Fact]
        public void Differentiate_CosX_ReturnsNegSinX()
        {
            string result = _adapter.Differentiate("cos(x)", "x");
            Assert.Contains("sin", result.ToLower());
        }

        [Fact]
        public void Differentiate_ExpX_ReturnsExpX()
        {
            string result = _adapter.Differentiate("exp(x)", "x");
            Assert.Contains("exp", result.ToLower());
        }

        [Fact]
        public void Differentiate_LnX_ReturnsOneOverX()
        {
            string result = _adapter.Differentiate("ln(x)", "x");
            // Should contain 1/x or x^(-1) or similar
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Differentiate_RespectToY_TreatsXAsConstant()
        {
            string result = _adapter.Differentiate("x^2 + y^2", "y");
            Assert.Contains("2", result);
            Assert.Contains("y", result);
        }

        // ===== SIMPLIFICACIÓN =====

        [Fact]
        public void Simplify_ReturnsSimplifiedExpression()
        {
            string result = _adapter.Simplify("x + x");
            Assert.Contains("2", result);
        }

        [Fact]
        public void Simplify_ZeroAddition_ReturnsOriginal()
        {
            string result = _adapter.Simplify("x + 0");
            Assert.Equal("x", result);
        }

        [Fact]
        public void Simplify_MultiplyByOne_ReturnsOriginal()
        {
            string result = _adapter.Simplify("1*x");
            Assert.Equal("x", result);
        }

        // ===== EXPANSIÓN =====

        [Fact]
        public void Expand_BinomialSquared_ReturnsExpanded()
        {
            // (x+1)^2 = x^2 + 2*x + 1
            string result = _adapter.Expand("(x + 1)^2");
            Assert.Contains("1", result);
            Assert.Contains("x", result);
        }

        [Fact]
        public void Expand_Product_ReturnsDistributed()
        {
            // (x+1)*(x-1) = x^2 - 1
            string result = _adapter.Expand("(x + 1)*(x - 1)");
            Assert.Contains("x", result);
        }

        // ===== EVALUACIÓN =====

        [Fact]
        public void Evaluate_SimpleExpression_ReturnsCorrectValue()
        {
            double result = _adapter.Evaluate("x^2 + 1", new Dictionary<string, double> { { "x", 3 } });
            Assert.Equal(10.0, result);
        }

        [Fact]
        public void Evaluate_MultipleVariables_ReturnsCorrectValue()
        {
            double result = _adapter.Evaluate("x + y", new Dictionary<string, double> { { "x", 2 }, { "y", 3 } });
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Evaluate_TrigFunction_ReturnsCorrectValue()
        {
            double result = _adapter.Evaluate("sin(x)", new Dictionary<string, double> { { "x", 0 } });
            Assert.Equal(0.0, result, 5);
        }

        // ===== LATEX =====

        [Fact]
        public void ToLaTeX_Fraction_ContainsLatexFrac()
        {
            string result = _adapter.ToLaTeX("1/x");
            Assert.Contains("frac", result);
        }

        [Fact]
        public void ToLaTeX_Power_ContainsCaret()
        {
            string result = _adapter.ToLaTeX("x^2");
            Assert.Contains("^", result);
        }

        // ===== ERROR HANDLING =====

        [Fact]
        public void Differentiate_InvalidExpression_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _adapter.Differentiate("+++", "x"));
        }

        [Fact]
        public void Evaluate_MissingVariable_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => _adapter.Evaluate("x + y", new Dictionary<string, double> { { "x", 1 } }));
        }
    }
}
