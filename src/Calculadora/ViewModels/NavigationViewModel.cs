using System.Windows.Input;
using Calculadora.Commands;
using System.Collections.Generic;

namespace Calculadora.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private readonly Dictionary<string, BaseViewModel> _viewModels;
        private bool _isSidebarCollapsed;
        private string _activeMode = "Basic";

        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set
            {
                _isSidebarCollapsed = value;
                OnPropertyChanged(nameof(IsSidebarCollapsed));
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        public double SidebarWidth => IsSidebarCollapsed ? 60 : 220;

        public string ActiveMode
        {
            get => _activeMode;
            set
            {
                _activeMode = value;
                OnPropertyChanged(nameof(ActiveMode));
            }
        }

        public ICommand NavigateCommand { get; }
        public ICommand ToggleSidebarCommand { get; }

        public NavigationViewModel()
        {
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
            ToggleSidebarCommand = new RelayCommand<object>(_ => IsSidebarCollapsed = !IsSidebarCollapsed);

            _currentView = _viewModels["Basic"];
        }

        private void Navigate(string? viewName)
        {
            if (viewName != null && _viewModels.TryGetValue(viewName, out var viewModel))
            {
                CurrentView = viewModel;
                ActiveMode = viewName;
            }
        }
    }
}
