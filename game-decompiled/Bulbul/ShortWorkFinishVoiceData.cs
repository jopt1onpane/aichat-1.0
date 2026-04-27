using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class ShortWorkFinishVoiceData
{
	[SerializeField]
	[Header("短時間判定分数")]
	private float judgeShortMinutes;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float JudgeShortMinutes => judgeShortMinutes;

	public float SelectionProbability => selectionProbability;
}
