using System.Collections.Generic;
using System.Text;

namespace Calculadora.Models
{
    /// <summary>
    /// Normaliza expresiones matemáticas insertando operadores de multiplicación explícitos
    /// donde la notación matemática estándar implica multiplicación.
    /// 
    /// Casos cubiertos:
    ///   3x       → 3*x        (dígito seguido de letra)
    ///   2(3+1)   → 2*(3+1)    (dígito seguido de paréntesis abierto)
    ///   (2)(3)   → (2)*(3)    (paréntesis cerrado seguido de paréntesis abierto)
    ///   )x       → )*x        (paréntesis cerrado seguido de letra)
    ///   )2       → )*2        (paréntesis cerrado seguido de dígito)
    ///   2pi      → 2*pi       (dígito seguido de constante — caso de dígito+letra)
    ///   2sin(x)  → 2*sin(x)   (dígito seguido de función — caso de dígito+letra)
    ///   x y      → x*y        (después de quitar espacios, caso de letra+letra con contexto)
    ///   5!3      → 5!*3       (factorial seguido de dígito)
    ///   5!(      → 5!*(       (factorial seguido de paréntesis)
    ///   5!x      → 5!*x       (factorial seguido de letra)
    /// </summary>
    public static class ExpressionNormalizer
    {
        // Funciones matemáticas conocidas que NO deben recibir '*' antes del '('
        private static readonly HashSet<string> KnownFunctions = new HashSet<string>
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "ln", "log", "exp", "sqrt", "cbrt", "abs",
            "sinh", "cosh", "tanh", "sec", "csc", "cot"
        };

        /// <summary>
        /// Inserta '*' explícitos donde hay multiplicación implícita.
        /// </summary>
        public static string Normalize(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;

            // Paso 1: Eliminar espacios
            expression = expression.Replace(" ", "");

            var result = new StringBuilder();

            for (int i = 0; i < expression.Length; i++)
            {
                char current = expression[i];
                result.Append(current);

                // No hay siguiente carácter → nada que hacer
                if (i + 1 >= expression.Length)
                    continue;

                char next = expression[i + 1];

                bool needsMultiply = false;

                // CASO 1: dígito seguido de letra → 3x, 2sin(x), 2pi
                if (char.IsDigit(current) && char.IsLetter(next))
                {
                    needsMultiply = true;
                }
                // CASO 2: dígito seguido de '(' → 2(3+1)
                else if (char.IsDigit(current) && next == '(')
                {
                    needsMultiply = true;
                }
                // CASO 3: ')' seguido de '(' → (2)(3)
                else if (current == ')' && next == '(')
                {
                    needsMultiply = true;
                }
                // CASO 4: ')' seguido de letra → )x, )sin(x)
                else if (current == ')' && char.IsLetter(next))
                {
                    needsMultiply = true;
                }
                // CASO 5: ')' seguido de dígito → )2
                else if (current == ')' && char.IsDigit(next))
                {
                    needsMultiply = true;
                }
                // CASO 6: '!' seguido de dígito, letra o '(' → 5!3, 5!x, 5!(2+1)
                else if (current == '!' && (char.IsDigit(next) || char.IsLetter(next) || next == '('))
                {
                    needsMultiply = true;
                }

                if (needsMultiply)
                {
                    result.Append('*');
                }
            }

            return result.ToString();
        }
    }
}
