using System;
using System.Text.Json.Serialization;
using Bulbul.CustomJsonConverter;

namespace Bulbul.Web;

public readonly struct ShopMaintenance(bool isMaintenance, DateTime startDate, DateTime endDate, string mainText)
{
	public readonly bool IsMaintenance = isMaintenance;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime StartDate = startDate;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime EndDate = endDate;

	public readonly string MainText = mainText;
}
