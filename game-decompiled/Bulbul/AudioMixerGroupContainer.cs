using System;
using DG.Tweening;
using NestopiSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Bulbul;

public class AudioMixerGroupContainer
{
	private readonly MasterDataLoader _masterDataLoader;

	public readonly AudioMixer AudioMixer;

	public readonly AudioMixerGroup MasterGroup;

	public readonly AudioMixerGroup SEGroup;

	public readonly AudioMixerGroup MusicGroup;

	public readonly AudioMixerGroup VoiceGroup;

	public readonly AudioMixerGroup AmbientBGMGroup;

	public readonly AudioMixerGroup AmbientSEGroup;

	private Tween _ambientTween;

	private Tween _ambientSeTween;

	private float _scenarioFadeVolume = 1f;

	public AudioMixerGroupContainer(MasterDataLoader masterDataLoader, AudioMixer audioMixer, AudioMixerGroups groups)
	{
		_masterDataLoader = masterDataLoader;
		AudioMixer = audioMixer;
		MasterGroup = groups.MasterGroup;
		SEGroup = groups.SEGroup;
		MusicGroup = groups.MusicGroup;
		VoiceGroup = groups.VoiceGroup;
		AmbientBGMGroup = groups.AmbientBGMGroup;
		AmbientSEGroup = groups.AmbientSEGroup;
	}

	public void ManualUpdateVolume(AudioMixerType audioMixerType)
	{
		float decibel = GetDecibel(audioMixerType);
		switch (audioMixerType)
		{
		case AudioMixerType.Master:
			MasterGroup.audioMixer.SetFloat("MasterVolume", decibel);
			break;
		case AudioMixerType.Music:
			MusicGroup.audioMixer.SetFloat("MusicVolume", decibel);
			break;
		case AudioMixerType.SystemSE:
			SEGroup.audioMixer.SetFloat("SystemSEVolume", decibel);
			break;
		case AudioMixerType.Voice:
			VoiceGroup.audioMixer.SetFloat("VoiceVolume", decibel);
			break;
		case AudioMixerType.AmbientBGM:
			AmbientBGMGroup.audioMixer.SetFloat("AmbientBGMVolume", decibel);
			break;
		case AudioMixerType.AmbientSE:
			AmbientSEGroup.audioMixer.SetFloat("AmbientSEVolume", decibel);
			break;
		}
	}

	public void ManualUpdateAllVolume()
	{
		ManualUpdateVolume(AudioMixerType.Master);
		ManualUpdateVolume(AudioMixerType.Music);
		ManualUpdateVolume(AudioMixerType.SystemSE);
		ManualUpdateVolume(AudioMixerType.Voice);
		ManualUpdateVolume(AudioMixerType.AmbientBGM);
		ManualUpdateVolume(AudioMixerType.AmbientSE);
	}

	public void FadeVolumeForScenario(float to, float duration, Action onEndCallback = null)
	{
		to = Mathf.Clamp01(to);
		_ambientSeTween?.Kill();
		_ambientSeTween = DOTween.To(() => _scenarioFadeVolume, delegate(float x)
		{
			_scenarioFadeVolume = x;
			ManualUpdateAllVolume();
		}, to, duration).OnComplete(delegate
		{
			onEndCallback?.Invoke();
		});
	}

	public float GetDecibel(AudioMixerType audioMixerType)
	{
		float volume = _masterDataLoader.AllVolumeData.GetVolume(audioMixerType);
		float num = 0f;
		switch (audioMixerType)
		{
		case AudioMixerType.Master:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.MasterVolumeInfo.Value * volume);
			MasterGroup.audioMixer.SetFloat("MasterVolume", num);
			break;
		case AudioMixerType.Music:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value * volume * _scenarioFadeVolume);
			MusicGroup.audioMixer.SetFloat("MusicVolume", num);
			break;
		case AudioMixerType.SystemSE:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.Value * volume);
			SEGroup.audioMixer.SetFloat("SystemSEVolume", num);
			break;
		case AudioMixerType.Voice:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.VoiceVolumeInfo.Value * volume * _scenarioFadeVolume);
			VoiceGroup.audioMixer.SetFloat("VoiceVolume", num);
			break;
		case AudioMixerType.AmbientBGM:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.Value * volume * _scenarioFadeVolume);
			AmbientBGMGroup.audioMixer.SetFloat("AmbientBGMVolume", num);
			break;
		case AudioMixerType.AmbientSE:
			num = AudioUtil.VolumeToDecibel(SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.Value * volume * _scenarioFadeVolume);
			AmbientSEGroup.audioMixer.SetFloat("AmbientSEVolume", num);
			break;
		}
		return num;
	}
}
