# Estado de validación antes de commit/push

## Actualización de release estable: 2026-10-23

- El propietario confirma que ejecutó el instalador local `desktop-idiomas-prueba-20261023-160659` y que funciona correctamente, y autoriza commit/push y un nuevo release estable Desktop `v1.0.1.0`. SHA256 del instalador probado: `eef0c38905ecbb58e97545d44be5af7db3db458eabf1509dd54f5e913c877579`.
- Ese artefacto de prueba contenía los cambios locales y aún mostraba `1.0.1.0-rc.1`. El instalador estable se recompila desde el commit de release, con metadatos `1.0.1.0`, checksums y procedencia propios; no se presenta como idéntico byte a byte al probado.
- Regresiones locales de esta entrega: configuración e idiomas de los ocho plugins, importación XML, créditos/licencia, política administrativa opcional, caché offline, carga de tipos, selección de idioma nativo y serialización x64/Win32. Instalador bilingüe compilado sin ejecutarlo por el agente.
- No se deduce de la confirmación general del propietario una nueva matriz completa de login/RDP/DPI/NVDA/diseñador. Persisten los límites de aceptación detallados en `docs/LANGUAGES.md` y `docs/OFFLINE_SYNCHRONIZATION.md` y los pendientes históricos de abajo.
- Se conserva `AssemblyVersion=1.0.0.0`; versiones de archivo/producto `1.0.1.0`. El instalador sigue sin firma Authenticode. AdminWeb, Docker Hub y el prerelease anterior no se modifican.

## Historial de estabilización

Actualizado: 2026-10-21. El usuario autorizó commit/push de estabilización en Desktop `master` y AdminWeb `main`, incluida la migración previa a log4net. La publicación inicial se completó y también se autorizó publicar las correcciones posteriores de RDP/grupos y sus pruebas en Desktop `master`. No equivale a publicar una versión certificada ni un instalador firmado.

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
| RDP | PASS en VM con NLA | Usuario confirma escritorio remoto; sesión online, autenticación SQL y logoff verificados tras corregir credenciales nativas y consulta de grupos |
| Cambio de contraseña web → Windows | PENDING | No certificado por las pruebas anteriores |
| Desinstalación y reversión reales | PASS en VM | Marcadores/6 archivos preservados, reinicio y acceso nativo; snapshots original/validado restaurados y comprobados |
| CA, SMTP y proxy institucionales | PENDING | No se usaron servicios o credenciales reales |
| Carga y paginación SQL completa | PENDING | Mejora funcional aún no implementada; el snapshot conserva colecciones completas |

## VM autorizada

### Ampliación: Docker Hub y respaldo previo a desinstalación

- Snapshot frío del estado validado: `OpenCredential-validated-before-uninstall-20261020-160640`, UUID `914db756-2064-43e0-ade1-0de8b16d7576`. Se conserva también el snapshot anterior; el usuario autorizó desinstalación y restauración solo de esta VM, retornando después al estado validado.
- Preflight inicial del invitado: Windows 11 Pro (SKU 48), RDP desactivado y reglas RDP deshabilitadas. Después se habilitó la prueba temporal autorizada descrita abajo y se retiraron sus cambios al finalizar.
- AdminWeb publicado en Docker Hub desde `74cdb3e`: `pedropablobm/opencredential-adminweb:test` y `:sha-74cdb3e`, Linux amd64. Digest remoto verificado para ambas etiquetas: `sha256:df7e7f9145fc979d1beee25a7d1088c00741cd231b6e59fe6173409eddd4cba6`.
- Imagen validada en contenedor aislado sin red externa: login, summary autenticado, PDF, readiness y exclusión de archivos de pruebas/configuración. No se recrearon servicios existentes.
- `latest` no se modificó (digest remoto conservado `sha256:26a34e34abc2cb4f154976b663db292aae67fdca11c97a6487fd4bbcc3c3dea2`). Los despliegues que usan `latest` NO reciben esta actualización; deben seleccionar una etiqueta publicada expresamente.
- `Invoke-GuestUninstallValidation.ps1`: guardas VM/admin, confirmación explícita, marcadores de datos/registro y hashes de archivos persistentes tras detener servicio; verifica retirada de servicio/proveedor y preservación. No reinicia, restaura snapshots ni exporta configuraciones. Resultado ejecutado descrito a continuación.
- Desinstalación ejecutada: salida 0, servicio/proveedor retirados, marcadores preservados y seis archivos persistentes con hashes idénticos. Reinicio posterior y acceso administrativo nativo confirmados; comprobación automática verifica boot posterior y ausencia de servicio/proveedor.
- Reversión ejecutada con la VM apagada: snapshot original y posterior retorno al snapshot validado. Ambos informes PASS; el segundo verifica servicio Running, proveedor, usuario sintético, tres SQLite y hashes de ambos plugins. No se eliminan snapshots.
- RDP autorizado temporalmente: NLA activa, permiso de usuario sintético y regla TCP solo para 10.0.2.2; reenvío NAT solo 127.0.0.1:43389. La primera prueba alcanza OpenCredential remoto pero genera rechazos/bloqueo en SQL. Se detectó copia directa de contraseñas CredProtect serializadas; regresión nativa reproduce pérdida de plaintext y corrección añade descifrado/fallo cerrado antes de los plugins. La instalación, repetición y retirada de permisos/RDP/firewall/reenvío se completaron como se detalla abajo.

### RDP retomado 2026-10-21

- Usuario desinstaló/reinstaló manualmente el instalador anterior. Se conservaron rutas de búsqueda de pGina que ya no correspondían a la carpeta actual. Tras ajustar manualmente a `C:\Program Files\OpenCredential\Plugins`, cargar los ocho plugins y reiniciar servicio, la simulación con servicio real vuelve a pasar. No se borró configuración ni cachés para resolverlo.
- `UnprotectAndCopyPassword` distingue texto normal de credenciales CredProtect y usa CredUnprotect antes de autenticar por plugins. Si Windows no puede descifrar, devuelve HRESULT sin entregar la cadena protegida al servicio; el proveedor comprueba ese error antes de inicializar la credencial. Buffers temporales se borran antes de liberarse. No se modifica protección de salida ni NLA.
- Pruebas nativas x64/Win32 aprobadas: texto normal y Unicode/espacios, vacío/nulo, round-trip real CredProtect y errores simulados de contexto/descifrado antes y después de reservar memoria. Pruebas no registran proveedores ni autentican usuarios reales.
- Compilación completa Release x64/Win32 y Desktop/SQLite/DPAPI/carga de plugins aprobadas. Instalador RdpFix separado compilado para instalación manual solo en la VM; comparar DLL de System32/SysWOW64 y reinicio con `Inspect-GuestRdpBuild.ps1` antes de nuevos intentos. No se declara corregido el RDP integral hasta probarlo.
- Paquete nativo RdpFix instalado: hashes System32/SysWOW64, servicio, proveedor, NLA y boot posterior aprobados. Siguiente intento RDP es denegado por derechos, antes de autenticar en SQL. Inspección de política confirma usuario habilitado/sin bloqueo pero fuera de Usuarios de escritorio remoto; sin coincidencias de denegación. Local Machine sincroniza grupos estrictamente y puede retirar membresías añadidas manualmente.
- Para el ensayo temporal se creó `OC_QA_RDP_Only` únicamente en la base sintética y se asignó solo a `ocqa_student`. Usuario añadió regla Gateway de Database Auth hacia el grupo local Usuarios de escritorio remoto y lo ordenó antes de Local Machine. Esta regla y membresía/grupo SQL deben retirarse al finalizar, junto a los permisos/reglas RDP anteriores; no añadir el grupo RDP a MandatoryGroups de todos los usuarios.
- La primera emulación con grupos descubre SQLSTATE 42883 (integer=text). `IsMemberOfGroup` convertía la clave a string. Corrección mínima: mantener el objeto/tipo devuelto por PostgreSQL y rechazar NULL como no miembro. La regresión con el mismo código y tablas temporales reproduce el fallo antes del cambio; incluye claves integer/bigint/uuid/text, membresía positiva/negativa, usuario inexistente, NULL y nombre parametrizado. No se modifican columnas del esquema de laboratorio ni producción.
- Corrección de grupos aprobada en las cuatro variantes y paquete combinado RdpGroupsFix compilado/instalado manualmente. Usuario confirma servicio en verde, pertenencia RDP con política permitida y escritorio remoto. SQL confirma sesión de Windows 2 online iniciada 2026-10-21 14:38:11 UTC y cerrada 15:29:46 UTC con estado ended/logoff, sin bloqueo ni intentos fallidos.
- Retirada temporal completada: usuario elimina regla Gateway y desactiva solo Gateway de Database Auth. `rdp-restored.json` confirma ajustes RDP originales, regla de firewall retirada y membresía local restaurada. Desde el anfitrión se elimina únicamente reenvío ocqa-rdp; puerto 43389 ya no escucha. SQL retira solo OC_QA_RDP_Only y su membresía sintética, verificando que Estudiantes se conserva. `rdp-final-cleanup.json` del 2026-10-21 15:44:55 UTC aprueba nueve comprobaciones: autenticación Database Auth y gateway Local Machine conservados, regla/gateway/membresía RDP temporales retirados, RDP desactivado, NLA intacta, firewall limpio y servicio Running.

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
- Snapshot previo: `OpenCredential-before-validation-20261019-105158` (`a60dc808-c01d-424b-8f26-adbb036cc90c`). Restaurado y validado en la prueba autorizada; no eliminado. Después se regresó al snapshot validado, también conservado. Esos snapshots son anteriores a las correcciones posteriores RDP/grupos.
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
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tests\Test-CredentialSerialization.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\OpenCredential.AdminWeb\Tests\Test-Transport.ps1
dotnet run --project .\OpenCredential.AdminWeb\Tests\RegressionTests.csproj
node --test .\OpenCredential.AdminWeb\Tests\frontend.test.cjs
```

Dentro de la VM, seguir `Tests/Guest-README.md`. El script no ejecuta login con cuentas reales, no exporta secretos, no reinicia, no desinstala ni restaura snapshots automáticamente. Los casos manuales permanecen Pending hasta ejecutarse y documentarse.

## Condiciones para publicar

Resultado observado del primer ensayo en VM: salida 0, marcadores conservados y proveedor registrado; recheck confirma servicio Running. Se añadió espera explícita de estado en InstallUtil/script. Usuario cambió `pGina` a Manual y ejecutó las pruebas de login/offline detalladas arriba. Posteriormente se validaron reinicio, desinstalación, reversión y RDP en esta VM concreta; no se certifican otros desinstaladores legados ni plataformas no ensayadas.

Commit/push autorizados en las dos ramas actuales como estabilización, conservando estos pendientes explícitos. No incluir instaladores generados, logs, cachés, `.env`, datos de la VM ni claves de laboratorio. La DLL de log4net previamente versionada en SDK acompaña su migración autorizada; no agregar otros artefactos generados. La compilación no sustituye firma de publicación ni pruebas de actualización/reversión.
