using System;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class StoryData
{
	[ES3Serializable]
	private bool _isUseAuto;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsUseAuto = new ReactiveProperty<bool>();

	public StoryData()
	{
		_isUseAuto = true;
		IsUseAuto.Value = true;
	}

	public void LoadSetup()
	{
		IsUseAuto.Value = _isUseAuto;
	}

	public void SaveReady()
	{
		_isUseAuto = IsUseAuto.Value;
	}
}
