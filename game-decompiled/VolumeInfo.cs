using System;
using GUPS.Obfuscator.Attribute;
using NestopiSystem;
using R3;
using UnityEngine;

[Serializable]
[DoNotObfuscateClass]
public class VolumeInfo
{
	[ES3NonSerializable]
	public ReactiveProperty<bool> IsMute = new ReactiveProperty<bool>();

	[ES3NonSerializable]
	public ReactiveProperty<float> RawValue { get; private set; } = new ReactiveProperty<float>();

	public float Value
	{
		get
		{
			if (!IsMute.Value)
			{
				return RawValue.Value;
			}
			return 0f;
		}
		set
		{
			RawValue.Value = Mathf.Clamp01(value);
		}
	}

	public float Decibel => AudioUtil.VolumeToDecibel(Value);

	public VolumeInfo()
	{
		IsMute = new ReactiveProperty<bool>(value: false);
		RawValue.Value = 0.5f;
	}

	public VolumeInfo(bool isMute, float rawValue)
	{
		IsMute = new ReactiveProperty<bool>(isMute);
		RawValue.Value = rawValue;
	}
}
