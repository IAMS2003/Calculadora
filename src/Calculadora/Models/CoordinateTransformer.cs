using System.Windows;

namespace Calculadora.Models
{
    public class CoordinateTransformer
    {
        public double CenterX { get; set; } = 0;
        public double CenterY { get; set; } = 0;
        public double Scale { get; set; } = 50; // pixels per unit
        
        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }

        public CoordinateTransformer(double screenWidth, double screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }

        public Point MathToScreen(double mathX, double mathY)
        {
            double screenX = (mathX - CenterX) * Scale + (ScreenWidth / 2);
            double screenY = (ScreenHeight / 2) - (mathY - CenterY) * Scale;
            return new Point(screenX, screenY);
        }

        public Point ScreenToMath(double screenX, double screenY)
        {
            double mathX = (screenX - (ScreenWidth / 2)) / Scale + CenterX;
            double mathY = ((ScreenHeight / 2) - screenY) / Scale + CenterY;
            return new Point(mathX, mathY);
        }

        public void Pan(double screenDeltaX, double screenDeltaY)
        {
            // Delta represents how much the MOUSE moved in screen space.
            // If mouse moves right (positive DeltaX), the world should move right,
            // meaning the CenterX decreases.
            CenterX -= screenDeltaX / Scale;
            CenterY += screenDeltaY / Scale;
        }

        public void Zoom(double factor, double mouseScreenX, double mouseScreenY)
        {
            // Calculate where the mouse is in math coordinates BEFORE zoom
            Point mathBefore = ScreenToMath(mouseScreenX, mouseScreenY);
            
            // Apply zoom
            Scale *= factor;
            
            // Calculate where that same math coordinate would be AFTER zoom,
            // if we didn't adjust the center
            Point mathAfter = ScreenToMath(mouseScreenX, mouseScreenY);
            
            // Adjust the center so the point under the mouse stays exactly the same
            CenterX += mathBefore.X - mathAfter.X;
            CenterY += mathBefore.Y - mathAfter.Y;
        }
        
        // Helper to find the bounds of the current viewport
        public (double minX, double maxX, double minY, double maxY) GetMathBounds()
        {
            Point topLeft = ScreenToMath(0, 0);
            Point bottomRight = ScreenToMath(ScreenWidth, ScreenHeight);
            
            return (topLeft.X, bottomRight.X, bottomRight.Y, topLeft.Y);
        }
    }
}
