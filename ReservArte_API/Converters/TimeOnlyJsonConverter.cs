using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReservArte_API.Converters;

/// <summary>
/// Convierte TimeOnly en JSON aceptando formatos "HH:mm" y "HH:mm:ss".
/// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string SerializationFormat = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            throw new JsonException("El valor de hora no puede ser nulo o vacío.");
        return TimeOnly.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(SerializationFormat));
    }
}
