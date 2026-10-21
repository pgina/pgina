# Estabilización coordinada Desktop / AdminWeb

## Alcance y compatibilidad

Cambios de trabajo sobre Desktop `4a96df6` y AdminWeb `bb97ddf`; no constituyen una publicación ni un instalador certificado. AdminWeb es el repositorio independiente anidado `OpenCredential.AdminWeb/`. Sus cambios se revisan y versionan separadamente. No se sincroniza automáticamente la copia externa.

El contrato compartido usa el esquema inglés: `users`, `groups`, `user_groups`, `login_sessions`; los timestamps sin zona representan **UTC**. Antes de actualizar, comprobar si una instalación antigua guardó horas locales: no hay conversión destructiva automática del historial.

AdminWeb agrega `admin_id_sequences` para asignar IDs atómicamente y un índice único de nombres sin distinguir mayúsculas en PostgreSQL. Si existen nombres duplicados por capitalización, resolverlos con respaldo y revisión antes de aplicar el esquema. No se eliminan registros para solucionar conflictos.

## Cambios que requieren atención

- Contraseñas nuevas: mínimo 12 caracteres y máximo 72 bytes UTF-8. Las claves heredadas siguen verificándose y pueden migrarse a BCrypt tras validarse; editar/importar sin clave no modifica el hash ni el algoritmo.
- `GroupIds=null` u omitido conserva grupos; una lista vacía los elimina expresamente.
- Las mutaciones HTTP requieren `X-OpenCredential-Request: 1`. Ambas interfaces lo envían. Desktop accede a la base, no depende de esta cabecera.
- Cambios de clave, estado o membresías invalidan cookies anteriores. Tras actualizar, los administradores y usuarios deben iniciar sesión de nuevo.
- JSON conserva hashes y tokens usando un contrato privado de almacenamiento. Los hashes que una versión anterior ya perdió **no pueden recuperarse**: requieren restablecimiento. Respaldar `App_Data` antes de actualizar.
- No se crean cuentas de demostración por defecto. Usar una cuenta administrativa configurada mediante secretos y asignar grupos a los usuarios desde ella.
- Offline: vencimiento predeterminado 24 h, bloqueo local y hashes DPAPI ligados a la máquina. Una sincronización completa retira usuarios eliminados. No puede garantizarse revocación instantánea de una máquina desconectada.
- Sin conexión, las reglas de autorización/gateway que dependen de grupos deniegan acceso en lugar de omitirse. Solo se evalúan reglas incondicionales después de autenticar con Database Auth. Probar esta política antes del despliegue.
- Los plugins antiguos de terceros pueden necesitar recompilación por la migración a log4net 3.5.0. El usuario autorizó incluirla; no se certifica compatibilidad con extensiones no probadas.

## Validación reproducible

1. Compilar con `Build-OpenCredential.ps1 -Configuration Release`; solo genera artefactos locales.
2. Ejecutar `Tests/Test-Desktop.ps1` desde Windows PowerShell 5.1: SQLite/DPAPI, bloqueo, vencimiento, revocación y carga de tipos de plugins. No instala servicios ni escribe configuración en el registro.
3. En AdminWeb: `dotnet run --project Tests/RegressionTests.csproj` y `node --test Tests/frontend.test.cjs`. Se generan datos sintéticos en directorios temporales y un servidor HTTP de loopback que finaliza al acabar.
4. Pruebas SQL opcionales: `--postgres-port PUERTO` y/o `--mysql-port PUERTO` después de `--` en el comando de regresión. **Solo** apuntar a instancias desechables de localhost: crean bases de prueba nuevas, sin eliminar bases existentes. No usar puertos de producción.
5. Comprobar el esquema y el flujo completo en una VM Windows antes de instalar: web → usuario/grupos → login real → bloqueo/desbloqueo → logoff → web → cambio de clave → nuevo login → caída y recuperación de red.

## Actualización y reversión

1. Respaldar base compartida, datos de AdminWeb, claves de protección de cookies, configuración de escritorio y paquete instalado anterior. No publicar ni copiar secretos al repositorio.
2. En mantenimiento, aplicar el esquema compatible de AdminWeb con `Ajustar tablas de AdminWeb` o `AutoInitialize` usando una identidad de migración autorizada. El usuario operativo debe tener mínimos privilegios.
3. Actualizar AdminWeb y después un equipo piloto. Configurar TLS, proxy de confianza y SMTP institucional; consultar el README del repositorio web.
4. Mantener una cuenta administrativa local de emergencia y una vía alternativa de acceso a Windows. No probar una nueva cadena de autenticación en el único equipo de administración.
5. Ante una regresión, volver al paquete/código anterior revisado, sin borrar tablas ni volúmenes. La caché DPAPI nueva no es legible por versiones anteriores: deshabilitar el fallback y reconstruir una caché compatible mediante sincronización online controlada. No restaurar una base antigua sobre datos recientes sin aprobación explícita.

## Pendientes de aceptación, no certificados por una compilación

Validaciones ejecutadas en este trabajo: build Desktop x64/Win32 y plugins, carga de tipos de los ocho plugins core, regresiones SQLite/DPAPI, 12 grupos de regresión web/HTTP/SQL con PostgreSQL 16 y MySQL 8.4, tres pruebas de frontend, build Release web y prueba funcional de la imagen Linux (login, datos y PDF). Los contenedores SQL usados contienen únicamente datos sintéticos y no tienen volúmenes persistentes.

- Validados en VM Windows 11: instalación inicial, actualización integrada con marcadores/hashes y login posterior, login nuevo online/offline, usuario inactivo, clave incorrecta, bloqueo/expiración, logger online y sincronización de cola con origen offline_cache. Posteriormente se validaron desinstalación con preservación/reinicio, reversión por snapshots y RDP con NLA tras corregir recepción de credenciales protegidas y claves de grupos PostgreSQL. Windows 10 no se probó en VM.
- Validación con CA institucional, servidor SMTP real y proxy HTTPS del despliegue.
- Pruebas de carga y paginación SQL de todos los listados: el snapshot heredado todavía carga colecciones completas.
- Asistente de despliegue/configuración y mejoras adicionales de UX; no son requisitos para las correcciones de integridad.
- Firma de instaladores y compatibilidad de plugins de terceros.

## Ampliación de pruebas antes de commit/push

Consultar [QA_READINESS.md](QA_READINESS.md). Se corrigió el borrado de datos/configuración del instalador y se validaron la instalación inicial y actualización integrada FinalUpdate con marcadores conservados. Las correcciones UTC, parámetros SQL nulos y origen offline también se validaron en la VM; servicio y login posterior a la actualización pasaron. Desinstalación, reinicio y reversión por snapshots se probaron después en esa VM concreta; no certifican todos los metadatos de desinstaladores antiguos ni una reversión de esquema/datos en producción.

Se agregaron pruebas de HTTPS/STARTTLS en Linux aislado con CA efímera por proceso, rechazo de certificados inválidos y negativa a transmitir correo sin TLS. No se agregaron certificados al almacén del anfitrión ni se enviaron correos a servicios externos. Estas pruebas no sustituyen la validación de la CA y SMTP institucionales.
