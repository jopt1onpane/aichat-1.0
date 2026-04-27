using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public class ChangeMotionSoundMarkerReceiver : MonoBehaviour, INotificationReceiver
{
	[Inject]
	private MotionSoundController _motionSoundCtr;

	public void OnNotify(Playable origin, INotification notification, object context)
	{
		ChangeMotionSoundMarker changeMotionSoundMarker = notification as ChangeMotionSoundMarker;
		if (!(changeMotionSoundMarker == null))
		{
			_motionSoundCtr.ChangeVoicePlayKind(changeMotionSoundMarker.MotionPlayKind);
		}
	}
}
