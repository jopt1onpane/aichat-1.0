using UnityEngine;

namespace NestopiSystem;

public static class AudioUtil
{
	public static float VolumeToDecibel(float volume)
	{
		return Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp(volume, 0f, 1f)), -80f, 0f);
	}

	public static float DecibelToVolume(float decibel)
	{
		return Mathf.Clamp(Mathf.Pow(10f, Mathf.Clamp(decibel, -80f, 0f) / 20f), 0f, 1f);
	}
}
