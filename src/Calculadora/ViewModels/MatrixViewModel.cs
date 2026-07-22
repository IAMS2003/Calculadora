using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    /// <summary>
    /// Representa una celda editable en la grilla de la matriz.
    /// </summary>
    public class MatrixCell : BaseViewModel
    {
        private string _value = "0";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public double NumericValue
        {
            get
            {
                if (double.TryParse(_value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                    return result;
                return 0;
            }
        }
    }

    public class MatrixViewModel : BaseViewModel
    {
        private int _rowsA = 2;
        private int _colsA = 2;
        private int _rowsB = 2;
        private int _colsB = 2;
        private string _resultText = "";
        private string _errorMessage = "";
        private string _selectedOperation = "Suma";

        public ObservableCollection<ObservableCollection<MatrixCell>> MatrixA { get; } = new();
        public ObservableCollection<ObservableCollection<MatrixCell>> MatrixB { get; } = new();

        public int RowsA
        {
            get => _rowsA;
            set { _rowsA = Math.Max(1, Math.Min(value, 6)); OnPropertyChanged(nameof(RowsA)); RebuildMatrix(MatrixA, _rowsA, _colsA); }
        }

        public int ColsA
        {
            get => _colsA;
            set { _colsA = Math.Max(1, Math.Min(value, 6)); OnPropertyChanged(nameof(ColsA)); RebuildMatrix(MatrixA, _rowsA, _colsA); }
        }

        public int RowsB
        {
            get => _rowsB;
            set { _rowsB = Math.Max(1, Math.Min(value, 6)); OnPropertyChanged(nameof(RowsB)); RebuildMatrix(MatrixB, _rowsB, _colsB); }
        }

        public int ColsB
        {
            get => _colsB;
            set { _colsB = Math.Max(1, Math.Min(value, 6)); OnPropertyChanged(nameof(ColsB)); RebuildMatrix(MatrixB, _rowsB, _colsB); }
        }

        public string SelectedOperation
        {
            get => _selectedOperation;
            set { _selectedOperation = value; OnPropertyChanged(nameof(SelectedOperation)); }
        }

        public string ResultText
        {
            get => _resultText;
            set { _resultText = value; OnPropertyChanged(nameof(ResultText)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ObservableCollection<string> Operations { get; } = new()
        {
            "Suma", "Resta", "Multiplicación", "Determinante A", "Inversa A", "Transpuesta A", "Traza A"
        };

        public ICommand CalculateCommand { get; }
        public ICommand ClearCommand { get; }

        public MatrixViewModel()
        {
            RebuildMatrix(MatrixA, _rowsA, _colsA);
            RebuildMatrix(MatrixB, _rowsB, _colsB);

            CalculateCommand = new RelayCommand<object>(ExecuteCalculate);
            ClearCommand = new RelayCommand<object>(ExecuteClear);
        }

        private void RebuildMatrix(ObservableCollection<ObservableCollection<MatrixCell>> matrix, int rows, int cols)
        {
            matrix.Clear();
            for (int i = 0; i < rows; i++)
            {
                var row = new ObservableCollection<MatrixCell>();
                for (int j = 0; j < cols; j++)
                    row.Add(new MatrixCell());
                matrix.Add(row);
            }
        }

        private MatrixModel ReadMatrix(ObservableCollection<ObservableCollection<MatrixCell>> cells)
        {
            int rows = cells.Count;
            int cols = cells[0].Count;
            var matrix = new MatrixModel(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = cells[i][j].NumericValue;
            return matrix;
        }

        private void ExecuteCalculate(object? parameter)
        {
            ErrorMessage = "";
            ResultText = "";

            try
            {
                var a = ReadMatrix(MatrixA);

                switch (SelectedOperation)
                {
                    case "Suma":
                        var bSum = ReadMatrix(MatrixB);
                        ResultText = (a + bSum).ToString();
                        break;
                    case "Resta":
                        var bSub = ReadMatrix(MatrixB);
                        ResultText = (a - bSub).ToString();
                        break;
                    case "Multiplicación":
                        var bMul = ReadMatrix(MatrixB);
                        ResultText = (a * bMul).ToString();
                        break;
                    case "Determinante A":
                        double det = a.Determinant();
                        ResultText = $"det(A) = {det.ToString("G6", CultureInfo.InvariantCulture)}";
                        break;
                    case "Inversa A":
                        ResultText = a.Inverse().ToString();
                        break;
                    case "Transpuesta A":
                        ResultText = a.Transpose().ToString();
                        break;
                    case "Traza A":
                        double trace = a.Trace();
                        ResultText = $"tr(A) = {trace.ToString("G6", CultureInfo.InvariantCulture)}";
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void ExecuteClear(object? parameter)
        {
            RebuildMatrix(MatrixA, _rowsA, _colsA);
            RebuildMatrix(MatrixB, _rowsB, _colsB);
            ResultText = "";
            ErrorMessage = "";
        }
    }
}
