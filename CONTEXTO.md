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
- [ ] Base de datos en Firestore — **pendiente: crear el proyecto real en Firebase Console y pegar el Project ID en `Services/FirebaseConfig.cs`** (ver README.md)
- [ ] Video de demostración (5–8 minutos) — pendiente de grabar por el estudiante

## Estado de la implementación
Proyecto `VetSucursales/` (.NET MAUI, targets: `net10.0-android`, `net10.0-windows10.0.19041.0`; se quitó iOS/MacCatalyst porque este entorno de desarrollo es Windows sin Mac de compilación).

- `Models/Sucursal.cs` — modelo de datos.
- `Services/FirebaseConfig.cs` — **placeholder de configuración, hay que editarlo con el Project ID real**.
- `Services/FirestoreService.cs` — CRUD contra la API REST de Firestore (`https://firestore.googleapis.com/v1/...`). Se usó REST en vez del SDK `Google.Cloud.Firestore` para evitar problemas de gRPC/trimming en Android/iOS.
- `ViewModels/` — `SucursalListViewModel`, `SucursalFormViewModel` (sirve para registrar y editar), `SucursalDetailViewModel`. MVVM con `CommunityToolkit.Mvvm`.
- `Views/` — `SucursalListPage`, `SucursalFormPage`, `SucursalDetailPage`.
- Navegación por rutas de Shell (`AppShell.xaml`/`.xaml.cs`), DI en `MauiProgram.cs`.
- Build verificado: `dotnet build -f net10.0-windows10.0.19041.0` y `dotnet build -f net10.0-android` — ambos compilan sin errores (10-ago-2026).

## Pendiente / próximos pasos
1. El usuario debe crear un proyecto real en https://console.firebase.google.com, habilitar Firestore (modo de prueba) y copiar el **Project ID** a `VetSucursales/Services/FirebaseConfig.cs`.
2. Probar la app en un emulador/dispositivo Android o en Windows (`dotnet build -t:Run -f net10.0-windows10.0.19041.0`) contra el Firestore real.
3. Grabar el video de demostración (5–8 min) mostrando las 5 operaciones CRUD + validaciones.
4. Antes de entregar: revisar que Firestore no quede en modo de prueba abierto de forma indefinida (las reglas "allow read, write: if true" expiran solas a los 30 días; para producción real se recomendarían reglas más estrictas, pero para este laboratorio académico es aceptable).

## Notas de diseño / decisiones tomadas
- Se usó la API REST de Firestore con `HttpClient` (no el SDK gRPC de Google.Cloud.Firestore) por compatibilidad con Android/iOS en MAUI.
- Validación de campos obligatorios implementada a mano en `SucursalFormViewModel.Validate()` (todos los 6 campos son requeridos; el teléfono además exige mínimo 8 dígitos).
- Eliminación pide confirmación vía `Shell.Current.DisplayAlertAsync`.
- Se removieron los targets iOS/MacCatalyst del `.csproj` porque este entorno no tiene un Mac para compilarlos; si se necesita iOS, hay que agregarlos de nuevo y compilar desde Visual Studio con un Mac emparejado (o quitar esa restricción si se trabaja desde una Mac).
