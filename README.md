# Laboratorio #2 — Módulo de Sucursales

Universidad Latina de Costa Rica · Programación Móvil

Módulo completo para la administración de sucursales de una clínica veterinaria, desarrollado en **.NET MAUI** con persistencia en **Firebase Cloud Firestore**.

Ver `Laboratorio_2_Modulo_Sucursales_Profesional.pdf` para el enunciado original y `CONTEXTO.md` para el estado/checklist del avance.

## Funcionalidades

- Registrar una sucursal
- Listar las sucursales registradas
- Consultar el detalle de una sucursal
- Modificar la información de una sucursal
- Eliminar una sucursal (con confirmación)
- Validación de campos obligatorios

Cada sucursal guarda: **Nombre, Dirección, Teléfono, Horario de atención, Encargado y Descripción**.

## Estructura del proyecto

```
VetSucursales/
├── Models/Sucursal.cs            Modelo de datos
├── Services/
│   ├── FirebaseConfig.cs         Project ID / configuración de Firebase (editar antes de ejecutar)
│   ├── IFirestoreService.cs
│   └── FirestoreService.cs       CRUD contra la API REST de Firestore
├── ViewModels/                   MVVM (CommunityToolkit.Mvvm)
├── Views/                        Páginas XAML (Listado, Formulario, Detalle)
├── Converters/                   Convertidores usados en los bindings XAML
├── AppShell.xaml(.cs)            Navegación (Shell)
└── MauiProgram.cs                Inyección de dependencias
```

## Configurar Firebase

1. Ir a [Firebase Console](https://console.firebase.google.com) y crear un proyecto nuevo (o usar uno existente).
2. En **Build → Firestore Database**, crear la base de datos en **modo de prueba** (test mode). Esto habilita reglas abiertas de lectura/escritura durante el desarrollo:
   ```
   rules_version = '2';
   service cloud.firestore {
     match /databases/{database}/documents {
       match /{document=**} {
         allow read, write: if true;
       }
     }
   }
   ```
   > Estas reglas son solo para el desarrollo del laboratorio. No usar en producción.
3. Copiar el **Project ID** del proyecto (Configuración del proyecto → General).
4. Editar `VetSucursales/Services/FirebaseConfig.cs` y reemplazar:
   ```csharp
   public const string ProjectId = "TU_FIREBASE_PROJECT_ID";
   ```
   por el Project ID real.
5. La app crea la colección `sucursales` automáticamente al registrar la primera sucursal — no es necesario crearla manualmente.

No se requiere descargar `google-services.json` ni instalar SDKs nativos de Firebase: la app habla directamente con la API REST de Firestore (`https://firestore.googleapis.com/v1/...`) mediante `HttpClient`, evitando problemas de compatibilidad de gRPC/AOT en Android e iOS.

## Requisitos para compilar

- Visual Studio 2022 (17.12+) con la carga de trabajo **.NET Multi-platform App UI development**, o el SDK de .NET 10 con los workloads `maui-android` y `maui-windows` instalados (`dotnet workload install maui`).
- Un emulador/dispositivo Android, o Windows 10/11 para ejecutar el target `net10.0-windows`.

## Ejecutar

```bash
cd VetSucursales
dotnet build -f net10.0-android            # o net10.0-windows10.0.19041.0
dotnet build -t:Run -f net10.0-windows10.0.19041.0   # ejecutar en Windows
```

También se puede abrir `VetSucursales.csproj` en Visual Studio y ejecutar con F5 seleccionando el destino deseado (Windows Machine o un emulador Android).

## Entregables del laboratorio

- [x] Proyecto completo (este repositorio)
- [x] Código fuente funcional
- [ ] Base de datos en Firestore (crear el proyecto real siguiendo los pasos de arriba)
- [ ] Video de demostración (5–8 minutos)
