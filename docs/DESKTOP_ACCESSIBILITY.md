# Accesibilidad de la configuración Desktop

## Alcance

Mejora de la ventana principal WinForms de configuración, sin cambiar .NET Framework 4.8, el modo DPI, formatos de configuración, contratos de plugins, instalador ni proveedor nativo de credenciales. Las ventanas propias de cada plugin, AdminWeb y el inicio de sesión real de Windows no se rediseñan en este cambio.

- Cabecera y barra de acciones común: guardar y cerrar, aplicar cambios, cerrar sin guardar cambios pendientes. Las acciones ya aplicadas no se revierten al cerrar.
- General, selección/orden de plugins, simulación y opciones del proveedor mantienen sus controles/eventos, con distribución mediante tablas y desplazamiento cuando no cabe el contenido. Los controles de registro/servicio siguen actuando inmediatamente y ahora lo advierten junto a las acciones.
- Botones con texto en lugar de flechas sin nombre; nombres accesibles para campos, tablas y resultados. Tab sale de las tablas; las flechas navegan dentro. En simulación, Alt+U enfoca usuario y Alt+P contraseña.
- Alt+S guarda y cierra; Alt+A aplica; Alt+C cierra sin guardar cambios pendientes. No se asigna una acción global a Enter ni Escape para evitar guardados, simulaciones o cierres accidentales.
- Resultados de simulación en texto (Success, Failure, Not run), además de la tabla de resultados. Contraste mediante colores de Windows, sin depender de fondos rojo/verde o amarillo fijo.
- Las contraseñas siguen ocultas; la opción explícita Show Text conserva su funcionamiento. Los textos nuevos están en `ConfigurationUI.resx`, sin introducir una traducción parcial de toda la aplicación.

La composición está en `ConfigurationUI.Presentation.cs`; no se modifica el archivo generado `ConfigurationUI.Designer.cs`. Se conserva el camino público de inicialización normal. Un constructor **interno** permite crear únicamente la presentación para las pruebas, sin inicializar registro, servicio, carga de plugins ni datos reales. No existe una opción nueva de línea de comandos para saltarse la inicialización.

En esa ventana sintética, los botones capaces de guardar, simular, cargar plugins o cambiar el sistema están deshabilitados como defensa adicional frente a entrada accidental. Las capturas no representan estados reales del servicio ni una instalación del producto.

## Validación automatizada

Desde la raíz, en Windows PowerShell:

```powershell
# Modifica únicamente salidas locales de compilación y abre una ventana sintética fuera de pantalla.
# No instala, registra, autentica, escribe configuración ni controla servicios.
.\Tests\Test-ConfigurationUI.ps1

# Opcional: generar capturas del formulario sintético, nunca de configuración real.
.\Tests\Test-ConfigurationUI.ps1 -ScreenshotDirectory "$env:LOCALAPPDATA\Temp\opencode\ui-preview"
```

La prueba compila el proyecto administrado y el ejecutable de pruebas; comprueba nombres accesibles, salida real del grid con Tab, mnemónicos de usuario/contraseña, enmascaramiento, resultados textuales, ordenamiento con filas sintéticas, ausencia de solapamientos entre secciones y acceso por desplazamiento a controles finales. Revisa la barra de acciones en ventanas de 860×580 y 1200×900 y con escalado programático de controles al 150 %. No utiliza la simulación de autenticación real ni pulsa botones de guardado, servicio o registro.

También construye la pestaña Offline availability de Database Auth en modo aislado: verifica límites de antigüedad, nombres accesibles y acciones reales deshabilitadas. El ejecutable tiene un límite predeterminado de 60 segundos (`-TestTimeoutSeconds`, 1–600), independiente de la compilación. Si lo supera, el lanzador muestra las etapas TRACE y termina únicamente el proceso sintético que acaba de crear. No cierra aplicaciones existentes. Las excepciones de UI se propagan al ejecutable de pruebas en vez de abrir un diálogo de error fuera de pantalla.

GitHub Actions incluye esta prueba; su ejecución hospedada para estos cambios requiere publicarlos con autorización. Compilar/pruebas locales no acredita CI remoto.

## Aceptación manual pendiente

- Narrador o NVDA: lectura de campos, checkboxes del grid, cambios de estado y orden de exploración.
- Temas de contraste de Windows reales, tipografía ampliada y escalas DPI reales 125/150/200 %, incluidos cambios de monitor. El escalado programático no equivale a probar DPI reales.
- Visual Studio Designer: abrir, guardar, cerrar y reabrir el formulario; compilar y comprobar que no se reescriben los layouts runtime. No se declara validada esta operación a partir de una compilación.
- En VM piloto, configurar plugins existentes y comprobar el flujo completo sin perder configuración. Las pruebas de presentación no sustituyen login, actualización ni aceptación funcional.

No se afirma conformidad integral con WCAG/UI Automation ni compatibilidad de todas las extensiones por estos cambios.
