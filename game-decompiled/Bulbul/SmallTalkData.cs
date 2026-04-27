using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class SmallTalkData
{
	[SerializeField]
	[Header("次の小話までのインターバル(分)")]
	private float intervalMinutes = 5f;

	public float IntervalMinutes => intervalMinutes;
}
