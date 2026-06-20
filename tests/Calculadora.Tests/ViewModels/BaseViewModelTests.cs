using System;
using Calculadora.ViewModels;
using Xunit;

namespace Calculadora.Tests.ViewModels
{
    public class BaseViewModelTests
    {
        private class TestViewModel : BaseViewModel
        {
            private string _name = string.Empty;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            public bool SetNameWithCallback(string name, Action callback)
            {
                var temp = _name;
                return SetProperty(ref _name, name, nameof(Name), callback);
            }

            public void RaiseManual(string propertyName)
            {
                OnPropertyChanged(propertyName);
            }
        }

        [Fact]
        public void SetProperty_WhenValueChanges_UpdatesBackingFieldAndRaisesEvent()
        {
            // Arrange
            var vm = new TestViewModel();
            string? raisedPropertyName = null;
            vm.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

            // Act
            vm.Name = "New Name";

            // Assert
            Assert.Equal("New Name", vm.Name);
            Assert.Equal("Name", raisedPropertyName);
        }

        [Fact]
        public void SetProperty_WhenValueIsSame_DoesNotRaiseEventOrChangeValue()
        {
            // Arrange
            var vm = new TestViewModel { Name = "Same" };
            bool eventRaised = false;
            vm.PropertyChanged += (sender, e) => eventRaised = true;

            // Act
            vm.Name = "Same";

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void SetProperty_InvokesOnChangedCallback_WhenValueChanges()
        {
            // Arrange
            var vm = new TestViewModel();
            bool callbackInvoked = false;

            // Act
            vm.SetNameWithCallback("New Name", () => callbackInvoked = true);

            // Assert
            Assert.True(callbackInvoked);
        }

        [Fact]
        public void OnPropertyChanged_RaisesEventWithGivenName()
        {
            // Arrange
            var vm = new TestViewModel();
            string? raisedPropertyName = null;
            vm.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

            // Act
            vm.RaiseManual("CustomProperty");

            // Assert
            Assert.Equal("CustomProperty", raisedPropertyName);
        }
    }
}
