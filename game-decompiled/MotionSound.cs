using System.Linq;
using Bulbul;
using KanKikuchi.AudioManager;
using NestopiSystem.DIContainers;

public class MotionSound
{
	protected AmbientSeService _ambientSeService;

	protected MasterDataLoader _masterDataLoader;

	protected DecorationService _decorationService;

	public virtual void Play()
	{
	}

	protected void Play(AmbientSeType soundType, bool isAllowsDuplicate = false)
	{
		if (_ambientSeService == null)
		{
			_ambientSeService = RoomLifetimeScope.Resolve<AmbientSeService>();
		}
		if (_masterDataLoader == null)
		{
			_masterDataLoader = ProjectLifetimeScope.Resolve<MasterDataLoader>();
		}
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = soundType,
			IsAllowsDuplicate = isAllowsDuplicate,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(soundType)
		});
	}

	protected void Stop(AmbientSeType soundType)
	{
		if (_ambientSeService == null)
		{
			_ambientSeService = RoomLifetimeScope.Resolve<AmbientSeService>();
		}
		_ambientSeService.Stop(soundType);
	}

	protected bool IsPlaying(AmbientSeType soundType)
	{
		if (_masterDataLoader == null)
		{
			_masterDataLoader = ProjectLifetimeScope.Resolve<MasterDataLoader>();
		}
		AmbientSeSoundMasterData ambientSeSoundMasterData = _masterDataLoader.AmbientSeMasterList.FirstOrDefault((AmbientSeSoundMasterData x) => x.AmbientSeSound == soundType);
		if (ambientSeSoundMasterData == null)
		{
			return false;
		}
		AudioPlayer audioPlayerByName = SingletonMonoBehaviour<AmbientSEManager>.Instance.GetAudioPlayerByName(ambientSeSoundMasterData.AudioClipName);
		if (audioPlayerByName == null)
		{
			return false;
		}
		if (audioPlayerByName.CurrentState != AudioPlayer.State.Playing && audioPlayerByName.CurrentState != AudioPlayer.State.Delay)
		{
			return audioPlayerByName.CurrentState == AudioPlayer.State.Fading;
		}
		return true;
	}
}
