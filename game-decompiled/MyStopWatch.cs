using System.Diagnostics;
using UnityEngine;

public class MyStopWatch
{
	private Stopwatch _watch = new Stopwatch();

	private float _targetSeconds = -1f;

	public Stopwatch Watch => _watch;

	public bool IsElapsedTargetTime()
	{
		if (_watch.Elapsed.TotalSeconds >= (double)_targetSeconds || _targetSeconds <= 0f)
		{
			return true;
		}
		return false;
	}

	public void ChangeTargetSecondsForRandom(float delaySecondsMin, float delaySecondsMax)
	{
		SetTargetSeconds(Random.Range(delaySecondsMin, delaySecondsMax));
	}

	public void SetTargetSeconds(float targetSeconds)
	{
		_targetSeconds = targetSeconds;
	}
}
