using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class ShortWorkedBreakStartVoiceData
{
	[SerializeField]
	[Header("短い作業判定分数")]
	private float _shortWorkMinutes;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float ShortWorkMinutes => _shortWorkMinutes;

	public float SelectionProbability => selectionProbability;
}
