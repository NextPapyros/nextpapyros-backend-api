# 📧 Configuración de Email para Recuperación de Contraseña

Este documento explica cómo configurar el envío de correos electrónicos para la funcionalidad de recuperación de contraseña.

## ⚙️ Configuración Requerida

### 1. Crear archivo de configuración local

Copia el archivo de ejemplo y renómbralo:

```bash
cd src/NextPapyros.API
cp appsettings.Development.json.example appsettings.Development.json
```

### 2. Configurar Gmail

#### Paso 1: Habilitar verificación en dos pasos
1. Ve a tu [Cuenta de Google](https://myaccount.google.com/)
2. Selecciona **"Seguridad"** en el menú izquierdo
3. En "Cómo inicias sesión en Google", activa la **verificación en dos pasos**

#### Paso 2: Generar contraseña de aplicación
1. Vuelve a **"Seguridad"**
2. Busca **"Contraseñas de aplicaciones"** (aparece solo si tienes verificación en dos pasos)
3. Selecciona:
   - Aplicación: **Correo**
   - Dispositivo: **Otro (nombre personalizado)** → escribe "NextPapyros"
4. Click en **"Generar"**
5. Copia la contraseña de 16 caracteres (formato: `xxxx xxxx xxxx xxxx`)

#### Paso 3: Actualizar appsettings.Development.json

Edita el archivo `appsettings.Development.json` que acabas de crear:

```json
{
  "Email": {
    "From": "tu-correo@gmail.com",
    "Password": "xxxx xxxx xxxx xxxx",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587"
  }
}
```

**Importante:** 
- Usa tu correo de Gmail en `"From"`
- Usa la contraseña de aplicación (16 caracteres) en `"Password"`, NO tu contraseña normal de Gmail
- Este archivo está en `.gitignore` y NO se subirá al repositorio

### 3. Configurar PostgreSQL

También actualiza la cadena de conexión en el mismo archivo:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=NextPapyrosDb;Username=TU_USUARIO_POSTGRES;Password=TU_PASSWORD_POSTGRES;"
  }
}
```

## 🧪 Probar la Configuración

### Opción 1: Crear usuario de prueba

```bash
psql NextPapyrosDb -c "INSERT INTO \"Usuarios\" (\"Nombre\", \"Email\", \"PasswordHash\", \"Activo\") VALUES ('Test User', 'tu-correo@gmail.com', '\$2a\$11\$dummyhash', true);"
```

### Opción 2: Usar Swagger

1. Inicia la aplicación: `dotnet run`
2. Ve a: http://localhost:5288/swagger
3. Prueba el endpoint `POST /auth/forgot-password`:

```json
{
  "email": "tu-correo@gmail.com"
}
```

Deberías recibir un correo con el código de recuperación en menos de 1 minuto.

## 🔒 Seguridad

- ✅ **NUNCA** subas `appsettings.Development.json` al repositorio
- ✅ **NUNCA** compartas tu contraseña de aplicación de Gmail
- ✅ Revoca la contraseña de aplicación desde tu cuenta de Google si crees que fue comprometida
- ✅ El archivo `appsettings.Development.json` está incluido en `.gitignore`

## ❓ Troubleshooting

### Error: "Error al enviar correo"
- Verifica que la contraseña de aplicación sea correcta (16 caracteres)
- Asegúrate de tener verificación en dos pasos habilitada
- Verifica que tu correo sea de Gmail

### No llega el correo
- Revisa la carpeta de spam
- Verifica que el email en la BD coincida con tu correo real
- Revisa los logs de la aplicación para ver si hubo errores

### Error de conexión SMTP
- Verifica que el puerto sea 587
- Asegúrate de que el SmtpServer sea "smtp.gmail.com"

## 📚 Más Información

- [Contraseñas de aplicaciones de Google](https://support.google.com/accounts/answer/185833)
- [Documentación de MailKit](https://github.com/jstedfast/MailKit)
