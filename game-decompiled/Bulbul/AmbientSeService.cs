using System.Linq;
using KanKikuchi.AudioManager;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class AmbientSeService
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	public void Play(AmbientSeParam parameter)
	{
		AmbientSeSoundMasterData ambientSeSoundMasterData = _masterDataLoader.AmbientSeMasterList.FirstOrDefault((AmbientSeSoundMasterData x) => x.AmbientSeSound == parameter.AmbientSeSound);
		if (ambientSeSoundMasterData == null)
		{
			Debug.LogWarning($"{parameter.AmbientSeSound}用環境音がない");
			return;
		}
		SingletonMonoBehaviour<AmbientSEManager>.Instance.GetAudioPlayerByName(ambientSeSoundMasterData.AudioClipName);
		float volumeRate = Mathf.Clamp(parameter.VolumeRate, 0f, 1f);
		SingletonMonoBehaviour<AmbientSEManager>.Instance.Play(ambientSeSoundMasterData.AudioClip, volumeRate, 0f, 1f, isLoop: false, parameter.IsAllowsDuplicate);
	}

	public void Stop(AmbientSeType sound)
	{
		AmbientSeSoundMasterData ambientSeSoundMasterData = _masterDataLoader.AmbientSeMasterList.FirstOrDefault((AmbientSeSoundMasterData x) => x.AmbientSeSound == sound);
		if (ambientSeSoundMasterData != null)
		{
			SingletonMonoBehaviour<AmbientSEManager>.Instance.Stop(ambientSeSoundMasterData.AudioClipName);
		}
	}
}
