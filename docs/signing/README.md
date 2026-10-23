# Preparación para firma Authenticode

Estado: **sin solicitud de firma activa y sin certificado configurado**. El workflow nuevo compila y entrega artefactos **sin firmar**. No modifica el release existente ni Docker Hub, no instala OpenCredential y no requiere secretos.

## Qué queda preparado

- `.github/workflows/desktop-unsigned.yml`: PR a master, push a master y ejecución manual; runner GitHub-hosted `windows-2025`, permisos `contents: read`, actions fijadas por SHA y checkout sin persistir credenciales. Los PR se compilan/prueban pero no generan artefactos promocionables.
- Git usa `core.longpaths=true` solo mediante el entorno del job para admitir rutas largas; no se cambia la configuración global del desarrollador. Los cachés de paquetes NuGet se restauran, no se versionan.
- `scripts/ci/Restore-DesktopDependencies.ps1`: restaura los `packages.config` versionados en las rutas esperadas usando exclusivamente nuget.org. No necesita evaluar proyectos C++ durante restore.
- `Build-OpenCredential.ps1 -PlatformToolset v143`: permite el toolset del runner VS 2022 sin cambiar el v145 predeterminado de los proyectos locales. El mismo parámetro está disponible para la regresión nativa.
- Paquete de evaluación explícitamente `unsigned`, con SHA256 y `build-info.json` (commit, estado limpio/sucio, versiones/herramientas y contexto público de Actions). El script rechaza un árbol sucio en CI y una salida existente con archivos. No se empaquetan `.env`, claves ni informes de VM.
- Política propuesta y privacidad: [Code signing policy](../../CODE_SIGNING.md), [Privacy policy](../../PRIVACY.md). Pedro Pablo Bermudez Medina (`@pedropablobm`) es el aprobador designado. No se ha configurado aún protección de rama/entorno en GitHub.

Esto es una compilación automatizada verificable, no una garantía de reproducibilidad bit a bit: las imágenes hospedadas y herramientas pueden cambiar. El build-info no reemplaza la verificación independiente de origen del conector SignPath.

### Validación local — 2026-10-23

En una exportación aislada del código, sin los cachés NuGet retirados del índice, pasaron la restauración de los 12 manifiestos, compilación Release x64/Win32 y plugins con `v145`, regresión nativa de credenciales en ambas arquitecturas, SQLite/DPAPI y carga de los ocho plugins, guardas del script de VM y compilación/preservación del instalador. No se ejecutó el instalador en el anfitrión. `actionlint` y los enlaces Markdown locales también pasaron. Hubo advertencias de compilación, sin errores; esto no acredita aún `v143` en el runner hospedado.

La limpieza retira del versionado 734 archivos de caché NuGet y `Plugins/SDK/log4net.pdb` (aproximadamente 224 MiB de contenido), conservando sus copias locales. No elimina historial Git, DLL necesarias, redistribuibles ni datos persistentes. En clones nuevos se debe restaurar NuGet antes de compilar.

La revisión de los binarios locales detectó que los ocho plugins conservan versión de componente `1.0.0.0` y productos distintos (LDAP sin nombre de producto); `OpenCredentialGINA.dll` también conserva `1.0.0.0`. Se registra para definir con el proveedor el alcance/metadatos de firma, sin cambiar versiones ni compatibilidad de ensamblados automáticamente.

## Validar antes de publicar estos cambios

1. Revisar el diff, la política/privacidad y CODEOWNERS; no cambiar la licencia sin aprobación.
2. Restaurar dependencias y compilar en una copia limpia. En local puede omitirse `-PlatformToolset` para usar el toolset instalado. En el runner se selecciona explícitamente v143.
3. Ejecutar `Tests/Test-CredentialSerialization.ps1`, `Tests/Test-Desktop.ps1`, `Tests/Test-GuestScript.ps1` y `Tests/Test-Installer.ps1`. Las pruebas no ejecutan el instalador en el anfitrión.
4. Para una vista previa local, `New-UnsignedArtifact.ps1` admite `-AllowDirtySource` explícito y marca el resultado como sucio. Ese bypass está prohibido cuando `GITHUB_ACTIONS=true`; no sirve para una solicitud de firma confiable.
5. Solo después de autorizar commit/push, comprobar un run real de GitHub Actions. La validación local no permite afirmar que un runner hospedado ya pasó.

## Solicitud gratuita: acciones del propietario

1. Solicitar en <https://signpath.org/apply> para el repositorio público y sus releases existentes. Explicar que es un fork de pGina y un proveedor de credenciales con servicio/gateway, modificaciones de inicio de sesión y desinstalador, no una aplicación de escritorio convencional.
   Usar el [borrador de solicitud](APPLICATION.md), verificando primero sus pendientes; no se ha enviado ni aceptado ningún formulario.
2. Revisar sus condiciones de licencia/forks/componentes y declarar los responsables/MFA. La aceptación por reputación/control/licencia es discrecional. GitHub detecta actualmente la licencia como NOASSERTION; revisar la declaración con el propietario, sin reescribir ni relicenciar automáticamente el código heredado.
3. Si lo aprueban, el propietario instala la aplicación GitHub de SignPath con acceso solo al repositorio necesario y configura el proyecto/origen hospedado. No entregar tokens ni claves al asistente.
4. Homogeneizar los metadatos de producto/versión de los binarios propios elegibles y definir una lista de artefactos estricta. No re-firmar DLL upstream ni redistribuibles Microsoft. Someter a revisión cualquier cambio de recursos/AssemblyInfo.
5. Configurar manualmente la política de producción con Pedro como aprobador y un entorno protegido para las solicitudes. Mantener credenciales de envío de mínimo privilegio en GitHub/environment secrets. No usar un certificado de prueba/autofirmado como si fuera públicamente confiable.
6. Incorporar la acción oficial de envío fijada por SHA y el artifact-id del mismo run; no aceptar URLs/cargas arbitrarias ni tomar artefactos de PR como releases. El workflow actual deliberadamente no contiene ese paso, valores de organización/proyecto supuestos ni un secret de firma.
7. Tras aprobación manual, verificar firmas/publisher/timestamp de todos los binarios seleccionados, generar SHA256 de los resultados firmados y publicar un release nuevo explícito. No reemplazar silenciosamente la RC sin firma.

La documentación del conector contempla subida de artefactos Actions v4 o posterior y verificación independiente de origen; debe revalidarse con la configuración efectiva asignada por SignPath. Fuentes: <https://signpath.org/terms>, <https://about.signpath.io/documentation/trusted-build-systems/github> y <https://github.com/actions/runner-images/blob/main/images/windows/Windows2025-Readme.md> (consultadas 2026-10-22).
