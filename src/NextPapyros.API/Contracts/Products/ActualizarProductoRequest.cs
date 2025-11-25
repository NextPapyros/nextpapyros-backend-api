namespace NextPapyros.API.Contracts.Productos;

public record ActualizarProductoRequest(
    string Nombre,
    string Categoria,
    double Costo,
    double Precio,
    int StockMinimo);
