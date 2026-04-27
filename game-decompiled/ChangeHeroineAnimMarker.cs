using System;
using Bulbul;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ChangeHeroineAnimMarker : Marker, INotification
{
	[SerializeField]
	private HeroineService.AnimationType animationType;

	public HeroineService.AnimationType AnimationType => animationType;

	public PropertyName id => new PropertyName("ChangeHeroineAnim");
}
