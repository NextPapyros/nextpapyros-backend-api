# 🧩 NextPapyros Backend API

Sistema de gestión empresarial para **inventario, ventas, compras, recepciones y proveedores**, desarrollado con **.NET 8** siguiendo los principios de **Clean Architecture** y **SOLID**.

---

## 🚀 Características Principales

- 🔐 **Autenticación JWT** con sistema de roles y permisos
- 🔑 **Recuperación de Contraseña** por correo electrónico con tokens seguros
- 👥 **Gestión de Empleados** con roles y control de acceso (Admin)
- 📦 **Gestión de Inventario** con control de stock y movimientos
- 🏢 **Gestión de Proveedores** con validación de duplicados
- 🛒 **Órdenes de Compra** y recepciones de mercancía
- 💰 **Registro de Ventas** con actualización automática de stock
- 📄 **Comprobantes PDF** generados automáticamente por venta
- 🔄 **Devoluciones** con trazabilidad completa
- 📊 **Reportes** exportables a CSV/PDF (top productos, stock bajo, ingresos mensuales)
- 📝 **Auditoría** con logs de operaciones
- 🔒 **Transacciones atómicas** con patrón Unit of Work
- 📧 **Envío de correos** con MailKit y SMTP

---

## 🛠️ Tecnologías

| Tecnología | Versión | Descripción |
|------------|---------|-------------|
| **.NET** | 8.0 | Framework principal |
| **Entity Framework Core** | 9.0 | ORM y migraciones |
| **PostgreSQL** | 16+ | Base de datos |
| **Npgsql** | 9.0 | Provider de PostgreSQL |
| **JWT Bearer** | 8.0 | Autenticación |
| **Swagger/OpenAPI** | 6.x | Documentación API |
| **BCrypt.Net** | 4.0 | Hashing de contraseñas |
| **QuestPDF** | 2024.10 | Generación de PDFs |
| **MailKit** | 4.14 | Envío de correos SMTP |

---

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con separación en capas:

```plaintext
┌─────────────────────────────────────────┐
│         NextPapyros.API                 │  ← Presentación
│  (Controllers, DTOs, Middleware)        │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      NextPapyros.Application            │  ← Aplicación
│      (Interfaces, Use Cases)            │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        NextPapyros.Domain               │  ← Dominio
│  (Entities, Interfaces, Lógica)         │
└─────────────────────────────────────────┘
                  ↑
┌─────────────────────────────────────────┐
│     NextPapyros.Infrastructure          │  ← Infraestructura
│  (EF Core, Repos, Auth, Migrations)     │
└─────────────────────────────────────────┘
```

### Patrones Implementados

- **Repository Pattern**: Abstracción de acceso a datos
- **Unit of Work**: Gestión de transacciones atómicas
- **Dependency Injection**: Inversión de control con ASP.NET Core IoC
- **Strategy Pattern**: Exportación dinámica de reportes (CSV, PDF)
- **Singleton Pattern**: Servicios stateless (Auth, Hashing)

---

## 📦 Estructura del Proyecto

```plaintext
nextpapyros-backend-api/
├── src/
│   ├── NextPapyros.API/              # 🌐 Presentación
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs         # Autenticación y registro
│   │   │   ├── EmpleadosController.cs    # Gestión de empleados
│   │   │   ├── ProductosController.cs    # Gestión de productos
│   │   │   ├── ProveedoresController.cs  # Gestión de proveedores
│   │   │   ├── VentasController.cs       # Registro de ventas
│   │   │   ├── RecepcionesController.cs  # Recepciones de mercancía
│   │   │   └── ReportesController.cs     # Exportación de reportes
│   │   ├── Contracts/                # DTOs Request/Response
│   │   ├── Startup/                  # DbSeeder
│   │   └── Program.cs                # Configuración
│   │
│   ├── NextPapyros.Application/      # 📦 Aplicación
│   │   ├── Email/                    # Interfaces de email
│   │   └── Reports/                  # Interfaces de reportes
│   │
│   ├── NextPapyros.Domain/           # 🎯 Dominio
│   │   ├── Entities/                 # Modelos de negocio
│   │   │   ├── Usuario.cs
│   │   │   ├── Producto.cs
│   │   │   ├── Proveedor.cs
│   │   │   ├── Venta.cs
│   │   │   ├── Recepcion.cs
│   │   │   └── OrdenCompra.cs
│   │   └── Repositories/             # Interfaces de repositorios
│   │       ├── IUsuarioRepository.cs
│   │       ├── IProductoRepository.cs
│   │       ├── IProveedorRepository.cs
│   │       ├── IVentaRepository.cs
│   │       ├── IRecepcionRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   └── NextPapyros.Infrastructure/   # 🔧 Infraestructura
│       ├── Auth/                     # JWT, BCrypt
│       ├── Email/                    # Servicio de correos (Gmail SMTP)
│       ├── Persistence/              # DbContext, UnitOfWork
│       ├── Repositories/             # Implementaciones de repositorios
│       ├── Reports/                  # Exportadores CSV/PDF, Comprobantes
│       └── Migrations/               # EF Migrations
│
└── NextPapyros.sln
```

---

## 🚀 Inicio Rápido

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 16+ ([Homebrew en macOS](https://formulae.brew.sh/formula/postgresql@16) o [Docker](https://hub.docker.com/_/postgres))

### Instalación Rápida

```bash
# 1. Clonar repositorio
git clone https://github.com/NextPapyros/nextpapyros-backend-api.git
cd nextpapyros-backend-api

# 2. Configurar PostgreSQL
# Opción A - Homebrew en macOS:
brew install postgresql@16
brew services start postgresql@16
psql postgres -c "CREATE DATABASE \"NextPapyrosDb\";"

# Opción B - Docker:
docker run --name postgres-nextpapyros \
  -e POSTGRES_DB=NextPapyrosDb \
  -e POSTGRES_PASSWORD=Password123* \
  -p 5432:5432 -d postgres:16

# 3. Configurar Email (OPCIONAL - para recuperación de contraseña)
cd src/NextPapyros.API
cp appsettings.Development.json.example appsettings.Development.json
# Edita appsettings.Development.json con tu correo Gmail y contraseña de aplicación
# Ver instrucciones detalladas en SETUP_EMAIL.md

# 4. Aplicar migraciones
dotnet ef database update

# 5. Ejecutar
dotnet run
```

**📖 Para instrucciones detalladas**, consulta:
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Guía completa de instalación
- **[SETUP_EMAIL.md](SETUP_EMAIL.md)** - Configuración de correo para recuperación de contraseña

---

## 📚 Documentación API

### Swagger UI

Accede a la documentación interactiva en:

- **Desarrollo**: [http://localhost:5288/swagger](http://localhost:5288/swagger)
- **HTTPS**: [https://localhost:7037/swagger](https://localhost:7037/swagger)

### Autenticación

1. **Login** con las credenciales por defecto:
   ```json
   POST /auth/login
   {
     "email": "mail@mail.com",
     "password": "Password123*"
   }
   ```

2. **Copia el token** de la respuesta

3. **Autoriza en Swagger**: Click en "Authorize" → `Bearer {tu-token}`

### Endpoints Principales

| Módulo | Endpoint | Método | Descripción |
|--------|----------|--------|-------------|
| **Auth** | `/auth/login` | POST | Iniciar sesión (retorna JWT) |
| **Auth** | `/auth/register` | POST | Registrar usuario (solo Admin) |
| **Auth** | `/auth/forgot-password` | POST | Solicitar token de recuperación |
| **Auth** | `/auth/reset-password` | POST | Restablecer contraseña con token |
| **Empleados** | `/empleados` | GET | Listar empleados (Admin) |
| **Empleados** | `/empleados` | POST | Crear empleado (Admin) |
| **Empleados** | `/empleados/{id}` | GET | Obtener empleado por ID (Admin) |
| **Empleados** | `/empleados/{id}` | PUT | Actualizar empleado (Admin) |
| **Empleados** | `/empleados/{id}/inhabilitar` | PATCH | Inhabilitar empleado (Admin) |
| **Productos** | `/products` | GET | Listar productos |
| **Productos** | `/products` | POST | Crear producto (Admin) |
| **Productos** | `/products/{codigo}` | GET | Obtener producto por código |
| **Proveedores** | `/proveedores` | GET | Listar proveedores activos |
| **Proveedores** | `/proveedores` | POST | Crear proveedor (Admin) |
| **Proveedores** | `/proveedores/{id}` | GET | Obtener proveedor por ID |
| **Ventas** | `/ventas` | POST | Registrar venta (reduce stock) |
| **Ventas** | `/ventas/{id}` | GET | Consultar venta |
| **Ventas** | `/ventas/{id}/comprobante` | GET | Generar comprobante PDF |
| **Ventas** | `/ventas/pos/buscar` | GET | Buscar productos para POS |
| **Recepciones** | `/recepciones` | POST | Registrar recepción (incrementa stock) |
| **Recepciones** | `/recepciones/{id}` | GET | Consultar recepción |
| **Reportes** | `/reportes/top-productos` | GET | Top productos vendidos (CSV/PDF) |
| **Reportes** | `/reportes/stock-bajo` | GET | Productos con stock bajo |
| **Reportes** | `/reportes/ingresos-mensuales` | GET | Ingresos por mes |

---

## 👤 Usuario Administrador

Al iniciar la aplicación por primera vez, se crea automáticamente:

| Campo | Valor |
|-------|-------|
| Email | `mail@mail.com` |
| Password | `Password123*` |
| Rol | Administrador |

⚠️ **Cambia estas credenciales en producción**.

---

## 🧪 Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📖 Documentación Adicional

- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Guía completa de instalación y configuración
- **[SETUP_EMAIL.md](SETUP_EMAIL.md)** - Configuración de Gmail para recuperación de contraseña
- **[Swagger UI](http://localhost:5288/swagger)** - Documentación interactiva de la API
- **[Issues](https://github.com/NextPapyros/nextpapyros-backend-api/issues)** - Reportar bugs o solicitar features

---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Por favor:

1. Lee **[CONTRIBUTING.md](CONTRIBUTING.md)** para instrucciones detalladas
2. Haz fork del proyecto
3. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
4. Commit tus cambios (`git commit -m 'Add AmazingFeature'`)
5. Push a la rama (`git push origin feature/AmazingFeature`)
6. Abre un Pull Request

### Estándares de Código

- ✅ Sigue los principios **SOLID**
- ✅ Mantén la **separación de responsabilidades** por capas
- ✅ Usa **nombres descriptivos** en español para entidades de negocio
- ✅ Aplica **patrones de diseño** (Repository, Unit of Work, Strategy)
- ✅ Añade **comentarios XML** para documentación Swagger
- ✅ Implementa **validaciones de negocio** antes de operaciones
- ✅ Usa **transacciones** (Unit of Work) para operaciones de múltiples pasos

---

## 🗺️ Roadmap

- [x] Autenticación JWT con roles
- [x] Recuperación de contraseña por email
- [x] Gestión de empleados (CRUD completo)
- [x] Gestión de productos con stock
- [x] Gestión de proveedores
- [x] Sistema de ventas con actualización de inventario
- [x] Comprobantes PDF de ventas
- [x] Recepciones de mercancía
- [x] Reportes exportables (CSV/PDF)
- [x] Patrón Unit of Work para transacciones
- [x] Migración a PostgreSQL
- [x] Envío de correos con MailKit
- [ ] Pruebas unitarias e integración
- [ ] Gestión de devoluciones completa
- [ ] Órdenes de compra con seguimiento
- [ ] Dashboard de analytics
- [ ] Cache distribuido con Redis
- [ ] API GraphQL
- [ ] Frontend React/Angular
- [ ] CI/CD con GitHub Actions

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

## 📞 Soporte

- 🐛 **Bugs**: [GitHub Issues](https://github.com/NextPapyros/nextpapyros-backend-api/issues)
- 💬 **Discusiones**: [GitHub Discussions](https://github.com/NextPapyros/nextpapyros-backend-api/discussions)
- 📧 **Email**: soporte@nextpapyros.com

---

<div align="center">

**Hecho con ❤️ por el equipo de NextPapyros**

[⬆ Volver arriba](#-nextpapyros-backend-api)

</div>
