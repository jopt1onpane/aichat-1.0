using System;
using System.Globalization;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class PomodoroData
{
	[ES3NonSerializable]
	public ReactiveProperty<int> LoopCount;

	[ES3Serializable]
	private int _loopCountForSave;

	[ES3NonSerializable]
	public ReactiveProperty<int> WorkMinutes;

	[ES3Serializable]
	private int _workMinutesForSave;

	[ES3NonSerializable]
	public ReactiveProperty<int> BreakMinutes;

	[ES3Serializable]
	private int _breakMinutesForSave;

	[ES3Serializable]
	private string _lastWorkStartDateTimeString;

	[ES3NonSerializable]
	public DateTime LastWorkStartDateTimeString
	{
		get
		{
			if (string.IsNullOrEmpty(_lastWorkStartDateTimeString))
			{
				return DateTime.Now;
			}
			if (!DateTime.TryParseExact(_lastWorkStartDateTimeString, "yyyyMMddHHmmss", null, DateTimeStyles.None, out var result))
			{
				return DateTime.Now;
			}
			return result;
		}
	}

	public PomodoroData()
	{
		LoopCount = new ReactiveProperty<int>(2);
		_loopCountForSave = LoopCount.Value;
		WorkMinutes = new ReactiveProperty<int>(25);
		_workMinutesForSave = WorkMinutes.Value;
		BreakMinutes = new ReactiveProperty<int>(5);
		_breakMinutesForSave = BreakMinutes.Value;
	}

	public void LoadSetup()
	{
		LoopCount.Value = _loopCountForSave;
		WorkMinutes.Value = _workMinutesForSave;
		BreakMinutes.Value = _breakMinutesForSave;
	}

	public void SaveReady()
	{
		_loopCountForSave = LoopCount.Value;
		_workMinutesForSave = WorkMinutes.Value;
		_breakMinutesForSave = BreakMinutes.Value;
	}

	public void OnStartPomodoro()
	{
		_lastWorkStartDateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
	}
}
