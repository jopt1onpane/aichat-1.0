using System.Threading;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using R3;

public interface ISpecialService
{
	ReadOnlyReactiveProperty<bool> IsActive { get; }

	void SetEnableNewIconMark();

	void Setup();

	void CreateList();

	void ActivateList();

	void DeactivateList();

	bool IsPossibleChangeSpecial();

	bool IsPossibleReadNextSpecialEpisodeNumber();

	int GetNextEpisodeNumber(ScenarioType scenarioType);

	LevelData GetCurrentLevelData();

	float AdjustUpperExp(float exp);

	ScenarioType GetNextLongTalkScenarioType();

	void OnEndStory(ScenarioType scenarioType, int episodeNumber);

	bool IsNeedUsePossibleAnnounce();

	void PlayPossibleAnnounce();

	UniTask PlaySpecialModeStartAnnounce(SpecialService.CollaborationType specialType, CancellationToken ct);

	bool IsNeedUseFinishAnnounce();

	void PlayFinishAnnounce();
}
