using System;
using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class DecorationSkinListViewForMobile : OSA<DecorationSkinParams, DecorationSkinListItemViewForMobileHolder>
{
	[SerializeField]
	private SimpleNoticeDialog _mobileDemoEditionLockedNotice;

	private OSAListDataHelper<DecorationSkinListItemModelForMobile> _data;

	private DecorationSkinListItemModelForMobile[] _requestedSetupData;

	private Subject<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)> _onSelectedDecoration = new Subject<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)>();

	private Subject<(DecorationService.DecorationSkinType, int)> _onSelectedShopItem = new Subject<(DecorationService.DecorationSkinType, int)>();

	private bool isPendingUpdateVisible;

	public Observable<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)> OnSelectedDecoration => _onSelectedDecoration;

	public Observable<(DecorationService.DecorationSkinType, int)> OnSelectedShopItem => _onSelectedShopItem;

	public Func<int> CurrentPlayerPointGetter { get; set; }

	protected override void Start()
	{
		_data = new OSAListDataHelper<DecorationSkinListItemModelForMobile>(this);
		_Params.ItemPrefab.gameObject.SetActive(value: false);
		base.Start();
		if (_requestedSetupData != null)
		{
			SetupModel(_requestedSetupData);
			_requestedSetupData = null;
		}
	}

	protected override void Update()
	{
		if (base.IsInitialized)
		{
			base.Update();
			if (isPendingUpdateVisible)
			{
				UpdateViewVisible();
				isPendingUpdateVisible = false;
			}
		}
	}

	public void SetupModel(DecorationSkinListItemModelForMobile[] skinModels)
	{
		if (!base.IsInitialized)
		{
			_requestedSetupData = skinModels;
			return;
		}
		if (skinModels != null)
		{
			_data.ResetItems(skinModels);
			return;
		}
		_data.List.Clear();
		_data.NotifyListChangedExternally();
	}

	protected override DecorationSkinListItemViewForMobileHolder CreateViewsHolder(int itemIndex)
	{
		DecorationSkinListItemViewForMobileHolder decorationSkinListItemViewForMobileHolder = new DecorationSkinListItemViewForMobileHolder();
		decorationSkinListItemViewForMobileHolder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		decorationSkinListItemViewForMobileHolder.View.OnClickSelectButton.Subscribe(delegate((DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType) skinType)
		{
			_onSelectedDecoration.OnNext(skinType);
		}).AddTo(this);
		decorationSkinListItemViewForMobileHolder.View.OnClickShopButton.Subscribe(delegate((DecorationService.DecorationSkinType, int) ShopData)
		{
			_onSelectedShopItem.OnNext(ShopData);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(decorationSkinListItemViewForMobileHolder.View.OnClickMobileDemoEditionLocked, delegate
		{
			_mobileDemoEditionLockedNotice.Activate();
		}).AddTo(this);
		decorationSkinListItemViewForMobileHolder.View.CurrentPlayerPointGetter = CurrentPlayerPointGetter;
		return decorationSkinListItemViewForMobileHolder;
	}

	protected override void UpdateViewsHolder(DecorationSkinListItemViewForMobileHolder item)
	{
		DecorationSkinListItemModelForMobile model = _data[item.ItemIndex];
		item.UpdateView(model);
	}

	public void ChangePurchased(DecorationService.DecorationSkinType type, bool isOnlyDataUpdate = true)
	{
		DecorationSkinListItemModelForMobile decorationSkinListItemModelForMobile = _data.FirstOrDefault((DecorationSkinListItemModelForMobile x) => x.skinType == type);
		if (decorationSkinListItemModelForMobile != null)
		{
			decorationSkinListItemModelForMobile.IsPurchased = true;
			if (isOnlyDataUpdate)
			{
				UpdateViewVisible();
			}
		}
	}

	public void ChangeSelected(DecorationService.DecorationSkinType type)
	{
		foreach (DecorationSkinListItemModelForMobile item in (IEnumerable<DecorationSkinListItemModelForMobile>)_data)
		{
			if (item.skinType == type && item.DeactivationType != DecorationSkinListItemModelForMobile.DeactivationCategoryType.Other)
			{
				item.IsSelected = true;
				item.IsNew = false;
			}
			else
			{
				item.IsSelected = false;
			}
		}
		UpdateViewVisible();
	}

	public void SetUnselectedAllModel()
	{
		foreach (DecorationSkinListItemModelForMobile item in (IEnumerable<DecorationSkinListItemModelForMobile>)_data)
		{
			if (item.DeactivationType != DecorationSkinListItemModelForMobile.DeactivationCategoryType.Other)
			{
				item.IsSelected = false;
				continue;
			}
			item.IsSelected = true;
			item.IsNew = false;
		}
		UpdateViewVisible();
	}

	public void UpdateViewVisible()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			isPendingUpdateVisible = true;
			return;
		}
		foreach (DecorationSkinListItemViewForMobileHolder visibleItem in _VisibleItems)
		{
			visibleItem.UpdateView(_data[visibleItem.ItemIndex]);
		}
	}
}
