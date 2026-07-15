using System.Windows.Input;
using Calculadora.Commands;
using System.Collections.Generic;

namespace Calculadora.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private readonly Dictionary<string, BaseViewModel> _viewModels;

        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand NavigateCommand { get; }

        public NavigationViewModel()
        {
            // Initialize ViewModels to keep their state when navigating away
            _viewModels = new Dictionary<string, BaseViewModel>
            {
                { "Basic", new CalculatorViewModel() },
                { "Scientific", new ScientificViewModel() },
                { "Graph", new GraphPlotterViewModel() },
                { "Calculus", new CalculusViewModel() },
                { "Matrix", new MatrixViewModel() },
                { "History", new HistoryViewModel() }
            };

            NavigateCommand = new RelayCommand<string>(Navigate);

            // Set default view
            _currentView = _viewModels["Basic"];
        }

        private void Navigate(string? viewName)
        {
            if (viewName != null && _viewModels.TryGetValue(viewName, out var viewModel))
            {
                CurrentView = viewModel;
            }
        }
    }
}
