using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class PointUpInfoData
{
	[SerializeField]
	[Header("ポモドーロ: 作業ベース分")]
	private float pomodoroPointBaseMinutes;

	[SerializeField]
	[Header("ポモドーロ: 作業ベース分につき、ポイントがどれだけ貰えるか")]
	private float pomodoroPointBaseMax;

	[SerializeField]
	[Header("ポモドーロ: 開始時間から終了時間までのポイント増加上限時間")]
	private float pomodoroPointBaseMaxHour;

	public float PomodoroPointBaseMinutes => pomodoroPointBaseMinutes;

	public float PomodoroPointBaseMax => pomodoroPointBaseMax;

	public float PomodoroPointBaseMaxHour => pomodoroPointBaseMaxHour;
}
