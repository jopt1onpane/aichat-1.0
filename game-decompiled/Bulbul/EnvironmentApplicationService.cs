using VContainer;

namespace Bulbul;

public class EnvironmentApplicationService
{
	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private RoomAmbientSoundService _ambientSoundService;

	[Inject]
	private FireworksSoundService _fireworksSoundService;

	public void ApplyWindow(WindowViewType windowType, bool active)
	{
		if (active)
		{
			_windowViewService.ActivateWindow(windowType);
		}
		else
		{
			_windowViewService.DeactivateWindow(windowType);
		}
		switch (windowType)
		{
		case WindowViewType.ThunderRain:
			if (active)
			{
				ReplayAmbientSound(AmbientSoundType.ThunderRain);
			}
			break;
		case WindowViewType.Fireworks:
			if (active)
			{
				_fireworksSoundService.Replay();
			}
			else if (_fireworksSoundService.IsPlaying() && _fireworksSoundService.GetVolume() <= 0f)
			{
				_fireworksSoundService.Stop();
			}
			break;
		}
	}

	public void ApplySound(AmbientSoundType ambientSoundType, bool active, float volume)
	{
		if (!active)
		{
			volume = 0f;
		}
		if (ambientSoundType - 5 <= AmbientSoundType.PinkNoise)
		{
			if (active)
			{
				if (!_fireworksSoundService.IsPlaying())
				{
					_fireworksSoundService.Replay();
				}
			}
			else if (!_windowViewService.IsActiveWindow(WindowViewType.Fireworks))
			{
				_fireworksSoundService.Stop();
			}
			_ambientSoundService.ChangeVolume(AmbientSoundType.Fireworks_First, volume);
			_ambientSoundService.ChangeVolume(AmbientSoundType.Fireworks_Second, volume);
		}
		else if (active)
		{
			if (!_ambientSoundService.IsPlaying(ambientSoundType))
			{
				_ambientSoundService.Play(new AmbientSoundParam
				{
					AmbientSound = ambientSoundType,
					Volume = volume,
					IsLoop = true,
					IsAllowsDuplicate = true
				});
			}
			_ambientSoundService.ChangeVolume(ambientSoundType, volume);
		}
		else
		{
			_ambientSoundService.ChangeVolume(ambientSoundType, volume);
		}
	}

	public void ReplayAmbientSound(AmbientSoundType ambientSoundType)
	{
		_ambientSoundService.Replay(new AmbientSoundParam
		{
			AmbientSound = ambientSoundType,
			Volume = _ambientSoundService.CurrentVolume(ambientSoundType),
			IsLoop = true,
			IsAllowsDuplicate = true
		});
	}
}
