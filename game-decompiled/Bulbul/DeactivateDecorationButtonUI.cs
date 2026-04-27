using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class DeactivateDecorationButtonUI : DecorationButtonUIBase
{
	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private DecorationDataService _decorationDataService;

	[SerializeField]
	private DecorationService.DecorationCategoryType _category;

	[SerializeField]
	private Button _changeButton;

	[SerializeField]
	private InteractableUI _interactableUI;

	public void Setup()
	{
		_interactableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_changeButton.OnClickAsObservable(), delegate
		{
			_decorationService.DeactivateAllModels(_category, isSave: true);
		}).AddTo(this);
		foreach (DecorationSkinMasterData item in _masterDataLoader.DecorationMaster.GetSkinsByCategory(_category))
		{
			DecorationService.DecorationSkinType skinType = item.SkinType;
			_decorationDataService.IsDecorationActive(skinType).Subscribe(delegate(bool isActive)
			{
				OnChangeSkinActive(skinType, isActive);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(_unlockItemService.Decoration.GetLockState(skinType).IsLocked, delegate
			{
				OnChangeSkinLocked();
			}).AddTo(this);
		}
	}

	private void OnChangeSkinActive(DecorationService.DecorationSkinType skinType, bool isActive)
	{
		if (_masterDataLoader.DecorationMaster.GetSkinsByCategory(_category).Any((DecorationSkinMasterData x) => _decorationDataService.IsDecorationActive(x.SkinType).CurrentValue))
		{
			_interactableUI.DeactivateUseUI();
		}
		else
		{
			_interactableUI.ActivateUseUI();
		}
	}

	private void OnChangeSkinLocked()
	{
		IEnumerable<DecorationSkinMasterData> skinsByCategory = _masterDataLoader.DecorationMaster.GetSkinsByCategory(_category);
		bool active = skinsByCategory.Any((DecorationSkinMasterData x) => !_unlockItemService.Decoration.GetLockState(x.SkinType).IsLocked.CurrentValue) || (skinsByCategory.All((DecorationSkinMasterData x) => _unlockItemService.Decoration.IsPurchasableType(x.SkinType, out var _)) ? true : false);
		base.gameObject.SetActive(active);
	}
}
