# 🧩 NextPapyros Backend API# 🧩 NextPapyros Backend API



Sistema de gestión empresarial para **inventario, ventas, compras, recepciones y devoluciones**, desarrollado con **.NET 8** siguiendo los principios de **Clean Architecture** y **SOLID**.**NextPapyros** es una aplicación modular diseñada bajo los principios de **Clean Architecture** y **SOLID**, utilizando **.NET 8**, **C#**, **Entity Framework Core** y **SQL Server**.  

El objetivo del sistema es **gestionar inventario, ventas, compras, devoluciones y generación de reportes** de manera limpia, escalable y mantenible.

---

---

## 🚀 Características Principales

## 📋 Tabla de Contenidos

- 🔐 **Autenticación JWT** con sistema de roles y permisos

- 📦 **Gestión de Inventario** con control de stock y alertas- [Tecnologías Principales](#-tecnologías-principales)

- 🛒 **Órdenes de Compra** y recepciones de mercancía- [Estructura del Proyecto](#-estructura-del-proyecto)

- 💰 **Registro de Ventas** con múltiples métodos de pago- [Funcionalidades](#-funcionalidades)

- 🔄 **Devoluciones** con trazabilidad completa- [Requisitos Previos](#-requisitos-previos)

- 📊 **Reportes y Analytics** (top productos, stock bajo, ingresos mensuales)- [Instalación y Configuración](#-instalación-y-configuración)

- 📝 **Auditoría** con logs de operaciones  - [1. Clonar el Repositorio](#1-clonar-el-repositorio)

  - [2. Configuración de la Base de Datos](#2-configuración-de-la-base-de-datos)

---  - [3. Configurar la Cadena de Conexión](#3-configurar-la-cadena-de-conexión)

  - [4. Aplicar Migraciones](#4-aplicar-migraciones)

## 🛠️ Tecnologías  - [5. Ejecutar el Proyecto](#5-ejecutar-el-proyecto)

- [Configuración Específica por Sistema Operativo](#-configuración-específica-por-sistema-operativo)

| Tecnología | Versión | Descripción |  - [Windows (SQL Server Local)](#windows-sql-server-local)

|------------|---------|-------------|  - [macOS / Linux (Docker)](#macos--linux-docker)

| **.NET** | 8.0 | Framework principal |- [Endpoints y Documentación API](#-endpoints-y-documentación-api)

| **Entity Framework Core** | 9.0 | ORM y migraciones |- [Usuario Administrador por Defecto](#-usuario-administrador-por-defecto)

| **SQL Server** | 2019+ | Base de datos |- [Troubleshooting](#-troubleshooting)

| **JWT Bearer** | 8.0 | Autenticación |- [Contribución](#-contribución)

| **Swagger/OpenAPI** | 6.x | Documentación API |

| **BCrypt.Net** | 4.0 | Hashing de contraseñas |---



---## 🚀 Tecnologías Principales



## 🏗️ Arquitectura| Tecnología | Versión | Descripción |

|-------------|---------|-------------|

El proyecto sigue **Clean Architecture** con separación en capas:| **.NET** | 8.0 | Framework principal para el desarrollo del backend |

| **C#** | 12 | Lenguaje de programación |

```plaintext| **Entity Framework Core** | 9.0 | ORM para acceso a datos y migraciones |

┌─────────────────────────────────────────┐| **SQL Server** | 2019+ | Base de datos relacional |

│         NextPapyros.API                 │  ← Capa de Presentación| **JWT Bearer** | 8.0 | Autenticación basada en tokens |

│  (Controllers, DTOs, Middleware)        │| **Swagger/OpenAPI** | 6.x | Documentación interactiva de la API |

└─────────────────────────────────────────┘| **BCrypt.Net** | 4.0 | Hashing seguro de contraseñas |

                  ↓

┌─────────────────────────────────────────┐### Arquitectura y Patrones

│      NextPapyros.Application            │  ← Capa de Aplicación

│      (Casos de Uso, Services)           │- **Clean Architecture**: Separación en capas independientes

└─────────────────────────────────────────┘- **Principios SOLID**: Código limpio, extensible y mantenible

                  ↓- **Patrones de Diseño**: Repository, Unit of Work, Domain Services

┌─────────────────────────────────────────┐- **DDD (Domain-Driven Design)**: Modelado basado en el dominio del negocio

│        NextPapyros.Domain               │  ← Capa de Dominio

│  (Entities, Interfaces, Lógica)         │---

└─────────────────────────────────────────┘

                  ↑## 🧱 Estructura del Proyecto

┌─────────────────────────────────────────┐

│     NextPapyros.Infrastructure          │  ← Capa de Infraestructura```plaintext

│  (EF Core, Repos, Auth, Migrations)     │nextpapyros-backend-api/

└─────────────────────────────────────────┘├── src/

```│   ├── NextPapyros.API/              # 🌐 Capa de Presentación

│   │   ├── Controllers/              # Controladores REST

### Patrones Implementados│   │   ├── Contracts/                # DTOs y contratos de API

│   │   ├── Startup/                  # Configuración inicial (Seeder)

- **Repository Pattern**: Abstracción de acceso a datos│   │   ├── appsettings.json          # Configuración general

- **Unit of Work**: Gestión de transacciones│   │   └── Program.cs                # Punto de entrada

- **Dependency Injection**: Inversión de dependencias│   │

- **Domain Services**: Lógica de negocio compleja│   ├── NextPapyros.Application/      # 📦 Capa de Aplicación

│   │   └── (Casos de uso y lógica de aplicación)

---│   │

│   ├── NextPapyros.Domain/           # 🎯 Capa de Dominio

## 📦 Estructura del Proyecto│   │   ├── Entities/                 # Entidades de negocio

│   │   │   ├── Producto.cs

```plaintext│   │   │   ├── Proveedor.cs

nextpapyros-backend-api/│   │   │   ├── OrdenCompra.cs

├── src/│   │   │   ├── Recepcion.cs

│   ├── NextPapyros.API/              # 🌐 Presentación│   │   │   ├── Venta.cs

│   │   ├── Controllers/              # Endpoints REST│   │   │   ├── Devolucion.cs

│   │   ├── Contracts/                # DTOs Request/Response│   │   │   ├── Usuario.cs

│   │   ├── Startup/                  # DbSeeder│   │   │   ├── Rol.cs

│   │   └── Program.cs                # Configuración│   │   │   └── ...

│   ││   │   └── Repositories/             # Interfaces de repositorios

│   ├── NextPapyros.Application/      # 📦 Aplicación│   │

│   ││   └── NextPapyros.Infrastructure/   # 🔧 Capa de Infraestructura

│   ├── NextPapyros.Domain/           # 🎯 Dominio│       ├── Auth/                     # Autenticación y seguridad

│   │   ├── Entities/                 # Modelos de negocio│       ├── Persistence/              # DbContext y configuración EF

│   │   └── Repositories/             # Interfaces│       ├── Repositories/             # Implementación de repositorios

│   ││       └── Migrations/               # Migraciones de base de datos

│   └── NextPapyros.Infrastructure/   # 🔧 Infraestructura│

│       ├── Auth/                     # JWT, BCrypt├── NextPapyros.sln                   # Solución de Visual Studio

│       ├── Persistence/              # DbContext└── README.md                         # Este archivo

│       ├── Repositories/             # Implementaciones```

│       └── Migrations/               # EF Migrations

│---

└── NextPapyros.sln

```## ✨ Funcionalidades



---### 🔐 Autenticación y Autorización

- Sistema de autenticación basado en **JWT (JSON Web Tokens)**

## 🚀 Inicio Rápido- Gestión de **usuarios, roles y permisos**

- Hashing seguro de contraseñas con **BCrypt**

### Requisitos- Autorización basada en roles



- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)### 📦 Gestión de Inventario

- SQL Server (Windows) o Docker (macOS/Linux)- Administración de **productos** (código, nombre, categoría, stock)

- Control de **proveedores** y relación productos-proveedores

### Instalación Rápida- Registro de **movimientos de inventario**



```bash### 🛒 Compras y Recepciones

# 1. Clonar repositorio- Creación y seguimiento de **órdenes de compra**

git clone https://github.com/NextPapyros/nextpapyros-backend-api.git- Gestión de **recepciones** de mercancía

cd nextpapyros-backend-api- Estados de órdenes: Pendiente, Confirmada, Recibida, Cancelada



# 2. Configurar base de datos (Docker en macOS/Linux)### 💰 Ventas

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=ReplacePasswordHere" \- Registro de **ventas** con múltiples líneas

  -p 1433:1433 --name sqlserver-nextpapyros \- Soporte para diferentes **métodos de pago** (Efectivo, Tarjeta, Transferencia, etc.)

  -d mcr.microsoft.com/mssql/server:2019-latest- Estados de ventas: Confirmada, Anulada



# 3. Aplicar migraciones### 🔄 Devoluciones

cd src/NextPapyros.API- Gestión de **devoluciones de ventas**

dotnet ef database update- Control de líneas devueltas con cantidades

- Estados: Pendiente, Aprobada, Rechazada

# 4. Ejecutar

dotnet run### 📊 Auditoría

```- **Log de operaciones** para trazabilidad

- Registro de acciones críticas del sistema

**📖 Para instrucciones detalladas**, consulta **[CONTRIBUTING.md](CONTRIBUTING.md)**.

---

---

## 📋 Requisitos Previos

## 📚 Documentación API

Antes de comenzar, asegúrate de tener instalado lo siguiente:

### Swagger UI

### Requisitos Comunes (Todos los Sistemas Operativos)

Accede a la documentación interactiva en:

1. **.NET 8 SDK**

- **Desarrollo**: [http://localhost:5288/swagger](http://localhost:5288/swagger)   - Descarga: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

- **HTTPS**: [https://localhost:7037/swagger](https://localhost:7037/swagger)   - Verifica la instalación:

     ```bash

### Autenticación     dotnet --version

     ```

1. **Login** con las credenciales por defecto:     Deberías ver algo como `8.0.x`

   ```json

   POST /auth/login2. **Git**

   {   - Descarga: [https://git-scm.com/downloads](https://git-scm.com/downloads)

     "email": "mail@mail.com",

     "password": "mailPassword"3. **Editor de Código** (Recomendado)

   }   - [Visual Studio 2022](https://visualstudio.microsoft.com/) (Windows/macOS)

   ```   - [Visual Studio Code](https://code.visualstudio.com/) (Todos los OS) + extensión C#

   - [JetBrains Rider](https://www.jetbrains.com/rider/) (Todos los OS)

2. **Copia el token** de la respuesta

### Requisitos Específicos según Sistema Operativo

3. **Autoriza en Swagger**: Click en "Authorize" → `Bearer {tu-token}`

#### Windows

### Endpoints Principales- **SQL Server 2019 o superior** (Express, Developer o Enterprise)

  - Descarga SQL Server: [https://www.microsoft.com/sql-server/sql-server-downloads](https://www.microsoft.com/sql-server/sql-server-downloads)

| Módulo | Endpoint | Método | Descripción |  - Descarga SQL Server Management Studio (SSMS): [https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)

|--------|----------|--------|-------------|

| **Auth** | `/auth/login` | POST | Iniciar sesión |#### macOS / Linux

| **Auth** | `/auth/register` | POST | Registrar usuario (Admin) |- **Docker Desktop**

| **Productos** | `/products` | GET | Listar productos |  - macOS: [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)

| **Productos** | `/products` | POST | Crear producto |  - Linux: [https://docs.docker.com/engine/install/](https://docs.docker.com/engine/install/)

| **Productos** | `/products/{codigo}` | GET | Obtener producto |

| **Ventas** | `/ventas` | POST | Registrar venta |---

| **Ventas** | `/ventas/{id}` | GET | Consultar venta |

| **Recepciones** | `/recepciones` | POST | Registrar recepción |## 🚀 Instalación y Configuración

| **Reportes** | `/reportes/top-productos` | GET | Top productos vendidos |

| **Reportes** | `/reportes/stock-bajo` | GET | Productos con stock bajo |### 1. Clonar el Repositorio

| **Reportes** | `/reportes/ingresos-mensuales` | GET | Ingresos por mes |

```bash

---git clone https://github.com/NextPapyros/nextpapyros-backend-api.git

cd nextpapyros-backend-api

## 👤 Usuario Administrador```



Al iniciar la aplicación por primera vez, se crea automáticamente:### 2. Configuración de la Base de Datos



| Campo | Valor |Elige la opción según tu sistema operativo:

|-------|-------|

| Email | `mail@mail.com` |#### Opción A: Windows (SQL Server Local)

| Password | `mailPassword` |

| Rol | Administrador |Si tienes SQL Server instalado localmente en Windows:



⚠️ **Cambia estas credenciales en producción**.1. **Asegúrate de que SQL Server esté ejecutándose**

   - Abre SQL Server Configuration Manager

---   - Verifica que el servicio SQL Server esté activo



## 🧪 Pruebas2. **Crea la base de datos** (Opcional - EF lo hará automáticamente)

   - Abre SQL Server Management Studio (SSMS)

```bash   - Conéctate a tu instancia local (generalmente `localhost` o `.\SQLEXPRESS`)

# Ejecutar todas las pruebas   - La base de datos `NextPapyrosDb` se creará automáticamente al aplicar las migraciones

dotnet test

3. **Toma nota de tu cadena de conexión**

# Con cobertura   - Instancia por defecto: `Server=localhost;Database=NextPapyrosDb;Trusted_Connection=True;TrustServerCertificate=True;`

dotnet test /p:CollectCoverage=true   - SQL Server Express: `Server=localhost\SQLEXPRESS;Database=NextPapyrosDb;Trusted_Connection=True;TrustServerCertificate=True;`

```

#### Opción B: macOS / Linux (Docker)

---

Si estás en macOS o Linux, usa Docker para ejecutar SQL Server:

## 📖 Documentación Adicional

1. **Asegúrate de que Docker esté instalado y ejecutándose**

- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Guía completa de instalación y configuración   ```bash

- **[Swagger UI](http://localhost:5288/swagger)** - Documentación interactiva de la API   docker --version

- **[Issues](https://github.com/NextPapyros/nextpapyros-backend-api/issues)** - Reportar bugs o solicitar features   # Deberías ver algo como: Docker version 24.x.x

   ```

---

2. **Ejecuta SQL Server en un contenedor Docker**

## 🤝 Contribuir   ```bash

   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=ReplaceYourPasswordHere" \

¡Las contribuciones son bienvenidas! Por favor:     -p 1433:1433 --name sqlserver-nextpapyros \

     -d mcr.microsoft.com/mssql/server:2019-latest

1. Lee **[CONTRIBUTING.md](CONTRIBUTING.md)** para instrucciones detalladas   ```

2. Haz fork del proyecto

3. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)   > **Nota sobre la contraseña**: La contraseña `***********` debe cumplir con los requisitos de SQL Server (mayúsculas, minúsculas, números y caracteres especiales). Puedes cambiarla, pero recuerda actualizarla en `appsettings.json`.

4. Commit tus cambios (`git commit -m 'Add AmazingFeature'`)

5. Push a la rama (`git push origin feature/AmazingFeature`)3. **Verifica que el contenedor esté ejecutándose**

6. Abre un Pull Request   ```bash

   docker ps

### Estándares de Código   ```

   Deberías ver `sqlserver-nextpapyros` en la lista.

- ✅ Sigue los principios **SOLID**

- ✅ Mantén la **separación de responsabilidades**4. **Comandos útiles de Docker**

- ✅ Usa **nombres descriptivos** en inglés   ```bash

- ✅ Añade **comentarios XML** para métodos públicos   # Detener el contenedor

- ✅ Escribe **pruebas** para nuevas funcionalidades   docker stop sqlserver-nextpapyros



---   # Iniciar el contenedor

   docker start sqlserver-nextpapyros

## 🗺️ Roadmap

   # Ver logs del contenedor

- [ ] Pruebas unitarias e integración   docker logs sqlserver-nextpapyros

- [ ] Soporte para PostgreSQL

- [ ] Implementación de CQRS   # Eliminar el contenedor (¡perderás los datos!)

- [ ] GraphQL API   docker rm -f sqlserver-nextpapyros

- [ ] Eventos de dominio   ```

- [ ] Cache distribuido con Redis

- [ ] Frontend React/Angular### 3. Configurar la Cadena de Conexión

- [ ] API de reportes avanzados

- [ ] Internacionalización (i18n)Edita el archivo `src/NextPapyros.API/appsettings.json` o `appsettings.Development.json`:

- [ ] Containerización completa

- [ ] CI/CD con GitHub Actions**Para Windows (Autenticación de Windows):**

```json

---{

  "ConnectionStrings": {

## 📄 Licencia    "Default": "Server=localhost;Database=NextPapyrosDb;Trusted_Connection=True;TrustServerCertificate=True;"

  }

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.}

```

---

**Para Windows (Autenticación SQL):**

## 📞 Soporte```json

{

- 🐛 **Bugs**: [GitHub Issues](https://github.com/NextPapyros/nextpapyros-backend-api/issues)  "ConnectionStrings": {

- 💬 **Discusiones**: [GitHub Discussions](https://github.com/NextPapyros/nextpapyros-backend-api/discussions)    "Default": "Server=localhost;Database=NextPapyrosDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"

- 📧 **Email**: soporte@nextpapyros.com  }

}

---```



<div align="center">**Para macOS/Linux (Docker):**

```json

**Hecho con ❤️ por el equipo de NextPapyros**{

  "ConnectionStrings": {

[⬆ Volver arriba](#-nextpapyros-backend-api)    "Default": "Server=localhost,1433;Database=NextPapyrosDb;User Id=sa;Password=ReplaceYourPasswordHere;TrustServerCertificate=True;"

  }

</div>}

```

> ⚠️ **Importante**: No subas contraseñas reales a repositorios públicos. En producción, usa variables de entorno o servicios de gestión de secretos.

### 4. Aplicar Migraciones

Las migraciones de Entity Framework crearán todas las tablas necesarias en la base de datos.

```bash
# Navega al directorio del proyecto API
cd src/NextPapyros.API

# Aplica las migraciones
dotnet ef database update

# Si el comando anterior falla, instala la herramienta EF:
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Deberías ver un mensaje similar a:
```
Applying migration '20251014201724_InitialCreate'.
Done.
```

### 5. Ejecutar el Proyecto

```bash
# Desde src/NextPapyros.API/
dotnet run

# O si prefieres especificar el perfil de desarrollo:
dotnet run --launch-profile https
```

La aplicación se ejecutará en:
- **HTTP**: `http://localhost:5288`
- **HTTPS**: `https://localhost:7037`

Deberías ver un mensaje similar a:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7037
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5288
```

---

## 🖥️ Configuración Específica por Sistema Operativo

### Windows (SQL Server Local)

#### Instalación de SQL Server Express (Gratuito)

1. **Descarga SQL Server 2019 Express**
   - [Enlace de descarga](https://www.microsoft.com/sql-server/sql-server-downloads)

2. **Ejecuta el instalador**
   - Selecciona "Basic" o "Custom"
   - Sigue el asistente de instalación
   - Toma nota del nombre de la instancia (por ejemplo, `SQLEXPRESS`)

3. **Instala SQL Server Management Studio (SSMS)**
   - [Enlace de descarga](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
   - SSMS te permitirá administrar la base de datos visualmente

4. **Habilita TCP/IP (si es necesario)**
   - Abre "SQL Server Configuration Manager"
   - Ve a "SQL Server Network Configuration" > "Protocols for [tu instancia]"
   - Habilita "TCP/IP"
   - Reinicia el servicio SQL Server

5. **Configura el proyecto**
   - Usa la cadena de conexión de Windows (ver sección 3 arriba)
   - Aplica las migraciones

#### Ejecutar con Visual Studio

1. Abre `NextPapyros.sln` con Visual Studio 2022
2. Establece `NextPapyros.API` como proyecto de inicio (clic derecho > Set as Startup Project)
3. Presiona `F5` o haz clic en "Start Debugging"

### macOS / Linux (Docker)

#### Configuración Completa con Docker

1. **Instala Docker Desktop**
   - macOS: [Docker Desktop para Mac](https://www.docker.com/products/docker-desktop)
   - Linux: [Docker Engine](https://docs.docker.com/engine/install/)

2. **Crea un archivo `docker-compose.yml`** (opcional, para mayor comodidad)

   En la raíz del proyecto, crea `docker-compose.yml`:

   ```yaml
   version: '3.8'
   
   services:
     sqlserver:
       image: mcr.microsoft.com/mssql/server:2019-latest
       container_name: nextpapyros-sqlserver
       environment:
         - ACCEPT_EULA=Y
         - SA_PASSWORD=ReplaceYourPasswordHere
         - MSSQL_PID=Express
       ports:
         - "1433:1433"
       volumes:
         - sqlserver-data:/var/opt/mssql
   
   volumes:
     sqlserver-data:
   ```

3. **Inicia SQL Server con Docker Compose**
   ```bash
   docker-compose up -d
   ```

4. **Verifica el estado**
   ```bash
   docker-compose ps
   ```

5. **Detén los servicios**
   ```bash
   docker-compose down
   # Para eliminar también los datos:
   docker-compose down -v
   ```

#### Ejecutar con VS Code

1. Abre el proyecto en VS Code
2. Instala la extensión "C# Dev Kit" o "C#"
3. Abre una terminal integrada (`Ctrl+` ` o `Cmd+` `)
4. Ejecuta:
   ```bash
   cd src/NextPapyros.API
   dotnet run
   ```

#### Ejecutar con Rider (JetBrains)

1. Abre `NextPapyros.sln` con Rider
2. Selecciona la configuración de ejecución `NextPapyros.API`
3. Haz clic en el botón "Run" o presiona `Ctrl+R` (Windows/Linux) / `Cmd+R` (macOS)

---

## 📚 Endpoints y Documentación API

### Swagger UI

Una vez que el proyecto esté ejecutándose, accede a la documentación interactiva:

**URL**: [http://localhost:5288/swagger](http://localhost:5288/swagger)  
o  
**URL**: [https://localhost:7037/swagger](https://localhost:7037/swagger)

Swagger te permite:
- Ver todos los endpoints disponibles
- Probar las APIs directamente desde el navegador
- Ver los modelos de datos (DTOs)
- Autenticarte con JWT

### Autenticación

1. **Obtén un token JWT**
   - Endpoint: `POST /api/auth/login`
   - Cuerpo:
     ```json
     {
       "email": "mail@mail.com",
       "password": "mailPassword"
     }
     ```
   - Respuesta:
     ```json
     {
       "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
     }
     ```

2. **Autoriza en Swagger**
   - Haz clic en el botón "Authorize" (candado) en la parte superior derecha
   - Ingresa: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
   - Haz clic en "Authorize"

3. **Usa los endpoints protegidos**
   - Ahora puedes probar cualquier endpoint que requiera autenticación

### Principales Endpoints

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/login` | Iniciar sesión | No |
| `GET` | `/api/productos` | Listar productos | Sí |
| `POST` | `/api/productos` | Crear producto | Sí |
| `GET` | `/api/proveedores` | Listar proveedores | Sí |
| `POST` | `/api/ordenes-compra` | Crear orden de compra | Sí |
| `POST` | `/api/ventas` | Registrar venta | Sí |
| `POST` | `/api/devoluciones` | Crear devolución | Sí |

> **Nota**: La lista completa de endpoints está disponible en Swagger.

---

## 👤 Usuario Administrador por Defecto

El sistema crea automáticamente un usuario administrador al iniciar por primera vez (mediante `DbSeeder`):

| Campo | Valor |
|-------|-------|
| **Email** | `mail@mail.com` |
| **Password** | `mailPassword` |
| **Rol** | Administrador |

> ⚠️ **Seguridad**: Cambia estas credenciales en producción. Este usuario está destinado solo para desarrollo y pruebas.

---

## 🛠️ Troubleshooting

### Error: "Cannot connect to SQL Server"

**Problema**: No se puede conectar a la base de datos.

**Soluciones**:

1. **Windows**: Verifica que SQL Server esté ejecutándose
   ```powershell
   # En PowerShell (como administrador)
   Get-Service | Where-Object {$_.Name -like "*SQL*"}
   ```

2. **macOS/Linux**: Verifica que el contenedor Docker esté activo
   ```bash
   docker ps
   # Si no está ejecutándose:
   docker start sqlserver-nextpapyros
   ```

3. **Verifica la cadena de conexión** en `appsettings.json`

4. **Prueba la conexión manualmente**:
   - Windows: Usa SSMS para conectarte
   - macOS/Linux: Usa Azure Data Studio o una herramienta como DBeaver

### Error: "Login failed for user 'sa'"

**Problema**: Contraseña incorrecta o usuario no autorizado.

**Soluciones**:

1. Verifica que la contraseña en `appsettings.json` coincida con la del servidor SQL
2. Si usas Docker, asegúrate de que la contraseña cumple los requisitos de SQL Server
3. Si cambiaste la contraseña del contenedor, recréalo:
   ```bash
   docker rm -f sqlserver-nextpapyros
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=NuevaPassword123*" \
     -p 1433:1433 --name sqlserver-nextpapyros \
     -d mcr.microsoft.com/mssql/server:2019-latest
   ```

### Error: "A network-related or instance-specific error occurred"

**Problema**: SQL Server no está escuchando en el puerto correcto.

**Soluciones**:

1. Verifica que el puerto 1433 no esté bloqueado por el firewall
2. En Windows, habilita TCP/IP en SQL Server Configuration Manager
3. Verifica que no haya otro servicio usando el puerto 1433:
   ```bash
   # macOS/Linux
   lsof -i :1433
   
   # Windows (PowerShell)
   netstat -ano | findstr :1433
   ```

### Error: "The term 'dotnet-ef' is not recognized"

**Problema**: Las herramientas de Entity Framework no están instaladas.

**Solución**:
```bash
dotnet tool install --global dotnet-ef
# O actualiza si ya está instalado:
dotnet tool update --global dotnet-ef
```

### Error: "No migrations were applied"

**Problema**: Las migraciones no se aplicaron correctamente.

**Soluciones**:

1. Verifica que la base de datos existe y es accesible
2. Elimina la base de datos y vuelve a crear:
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```
3. Si las migraciones están corruptas, regéneralas:
   ```bash
   # ⚠️ Esto eliminará las migraciones existentes
   rm -rf src/NextPapyros.Infrastructure/Migrations/*
   dotnet ef migrations add InitialCreate --project src/NextPapyros.Infrastructure --startup-project src/NextPapyros.API
   dotnet ef database update --project src/NextPapyros.Infrastructure --startup-project src/NextPapyros.API
   ```

### Error al ejecutar en macOS con Apple Silicon (M1/M2)

**Problema**: El contenedor de SQL Server no es compatible con arquitectura ARM.

**Solución**:

1. Usa la emulación x86:
   ```bash
   docker run --platform linux/amd64 \
     -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=ReplaceYourPasswordHere" \
     -p 1433:1433 --name sqlserver-nextpapyros \
     -d mcr.microsoft.com/mssql/server:2019-latest
   ```

2. O considera usar Azure SQL Edge (compatible con ARM):
   ```bash
   docker run --cap-add SYS_PTRACE \
     -e "ACCEPT_EULA=1" -e "MSSQL_SA_PASSWORD=ReplaceYourPasswordHere" \
     -p 1433:1433 --name sqlserver-nextpapyros \
     -d mcr.microsoft.com/azure-sql-edge
   ```

### Error: "Unable to bind to https://localhost:7037"

**Problema**: El puerto ya está en uso o hay problemas con el certificado HTTPS.

**Soluciones**:

1. Ejecuta en modo HTTP:
   ```bash
   dotnet run --launch-profile http
   ```

2. Genera el certificado de desarrollo:
   ```bash
   dotnet dev-certs https --trust
   ```

3. Cambia el puerto en `launchSettings.json` si está ocupado.

---

## 🤝 Contribución

¡Las contribuciones son bienvenidas! Si deseas contribuir:

1. Haz un fork del repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Realiza tus cambios y commits (`git commit -m 'Add some AmazingFeature'`)
4. Haz push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

### Guías de Estilo

- Sigue los principios SOLID
- Mantén la separación de responsabilidades entre capas
- Escribe código limpio y autodocumentado
- Añade comentarios XML en métodos públicos
- Usa nombres descriptivos en inglés para clases y métodos

---

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

---

## 📞 Contacto y Soporte

Si tienes problemas, preguntas o sugerencias:

- **Issues**: [GitHub Issues](https://github.com/NextPapyros/nextpapyros-backend-api/issues)
- **Discusiones**: [GitHub Discussions](https://github.com/NextPapyros/nextpapyros-backend-api/discussions)

---

## 🎯 Roadmap

- [ ] Implementar pruebas unitarias e integración
- [ ] Añadir soporte para PostgreSQL
- [ ] Crear frontend con React/Angular
- [ ] Implementar API de reportes avanzados
- [ ] Añadir soporte para múltiples idiomas
- [ ] Dockerizar la aplicación completa
- [ ] Implementar CI/CD con GitHub Actions

---

**¡Gracias por usar NextPapyros! 🎉**
