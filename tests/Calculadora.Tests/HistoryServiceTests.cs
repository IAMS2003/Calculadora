using System.Linq;
using Xunit;
using Calculadora.Services;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Calculadora.Tests
{
    [Collection("Sequential")]
    public class HistoryServiceTests
    {
        [Fact]
        public void Add_ValidEntry_AddsToList()
        {
            HistoryService.Instance.Clear();
            HistoryService.Instance.Add("Test", "2 + 2", "4");

            Assert.Single(HistoryService.Instance.Entries);
            var entry = HistoryService.Instance.Entries.First();
            Assert.Equal("Test", entry.Module);
            Assert.Equal("2 + 2", entry.Expression);
            Assert.Equal("4", entry.Result);
        }

        [Fact]
        public void Add_MultipleEntries_MostRecentFirst()
        {
            HistoryService.Instance.Clear();
            HistoryService.Instance.Add("Básica", "1 + 1", "2");
            HistoryService.Instance.Add("Científica", "sin(90)", "1");

            Assert.Equal(2, HistoryService.Instance.Entries.Count);
            Assert.Equal("Científica", HistoryService.Instance.Entries[0].Module);
            Assert.Equal("Básica", HistoryService.Instance.Entries[1].Module);
        }

        [Fact]
        public void Add_EmptyOrNull_Ignored()
        {
            HistoryService.Instance.Clear();
            HistoryService.Instance.Add("Test", "", "4");
            HistoryService.Instance.Add("Test", "2 + 2", "");

            Assert.Empty(HistoryService.Instance.Entries);
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            HistoryService.Instance.Clear();
            HistoryService.Instance.Add("Test", "2 + 2", "4");
            HistoryService.Instance.Clear();

            Assert.Empty(HistoryService.Instance.Entries);
        }
    }
}
