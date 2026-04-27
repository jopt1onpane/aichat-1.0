using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ChangeMotionSoundMarker : Marker, INotification
{
	[SerializeField]
	private MotionSoundController.MotionVoicePlayKind motionPlayKind;

	public MotionSoundController.MotionVoicePlayKind MotionPlayKind => motionPlayKind;

	public PropertyName id => new PropertyName("ChangeMotionSound");
}
