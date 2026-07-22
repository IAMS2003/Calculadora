using System.Windows.Media;
using Calculadora.ViewModels;

namespace Calculadora.Models
{
    public class FunctionItem : BaseViewModel
    {
        private string _expression = "";
        private string _colorHex = "#00FFFF"; // Default Cyan

        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnPropertyChanged(nameof(Expression));
            }
        }

        public string ColorHex
        {
            get => _colorHex;
            set
            {
                _colorHex = value;
                OnPropertyChanged(nameof(ColorHex));
                OnPropertyChanged(nameof(Brush));
            }
        }

        public Brush Brush
        {
            get
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(ColorHex);
                    var brush = new SolidColorBrush(color);
                    brush.Freeze();
                    return brush;
                }
                catch
                {
                    return Brushes.Cyan;
                }
            }
        }

        public FunctionItem(string expression, string colorHex = "#00FFFF")
        {
            _expression = expression;
            _colorHex = colorHex;
        }
    }
}
