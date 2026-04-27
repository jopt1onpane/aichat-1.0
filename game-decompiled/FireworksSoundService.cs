using Bulbul;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public class FireworksSoundService : MonoBehaviour
{
	public enum FireworksSoundType
	{
		Don,
		Parapara
	}

	[Inject]
	private RoomAmbientSoundService _ambientSoundService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[SerializeField]
	private PlayableDirector _director;

	[Header("花火サウンド ランダム化設定")]
	[SerializeField]
	[Range(0.5f, 1.5f)]
	private float _volumeRandomMin = 0.9f;

	[SerializeField]
	[Range(0.5f, 1.5f)]
	private float _volumeRandomMax = 1.1f;

	[SerializeField]
	[Range(0.5f, 2f)]
	private float _pitchRandomMin = 0.95f;

	[SerializeField]
	[Range(0.5f, 2f)]
	private float _pitchRandomMax = 1.05f;

	public void Replay()
	{
		_director.Pause();
		_director.time = 0.0;
		_director.Evaluate();
		_director.extrapolationMode = DirectorWrapMode.Loop;
		_director.Play();
	}

	public void Stop()
	{
		_director.Stop();
	}

	public bool IsPlaying()
	{
		return _director.state == PlayState.Playing;
	}

	public void PlaySound(FireworksSoundType type)
	{
		switch (type)
		{
		case FireworksSoundType.Don:
			PlayFireWorksFirst();
			break;
		case FireworksSoundType.Parapara:
			PlayFireWorksSecond();
			break;
		}
	}

	private void PlayFireWorksFirst()
	{
		AmbientSoundParam parameter = CreateRandomizedParam(AmbientSoundType.Fireworks_First);
		_ambientSoundService.Play(parameter);
	}

	private void PlayFireWorksSecond()
	{
		AmbientSoundParam parameter = CreateRandomizedParam(AmbientSoundType.Fireworks_Second);
		_ambientSoundService.Play(parameter);
	}

	private AmbientSoundParam CreateRandomizedParam(AmbientSoundType type)
	{
		float volume = Mathf.Clamp01(GetVolume() * Random.Range(Mathf.Min(_volumeRandomMin, _volumeRandomMax), Mathf.Max(_volumeRandomMin, _volumeRandomMax)));
		float pitch = Random.Range(Mathf.Min(_pitchRandomMin, _pitchRandomMax), Mathf.Max(_pitchRandomMin, _pitchRandomMax));
		return new AmbientSoundParam
		{
			AmbientSound = type,
			Volume = volume,
			Pitch = pitch,
			IsLoop = false,
			IsAllowsDuplicate = true
		};
	}

	public float GetVolume()
	{
		(float volume, bool isMute) volume = _environmentDataService.GetVolume(AmbientSoundType.Fireworks_First);
		var (result, _) = volume;
		if (volume.isMute)
		{
			result = 0f;
		}
		return result;
	}
}
