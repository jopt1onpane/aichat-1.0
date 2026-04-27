using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class LeaveChairJudge : IDisposable
{
	public enum LeaveChairDestination
	{
		None,
		Sofa,
		Far
	}

	private LeaveChairDestination _destination;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	private LeaveChairData _leaveChairMasterData;

	private float _lastStartWorkTimeSeconds;

	private int _totalWorkMinutes;

	private float _lastStartBreakTimeSeconds;

	private int _totalBreakMinutes;

	private int _consideredLongWorkMinutes;

	private int _consideredLongBreakMinutes;

	private readonly CompositeDisposable _disposable = new CompositeDisposable();

	public LeaveChairDestination Destination => _destination;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_heroineService = RoomLifetimeScope.Resolve<HeroineService>();
		_masterDataLoader = RoomLifetimeScope.Resolve<MasterDataLoader>();
		_leaveChairMasterData = _masterDataLoader.HeroineAIMasterData.LeaveChairData;
		CalculateConsideredLongMinutes();
		SetupMeasurementTime();
		SetupJudge();
		void MeasurementEndBreakTime()
		{
			_totalBreakMinutes += (int)(Time.time - _lastStartBreakTimeSeconds) / 60;
		}
		void MeasurementEndWorkTime()
		{
			_totalWorkMinutes += (int)(Time.time - _lastStartWorkTimeSeconds) / 60;
		}
		void MeasurementStartBreakTime()
		{
			_lastStartBreakTimeSeconds = Time.time;
		}
		void MeasurementStartWorkTime()
		{
			_lastStartWorkTimeSeconds = Time.time;
		}
		void SetupJudge()
		{
			ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromMinutes(_leaveChairMasterData.LotteryIntervalMinutes)), delegate
			{
				if (_pomodoroService.CurrentPomodoroType != PomodoroService.PomodoroType.Work)
				{
					if (_heroineService.IsLeaveChair())
					{
						return;
					}
					MeasurementEndBreakTime();
					MeasurementStartBreakTime();
				}
				JudgeLeaveChair();
			}).AddTo(_disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
			{
				JudgeLeaveChair();
			}).AddTo(_disposable);
		}
		void SetupMeasurementTime()
		{
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnStartWork, delegate
			{
				MeasurementStartWorkTime();
			}).AddTo(_disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnEndWork, delegate
			{
				MeasurementEndWorkTime();
			}).AddTo(_disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnStartBreak, delegate
			{
				MeasurementStartBreakTime();
			}).AddTo(_disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnEndBreak, delegate
			{
				MeasurementEndBreakTime();
			}).AddTo(_disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnStartPomodoro, delegate
			{
				MeasurementEndBreakTime();
			}).AddTo(_disposable);
			_pomodoroService.OnCompletePomodoro.Subscribe(delegate(PomodoroService.PomodoroType pomodoroType)
			{
				switch (pomodoroType)
				{
				case PomodoroService.PomodoroType.Work:
					MeasurementEndWorkTime();
					break;
				case PomodoroService.PomodoroType.Break:
					MeasurementEndBreakTime();
					break;
				}
				MeasurementStartBreakTime();
			}).AddTo(_disposable);
			MeasurementStartBreakTime();
		}
	}

	private void JudgeLeaveChair()
	{
		if (!_pomodoroService.IsCurrentWorking() && (_pomodoroService.CurrentPomodoroType != PomodoroService.PomodoroType.Complete || !_heroineService.IsNeedChangeWantTalk()))
		{
			bool num = _totalWorkMinutes >= _consideredLongWorkMinutes;
			bool flag = _totalBreakMinutes >= _consideredLongBreakMinutes;
			if ((num || flag) && ProbabilityUtility.IsOccurredInPercent(_leaveChairMasterData.OccurrenceProbabilityPercent))
			{
				_destination = ((UnityEngine.Random.value < 0.5f) ? LeaveChairDestination.Sofa : LeaveChairDestination.Far);
			}
		}
	}

	private void CalculateConsideredLongMinutes()
	{
		_consideredLongWorkMinutes = UnityEngine.Random.Range(_leaveChairMasterData.WorkMinutesMin, _leaveChairMasterData.WorkMinutesMax + 1);
		_consideredLongBreakMinutes = UnityEngine.Random.Range(_leaveChairMasterData.BreakMinutesMin, _leaveChairMasterData.BreakMinutesMax + 1);
	}

	public bool NeedLeaveChair()
	{
		return _destination != LeaveChairDestination.None;
	}

	public void OnLeaveChair()
	{
		ResetJudgeConditions();
		_destination = LeaveChairDestination.None;
	}

	private void ResetJudgeConditions()
	{
		_totalBreakMinutes = 0;
		_totalWorkMinutes = 0;
		CalculateConsideredLongMinutes();
	}

	public void Dispose()
	{
		_disposable?.Dispose();
	}
}
