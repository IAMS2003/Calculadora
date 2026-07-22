using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class FunctionItemTests
    {
        [Fact]
        public void FunctionItem_Initialization_SetsExpressionAndColor()
        {
            var item = new FunctionItem("sin(x)", "#FF00FF");
            Assert.Equal("sin(x)", item.Expression);
            Assert.Equal("#FF00FF", item.ColorHex);
            Assert.NotNull(item.Brush);
        }

        [Fact]
        public void FunctionItem_ColorHexChange_UpdatesBrush()
        {
            var item = new FunctionItem("x^2", "#00FFFF");
            item.ColorHex = "#FFFF00";
            Assert.Equal("#FFFF00", item.ColorHex);
            Assert.NotNull(item.Brush);
        }

        [Fact]
        public void FunctionItem_InvalidColorHex_FallbackToCyan()
        {
            var item = new FunctionItem("x^2", "InvalidColor");
            Assert.NotNull(item.Brush);
        }
    }
}
