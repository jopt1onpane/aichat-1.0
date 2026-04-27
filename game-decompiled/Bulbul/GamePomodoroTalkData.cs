using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class GamePomodoroTalkData
{
	[SerializeField]
	[Range(0f, 1f)]
	[Header("会話発生確率の最大値")]
	private float maxTalkProbability;

	[SerializeField]
	[Header("会話発生確率が上昇する上限の作業時間(分)")]
	private float maxWorkedMinutes;

	[SerializeField]
	[Header("会話発生可能時間（分）")]
	private float _pomodoroChoiceTalkAvailableTimeMinutes;

	public float MaxTalkProbability => maxTalkProbability;

	public float MaxWorkedMinutes => maxWorkedMinutes;

	public float PomodoroChoiceTalkAvailableTimeMinutes => _pomodoroChoiceTalkAvailableTimeMinutes;
}
