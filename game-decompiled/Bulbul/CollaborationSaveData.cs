using System;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class CollaborationSaveData
{
	[ES3NonSerializable]
	private ReactiveProperty<SpecialService.CollaborationType> _currentType = new ReactiveProperty<SpecialService.CollaborationType>();

	[ES3Serializable]
	private SpecialService.CollaborationType _currentTypeForSave;

	public AlterEgoSaveData AlterEgoData;

	public BearsRestaurantSaveData BearsRestaurantData;

	public Valentine2026SaveData Valentine2026Data;

	public LunaNewYear2026SaveData LunaNewYear2026Data;

	public NearSpring2026SaveData NearSpring2026Data;

	[ES3NonSerializable]
	public ReactiveProperty<SpecialService.CollaborationType> CurrentType => _currentType;

	public CollaborationSaveData()
	{
		_currentType.Value = SpecialService.CollaborationType.None;
		AlterEgoData = new AlterEgoSaveData();
		BearsRestaurantData = new BearsRestaurantSaveData();
		Valentine2026Data = new Valentine2026SaveData();
		LunaNewYear2026Data = new LunaNewYear2026SaveData();
		NearSpring2026Data = new NearSpring2026SaveData();
	}

	public void SaveReady()
	{
		_currentTypeForSave = _currentType.Value;
	}

	public void LoadSetup()
	{
		_currentType.Value = _currentTypeForSave;
	}
}
