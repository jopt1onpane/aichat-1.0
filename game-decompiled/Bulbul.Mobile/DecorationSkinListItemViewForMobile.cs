using System;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class DecorationSkinListItemViewForMobile : MonoBehaviour
{
	private static readonly string _format = "{0}";

	private SystemSeService _systemSeService;

	[SerializeField]
	private DecorationSkinTypeIconDBForMobile _skinIconDB;

	[SerializeField]
	[Header("Skinではないけどスキン扱いとして外すを表示するので例外的に適用するSprite 0:眼鏡 1:それ以外\u30002:眼鏡Active 3:それ以外Active")]
	private Sprite[] _deactivationSkinSprites = new Sprite[4];

	[SerializeField]
	[Header("購入済みもしくは解放済みの時のアイコンイメージ")]
	private Image _skinIconImage;

	[SerializeField]
	[Header("購入できるときのアイコンイメージ")]
	private Image _shopSkinIconImage;

	[SerializeField]
	[Header("購入済みもしくは解放済みの時に使うRoot")]
	private GameObject _mainRoot;

	[SerializeField]
	[Header("購入できる場合に使うRoot")]
	private GameObject _shopRoot;

	[SerializeField]
	[Header("購入済みの時の値段用Root")]
	private GameObject _purchasedRoot;

	[SerializeField]
	private TextMeshProUGUI _priceText;

	[SerializeField]
	private TextMeshProUGUI _soldoutText;

	[SerializeField]
	private GameObject _newObj;

	[SerializeField]
	private Button[] _shopButtons;

	[SerializeField]
	private InteractableUI _selectButton;

	[SerializeField]
	private Color _normalPriceTextColor;

	[SerializeField]
	private Color _notEnoughPointTextColor;

	[SerializeField]
	[Header("値段ボタンの下地のスプライト 0:が不足で1が足りてる")]
	private Sprite[] _purchaseBaseSprites = new Sprite[2];

	[SerializeField]
	[Header("値段ボタンの下地Image")]
	private Image _purchaseBaseImage;

	[SerializeField]
	[Header("モバイル版未購入者の制限表示")]
	private GameObject _mobileDemoEditionLockedObj;

	private DecorationService.DecorationSkinType _skinType;

	private DecorationSkinListItemModelForMobile.DeactivationCategoryType _deactivationType;

	private int _price;

	private bool _isSelected;

	private bool _isMobileDemoEditionLocked;

	private Subject<Unit> _onClickMobileDemoEditionLocked = new Subject<Unit>();

	private Subject<(DecorationService.DecorationSkinType, int)> _onClickShopButton = new Subject<(DecorationService.DecorationSkinType, int)>();

	private Subject<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)> _onClickSelectButton = new Subject<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)>();

	public Observable<Unit> OnClickMobileDemoEditionLocked => _onClickMobileDemoEditionLocked;

	public Observable<(DecorationService.DecorationSkinType, int)> OnClickShopButton => _onClickShopButton;

	public Observable<(DecorationService.DecorationSkinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType)> OnClickSelectButton => _onClickSelectButton;

	public Func<int> CurrentPlayerPointGetter { private get; set; }

	private void Awake()
	{
		_soldoutText.GetComponent<TextLocalizationBehaviour>().Set("ui_decoration_soldout");
		if (_systemSeService == null)
		{
			_systemSeService = ProjectLifetimeScope.Resolve<SystemSeService>();
		}
		ObservableSubscribeExtensions.Subscribe(_selectButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_systemSeService.PlaySelect();
			if (_isMobileDemoEditionLocked)
			{
				_onClickMobileDemoEditionLocked.OnNext(Unit.Default);
			}
			else if (!_isSelected)
			{
				_onClickSelectButton.OnNext((_skinType, _deactivationType));
			}
		}).AddTo(this);
		Button[] shopButtons = _shopButtons;
		for (int num = 0; num < shopButtons.Length; num++)
		{
			ObservableSubscribeExtensions.Subscribe(shopButtons[num].OnClickAsObservable(), delegate
			{
				_systemSeService.PlaySelect();
				_onClickShopButton.OnNext((_skinType, _price));
			}).AddTo(this);
		}
	}

	public void SetSkin(DecorationService.DecorationSkinType skinType, DecorationSkinListItemModelForMobile.DeactivationCategoryType removedCategoryType)
	{
		_skinType = skinType;
		_deactivationType = removedCategoryType;
		Sprite sprite = null;
		switch (removedCategoryType)
		{
		case DecorationSkinListItemModelForMobile.DeactivationCategoryType.None:
			sprite = _skinIconDB.GetSkinTypeIcon(skinType);
			break;
		case DecorationSkinListItemModelForMobile.DeactivationCategoryType.Glass:
			sprite = _deactivationSkinSprites[0];
			break;
		case DecorationSkinListItemModelForMobile.DeactivationCategoryType.Other:
			sprite = _deactivationSkinSprites[1];
			break;
		}
		_skinIconImage.sprite = sprite;
		_shopSkinIconImage.sprite = sprite;
	}

	public void SetPriceData(int price, bool isPurchased)
	{
		_price = price;
		_mainRoot.SetActive(isPurchased);
		_shopRoot.SetActive(!isPurchased);
		_purchasedRoot.SetActive(isPurchased);
		if (price <= 0)
		{
			_soldoutText.enabled = false;
			return;
		}
		if (isPurchased)
		{
			_soldoutText.enabled = true;
			return;
		}
		bool flag = CurrentPlayerPointGetter() >= price;
		_priceText.SetText(_format, price);
		_priceText.color = (flag ? _normalPriceTextColor : _notEnoughPointTextColor);
		_purchaseBaseImage.sprite = (flag ? _purchaseBaseSprites[1] : _purchaseBaseSprites[0]);
	}

	public void SetActiveNewObj(bool active)
	{
		if (_newObj.activeSelf != active)
		{
			_newObj.SetActive(active);
		}
	}

	public void SetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		_selectButton.SetUseUI(isSelected);
		switch (_deactivationType)
		{
		case DecorationSkinListItemModelForMobile.DeactivationCategoryType.Glass:
			_skinIconImage.sprite = (isSelected ? _deactivationSkinSprites[2] : _deactivationSkinSprites[0]);
			break;
		case DecorationSkinListItemModelForMobile.DeactivationCategoryType.Other:
			_skinIconImage.sprite = (isSelected ? _deactivationSkinSprites[3] : _deactivationSkinSprites[1]);
			break;
		}
	}

	public void SetActiveMobileDemoEditionLockedObj(bool active)
	{
		_isMobileDemoEditionLocked = active;
		if (_mobileDemoEditionLockedObj.activeSelf != active)
		{
			_mobileDemoEditionLockedObj.SetActive(active);
		}
	}
}
