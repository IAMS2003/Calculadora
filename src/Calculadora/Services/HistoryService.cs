using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Calculadora.Services
{
    /// <summary>
    /// Registro de una operación realizada en la calculadora.
    /// </summary>
    public class HistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string Module { get; set; } = "";
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";

        public string FormattedTime => Timestamp.ToString("HH:mm:ss");
        public string Summary => $"{Expression} = {Result}";
    }

    /// <summary>
    /// Servicio singleton para almacenar y consultar el historial de operaciones
    /// de todos los módulos de la calculadora.
    /// </summary>
    public class HistoryService
    {
        private static readonly Lazy<HistoryService> _instance =
            new Lazy<HistoryService>(() => new HistoryService());
        public static HistoryService Instance => _instance.Value;

        public ObservableCollection<HistoryEntry> Entries { get; } = new();

        private const int MaxEntries = 200;

        private readonly object _lock = new object();

        /// <summary>
        /// Registra una nueva operación en el historial.
        /// </summary>
        /// <param name="module">Módulo de origen (ej: "Básica", "Científica", "Simbólico")</param>
        /// <param name="expression">Expresión evaluada</param>
        /// <param name="result">Resultado obtenido</param>
        public void Add(string module, string expression, string result)
        {
            if (string.IsNullOrWhiteSpace(expression) || string.IsNullOrWhiteSpace(result))
                return;

            var entry = new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Module = module,
                Expression = expression,
                Result = result
            };

            lock (_lock)
            {
                // Insertar al inicio para que el más reciente aparezca primero
                Entries.Insert(0, entry);

                // Limitar el tamaño del historial
                while (Entries.Count > MaxEntries)
                    Entries.RemoveAt(Entries.Count - 1);
            }
        }

        /// <summary>
        /// Limpia todo el historial.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Entries.Clear();
            }
        }
    }
}
