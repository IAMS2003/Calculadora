using System;

namespace Calculadora.Models
{
    /// <summary>
    /// Excepción lanzada cuando un método numérico iterativo no converge
    /// dentro del número máximo de iteraciones permitido.
    /// </summary>
    public class ConvergenceException : Exception
    {
        public int IterationsPerformed { get; }

        public ConvergenceException(string message, int iterations)
            : base(message)
        {
            IterationsPerformed = iterations;
        }
    }

    /// <summary>
    /// Métodos de cálculo numérico: integración definida (Simpson, Trapecio)
    /// y resolución de ecuaciones (Newton-Raphson, fórmula cuadrática).
    /// </summary>
    public static class NumericalCalculus
    {
        // ===== INTEGRACIÓN NUMÉRICA =====

        /// <summary>
        /// Calcula la integral definida usando el método del Trapecio compuesto.
        /// ∫[a,b] f(x) dx ≈ h/2 * [f(a) + 2*f(x1) + 2*f(x2) + ... + f(b)]
        /// </summary>
        /// <param name="f">Función a integrar</param>
        /// <param name="a">Límite inferior</param>
        /// <param name="b">Límite superior</param>
        /// <param name="n">Número de subintervalos (mayor = más preciso)</param>
        public static double TrapezoidalRule(Func<double, double> f, double a, double b, int n = 1000)
        {
            if (n <= 0) throw new ArgumentException("El número de subintervalos debe ser positivo.");

            // Si a > b, invertir y negar el resultado
            if (a > b)
                return -TrapezoidalRule(f, b, a, n);
            if (Math.Abs(a - b) < 1e-15)
                return 0;

            double h = (b - a) / n;
            double sum = 0.5 * (f(a) + f(b));

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                double fx = f(x);
                if (double.IsNaN(fx) || double.IsInfinity(fx))
                    throw new ArithmeticException($"La función no es evaluable en x={x:G4}. Posible discontinuidad.");
                sum += fx;
            }

            return h * sum;
        }

        /// <summary>
        /// Calcula la integral definida usando el método de Simpson compuesto (1/3).
        /// Más preciso que el Trapecio para funciones suaves.
        /// </summary>
        public static double SimpsonRule(Func<double, double> f, double a, double b, int n = 1000)
        {
            if (n <= 0) throw new ArgumentException("El número de subintervalos debe ser positivo.");
            if (n % 2 != 0) n++; // Simpson requiere número par de subintervalos

            if (a > b)
                return -SimpsonRule(f, b, a, n);
            if (Math.Abs(a - b) < 1e-15)
                return 0;

            double h = (b - a) / n;
            double sum = f(a) + f(b);

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                double fx = f(x);
                if (double.IsNaN(fx) || double.IsInfinity(fx))
                    throw new ArithmeticException($"La función no es evaluable en x={x:G4}. Posible discontinuidad.");
                sum += (i % 2 == 0) ? 2 * fx : 4 * fx;
            }

            return (h / 3) * sum;
        }

        // ===== RESOLUCIÓN DE ECUACIONES =====

        /// <summary>
        /// Encuentra una raíz de f(x) = 0 usando el método de Newton-Raphson.
        /// f'(x) se aproxima numéricamente con diferencias finitas centrales.
        /// </summary>
        /// <param name="f">Función cuya raíz se busca</param>
        /// <param name="x0">Punto inicial (estimación)</param>
        /// <param name="tolerance">Tolerancia de convergencia</param>
        /// <param name="maxIterations">Máximo número de iteraciones</param>
        public static double NewtonRaphson(Func<double, double> f, double x0,
            double tolerance = 1e-10, int maxIterations = 1000)
        {
            double x = x0;
            double dx = 1e-8; // Para aproximar la derivada numéricamente

            for (int i = 0; i < maxIterations; i++)
            {
                double fx = f(x);

                // ¿Convergió?
                if (Math.Abs(fx) < tolerance)
                    return x;

                // Derivada numérica (diferencias centrales)
                double fprime = (f(x + dx) - f(x - dx)) / (2 * dx);

                if (Math.Abs(fprime) < 1e-15)
                    throw new ConvergenceException(
                        $"Derivada casi cero en x={x:G6}. El método no puede continuar. Prueba con otro punto inicial.", i);

                x = x - fx / fprime;

                if (double.IsNaN(x) || double.IsInfinity(x))
                    throw new ConvergenceException(
                        "El método divergió (resultado NaN/Infinito). Prueba con otro punto inicial.", i);
            }

            throw new ConvergenceException(
                $"No se encontró raíz después de {maxIterations} iteraciones. Último x={x:G6}. Prueba con otro punto inicial.", maxIterations);
        }

        /// <summary>
        /// Resuelve la ecuación cuadrática ax² + bx + c = 0 usando la fórmula general.
        /// Retorna las raíces reales. Si el discriminante es negativo, retorna las raíces complejas como strings.
        /// </summary>
        public static (double? root1, double? root2, string description) SolveQuadratic(double a, double b, double c)
        {
            if (Math.Abs(a) < 1e-15)
            {
                // Ecuación lineal: bx + c = 0
                if (Math.Abs(b) < 1e-15)
                    return (null, null, "La ecuación no tiene variable (0 = 0 o sin solución).");

                double root = -c / b;
                return (root, null, $"Ecuación lineal. x = {root:G6}");
            }

            double discriminant = b * b - 4 * a * c;

            if (discriminant > 0)
            {
                double r1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                double r2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                return (r1, r2, $"Dos raíces reales: x₁ = {r1:G6}, x₂ = {r2:G6}");
            }
            else if (Math.Abs(discriminant) < 1e-12)
            {
                double r = -b / (2 * a);
                return (r, r, $"Raíz doble: x = {r:G6}");
            }
            else
            {
                // Raíces complejas
                double realPart = -b / (2 * a);
                double imagPart = Math.Sqrt(-discriminant) / (2 * a);
                return (null, null,
                    $"Sin raíces reales (Δ = {discriminant:G4} < 0).\n" +
                    $"Raíces complejas: x = {realPart:G6} ± {imagPart:G6}i");
            }
        }
    }
}
