using System;
using System.Collections.Generic;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace Calculadora.Models
{
    /// <summary>
    /// Capa adaptadora que envuelve Math.NET Symbolics (F#) en una API idiomática de C#.
    /// Aisla los tipos de F# (FSharpOption, discriminated unions) del resto de la aplicación,
    /// exponiendo solo tipos primitivos de C# (string, double).
    /// </summary>
    public class SymbolicAdapter
    {
        /// <summary>
        /// Deriva simbólicamente una expresión respecto a una variable.
        /// Ejemplo: Differentiate("x^2 + 3*x", "x") → "2*x + 3"
        /// </summary>
        public string Differentiate(string expression, string variable = "x")
        {
            try
            {
                var expr = Expr.Parse(expression);
                var derivative = expr.Differentiate(variable);
                return FormatExpression(derivative);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"No se pudo derivar la expresión: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Simplifica una expresión algebraica.
        /// Ejemplo: Simplify("x + x") → "2*x"
        /// </summary>
        public string Simplify(string expression)
        {
            try
            {
                var expr = Expr.Parse(expression);
                // Math.NET Symbolics aplica simplificación automática al parsear
                // y con RationalSimplify para simplificaciones más agresivas
                return FormatExpression(expr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"No se pudo simplificar la expresión: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Expande una expresión algebraica (distribuye productos, elimina paréntesis).
        /// Ejemplo: Expand("(x+1)^2") → "1 + 2*x + x^2"
        /// </summary>
        public string Expand(string expression)
        {
            try
            {
                var expr = Expr.Parse(expression);
                var expanded = expr.Expand();
                return FormatExpression(expanded);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"No se pudo expandir la expresión: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Evalúa numéricamente una expresión simbólica con valores dados para las variables.
        /// Ejemplo: Evaluate("x^2 + 1", {{"x", 3}}) → 10.0
        /// </summary>
        public double Evaluate(string expression, Dictionary<string, double> variables)
        {
            try
            {
                var expr = Expr.Parse(expression);

                // Construir el diccionario de símbolos para Math.NET
                var symbols = new Dictionary<string, FloatingPoint>();
                foreach (var kvp in variables)
                {
                    symbols[kvp.Key] = kvp.Value;
                }

                var result = expr.Evaluate(symbols);

                if (result is FloatingPoint.Real realResult)
                {
                    return realResult.Item;
                }

                throw new InvalidOperationException("El resultado no es un número real.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"No se pudo evaluar la expresión: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convierte una expresión a formato LaTeX.
        /// Ejemplo: ToLaTeX("x^2 + 3*x + 1") → "x^{2} + 3 \\cdot x + 1"
        /// </summary>
        public string ToLaTeX(string expression)
        {
            try
            {
                var expr = Expr.Parse(expression);
                return expr.ToLaTeX();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"No se pudo convertir a LaTeX: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Formatea una expresión simbólica como string legible en notación infija.
        /// </summary>
        private string FormatExpression(Expr expression)
        {
            return expression.ToString();
        }
    }
}
