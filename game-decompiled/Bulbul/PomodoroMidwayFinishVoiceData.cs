using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class PomodoroMidwayFinishVoiceData
{
	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float SelectionProbability => selectionProbability;
}
