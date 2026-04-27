using R3;
using TMPro;
using UnityEngine;

namespace Bulbul.Mobile;

public class DecorationListViewForMobile : MonoBehaviour
{
	[SerializeField]
	private DecorationCategoryTabListViewForMobile _tabListView;

	[SerializeField]
	private DecorationModelButtonsViewForMobile _modelButtonsView;

	[SerializeField]
	private DecorationSkinListViewForMobile _skinListView;

	[SerializeField]
	private DecorationPurchaseDialog _purchaseDialog;

	[SerializeField]
	private TextMeshProUGUI _lockedCategoryListText;

	private Subject<DecorationService.DecorationSkinType> _onPurchasedDecoration = new Subject<DecorationService.DecorationSkinType>();

	public DecorationSkinListViewForMobile SkinListView => _skinListView;

	public Observable<(DecorationService.DecorationCategoryType, bool)> OnChangedTab => _tabListView.OnChangedTab;

	public Observable<DecorationService.DecorationModelType> OnChangedDecorationModelType => _modelButtonsView.OnChangedDecorationModelType;

	public Observable<DecorationService.DecorationSkinType> OnPurchasedDecoration => _onPurchasedDecoration;

	public void Setup()
	{
		_tabListView.Setup();
		_modelButtonsView.Setup();
		_purchaseDialog.Initialize();
		_skinListView.OnSelectedShopItem.Subscribe(delegate((DecorationService.DecorationSkinType, int) shopData)
		{
			_purchaseDialog.Open(shopData.Item1);
		}).AddTo(this);
		_purchaseDialog.OnPurchased.Subscribe(delegate(DecorationService.DecorationSkinType skinType)
		{
			_onPurchasedDecoration.OnNext(skinType);
		}).AddTo(this);
	}

	public void SetCategoryKnown(DecorationService.DecorationCategoryType category, bool isKnown)
	{
		if (isKnown)
		{
			_tabListView.SetTabDefault(category);
		}
		else
		{
			_tabListView.SetTabUnReleased(category);
		}
	}

	public void SetCategory(DecorationService.DecorationCategoryType categoryType)
	{
		_tabListView.SetTab(categoryType);
	}

	public void SetModelTypes(DecorationService.DecorationCategoryType categoryType, DecorationService.DecorationModelType[] modelTypes, bool isSelectedFirst = true)
	{
		_modelButtonsView.Set(categoryType, modelTypes);
		if (isSelectedFirst)
		{
			_modelButtonsView.SelectButton(0, isForce: true);
		}
	}

	public void SetSkins(DecorationSkinListItemModelForMobile[] skinModels)
	{
		_skinListView.SetupModel(skinModels);
	}

	public void HideAllModelButtons()
	{
		_modelButtonsView.HideAllButtons();
	}

	public void SetActiveLockedCategoryText(bool active)
	{
		_lockedCategoryListText.enabled = active;
	}

	public bool GetIsUnlockedCategory(DecorationService.DecorationCategoryType categoryType)
	{
		return _tabListView.GetIsUnlockTab(categoryType);
	}
}
