# 🧩 NextPapyros Backend API

**NextPapyros** es una aplicación modular diseñada bajo los principios de **Clean Architecture** y **SOLID**, utilizando **.NET 8**, **C#**, **Entity Framework Core** y **SQL Server (Docker)**.  
El objetivo del sistema es **gestionar inventario, ventas, compras, devoluciones y generación de reportes** de manera limpia, escalable y mantenible.

---

## 🚀 Tecnologías Principales

| Tecnología | Descripción |
|-------------|-------------|
| **.NET 8 (C#)** | Framework principal para el desarrollo del backend |
| **Entity Framework Core** | ORM para acceso a datos y migraciones |
| **SQL Server** | Base de datos principal |
| **Clean Architecture** | Separación en capas independientes |
| **Principios SOLID** | Código limpio, extensible y mantenible |
| **Patrones de Diseño** | Repository, Unit of Work, Strategy, Template Method, Domain Service |

---

## 🧱 Estructura del Proyecto

```plaintext
NextPapyros-backend-api/
├── src/
│   ├── NextPapyros.API/              # Capa de presentación (endpoints)
│   ├── NextPapyros.Application/      # Casos de uso / lógica de aplicación
│   ├── NextPapyros.Domain/           # Entidades de dominio y lógica de negocio
│   └── NextPapyros.Infrastructure/   # Persistencia, EF Core y repositorios
├── NextPapyros.sln                   # Solución principal
└── README.md
