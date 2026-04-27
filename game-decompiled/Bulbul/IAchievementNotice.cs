using R3;

namespace Bulbul;

public interface IAchievementNotice
{
	Observable<Unit> OnEndFirstTutorial { get; }

	Observable<Unit> OnEndNewMainStory { get; }

	Observable<Unit> OnCompleteTodo { get; }

	Observable<Unit> OnCompletePomodoro { get; }

	Observable<Unit> OnUpdateWorkHour { get; }

	Observable<Unit> OnEndEditNote { get; }

	Observable<Unit> OnEndEditDiary { get; }

	Observable<Unit> OnCompleteImportMusic { get; }

	void Setup();
}
