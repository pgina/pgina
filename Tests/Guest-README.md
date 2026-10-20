# Validación dentro de la VM Windows

Este paquete es de laboratorio, no una versión publicada. Solo se comparte el instalador revisado, su SHA256 y scripts; nunca `.env`, claves ni configuración real del anfitrión.

## Antes de instalar

1. Confirmar el snapshot `OpenCredential-before-validation-*` y mantener la VM sin red hasta revisar que los plugins no apunten a servidores institucionales.
2. Iniciar sesión manualmente dentro de Windows. No compartir contraseñas con el asistente.
3. Mantener una cuenta administrativa local alternativa funcional; no deshabilitar el proveedor de contraseña de Windows.
4. Abrir **Windows PowerShell como administrador dentro de la VM**, no en el anfitrión.

Inspección sin ejecutar el instalador:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestValidation.ps1"
```

Instalación/actualización del paquete de revisión (la confirmación aplica solo a esta VM):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestValidation.ps1" -Install -AllowUnsignedReviewInstaller
```

El script comprueba el hash, exige VM y privilegios administrativos, crea marcadores sintéticos y verifica que instalación/actualización conserve directorios y registro, que el servicio esté activo y que el proveedor esté registrado. La compilación de revisión no está firmada; el permiso explícito no debe usarse en producción.

El resultado sin secretos queda en `C:\ProgramData\OpenCredential-QA\validation-result.json`. Si existe la carpeta compartida de resultados, copia allí únicamente ese JSON. No copia el registro, credenciales, logs de autenticación ni cachés existentes. No reinicia ni restaura snapshots automáticamente.

Si una comprobación inicial se ejecutó mientras el servicio aún arrancaba, se puede reevaluar el estado y los marcadores sin reinstalar ni arrancar/detener servicios:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestValidation.ps1" -Recheck
```

El campo `RecheckOnly=true` identifica esta comprobación. Conserva el código de salida del instalador previo y vuelve a comprobar marcadores, servicio y registro del proveedor. No convierte en aprobadas las pruebas manuales de login.

## Pruebas de aceptación posteriores

### Actualización integrada final de revisión

Con sesión administrativa alternativa abierta, snapshot previo conservado y configuración cerrada, `Invoke-GuestFinalUpdate.ps1` instala el paquete `FinalUpdate` sin reiniciar Windows. Reutiliza las comprobaciones VM/admin/SHA256/preservación/arranque y admite únicamente el instalador de revisión sin firma aprobado para este laboratorio. Compara además los hashes instalados de DatabaseAuth y DatabaseLogger. No exporta configuración ni secretos; copia informes de estado a la carpeta de resultados. Repetir después autenticación por servicio y login mediante OpenCredential. Un resultado PASS no certifica desinstalación ni reversión.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestFinalUpdate.ps1"
```

### Corrección de reproducción PostgreSQL del logger

`Invoke-GuestLoggerFix.ps1` es exclusivo de la VM y del paquete `LoggerReplayFix`. Con configuración cerrada y PowerShell elevado, valida el hash de la DLL, detiene el servicio y respalda la DLL anterior y la cola SQLite por defecto (incluidos WAL/SHM si existen), comprobando SHA256 antes de actualizar y arrancar el servicio. No borra ni restaura automáticamente la cola: los registros reproducidos correctamente serán retirados por el propio logger. La restauración automática de una cola ya parcialmente procesada podría duplicar registros SQL. Conserva la sesión administrativa y el snapshot; si la ruta de cola fue personalizada, no uses este paquete sin adaptarlo primero.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestLoggerFix.ps1"
```

Tras la actualización, dejar NAT conectado, comprobar pendientes con `Consultar-Cola.ps1` y contrastar registros/estados en PostgreSQL. El mensaje PASS solo valida respaldo/copia/arranque, no el replay funcional.

Para el paquete posterior `LoggerOriginFix`, ejecutar `Invoke-GuestLoggerOriginFix.ps1` en la misma carpeta compartida. Reutiliza las mismas protecciones y respaldos. Los nuevos inicios reproducidos se etiquetan `offline_cache` sin heredar `online`; no altera filas históricas ni demuestra por sí mismo qué autenticador se utilizó. Repetir una sesión offline nueva y verificar en SQL el origen, cierre y cola en 0.

### Corrección de bloqueo UTC de laboratorio

`Invoke-GuestLockoutFix.ps1` actualiza únicamente `OpenCredential.Plugin.DatabaseAuth.dll` del directorio Core junto al servicio instalado. Requiere VM, PowerShell elevado, configuración cerrada, servicio Running y hash SHA256 del paquete `LockoutUtcFix` correcto. Respalda y verifica la DLL anterior en `C:\ProgramData\OpenCredential-QA`, detiene/espera el servicio, copia/verifica la DLL y vuelve a iniciarlo. Ante fallo de copia/arranque intenta restaurar el respaldo. No modifica cuentas, claves, registro ni fechas SQL; no reinicia Windows. Conserva la sesión administrativa alternativa y el snapshot antes de ejecutar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "\\VBOXSVR\OpenCredentialQA\Invoke-GuestLockoutFix.ps1"
```

El informe `lockout-fix-result.json` prueba copia/arranque, no aceptación funcional. Repetir clave válida tras vencimiento y bloqueo de 3 intentos / 1 minuto. El mensaje nuevo indica explícitamente `UTC`.

Los casos de login, RDP, bloqueo, offline, desinstalación y reversión permanecen **Pending** hasta ejecutarlos. No confundir un proveedor registrado o una simulación con un inicio de sesión real.

Usar únicamente una base aislada y usuarios sintéticos. Antes de reconectar la interfaz de red se revisará la configuración de prueba; no utilizar una base institucional ni credenciales de producción.

Una desinstalación o restauración de snapshot requiere confirmación adicional: podría retirar el proveedor de credenciales o revertir cambios de la VM. Una actualización desde instaladores antiguos también puede conservar metadatos de desinstalación previos, por lo que la preservación durante desinstalación debe comprobarse expresamente.
