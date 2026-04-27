using Bulbul;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public class ChangeHeroineAnimMarkerReceiver : MonoBehaviour, INotificationReceiver
{
	[Inject]
	private HeroineService _heroineService;

	public void OnNotify(Playable origin, INotification notification, object context)
	{
		ChangeHeroineAnimMarker changeHeroineAnimMarker = notification as ChangeHeroineAnimMarker;
		if (!(changeHeroineAnimMarker == null))
		{
			_heroineService.ChangeAnimation(changeHeroineAnimMarker.AnimationType);
		}
	}
}
