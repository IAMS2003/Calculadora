using Calculadora.ViewModels;
using Xunit;

namespace Calculadora.Tests.ViewModels
{
    public class CalculatorViewModelTests
    {
        [Fact]
        public void Constructor_InitializesDisplayTextToZero()
        {
            // Act
            var viewModel = new CalculatorViewModel();

            // Assert
            Assert.Equal("0", viewModel.DisplayText);
        }

        [Fact]
        public void NumberInput_AppendsDigitsCorrectly()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("5");
            vm.NumberInputCommand.Execute("3");
            vm.NumberInputCommand.Execute("9");

            // Assert
            Assert.Equal("539", vm.DisplayText);
        }

        [Fact]
        public void NumberInput_IgnoresDuplicateDecimalPoint()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("5");
            vm.NumberInputCommand.Execute(".");
            vm.NumberInputCommand.Execute("2");
            vm.NumberInputCommand.Execute(".");
            vm.NumberInputCommand.Execute("3");

            // Assert
            Assert.Equal("5.23", vm.DisplayText);
        }

        [Fact]
        public void NumberInput_DecimalPointOnEmpty_StartsAsZeroDot()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute(".");

            // Assert
            Assert.Equal("0.", vm.DisplayText);
        }

        [Fact]
        public void NumberInput_ReplacesInitialLeadingZero()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("0");
            vm.NumberInputCommand.Execute("0");
            vm.NumberInputCommand.Execute("7");

            // Assert
            Assert.Equal("7", vm.DisplayText);
        }

        [Fact]
        public void NumberInput_LimitsToMaximum15Digits()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act - input 16 digits
            for (int i = 0; i < 16; i++)
            {
                vm.NumberInputCommand.Execute("1");
            }

            // Assert
            Assert.Equal("111111111111111", vm.DisplayText);
            Assert.Equal(15, vm.DisplayText.Length);
        }

        [Fact]
        public void NumberInput_LimitsTo15Digits_ExcludingDecimalPoint()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act - input "1.1111..." (1 decimal point, 15 digits total)
            vm.NumberInputCommand.Execute("1");
            vm.NumberInputCommand.Execute(".");
            for (int i = 0; i < 15; i++)
            {
                vm.NumberInputCommand.Execute("2");
            }

            // Assert
            // Total length is 17: "1" (1 digit) + "." + 14 "2"s (14 digits) = 15 digits total
            Assert.Equal("1.22222222222222", vm.DisplayText);
            int digitCount = 0;
            foreach (char c in vm.DisplayText)
            {
                if (char.IsDigit(c)) digitCount++;
            }
            Assert.Equal(15, digitCount);
        }

        [Fact]
        public void OperationInput_SetsOperatorAndKeepsDisplay()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");

            // Assert
            Assert.Equal("5", vm.DisplayText);
        }

        [Fact]
        public void OperationInput_ChainsOperations_AndShowsIntermediateResult()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 5 + 3 * 2 (WPF chains from left to right, so 5 + 3 = 8, then 8 * 2)
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("3");
            vm.OperationCommand.Execute("*"); // Triggers calculation: 5 + 3 = 8

            // Assert
            Assert.Equal("8", vm.DisplayText);
        }

        [Fact]
        public void OperationInput_StandalonePercentage_DividesBy100()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("1");
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("%");

            // Assert
            Assert.Equal("0.15", vm.DisplayText);
        }

        [Fact]
        public void OperationInput_ContextualPercentage_CalculatesRelativeValue()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 200 + 10%
            vm.NumberInputCommand.Execute("2");
            vm.NumberInputCommand.Execute("0");
            vm.NumberInputCommand.Execute("0");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("1");
            vm.NumberInputCommand.Execute("0");
            vm.OperationCommand.Execute("%"); // Calculates 10% of 200 = 20

            // Assert
            Assert.Equal("20", vm.DisplayText);
        }

        [Fact]
        public void Equals_WithoutOperation_IsNoOp()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act
            vm.NumberInputCommand.Execute("5");
            vm.EqualsCommand.Execute(null);

            // Assert
            Assert.Equal("5", vm.DisplayText);
        }

        [Fact]
        public void Equals_ExecutesCalculationCorrectly()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 5 + 3 = 8
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("3");
            vm.EqualsCommand.Execute(null);

            // Assert
            Assert.Equal("8", vm.DisplayText);
        }

        [Fact]
        public void Equals_DivideByZero_DisplaysError()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 5 / 0 = Error
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("/");
            vm.NumberInputCommand.Execute("0");
            vm.EqualsCommand.Execute(null);

            // Assert
            Assert.Equal("Error", vm.DisplayText);
        }

        [Fact]
        public void Equals_ResultDisplayed_NewNumberStartsNewCalculation()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 5 + 3 = 8, then press 7
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("3");
            vm.EqualsCommand.Execute(null);
            vm.NumberInputCommand.Execute("7");

            // Assert
            Assert.Equal("7", vm.DisplayText);
        }

        [Fact]
        public void Equals_ResultDisplayed_NewOperatorChains()
        {
            // Arrange
            var vm = new CalculatorViewModel();

            // Act: 5 + 3 = 8, then press * 2 = 16
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("3");
            vm.EqualsCommand.Execute(null); // Displays 8
            vm.OperationCommand.Execute("*"); // Sets _firstOperand to 8, wait for next operand
            vm.NumberInputCommand.Execute("2");
            vm.EqualsCommand.Execute(null);

            // Assert
            Assert.Equal("16", vm.DisplayText);
        }

        [Fact]
        public void Clear_FromErrorState_ResetsToZero()
        {
            // Arrange
            var vm = new CalculatorViewModel();
            // Trigger error: 5 / 0 = Error
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("/");
            vm.NumberInputCommand.Execute("0");
            vm.EqualsCommand.Execute(null);
            Assert.Equal("Error", vm.DisplayText);

            // Act
            vm.ClearCommand.Execute(null);

            // Assert
            Assert.Equal("0", vm.DisplayText);
        }

        [Fact]
        public void Clear_FromCalculationState_ResetsToZero()
        {
            // Arrange
            var vm = new CalculatorViewModel();
            vm.NumberInputCommand.Execute("5");
            vm.OperationCommand.Execute("+");
            vm.NumberInputCommand.Execute("3");

            // Act
            vm.ClearCommand.Execute(null);

            // Assert
            Assert.Equal("0", vm.DisplayText);
        }
    }
}
