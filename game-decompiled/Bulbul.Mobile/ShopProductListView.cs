using Bulbul.Web;
using R3;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Bulbul.Mobile;

public class ShopProductListView : MonoBehaviour
{
	[SerializeField]
	private ShopProductListItemView _upgradePassItemView;

	private Subject<string> _onClickPurchaseButton = new Subject<string>();

	private PurchasingCtrl _purchasingCtrl;

	public Observable<string> OnClickPurchaseButton => _onClickPurchaseButton;

	public void Setup()
	{
		_purchasingCtrl = PurchasingCtrl.GetInstance();
		Product product = _purchasingCtrl.FindProduct("");
		if (product == null)
		{
			_upgradePassItemView.gameObject.SetActive(value: false);
			return;
		}
		_upgradePassItemView.gameObject.SetActive(value: true);
		_upgradePassItemView.SetPriceText(product.metadata.localizedPriceString);
		ObservableSubscribeExtensions.Subscribe(_upgradePassItemView.OnClickPurchaseButton, delegate
		{
			_onClickPurchaseButton.OnNext("");
		}).AddTo(this);
		UpdateItems();
	}

	public void UpdateItems(ShopSetting[] shopSettings = null)
	{
	}

	private int CalcViewSalePer(decimal basePrice, decimal salePrice)
	{
		return (int)((1m - salePrice / basePrice) * 100m);
	}
}
