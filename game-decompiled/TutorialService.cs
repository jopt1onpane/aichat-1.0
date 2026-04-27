using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using R3;

public class TutorialService : IDisposable
{
	public enum TutorialPageType
	{
		None,
		GameDemo,
		ScreenUI,
		PomodoroTimer,
		LevelAndStory,
		NewEnviroment,
		Count
	}

	public enum TutorialPageOpenType
	{
		None,
		OnlyGameDemo,
		HelpAll,
		ALL,
		Count
	}

	private Subject<Unit> _onEndFirstTutorial = new Subject<Unit>();

	private ReactiveProperty<TutorialPageType> _currentPageType = new ReactiveProperty<TutorialPageType>();

	private TutorialPageOpenType _currentPageOpenType;

	private Subject<Unit> _onClose = new Subject<Unit>();

	private Subject<Unit> _onStartFocusPomodoro = new Subject<Unit>();

	private Subject<Unit> _onEndFocusPomodoro = new Subject<Unit>();

	private Subject<Unit> _onStartFocusNote = new Subject<Unit>();

	private Subject<Unit> _onEndFocusNote = new Subject<Unit>();

	private Subject<Unit> _onStartFocusTodo = new Subject<Unit>();

	private Subject<Unit> _onEndFocusTodo = new Subject<Unit>();

	private CancellationTokenSource _cts = new CancellationTokenSource();

	public Observable<Unit> OnEndFirstTutorial => _onEndFirstTutorial;

	public ReadOnlyReactiveProperty<TutorialPageType> CurrentPageType => _currentPageType;

	public TutorialPageOpenType CurrentPageOpenType => _currentPageOpenType;

	public Observable<Unit> OnClose => _onClose;

	public Observable<Unit> OnStartFocusPomodoro => _onStartFocusPomodoro;

	public Observable<Unit> OnEndFocusPomodoro => _onEndFocusPomodoro;

	public Observable<Unit> OnStartFocusNote => _onStartFocusNote;

	public Observable<Unit> OnEndFocusNote => _onEndFocusNote;

	public Observable<Unit> OnStartFocusTodo => _onStartFocusTodo;

	public Observable<Unit> OnEndFocusTodo => _onEndFocusTodo;

	public void Dispose()
	{
	}

	public void ReadyTutorial()
	{
		if (_cts == null || _cts.IsCancellationRequested)
		{
			_cts?.Dispose();
			_cts = new CancellationTokenSource();
		}
	}

	public void OnEndFirstTutorialProcess()
	{
		_onEndFirstTutorial.OnNext(Unit.Default);
	}

	public bool IsCloseTutorialPage()
	{
		if (_currentPageType.Value == TutorialPageType.None)
		{
			return true;
		}
		return false;
	}

	public void OpenTutorial(TutorialPageType pageType, TutorialPageOpenType pageOpenType)
	{
		_currentPageOpenType = pageOpenType;
		_currentPageType.Value = pageType;
	}

	public void ToPreviousPage()
	{
		int value = (int)(_currentPageType.Value - 1);
		_currentPageType.Value = (TutorialPageType)value;
	}

	public void ToNextPage()
	{
		int value = (int)(_currentPageType.Value + 1);
		_currentPageType.Value = (TutorialPageType)value;
	}

	public void CloseTutorial()
	{
		_currentPageType.Value = TutorialPageType.None;
		SaveDataManager.Instance.SavePlayerData();
		_onClose.OnNext(Unit.Default);
	}

	public async UniTask FocusPomodoro(float focusSeconds)
	{
		_onStartFocusPomodoro.OnNext(Unit.Default);
		await UniTask.Delay(TimeSpan.FromSeconds(focusSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cts.Token);
		_onEndFocusPomodoro.OnNext(Unit.Default);
	}

	public async UniTask FocusTodo(float focusSeconds)
	{
		_onStartFocusTodo.OnNext(Unit.Default);
		await UniTask.Delay(TimeSpan.FromSeconds(focusSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cts.Token);
		_onEndFocusTodo.OnNext(Unit.Default);
	}

	public async UniTask FocusNote(float focusSeconds)
	{
		_onStartFocusNote.OnNext(Unit.Default);
		await UniTask.Delay(TimeSpan.FromSeconds(focusSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cts.Token);
		_onEndFocusNote.OnNext(Unit.Default);
	}

	public void Cancel()
	{
		_cts.Cancel();
	}

	public void EndTidying()
	{
		_onEndFocusPomodoro.OnNext(Unit.Default);
		_onEndFocusTodo.OnNext(Unit.Default);
		_onEndFocusNote.OnNext(Unit.Default);
	}
}
