using System;
using R3;
using VContainer;

namespace Bulbul;

public class AchievementNoticeService : IDisposable, IAchievementNotice
{
	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private FacilityStory _facilityStory;

	[Inject]
	private FacilityTodo _facilityTodo;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private NoteService _noteService;

	[Inject]
	private FacilityCalendar _facilityCalendar;

	[Inject]
	private MusicService _musicService;

	private DisposableBag _disposable;

	private Subject<Unit> _onCompleteTodo = new Subject<Unit>();

	private Subject<Unit> _onCompletePomodoro = new Subject<Unit>();

	private Subject<Unit> _onCompleteImportMusic = new Subject<Unit>();

	private bool _isSetup;

	Observable<Unit> IAchievementNotice.OnEndFirstTutorial => _tutorialService.OnEndFirstTutorial;

	Observable<Unit> IAchievementNotice.OnEndNewMainStory => _facilityStory.OnEndNewMainStory;

	Observable<Unit> IAchievementNotice.OnCompleteTodo => _onCompleteTodo;

	Observable<Unit> IAchievementNotice.OnCompletePomodoro => _onCompletePomodoro;

	Observable<Unit> IAchievementNotice.OnUpdateWorkHour => _pomodoroService.OnUpdateWorkHour;

	Observable<Unit> IAchievementNotice.OnEndEditNote => _noteService.OnEndEditNote;

	Observable<Unit> IAchievementNotice.OnEndEditDiary => _facilityCalendar.OnEndEditDiaryEvent;

	Observable<Unit> IAchievementNotice.OnCompleteImportMusic => _onCompleteImportMusic;

	public void Dispose()
	{
		_disposable.Dispose();
		_onCompleteTodo.Dispose();
	}

	void IAchievementNotice.Setup()
	{
		if (!_isSetup)
		{
			_isSetup = true;
			ObservableSubscribeExtensions.Subscribe(_musicService.OnCompleteImportMusic, delegate
			{
				_onCompleteImportMusic.OnNext(Unit.Default);
			}).AddTo(ref _disposable);
			ObservableSubscribeExtensions.Subscribe(_facilityTodo.OnCompleteTodo, delegate
			{
				_onCompleteTodo.OnNext(Unit.Default);
			}).AddTo(ref _disposable);
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
			{
				_onCompletePomodoro.OnNext(Unit.Default);
			}).AddTo(ref _disposable);
		}
	}
}
