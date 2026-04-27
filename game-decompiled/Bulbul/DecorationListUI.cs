using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class DecorationListUI : MonoBehaviour, IDecorationListUI
{
	[Inject]
	private ChangeOrderService _changeOrderService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private IOnClickButtonAllUIDeactivateProvider _onClickButtonAllUIDeactivateProvider;

	[SerializeField]
	private SerializedDictionary<DecorationService.DecorationCategoryType, TextLocalizationBehaviour> _titleTexts;

	[SerializeField]
	private SerializedDictionary<DecorationService.DecorationCategoryType, ItemSkinSelectorShowAnim> _skinSelectorAnims;

	[SerializeField]
	private PurchasePopover _purchasePopover;

	[SerializeField]
	private PlayerPointView _ownPointView;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("新規アイコン")]
	private NewDecorationMarkUI _newIcon;

	[SerializeField]
	private GameObject _parentObj;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("機能を閉じるボタン")]
	private Button _closeButton;

	[SerializeField]
	private Button _hideButton;

	private const string LockTitleLocalizeID = "ui_lock_title";

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public void Setup()
	{
		_parentObj.SetActive(value: false);
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_newIcon.Setup();
		ObservableSubscribeExtensions.Subscribe(_hideButton.OnClickAsObservable(), delegate
		{
			_onClickButtonAllUIDeactivateProvider.OnClickButtonAllUIDeactivate();
		}).AddTo(this);
		_purchasePopover.Setup(_scrollRect);
		foreach (ItemSkinSelectorShowAnim value3 in _skinSelectorAnims.Values)
		{
			value3.Initialize();
			value3.SelectorUI.Initialize();
		}
		DecorationSkinMasterData[] decorationSkins = _masterDataLoader.DecorationMaster.DecorationSkins;
		foreach (DecorationSkinMasterData obj in decorationSkins)
		{
			DecorationService.DecorationSkinType skinType = obj.SkinType;
			UnlockDecoration.IDecorationUnlockData lockState = _unlockItemService.Decoration.GetLockState(skinType);
			DecorationCategoryMasterData category = _masterDataLoader.DecorationMaster.GetCategoryBySkin(skinType);
			ObservableSubscribeExtensions.Subscribe(lockState.IsLocked, delegate
			{
				if (_titleTexts.TryGetValue(category.CategoryType, out var value2))
				{
					bool flag2 = IsCategoryKnown(category.CategoryType);
					value2.Set(flag2 ? category.CategoryNameLocalizeID : "ui_lock_title");
					value2.Text.color = GetTextColor(!flag2);
				}
			}).AddTo(this);
		}
		foreach (DecorationCategoryMasterData allCategory in _masterDataLoader.DecorationMaster.GetAllCategories())
		{
			if (_titleTexts.TryGetValue(allCategory.CategoryType, out var value))
			{
				bool flag = IsCategoryKnown(allCategory.CategoryType);
				value.Set(flag ? allCategory.CategoryNameLocalizeID : "ui_lock_title");
				value.Text.color = GetTextColor(!flag);
			}
		}
		DecorationButtonUI[] componentsInChildren = GetComponentsInChildren<DecorationButtonUI>();
		foreach (DecorationButtonUI obj2 in componentsInChildren)
		{
			DecorationService.DecorationModelType modelType = obj2.ModelType;
			DecorationService.DecorationCategoryType categoryType = _masterDataLoader.DecorationMaster.GetCategoryByModel(modelType).CategoryType;
			ItemSkinSelectorShowAnim selectorAnim = _skinSelectorAnims[categoryType];
			obj2.Setup(selectorAnim, _purchasePopover);
			ObservableSubscribeExtensions.Subscribe(obj2.OnShowSelector, delegate
			{
				foreach (ItemSkinSelectorShowAnim value4 in _skinSelectorAnims.Values)
				{
					if (!(value4 == selectorAnim))
					{
						value4.Hide();
					}
				}
			}).AddTo(this);
		}
		DeactivateDecorationButtonUI[] componentsInChildren2 = GetComponentsInChildren<DeactivateDecorationButtonUI>();
		for (int num = 0; num < componentsInChildren2.Length; num++)
		{
			componentsInChildren2[num].Setup();
		}
		_ownPointView.SetPoint(_playerPointService.Point, withAnimation: false);
		ObservableSubscribeExtensions.Subscribe(_playerPointService.OnPointChange, delegate
		{
			_ownPointView.SetPoint(_playerPointService.Point, withAnimation: true);
		}).AddTo(this);
	}

	private Color32 GetTextColor(bool isLock)
	{
		if (!isLock)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		return new Color32(128, 128, 128, 128);
	}

	public void Activate()
	{
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Decoration);
		_parentObj.SetActive(value: true);
		_facilityOpenButton.ActivateUseUI();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.verticalNormalizedPosition = 1f;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_parentObj.SetActive(value: false);
		});
	}

	private bool IsCategoryKnown(DecorationService.DecorationCategoryType category)
	{
		return IsCategoryKnown(category, _masterDataLoader, _unlockItemService);
	}

	public static bool IsCategoryKnown(DecorationService.DecorationCategoryType category, MasterDataLoader masterDataLoader, UnlockItemService unlockItemService)
	{
		IEnumerable<DecorationSkinMasterData> skinsByCategory = masterDataLoader.DecorationMaster.GetSkinsByCategory(category);
		if (skinsByCategory.Any((DecorationSkinMasterData x) => IsSkinUnlocked(x.SkinType)))
		{
			return true;
		}
		if (skinsByCategory.All((DecorationSkinMasterData x) => IsPurchasableType(x.SkinType)))
		{
			return true;
		}
		return false;
		bool IsPurchasableType(DecorationService.DecorationSkinType skinType)
		{
			int price;
			return unlockItemService.Decoration.IsPurchasableType(skinType, out price);
		}
		bool IsSkinUnlocked(DecorationService.DecorationSkinType skinType)
		{
			return !unlockItemService.Decoration.GetLockState(skinType).IsLocked.CurrentValue;
		}
	}
}
