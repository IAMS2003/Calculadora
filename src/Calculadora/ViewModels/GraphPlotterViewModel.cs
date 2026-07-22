using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;
using Calculadora.Services;

namespace Calculadora.ViewModels
{
    public class GraphPlotterViewModel : BaseViewModel
    {
        public ObservableCollection<FunctionItem> Functions { get; } = new ObservableCollection<FunctionItem>();

        private static readonly string[] PresetColors = new[]
        {
            "#00FFFF", // Cyan
            "#FF00FF", // Magenta
            "#FFFF00", // Yellow
            "#30D158", // Green
            "#FF9F0A", // Orange
            "#FF453A", // Red
            "#BF5AF2", // Purple
            "#FFFFFF"  // White
        };

        public IEnumerable<string> AvailableColors => PresetColors;

        private string _newExpression = "";
        private string _selectedColor = PresetColors[0];

        public string NewExpression
        {
            get => _newExpression;
            set
            {
                _newExpression = value;
                OnPropertyChanged(nameof(NewExpression));
            }
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }

        public ICommand AddExpressionCommand { get; }
        public ICommand RemoveExpressionCommand { get; }
        public ICommand ChangeColorCommand { get; }

        public GraphPlotterViewModel()
        {
            Functions.Add(new FunctionItem("sin(x)", PresetColors[0]));
            
            AddExpressionCommand = new RelayCommand<object>(ExecuteAddExpression, CanExecuteAddExpression);
            RemoveExpressionCommand = new RelayCommand<FunctionItem>(ExecuteRemoveExpression);
            ChangeColorCommand = new RelayCommand<FunctionItem>(ExecuteChangeColor);

            EventAggregator.Instance.SendToGraphRequested += OnSendToGraphRequested;
        }

        private void OnSendToGraphRequested(string expression)
        {
            if (Functions.Count < 6 && !Functions.Any(f => f.Expression == expression))
            {
                string nextColor = PresetColors[Functions.Count % PresetColors.Length];
                Functions.Add(new FunctionItem(expression, nextColor));
            }
        }

        private bool CanExecuteAddExpression(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NewExpression) && Functions.Count < 6;
        }

        private void ExecuteAddExpression(object? parameter)
        {
            string color = SelectedColor;
            Functions.Add(new FunctionItem(NewExpression, color));
            NewExpression = "";

            // Rotate default color for next addition
            SelectedColor = PresetColors[Functions.Count % PresetColors.Length];
        }

        private void ExecuteRemoveExpression(FunctionItem? item)
        {
            if (item != null && Functions.Contains(item))
            {
                Functions.Remove(item);
            }
        }

        private void ExecuteChangeColor(FunctionItem? item)
        {
            if (item == null) return;
            int currentIndex = System.Array.IndexOf(PresetColors, item.ColorHex);
            int nextIndex = (currentIndex + 1) % PresetColors.Length;
            item.ColorHex = PresetColors[nextIndex];
        }
    }
}
