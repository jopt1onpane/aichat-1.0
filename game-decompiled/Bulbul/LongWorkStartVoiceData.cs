using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class LongWorkStartVoiceData
{
	[SerializeField]
	[Header("長時間開始判定時間")]
	private float judgeLongMinutes;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float JudgeLongMinutes => judgeLongMinutes;

	public float SelectionProbability => selectionProbability;
}
