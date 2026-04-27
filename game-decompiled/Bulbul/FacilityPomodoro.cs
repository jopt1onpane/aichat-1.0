using System;
using R3;
using VContainer;

namespace Bulbul;

public class FacilityPomodoro : FacilityBase
{
	public struct PomodoroTimerSettings
	{
		public TimeSpan WorkTimeSpan;

		public TimeSpan RestTimeSpan;

		public int LoopCount;

		public bool SoundFinished;

		public bool IsValid;
	}

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private IPomodoroTimerUI _pomodoroTimerUI;

	[Inject]
	private IPomodoroStopSignUI _pomodoroStopSignUI;

	public void Setup()
	{
		_pomodoroService.Setup();
		_pomodoroTimerUI.Setup();
		_pomodoroStopSignUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonIncrementWorkTime, delegate
		{
			_pomodoroService.IncrementWorkMinutes();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonDecrementWorkTime, delegate
		{
			_pomodoroService.DecrementWorkMinutes();
		}).AddTo(this);
		_pomodoroTimerUI.OnEndEditWorkTimeMaxMinutesText.Subscribe(delegate(int workTimeMinutes)
		{
			_pomodoroService.SetWorkMinutes(workTimeMinutes);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonIncrementBreakTime, delegate
		{
			_pomodoroService.IncrementBreakMinutes();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonDecrementBreakTime, delegate
		{
			_pomodoroService.DecrementBreakMinutes();
		}).AddTo(this);
		_pomodoroTimerUI.OnEndEditBreakTimeMaxMinutesText.Subscribe(delegate(int breakTimeMinutes)
		{
			_pomodoroService.SetBreakMinutes(breakTimeMinutes);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonIncrementLoopCount, delegate
		{
			_pomodoroService.IncrementLoopCount();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonDecrementLoopCount, delegate
		{
			_pomodoroService.DecrementLoopCount();
		}).AddTo(this);
		_pomodoroTimerUI.OnEndEditLoopCountText.Subscribe(delegate(int loopCount)
		{
			_pomodoroService.SetLoopCount(loopCount);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonReset, delegate
		{
			_pomodoroService.ResetTimer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonStartPomodoro, delegate
		{
			_pomodoroService.StartPomodoro();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonPlayOrPausePomodoro, delegate
		{
			_pomodoroService.PlayOrPausePomodoroTimer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroTimerUI.OnClickButtonSkipPomodoro, delegate
		{
			_pomodoroService.SkipTimer();
		}).AddTo(this);
	}

	public void UpdateFacility()
	{
		_pomodoroService.Update();
	}

	public bool IsTalkStartReady()
	{
		return _pomodoroService.IsTalkStartReady();
	}

	public bool IsTalkPlayEnd()
	{
		return _pomodoroService.IsTalkPlayEnd();
	}

	public void StartTalk()
	{
		_pomodoroService.StartTalk();
	}

	public void EndTalk()
	{
		_pomodoroService.EndTalk();
	}

	public bool IsCurrentWorking()
	{
		return _pomodoroService.IsCurrentWorking();
	}

	public bool IsCurrentResting()
	{
		return _pomodoroService.IsCurrentResting();
	}
}
