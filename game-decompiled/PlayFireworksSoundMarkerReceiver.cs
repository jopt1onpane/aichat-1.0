using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public class PlayFireworksSoundMarkerReceiver : MonoBehaviour, INotificationReceiver
{
	[Inject]
	private FireworksSoundService _fireworksSoundService;

	public void OnNotify(Playable origin, INotification notification, object context)
	{
		PlayFireworksSoundMarker playFireworksSoundMarker = notification as PlayFireworksSoundMarker;
		if (!(playFireworksSoundMarker == null))
		{
			_fireworksSoundService.PlaySound(playFireworksSoundMarker.SoundType);
		}
	}
}
