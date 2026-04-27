using System;
using System.Globalization;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class DatePeriodData
{
	[ES3Serializable]
	private string _startDate;

	[ES3Serializable]
	private string _endDate;

	private const string DateFormat = "yyyyMMdd";

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public DatePeriodData()
	{
	}

	public DatePeriodData(DateTime start, DateTime? end)
	{
		StartDate = start;
		EndDate = end;
	}

	public void LoadSetup()
	{
		StartDate = DateTime.ParseExact(_startDate, "yyyyMMdd", CultureInfo.InvariantCulture);
		EndDate = ((_endDate != null) ? new DateTime?(DateTime.ParseExact(_endDate, "yyyyMMdd", CultureInfo.InvariantCulture)) : ((DateTime?)null));
	}

	public void SaveReady()
	{
		_startDate = StartDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		_endDate = EndDate?.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
	}

	public bool IsDateInPeriod(DateTime date)
	{
		if (date.Date < StartDate.Date)
		{
			return false;
		}
		if (EndDate.HasValue)
		{
			return date.Date <= EndDate.Value.Date;
		}
		return true;
	}
}
