using Bulbul;
using Bulbul.MasterData;
using R3;

public class SpecialSelectCellAlterEgo : SpecialSelectCell
{
	public override void Setup(SpecialService.CollaborationType specialType, Observable<ScenarioType> onEndStory)
	{
		_episodeCountMax = 2;
		_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LastReadEpisodeNumber;
		base.Setup(specialType, onEndStory);
		onEndStory.Subscribe(delegate(ScenarioType scenarioType)
		{
			if (scenarioType == ScenarioType.Special_AlterEgo)
			{
				_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LastReadEpisodeNumber;
			}
		}).AddTo(this);
	}
}
