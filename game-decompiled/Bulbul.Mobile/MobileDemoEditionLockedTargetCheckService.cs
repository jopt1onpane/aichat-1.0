using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using FastEnumUtility;
using VContainer;

namespace Bulbul.Mobile;

public class MobileDemoEditionLockedTargetCheckService
{
	private MasterDataLoader _masterDataLoader;

	private Dictionary<DecorationService.DecorationSkinType, string> _decorationSkinTypeNameCaches;

	private Dictionary<DecorationService.DecorationCategoryType, (DecorationService.DecorationModelType, DecorationService.DecorationSkinType)> _decorationCategoryDefaultSkinCaches;

	private Dictionary<EnvironmentType, string> _environmentTypeNameCaches;

	[Inject]
	public MobileDemoEditionLockedTargetCheckService(MasterDataLoader masterDataLoader)
	{
		_masterDataLoader = masterDataLoader;
		_decorationSkinTypeNameCaches = new Dictionary<DecorationService.DecorationSkinType, string>();
		_environmentTypeNameCaches = new Dictionary<EnvironmentType, string>();
		_decorationCategoryDefaultSkinCaches = new Dictionary<DecorationService.DecorationCategoryType, (DecorationService.DecorationModelType, DecorationService.DecorationSkinType)>();
		foreach (Member<DecorationService.DecorationSkinType> member in FastEnum.GetMembers<DecorationService.DecorationSkinType>())
		{
			_decorationSkinTypeNameCaches.Add(member.Value, member.Value.ToString());
		}
		foreach (Member<DecorationService.DecorationCategoryType> member2 in FastEnum.GetMembers<DecorationService.DecorationCategoryType>())
		{
			if (TrySearchDecorationDefaultSkin(member2.Value, out var decorationModelType, out var defaultSkinType))
			{
				_decorationCategoryDefaultSkinCaches.Add(member2.Value, (decorationModelType, defaultSkinType));
			}
		}
		foreach (Member<EnvironmentType> member3 in FastEnum.GetMembers<EnvironmentType>())
		{
			_environmentTypeNameCaches.Add(member3.Value, member3.Value.ToString());
		}
	}

	private bool TrySearchDecorationDefaultSkin(DecorationService.DecorationCategoryType categoryType, out DecorationService.DecorationModelType decorationModelType, out DecorationService.DecorationSkinType defaultSkinType)
	{
		defaultSkinType = DecorationService.DecorationSkinType.MugCup_1;
		decorationModelType = DecorationService.DecorationModelType.Cup_1;
		foreach (DecorationSkinMasterData item in _masterDataLoader.DecorationMaster.GetSkinsByCategory(categoryType))
		{
			if (item.IsCategoryDefault)
			{
				defaultSkinType = item.SkinType;
				decorationModelType = item.ModelType;
				return true;
			}
		}
		return false;
	}

	public bool CheckDecorationSkinLocked(DecorationService.DecorationSkinType skinType)
	{
		return CheckDecorationSkinLocked(_masterDataLoader.UnlockDecorationMasterList, GetDecorationSkinTypeName(skinType));
	}

	private bool CheckDecorationSkinLocked(IReadOnlyList<UnlockDecorationData> masters, string skinType)
	{
		return masters.FirstOrDefault((UnlockDecorationData d) => d.ItemType == skinType)?.IsMobileDemoUserLockedTarget ?? false;
	}

	public bool CheckEnvironmentLocked(EnvironmentType environmentType)
	{
		return CheckEnvironmentLocked(_masterDataLoader.UnlockEnvironmentMasterList, GetEnvironmentTypeName(environmentType));
	}

	private bool CheckEnvironmentLocked(IReadOnlyList<UnlockEnvironmentData> masters, string environmentType)
	{
		return masters.FirstOrDefault((UnlockEnvironmentData d) => d.ItemType == environmentType)?.IsMobileDemoUserLockedTarget ?? false;
	}

	private string GetDecorationSkinTypeName(DecorationService.DecorationSkinType skinType)
	{
		if (_decorationSkinTypeNameCaches.TryGetValue(skinType, out var value))
		{
			return value;
		}
		return skinType.ToString();
	}

	private string GetEnvironmentTypeName(EnvironmentType environmentType)
	{
		if (_environmentTypeNameCaches.TryGetValue(environmentType, out var value))
		{
			return value;
		}
		return environmentType.ToString();
	}

	public (DecorationService.DecorationModelType?, DecorationService.DecorationSkinType?) GetDecorationCategoryDefault(DecorationService.DecorationModelType decorationModelType)
	{
		DecorationCategoryMasterData categoryByModel = _masterDataLoader.DecorationMaster.GetCategoryByModel(decorationModelType);
		if (_decorationCategoryDefaultSkinCaches.TryGetValue(categoryByModel.CategoryType, out var value))
		{
			(DecorationService.DecorationModelType, DecorationService.DecorationSkinType) tuple = value;
			return (tuple.Item1, tuple.Item2);
		}
		return (null, null);
	}
}
