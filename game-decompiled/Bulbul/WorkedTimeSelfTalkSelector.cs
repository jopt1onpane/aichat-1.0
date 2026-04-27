using System;
using R3;
using UnityEngine;

namespace Bulbul;

public class WorkedTimeSelfTalkSelector : IDisposable
{
	public enum EpisodeNumber
	{
		WorkedTime_2Hour_1 = 1,
		WorkedTime_5Hour_1,
		WorkedTime_10Hour_1,
		WorkedTime_10Hour_2
	}

	private ITalkSelector _2HourSelector;

	private ITalkSelector _5HourSelector;

	private ITalkSelector _10HourSelector;

	private float _2HourThresholdMinSeconds;

	private float _2HourThresholdMaxSeconds;

	private float _5HourThresholdMinSeconds;

	private float _5HourThresholdMaxSeconds;

	private float _10HourThresholdMinSeconds;

	private float _10HourThresholdMaxSeconds;

	private float _lastStartWorkTimeSeconds;

	private float _totalWorkedSeconds;

	private DisposableBag _disposableBag;

	public void Setup()
	{
		_disposableBag.Clear();
		_2HourSelector = new WorkedTime2HourSelfTalkSelector();
		_5HourSelector = new WorkedTime5HourSelfTalkSelector();
		_10HourSelector = new WorkedTime10HourSelfTalkSelector();
		_2HourSelector.Setup();
		_5HourSelector.Setup();
		_10HourSelector.Setup();
		_2HourThresholdMinSeconds = 7200f;
		_2HourThresholdMaxSeconds = 10800f;
		_5HourThresholdMinSeconds = 18000f;
		_5HourThresholdMaxSeconds = 21600f;
		_10HourThresholdMinSeconds = 36000f;
		_10HourThresholdMaxSeconds = 54000f;
		_lastStartWorkTimeSeconds = 0f;
		_totalWorkedSeconds = 0f;
		PomodoroService pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		ObservableSubscribeExtensions.Subscribe(pomodoroService.OnStartWork, delegate
		{
			MeasurementStartWorkTime();
		}).AddTo(ref _disposableBag);
		ObservableSubscribeExtensions.Subscribe(pomodoroService.OnEndWork, delegate
		{
			MeasurementEndWorkTime();
		}).AddTo(ref _disposableBag);
		pomodoroService.OnCompletePomodoro.Subscribe(delegate(PomodoroService.PomodoroType completePomodoroType)
		{
			if (completePomodoroType == PomodoroService.PomodoroType.Work)
			{
				MeasurementEndWorkTime();
			}
		}).AddTo(ref _disposableBag);
		void MeasurementEndWorkTime()
		{
			_totalWorkedSeconds += Mathf.Ceil(Time.time - _lastStartWorkTimeSeconds);
		}
		void MeasurementStartWorkTime()
		{
			_lastStartWorkTimeSeconds = Time.time;
		}
	}

	public ScenarioInfo TakeNextVoice()
	{
		ITalkSelector talkSelector = null;
		if (_totalWorkedSeconds >= _10HourThresholdMinSeconds && _totalWorkedSeconds < _10HourThresholdMaxSeconds && _10HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _10HourSelector;
		}
		else if (_totalWorkedSeconds >= _5HourThresholdMinSeconds && _totalWorkedSeconds < _5HourThresholdMaxSeconds && _5HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _5HourSelector;
		}
		else if (_totalWorkedSeconds >= _2HourThresholdMinSeconds && _totalWorkedSeconds < _2HourThresholdMaxSeconds && _2HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _2HourSelector;
		}
		if (talkSelector == null)
		{
			return null;
		}
		return new ScenarioInfo(talkSelector.GetScenarioType(), talkSelector.TakeNextVoice());
	}

	public void Dispose()
	{
		_disposableBag.Dispose();
	}
}
