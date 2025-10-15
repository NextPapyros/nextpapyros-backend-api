namespace NextPapyros.API.Contracts.Recepciones;

public record RecepcionLineaRequest(string ProductoCodigo, int CantidadRecibida);

public record RegistrarRecepcionRequest(
    int OrdenCompraId,
    string NroFacturaGuia,
    IEnumerable<RecepcionLineaRequest> Lineas
);

public record RecepcionResponse(
    int Id, DateTime Fecha, int OrdenCompraId, string NroFacturaGuia,
    IEnumerable<RecepcionLineaItem> Lineas
);

public record RecepcionLineaItem(int Id, string ProductoCodigo, string ProductoNombre, int CantidadRecibida);