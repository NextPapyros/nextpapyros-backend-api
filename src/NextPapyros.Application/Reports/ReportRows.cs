namespace NextPapyros.Application.Reports;

public sealed record TopProductoRow(string Codigo, string Nombre, int CantidadVendida, double Ingresos);
public sealed record StockBajoRow(string Codigo, string Nombre, int Stock, int StockMinimo);
public sealed record IngresoMensualRow(int Mes, double Ingresos);