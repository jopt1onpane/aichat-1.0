namespace Bulbul;

public interface ILevelUpDirectionController
{
	bool IsReadyStartLevelUpDirection { get; }

	bool IsEndLevelUpDirection { get; }

	void StartLevelUpDirection();

	void EndLevelUpDirection();
}
