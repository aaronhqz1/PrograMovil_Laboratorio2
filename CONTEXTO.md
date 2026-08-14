# Contexto del Laboratorio #2 — Módulo de Sucursales

Archivo de trabajo para no perder el hilo entre sesiones. Editar según avance el proyecto.

## Origen
- Documento: `Laboratorio_2_Modulo_Sucursales_Profesional.pdf` (Universidad Latina de Costa Rica, curso Programación Móvil).
- Entrega: miércoles 12 de agosto, antes de las 11:59 p.m., por Campus Virtual.

## Objetivo
Módulo completo de administración de sucursales de una clínica veterinaria, en **.NET MAUI** + **Firebase Cloud Firestore**.

## Requerimientos funcionales (checklist)
- [x] Registrar una sucursal
- [x] Listar las sucursales registradas
- [x] Consultar el detalle de una sucursal
- [x] Modificar la información
- [x] Eliminar una sucursal con confirmación
- [x] Validaciones de campos obligatorios

Campos mínimos por sucursal: Nombre, Dirección, Teléfono, Horario de atención, Encargado, Descripción.

## Criterios de evaluación (100 pts)
| Criterio | Puntos |
|---|---|
| Registro | 20 |
| Listado | 20 |
| Consulta | 15 |
| Edición | 20 |
| Eliminación | 15 |
| Diseño y validaciones | 10 |

## Entregables
- [x] Proyecto completo en Visual Studio (`VetSucursales/`)
- [x] Código fuente funcional
- [x] Base de datos en Firestore — proyecto real `programovillaboratorio2` creado en Firebase Console, Project ID ya pegado en `Services/FirebaseConfig.cs`. Reglas confirmadas por el usuario: modo de prueba, `allow read, write: if request.time < timestamp.date(2026, 9, 12);` — abiertas hasta esa fecha. **Recordatorio**: hay que endurecerlas o renovarlas antes del 12-sep-2026, o el CRUD empezará a fallar con 403.
- [ ] Video de demostración (5–8 minutos) — pendiente de grabar por el estudiante

## Estado de la implementación
Proyecto `VetSucursales/` (.NET MAUI, targets: `net10.0-android`, `net10.0-windows10.0.19041.0`; se quitó iOS/MacCatalyst porque este entorno de desarrollo es Windows sin Mac de compilación).

- `Models/Sucursal.cs` — modelo de datos (Id, Nombre, Dirección, Teléfono, HorarioAtencion, Encargado, Descripción) + `Clone()`.
- `Services/FirebaseConfig.cs` — configurado con el proyecto real: `ProjectId = "programovillaboratorio2"`. `ApiKey` vacío (peticiones anónimas, ver reglas de Firestore en "Entregables").
- `Services/IFirestoreService.cs` / `FirestoreService.cs` — servicio centralizado (inyectado por DI como singleton) con el CRUD completo contra la API REST de Firestore. Ningún ViewModel llama a Firestore directamente. No requiere `google-services.json` ni SDK nativo de Firebase.
- `ViewModels/` — `SucursalListViewModel`, `SucursalFormViewModel` (registrar/editar), `SucursalDetailViewModel`. MVVM con `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Manejo de errores con try/catch en toda operación de I/O, expuesto vía `BaseViewModel.ErrorMessage`/`HasError`.
- `Views/` — `SucursalListPage`, `SucursalFormPage`, `SucursalDetailPage`. Code-behind solo hace `InitializeComponent` + wiring de DI (y `OnAppearing` → `LoadCommand` en la lista); cero lógica de negocio fuera de los ViewModels.
- Navegación: rutas registradas en `AppShell.xaml.cs` (`Routing.RegisterRoute`), parámetro `Id` pasado vía `QueryProperty` a Form/Detail. Back button automático de Shell.
- `Platforms/Android/AndroidManifest.xml` — tiene `INTERNET` y `ACCESS_NETWORK_STATE`.
- Build verificado: `dotnet build -f net10.0-windows10.0.19041.0` y `dotnet build -f net10.0-android` — ambos compilan sin errores (14-ago-2026).
- Ejecución verificada en el emulador Android (`emulator-5554`) con datos reales de Firestore: navegación Listado → Detalle → Editar funcionando, back button de Shell automático, tarjetas y FAB renderizando correctamente.

### Pase de UI/UX (14-ago-2026)
Se revisó el módulo contra un checklist de 10 estándares de UI/UX y una lista de buenas prácticas de arquitectura. La arquitectura ya cumplía todo (MVVM, DI, manejo de errores, async/await, rutas de Shell); se encontraron y corrigieron estas brechas de UI/UX:

- **Feedback de éxito**: no existía ningún mensaje tras guardar/editar/eliminar (solo se navegaba de vuelta). Se agregó el paquete `CommunityToolkit.Maui` (v13.0.0 — es la versión más alta compatible con el `Microsoft.Maui.Controls` 10.0.20 que trae el workload instalado; versiones más nuevas del toolkit exigen Controls ≥ 10.0.60 y producían downgrade conflict) y se usa `Toast.Make(...).Show()` en `SaveAsync` (Form) y `DeleteAsync` (Detail).
- **Validación en tiempo real**: antes solo se validaba al presionar "Guardar". Se agregó `SucursalFormViewModel.ValidateFieldsCommand`, enlazado al evento `Unfocused` de cada `Entry`/`Editor` vía `toolkit:EventToCommandBehavior` (sin lógica en code-behind).
- **Teléfono**: la validación pasó de "contar dígitos" a una expresión regular (`^\+?[0-9\s\-\(\)]{7,20}$`) que además exige mínimo 8 dígitos.
- **Campos inválidos**: cada `Border` que envuelve un `Entry`/`Editor` tiene un `DataTrigger` que cambia el `Stroke` a rojo (`DangerBrush`) cuando el error correspondiente no está vacío — antes los campos no tenían borde visible en absoluto.
- **Estado vacío**: se agregó un botón "Registrar sucursal" explícito (antes solo había un texto sugiriendo tocar el FAB).
- **Contraste**: el color del ícono ">" en las tarjetas de la lista se oscureció (Gray300→Gray400 en claro) para acercarse al mínimo AA de contraste no-textual.
- **Diseño visual**: se reutilizó la paleta existente (`Colors.xaml`) agregando solo tokens semánticos que faltaban (`Danger`, `DangerSurface`, `CardBackground`) — no se introdujo una paleta nueva. Se agregaron estilos reutilizables en `Styles.xaml` (`CardBorder`, `FieldBorder`, `FieldLabel`, `CaptionLabel`, `FieldErrorLabel`, `ErrorBanner`, `SecondaryButton`, `DangerButton`, `FabButton`, `AvatarBorder`/`AvatarLabel`, `PageTitle`).
- **Listado**: tarjetas con avatar (inicial del nombre), ícono de dirección/teléfono, chevron de navegación, y FAB circular (patrón Material) para "agregar" en vez del botón de ancho completo anterior.
- **Detalle**: encabezado tipo "hero" (avatar + nombre + encargado) seguido de una tarjeta agrupada con íconos por campo y separadores.
- **Accesibilidad**: `SemanticProperties.Description` en el FAB y en cada tarjeta de la lista (para lectores de pantalla).
- Limitación de tooling encontrada: el compilador XAML de este proyecto (`MauiXamlInflator=SourceGen` y también el XamlC clásico) no resuelve `BasedOn="{StaticResource {x:Type Button}}"` (error `Key must be a string literal` / `XC0009`). Los estilos `SecondaryButton`/`DangerButton`/`FabButton` repiten las propiedades base de `Button` en vez de heredar del estilo implícito.

## Pendiente / próximos pasos
1. Probar manualmente el flujo completo de validación (campo vacío al perder foco, teléfono inválido, Guardar con errores) en el emulador — se verificó navegación y renderizado visual, pero no cada combinación de validación.
2. Grabar el video de demostración (5–8 min) mostrando las 5 operaciones CRUD + validaciones + feedback visual nuevo.
3. Antes de entregar: revisar las reglas de Firestore antes del 12-sep-2026 (fecha de expiración del modo de prueba).
4. Opcional (no bloqueante, mencionado como "opcional" en el checklist de UI/UX): swipe-to-delete en la lista con `SwipeView`, y una validación de ancho para tablets.

## Notas de diseño / decisiones tomadas
- Se usó la API REST de Firestore con `HttpClient` (no el SDK gRPC de Google.Cloud.Firestore) por compatibilidad con Android/iOS en MAUI.
- El servicio se llama `IFirestoreService`/`FirestoreService` (no `ISucursalService`) — es funcionalmente equivalente a lo pedido (CRUD centralizado, inyectado por DI, sin llamadas directas desde ViewModels); no se renombró para evitar churn sin beneficio funcional.
- Iconografía: se usan glifos Unicode/emoji (📍📞🕐📝) en vez de una fuente de íconos dedicada, para no agregar una dependencia nueva solo por eso; es un solo set usado consistentemente en Listado y Detalle.
- Feedback de éxito vía `CommunityToolkit.Maui` Toast (no Snackbar, que requeriría una acción/botón que no aplica aquí); los errores se mantienen en un banner inline (persistente, el usuario lo lee a su ritmo) en vez de un toast transitorio.
- Validación de campos obligatorios en `SucursalFormViewModel.Validate()` (los 6 campos son requeridos; el teléfono además exige regex + mínimo 8 dígitos), ejecutada tanto al perder el foco de cualquier campo como al intentar guardar.
- Eliminación pide confirmación vía `Shell.Current.DisplayAlertAsync`, con el botón "Eliminar" en rojo (`DangerButton`).
- Se removieron los targets iOS/MacCatalyst del `.csproj` porque este entorno no tiene un Mac para compilarlos; si se necesita iOS, hay que agregarlos de nuevo y compilar desde Visual Studio con un Mac emparejado (o quitar esa restricción si se trabaja desde una Mac).
