namespace Bulbul;

public interface IGameDemoView
{
	bool IsPlayEndCanvasActiveSelf { get; }

	bool IsTimeLimitObjectActiveSelf { get; }

	void Setup(GameDemoService gameDemoService);

	void ActivateTimeLimit();

	void DeactivateTimeLimit();

	void ActivateThankyouForPlaying();

	void DeactivateThankyouForPlaying();
}
