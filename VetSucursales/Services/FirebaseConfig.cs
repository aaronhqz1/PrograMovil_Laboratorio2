namespace VetSucursales.Services;

/// <summary>
/// Datos de conexión al proyecto de Firebase. Reemplace <see cref="ProjectId"/> con el
/// Project ID real de su proyecto de Firebase (ver README.md, sección "Configurar Firebase").
/// </summary>
public static class FirebaseConfig
{
    public const string ProjectId = "programovillaboratorio2";

    /// <summary>
    /// API Key web del proyecto (opcional). Firestore en modo de prueba no la exige,
    /// pero si sus reglas de seguridad la requieren, colóquela aquí.
    /// </summary>
    public const string ApiKey = "";

    public const string DatabaseId = "(default)";

    public const string Collection = "sucursales";

    public static bool IsConfigured => ProjectId != "TU_FIREBASE_PROJECT_ID" && !string.IsNullOrWhiteSpace(ProjectId);
}
