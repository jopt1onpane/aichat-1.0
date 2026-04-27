using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "DecorationMaster", menuName = "ScriptableObject/DecorationMaster")]
public class DecorationMaster : ScriptableObject
{
	[SerializeField]
	public DecorationCategoryMasterData[] DecorationCategories;

	[SerializeField]
	public DecorationModelMasterData[] DecorationModels;

	[SerializeField]
	public DecorationSkinMasterData[] DecorationSkins;

	public DecorationCategoryMasterData GetCategory(DecorationService.DecorationCategoryType decorationCategoryType)
	{
		return DecorationCategories.First((DecorationCategoryMasterData x) => x.CategoryType == decorationCategoryType);
	}

	public IReadOnlyList<DecorationCategoryMasterData> GetAllCategories()
	{
		return DecorationCategories;
	}

	public DecorationModelMasterData GetModel(DecorationService.DecorationModelType decorationModelType)
	{
		return DecorationModels.First((DecorationModelMasterData x) => x.ModelType == decorationModelType);
	}

	public IEnumerable<DecorationService.DecorationModelType> GetModelTypesByCategory(DecorationService.DecorationCategoryType decorationCategoryType)
	{
		return from x in DecorationModels
			where x.CategoryType == decorationCategoryType
			select x.ModelType;
	}

	public DecorationSkinMasterData GetSkin(DecorationService.DecorationSkinType decorationSkinType)
	{
		return DecorationSkins.First((DecorationSkinMasterData x) => x.SkinType == decorationSkinType);
	}

	public IEnumerable<DecorationSkinMasterData> GetSkinsByModel(DecorationService.DecorationModelType decorationModelType)
	{
		return DecorationSkins.Where((DecorationSkinMasterData x) => x.ModelType == decorationModelType);
	}

	public IEnumerable<DecorationSkinMasterData> GetSkinsByCategory(DecorationService.DecorationCategoryType decorationCategoryType)
	{
		return DecorationSkins.Where((DecorationSkinMasterData x) => GetModelBySkin(x.SkinType).CategoryType == decorationCategoryType);
	}

	public DecorationCategoryMasterData GetCategoryByModel(DecorationService.DecorationModelType modelType)
	{
		DecorationModelMasterData model = GetModel(modelType);
		return DecorationCategories.First((DecorationCategoryMasterData x) => x.CategoryType == model.CategoryType);
	}

	public DecorationCategoryMasterData GetCategoryBySkin(DecorationService.DecorationSkinType skinType)
	{
		DecorationSkinMasterData skin = GetSkin(skinType);
		return GetCategoryByModel(skin.ModelType);
	}

	public DecorationModelMasterData GetModelBySkin(DecorationService.DecorationSkinType skinType)
	{
		DecorationSkinMasterData skin = GetSkin(skinType);
		return DecorationModels.First((DecorationModelMasterData x) => x.ModelType == skin.ModelType);
	}
}
