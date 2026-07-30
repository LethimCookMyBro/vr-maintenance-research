using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TMUVR.MaintenanceResearch
{
    public static class CsvUtility
    {
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static string Cell(object value)
        {
            if (value == null)
                return string.Empty;

            var text = value switch
            {
                float number => number.ToString("0.000000", Invariant),
                double number => number.ToString("0.000000", Invariant),
                bool boolean => boolean ? "true" : "false",
                DateTimeOffset timestamp => timestamp.ToString("O", Invariant),
                _ => Convert.ToString(value, Invariant) ?? string.Empty,
            };

            return text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + text.Replace("\"", "\"\"") + "\"" : text;
        }

        public static void WriteRow(TextWriter writer, params object[] cells)
        {
            for (var i = 0; i < cells.Length; i++)
            {
                if (i > 0)
                    writer.Write(',');
                writer.Write(Cell(cells[i]));
            }
            writer.WriteLine();
        }

        public static StreamWriter CreateUtf8Writer(string path, params string[] header)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var writer = new StreamWriter(path, true, new UTF8Encoding(false)) { AutoFlush = true };
            if (new FileInfo(path).Length == 0)
                WriteRow(writer, header);
            return writer;
        }
    }
}
