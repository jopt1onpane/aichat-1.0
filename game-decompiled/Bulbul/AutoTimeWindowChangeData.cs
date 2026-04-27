using System;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class AutoTimeWindowChangeData
{
	[ES3Serializable]
	public float TimeDayStart;

	[ES3Serializable]
	public float TimeSunsetStart;

	[ES3Serializable]
	public float TimeNightStart;

	[ES3Serializable]
	public SerializableReactiveProperty<bool> IsActiveAuto { get; private set; }

	public AutoTimeWindowChangeData()
	{
		IsActiveAuto = new SerializableReactiveProperty<bool>(value: false);
		InitTimeData();
	}

	public void InitTimeData()
	{
		TimeDayStart = 5f;
		TimeSunsetStart = 17f;
		TimeNightStart = 20f;
	}
}
