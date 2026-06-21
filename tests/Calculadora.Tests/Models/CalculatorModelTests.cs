using Calculadora.Models;
using Xunit;

namespace Calculadora.Tests.Models
{
    public class CalculatorModelTests
    {
        private readonly CalculatorModel _model = new CalculatorModel();

        [Theory]
        [InlineData(2, 3, 5)]
        [InlineData(-2, -3, -5)]
        [InlineData(1.5, 2.5, 4.0)]
        [InlineData(0, 5, 5)]
        [InlineData(5, 0, 5)]
        [InlineData(-5, 5, 0)]
        public void Add_ReturnsCorrectSum(double a, double b, double expected)
        {
            // Act
            double result = _model.Add(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(-5, -3, -2)]
        [InlineData(5, -3, 8)]
        [InlineData(1.5, 0.5, 1.0)]
        [InlineData(0, 5, -5)]
        [InlineData(5, 5, 0)]
        public void Subtract_ReturnsCorrectDifference(double a, double b, double expected)
        {
            // Act
            double result = _model.Subtract(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2, 3, 6)]
        [InlineData(-2, -3, 6)]
        [InlineData(-2, 3, -6)]
        [InlineData(1.5, 2, 3.0)]
        [InlineData(5, 0, 0)]
        [InlineData(0, 5, 0)]
        [InlineData(1, 5, 5)]
        public void Multiply_ReturnsCorrectProduct(double a, double b, double expected)
        {
            // Act
            double result = _model.Multiply(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(6, 2, 3.0)]
        [InlineData(-6, 2, -3.0)]
        [InlineData(1.5, 2, 0.75)]
        [InlineData(0, 5, 0.0)]
        public void Divide_ReturnsCorrectQuotient(double a, double b, double expected)
        {
            // Act
            double result = _model.Divide(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Divide_ByZero_ThrowsDivideByZeroException()
        {
            // Assert & Act
            Assert.Throws<DivideByZeroException>(() => _model.Divide(5, 0));
        }

        [Theory]
        [InlineData(200, 10, 20.0)]
        [InlineData(200, 12.5, 25.0)]
        [InlineData(0, 10, 0.0)]
        [InlineData(200, 0, 0.0)]
        [InlineData(100, 33.33, 33.33)]
        [InlineData(-200, 10, -20.0)]
        public void Percentage_ReturnsCorrectPercentage(double value, double percentage, double expected)
        {
            // Act
            double result = _model.Percentage(value, percentage);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10, 5, OperationType.Add, 15.0)]
        [InlineData(10, 5, OperationType.Subtract, 5.0)]
        [InlineData(10, 5, OperationType.Multiply, 50.0)]
        [InlineData(10, 5, OperationType.Divide, 2.0)]
        [InlineData(200, 10, OperationType.Percentage, 20.0)]
        public void Calculate_DispatchesCorrectly(double a, double b, OperationType op, double expected)
        {
            // Act
            double result = _model.Calculate(a, b, op);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculate_WithInvalidOperation_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var invalidOp = (OperationType)999;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _model.Calculate(10, 5, invalidOp));
        }
    }
}
