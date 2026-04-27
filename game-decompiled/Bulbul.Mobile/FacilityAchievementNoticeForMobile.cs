using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityAchievementNoticeForMobile : MonoBehaviour, IAchievementNotice
{
	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private NoteService _noteService;

	[Inject]
	private MusicService _musicService;

	[SerializeField]
	private FacilityTodoListContentsUI _facilityTodo;

	[SerializeField]
	private FacilityCalendarContentsUI _facilityCalendar;

	[SerializeField]
	private FacilityStory _facilityStory;

	private DisposableBag _disposable;

	private Subject<Unit> _onCompletePomodoro = new Subject<Unit>();

	private Subject<Unit> _onCompleteImportMusic = new Subject<Unit>();

	private bool _isSetup;

	Observable<Unit> IAchievementNotice.OnEndFirstTutorial => _tutorialService.OnEndFirstTutorial;

	Observable<Unit> IAchievementNotice.OnEndNewMainStory => _facilityStory.OnEndNewMainStory;

	Observable<Unit> IAchievementNotice.OnCompleteTodo => _facilityTodo.OnAchievementNoticeOnCompleteTodo;

	Observable<Unit> IAchievementNotice.OnCompletePomodoro => _onCompletePomodoro;

	Observable<Unit> IAchievementNotice.OnUpdateWorkHour => _pomodoroService.OnUpdateWorkHour;

	Observable<Unit> IAchievementNotice.OnEndEditNote => _noteService.OnEndEditNote;

	Observable<Unit> IAchievementNotice.OnEndEditDiary => _facilityCalendar.OnAchievementNoticeOnEndEditDiary;

	Observable<Unit> IAchievementNotice.OnCompleteImportMusic => _onCompleteImportMusic;

	public void Dispose()
	{
		_disposable.Dispose();
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
			ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
			{
				_onCompletePomodoro.OnNext(Unit.Default);
			}).AddTo(ref _disposable);
		}
	}
}
