using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class LongWorkedBreakStartVoiceData
{
	[SerializeField]
	[Header("長時間作業判定分数")]
	private float judgeLongMinutes;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float JudgeLongMinutes => judgeLongMinutes;

	public float SelectionProbability => selectionProbability;
}
