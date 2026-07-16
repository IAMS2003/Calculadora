using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Calculadora.Models
{
    public class ExpressionParser
    {
        private readonly string _expression;
        private int _pos;
        private readonly bool _isDegrees;

        public ExpressionParser(string expression, bool isDegrees = false)
        {
            // Normalizar: elimina espacios e inserta '*' en multiplicaciones implícitas
            _expression = ExpressionNormalizer.Normalize(expression);
            _pos = 0;
            _isDegrees = isDegrees;
        }

        public double Parse()
        {
            if (string.IsNullOrWhiteSpace(_expression))
                return 0;

            double result = ParseExpression();
            if (_pos < _expression.Length)
                throw new Exception($"Unexpected character at position {_pos}: {_expression[_pos]}");

            return result;
        }

        // Expression = Term { ("+"|"-") Term }
        private double ParseExpression()
        {
            double result = ParseTerm();
            while (_pos < _expression.Length)
            {
                char op = _expression[_pos];
                if (op != '+' && op != '-') break;
                _pos++;
                double nextTerm = ParseTerm();
                result = ScientificFunctions.EvaluateBinaryFunction(op.ToString(), result, nextTerm);
            }
            return result;
        }

        // Term = Factor { ("*"|"/"|"%"|implicit) Factor }
        // Multiplicación implícita: un número o ')' seguido directamente de '(' o una letra
        // Ejemplo: 2x → 2*x, 2(3) → 2*(3), sin(x)cos(x) → sin(x)*cos(x)
        private double ParseTerm()
        {
            double result = ParseFactor();
            while (_pos < _expression.Length)
            {
                char op = _expression[_pos];
                if (op == '*' || op == '/' || op == '%')
                {
                    _pos++;
                    double nextFactor = ParseFactor();
                    result = ScientificFunctions.EvaluateBinaryFunction(op.ToString(), result, nextFactor);
                }
                else if (op == '(' || char.IsLetter(op))
                {
                    // Multiplicación implícita: el siguiente token es '(' o un identificador
                    double nextFactor = ParseFactor();
                    result = ScientificFunctions.EvaluateBinaryFunction("*", result, nextFactor);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        // Factor = Power { "^" Power }
        private double ParseFactor()
        {
            double result = ParsePower();
            while (_pos < _expression.Length && _expression[_pos] == '^')
            {
                _pos++;
                double nextPower = ParseFactor(); // Right-associative
                result = ScientificFunctions.EvaluateBinaryFunction("^", result, nextPower);
            }
            return result;
        }

        // Power = { "-" | "+" } Value [ "!" ]
        private double ParsePower()
        {
            int sign = 1;
            while (_pos < _expression.Length && (_expression[_pos] == '+' || _expression[_pos] == '-'))
            {
                if (_expression[_pos] == '-') sign = -sign;
                _pos++;
            }

            double result = ParseValue();

            // Check for factorial
            if (_pos < _expression.Length && _expression[_pos] == '!')
            {
                _pos++;
                result = ScientificFunctions.EvaluateUnaryFunction("!", result);
            }

            return sign * result;
        }

        // Value = Number | Constant | Function "(" Expression ")" | "(" Expression ")"
        private double ParseValue()
        {
            if (_pos >= _expression.Length)
                throw new Exception("Unexpected end of expression");

            char c = _expression[_pos];

            if (c == '(')
            {
                _pos++;
                double result = ParseExpression();
                if (_pos >= _expression.Length || _expression[_pos] != ')')
                    throw new Exception("Missing closing parenthesis");
                _pos++;
                return result;
            }

            if (char.IsDigit(c) || c == '.')
            {
                return ParseNumber();
            }

            if (char.IsLetter(c))
            {
                string name = ParseIdentifier();

                // Constants
                if (name.Equals("pi", StringComparison.OrdinalIgnoreCase)) return Math.PI;
                if (name.Equals("e", StringComparison.OrdinalIgnoreCase)) return Math.E;

                // Function calls
                if (_pos < _expression.Length && _expression[_pos] == '(')
                {
                    _pos++;
                    double arg = ParseExpression();
                    if (_pos >= _expression.Length || _expression[_pos] != ')')
                        throw new Exception("Missing closing parenthesis for function call");
                    _pos++;
                    return ScientificFunctions.EvaluateUnaryFunction(name, arg, _isDegrees);
                }
                
                throw new Exception($"Unknown identifier: {name}");
            }

            throw new Exception($"Unexpected character: {c}");
        }

        private double ParseNumber()
        {
            int startPos = _pos;
            while (_pos < _expression.Length && (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.'))
            {
                _pos++;
            }
            string numStr = _expression.Substring(startPos, _pos - startPos);
            if (!double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                throw new Exception($"Invalid number format: {numStr}");
            return result;
        }

        private string ParseIdentifier()
        {
            int startPos = _pos;
            while (_pos < _expression.Length && char.IsLetter(_expression[_pos]))
            {
                _pos++;
            }
            return _expression.Substring(startPos, _pos - startPos);
        }
    }
}
