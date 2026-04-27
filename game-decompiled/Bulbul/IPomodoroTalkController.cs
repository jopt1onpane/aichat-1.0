namespace Bulbul;

public interface IPomodoroTalkController
{
	bool IsTalkStartReady { get; }

	bool IsTalkPlayEnd { get; }

	bool IsCurrentWorking { get; }

	bool IsCurrentResting { get; }

	void StartTalk();

	void EndTalk();
}
