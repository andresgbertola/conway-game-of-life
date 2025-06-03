using GOL.Domain.Entities;
using System.Text.Json;
namespace GOL.Domain.Serializers
{
    public static class CellCoordinatesSerializer
    {
        private static JsonSerializerOptions _cellCoordinatesConverterOptions = new JsonSerializerOptions
        {
            Converters = { new CellCoordinatesConverter() },
            WriteIndented = false // no extra whitespace
        };
        public static string Serialize(this IEnumerable<CellCoordinates> cellCoordinates)
        {
            return JsonSerializer.Serialize(cellCoordinates, _cellCoordinatesConverterOptions);
        }

        public static List<CellCoordinates>? Deserialize(string json)
        {
            if (json == null) return null;

            return JsonSerializer.Deserialize<List<CellCoordinates>>(json, _cellCoordinatesConverterOptions);
        }

    }
}
