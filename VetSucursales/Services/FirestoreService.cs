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

    private static JsonObject StringValue(string? value) => new() { ["stringValue"] = value?.Trim() ?? string.Empty };

    private static JsonObject MapValue(JsonObject fields) => new() { ["mapValue"] = new JsonObject { ["fields"] = fields } };

    private static JsonObject ArrayValue(IEnumerable<JsonNode> values) =>
        new() { ["arrayValue"] = new JsonObject { ["values"] = new JsonArray(values.ToArray()) } };

    private static JsonObject BuildDireccionFields(Direccion direccion) => new()
    {
        ["direccion1"] = StringValue(direccion.Direccion1),
        ["direccion2"] = StringValue(direccion.Direccion2),
        ["ciudad"] = StringValue(direccion.Ciudad),
        ["estado"] = StringValue(direccion.Estado),
        ["codigoPostal"] = StringValue(direccion.CodigoPostal),
    };

    private static JsonObject BuildEncargadoFields(Encargado encargado) => new()
    {
        ["nombre"] = StringValue(encargado.Nombre),
        ["apellido"] = StringValue(encargado.Apellido),
        ["telefono"] = StringValue(encargado.Telefono),
        ["correo"] = StringValue(encargado.Correo),
    };

    private static JsonObject BuildRangoValue(RangoHorario rango) => MapValue(new JsonObject
    {
        ["horaInicio"] = StringValue(rango.HoraInicio.ToString(@"hh\:mm")),
        ["horaCierre"] = StringValue(rango.HoraCierre.ToString(@"hh\:mm")),
    });

    private static JsonObject BuildHorarioDiaValue(HorarioDia dia) => MapValue(new JsonObject
    {
        ["diaSemana"] = StringValue(dia.DiaSemana.ToString()),
        ["estado"] = StringValue(dia.Estado.ToString()),
        ["rangos"] = ArrayValue(dia.Rangos.Select(r => (JsonNode)BuildRangoValue(r))),
    });

    private static JsonObject BuildFields(Sucursal sucursal) => new()
    {
        ["nombre"] = StringValue(sucursal.Nombre),
        ["direccion"] = MapValue(BuildDireccionFields(sucursal.Direccion)),
        ["telefono"] = StringValue(sucursal.Telefono),
        ["horario"] = ArrayValue(sucursal.Horario.Select(h => (JsonNode)BuildHorarioDiaValue(h))),
        ["encargado"] = MapValue(BuildEncargadoFields(sucursal.Encargado)),
        ["descripcion"] = StringValue(sucursal.Descripcion),
    };

    /// <summary>Lee la dirección; si el documento es de antes de la reestructuración (donde "direccion"
    /// era un stringValue plano), ese texto se recupera en Direccion1 en vez de perderse.</summary>
    private static Direccion ParseDireccion(JsonNode? fields)
    {
        var node = fields?["direccion"];
        var mapFields = node?["mapValue"]?["fields"];
        if (mapFields is not null)
        {
            string Get(string key) => mapFields[key]?["stringValue"]?.GetValue<string>() ?? string.Empty;
            return new Direccion
            {
                Direccion1 = Get("direccion1"),
                Direccion2 = Get("direccion2"),
                Ciudad = Get("ciudad"),
                Estado = Get("estado"),
                CodigoPostal = Get("codigoPostal"),
            };
        }

        var legacyTexto = node?["stringValue"]?.GetValue<string>();
        return new Direccion { Direccion1 = legacyTexto ?? string.Empty };
    }

    /// <summary>Lee el encargado; retrocompatible con el "encargado" de texto plano de antes de la
    /// reestructuración (se recupera como Nombre).</summary>
    private static Encargado ParseEncargado(JsonNode? fields)
    {
        var node = fields?["encargado"];
        var mapFields = node?["mapValue"]?["fields"];
        if (mapFields is not null)
        {
            string Get(string key) => mapFields[key]?["stringValue"]?.GetValue<string>() ?? string.Empty;
            return new Encargado
            {
                Nombre = Get("nombre"),
                Apellido = Get("apellido"),
                Telefono = Get("telefono"),
                Correo = Get("correo"),
            };
        }

        var legacyTexto = node?["stringValue"]?.GetValue<string>();
        return new Encargado { Nombre = legacyTexto ?? string.Empty };
    }

    /// <summary>Lee el horario estructurado por día. Los documentos de antes de la reestructuración
    /// guardaban "horarioAtencion" como texto libre de toda la semana; ese texto no se puede
    /// descomponer automáticamente en días/rangos, así que se deja la semana en blanco (Cerrado) y
    /// debe reingresarse manualmente (ver CONTEXTO.md).</summary>
    private static List<HorarioDia> ParseHorario(JsonNode? fields)
    {
        var valores = fields?["horario"]?["arrayValue"]?["values"]?.AsArray();
        if (valores is null || valores.Count == 0)
            return Sucursal.CrearHorarioSemanaVacio();

        var dias = new List<HorarioDia>();
        foreach (var valor in valores)
        {
            var mapFields = valor?["mapValue"]?["fields"];
            var diaTexto = mapFields?["diaSemana"]?["stringValue"]?.GetValue<string>();
            if (mapFields is null || !Enum.TryParse<DiaSemana>(diaTexto, out var dia))
                continue;

            Enum.TryParse<EstadoHorarioDia>(mapFields["estado"]?["stringValue"]?.GetValue<string>(), out var estado);

            var rangos = new List<RangoHorario>();
            var rangosArray = mapFields["rangos"]?["arrayValue"]?["values"]?.AsArray();
            if (rangosArray is not null)
            {
                foreach (var rangoValor in rangosArray)
                {
                    var rangoFields = rangoValor?["mapValue"]?["fields"];
                    var inicioTexto = rangoFields?["horaInicio"]?["stringValue"]?.GetValue<string>();
                    var cierreTexto = rangoFields?["horaCierre"]?["stringValue"]?.GetValue<string>();
                    if (TimeSpan.TryParse(inicioTexto, out var inicio) && TimeSpan.TryParse(cierreTexto, out var cierre))
                        rangos.Add(new RangoHorario { HoraInicio = inicio, HoraCierre = cierre });
                }
            }

            dias.Add(new HorarioDia { DiaSemana = dia, Estado = estado, Rangos = rangos });
        }

        foreach (var dia in Enum.GetValues<DiaSemana>())
        {
            if (!dias.Any(h => h.DiaSemana == dia))
                dias.Add(new HorarioDia { DiaSemana = dia, Estado = EstadoHorarioDia.Cerrado });
        }

        return dias.OrderBy(h => (int)h.DiaSemana).ToList();
    }

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
            Direccion = ParseDireccion(fields),
            Telefono = Get("telefono"),
            Horario = ParseHorario(fields),
            Encargado = ParseEncargado(fields),
            Descripcion = Get("descripcion"),
        };
    }
}
