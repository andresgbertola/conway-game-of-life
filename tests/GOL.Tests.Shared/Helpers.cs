using GOL.Domain.Entities;
using GOL.Domain.Serializers;
using System.Text.Json;

namespace GOL.Tests.Shared
{
    public static class Helpers
    {       
        public static List<CellCoordinates> GetStateFromJsonFile(string name)
        {
            // Build the file path relative to the output directory.
            string filePath = Path.Combine(AppContext.BaseDirectory, "GameOfLifeServiceTests.json");
            string jsonContent = File.ReadAllText(filePath);

            // Parse the JSON document.
            using var jsonDoc = JsonDocument.Parse(jsonContent);
            JsonElement root = jsonDoc.RootElement;

            // Get the CrossStateExpected property.
            JsonElement crossStateExpectedElement = root.GetProperty(name);

            return CellCoordinatesSerializer.Deserialize(crossStateExpectedElement.GetRawText());
        }
    }
}
