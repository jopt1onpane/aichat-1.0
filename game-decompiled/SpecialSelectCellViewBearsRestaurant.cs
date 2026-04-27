using Bulbul;
using VContainer;

public class SpecialSelectCellViewBearsRestaurant : SpecialSelectCellView
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	protected override float GetCurrentProgress()
	{
		if (_masterDataLoader == null)
		{
			_masterDataLoader = RoomLifetimeScope.Resolve<MasterDataLoader>();
		}
		return LevelData.CalculateTotalExp(_masterDataLoader, GetSpecialType());
	}

	protected override float GetMaxProgress()
	{
		if (_masterDataLoader == null)
		{
			_masterDataLoader = RoomLifetimeScope.Resolve<MasterDataLoader>();
		}
		return GetLevelData().CalculateTargetLevelNecessaryExp(_masterDataLoader, GetMaxLevel(), 1, isSubtractCurrentExp: false, GetSpecialType());
	}

	private SpecialService.CollaborationType GetSpecialType()
	{
		return SpecialService.CollaborationType.BearsRestaurant;
	}

	private LevelData GetLevelData()
	{
		return SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData;
	}

	private int GetMaxLevel()
	{
		return 2;
	}
}
