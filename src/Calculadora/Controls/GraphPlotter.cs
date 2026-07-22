using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
        private Point _hoverPoint;
        private bool _isDragging = false;
        private bool _hasHover = false;
        
        private static readonly Pen AxisPen = new Pen(Brushes.White, 1);
        private static readonly Pen GridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1);
        private static readonly Pen TangentPen = new Pen(Brushes.Yellow, 1.5) { DashStyle = DashStyles.Dash };
        
        private static readonly Brush[] FunctionBrushes = new Brush[] 
        { 
            Brushes.Cyan, 
            Brushes.Magenta, 
            Brushes.Yellow, 
            Brushes.Lime, 
            Brushes.Orange,
            Brushes.Red,
            Brushes.Violet
        };

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.Register("Functions", typeof(IEnumerable<FunctionItem>), typeof(GraphPlotter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnFunctionsChanged));

        public static readonly DependencyProperty ExpressionsProperty =
            DependencyProperty.Register("Expressions", typeof(IEnumerable<string>), typeof(GraphPlotter), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnExpressionsChanged));

        public static readonly DependencyProperty ShowKeyPointsProperty =
            DependencyProperty.Register("ShowKeyPoints", typeof(bool), typeof(GraphPlotter),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowKeyPoints
        {
            get => (bool)GetValue(ShowKeyPointsProperty);
            set => SetValue(ShowKeyPointsProperty, value);
        }

        public IEnumerable<FunctionItem> Functions
        {
            get { return (IEnumerable<FunctionItem>)GetValue(FunctionsProperty); }
            set { SetValue(FunctionsProperty, value); }
        }

        public IEnumerable<string> Expressions
        {
            get { return (IEnumerable<string>)GetValue(ExpressionsProperty); }
            set { SetValue(ExpressionsProperty, value); }
        }

        private static void OnFunctionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var plotter = (GraphPlotter)d;

            if (e.OldValue is INotifyCollectionChanged oldCol)
            {
                oldCol.CollectionChanged -= plotter.OnFunctionsCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCol)
            {
                newCol.CollectionChanged += plotter.OnFunctionsCollectionChanged;
            }

            if (e.NewValue is IEnumerable<FunctionItem> items)
            {
                foreach (var item in items)
                {
                    item.PropertyChanged -= plotter.OnFunctionItemPropertyChanged;
                    item.PropertyChanged += plotter.OnFunctionItemPropertyChanged;
                }
            }

            plotter.InvalidateVisual();
        }

        private static void OnExpressionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var plotter = (GraphPlotter)d;

            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= plotter.OnCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += plotter.OnCollectionChanged;
            }

            plotter.InvalidateVisual();
        }

        private void OnFunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FunctionItem item in e.NewItems)
                {
                    item.PropertyChanged -= OnFunctionItemPropertyChanged;
                    item.PropertyChanged += OnFunctionItemPropertyChanged;
                }
            }
            InvalidateVisual();
        }

        private void OnFunctionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public GraphPlotter()
        {
            this.ClipToBounds = true;
            this.Focusable = true;
            
            this.MouseWheel += OnMouseWheel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.MouseLeave += OnMouseLeave;
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
            Point currentPos = e.GetPosition(this);
            _hoverPoint = currentPos;
            _hasHover = true;

            if (_isDragging)
            {
                double deltaX = currentPos.X - _lastMousePosition.X;
                double deltaY = currentPos.Y - _lastMousePosition.Y;
                
                _transformer.Pan(deltaX, deltaY);
                _lastMousePosition = currentPos;
            }

            InvalidateVisual();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            this.ReleaseMouseCapture();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _hasHover = false;
            InvalidateVisual();
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

            if (ShowKeyPoints)
            {
                DrawKeyPoints(dc);
            }

            if (_hasHover)
            {
                DrawHoverInfo(dc);
            }
        }

        private void DrawGrid(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();
            
            double rawStep = 50.0 / _transformer.Scale;
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
                if (Math.Abs(x) < 0.0001) continue;
                Point p1 = _transformer.MathToScreen(x, bounds.minY);
                Point p2 = _transformer.MathToScreen(x, bounds.maxY);
                dc.DrawLine(GridPen, p1, p2);
                
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

        private void DrawText(DrawingContext dc, string text, Point origin, Brush? brush = null)
        {
            FormattedText ft = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                11,
                brush ?? Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            
            dc.DrawText(ft, origin);
        }

        private void DrawAxes(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();
            
            Point xStart = _transformer.MathToScreen(bounds.minX, 0);
            Point xEnd = _transformer.MathToScreen(bounds.maxX, 0);
            dc.DrawLine(AxisPen, xStart, xEnd);

            Point yStart = _transformer.MathToScreen(0, bounds.minY);
            Point yEnd = _transformer.MathToScreen(0, bounds.maxY);
            dc.DrawLine(AxisPen, yStart, yEnd);
        }

        private void DrawFunctions(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();

            if (Functions != null)
            {
                foreach (var func in Functions)
                {
                    if (string.IsNullOrWhiteSpace(func.Expression)) continue;
                    Pen funcPen = new Pen(func.Brush, 2);
                    DrawSingleFunction(dc, funcPen, func.Expression, bounds);
                }
            }
            else if (Expressions != null)
            {
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
        }

        private void DrawSingleFunction(DrawingContext dc, Pen pen, string expression, (double minX, double maxX, double minY, double maxY) bounds)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                bool isFirstPoint = true;
                bool wasValid = false;
                
                double stepX = 1.0 / _transformer.Scale;
                double prevY = 0;

                for (double mathX = bounds.minX; mathX <= bounds.maxX; mathX += stepX)
                {
                    try
                    {
                        string injected = expression.Replace("x", $"({mathX.ToString(CultureInfo.InvariantCulture)})");
                        var parser = new ExpressionParser(injected, false);
                        double mathY = parser.Parse();

                        if (double.IsNaN(mathY) || double.IsInfinity(mathY))
                        {
                            wasValid = false;
                            continue;
                        }

                        if (wasValid && Math.Abs(mathY - prevY) > (bounds.maxY - bounds.minY))
                        {
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

        private void DrawKeyPoints(DrawingContext dc)
        {
            var bounds = _transformer.GetMathBounds();
            var expressionsList = Functions != null
                ? Functions.Where(f => !string.IsNullOrWhiteSpace(f.Expression)).Select(f => f.Expression).ToList()
                : (Expressions != null ? Expressions.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() : new List<string>());

            if (expressionsList.Count == 0) return;

            foreach (var expr in expressionsList)
            {
                var points = FunctionAnalyzer.FindKeyPoints(expr, bounds.minX, bounds.maxX);
                foreach (var kp in points)
                {
                    Point sp = _transformer.MathToScreen(kp.X, kp.Y);
                    if (sp.X < 0 || sp.X > ActualWidth || sp.Y < 0 || sp.Y > ActualHeight) continue;

                    Brush dotBrush = kp.Type switch
                    {
                        KeyPointType.Root => Brushes.LimeGreen,
                        KeyPointType.YIntercept => Brushes.Cyan,
                        KeyPointType.Maximum => Brushes.Orange,
                        KeyPointType.Minimum => Brushes.DeepSkyBlue,
                        _ => Brushes.Yellow
                    };

                    dc.DrawEllipse(dotBrush, new Pen(Brushes.Black, 1), sp, 5, 5);
                    DrawText(dc, kp.Label, new Point(sp.X + 7, sp.Y - 14), dotBrush);
                }
            }

            // Intersecciones entre pares de funciones
            if (expressionsList.Count >= 2)
            {
                for (int i = 0; i < expressionsList.Count - 1; i++)
                {
                    for (int j = i + 1; j < expressionsList.Count; j++)
                    {
                        var intersections = FunctionAnalyzer.FindIntersections(expressionsList[i], expressionsList[j], bounds.minX, bounds.maxX);
                        foreach (var ip in intersections)
                        {
                            Point sp = _transformer.MathToScreen(ip.X, ip.Y);
                            if (sp.X < 0 || sp.X > ActualWidth || sp.Y < 0 || sp.Y > ActualHeight) continue;

                            dc.DrawEllipse(Brushes.Yellow, new Pen(Brushes.Black, 1), sp, 5, 5);
                            DrawText(dc, ip.Label, new Point(sp.X + 7, sp.Y - 14), Brushes.Yellow);
                        }
                    }
                }
            }
        }

        private void DrawHoverInfo(DrawingContext dc)
        {
            Point mathP = _transformer.ScreenToMath(_hoverPoint.X, _hoverPoint.Y);
            string coords = $"({mathP.X:0.##}, {mathP.Y:0.##})";

            // Dibujar mira/cursor en pantalla
            Pen hoverPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)), 1)
            {
                DashStyle = DashStyles.Dash
            };

            dc.DrawLine(hoverPen, new Point(_hoverPoint.X, 0), new Point(_hoverPoint.X, ActualHeight));
            dc.DrawLine(hoverPen, new Point(0, _hoverPoint.Y), new Point(ActualWidth, _hoverPoint.Y));

            // Caja con coordenadas
            Rect infoRect = new Rect(_hoverPoint.X + 10, _hoverPoint.Y - 25, 110, 22);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(200, 44, 44, 46)), new Pen(Brushes.Gray, 1), infoRect, 4, 4);
            DrawText(dc, coords, new Point(_hoverPoint.X + 15, _hoverPoint.Y - 22), Brushes.White);
        }
    }
}
