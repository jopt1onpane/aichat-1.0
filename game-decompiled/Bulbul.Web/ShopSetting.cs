using System;
using System.Text.Json.Serialization;
using Bulbul.CustomJsonConverter;

namespace Bulbul.Web;

public readonly struct ShopSetting(string productId, DateTime startDate, DateTime endDate)
{
	public readonly string ProductId = productId;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime StartDate = startDate;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime EndDate = endDate;
}
