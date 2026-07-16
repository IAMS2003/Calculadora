using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Calculadora.Models;

namespace Calculadora.Controls
{
    public class GraphPlotter : FrameworkElement
    {
        private CoordinateTransformer _transformer = new CoordinateTransformer(0, 0);
        
        private Point _lastMousePosition;
        private bool _isDragging = false;
        
        private static readonly Pen AxisPen = new Pen(Brushes.White, 1);
        private static readonly Pen GridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1);
        
        // Colores para diferentes funciones
        private static readonly Brush[] FunctionBrushes = new Brush[] 
        { 
            Brushes.Cyan, 
            Brushes.Magenta, 
            Brushes.Yellow, 
            Brushes.Lime, 
            Brushes.Orange 
        };

        public static readonly DependencyProperty ExpressionsProperty =
            DependencyProperty.Register("Expressions", typeof(IEnumerable<string>), typeof(GraphPlotter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnExpressionsChanged));

        private static void OnExpressionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var plotter = (GraphPlotter)d;

            // Desuscribirse de la colección anterior
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= plotter.OnCollectionChanged;
            }

            // Suscribirse a la nueva colección
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += plotter.OnCollectionChanged;
            }

            plotter.InvalidateVisual();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // La colección cambió internamente (Add/Remove), redibujar
            InvalidateVisual();
        }

        public IEnumerable<string> Expressions
        {
            get { return (IEnumerable<string>)GetValue(ExpressionsProperty); }
            set { SetValue(ExpressionsProperty, value); }
        }

        public GraphPlotter()
        {
            this.ClipToBounds = true;
            this.Focusable = true;
            
            // Suscribirse a eventos de mouse para interactividad
            this.MouseWheel += OnMouseWheel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _transformer.ScreenWidth = this.ActualWidth;
            _transformer.ScreenHeight = this.ActualHeight;
            InvalidateVisual();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
            Point mousePos = e.GetPosition(this);
            
            _transformer.Zoom(zoomFactor, mousePos.X, mousePos.Y);
            InvalidateVisual();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
                this.CaptureMouse();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPos = e.GetPosition(this);
                double deltaX = currentPos.X - _lastMousePosition.X;
                double deltaY = currentPos.Y - _lastMousePosition.Y;
                
                _transformer.Pan(deltaX, deltaY);
                _lastMousePosition = currentPos;
                
                InvalidateVisual();
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            this.ReleaseMouseCapture();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            if (this.ActualWidth == 0 || this.ActualHeight == 0) return;
            
            // Fondo oscuro
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(28, 28, 28)), null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            DrawGrid(dc);
            DrawAxes(dc);
            DrawFunctions(dc);
        }

        private void DrawGrid(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();
            
            // Calcular el paso (step) de la cuadrícula basado en el zoom (Scale)
            // Queremos aproximadamente líneas cada 50 pixeles
            double rawStep = 50.0 / _transformer.Scale;
            // Redondear a la potencia de 10 más cercana o múltiplos de 2/5 para pasos bonitos
            double log = Math.Log10(rawStep);
            double pow10 = Math.Pow(10, Math.Floor(log));
            double mantissa = rawStep / pow10;
            
            double step;
            if (mantissa < 2) step = 1 * pow10;
            else if (mantissa < 5) step = 2 * pow10;
            else step = 5 * pow10;

            // Dibujar lineas verticales (X)
            double startX = Math.Floor(bounds.minX / step) * step;
            for (double x = startX; x <= bounds.maxX; x += step)
            {
                if (Math.Abs(x) < 0.0001) continue; // Saltamos el eje central que se dibuja después
                Point p1 = _transformer.MathToScreen(x, bounds.minY);
                Point p2 = _transformer.MathToScreen(x, bounds.maxY);
                dc.DrawLine(GridPen, p1, p2);
                
                // Etiquetas
                DrawText(dc, FormatNumber(x), new Point(p1.X + 2, _transformer.MathToScreen(0, 0).Y + 2));
            }

            // Dibujar lineas horizontales (Y)
            double startY = Math.Floor(bounds.minY / step) * step;
            for (double y = startY; y <= bounds.maxY; y += step)
            {
                if (Math.Abs(y) < 0.0001) continue;
                Point p1 = _transformer.MathToScreen(bounds.minX, y);
                Point p2 = _transformer.MathToScreen(bounds.maxX, y);
                dc.DrawLine(GridPen, p1, p2);
                
                DrawText(dc, FormatNumber(y), new Point(_transformer.MathToScreen(0, 0).X + 2, p1.Y - 15));
            }
        }

        private string FormatNumber(double n)
        {
            if (Math.Abs(n) >= 10000 || (Math.Abs(n) < 0.001 && n != 0))
                return n.ToString("0.##E0", CultureInfo.InvariantCulture);
            return n.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void DrawText(DrawingContext dc, string text, Point origin)
        {
            FormattedText ft = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                10,
                Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            
            dc.DrawText(ft, origin);
        }

        private void DrawAxes(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();
            
            // Eje X
            Point xStart = _transformer.MathToScreen(bounds.minX, 0);
            Point xEnd = _transformer.MathToScreen(bounds.maxX, 0);
            dc.DrawLine(AxisPen, xStart, xEnd);

            // Eje Y
            Point yStart = _transformer.MathToScreen(0, bounds.minY);
            Point yEnd = _transformer.MathToScreen(0, bounds.maxY);
            dc.DrawLine(AxisPen, yStart, yEnd);
        }

        private void DrawFunctions(DrawingContext dc)
        {
            if (Expressions == null) return;

            var bounds = _transformer.GetMathBounds();
            int index = 0;
            
            foreach (var expr in Expressions)
            {
                if (string.IsNullOrWhiteSpace(expr)) continue;
                
                Brush brush = FunctionBrushes[index % FunctionBrushes.Length];
                Pen funcPen = new Pen(brush, 2);
                
                DrawSingleFunction(dc, funcPen, expr, bounds);
                index++;
            }
        }

        private void DrawSingleFunction(DrawingContext dc, Pen pen, string expression, (double minX, double maxX, double minY, double maxY) bounds)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                bool isFirstPoint = true;
                bool wasValid = false;
                
                // Muestreo adaptativo básico: 1 pixel en pantalla
                double stepX = 1.0 / _transformer.Scale;
                double prevY = 0;

                for (double mathX = bounds.minX; mathX <= bounds.maxX; mathX += stepX)
                {
                    try
                    {
                        // TODO: Optimize this. Instantiating a parser per pixel is very slow.
                        // We will inject the X value by replacing "x" with the number.
                        // This is a naive implementation just for MVP.
                        string injected = expression.Replace("x", $"({mathX.ToString(CultureInfo.InvariantCulture)})");
                        var parser = new ExpressionParser(injected, false);
                        double mathY = parser.Parse();

                        if (double.IsNaN(mathY) || double.IsInfinity(mathY))
                        {
                            wasValid = false;
                            continue;
                        }

                        // Detección heurística de asíntotas
                        if (wasValid && Math.Abs(mathY - prevY) > (bounds.maxY - bounds.minY))
                        {
                            // Salto demasiado grande, probablemente una discontinuidad (ej. tan(x))
                            wasValid = false;
                        }

                        Point screenP = _transformer.MathToScreen(mathX, mathY);

                        if (!wasValid || isFirstPoint)
                        {
                            ctx.BeginFigure(screenP, false, false);
                            isFirstPoint = false;
                        }
                        else
                        {
                            ctx.LineTo(screenP, true, true);
                        }

                        wasValid = true;
                        prevY = mathY;
                    }
                    catch
                    {
                        wasValid = false;
                    }
                }
            }
            
            geometry.Freeze();
            dc.DrawGeometry(null, pen, geometry);
        }
    }
}
