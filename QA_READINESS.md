# Estado de validación antes de commit/push

Fecha de trabajo: 2026-10-20. El usuario autorizó commit/push de estabilización en Desktop `master` y AdminWeb `main`, incluida la migración previa a log4net. No equivale a publicar una versión certificada ni un instalador firmado.

| Prueba | Resultado | Alcance |
|---|---|---|
| Desktop x64/Win32 y plugins core | PASS previo | Compilación y carga de tipos; no equivale a login real |
| SQLite/DPAPI, bloqueo, caducidad y revocación | PASS previo | Datos sintéticos, sin configuración de registro real |
| Web JSON, autenticación, roles, recuperación y SQL | PASS previo | Incluye PostgreSQL 16 y MySQL 8.4 aislados |
| Proxy confiable y no confiable | PASS ampliado | Redirección HTTPS, cookie Secure y auditoría de IP reenviada |
| HTTPS | PASS ampliado | CA de laboratorio confiable; rechazo de CA desconocida, expiración y hostname incorrecto |
| SMTP STARTTLS | PASS ampliado | Token recibido por servidor local de pruebas; no se entrega sin TLS o con CA desconocida |
| Instalador: preservación y compilación | PASS ampliado | Inno Setup 6; artefacto separado, sin ejecutarlo en el anfitrión |
| Redistribuibles VC++ incluidos | PASS | Firmas Authenticode válidas |
| Script de pruebas del invitado | PASS de seguridad | Parseo correcto y rechazo del anfitrión antes de escribir/instalar |
| Instalación real inicial | PASS | Salida 0, marcadores conservados, proveedor registrado y servicio Running |
| Actualización con instalador integrado | PASS en VM | Salida 0, marcadores conservados, servicio/proveedor correctos y hashes de ambos plugins verificados; servicio y login posterior aprobados |
| Login Windows y rechazo de cuenta inactiva | PASS en VM | Usuario sintético; Database Auth único autenticador configurado, gateway Local Machine |
| Bloqueo y expiración | PASS en VM | 3 intentos / 1 minuto, corrección UTC instalada y repetida |
| Logger online | PASS SQL/VM | Inicio, heartbeat periódico, lock/unlock y cierre verificados |
| Offline y reproducción de cola | PASS SQL/VM | Login nuevo offline, rechazo de clave incorrecta, cola en 0, origen offline_cache y cierre confirmados |
| RDP y cambio de contraseña web → Windows | PENDING | No certificados por las pruebas anteriores |
| Desinstalación y reversión reales | PENDING | Confirmación adicional; no restaurar snapshots ni purgar datos automáticamente |
| CA, SMTP y proxy institucionales | PENDING | No se usaron servicios o credenciales reales |
| Carga y paginación SQL completa | PENDING | Mejora funcional aún no implementada; el snapshot conserva colecciones completas |

## VM autorizada

### Seguimiento 2026-10-20

- Recheck de instalación aprobado: servicio Running, salida 0, marcadores conservados y proveedor registrado. El usuario puso `pGina` en inicio Manual.
- Laboratorio sintético PostgreSQL preparado, NIC NAT conectada tras revisar plugins; caché offline desactivada. No usar conexiones institucionales.
- Usuario confirma login nuevo y reentrada de `ocqa_student` mediante OpenCredential con Database Auth como único autenticador; Gateway Local Machine crea la cuenta habilitada. Servicio rechaza clave incorrecta y usuario inactivo.
- Prueba de bloqueo descubre defecto: SQL ya considera vencido el bloqueo pero el cliente UTC-5 prolonga cinco horas un TIMESTAMP sin zona. Corrección en lectura UTC compartida de PostgreSQL/MySQL y mensaje explícito UTC; repetir expiración en VM tras actualizar el plugin. No se alteraron relojes ni se desbloqueó la cuenta manualmente.
- Las pruebas anteriores no completan offline integral, RDP, actualización, desinstalación ni reversión; no hay autorización nueva de commit/push.
- Seguimiento de aceptación: usuario confirma bloqueo 3 intentos / 1 minuto y expiración tras actualizar DatabaseAuth; acceso offline permitido con caché y rechazo de clave incorrecta, login nuevo offline y retorno online sin fallback. Logger online verificado directamente en SQL: logon, heartbeat periódico, lock, unlock y logoff.
- Cola offline: 5 registros sintéticos pendientes observados en la VM. La reproducción no vacía la cola tras reconectar; PostgreSQL informa parámetro sin tipo. Regresión aislada con el código de replay reproduce SQLSTATE 42P08 y pasa tras tipar parámetros nulos de texto en OfflineLogQueue/SessionLogger. Requiere instalar LoggerReplayFix y verificar llegada a SQL, cierre y ausencia de pendientes; no borrar la cola manualmente.
- LoggerReplayFix instalado con respaldo de DLL/SQLite verificado; usuario confirma cola en 0 y SQL confirma llegada de eventos/inicio/cierre. Se detecta clasificación heredada `online`: corregida inserción de logon reproducido para guardar `offline_cache`, coherente con reconciliación de sesiones pendientes. No se reescribe la fila histórica; repetir una sesión offline nueva tras actualizar el paquete LoggerOriginFix. La etiqueta refleja registro diferido del inicio, no es por sí sola prueba del mecanismo de autenticación.
- LoggerOriginFix instalado y nueva sesión offline validada en SQL: origen `offline_cache`, estado `ended`, cierre `logoff`, eventos logon/logoff sincronizados. Usuario confirma cola nuevamente en 0. La fila anterior mal clasificada no se modificó.
- FinalUpdate ejecutado el 2026-10-20: actualización de instalación existente con salida 0, marcadores de datos/registro preservados, servicio Running, proveedor registrado y hashes correctos de DatabaseAuth/DatabaseLogger. Usuario confirma simulación por servicio y login nuevo mediante OpenCredential después de actualizar. Hubo tres intentos rechazados y bloqueo transitorio antes del chequeo final; venció sin cambios manuales, se verificó que la clave sintética seguía coincidiendo con SQL y las pruebas posteriores pasaron. No se atribuye una causa no demostrada a esos rechazos.

- VirtualBox 7.2.6, VM existente `Pruebas Win 11 pgina`, apagada al preparar las pruebas, 4 CPU / 4 GB RAM.
- Snapshot previo: `OpenCredential-before-validation-20261019-105158` (`a60dc808-c01d-424b-8f26-adbb036cc90c`). No se ha restaurado ni eliminado.
- NIC desconectada para impedir conexiones de una configuración antigua a bases reales.
- Compartición de entrada `\\VBOXSVR\OpenCredentialQA` de solo lectura; salida `\\VBOXSVR\OpenCredentialQAResults` limitada a un directorio de resultados nuevo.
- Paquete local: `C:\Users\Oitel\AppData\Local\Temp\opencode\OpenCredential-VM-QA-20261019-110216`.
- La VM no se inició mientras el anfitrión no dispusiera del margen de memoria acordado. No se cerraron aplicaciones ni detuvieron servicios existentes.
- No hay control gráfico del invitado en esta sesión: el usuario debe iniciar sesión y ejecutar el script en Windows PowerShell elevado. No es necesario compartir credenciales.

## Comandos de validación

En el anfitrión, solo compilar/verificar:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tests\Test-Installer.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tests\Test-Desktop.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\OpenCredential.AdminWeb\Tests\Test-Transport.ps1
dotnet run --project .\OpenCredential.AdminWeb\Tests\RegressionTests.csproj
node --test .\OpenCredential.AdminWeb\Tests\frontend.test.cjs
```

Dentro de la VM, seguir `Tests/Guest-README.md`. El script no ejecuta login con cuentas reales, no exporta secretos, no reinicia, no desinstala ni restaura snapshots automáticamente. Los casos manuales permanecen Pending hasta ejecutarse y documentarse.

## Condiciones para publicar

Resultado observado del primer ensayo en VM: salida 0, marcadores conservados y proveedor registrado; recheck confirma servicio Running. Se añadió espera explícita de estado en InstallUtil/script. Usuario cambió `pGina` a Manual y ejecutó las pruebas de login/offline detalladas arriba. No se ha probado reinicio/reversión ni se certifica el desinstalador legado.

Commit/push autorizados en las dos ramas actuales como estabilización, conservando estos pendientes explícitos. No incluir instaladores generados, logs, cachés, `.env`, datos de la VM ni claves de laboratorio. La DLL de log4net previamente versionada en SDK acompaña su migración autorizada; no agregar otros artefactos generados. La compilación no sustituye firma de publicación ni pruebas de actualización/reversión.
