using System.Windows.Controls;
using System.Windows.Input;
using Calculadora.ViewModels;

namespace Calculadora.Views
{
    public partial class BasicCalculatorView : UserControl
    {
        public BasicCalculatorView()
        {
            InitializeComponent();
            // To ensure it receives keyboard events, make it focusable
            this.Focusable = true;
            this.Loaded += (s, e) => this.Focus();
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            var viewModel = DataContext as CalculatorViewModel;
            if (viewModel == null || string.IsNullOrEmpty(e.Text)) return;

            string input = e.Text;

            // Mapear números y punto decimal
            if (char.IsDigit(input[0]) || input == ".")
            {
                if (viewModel.NumberInputCommand.CanExecute(input))
                {
                    viewModel.NumberInputCommand.Execute(input);
                    e.Handled = true;
                }
            }
            // Mapear operadores estándar
            else if (input == "+" || input == "-" || input == "*" || input == "/" || input == "%")
            {
                if (viewModel.OperationCommand.CanExecute(input))
                {
                    viewModel.OperationCommand.Execute(input);
                    e.Handled = true;
                }
            }
            // Mapear operadores alternativos (como 'x' para multiplicación o '÷' para división)
            else if (input == "x" || input == "X" || input == "×")
            {
                if (viewModel.OperationCommand.CanExecute("*"))
                {
                    viewModel.OperationCommand.Execute("*");
                    e.Handled = true;
                }
            }
            else if (input == "÷")
            {
                if (viewModel.OperationCommand.CanExecute("/"))
                {
                    viewModel.OperationCommand.Execute("/");
                    e.Handled = true;
                }
            }
            // Mapear el signo de igual
            else if (input == "=")
            {
                if (viewModel.EqualsCommand.CanExecute(null))
                {
                    viewModel.EqualsCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var viewModel = DataContext as CalculatorViewModel;
            if (viewModel == null) return;

            switch (e.Key)
            {
                case Key.Escape:
                case Key.Back: // Backspace y Esc limpian la calculadora (C)
                    if (viewModel.ClearCommand.CanExecute(null))
                    {
                        viewModel.ClearCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case Key.Enter:
                    if (viewModel.EqualsCommand.CanExecute(null))
                    {
                        viewModel.EqualsCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }
    }
}
