using Bulbul;
using Bulbul.MasterData;
using R3;

public class SpecialSelectCellBearsRestaurant : SpecialSelectCell
{
	public override void Setup(SpecialService.CollaborationType specialType, Observable<ScenarioType> onEndStory)
	{
		_episodeCountMax = 2;
		_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LastReadEpisodeNumber;
		base.Setup(specialType, onEndStory);
		onEndStory.Subscribe(delegate(ScenarioType scenarioType)
		{
			if (scenarioType == ScenarioType.Special_BearsRestaurant)
			{
				_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LastReadEpisodeNumber;
			}
		}).AddTo(this);
	}
}
