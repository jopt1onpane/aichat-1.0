using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class GamePomodoroVoiceData
{
	[SerializeField]
	[Header("作業:長時間開始")]
	private LongWorkStartVoiceData longWorkStartVoiceData;

	[SerializeField]
	[Header("作業:連続開始")]
	private ContinuousWorkStartVoiceData continuousWorkStartVoiceData;

	[SerializeField]
	[Header("休憩:直前の作業が短い時の休憩開始")]
	private ShortWorkedBreakStartVoiceData shortWorkedBreakStartVoiceData;

	[SerializeField]
	[Header("休憩:直前の作業が長い時の休憩開始")]
	private LongWorkedBreakStartVoiceData longWorkedBreakStartVoiceData;

	[SerializeField]
	[Header("終了:長い作業終了")]
	private LongWorkFinishVoiceData longWorkFinishVoiceData;

	[SerializeField]
	[Header("終了:短い作業終了")]
	private ShortWorkFinishVoiceData shortWorkFinishVoiceData;

	[SerializeField]
	[Header("終了:途中で終了")]
	private PomodoroMidwayFinishVoiceData midwayFinishVoiceData;

	public LongWorkStartVoiceData LongWorkStartVoiceData => longWorkStartVoiceData;

	public ContinuousWorkStartVoiceData ContinuousWorkStartVoiceData => continuousWorkStartVoiceData;

	public ShortWorkedBreakStartVoiceData ShortWorkedBreakStartVoiceData => shortWorkedBreakStartVoiceData;

	public LongWorkedBreakStartVoiceData LongWorkedBreakStartVoiceData => longWorkedBreakStartVoiceData;

	public LongWorkFinishVoiceData LongWorkFinishVoiceData => longWorkFinishVoiceData;

	public ShortWorkFinishVoiceData ShortWorkFinishVoiceData => shortWorkFinishVoiceData;

	public PomodoroMidwayFinishVoiceData MidwayFinishVoiceData => midwayFinishVoiceData;
}
