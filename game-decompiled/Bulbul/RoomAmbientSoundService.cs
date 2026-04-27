using System.Collections.Generic;
using System.Linq;
using KanKikuchi.AudioManager;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class RoomAmbientSoundService
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	public void Play(AmbientSoundParam parameter)
	{
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(parameter.AmbientSound);
		if (ambientSoundData != null)
		{
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.GetAudioPlayerByName(ambientSoundData.AudioClipName);
			float pitch = ((parameter.Pitch <= 0f) ? 1f : parameter.Pitch);
			float volumeRate = Mathf.Clamp01(parameter.Volume);
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.Play(ambientSoundData.AudioClip, volumeRate, 0f, pitch, parameter.IsLoop, parameter.IsAllowsDuplicate);
		}
	}

	public void Stop(AmbientSoundType type)
	{
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(type);
		if (ambientSoundData != null)
		{
			Stop(ambientSoundData);
		}
	}

	private void Stop(AmbientSoundMasterData ambientSound)
	{
		if (SingletonMonoBehaviour<AmbientBGMManager>.Instance.GetAudioPlayerByName(ambientSound.AudioClipName) != null)
		{
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.Stop(ambientSound.AudioClip);
		}
	}

	public bool IsPlaying(AmbientSoundType type)
	{
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(type);
		if (ambientSoundData == null)
		{
			return false;
		}
		if (SingletonMonoBehaviour<AmbientBGMManager>.Instance.GetAudioPlayerByName(ambientSoundData.AudioClipName) == null)
		{
			return false;
		}
		return true;
	}

	public void ChangeVolume(AmbientSoundType type, float volume)
	{
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(type);
		if (ambientSoundData == null)
		{
			return;
		}
		List<AudioPlayer> audioPlayersByName = SingletonMonoBehaviour<AmbientBGMManager>.Instance.GetAudioPlayersByName(ambientSoundData.AudioClipName);
		if (audioPlayersByName == null)
		{
			return;
		}
		foreach (AudioPlayer item in audioPlayersByName)
		{
			item.ChangeVolumeRate(volume);
		}
	}

	public float CurrentVolume(AmbientSoundType type)
	{
		float num = 0f;
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(type);
		if (ambientSoundData == null)
		{
			return num;
		}
		return SingletonMonoBehaviour<AmbientBGMManager>.Instance.GetAudioPlayerByName(ambientSoundData.AudioClipName)?.CurrentVolume ?? num;
	}

	public void Replay(AmbientSoundParam parameter)
	{
		AmbientSoundMasterData ambientSoundData = GetAmbientSoundData(parameter.AmbientSound);
		if (ambientSoundData != null)
		{
			Stop(ambientSoundData);
			float pitch = ((parameter.Pitch <= 0f) ? 1f : parameter.Pitch);
			float volumeRate = Mathf.Clamp01(parameter.Volume);
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.Play(ambientSoundData.AudioClip, volumeRate, 0f, pitch, parameter.IsLoop, parameter.IsAllowsDuplicate);
		}
	}

	private AmbientSoundMasterData GetAmbientSoundData(AmbientSoundType soundType)
	{
		AmbientSoundMasterData result = null;
		AmbientSoundMasterData ambientSoundMasterData = _masterDataLoader.AmbientMasterList.FirstOrDefault((AmbientSoundMasterData x) => x.AmbientSound == soundType);
		if (ambientSoundMasterData == null)
		{
			Debug.LogWarning($"{soundType}用環境音がない");
		}
		else
		{
			result = ambientSoundMasterData;
		}
		return result;
	}
}
