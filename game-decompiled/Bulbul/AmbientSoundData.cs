using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class AmbientSoundData
{
	public AmbientSoundType AmbientSoundType;

	public float SoundVolume;

	public bool IsMuteAmbient;

	public AmbientSoundData()
	{
		AmbientSoundType = AmbientSoundType.RadioNoise;
		SoundVolume = 0f;
		IsMuteAmbient = true;
	}

	public AmbientSoundData(AmbientSoundType ambientSoundType)
	{
		AmbientSoundType = ambientSoundType;
		SoundVolume = 0f;
		IsMuteAmbient = true;
	}

	public void CopyFrom(AmbientSoundData other)
	{
		AmbientSoundType = other.AmbientSoundType;
		SoundVolume = other.SoundVolume;
		IsMuteAmbient = other.IsMuteAmbient;
	}

	public bool IsSame(AmbientSoundData other)
	{
		if (AmbientSoundType == other.AmbientSoundType && SoundVolume == other.SoundVolume)
		{
			return IsMuteAmbient == other.IsMuteAmbient;
		}
		return false;
	}
}
