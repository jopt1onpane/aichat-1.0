using System;
using UnityEngine;

namespace Bulbul;

public class PomodoroTimerService
{
	private float _elapsedTimeSeconds;

	private float _useTimeSeconds;

	private bool _isRunning;

	private TimeSpan _currentTimeSpan;

	public void SetUp()
	{
	}

	public void ResetTimer()
	{
		_elapsedTimeSeconds = 0f;
		_isRunning = false;
	}

	public void PlayTimer(PomodoroService.PomodoroType type)
	{
		switch (type)
		{
		case PomodoroService.PomodoroType.Work:
			_currentTimeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.WorkMinutes.Value);
			break;
		case PomodoroService.PomodoroType.Break:
			_currentTimeSpan = TimeSpan.FromMinutes(SaveDataManager.Instance.PomodoroData.BreakMinutes.Value);
			break;
		}
		_elapsedTimeSeconds = 0f;
		_isRunning = true;
	}

	public void PauseTimer()
	{
		_isRunning = false;
	}

	public void UnpauseTimer()
	{
		_isRunning = true;
	}

	public void SkipTimer()
	{
		_currentTimeSpan = TimeSpan.Zero;
		_useTimeSeconds = _elapsedTimeSeconds;
		_elapsedTimeSeconds = 0f;
	}

	public TimeSpan CurrentTimeSpan()
	{
		return _currentTimeSpan;
	}

	public TimeSpan RemainTimeSpan()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(_elapsedTimeSeconds);
		TimeSpan timeSpan2 = _currentTimeSpan - timeSpan;
		if (!(timeSpan2 <= TimeSpan.Zero))
		{
			return timeSpan2;
		}
		return TimeSpan.Zero;
	}

	public bool IsTimerEnd()
	{
		if (RemainTimeSpan() == TimeSpan.Zero)
		{
			return true;
		}
		return false;
	}

	public float GetUseTimeSeconds()
	{
		return _useTimeSeconds;
	}

	public void Update()
	{
		if (_isRunning && !IsTimerEnd())
		{
			_elapsedTimeSeconds += Time.deltaTime;
			_useTimeSeconds = _elapsedTimeSeconds;
			if (IsTimerEnd())
			{
				_useTimeSeconds = (float)_currentTimeSpan.TotalSeconds;
			}
		}
	}

	public float MoveAheadTime(float seconds)
	{
		float num = seconds - (float)RemainTimeSpan().TotalSeconds;
		_elapsedTimeSeconds += seconds;
		_useTimeSeconds = _elapsedTimeSeconds;
		if (IsTimerEnd())
		{
			_useTimeSeconds = (float)_currentTimeSpan.TotalSeconds;
		}
		if (!(num < 0f))
		{
			return num;
		}
		return 0f;
	}

	public void SetRemainingTime(TimeSpan remainingTime)
	{
		TimeSpan timeSpan = _currentTimeSpan - remainingTime;
		if (timeSpan >= TimeSpan.Zero)
		{
			_elapsedTimeSeconds = (float)timeSpan.TotalSeconds;
			_useTimeSeconds = _elapsedTimeSeconds;
		}
	}

	public void ChangePomodoroUpdate()
	{
		_isRunning = !_isRunning;
	}
}
