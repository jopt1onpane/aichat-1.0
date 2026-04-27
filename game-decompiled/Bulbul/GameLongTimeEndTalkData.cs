using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class GameLongTimeEndTalkData
{
	[SerializeField]
	[Header("長時間判定時間")]
	private float judgeLongHours;

	[SerializeField]
	[Header("選出確率")]
	[Range(0f, 100f)]
	private float selectionProbability;

	public float JudgeLongHours => judgeLongHours;

	public float SelectionProbability => selectionProbability;
}
