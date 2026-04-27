using System;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class LimitedTimeEventSaveData
{
	[ES3NonSerializable]
	private ReactiveProperty<LimitedTimeEventType> _currentType = new ReactiveProperty<LimitedTimeEventType>();

	[ES3Serializable]
	private LimitedTimeEventType _currentTypeForSave;

	public EventChristmas2025SaveData Christmas2025SaveData;

	[ES3NonSerializable]
	public ReactiveProperty<LimitedTimeEventType> CurrentType => _currentType;

	public LimitedTimeEventSaveData()
	{
		_currentType.Value = LimitedTimeEventType.None;
		Christmas2025SaveData = new EventChristmas2025SaveData();
	}

	public void LoadSetup()
	{
		_currentType.Value = _currentTypeForSave;
	}

	public void SaveReady()
	{
		_currentTypeForSave = _currentType.Value;
	}

	public void ChangeType(LimitedTimeEventType eventType)
	{
		_currentType.Value = eventType;
	}
}
