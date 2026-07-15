using System.Windows;
using Xunit;
using Calculadora.Models;

namespace Calculadora.Tests
{
    public class CoordinateTransformerTests
    {
        [Fact]
        public void MathToScreen_CenterZero_ReturnsScreenCenter()
        {
            var transformer = new CoordinateTransformer(800, 600);
            
            // At math origin (0,0), it should map to center of screen (400,300)
            Point p = transformer.MathToScreen(0, 0);
            
            Assert.Equal(400, p.X);
            Assert.Equal(300, p.Y);
        }

        [Fact]
        public void ScreenToMath_ScreenCenter_ReturnsZero()
        {
            var transformer = new CoordinateTransformer(800, 600);
            
            Point p = transformer.ScreenToMath(400, 300);
            
            Assert.Equal(0, p.X);
            Assert.Equal(0, p.Y);
        }

        [Fact]
        public void MathToScreen_PositiveX_MovesRightInScreen()
        {
            var transformer = new CoordinateTransformer(800, 600);
            transformer.Scale = 50; // 50 pixels per unit
            
            Point p = transformer.MathToScreen(1, 0);
            
            // X should be 400 + 50 = 450
            // Y should still be 300
            Assert.Equal(450, p.X);
            Assert.Equal(300, p.Y);
        }

        [Fact]
        public void MathToScreen_PositiveY_MovesUpInScreen()
        {
            var transformer = new CoordinateTransformer(800, 600);
            transformer.Scale = 50; 
            
            Point p = transformer.MathToScreen(0, 1);
            
            // X should be 400
            // Y should be 300 - 50 = 250 (Screen Y goes down)
            Assert.Equal(400, p.X);
            Assert.Equal(250, p.Y);
        }

        [Fact]
        public void Pan_UpdatesCenterCorrectly()
        {
            var transformer = new CoordinateTransformer(800, 600);
            transformer.Scale = 100;
            
            // Pan screen 100 pixels to the right
            transformer.Pan(100, 0);
            
            // This means we are looking at Math X = -1 in the center now
            Assert.Equal(-1, transformer.CenterX);
            Assert.Equal(0, transformer.CenterY);
        }
    }
}
