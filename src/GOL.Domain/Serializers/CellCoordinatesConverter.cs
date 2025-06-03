using GOL.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace GOL.Domain.Serializers
{
    public class CellCoordinatesConverter : JsonConverter<CellCoordinates>
    {
        public override CellCoordinates Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            reader.Read();
            int row = reader.GetInt32();

            reader.Read();
            int col = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();

            return new CellCoordinates(row, col);
        }

        public override void Write(Utf8JsonWriter writer, CellCoordinates value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Row);
            writer.WriteNumberValue(value.Col);
            writer.WriteEndArray();
        }
    }
}
