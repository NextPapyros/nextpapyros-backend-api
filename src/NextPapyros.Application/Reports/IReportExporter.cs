namespace NextPapyros.Application.Reports;

public interface IReportExporter
{
    /// <summary>Formato que soporta este exportador, ej.: "csv", "pdf". Minúsculas.</summary>
    string Format { get; }

    /// <summary>Exporta una secuencia de filas (objetos anónimos/DTOs) a un archivo binario.</summary>
    /// <param name="rows">Filas a exportar.</param>
    /// <param name="baseFileName">Nombre base sin extensión.</param>
    /// <returns>binario, content-type y fileName (con extensión).</returns>
    (byte[] Content, string ContentType, string FileName) Export<T>(IEnumerable<T> rows, string baseFileName);
}