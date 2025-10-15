# 🧩 NextPapyros Backend API

Sistema de gestión empresarial para **inventario, ventas, compras, recepciones y devoluciones**, desarrollado con **.NET 8** siguiendo los principios de **Clean Architecture** y **SOLID**.

---

## 🚀 Características Principales

- 🔐 **Autenticación JWT** con sistema de roles y permisos
- 📦 **Gestión de Inventario** con control de stock y alertas
- 🛒 **Órdenes de Compra** y recepciones de mercancía
- 💰 **Registro de Ventas** con múltiples métodos de pago
- 🔄 **Devoluciones** con trazabilidad completa
- 📊 **Reportes y Analytics** (top productos, stock bajo, ingresos mensuales)
- 📝 **Auditoría** con logs de operaciones

---

## 🛠️ Tecnologías

| Tecnología | Versión | Descripción |
|------------|---------|-------------|
| **.NET** | 8.0 | Framework principal |
| **Entity Framework Core** | 9.0 | ORM y migraciones |
| **SQL Server** | 2019+ | Base de datos |
| **JWT Bearer** | 8.0 | Autenticación |
| **Swagger/OpenAPI** | 6.x | Documentación API |
| **BCrypt.Net** | 4.0 | Hashing de contraseñas |

---

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con separación en capas:

```plaintext
┌─────────────────────────────────────────┐
│         NextPapyros.API                 │  ← Capa de Presentación
│  (Controllers, DTOs, Middleware)        │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      NextPapyros.Application            │  ← Capa de Aplicación
│      (Casos de Uso, Services)           │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        NextPapyros.Domain               │  ← Capa de Dominio
│  (Entities, Interfaces, Lógica)         │
└─────────────────────────────────────────┘
                  ↑
┌─────────────────────────────────────────┐
│     NextPapyros.Infrastructure          │  ← Capa de Infraestructura
│  (EF Core, Repos, Auth, Migrations)     │
└─────────────────────────────────────────┘
```

### Patrones Implementados

- **Repository Pattern**: Abstracción de acceso a datos
- **Unit of Work**: Gestión de transacciones
- **Dependency Injection**: Inversión de dependencias
- **Domain Services**: Lógica de negocio compleja

---

## 📦 Estructura del Proyecto

```plaintext
nextpapyros-backend-api/
├── src/
│   ├── NextPapyros.API/              # 🌐 Presentación
│   │   ├── Controllers/              # Endpoints REST
│   │   ├── Contracts/                # DTOs Request/Response
│   │   ├── Startup/                  # DbSeeder
│   │   └── Program.cs                # Configuración
│   │
│   ├── NextPapyros.Application/      # 📦 Aplicación
│   │
│   ├── NextPapyros.Domain/           # 🎯 Dominio
│   │   ├── Entities/                 # Modelos de negocio
│   │   └── Repositories/             # Interfaces
│   │
│   └── NextPapyros.Infrastructure/   # 🔧 Infraestructura
│       ├── Auth/                     # JWT, BCrypt
│       ├── Persistence/              # DbContext
│       ├── Repositories/             # Implementaciones
│       └── Migrations/               # EF Migrations
│
└── NextPapyros.sln
```

---

## 🚀 Inicio Rápido

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Windows) o Docker (macOS/Linux)

### Instalación Rápida

```bash
# 1. Clonar repositorio
git clone https://github.com/NextPapyros/nextpapyros-backend-api.git
cd nextpapyros-backend-api

# 2. Configurar base de datos (Docker en macOS/Linux)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123*" \
  -p 1433:1433 --name sqlserver-nextpapyros \
  -d mcr.microsoft.com/mssql/server:2019-latest

# 3. Aplicar migraciones
cd src/NextPapyros.API
dotnet ef database update

# 4. Ejecutar
dotnet run
```

**📖 Para instrucciones detalladas**, consulta **[CONTRIBUTING.md](CONTRIBUTING.md)**.

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
| **Auth** | `/auth/login` | POST | Iniciar sesión |
| **Auth** | `/auth/register` | POST | Registrar usuario (Admin) |
| **Productos** | `/products` | GET | Listar productos |
| **Productos** | `/products` | POST | Crear producto |
| **Productos** | `/products/{codigo}` | GET | Obtener producto |
| **Ventas** | `/ventas` | POST | Registrar venta |
| **Ventas** | `/ventas/{id}` | GET | Consultar venta |
| **Recepciones** | `/recepciones` | POST | Registrar recepción |
| **Reportes** | `/reportes/top-productos` | GET | Top productos vendidos |
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
- ✅ Mantén la **separación de responsabilidades**
- ✅ Usa **nombres descriptivos** en inglés
- ✅ Añade **comentarios XML** para métodos públicos
- ✅ Escribe **pruebas** para nuevas funcionalidades

---

## 🗺️ Roadmap

- [ ] Pruebas unitarias e integración
- [ ] Soporte para PostgreSQL
- [ ] Implementación de CQRS
- [ ] GraphQL API
- [ ] Eventos de dominio
- [ ] Cache distribuido con Redis
- [ ] Frontend React/Angular
- [ ] API de reportes avanzados
- [ ] Internacionalización (i18n)
- [ ] Containerización completa
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
