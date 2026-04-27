using System;
using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "AllVolumeMaster", menuName = "ScriptableObject/AllVolumeMaster")]
public class AllVolumeMaster : ScriptableObject
{
	[Header("----------PC----------")]
	[SerializeField]
	[Header("Music")]
	[Range(0f, 1f)]
	private float MusicVolume = 1f;

	[SerializeField]
	[Header("システムSE")]
	[Range(0f, 1f)]
	private float SystemSeVolume = 1f;

	[SerializeField]
	[Header("ボイス")]
	[Range(0f, 1f)]
	private float VoiceVolume = 1f;

	[SerializeField]
	[Header("環境音BGM")]
	[Range(0f, 1f)]
	private float AmbientBgmVolume = 1f;

	[SerializeField]
	[Header("生活音")]
	[Range(0f, 1f)]
	private float AmbientSeVolume = 1f;

	[Header("----------Mobile----------")]
	[SerializeField]
	[Header("Music")]
	[Range(0f, 1f)]
	private float MusicVolume_Mobile = 1f;

	[SerializeField]
	[Header("システムSE")]
	[Range(0f, 1f)]
	private float SystemSeVolume_Mobile = 1f;

	[SerializeField]
	[Header("ボイス")]
	[Range(0f, 1f)]
	private float VoiceVolume_Mobile = 1f;

	[SerializeField]
	[Header("環境音BGM")]
	[Range(0f, 1f)]
	private float AmbientBgmVolume_Mobile = 1f;

	[SerializeField]
	[Header("生活音")]
	[Range(0f, 1f)]
	private float AmbientSeVolume_Mobile = 1f;

	public float GetVolume(AudioMixerType audioMixerType)
	{
		return audioMixerType switch
		{
			AudioMixerType.Master => 1f, 
			AudioMixerType.Music => MusicVolume, 
			AudioMixerType.SystemSE => SystemSeVolume, 
			AudioMixerType.Voice => VoiceVolume, 
			AudioMixerType.AmbientBGM => AmbientBgmVolume, 
			AudioMixerType.AmbientSE => AmbientSeVolume, 
			_ => throw new ArgumentOutOfRangeException("audioMixerType", audioMixerType, null), 
		};
	}
}
