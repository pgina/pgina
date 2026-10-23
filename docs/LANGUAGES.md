# Idiomas y Acerca de

## Alcance implementado

La configuración principal permite seleccionar inglés, español o detección automática por el idioma de interfaz de Windows. Se traducen acciones, pestañas, etiquetas, estados, encabezados, nombres accesibles, confirmaciones, solicitudes de acceso administrativo y errores comunes de configuración. Los textos técnicos de logs, nombres/descripciones proporcionados por plugins y mensajes devueltos por estos conservan su origen.

Se han conectado las ventanas de los **ocho plugins oficiales**: Database Auth, Database Logger, SessionLimit, SingleUser, Sample, LDAP, LocalMachine y DriveMapper. Usan el idioma seleccionado en el proceso de configuración, con módulos independientes dentro del mismo paquete (`DatabaseAuth`, `DatabaseLogger`, `SessionLimit`, `SingleUser`, `Sample`, `Ldap`, `LocalMachine`, `DriveMapper`). Se traducen etiquetas, botones, validaciones, reglas de autorización/integración local, resúmenes de pruebas de tablas y estado de sincronización offline. Las reglas se formatean solo para mostrar: se conservan sus objetos, condiciones y serialización. Los fallos SQL se presentan sin imprimir la excepción completa en la ventana.

No cambia la autenticación, los valores persistidos, el formato de datos ni `CurrentCulture`. El visor de logs traduce sus controles, no el contenido técnico del registro. Se conservan SQL, rutas, nombres de grupos, enum/algoritmos técnicos, mensajes personalizados y respuestas arbitrarias de plugins/proveedores. Las opciones GINA de Windows antiguos están fuera del soporte actual y no forman parte de la traducción. AdminWeb sigue siendo otro repositorio y no se modifica.

Las ocho ventanas de plugins adaptadas utilizan composición runtime y mantienen intactos sus archivos `*.Designer.cs`. Sus campos se distribuyen mediante tablas/secciones con desplazamiento y acciones fuera del área desplazable para admitir textos más largos. Se conserva el valor original de modos TLS, proveedores, nombres SQL, UUID, condiciones e identificadores de enlace de datos. Los textos de opciones que se guardan por índice/objeto pueden tener una presentación traducida sin sustituir su valor. El texto legal de Acerca de continúa siendo el original.

## Selección

1. Abrir la configuración por su procedimiento administrativo normal.
2. En **Idioma / Language**, seleccionar **Español**, **English** o **Automático (Windows)**.
3. Pulsar **Guardar idioma** y volver a abrir la configuración. No se guarda ninguna otra edición pendiente ni se reinicia el servicio. Cerrar sin guardar puede descartar esas otras ediciones; el aviso de idioma lo indica.

La preferencia se guarda por usuario en `HKCU\Software\OpenCredential\Configuration\Language`. No modifica las opciones de autenticación en HKLM. El selector no cambia las ventanas ya abiertas ni el idioma de la pantalla de inicio de sesión. En una cultura regional como `es-CO`, cada clave busca primero el paquete regional, después `es` y finalmente el texto inglés integrado. Un archivo externo español dañado conserva el español integrado.

### Pantalla de inicio de sesión Windows

**General → Acceso a la configuración → Idioma del inicio de sesión Windows (todos los usuarios)** es una opción independiente: automático según el idioma del **sistema** Windows, inglés o español. Se guarda con Aplicar cambios/Guardar y cerrar en `HKLM\SOFTWARE\OpenCredential3\SignInLanguage`, vista nativa del sistema. Ambos proveedores (x64/Win32) leen esa misma vista mediante un búfer limitado y únicamente aceptan una cadena de texto; un fallo de lectura usa el idioma automático y un valor desconocido usa inglés. No cambia la preferencia personal de la configuración.

Los textos nativos integrados cubren etiquetas de usuario/contraseñas, bloqueo, progreso predeterminado, cancelación, resultados genéricos, estado del servicio y cambio de contraseña. Se aplican a pantallas de inicio de sesión creadas posteriormente; si Windows conserva una pantalla anterior puede ser necesario recrearla. No se cambia el protocolo con el servicio, identificadores internos como `MOTD`, serialización RDP/NLA ni contenido de credenciales. Los mensajes personalizados de bienvenida/progreso y las respuestas arbitrarias de plugins se conservan.

**Los paquetes XML externos no se cargan en LogonUI.** El proveedor nativo usa solo textos compilados en inglés/español y la preferencia administrativa por equipo, sin acceder a HKCU, archivos de usuario ni red. Añadir otro idioma al proveedor nativo requiere ampliar sus recursos y recompilarlo. Es un límite deliberado de seguridad, no una importación incompleta que se intente ejecutar durante el login.

### Instalador

Inno Setup ofrece inglés/español, incluidos los mensajes de dependencias y errores posteriores de OpenCredential. El idioma del instalador no cambia automáticamente ninguna preferencia de la configuración o del inicio de sesión; las interfaces de redistribuibles Microsoft y los errores del sistema conservan su idioma propio. La licencia original no se traduce ni modifica.

## Importar un idioma

**Importar idioma…** acepta exclusivamente XML de texto. La aplicación valida el archivo y lo instala con el nombre canónico de su cultura en `Languages` junto al ejecutable de configuración. En una instalación normal es un subdirectorio protegido de Program Files; no se recomienda hacer ese directorio escribible para usuarios sin privilegios. En entornos de desarrollo se utiliza la carpeta de salidas locales.

La importación requiere permisos de escritura sobre esa carpeta y no sobrescribe un idioma externo ya instalado. Una importación inválida no sustituye el archivo anterior. Después de importar, seleccionar el idioma y guardar la preferencia; reabrir la configuración para aplicarlo. El idioma inglés y el español están integrados y funcionan sin archivos externos.

Ejemplo de paquete parcial:

```xml
<?xml version="1.0" encoding="utf-8"?>
<language schemaVersion="1" culture="es-CO" nativeName="Español colombiano">
  <module id="Configuration">
    <text key="Action.Apply">Aplicar cambios</text>
    <text key="About.Link">Acerca de…</text>
  </module>
</language>
```

Se puede partir de `OpenCredential/src/Shared/Localization/es.xml`, cambiar `culture` y `nativeName` y traducir sus valores. Las claves distinguen mayúsculas y minúsculas; no se traducen. Algunas corresponden a recursos semánticos (`Action.Apply`) y otras a controles concretos (`Control.m_pluginConfigTab`). Las claves aún no utilizadas se ignoran. No se cargan DLL, scripts, objetos serializados ni `.resx` externos.

- Formato versión 1, cultura reconocida por .NET Framework 4.8.
- Hasta 512 KiB por archivo, 4096 entradas y 8192 caracteres por texto.
- DTD y entidades externas prohibidas; claves duplicadas y formatos inválidos rechazados.
- Conservar marcadores como `{0}`, `%u` y `%v`. Si los marcadores no coinciden con los del texto original, esa entrada utiliza el respaldo. Una traducción incompleta es válida.
- `&amp;` marca una tecla de acceso en WinForms; evitar colisiones dentro de una ventana. El español utiliza Alt+U para usuario y Alt+C para contraseña en la simulación.
- Los paquetes son contenido de confianza administrativa: pueden modificar advertencias visibles, aunque no ejecuten código. Revisar su procedencia y traducción antes de importarlos.

## Acerca de

El enlace permanente **Acerca de…** abre una ventana traducida con versión informativa, créditos a **Yuri Mercedes Bermúdez Mazuera** y **Pedro Pablo Bermúdez Medina**, origen pGina y texto legal original BSD-3-Clause. `LICENSE` y `NOTICE` se incrustan durante la compilación sin modificar sus contenidos, por lo que están disponibles sin conexión. El URL de GitHub se puede copiar; no se abre automáticamente un navegador desde el proceso elevado. No se atribuye a estos desarrolladores la autoría exclusiva del código de pGina ni de dependencias externas.

## Solicitud adicional de contraseña administrativa

En **General → Acceso a la configuración** se puede activar **Solicitar de nuevo la contraseña de administrador al abrir la configuración**. Está **desactivada por defecto**, incluso en instalaciones existentes que aún no tengan ese ajuste. Para cambiarla, usar Aplicar cambios o Guardar y cerrar; se aplica en la siguiente apertura. Cerrar sin guardar no cambia la opción. Como las demás opciones de General, una simulación que confirme el guardado de cambios pendientes también la guarda.

Esto solo controla la segunda solicitud dentro de OpenCredential: **se mantienen el manifiesto `requireAdministrator`, UAC y la comprobación de pertenencia administrativa**. Windows puede seguir mostrando su propio aviso UAC o pedir credenciales según su política. Al activar la opción adicional se conserva el límite de tres intentos y la validación de identidad/SID de la misma cuenta administrativa que ejecuta la interfaz. Cancelar o fallar la validación impide abrirla. No se almacena ninguna contraseña.

La política por equipo se guarda como `RequireConfigurationReauthentication` en el árbol existente `HKLM\SOFTWARE\OpenCredential3` (vista de registro del proceso). No se consulta HKCU ni un archivo de idioma para decidirla. El arranque solo lee la política, sin inicializar plugins ni crear valores antes de comprobar acceso. La ausencia de valor significa desactivada; un valor corrupto o un error de lectura deniega acceso. La inicialización de valores predeterminados conserva una opción ya activada. Ante corrupción, un administrador deberá revisar/restaurar este valor desde una sesión de recuperación autorizada; no se proporciona un bypass por línea de comandos.

Las pruebas usan lectores de política y validadores sintéticos: verifican el valor ausente/desactivado, opción activada, cancelación/fallo, rechazo de no administradores y política corrupta/inaccesible. No validan credenciales reales ni modifican HKLM. Queda pendiente comprobar en VM la persistencia al guardar/reabrir y los avisos UAC/CredUI reales.

## Validación y pendientes

`Tests/Test-ConfigurationUI.ps1` comprueba presentación aislada en inglés/español, nombres accesibles, mnemónicos, importación sintética, respaldo regional, XML inseguro/dañado, tamaño excesivo, marcadores incompatibles, conservación de archivos y contenido legal idéntico al repositorio. No escribe preferencias reales, no importa al directorio de la aplicación y no autentica usuarios reales.

La misma prueba compila y construye de forma aislada los ocho plugins. Verifica inglés/español, ausencia de solapamientos entre campos visibles, acciones finales accesibles al reducir la ventana, desplazamiento hasta el último campo del esquema, contraseñas ocultas, identidad de proveedores/TLS/columnas UUID y datos sintéticos conservados. Comprueba también formato de reglas sin alterar su serialización, resumen offline con UTC explícito y errores sin contenido de excepciones sintéticas. Todos los botones con acciones reales quedan deshabilitados en estas ventanas de prueba. El plugin Sample no crea su objeto de ajustes en modo aislado.

El modo de prueba es un constructor interno de presentación, no una opción de línea de comandos ni una vía de acceso administrativo alternativa. No se ejecutan las pruebas de conexión/creación de tablas ni los cambios de autenticación reales.

`Tests/Test-CredentialSerialization.ps1` comprueba en x64/Win32 selección automática/inglés/español, respaldo de textos, conservación de marcadores e identificadores, además de las regresiones de serialización. No lee el registro ni autentica usuarios reales. `Tests/Test-Installer.ps1` verifica los idiomas/mensajes y compila un instalador de prueba, sin ejecutarlo.

La implementación automatizada no sustituye la aceptación manual: antes de distribuir, validar selección/reapertura/importación, login/desbloqueo/cambio de contraseña/RDP en VM, lectores de pantalla, DPI reales y textos largos. El round-trip del diseñador Visual Studio no está validado. Los instaladores compilados por la prueba no son un release publicado ni se instalan en el anfitrión.
