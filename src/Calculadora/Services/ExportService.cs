using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Calculadora.Services
{
    public class ProjectData
    {
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public List<string> GraphExpressions { get; set; } = new();
        public List<HistorySaveEntry> History { get; set; } = new();
    }

    public class HistorySaveEntry
    {
        public string Module { get; set; } = "";
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";
    }

    public static class ExportService
    {
        /// <summary>
        /// Captura un FrameworkElement (como el Graficador 2D) y lo guarda como imagen PNG.
        /// </summary>
        public static bool ExportToPng(FrameworkElement element, string filePath)
        {
            if (element == null || string.IsNullOrWhiteSpace(filePath)) return false;

            try
            {
                int width = (int)Math.Max(element.ActualWidth, 800);
                int height = (int)Math.Max(element.ActualHeight, 600);

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    width, height, 96, 96, PixelFormats.Pbgra32);

                element.Measure(new Size(width, height));
                element.Arrange(new Rect(new Size(width, height)));

                renderBitmap.Render(element);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Guarda los datos del proyecto en un archivo JSON (.calc).
        /// </summary>
        public static bool SaveProject(ProjectData project, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(project, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Carga un archivo de proyecto JSON (.calc).
        /// </summary>
        public static ProjectData? LoadProject(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<ProjectData>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
