# Guía de Despliegue en Coolify

Este proyecto está configurado para ser desplegado fácilmente en Coolify u otros orquestadores de contenedores.

## Variables de Entorno Requeridas

Para que la aplicación funcione correctamente en producción, debes configurar las siguientes variables de entorno en tu panel de Coolify:

### Base de Datos (PostgreSQL)
| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings__Default` | Cadena de conexión a PostgreSQL | `Host=mi-db;Port=5432;Database=NextPapyros;Username=postgres;Password=secret` |

### Correo Electrónico (SMTP)
| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| `Email__From` | Dirección de correo del remitente | (Requerido) |
| `Email__Password` | Contraseña de aplicación del correo | (Requerido) |
| `Email__SmtpServer` | Servidor SMTP | `smtp.gmail.com` |
| `Email__SmtpPort` | Puerto SMTP | `587` |

### Configuración General
| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` |
| `ASPNETCORE_HTTP_PORTS` | Puerto de escucha | `8080` |

## Notas Importantes

1. **Base de Datos**: Asegúrate de que la base de datos PostgreSQL esté accesible desde el contenedor de la API. Si usas la base de datos interna de Coolify, usa el nombre del servicio como host.
2. **Migraciones**: La aplicación intentará ejecutar migraciones al inicio (ver `Program.cs`). Asegúrate de que el usuario de la base de datos tenga permisos para crear tablas.
