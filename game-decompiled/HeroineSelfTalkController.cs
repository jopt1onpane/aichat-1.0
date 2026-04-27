using System;
using R3;
using UnityEngine;

public class HeroineSelfTalkController : MonoBehaviour
{
	[SerializeField]
	[Header("喋る抽選の更新Delay秒数\n最低値")]
	private float _talkLotteryDelaySecondsMin;

	[SerializeField]
	[Header("最大値")]
	private float _talkLotteryDelaySecondsMax;

	[SerializeField]
	[Header("喋る確率")]
	[Range(0f, 100f)]
	private int _selfTalkProbability;

	[SerializeField]
	[Header("一度の休憩で何回まで喋るか")]
	private int _oneBreakSelfTalkMax = 2;

	[SerializeField]
	[Header("リセット後、何秒独り言の発生を抑制するか")]
	private int _resetDelaySeconds = 20;

	private DateTime _talkDelayStartDateTime;

	private float _talkLotteryDelaySeconds;

	private bool _isTalkReady;

	private int _oneBreakSelfTalkCount;

	private DateTime _resetDelayStartDateTime;

	public bool IsTalkReady => _isTalkReady;

	public void Setup(MotionSoundController motionSoundController)
	{
		ObservableSubscribeExtensions.Subscribe(motionSoundController.OnPlayMotionVoice, delegate
		{
			RestartTalkDelayTime();
		}).AddTo(this);
	}

	public void UpdateLottery()
	{
		if (!_isTalkReady && _oneBreakSelfTalkCount < _oneBreakSelfTalkMax && !((DateTime.Now - _resetDelayStartDateTime).TotalSeconds < (double)_resetDelaySeconds) && (DateTime.Now - _talkDelayStartDateTime).TotalSeconds >= (double)_talkLotteryDelaySeconds && UnityEngine.Random.Range(1, 101) <= _selfTalkProbability)
		{
			_isTalkReady = true;
		}
	}

	public void UseSelfTalk()
	{
		_oneBreakSelfTalkCount++;
		_isTalkReady = false;
		RestartTalkDelayTime();
	}

	public void RestartTalkDelayTime()
	{
		_isTalkReady = false;
		_talkDelayStartDateTime = DateTime.Now;
		_talkLotteryDelaySeconds = UnityEngine.Random.Range(_talkLotteryDelaySecondsMin, _talkLotteryDelaySecondsMax);
	}

	public void OnStartBreak()
	{
		_oneBreakSelfTalkCount = 0;
		_resetDelayStartDateTime = DateTime.Now;
		RestartTalkDelayTime();
	}
}
