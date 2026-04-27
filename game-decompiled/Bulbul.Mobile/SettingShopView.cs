using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingShopView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[SerializeField]
	private Button buyUpgradeButton;

	[SerializeField]
	private Button restoreButton;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	private Subject<Unit> onClickShopInit = new Subject<Unit>();

	public Observable<Unit> OnClickClose => onClickClose;

	public Observable<Unit> OnClickShopInit => onClickShopInit;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
	}

	public void Activate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			viewAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			viewAnimation.Deactivate().Forget();
		}
	}

	public void Setup(SettingData saveData)
	{
		viewAnimation.Setup();
		ObservableSubscribeExtensions.Subscribe(closeButton.OnClickAsObservable(), delegate
		{
			onClickClose?.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(buyUpgradeButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			PurchasingCtrl.GetInstance().ExecutePurchase("");
		}).AddTo(this);
		restoreButton.gameObject.SetActive(value: false);
	}

	public void AdjustToSavedata()
	{
	}
}
