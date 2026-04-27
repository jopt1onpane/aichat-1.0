using System;

namespace Bulbul;

public interface IPomodoroTimerStateView
{
	bool IsTransitioning { get; }

	void Setup();

	void SwitchStartTimer();

	void SwitchPlayTimer(PomodoroService.PomodoroType pomodoroType, Action<PomodoroService.PomodoroType> completedUsingTimerCanvasFade);

	void SwitchCompleteTimer(Action completedUsingTimerCanvasFade);

	void SwitchPauseTimer();

	void SwitchUnPauseTimer(PomodoroService.PomodoroType pomodoroType);

	void ActivateLockTimer();

	void DeactivateLockTimer();
}
