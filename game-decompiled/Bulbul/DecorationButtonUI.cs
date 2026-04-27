using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FastEnumUtility;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class DecorationButtonUI : DecorationButtonUIBase
{
	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private DecorationDataService _decorationDataService;

	[SerializeField]
	private DecorationService.DecorationModelType _model;

	[SerializeField]
	private Button _changeButton;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private RectTransform _newItemIcon;

	[SerializeField]
	private int _purchasePopoverParentHeight = 35;

	[SerializeField]
	[Header("一時的に使用できない場合のロックUI")]
	private LockUI _lockUI;

	[SerializeField]
	[Header("ロック時のツールチップ")]
	private TooltipTarget _tooltipTarget;

	private readonly ReactiveProperty<bool> _isNewIconActive = new ReactiveProperty<bool>();

	private readonly Subject<Unit> _onShowSelector = new Subject<Unit>();

	private ItemSkinSelectorShowAnim _skinSelectorAnim;

	private PurchasePopover _purchasePopover;

	public ReadOnlyReactiveProperty<bool> IsNewIconActive => _isNewIconActive;

	public Observable<Unit> OnShowSelector => _onShowSelector;

	public DecorationService.DecorationModelType ModelType => _model;

	public void Setup(ItemSkinSelectorShowAnim skinSelectorAnim, PurchasePopover purchasePopover)
	{
		_skinSelectorAnim = skinSelectorAnim;
		_purchasePopover = purchasePopover;
		_newItemIcon.gameObject.SetActive(value: false);
		_interactableUI.Setup();
		_lockUI.Setup();
		_tooltipTarget.gameObject.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(_changeButton.OnClickAsObservable(), delegate
		{
			if (_skinSelectorAnim.IsShowing && _skinSelectorAnim.SelectorUI.Target == _model.ToName())
			{
				HideSelector();
			}
			else
			{
				ShowSkinSelector();
			}
		}).AddTo(this);
		foreach (DecorationSkinMasterData item in _masterDataLoader.DecorationMaster.GetSkinsByModel(_model))
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
		if (IsNeedObserveLimitedTimeEventLock())
		{
			ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType, delegate
			{
				UpdateLockUIState();
			}).AddTo(this);
			UpdateLockUIState();
		}
	}

	public void HideSelector()
	{
		_skinSelectorAnim.Hide();
	}

	private void ShowSkinSelector()
	{
		if (IsTemporarilyLocked())
		{
			return;
		}
		_masterDataLoader.DecorationMaster.GetModel(_model);
		List<DecorationSkinMasterData> list = _masterDataLoader.DecorationMaster.GetSkinsByModel(_model).ToList();
		if (list.Count == 1)
		{
			HideSelector();
			DecorationSkinMasterData decorationSkinMasterData = list[0];
			_decorationService.ChangeDecoration(decorationSkinMasterData.SkinType, isSave: true);
			return;
		}
		ItemSkinSelectorUI selectorUI = _skinSelectorAnim.SelectorUI;
		_onShowSelector.OnNext(Unit.Default);
		if (list.Count == 0)
		{
			Debug.LogWarning($"DecorationButtonUI: skins.Count == 0. model: {_model}");
			_skinSelectorAnim.Hide();
			selectorUI.gameObject.SetActive(value: false);
			return;
		}
		_skinSelectorAnim.Show(_model);
		CompositeDisposable showDisposables = new CompositeDisposable();
		selectorUI.SetData(_model.ToName(), list, delegate(ItemSkinButton button, DecorationSkinMasterData decoration)
		{
			DecorationService.DecorationSkinType skinType = decoration.SkinType;
			ReactiveProperty<bool> isLocked = _unlockItemService.Decoration.GetLockState(skinType).IsLocked;
			ReadOnlyReactiveProperty<bool> source = _decorationDataService.IsDecorationActive(skinType);
			button.SetColors(decoration);
			isLocked.Subscribe(delegate(bool locked)
			{
				int price;
				ItemSkinButton.DisplayType displayType = (locked ? ((!_unlockItemService.Decoration.IsPurchasableType(skinType, out price)) ? ItemSkinButton.DisplayType.Locked : ItemSkinButton.DisplayType.Purchase) : ItemSkinButton.DisplayType.Open);
				button.SetDisplayType(displayType);
				button.SetNew(IsNewSkin(skinType));
			}).AddTo(showDisposables);
			button.SetOnClick(delegate
			{
				if (_unlockItemService.Decoration.IsPurchasableType(skinType, out var price))
				{
					_purchasePopover.Show(decoration.Thumbnail, price, _unlockItemService.Decoration.CanPurchase(skinType), () => Purchase(skinType), button.transform, _purchasePopoverParentHeight, new Vector3(0f, -34.5f));
					UniTask.Void(async delegate
					{
						using (_skinSelectorAnim.DisableCloseOnClickOutsideScope())
						{
							await _purchasePopover.WaitHide();
						}
					});
				}
				else
				{
					_decorationService.ChangeDecoration(skinType, isSave: true);
				}
			});
			source.Subscribe(delegate(bool isUsing)
			{
				button.SetUsing(isUsing);
				button.SetNew(IsNewSkin(skinType));
			}).AddTo(showDisposables);
		}, showDisposables);
	}

	private bool Purchase(DecorationService.DecorationSkinType decorationType)
	{
		bool num = _unlockItemService.Decoration.Purchase(decorationType);
		if (num)
		{
			_purchasePopover.HideWithPurchaseAnim();
			_systemSeService.PlayBuyItem();
			_decorationService.ChangeDecoration(decorationType, isSave: true);
		}
		return num;
	}

	private void OnChangeSkinActive(DecorationService.DecorationSkinType skinType, bool isActive)
	{
		if (isActive)
		{
			string decorationName = GetDecorationName(skinType);
			if (!SaveDataManager.Instance.DecorationProgressData.PlayedDecoration.Contains(decorationName))
			{
				SaveDataManager.Instance.DecorationProgressData.PlayedDecoration.Add(decorationName);
				SaveDataManager.Instance.SaveDecorationProgressData();
			}
		}
		if (_masterDataLoader.DecorationMaster.GetSkinsByModel(_model).Any((DecorationSkinMasterData x) => _decorationDataService.IsDecorationActive(x.SkinType).CurrentValue))
		{
			_interactableUI.ActivateUseUI();
			UpdateNew();
		}
		else
		{
			_interactableUI.DeactivateUseUI();
		}
	}

	private void OnChangeSkinLocked()
	{
		IEnumerable<DecorationSkinMasterData> skinsByModel = _masterDataLoader.DecorationMaster.GetSkinsByModel(_model);
		if (skinsByModel.Any((DecorationSkinMasterData x) => !_unlockItemService.Decoration.GetLockState(x.SkinType).IsLocked.CurrentValue) || (skinsByModel.All((DecorationSkinMasterData x) => _unlockItemService.Decoration.IsPurchasableType(x.SkinType, out var _)) ? true : false))
		{
			base.gameObject.SetActive(value: true);
			UpdateNew();
		}
		else
		{
			base.gameObject.SetActive(value: false);
			_newItemIcon.gameObject.SetActive(value: false);
		}
	}

	private string GetDecorationName(DecorationService.DecorationSkinType type)
	{
		return type.ToString();
	}

	private void UpdateNew()
	{
		if (_masterDataLoader.DecorationMaster.GetSkinsByModel(_model).Any((DecorationSkinMasterData x) => IsNewSkin(x.SkinType)))
		{
			ActivateNewIcon();
		}
		else
		{
			DeactivateNewIcon();
		}
	}

	private bool IsNewSkin(DecorationService.DecorationSkinType skinType)
	{
		UnlockDecoration.IDecorationUnlockData lockState = _unlockItemService.Decoration.GetLockState(skinType);
		if (lockState.IsNotLockCondition)
		{
			return false;
		}
		if (lockState.IsLocked.CurrentValue)
		{
			return false;
		}
		return !SaveDataManager.Instance.DecorationProgressData.PlayedDecoration.Contains(GetDecorationName(skinType));
	}

	private void ActivateNewIcon()
	{
		_newItemIcon.gameObject.SetActive(value: true);
		_isNewIconActive.Value = true;
	}

	private void DeactivateNewIcon()
	{
		_newItemIcon.gameObject.SetActive(value: false);
		_isNewIconActive.Value = false;
	}

	private bool IsTemporarilyLocked()
	{
		LimitedTimeEventSaveData limitedTimeEventSaveData = SaveDataManager.Instance.LimitedTimeEventSaveData;
		if (_model == DecorationService.DecorationModelType.Headphone_2)
		{
			return limitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.Christmas2025;
		}
		if (IsGlassesModel(_model))
		{
			return limitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.AprilFool2026;
		}
		return false;
	}

	private bool IsNeedObserveLimitedTimeEventLock()
	{
		if (_model != DecorationService.DecorationModelType.Headphone_2)
		{
			return IsGlassesModel(_model);
		}
		return true;
	}

	private bool IsGlassesModel(DecorationService.DecorationModelType model)
	{
		if (model != DecorationService.DecorationModelType.Glasses_1 && model != DecorationService.DecorationModelType.Glasses_2 && model != DecorationService.DecorationModelType.Glasses_3)
		{
			return model == DecorationService.DecorationModelType.Glasses_None;
		}
		return true;
	}

	private void UpdateLockUIState()
	{
		if (!(_lockUI == null))
		{
			if (IsTemporarilyLocked())
			{
				_lockUI.Activate();
				_changeButton.interactable = false;
				_tooltipTarget.gameObject.SetActive(value: true);
			}
			else
			{
				_lockUI.Deactivate();
				_changeButton.interactable = true;
				_tooltipTarget.gameObject.SetActive(value: false);
			}
		}
	}
}
