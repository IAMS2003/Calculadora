namespace Calculadora.Models
{
    public class CalculatorModel
    {
        public double Add(double a, double b)
        {
            return a + b;
        }

        public double Subtract(double a, double b)
        {
            return a - b;
        }

        public double Multiply(double a, double b)
        {
            return a * b;
        }

        public double Divide(double a, double b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("No se puede dividir entre cero.");
            }
            return a / b;
        }

        public double Percentage(double value, double percentage)
        {
            return value * (percentage / 100.0);
        }

        public double Calculate(double a, double b, OperationType op)
        {
            return op switch
            {
                OperationType.Add => Add(a, b),
                OperationType.Subtract => Subtract(a, b),
                OperationType.Multiply => Multiply(a, b),
                OperationType.Divide => Divide(a, b),
                OperationType.Percentage => Percentage(a, b),
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, "Operación no soportada.")
            };
        }
    }
}
