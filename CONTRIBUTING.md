# 🚀 Guía de Contribución - NextPapyros Backend API

¡Gracias por tu interés en contribuir a NextPapyros! Esta guía te ayudará a configurar el proyecto localmente.

---

## 📋 Tabla de Contenidos

- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Configuración de Base de Datos](#-configuración-de-base-de-datos)
- [Migraciones](#-migraciones)
- [Ejecutar el Proyecto](#-ejecutar-el-proyecto)
- [Estructura y Patrones](#-estructura-y-patrones)
- [Guía de Desarrollo](#-guía-de-desarrollo)
- [Troubleshooting](#-troubleshooting)

---

## 📋 Requisitos Previos

### Requisitos Obligatorios

1. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
   ```bash
   dotnet --version  # Debe mostrar 8.0.x
   ```

2. **[Git](https://git-scm.com/downloads)**
   ```bash
   git --version
   ```

3. **Base de Datos** (elige una):
   - **Windows**: SQL Server 2019+ o SQL Server Express
   - **macOS/Linux**: Docker con SQL Server o Azure SQL Edge

4. **Editor de Código** (elige uno):
   - Visual Studio 2022 (Windows, recomendado)
   - Visual Studio Code + C# Dev Kit (multiplataforma)
   - JetBrains Rider (multiplataforma)

---

## 🚀 Instalación y Configuración

### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/NextPapyros/nextpapyros-backend-api.git
cd nextpapyros-backend-api
```

### Paso 2: Restaurar Dependencias

```bash
dotnet restore
```

---

## 💾 Configuración de Base de Datos

### Opción A: Windows (SQL Server Local)

1. **Instalar SQL Server Express**: [Descargar aquí](https://www.microsoft.com/sql-server/sql-server-downloads)
2. **Configurar cadena de conexión** en `src/NextPapyros.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=NextPapyrosDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Opción B: macOS/Linux (Docker)

1. **Ejecutar SQL Server en contenedor**:

```bash
# Intel/AMD
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Apple Silicon (M1/M2/M3)
docker run --platform linux/amd64 \
  -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/mssql/server:2019-latest
```

2. **Configurar cadena de conexión** en `src/NextPapyros.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=NextPapyrosDb;User Id=sa;Password=Password123*;TrustServerCertificate=True;"
  }
}
```

---

## 🔄 Migraciones

### Aplicar Migraciones a la Base de Datos

```bash
cd src/NextPapyros.API
dotnet ef database update
```

### Crear Nueva Migración (solo si modificas entidades)

```bash
dotnet ef migrations add NombreDeLaMigracion \
  --project ../NextPapyros.Infrastructure \
  --startup-project .
```

---

## ▶️ Ejecutar el Proyecto

```bash
cd src/NextPapyros.API
dotnet run
```

Accede a:
- **Swagger**: http://localhost:5288/swagger
- **HTTPS**: https://localhost:7037/swagger

### Usuario por Defecto

El sistema crea automáticamente un usuario administrador:

| Campo | Valor |
|-------|-------|
| Email | `mail@mail.com` |
| Password | `Password123*` |
| Rol | Administrador |

---

## 🏗️ Estructura y Patrones

### Capas del Proyecto

```
NextPapyros.API           → Controladores, DTOs, Configuración
NextPapyros.Application   → Interfaces de casos de uso
NextPapyros.Domain        → Entidades, Interfaces de repositorios
NextPapyros.Infrastructure → Implementaciones, DbContext, Migrations
```

### Patrones Implementados

| Patrón | Ubicación | Propósito |
|--------|-----------|-----------|
| **Repository** | `Domain/Repositories` + `Infrastructure/Repositories` | Abstracción de acceso a datos |
| **Unit of Work** | `Infrastructure/Persistence/UnitOfWork.cs` | Transacciones atómicas |
| **Dependency Injection** | `Infrastructure/DependencyInjection.cs` | Inversión de control |
| **Strategy** | `Application/Reports/IReportExporter` | Exportación dinámica de reportes |

---

## 📝 Guía de Desarrollo

### Crear un Nuevo Endpoint

#### 1. Definir el Contrato (DTO)

```csharp
// src/NextPapyros.API/Contracts/MiModulo/MiContracts.cs
public record CrearEntidadRequest(string Nombre, string Descripcion);
public record EntidadResponse(int Id, string Nombre, string Descripcion);
```

#### 2. Crear la Interfaz del Repositorio

```csharp
// src/NextPapyros.Domain/Repositories/IEntidadRepository.cs
public interface IEntidadRepository
{
    Task<Entidad?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Entidad entidad, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

#### 3. Implementar el Repositorio

```csharp
// src/NextPapyros.Infrastructure/Repositories/EntidadRepository.cs
public class EntidadRepository(NextPapyrosDbContext db) : IEntidadRepository
{
    public Task<Entidad?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Entidades.FirstOrDefaultAsync(e => e.Id == id, ct);
    
    public async Task AddAsync(Entidad entidad, CancellationToken ct = default) =>
        await db.Entidades.AddAsync(entidad, ct);
}
```

#### 4. Registrar en DI

```csharp
// src/NextPapyros.Infrastructure/DependencyInjection.cs
services.AddScoped<IEntidadRepository, EntidadRepository>();
```

#### 5. Crear el Controlador

```csharp
// src/NextPapyros.API/Controllers/EntidadesController.cs
[ApiController]
[Route("entidades")]
public class EntidadesController(
    IEntidadRepository repo,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EntidadResponse>> Crear(
        [FromBody] CrearEntidadRequest req, 
        CancellationToken ct)
    {
        try
        {
            await unitOfWork.BeginAsync(ct);
            
            var entidad = new Entidad 
            { 
                Nombre = req.Nombre,
                Descripcion = req.Descripcion 
            };
            
            await repo.AddAsync(entidad, ct);
            await unitOfWork.CommitAsync(ct);
            
            return CreatedAtAction(nameof(Obtener), 
                new { id = entidad.Id }, 
                new EntidadResponse(entidad.Id, entidad.Nombre, entidad.Descripcion));
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
```

### Validaciones de Negocio

✅ **Validar antes de iniciar transacción**:
```csharp
if (string.IsNullOrWhiteSpace(req.Nombre))
    return BadRequest("El nombre es obligatorio.");

// Validar duplicados
var existente = await repo.GetByNombreAsync(req.Nombre, ct);
if (existente is not null)
    return Conflict("Ya existe una entidad con ese nombre.");
```

### Usar Unit of Work para Operaciones Complejas

Usa `IUnitOfWork` cuando necesites:
- ✅ Crear/actualizar múltiples entidades
- ✅ Modificar stock de inventario
- ✅ Registrar movimientos o logs
- ✅ Garantizar atomicidad (todo o nada)

**NO uses** Unit of Work para:
- ❌ Consultas simples (GET)
- ❌ Operaciones de una sola entidad sin relaciones

---

## 🐛 Troubleshooting

### Error: "Cannot connect to SQL Server"

**Windows**:
```bash
# Verificar que SQL Server esté ejecutándose
Get-Service | Where-Object {$_.Name -like "*SQL*"}
```

**macOS/Linux**:
```bash
# Verificar contenedor Docker
docker ps
docker logs sqlserver-nextpapyros
```

### Error: "Login failed for user"

Verifica la cadena de conexión en `appsettings.Development.json` y que la contraseña sea correcta.

### Error al aplicar migraciones

```bash
# Eliminar base de datos y recrear
dotnet ef database drop --force
dotnet ef database update
```

### Puerto en uso

Cambia el puerto en `src/NextPapyros.API/Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:7037;http://localhost:5288"
```

---

## 📚 Recursos Adicionales

- **[Documentación de .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)**
- **[Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)**
- **[ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)**
- **[Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)**

---

<div align="center">

**¿Preguntas?** Abre un [Issue](https://github.com/NextPapyros/nextpapyros-backend-api/issues) o contacta al equipo.

</div>
# CONTAINER ID   IMAGE                                        STATUS
# abc123def456   mcr.microsoft.com/mssql/server:2019-latest   Up 2 minutes
```

#### 4. Comandos Útiles de Docker

```bash
# Ver logs del contenedor
docker logs sqlserver-nextpapyros

# Detener el contenedor
docker stop sqlserver-nextpapyros

# Iniciar el contenedor
docker start sqlserver-nextpapyros

# Eliminar el contenedor (¡CUIDADO! Perderás los datos)
docker rm -f sqlserver-nextpapyros

# Con Docker Compose
docker-compose ps        # Ver estado
docker-compose logs -f   # Ver logs en tiempo real
docker-compose down      # Detener servicios
docker-compose down -v   # Detener y eliminar volúmenes
```

#### 5. Configurar Cadena de Conexión

Edita `src/NextPapyros.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=NextPapyrosDb;User Id=sa;Password=Password123*;TrustServerCertificate=True;"
  }
}
```

> ⚠️ **Nota sobre la contraseña**: La contraseña debe cumplir los requisitos de SQL Server:
> - Mínimo 8 caracteres
> - Incluir mayúsculas, minúsculas, números y símbolos

</details>

---

### Paso 4: Aplicar Migraciones de Base de Datos

Las migraciones de Entity Framework crearán automáticamente todas las tablas necesarias.

```bash
# Navegar al proyecto API
cd src/NextPapyros.API

# Instalar herramienta EF (si no está instalada)
dotnet tool install --global dotnet-ef

# O actualizar si ya existe
dotnet tool update --global dotnet-ef

# Aplicar migraciones
dotnet ef database update
```

**Salida esperada:**
```
Applying migration '20251014201724_InitialCreate'.
Done.
```

**Verificar que la base de datos fue creada:**

- **Windows (SSMS)**: Conéctate a tu instancia y verifica que existe `NextPapyrosDb`
- **Docker**: 
  ```bash
  docker exec -it sqlserver-nextpapyros /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "Password123*" \
    -Q "SELECT name FROM sys.databases WHERE name = 'NextPapyrosDb'"
  ```

---

### Paso 5: Ejecutar el Proyecto

#### Opción A: Línea de Comandos

```bash
# Desde src/NextPapyros.API/
dotnet run

# O especificar perfil
dotnet run --launch-profile https
```

#### Opción B: Visual Studio 2022

1. Abre `NextPapyros.sln`
2. Establece `NextPapyros.API` como proyecto de inicio
   - Clic derecho en el proyecto > "Set as Startup Project"
3. Presiona `F5` o haz clic en "▶ Start"

#### Opción C: Visual Studio Code

1. Abre la carpeta del proyecto
2. Instala la extensión "C# Dev Kit"
3. Presiona `F5` o ve a "Run and Debug"

#### Opción D: JetBrains Rider

1. Abre `NextPapyros.sln`
2. Selecciona la configuración `NextPapyros.API`
3. Presiona `Shift + F10` o haz clic en "Run"

---

### Paso 6: Verificar que Funcione

La aplicación debería estar ejecutándose en:

- **HTTP**: [http://localhost:5288](http://localhost:5288)
- **HTTPS**: [https://localhost:7037](https://localhost:7037)
- **Swagger**: [http://localhost:5288/swagger](http://localhost:5288/swagger)

**Prueba el endpoint de salud:**
```bash
curl http://localhost:5288/
# Respuesta esperada: OK
```

**Prueba el login:**
```bash
curl -X POST http://localhost:5288/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"mail@mail.com","password":"Password123*"}'
```

---

## 🛠️ Troubleshooting

### Error: "Cannot connect to SQL Server"

**Windows:**
```powershell
# Verifica que SQL Server esté ejecutándose
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Si no está ejecutándose, inícialo
Start-Service MSSQLSERVER  # o MSSQL$SQLEXPRESS
```

**macOS/Linux:**
```bash
# Verifica que el contenedor esté activo
docker ps

# Si no está ejecutándose, inícialo
docker start sqlserver-nextpapyros

# Ver logs para diagnosticar
docker logs sqlserver-nextpapyros
```

### Error: "Login failed for user 'sa'"

**Causas comunes:**
1. Contraseña incorrecta en `appsettings.json`
2. La contraseña de Docker no cumple los requisitos

**Solución:**
```bash
# Elimina y recrea el contenedor con una nueva contraseña
docker rm -f sqlserver-nextpapyros
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Actualiza appsettings.json con la nueva contraseña
```

### Error: "A network-related or instance-specific error occurred"

**Verifica que el puerto 1433 esté disponible:**

```bash
# macOS/Linux
lsof -i :1433

# Windows (PowerShell)
netstat -ano | findstr :1433
```

**Si otro servicio está usando el puerto:**
- Detén ese servicio, o
- Cambia el puerto en Docker: `-p 1434:1433` y actualiza la cadena de conexión a `Server=localhost,1434`

### Error: "The term 'dotnet-ef' is not recognized"

```bash
# Instala las herramientas de EF
dotnet tool install --global dotnet-ef

# Si ya está instalado, actualízalo
dotnet tool update --global dotnet-ef

# Verifica la instalación
dotnet ef --version
```

### Error: "Unable to bind to https://localhost:7037"

**Causa**: Puerto ocupado o certificado HTTPS no configurado.

**Soluciones:**

1. **Ejecuta en modo HTTP:**
   ```bash
   dotnet run --launch-profile http
   ```

2. **Genera el certificado de desarrollo:**
   ```bash
   dotnet dev-certs https --trust
   ```

3. **Cambia el puerto** en `Properties/launchSettings.json`:
   ```json
   "applicationUrl": "https://localhost:7038;http://localhost:5289"
   ```

### Error: "No migrations were applied"

```bash
# Elimina la base de datos y vuelve a crearla
dotnet ef database drop --force
dotnet ef database update

# Si las migraciones están corruptas, elimínalas y recréalas
rm -rf ../NextPapyros.Infrastructure/Migrations/*
dotnet ef migrations add InitialCreate --project ../NextPapyros.Infrastructure
dotnet ef database update
```

### Problemas con Apple Silicon (M1/M2/M3)

Si el contenedor SQL Server no inicia en Macs con chip Apple:

**Opción 1: Emulación x86**
```bash
docker run --platform linux/amd64 \
  -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/mssql/server:2019-latest
```

**Opción 2: Azure SQL Edge (Nativo ARM)**
```bash
docker run --cap-add SYS_PTRACE \
  -e "ACCEPT_EULA=1" -e "MSSQL_SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/azure-sql-edge
```

---

## 📖 Guías de Desarrollo

### Crear una Nueva Migración

```bash
cd src/NextPapyros.API

# Agregar una nueva migración
dotnet ef migrations add NombreDeTuMigracion --project ../NextPapyros.Infrastructure

# Aplicar la migración
dotnet ef database update
```

### Rollback de Migraciones

```bash
# Volver a una migración específica
dotnet ef database update NombreMigracionAnterior

# Eliminar la última migración (si no ha sido aplicada)
dotnet ef migrations remove --project ../NextPapyros.Infrastructure
```

### Ejecutar con Hot Reload

```bash
dotnet watch run
# Los cambios en el código se aplicarán automáticamente
```

### Limpiar y Recompilar

```bash
# Limpiar artefactos de compilación
dotnet clean

# Recompilar
dotnet build

# O hacer ambas cosas
dotnet clean && dotnet build
```

---

## 🤝 Proceso de Contribución

### 1. Fork y Clonar

```bash
# Fork en GitHub, luego clona tu fork
git clone https://github.com/TU_USUARIO/nextpapyros-backend-api.git
cd nextpapyros-backend-api

# Agrega el repositorio original como upstream
git remote add upstream https://github.com/NextPapyros/nextpapyros-backend-api.git
```

### 2. Crear una Rama

```bash
# Actualiza tu main
git checkout main
git pull upstream main

# Crea una nueva rama
git checkout -b feature/mi-nueva-funcionalidad
# o
git checkout -b fix/correccion-de-bug
```

### 3. Hacer Cambios

- ✅ Escribe código limpio y documentado
- ✅ Sigue las convenciones de C# y .NET
- ✅ Agrega pruebas si es posible
- ✅ Actualiza documentación si es necesario

### 4. Commit

```bash
git add .
git commit -m "feat: agregar funcionalidad X"

# Tipos de commit:
# feat: nueva funcionalidad
# fix: corrección de bug
# docs: cambios en documentación
# style: cambios de formato (sin afectar código)
# refactor: refactorización de código
# test: agregar o corregir pruebas
# chore: tareas de mantenimiento
```

### 5. Push y Pull Request

```bash
# Push a tu fork
git push origin feature/mi-nueva-funcionalidad

# Luego abre un Pull Request en GitHub
```

### Estándares de Código

#### Nombres

```csharp
// ✅ BIEN: PascalCase para clases, métodos, propiedades
public class ProductoService
{
    public async Task<Producto> ObtenerProductoAsync(string codigo) { }
}

// ✅ BIEN: camelCase para variables locales y parámetros
var productoCodigo = "ABC123";
public void Procesar(string codigoProducto) { }

// ❌ MAL
public class producto_service  // snake_case
{
    public async Task<Producto> obtener_producto(string Codigo) { }
}
```

#### Comentarios XML

```csharp
/// <summary>
/// Obtiene un producto por su código único.
/// </summary>
/// <param name="codigo">El código único del producto.</param>
/// <param name="ct">Token de cancelación.</param>
/// <returns>El producto si existe, null en caso contrario.</returns>
/// <exception cref="ArgumentNullException">Si el código es null o vacío.</exception>
public async Task<Producto?> ObtenerProductoAsync(string codigo, CancellationToken ct)
{
    // Implementación
}
```

#### Organización de Usings

```csharp
// 1. System
using System;
using System.Collections.Generic;
using System.Linq;

// 2. Microsoft
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// 3. Third-party

// 4. Tu proyecto
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
```

---

## 📝 Checklist Pre-Commit

Antes de hacer commit, verifica:

- [ ] El código compila sin errores: `dotnet build`
- [ ] No hay advertencias críticas
- [ ] El código sigue las convenciones de estilo
- [ ] Agregaste comentarios XML para APIs públicas
- [ ] Actualizaste la documentación si es necesario
- [ ] Las pruebas pasan (si existen): `dotnet test`
- [ ] Probaste manualmente en Swagger

---

## 🎯 Recursos Adicionales

### Documentación

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Herramientas Útiles

- **Azure Data Studio**: Cliente multiplataforma para SQL Server
  - [Descargar](https://docs.microsoft.com/sql/azure-data-studio/)
  
- **Postman**: Cliente REST alternativo a Swagger
  - [Descargar](https://www.postman.com/downloads/)

- **Docker Desktop**: Para gestión visual de contenedores
  - Ya instalado si sigues esta guía

### Comunidad

- [.NET Community](https://dotnet.microsoft.com/platform/community)
- [Stack Overflow - .NET](https://stackoverflow.com/questions/tagged/.net)
- [Reddit - r/dotnet](https://reddit.com/r/dotnet)

---

## 💡 Consejos

1. **Usa Hot Reload**: `dotnet watch run` para ver cambios en tiempo real
2. **Aprovecha Swagger**: Prueba los endpoints directamente desde el navegador
3. **Usa Git correctamente**: Commits pequeños y frecuentes
4. **Lee los logs**: Contienen información valiosa para debugging
5. **Pregunta**: Si tienes dudas, abre una [discusión](https://github.com/NextPapyros/nextpapyros-backend-api/discussions)

---

**¡Gracias por contribuir a NextPapyros! 🎉**

Si tienes problemas que no están cubiertos en esta guía, por favor [abre un issue](https://github.com/NextPapyros/nextpapyros-backend-api/issues).
