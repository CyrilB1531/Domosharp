using Domosharp.Infrastructure.Entities;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domosharp.Infrastructure
{
  internal static class JsonExtensions
  {
    public static JsonSerializerOptions FullObjectOnDeserializing { get
      {
        var options = new JsonSerializerOptions()
        {
          UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };
        options.Converters.Add(new BooleanJsonConverter());
        options.Converters.Add(new ShutterOptionJsonConverter());
        return options;
      }
    }
  }

  internal class BooleanJsonConverter : JsonConverter<bool>
  {
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      => reader.GetInt32() != 0;

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteNumberValue(value ? 1 : 0);
  }

  internal class ShutterOptionJsonConverter : JsonConverter<ShutterOption>
  {
    public override ShutterOption Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      => new (reader.GetByte());

    public override void Write(Utf8JsonWriter writer, ShutterOption value, JsonSerializerOptions options) => writer.WriteNumberValue(value.GetValue());
  }
}
