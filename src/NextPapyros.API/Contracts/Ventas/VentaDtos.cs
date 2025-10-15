using NextPapyros.Domain.Entities.Enums;

namespace NextPapyros.API.Contracts.Ventas;

public record VentaLineaRequest(string ProductoCodigo, int Cantidad, double PrecioUnitario);

public record RegistrarVentaRequest(
    MetodoPago MetodoPago,
    IEnumerable<VentaLineaRequest> Lineas
);

public record VentaResponse(
    int Id, DateTime Fecha, double Total, string Estado, MetodoPago MetodoPago,
    IEnumerable<VentaLineaItem> Lineas
);

public record VentaLineaItem(
    int Id, string ProductoCodigo, string ProductoNombre, int Cantidad, double PrecioUnitario, double Subtotal
);