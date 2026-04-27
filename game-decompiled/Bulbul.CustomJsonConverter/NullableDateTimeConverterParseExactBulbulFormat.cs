using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bulbul.CustomJsonConverter;

public class NullableDateTimeConverterParseExactBulbulFormat : JsonConverter<DateTime?>
{
	private const string format = "yyyy-MM-dd HH:mm:ss";

	public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string text = reader.GetString();
		if (string.IsNullOrEmpty(text) || text == "null")
		{
			return null;
		}
		return DateTime.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
	}

	public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}
