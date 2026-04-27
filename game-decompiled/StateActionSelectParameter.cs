using System;
using UnityEngine;

[Serializable]
public class StateActionSelectParameter
{
	[SerializeField]
	[Header("確認用：値が高いほど再生されやすくなる")]
	public float _possibilityAmount;

	[SerializeField]
	[Header("選択される可能性の上昇割合")]
	[Range(0f, 1f)]
	public float _addPossibilityRatio;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float _playedSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float _playedInvalidSeconds;

	public MyStopWatch _invalidStopWatch = new MyStopWatch();

	public void InitAll()
	{
		_possibilityAmount = 0f;
		InitStopWatch();
	}

	public void InitStopWatch()
	{
		_invalidStopWatch.Watch.Stop();
		_invalidStopWatch.SetTargetSeconds(0f);
	}

	public float UpdatePossibilityAmount(float baseAmount)
	{
		float num = baseAmount * _addPossibilityRatio;
		if (_invalidStopWatch.IsElapsedTargetTime())
		{
			_possibilityAmount += num;
		}
		return num;
	}

	public float UseAction()
	{
		float num = _possibilityAmount * _playedSubtractRatio;
		_possibilityAmount -= num;
		_invalidStopWatch.SetTargetSeconds(_playedInvalidSeconds);
		_invalidStopWatch.Watch.Restart();
		return num;
	}
}
