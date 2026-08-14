using VetSucursales.Models;

namespace VetSucursales.Helpers;

/// <summary>Genera sucursales de prueba con datos variados (dirección, horario y encargado) para
/// poblar Firestore rápidamente. Usa el mismo modelo Sucursal que el resto de la app, así que se
/// guarda con el formato real a través de IFirestoreService.AddSucursalAsync — no duplica la
/// lógica de serialización.</summary>
public static class TestDataGenerator
{
    private static readonly string[] Ciudades =
    {
        "San José", "Escazú", "Alajuela", "Heredia", "Cartago", "Liberia",
        "Puntarenas", "Limón", "Desamparados", "Curridabat", "Grecia", "San Carlos"
    };

    private static readonly string[] Calles =
    {
        "Avenida Central", "Calle 5", "Boulevard Rohrmoser", "Autopista General Cañas",
        "Calle 10", "Avenida Segunda", "Diagonal a la iglesia", "100 metros norte del parque"
    };

    private static readonly string[] NombresPropios =
    {
        "María", "José", "Ana", "Carlos", "Laura", "Luis", "Sofía", "Diego", "Andrea", "Kevin"
    };

    private static readonly string[] Apellidos =
    {
        "Rodríguez", "Vargas", "Jiménez", "Mora", "Solís", "Rojas", "Chacón", "Araya", "Castro", "Núñez"
    };

    private static readonly string[] Descripciones =
    {
        "Clínica veterinaria con atención general y emergencias.",
        "Consulta general, vacunación y cirugía menor.",
        "Atención de mascotas pequeñas y exóticas.",
        "Clínica veterinaria con farmacia y grooming.",
        "Servicio de consulta, laboratorio y hospitalización."
    };

    public static List<Sucursal> Generar(int cantidad)
    {
        var random = new Random();
        var sucursales = new List<Sucursal>(cantidad);

        for (int i = 1; i <= cantidad; i++)
            sucursales.Add(CrearUna(random, i));

        return sucursales;
    }

    private static Sucursal CrearUna(Random random, int indice)
    {
        var ciudad = Elegir(random, Ciudades);

        return new Sucursal
        {
            Nombre = $"Sucursal de prueba {indice} - {ciudad}",
            Direccion = new Direccion
            {
                Direccion1 = $"{Elegir(random, Calles)}, {random.Next(1, 300)} metros",
                Direccion2 = random.Next(0, 3) == 0 ? $"Local {random.Next(1, 20)}" : string.Empty,
                Ciudad = ciudad,
                Estado = Elegir(random, CostaRicaProvincias.Todas),
                CodigoPostal = random.Next(10000, 99999).ToString(),
            },
            Telefono = TelefonoAleatorio(random),
            Horario = HorarioAleatorio(random),
            Encargado = new Encargado
            {
                Nombre = Elegir(random, NombresPropios),
                Apellido = Elegir(random, Apellidos),
                Telefono = TelefonoAleatorio(random),
                Correo = $"encargado{indice}@vetsucursales.com",
            },
            Descripcion = Elegir(random, Descripciones),
        };
    }

    private static string TelefonoAleatorio(Random random) =>
        $"{random.Next(2000, 9000)}-{random.Next(1000, 9999)}";

    private static List<HorarioDia> HorarioAleatorio(Random random)
    {
        var horario = new List<HorarioDia>();

        foreach (var dia in Enum.GetValues<DiaSemana>())
        {
            var estado = (EstadoHorarioDia)random.Next(0, 3);
            var rangos = new List<RangoHorario>();

            if (estado == EstadoHorarioDia.HorarioPersonalizado)
            {
                rangos.Add(new RangoHorario { HoraInicio = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(12, 0, 0) });

                if (random.Next(0, 2) == 0)
                    rangos.Add(new RangoHorario { HoraInicio = new TimeSpan(13, 0, 0), HoraCierre = new TimeSpan(17, 0, 0) });
            }

            horario.Add(new HorarioDia { DiaSemana = dia, Estado = estado, Rangos = rangos });
        }

        return horario;
    }

    private static T Elegir<T>(Random random, IReadOnlyList<T> valores) => valores[random.Next(valores.Count)];
}
