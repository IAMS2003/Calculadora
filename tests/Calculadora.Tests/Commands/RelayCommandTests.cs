using System;
using Calculadora.Commands;
using Xunit;

namespace Calculadora.Tests.Commands
{
    public class RelayCommandTests
    {
        [Fact]
        public void Execute_CallsAction()
        {
            // Arrange
            bool executed = false;
            var command = new RelayCommand(() => executed = true);

            // Act
            command.Execute(null);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_ReturnsTrue_WhenNoPredicateProvided()
        {
            // Arrange
            var command = new RelayCommand(() => { });

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanExecute_ReturnsPredicateValue(bool predicateResult)
        {
            // Arrange
            var command = new RelayCommand(() => { }, () => predicateResult);

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.Equal(predicateResult, canExecute);
        }

        [Fact]
        public void Execute_Generic_CallsActionWithCorrectParameter()
        {
            // Arrange
            string? receivedParameter = null;
            var command = new RelayCommand<string>(param => receivedParameter = param);

            // Act
            command.Execute("TestParam");

            // Assert
            Assert.Equal("TestParam", receivedParameter);
        }

        [Theory]
        [InlineData("Valid", true)]
        [InlineData("Invalid", false)]
        public void CanExecute_Generic_ReturnsPredicateValueWithParameter(string parameter, bool expectedResult)
        {
            // Arrange
            var command = new RelayCommand<string>(
                param => { },
                param => param == "Valid"
            );

            // Act
            bool canExecute = command.CanExecute(parameter);

            // Assert
            Assert.Equal(expectedResult, canExecute);
        }

        [Fact]
        public void Execute_GenericValueType_WithNullParameter_PassesDefaultValue()
        {
            // Arrange
            int receivedParameter = -999;
            var command = new RelayCommand<int>(param => receivedParameter = param);

            // Act
            command.Execute(null);

            // Assert
            Assert.Equal(0, receivedParameter); // default(int) is 0
        }

        [Fact]
        public void CanExecute_GenericValueType_WithNullParameter_PassesDefaultValue()
        {
            // Arrange
            int? receivedParameter = null;
            var command = new RelayCommand<int>(
                param => { },
                param =>
                {
                    receivedParameter = param;
                    return true;
                }
            );

            // Act
            command.CanExecute(null);

            // Assert
            Assert.Equal(0, receivedParameter); // default(int) is 0
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenActionIsNull()
        {
            // Assert & Act
            Assert.Throws<ArgumentNullException>(() => new RelayCommand(null!));
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(null!));
        }
    }
}
