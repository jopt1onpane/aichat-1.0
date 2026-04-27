using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class CalendarDateData
{
	public double WorkTimeSeconds;

	public Dictionary<ulong, TodoData> CompleteTodoListDic;

	public string DiaryText;

	[ES3NonSerializable]
	public TimeSpan WorkTime => TimeSpan.FromSeconds(WorkTimeSeconds);

	public CalendarDateData()
	{
		WorkTimeSeconds = 0.0;
		CompleteTodoListDic = new Dictionary<ulong, TodoData>();
		DiaryText = string.Empty;
	}
}
