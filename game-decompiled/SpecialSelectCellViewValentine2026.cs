using Bulbul;
using VContainer;

public class SpecialSelectCellViewValentine2026 : SpecialSelectCellView
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
		return SpecialService.CollaborationType.Valentine2026;
	}

	private LevelData GetLevelData()
	{
		return SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData;
	}

	private int GetMaxLevel()
	{
		return 2;
	}
}
