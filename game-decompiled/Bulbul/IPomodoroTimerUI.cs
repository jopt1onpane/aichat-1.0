using R3;

namespace Bulbul;

public interface IPomodoroTimerUI
{
	Observable<Unit> OnClickButtonIncrementWorkTime { get; }

	Observable<Unit> OnClickButtonDecrementWorkTime { get; }

	Observable<int> OnEndEditWorkTimeMaxMinutesText { get; }

	Observable<Unit> OnClickButtonIncrementBreakTime { get; }

	Observable<Unit> OnClickButtonDecrementBreakTime { get; }

	Observable<int> OnEndEditBreakTimeMaxMinutesText { get; }

	Observable<Unit> OnClickButtonIncrementLoopCount { get; }

	Observable<Unit> OnClickButtonDecrementLoopCount { get; }

	Observable<int> OnEndEditLoopCountText { get; }

	Observable<Unit> OnClickButtonReset { get; }

	Observable<Unit> OnClickButtonStartPomodoro { get; }

	Observable<Unit> OnClickButtonPlayOrPausePomodoro { get; }

	Observable<Unit> OnClickButtonSkipPomodoro { get; }

	void Setup();
}
