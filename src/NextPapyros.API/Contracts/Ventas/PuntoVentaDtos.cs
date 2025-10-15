namespace NextPapyros.API.Contracts.Ventas;

public record ProductoPosResponse(
    string Codigo,
    string Nombre,
    string Categoria,
    double Precio,
    int StockDisponible
);

public record AgregarProductoRequest(
    string ProductoCodigo,
    int Cantidad
);

public record ProductoAgregadoResponse(
    string ProductoCodigo,
    string ProductoNombre,
    int Cantidad,
    double PrecioUnitario,
    double Subtotal,
    int StockRestante
);
