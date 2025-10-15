using System.Text;
using NextPapyros.Application.Reports;

namespace NextPapyros.Infrastructure.Reports;

public class CsvReportExporter : IReportExporter
{
    public string Format => "csv";

    public (byte[] Content, string ContentType, string FileName) Export<T>(IEnumerable<T> rows, string baseFileName)
    {
        var list = rows?.ToList() ?? [];
        var sb = new StringBuilder();

        if (list.Count > 0)
        {
            var props = typeof(T).GetProperties();
            sb.AppendLine(string.Join(",", props.Select(p => Escape(p.Name))));
            foreach (var row in list)
            {
                var vals = props.Select(p => Escape(p.GetValue(row)));
                sb.AppendLine(string.Join(",", vals));
            }
        }

        static string Escape(object? v)
        {
            var s = v?.ToString() ?? string.Empty;
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
                s = $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return (bytes, "text/csv; charset=utf-8", $"{baseFileName}.csv");
    }
}