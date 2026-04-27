using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class LevelUpInfoData
{
	[SerializeField]
	[Header("ポモドーロ: 作業ベース分")]
	private float pomodoroExpBaseMinutes;

	[SerializeField]
	[Header("ポモドーロ: 作業ベース分につき、経験値がどれだけ貰えるか")]
	private float pomodoroExpBaseMax;

	[SerializeField]
	[Header("ポモドーロ: 開始時間から終了時間までの経験値増加上限時間")]
	private float pomodoroExpBaseMaxHour;

	[SerializeField]
	[Header("レベルごとの必要経験値量")]
	private float[] nextLevelNecessaryExpArray;

	[SerializeField]
	[Header("上記レベルを超えた後に、レベル上昇に必要な経験値")]
	private float nextLevelNecessaryExpBase;

	[SerializeField]
	[Range(0f, 3f)]
	[Header("UI：ゲージ上昇速度")]
	private float _gaugeUpSpeed = 0.8f;

	public float PomodoroExpBaseMinutes => pomodoroExpBaseMinutes;

	public float PomodoroExpBaseMax => pomodoroExpBaseMax;

	public float PomodoroExpBaseMaxHour => pomodoroExpBaseMaxHour;

	public float[] NextLevelNecessaryExpArray => nextLevelNecessaryExpArray;

	public float NextLevelNecessaryExpBase => nextLevelNecessaryExpBase;

	public float GaugeUpSpeed => _gaugeUpSpeed;
}
