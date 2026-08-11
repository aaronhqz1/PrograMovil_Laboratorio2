using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using VetSucursales.Models;

namespace VetSucursales.Services;

/// <summary>
/// Acceso a Cloud Firestore mediante su API REST (https://firestore.googleapis.com/v1/...).
/// Se usa REST en lugar del SDK de Google.Cloud.Firestore para evitar dependencias de gRPC
/// que no son compatibles con el "trimming"/AOT de Android e iOS en .NET MAUI.
/// </summary>
public class FirestoreService : IFirestoreService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public FirestoreService(HttpClient http)
    {
        _http = http;
        _baseUrl = $"https://firestore.googleapis.com/v1/projects/{FirebaseConfig.ProjectId}" +
                   $"/databases/{FirebaseConfig.DatabaseId}/documents/{FirebaseConfig.Collection}";
    }

    public async Task<List<Sucursal>> GetSucursalesAsync()
    {
        EnsureConfigured();

        using var response = await _http.GetAsync(WithKey(_baseUrl));
        await EnsureSuccessAsync(response);

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        var documents = root?["documents"]?.AsArray();

        if (documents is null)
            return new List<Sucursal>();

        return documents
            .Where(d => d is not null)
            .Select(d => ParseDocument(d!))
            .OrderBy(s => s.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<Sucursal?> GetSucursalAsync(string id)
    {
        EnsureConfigured();

        using var response = await _http.GetAsync(WithKey($"{_baseUrl}/{id}"));

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonNode.Parse(json)!;
        return ParseDocument(doc);
    }

    public async Task<Sucursal> AddSucursalAsync(Sucursal sucursal)
    {
        EnsureConfigured();

        var body = new JsonObject { ["fields"] = BuildFields(sucursal) };
        using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync(WithKey(_baseUrl), content);
        await EnsureSuccessAsync(response);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonNode.Parse(json)!;
        return ParseDocument(doc);
    }

    public async Task UpdateSucursalAsync(Sucursal sucursal)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(sucursal.Id))
            throw new InvalidOperationException("No se puede modificar una sucursal sin Id.");

        var body = new JsonObject { ["fields"] = BuildFields(sucursal) };
        using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        using var response = await _http.PatchAsync(WithKey($"{_baseUrl}/{sucursal.Id}"), content);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteSucursalAsync(string id)
    {
        EnsureConfigured();

        using var response = await _http.DeleteAsync(WithKey($"{_baseUrl}/{id}"));
        await EnsureSuccessAsync(response);
    }

    private static void EnsureConfigured()
    {
        if (!FirebaseConfig.IsConfigured)
        {
            throw new InvalidOperationException(
                "Firebase no está configurado. Edite Services/FirebaseConfig.cs y coloque el " +
                "Project ID de su proyecto de Firebase.");
        }
    }

    private static string WithKey(string url) =>
        string.IsNullOrWhiteSpace(FirebaseConfig.ApiKey) ? url : $"{url}?key={FirebaseConfig.ApiKey}";

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        string message = body;

        try
        {
            var errorNode = JsonNode.Parse(body);
            var extracted = errorNode?["error"]?["message"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(extracted))
                message = extracted;
        }
        catch
        {
            // El cuerpo no era JSON; se usa el texto original.
        }

        throw new InvalidOperationException($"Error de Firestore ({(int)response.StatusCode}): {message}");
    }

    private static JsonObject BuildFields(Sucursal sucursal) => new()
    {
        ["nombre"] = new JsonObject { ["stringValue"] = sucursal.Nombre?.Trim() ?? string.Empty },
        ["direccion"] = new JsonObject { ["stringValue"] = sucursal.Direccion?.Trim() ?? string.Empty },
        ["telefono"] = new JsonObject { ["stringValue"] = sucursal.Telefono?.Trim() ?? string.Empty },
        ["horarioAtencion"] = new JsonObject { ["stringValue"] = sucursal.HorarioAtencion?.Trim() ?? string.Empty },
        ["encargado"] = new JsonObject { ["stringValue"] = sucursal.Encargado?.Trim() ?? string.Empty },
        ["descripcion"] = new JsonObject { ["stringValue"] = sucursal.Descripcion?.Trim() ?? string.Empty },
    };

    private static Sucursal ParseDocument(JsonNode doc)
    {
        var name = doc["name"]?.GetValue<string>() ?? string.Empty;
        var id = name.Contains('/') ? name[(name.LastIndexOf('/') + 1)..] : name;
        var fields = doc["fields"];

        string Get(string key) => fields?[key]?["stringValue"]?.GetValue<string>() ?? string.Empty;

        return new Sucursal
        {
            Id = id,
            Nombre = Get("nombre"),
            Direccion = Get("direccion"),
            Telefono = Get("telefono"),
            HorarioAtencion = Get("horarioAtencion"),
            Encargado = Get("encargado"),
            Descripcion = Get("descripcion"),
        };
    }
}
