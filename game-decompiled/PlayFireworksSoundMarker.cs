using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PlayFireworksSoundMarker : Marker, INotification
{
	[SerializeField]
	private FireworksSoundService.FireworksSoundType soundType;

	public FireworksSoundService.FireworksSoundType SoundType => soundType;

	public PropertyName id => new PropertyName("FireworksSoundType");
}
