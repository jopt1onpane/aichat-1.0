using R3;

namespace Bulbul;

public interface IPlayerLevelUIService
{
	Observable<LevelData> OnEndShowExpValue { get; }

	Observable<LevelData> OnUpdateUILevelData { get; }

	Observable<LevelData> OnEndAddExp { get; }

	Observable<LevelData> OnReadyLevelUP { get; }

	Observable<Unit> OnStartLevelUP { get; }

	Observable<LevelData> OnEndLevelUP { get; }

	Observable<float> OnAddNotAddedYetShowExp { get; }

	string CurrentShowLevel { get; }

	float NotAddedYetShowExp { get; }

	void Setup();

	void UpdateUI();

	bool IsAccumulatedLevelUpExp();

	void StartLevelUpDirection();

	void EndLevelUpDirection();

	bool IsEndLevelUpDirection();

	void OnAddExp(float exp);

	void FocusUIActivate();

	void FocusUIDeactivate();

	void SyncWithSaveData();
}
