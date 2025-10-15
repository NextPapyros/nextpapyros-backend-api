using NextPapyros.Application.Reports;

namespace NextPapyros.Infrastructure.Reports;

public class PdfReportExporter : IReportExporter
{
    public string Format => "pdf";

    public (byte[] Content, string ContentType, string FileName) Export<T>(IEnumerable<T> rows, string baseFileName)
    {
        var text = "PDF export is not implemented yet.\n\n" +
                   $"Rows: {rows?.Count() ?? 0}\n";
        var fakeBytes = System.Text.Encoding.UTF8.GetBytes(text);
        return (fakeBytes, "application/pdf", $"{baseFileName}.pdf");
    }
}