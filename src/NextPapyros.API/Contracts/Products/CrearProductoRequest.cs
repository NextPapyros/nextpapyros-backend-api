namespace NextPapyros.API.Contracts.Productos;

public record CrearProductoRequest(
    string Codigo,
    string Nombre,
    string Categoria,
    double Costo,
    double Precio,
    int StockMinimo);

public record ProductoResponse(
    string Codigo, string Nombre, string Categoria,
    double Costo, double Precio, int Stock, int StockMinimo, bool Activo);

public record AjusteStockRequest(
    int Cantidad, string Motivo
);