using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class ContinuousWorkStartVoiceData
{
	[SerializeField]
	[Header("連続開始判定分数")]
	private float continuousMinutes;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float ContinuousMinutes => continuousMinutes;

	public float SelectionProbability => selectionProbability;
}
