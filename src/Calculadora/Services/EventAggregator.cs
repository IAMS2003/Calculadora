using System;

namespace Calculadora.Services
{
    /// <summary>
    /// EventAggregator simple para comunicación desacoplada entre ViewModels.
    /// Permite que un módulo (ej. Cálculo) envíe datos a otro (ej. Graficador)
    /// sin referencias directas entre ellos.
    /// </summary>
    public class EventAggregator
    {
        private static readonly Lazy<EventAggregator> _instance = new Lazy<EventAggregator>(() => new EventAggregator());
        public static EventAggregator Instance => _instance.Value;

        /// <summary>
        /// Se dispara cuando un módulo quiere enviar una función al graficador.
        /// El string es la expresión matemática (ej. "2x+3").
        /// </summary>
        public event Action<string>? SendToGraphRequested;

        public void SendToGraph(string expression)
        {
            SendToGraphRequested?.Invoke(expression);
        }
    }
}
