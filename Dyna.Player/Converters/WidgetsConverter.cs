namespace Dyna.Player.Converters
{
    using Dyna.Player.Models;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class WidgetsConverter : JsonConverter<List<object>>
    {
        public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var widgets = new List<object>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                        {
                            var root = document.RootElement;
                            switch (root)
                            {
                                case JsonElement element when element.TryGetProperty("image", out JsonElement imageElement):
                                    //widgets.Add(JsonSerializer.Deserialize<WidgetElement>(imageElement.GetRawText(), options));
                                    break;
                                case JsonElement element when element.TryGetProperty("video", out JsonElement videoElement):
                                    //widgets.Add(JsonSerializer.Deserialize<WidgetElement>(videoElement.GetRawText(), options));
                                    break;
                                case JsonElement element when element.TryGetProperty("countdown", out JsonElement countdownElement):
                                    //widgets.Add(JsonSerializer.Deserialize<WidgetElement>(countdownElement.GetRawText(), options));
                                    break;
                                default:
                                    System.Diagnostics.Debug.WriteLine("Warning: Widget object missing expected property (image, video, countdown).");
                                    break;
                            }
                        }
                    }
                }
                return widgets;
            }
            else
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}