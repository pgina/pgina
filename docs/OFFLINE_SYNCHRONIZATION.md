# Réplica local de usuarios: alcance y validación

## Estado de la entrega

Funcionalidad incluida en Desktop `v1.0.1.0`. PostgreSQL y MySQL/MariaDB siguen siendo proveedores directos; no se requiere AdminWeb ni una API para sincronizar. No cambia el contrato público del plugin.

La réplica completa ya incluía usuarios que nunca habían iniciado sesión en el equipo. Esta entrega refuerza su actualización y permite consultar su estado. Replica material mínimo de autenticación (identidad, algoritmo/hash protegido, estado y bloqueo), no toda la base SQL ni sus grupos.

## Uso

1. Guardar la configuración de Database Auth y habilitar la caché local y el fallback offline según la política institucional.
2. Reabrir el plugin y seleccionar **Offline availability**. Consultar existencia de la copia, número de usuarios, registros dentro del límite de antigüedad, última sincronización completa y último intento (UTC).
3. Usar **Synchronize now** para actualizar con los ajustes **guardados**. Las ediciones sin guardar no se utilizan. La acción modifica la caché inmediatamente: Cancel no la revierte. Guardar está deshabilitado mientras se ejecuta; cerrar la ventana no cancela una sincronización iniciada.
4. Configurar **Maximum offline record age (hours)** entre 1 y 168 y guardar. El valor predeterminado es 24 horas. El cambio no renueva las fechas de los registros.

El temporizador revisa por defecto cada 30 segundos si corresponde sincronizar; el intervalo de sincronización predeterminado es de 5 minutos. Se conserva la configuración existente de estos intervalos. Un intento no debido no consulta SQL; no equivale a comprobar que el servidor esté disponible.

## Garantías y límites

- La descarga se materializa y valida antes del reemplazo. Null, identidades vacías/duplicadas, enumeración interrumpida y errores SQL/SQLite no sustituyen la última copia completa.
- Usuarios y metadatos de éxito se confirman en una transacción SQLite. Un fallo no renueva la fecha de éxito ni la antigüedad de los registros. El último intento puede quedar marcado como fallido o en curso si el proceso se interrumpe.
- Una respuesta **vacía válida** sí elimina los usuarios replicados: no se confunde con un proveedor MySQL desconectado, que ahora lanza un error.
- Un mutex global por ruta coordina sincronizaciones entre servicio y configuración, además de la exclusión dentro del proceso. No es un bloqueo de toda la autenticación ni sustituye las transacciones SQLite.
- El límite de antigüedad se aplica a cada registro. Las actualizaciones individuales online pueden tener una fecha posterior a la última copia completa. Se rechazan fechas de actualización que exceden en más de dos minutos el reloj UTC actual; esto no constituye protección integral contra manipulación del reloj.
- Los hashes se protegen con DPAPI de máquina y permisos de archivo restringidos. No hay vinculación criptográfica de la copia con la fuente SQL: al cambiar servidor, base o tabla debe revisarse la política antes de permitir fallback con una copia anterior. No se debe copiar la caché entre equipos.
- Contar registros frescos **no** significa que todas esas cuentas puedan entrar: se siguen comprobando contraseña, estado habilitado, bloqueo, fallback y autorización. Las reglas offline dependientes de grupos siguen fallando de forma restrictiva.
- Un usuario desconocido, contraseña incorrecta o cuenta bloqueada no se convierte en invitado. No se implementa aquí el plugin invitado ni una concesión temporal de acceso.
- El diagnóstico nuevo muestra cantidades y fechas, no identidades ni hashes. No compartir archivos de caché ni configuraciones sensibles.

## Validación reproducible

Desde la raíz, Windows PowerShell; estos comandos generan salidas locales de compilación y datos sintéticos, sin instalar Desktop ni autenticar usuarios reales:

```powershell
.\Tests\Test-Desktop.ps1
.\Tests\Test-ConfigurationUI.ps1
.\Build-OpenCredential.ps1 -Configuration Release
```

Las regresiones de caché cubren primer acceso offline de un usuario replicado sin login previo, DPAPI, contraseña, estado, bloqueo, caducidad, timestamps futuros, conservación ante respuestas fallidas/parciales/null/duplicadas, rollback conjunto de usuarios y metadatos, exclusión entre procesos y revocación con snapshot vacío. El test de MySQL cerrado inspecciona el ensamblado compilado; no abre una base de datos real. Las pruebas UI solo inspeccionan presentación aislada y no ejecutan sincronización real.

### Aceptación pendiente en VM

Con snapshot previo y una cuenta administrativa local de recuperación:

1. Replicar un usuario activo que no exista como cuenta Windows local y que nunca haya iniciado sesión en la VM.
2. Interrumpir únicamente la conectividad SQL de la VM y probar el primer login completo, incluida autorización y creación de cuenta por el gateway configurado. Las pruebas del motor de caché no acreditan ese flujo Windows.
3. Verificar rechazo de contraseña incorrecta, usuario inexistente, inactivo, bloqueado, registro vencido y autorización por grupos sin conexión.
4. Recuperar SQL, revocar la cuenta, sincronizar y verificar su rechazo offline. Probar ambos proveedores por separado.
5. Comprobar sincronización desde configuración y servicio, cierre de ventana durante la operación, recuperación tras error y accesibilidad con Narrador/NVDA y DPI reales.

La confirmación general de funcionamiento del instalador por el propietario para el release estable no sustituye la aceptación específica anterior. No se ha ejecutado el round-trip del diseñador de Visual Studio.
