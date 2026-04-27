using System;
using GUPS.Obfuscator.Attribute;
using R3;

[Serializable]
[DoNotObfuscateClass]
public class DecorationData
{
	public DecorationService.DecorationSkinType DecorationType;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();

	[ES3Serializable]
	public bool _isActiveForSave;

	public DecorationData(DecorationService.DecorationSkinType decorationType)
	{
		DecorationType = decorationType;
		IsActive.Value = false;
		_isActiveForSave = false;
	}

	public void LoadSetup()
	{
		IsActive = new ReactiveProperty<bool>();
		IsActive.Value = _isActiveForSave;
	}

	public void SaveReady()
	{
		_isActiveForSave = IsActive.Value;
	}

	public void CopyFrom(DecorationData other)
	{
		DecorationType = other.DecorationType;
		IsActive.Value = other.IsActive.Value;
		_isActiveForSave = other._isActiveForSave;
	}

	public bool IsSame(DecorationData other)
	{
		if (DecorationType == other.DecorationType)
		{
			return IsActive.Value == other.IsActive.Value;
		}
		return false;
	}
}
