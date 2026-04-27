using System;
using System.Net;
using System.Text.Json.Serialization;
using Bulbul.CustomJsonConverter;

namespace Bulbul.Web;

public readonly struct NewsData(ulong id, DateTime startDate, DateTime endDate, string title, string mainText) : IEquatable<NewsData>, IComparable<NewsData>
{
	public readonly ulong ID = id;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime StartDate = startDate;

	[JsonConverter(typeof(DateTimeConverterParseExactBulbulFormat))]
	public readonly DateTime EndDate = endDate;

	public readonly string Title = WebUtility.HtmlDecode(title);

	public readonly string MainText = WebUtility.HtmlDecode(mainText);

	public bool Equals(NewsData other)
	{
		if (ID == other.ID && StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate) && Title == other.Title)
		{
			return MainText == other.MainText;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is NewsData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ID, StartDate, EndDate, Title, MainText);
	}

	public int CompareTo(NewsData other)
	{
		int num = ID.CompareTo(other.ID);
		if (num != 0)
		{
			return num;
		}
		int num2 = StartDate.CompareTo(other.StartDate);
		if (num2 != 0)
		{
			return num2;
		}
		return EndDate.CompareTo(other.EndDate);
	}
}
