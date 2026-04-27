using Bulbul.Web;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class ShopWindowView : MonoBehaviour
{
	[SerializeField]
	private FacilityCommonActivateAnimationMobile _facilityCommonActivateAnimation;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _steamStorePageBannerButton;

	[SerializeField]
	private Button _ecSiteBannerButton;

	[SerializeField]
	private Button _specifiedCommercialTransactionActButton;

	[SerializeField]
	private Button _restoreButton;

	[SerializeField]
	private ShopProductListView _productListView;

	[SerializeField]
	private Image _waitingRaycastBlocker;

	[SerializeField]
	private ShopBannerView _shopBannerView;

	public ShopSetting[] CurrentShopSettings { get; set; }

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public Observable<Unit> OnClickSteamStorePageBannerButton => _steamStorePageBannerButton.OnClickAsObservable();

	public Observable<Unit> OnClickECSiteBannerButton => _ecSiteBannerButton.OnClickAsObservable();

	public Observable<string> OnClickPurchaseButton => _productListView.OnClickPurchaseButton;

	public Observable<Unit> OnClickSpecifiedCommercialTransactionActButton => _specifiedCommercialTransactionActButton.OnClickAsObservable();

	public Observable<Unit> OnClickRestoreButton => _restoreButton.OnClickAsObservable();

	public void Setup()
	{
		_facilityCommonActivateAnimation.Setup();
		_productListView.Setup();
		_shopBannerView.Setup();
	}

	public async UniTask Activate()
	{
		_productListView.UpdateItems(CurrentShopSettings);
		_shopBannerView.ResetBannerIdx();
		await _facilityCommonActivateAnimation.Activate();
	}

	public async UniTask Deactivate()
	{
		await _facilityCommonActivateAnimation.Deactivate();
	}

	public void SetActiveWaitingRaycastBlocker(bool active)
	{
		_waitingRaycastBlocker.enabled = active;
	}

	public void UpdateItems()
	{
		_productListView.UpdateItems();
	}
}
