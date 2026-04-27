using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class ShopProductListItemView : MonoBehaviour
{
	[Serializable]
	public struct SaleUIs
	{
		public TextMeshProUGUI _basePriceText;

		public TextMeshProUGUI _salePriceText;

		public TextLocalizationBehaviour _endDateText;

		public TextLocalizationBehaviour _salePerText;
	}

	[SerializeField]
	private TextLocalizationBehaviour _productNameText;

	[SerializeField]
	private TextLocalizationBehaviour _purchaseButtonText;

	[SerializeField]
	private Button _purchaseButton;

	[SerializeField]
	private TextMeshProUGUI _priceText;

	[SerializeField]
	private Image _normalBG;

	[SerializeField]
	private Image _grayBG;

	[SerializeField]
	private Color _normalTextColor;

	[SerializeField]
	private Color _grayTextColor;

	[SerializeField]
	private GameObject _salePriceLayoutObj;

	[SerializeField]
	private SaleUIs _saleUIs;

	public Observable<Unit> OnClickPurchaseButton => _purchaseButton.OnClickAsObservable();

	public void Setup()
	{
	}

	public void SetProductNameLocalizeID(string localizeID)
	{
		_productNameText.Set(localizeID);
	}

	public void SetPurchaseButtonTextLocalizeID(string localizeID)
	{
		_purchaseButtonText.Set(localizeID);
	}

	public void SetPriceText(string priceStr)
	{
		_priceText.text = priceStr;
	}

	public void SetEnablePurchase(bool enable)
	{
		_normalBG.enabled = enable;
		_grayBG.enabled = !enable;
		_purchaseButton.interactable = enable;
		_purchaseButtonText.Text.color = (enable ? _normalTextColor : _grayTextColor);
	}

	public void SetActiveNormalPriceLayout(bool active)
	{
		_priceText.enabled = active;
	}

	public void SetActiveSalePriceLayout(bool active)
	{
		if (_salePriceLayoutObj.activeSelf != active)
		{
			_salePriceLayoutObj.SetActive(active);
		}
	}

	public void SetSaleData(string basePrice, string salePrice, int salePer, string endDateString)
	{
		_saleUIs._basePriceText.SetText(basePrice);
		_saleUIs._salePriceText.SetText(salePrice);
		_saleUIs._salePerText.Set("ui_shop_sale_per_off", (string _) => _.Replace("{0}", salePer.ToString()));
		_saleUIs._endDateText.Set("ui_shop_sale_end_date", (string _) => _.Replace("{0}", endDateString));
	}
}
