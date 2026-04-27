using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class GameShortTimeEndTalkData
{
	[SerializeField]
	[Header("短時間判定時間")]
	private float judgeShortHours;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float JudgeShortHours => judgeShortHours;

	public float SelectionProbability => selectionProbability;
}
