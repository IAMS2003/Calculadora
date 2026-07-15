using System;

namespace Calculadora.Models
{
    public static class ScientificFunctions
    {
        public static double EvaluateUnaryFunction(string functionName, double value, bool isDegrees = false)
        {
            switch (functionName.ToLower())
            {
                case "sin":
                    return Math.Sin(isDegrees ? ToRadians(value) : value);
                case "cos":
                    return Math.Cos(isDegrees ? ToRadians(value) : value);
                case "tan":
                    return Math.Tan(isDegrees ? ToRadians(value) : value);
                case "asin":
                    return isDegrees ? ToDegrees(Math.Asin(value)) : Math.Asin(value);
                case "acos":
                    return isDegrees ? ToDegrees(Math.Acos(value)) : Math.Acos(value);
                case "atan":
                    return isDegrees ? ToDegrees(Math.Atan(value)) : Math.Atan(value);
                case "ln":
                    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Domain error");
                    return Math.Log(value);
                case "log":
                    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Domain error");
                    return Math.Log10(value);
                case "exp":
                    return Math.Exp(value);
                case "sqrt":
                    if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Domain error");
                    return Math.Sqrt(value);
                case "cbrt":
                    return Math.Cbrt(value);
                case "fact":
                case "!":
                    if (value < 0 || value != Math.Floor(value)) throw new ArgumentOutOfRangeException(nameof(value), "Domain error");
                    return Factorial((int)value);
                default:
                    throw new ArgumentException($"Unknown function: {functionName}");
            }
        }

        public static double EvaluateBinaryFunction(string operatorStr, double left, double right)
        {
            switch (operatorStr)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": 
                    if (right == 0) throw new DivideByZeroException();
                    return left / right;
                case "^": return Math.Pow(left, right);
                case "%": return left % right;
                default: throw new ArgumentException($"Unknown operator: {operatorStr}");
            }
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
        private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

        private static double Factorial(int n)
        {
            if (n == 0) return 1;
            double result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }
}
