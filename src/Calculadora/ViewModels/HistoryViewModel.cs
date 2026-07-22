using System.Collections.ObjectModel;
using System.Windows.Input;
using Calculadora.Commands;
using Calculadora.Services;

namespace Calculadora.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        public ObservableCollection<HistoryEntry> Entries => HistoryService.Instance.Entries;

        public ICommand ClearHistoryCommand { get; }
        public ICommand CopyExpressionCommand { get; }

        public HistoryViewModel()
        {
            ClearHistoryCommand = new RelayCommand<object>(ExecuteClearHistory);
            CopyExpressionCommand = new RelayCommand<HistoryEntry>(ExecuteCopyExpression);
        }

        private void ExecuteClearHistory(object? parameter)
        {
            HistoryService.Instance.Clear();
        }

        private void ExecuteCopyExpression(HistoryEntry? entry)
        {
            if (entry != null)
            {
                System.Windows.Clipboard.SetText(entry.Expression);
            }
        }
    }
}
